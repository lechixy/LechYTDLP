using LechYTDLP.Classes;
using LechYTDLP.Components;
using LechYTDLP.Util;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using Windows.Storage;

namespace LechYTDLP.Services
{
    public enum DownloadState
    {
        Queued,
        Downloading,
        Completed,
        Failed,
        Paused,
        Resuming
    }

    public class DownloadItem
    {
        public string Url { get; set; } = string.Empty;
        public VideoInfo Info { get; set; } = null!;
        public DownloadState State { get; set; } = DownloadState.Queued;
        public int Progress { get; set; } = 0;
        public SelectedFormat SelectedFormat { get; set; } = new();
        public string FilePath { get; set; } = string.Empty;
    }

    public class DownloadsService
    {
        private readonly Queue<DownloadItem> _queue = new();
        private static readonly List<DownloadItem> downloadItems = [];
        private readonly List<DownloadItem> _history = downloadItems;

        private bool _isRunning;
        private bool _isPaused;

        private readonly YTDLP _ytdlp = new();

        public bool IsPaused => _isPaused;
        public DownloadItem? CurrentMedia => _queue.Count == 0 ? null : _queue.Peek();

        public event Action? QueueUpdated;
        public event Action? CurrentMediaUpdated;

        public IReadOnlyCollection<DownloadItem> Queue => [.. _queue];
        public IReadOnlyCollection<DownloadItem> History => [.. _history];


        // Downloads count should be updated when _queue count changes
        public int DownloadsCount => _queue.Count;
        public static Action<int, string>? OnBadgeChanged;


        public async Task<bool> PauseOrResume()
        {
            var tcs = new TaskCompletionSource<bool>();

            if (_isPaused)
            {
                // Resume
                _isPaused = false;
                LogService.Add("Resuming download.", LogTag.LechYTDLP);
                TryStartNext();
                tcs.SetResult(true);
            }
            else
            {
                // Pause
                _isPaused = true;
                LogService.Add("Pausing download.", LogTag.LechYTDLP);
                await _ytdlp.StopYTDLPAsync();
                CurrentMedia!.State = DownloadState.Paused;
                CurrentMediaUpdated?.Invoke();
                tcs.SetResult(true);
            }

            return await tcs.Task;
        }

        public void Enqueue(string url, VideoInfo videoInfo, SelectedFormat selectedFormat)
        {
            var item = new DownloadItem
            {
                Url = url,
                State = DownloadState.Queued,
                Info = videoInfo,
                SelectedFormat = selectedFormat
            };

            _queue.Enqueue(item);

            if (_queue.Count == 1) CurrentMediaUpdated?.Invoke();
            else QueueUpdated?.Invoke();

            OnBadgeChanged?.Invoke(DownloadsCount, "Downloads");

            TryStartNext();
        }

        private async void TryStartNext()
        {
            if (_isRunning || IsPaused)
                return;

            if (_queue.Count == 0)
                return;

            _isRunning = true;
            _isPaused = false;

            var item = _queue.Peek();
            item.State = DownloadState.Queued;
            CurrentMediaUpdated?.Invoke();
            OnBadgeChanged?.Invoke(DownloadsCount, "Downloads");

            _ytdlp.OutputReceived += HandleYTDLPOutput;

            // İndirme işlemini başlat
            var args = new YTDLPDownloadArgs
            {
                SelectedFormat = item.SelectedFormat,
                OutputPath = $"{SettingsService.DownloadPath}\\{SettingsService.FilenameTemplate}",
                FFmpegLocation = $"{SettingsService.FFmpegPath}"
            };

            var processCode = await _ytdlp.DownloadVideo(item.Url, args);
            LogService.Add($"Download finished with code: {processCode}", LogTag.LechYTDLP);
            Debug.WriteLine($"Download finished with code: {processCode}");


            // Eğer indirme sürerken 'Pause' denildiyse, aşağıdaki işlemleri atla
            if (this.IsPaused && processCode == -1)
            {
                Debug.WriteLine("Download was paused. Exiting without updating queue.");
                _isRunning = false;
                return;
            }
            else if (processCode == -1)
            {
                Debug.WriteLine("Download failed with an error. Marking as failed.");
            }
            else
            {
                Debug.WriteLine("Download completed. Updating queue and history.");
            }

            item.State = processCode == 0 ? DownloadState.Completed : DownloadState.Failed;
            CurrentMediaUpdated?.Invoke();

            if (_queue.Count > 0) _queue.Dequeue();
            _history.Add(item);

            // İndirme tamamlandıktan sonra geçmişi dosyaya kaydet
            await App.DatabaseService.AddOrUpdateAsync(item);

            QueueUpdated?.Invoke();
            OnBadgeChanged?.Invoke(DownloadsCount, "Downloads");
            _isRunning = false;

            _ytdlp.OutputReceived -= HandleYTDLPOutput;

            TryStartNext();
        }

        private void HandleYTDLPOutput(string textLine)
        {
            if (textLine.Contains("Extracting URL"))
            {
                //DownloadBusyChanged?.Invoke(true);
                return;
            }

            if (textLine.Contains("is not a valid URL"))
            {
                //DownloadBusyChanged?.Invoke(false);
                return;
            }

            if (textLine.Contains("[download]") &&
                textLine.Contains("of") &&
                textLine.Contains("at"))
            {
                CurrentMedia!.State = DownloadState.Downloading;

                var parts = textLine.Split(' ');
                var percentPart = Array.Find(parts, p => p.EndsWith("%"));

                if (percentPart != null &&
                    double.TryParse(
                        percentPart.TrimEnd('%'),
                        NumberStyles.Any,
                        CultureInfo.InvariantCulture,
                        out var value))
                {
                    _queue.First().Progress = (int)value;
                }
            }

            if (textLine.Contains("100%") ||
                textLine.Contains("has already been downloaded"))
            {
                CurrentMedia!.State = DownloadState.Completed;
            }

            if (textLine.Contains("[download] Destination:"))
            {
                // If both audio and video are being downloaded, don't set the file path yet
                // Because it will be merged later and the final file path will be different.
                if (CurrentMedia!.SelectedFormat.AudioId != null && CurrentMedia.SelectedFormat.VideoId != null) return;
                CurrentMedia.FilePath = textLine.Split(["[download] Destination:"], StringSplitOptions.None)[1].Trim();
                Debug.WriteLine($"Set file path to: {CurrentMedia.FilePath}");
            }

            if (textLine.Contains("[Merger] Merging formats into"))
            {
                CurrentMedia!.FilePath = textLine.Split(["[Merger] Merging formats into"], StringSplitOptions.None)[1].Replace('"', ' ').Trim();
                Debug.WriteLine($"Set file path to: {CurrentMedia.FilePath}");
            }

            if (textLine.Contains("has already been downloaded"))
            {
                CurrentMedia!.FilePath = textLine.Split(["[download]"], StringSplitOptions.None)[1].Split(["has already been downloaded"], StringSplitOptions.None)[0].Trim();
            }

            if (textLine.Contains("[download] Resuming download"))
            {
                CurrentMedia!.State = DownloadState.Resuming;

                Debug.WriteLine("Resuming download...");
                //App.InfoBarService.Show(new InfoBarMessage(
                //    "Resuming download",
                //    $"Resuming download for {CurrentMedia.Info.Title}",
                //    InfoBarSeverity.Informational
                //));
            }

            CurrentMediaUpdated?.Invoke();
        }
    }
}

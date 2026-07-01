using LechYTDLP.Classes;
using LechYTDLP.Components;
using LechYTDLP.Util;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
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
        Resuming,
        TestingFormat
    }

    public class DownloadItem
    {
        public Guid Id { get; set; } = Guid.Empty;
        public string Url { get; set; } = string.Empty;
        public VideoInfo Info { get; set; } = null!;
        public DownloadState State { get; set; } = DownloadState.Queued;
        public int Progress { get; set; } = 0;
        public SelectedFormat SelectedFormat { get; set; } = new();
        public string FilePath { get; set; } = string.Empty;
    }

    public partial class DownloadsService
    {
        private readonly Queue<DownloadItem> _queue = new();
        private static readonly List<DownloadItem> downloadItems = [];
        private readonly List<DownloadItem> _history = downloadItems;

        private bool _isRunning;
        private bool _isPaused;

        private readonly YTDLP _ytdlp = new();

        public bool IsPaused => _isPaused;
        public DownloadItem? CurrentMedia => _queue.Count == 0 ? null : _queue.Peek();

        // EVENTS
        public event Action? QueueUpdated;
        public event Action<bool>? HistoryUpdated;
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
                LogService.Add("Resuming download.", LogTag.YTDLP);
                TryStartNext();
                tcs.SetResult(true);
            }
            else
            {
                // Pause
                _isPaused = true;
                LogService.Add("Pausing download.", LogTag.YTDLP);
                await _ytdlp.StopYTDLPAsync();
                CurrentMedia!.State = DownloadState.Paused;
                CurrentMediaUpdated?.Invoke();
                tcs.SetResult(true);
            }

            return await tcs.Task;
        }

        public void Enqueue(string url, VideoInfo videoInfo, SelectedFormat selectedFormat)
        {
            //if (selectedFormat == null)
            //{
            //    App.DownloadController.SearchAsync(url, videoInfo);
            //    return;
            //}

            var item = new DownloadItem
            {
                Id = Guid.NewGuid(),
                Url = url,
                State = DownloadState.Queued,
                Info = videoInfo,
                SelectedFormat = selectedFormat
            };

            _queue.Enqueue(item);

            AppNotification notification = new AppNotificationBuilder()
            .AddText(App.LocalizationService.Get("DownloadQueued"))
            .AddText(item.Info.Title ?? App.LocalizationService.Get("UnknownTitle"))
            .SetInlineImage(new Uri(WebUtility.HtmlEncode(item.Info.Thumbnail!)))
            .BuildNotification();

            AppNotificationManager.Default.Show(notification);

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
            var info = item.Info;
            item.State = DownloadState.Queued;
            CurrentMediaUpdated?.Invoke();
            OnBadgeChanged?.Invoke(DownloadsCount, "Downloads");

            _ytdlp.OutputReceived += HandleYTDLPOutput;

            //// Create file name and path
            //var ext = info.Ext ?? "mp4";
            //var fileName = ApplyTemplate(SettingsService.FilenameTemplate, info, ext);
            //var fullPath = Path.Combine(SettingsService.DownloadPath, fileName);
            //item.FilePath = fullPath;

            // We need to delete the file if it already exists, otherwise yt-dlp will rewrite it and json file will be wrong
            var printToFilePath = Path.Combine(LechKnownFolders.GetPath(LechKnownFolder.Documents), $"LechYTDLP\\Logs\\{info.Id}.info.json");
            if (File.Exists(printToFilePath))
            {
                try
                {
                    File.Delete(printToFilePath);
                } catch {
                    Debug.WriteLine($"Failed to delete {printToFilePath}");
                }
            }

            // İndirme işlemini başlat
            var args = new YTDLPDownloadArgs
            {
                // # Required arguments
                Url = item.Url,
                SelectedFormat = item.SelectedFormat,
                OutputPath = Path.Combine(SettingsService.DownloadPath, SettingsService.FilenameTemplate),
                FFmpegLocation = SettingsService.FFmpegPath,
                PrintToFile = $"\"video:%()j\" \"{printToFilePath}\"",

                Newline = true,
                NoColor = true,
                ProgressTemplate = "P|%(progress._percent_str)s",

                // Optional arguments
                EmbedThumbnail = SettingsService.EmbedThumbnail,
                EmbedSubs = SettingsService.EmbedSubs
            };

            var processCode = await _ytdlp.DownloadVideo(args, info);
            LogService.Add($"Download finished with code: {processCode}", LogTag.YTDLP);
            Debug.WriteLine($"Download finished with code: {processCode}");


            // Eğer indirme sürerken 'Pause' denildiyse, aşağıdaki işlemleri atla
            if (this.IsPaused && processCode == -1)
            {
                Debug.WriteLine("Download was paused. Exiting without updating queue.");
                _isRunning = false;
                return;
            }
            else if (processCode == 1)
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
            // Update history with new items from database
            HistoryUpdated?.Invoke(true);
            OnBadgeChanged?.Invoke(DownloadsCount, "Downloads");
            _isRunning = false;

            _ytdlp.OutputReceived -= HandleYTDLPOutput;

            TryStartNext();
        }

        //static string StripAnsi(string input)
        //{
        //    return Regex.Replace(input, @"\x1B\[[0-9;]*[mK]", "");
        //}

        private void HandleYTDLPOutput(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
                return;

            // Progress
            if (data.StartsWith("P|"))
            {
                var percentText = data.Substring(2).Replace("%", "").Trim();

                if (double.TryParse(percentText,
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out var percent))
                {
                    CurrentMedia!.State = DownloadState.Downloading;
                    CurrentMedia.Progress = (int)percent;
                    CurrentMediaUpdated?.Invoke();
                }

                return;
            }

            //// Dosya gerçekten oluştuysa tamam
            //if (CurrentMedia?.FilePath != null &&
            //    File.Exists(CurrentMedia.FilePath))
            //{
            //    CurrentMedia.State = DownloadState.Completed;
            //    CurrentMediaUpdated?.Invoke();
            //}
        }

        public async void RemoveFromHistory(DownloadItem item)
        {
            await App.DatabaseService.DeleteByGuidIdAsync(item.Id.ToString());
            HistoryUpdated?.Invoke(true);
        }

        private static string ApplyTemplate(string template, VideoInfo info, string ext)
        {
            var map = new Dictionary<string, string?>
            {
                ["id"] = info.Id,
                ["title"] = info.Title,
                ["ext"] = ext
            };

            // I doing this because some extractors use 'uploader' and some use 'channel' for the same thing, so I want to support both
            if (info.Extractor != null && info.Extractor.Equals("youtube", StringComparison.OrdinalIgnoreCase))
            {
                map.Add("uploader", info.UploaderId ?? info.Uploader ?? info.Channel);
            }
            else if (info.Extractor != null && info.Extractor.Equals("instagram", StringComparison.OrdinalIgnoreCase))
            {
                map.Add("uploader", info.Channel ?? info.Uploader ?? info.UploaderId);
            }
            else if (info.Extractor != null && info.Extractor.Equals("tiktok", StringComparison.OrdinalIgnoreCase))
            {
                map.Add("uploader", info.Channel ?? info.Uploader ?? info.UploaderId);
            }
            else
            {
                map.Add("uploader", info.UploaderId ?? info.Uploader ?? info.Channel);
            }

            // Remove @ from uploader/channel names to avoid issues with file systems
            map["uploader"] = map["uploader"]?.Replace('@', ' ').Trim();

            return FilenameTemplateRegex().Replace(template, match =>
            {
                var key = match.Groups[1].Value;
                var limitStr = match.Groups[2].Value;

                var value = map.ContainsKey(key) ? map[key] : "unknown";
                value = NormalizeFileName(value ?? "unknown");

                if (int.TryParse(limitStr, out var limit) && value.Length > limit)
                    value = value.Substring(0, limit);

                return value;
            });
        }

        [GeneratedRegex(@"%\(([^)]+)\)(?:\.(\d+)B)?s")]
        private static partial Regex FilenameTemplateRegex();

        private static string NormalizeFileName(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "unknown";

            input = input.Trim();

            foreach (var c in Path.GetInvalidFileNameChars())
                input = input.Replace(c, '_');

            return input;
        }
    }
}

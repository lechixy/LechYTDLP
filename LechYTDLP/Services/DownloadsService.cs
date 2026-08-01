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
using Windows.Media.Playlists;
using Windows.Storage;

namespace LechYTDLP.Services
{
    public enum DownloadState
    {
        Queued,
        Downloading,
        Completed,
        PartiallyCompleted,
        Failed,
        Paused,
        Resuming,
        TestingFormat,
    }

    public class DownloadItem
    {
        public Guid Id { get; set; } = Guid.Empty;
        public string Url { get; set; } = string.Empty;
        public VideoInfo Info { get; set; } = null!;
        public InfoType Type => Info.Type;

        public DownloadState State { get; set; } = DownloadState.Queued;
        public int Progress { get; set; } = 0;
        public SelectedFormat SelectedFormat { get; set; } = new();
        public SelectedFormat[] SelectedFormats { get; set; } = [];
        public string FilePath { get; set; } = string.Empty;
        public DownloadItemMeta Meta { get; set; } = null!;
    }

    public class DownloadItemMeta
    {
        // The number of videos in the playlist (includes unavailable videos)
        public int PlaylistVideoCount { get; set; } = 0;
        // The number of videos that were unavailable in the playlist and were skipped during the download process.
        public int PlaylistUnavailableVideoCount { get; set; } = 0;
        // The current index of the video being downloaded in the playlist. This is used to track the progress of the playlist download. (starts at 1)
        public int PlaylistCurrentIndex { get; set; } = 1;
        // The total number of videos in the playlist. This is used to track the progress of the playlist download.
        public int PlaylistAvailableVideoCount => PlaylistVideoCount - PlaylistUnavailableVideoCount;
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

        public void Enqueue(string url, InfoType type, VideoInfo videoInfo, SelectedFormat[] selectedFormats)
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
                // No need for type assigment since it's already in videoInfo
                // Type = type,
                State = DownloadState.Queued,
                Info = videoInfo,
                Meta = new DownloadItemMeta(),
            };

            if (type == InfoType.Video) item.SelectedFormat = selectedFormats.First();
            else if (type == InfoType.Playlist)
            {
                item.SelectedFormats = selectedFormats;

                // Set the playlist video count (includes unavailable videos)
                item.Meta.PlaylistVideoCount = videoInfo.PlaylistCount ?? 0;
            }

            if (item.SelectedFormat == null && item.SelectedFormats.Length == 0)
            {
                Debug.WriteLine($"Warning: Enqueue called with type {type} but no selected formats provided.");
                return;
            }

            _queue.Enqueue(item);

            var notification = new AppNotificationBuilder()
            .AddText(App.LocalizationService.Get("DownloadQueued"))
            .AddText(item.Info.Title ?? App.LocalizationService.Get("UnknownTitle"))
            .SetInlineImage(new Uri(WebUtility.HtmlEncode(item.Info.BestThumbnailUrl)));

            AppNotificationManager.Default.Show(notification.BuildNotification());

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
            _ytdlp.ErrorReceived += HandleYTDLPError;

            // We need to delete the file if it already exists, otherwise yt-dlp will rewrite it and json file will be wrong
            var printToFilePath = Path.Combine(LechKnownFolders.GetPath(LechKnownFolder.Documents), $"LechYTDLP\\Logs\\{info.Id}.info.json");
            if (File.Exists(printToFilePath))
            {
                try
                {
                    File.Delete(printToFilePath);
                }
                catch
                {
                    Debug.WriteLine($"Failed to delete {printToFilePath}");
                }
            }

            // Arguments for yt-dlp download (this is last argument)
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

            if (item.Type == InfoType.Playlist)
            {
                // We add one to each index because yt-dlp starts index 1
                args.PlaylistItems = string.Join(",", item.SelectedFormats.Select(f => f.Index + 1));

                // Might be useful to add a playlist template in the future, but for now, we'll just use the same template for all videos in the playlist.
                //args.OutputPath = Path.Combine(SettingsService.DownloadPath, SettingsService.FilenameTemplatePlaylist);
            }

            // TODO: In future, we might add every video in playlist (because in yt-dlp can't pass arguments to individual videos) 
            // If we want to do that, we need to change the way we handle the queue, it's currently designed to download playlist just in one format.
            //foreach (var video in playlist.Entries)
            //{
            //    var args = video.SelectedPreset.ToDownloadArgs();

            //    await YTDLP.DownloadVideoAsync(video.Url, args);
            //}

            // Start the download
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
                Debug.WriteLine("Download failed with an error. Checking if it's a partially failed download or completely failed.");
            }
            else
            {
                Debug.WriteLine("Download completed. Updating queue and history.");
            }

            item.State = processCode == 0 ? DownloadState.Completed : (item.Meta.PlaylistUnavailableVideoCount > 0 ? DownloadState.PartiallyCompleted : DownloadState.Failed);
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
            _ytdlp.ErrorReceived -= HandleYTDLPError;

            TryStartNext();
        }

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

            if (data.StartsWith("[download] Downloading item"))
            {
                var match = Regex.Match(data, @"\[download\] Downloading item (\d+) of (\d+)");
                if (CurrentMedia != null && match.Success)
                {
                    CurrentMedia.Meta.PlaylistCurrentIndex = int.Parse(match.Groups[1].Value);
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

        private void HandleYTDLPError(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
                return;

            if (data.Contains("unavailable videos are hidden"))
            {
                var match = Regex.Match(data, @"(\d+) unavailable videos are hidden");

                if (CurrentMedia != null && match.Success)
                {
                    CurrentMedia.Meta.PlaylistUnavailableVideoCount = int.Parse(match.Groups[1].Value);
                }
                return;

            }
        }

        public async void RemoveFromHistory(DownloadItem item)
        {
            await App.DatabaseService.DeleteByGuidIdAsync(item.Id.ToString());
            HistoryUpdated?.Invoke(true);
        }
    }
}

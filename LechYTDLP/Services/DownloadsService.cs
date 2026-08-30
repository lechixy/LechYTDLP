using LechYTDLP.Classes;
using LechYTDLP.Components;
using LechYTDLP.Util;
using Microsoft.Data.Sqlite;
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
using System.Threading;
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
        Cancelled
    }

    public class DownloadItem
    {
        public Guid Id { get; set; } = Guid.Empty;
        public string Url { get; set; } = string.Empty;
        public YtDlpData Info { get; set; } = null!;
        public InfoType Type => Info.Type;
        private DownloadState _state = DownloadState.Queued;
        public DownloadState State
        {
            get => _state;
            set
            {
                if (_state == value)
                    return;

                _state = value;
                NotifyChanged();
            }
        }
        private int _progress = 0;
        public int Progress
        {
            get => _progress;
            set
            {
                if (_progress == value)
                    return;

                _progress = value;
                NotifyChanged();
            }
        }
        public SelectedFormat SelectedFormat { get; set; } = new();
        public SelectedFormat[] SelectedFormats { get; set; } = [];
        public string FilePath { get; set; } = string.Empty;
        public DownloadItemMeta Meta { get; set; } = null!;
        public event EventHandler? Changed;
        public void NotifyChanged()
        {
            Changed?.Invoke(this, EventArgs.Empty);
        }
        public bool WantedToPause { get; set; } = false;
        public bool WantedToResume { get; set; } = false;
        public bool WantedToCancel { get; set; } = false;
        public CancellationTokenSource CancellationTokenSource { get; private set; } = new();
        public CancellationToken CancellationToken =>
            CancellationTokenSource.Token;
        public void Cancel()
        {
            CancellationTokenSource.Cancel();
        }
        public void RenewCancel()
        {
            if (CancellationTokenSource.IsCancellationRequested)
            {
                CancellationTokenSource.Dispose();
                // Create a new CancellationTokenSource
                var newCts = new CancellationTokenSource();
                // Use reflection to set the private field
                CancellationTokenSource = newCts;
            }
        }
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
        private readonly List<DownloadItem> _currentQueue = new();
        private readonly Queue<DownloadItem> _queue = new();
        private static readonly List<DownloadItem> downloadItems = [];
        private readonly List<DownloadItem> _history = downloadItems;

        private int _currentDownloads = 0;
        private bool _isPaused;

        private readonly YTDLP _ytdlp = new();

        public IReadOnlyCollection<DownloadItem> CurrentDownloads => _currentQueue;
        public bool IsPaused => _isPaused;

        // EVENTS
        public event Action? CurrentQueueUpdated;
        public event Action? InQueueUpdated;
        public event Action<bool>? HistoryQueueUpdated;

        public IReadOnlyCollection<DownloadItem> CurrentQueue => [.. _currentQueue];
        public IReadOnlyCollection<DownloadItem> Queue => [.. _queue];
        public IReadOnlyCollection<DownloadItem> History => [.. _history];


        // Downloads count should be updated when _queue count changes
        public int DownloadsCount => _currentQueue.Count + _queue.Count;
        public static Action<int, string>? OnBadgeChanged;

        public void Enqueue(string url, InfoType type, YtDlpData videoInfo, SelectedFormat[] selectedFormats)
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

            InQueueUpdated?.Invoke();

            OnBadgeChanged?.Invoke(DownloadsCount, "Downloads");

            TryStartNext();
        }

        private async void TryStartNext()
        {
            if (_currentDownloads > SettingsService.ConcurrentDownloads)
                return;

            if (_queue.Count == 0)
                return;

            _currentDownloads++;
            _isPaused = false;

            var item = _queue.Dequeue();
            _currentQueue.Add(item);

            item.State = DownloadState.Queued;
            CurrentQueueUpdated?.Invoke();
            OnBadgeChanged?.Invoke(DownloadsCount, "Downloads");

            await RunDownloadAsync(item);
        }

        // item'ın zaten _currentQueue'da olduğunu ve _currentDownloads'a sayıldığını varsayar.
        // Hem TryStartNext hem de ResumeDownload buradan çağırır.
        private async Task RunDownloadAsync(DownloadItem item)
        {
            var info = item.Info;

            // We want to delete the previous info.json file if it exists, because we will create a new one for this download.
            var printToFilePath = Path.Combine(LechKnownFolders.GetPath(LechKnownFolder.Documents), $"LechYTDLP\\Logs\\{info.Id}.info.json");
            if (File.Exists(printToFilePath))
            {
                try { File.Delete(printToFilePath); }
                catch { Debug.WriteLine($"Failed to delete {printToFilePath}"); }
            }

            var args = new DlArgs
            {
                Type = DlArgsType.YTDLP,
                Url = item.Url,
                SelectedFormat = item.SelectedFormat,
                OutputPath = Path.Combine(SettingsService.DownloadPath, SettingsService.FilenameTemplate),
                FFmpegLocation = SettingsService.FFmpegPath,
                PrintToFile = $"\"video:%()j\" \"{printToFilePath}\"",
                Newline = true,
                NoColor = true,
                ProgressTemplate = "P|%(progress._percent_str)s",
                EmbedThumbnail = SettingsService.EmbedThumbnail,
                EmbedSubs = SettingsService.EmbedSubs
            };

            if (item.Type == InfoType.Playlist)
            {
                args.PlaylistItems = string.Join(",", item.SelectedFormats.Select(f => f.Index + 1));
            }

            Action<string?> handleOutput = data =>
            {
                if (string.IsNullOrWhiteSpace(data)) return;

                if (data.StartsWith("P|"))
                {
                    var percentText = data.Substring(2).Replace("%", "").Trim();
                    if (double.TryParse(percentText, NumberStyles.Any, CultureInfo.InvariantCulture, out var percent))
                    {
                        item.State = DownloadState.Downloading;
                        item.Progress = (int)percent;
                    }
                    return;
                }

                if (data.StartsWith("[download] Downloading item"))
                {
                    var match = Regex.Match(data, @"\[download\] Downloading item (\d+) of (\d+)");
                    if (match.Success)
                        item.Meta.PlaylistCurrentIndex = int.Parse(match.Groups[1].Value);
                    return;
                }
            };

            Action<string?> handleError = data =>
            {
                if (string.IsNullOrWhiteSpace(data)) return;

                if (data.Contains("unavailable videos are hidden"))
                {
                    var match = Regex.Match(data, @"(\d+) unavailable videos are hidden");
                    if (match.Success)
                        item.Meta.PlaylistUnavailableVideoCount = int.Parse(match.Groups[1].Value);
                }
            };

            var downloadResult = await _ytdlp.DownloadVideo(args, item, handleOutput, handleError, item.CancellationToken);
            LogService.Add(App.LocalizationService.Get("DownloadFinishedWithCode", downloadResult.Code), LogTag.YTDLP);

            // Pause durumu: item _currentQueue'da kalır, history'e düşmez, sayaç azalır ve döner.
            if (item.WantedToPause && downloadResult.Code == ResultCode.Cancelled)
            {
                item.WantedToPause = false; // tüketildi, bir sonraki cancel yanlışlıkla pause sayılmasın
                item.State = DownloadState.Paused;
                CurrentQueueUpdated?.Invoke();
                _currentDownloads--;
                TryStartNext(); // kuyrukta bekleyen varsa onun yerine başlasın
                return;
            }

            // Cancel durumu: item _currentQueue'da kalır, history'e düşmez, sayaç azalır ve döner.
            if (item.WantedToCancel && downloadResult.Code == ResultCode.Cancelled)
            {
                item.WantedToCancel = false; // tüketildi, bir sonraki cancel yanlışlıkla cancel sayılmasın
                item.State = DownloadState.Cancelled;
            }
            // Normal tamamlanma veya hata durumu: item _currentQueue'dan çıkarılır, history'e düşer, sayaç azalır ve döner.
            else
            {
                item.State = downloadResult.Code == ResultCode.Success ? DownloadState.Completed :
                    (item.Meta.PlaylistUnavailableVideoCount > 0 ? DownloadState.PartiallyCompleted : DownloadState.Failed);
            }

            _currentQueue.Remove(item);
            _history.Add(item);

            CurrentQueueUpdated?.Invoke();
            await App.DatabaseService.AddOrUpdateAsync(item);
            InQueueUpdated?.Invoke();
            HistoryQueueUpdated?.Invoke(true);
            OnBadgeChanged?.Invoke(DownloadsCount, "Downloads");
            _currentDownloads--;

            TryStartNext();
        }

        public void PauseDownload(DownloadItem item)
        {
            item.WantedToPause = true;
            item.WantedToResume = false;
            item.Cancel();
        }

        public void ResumeDownload(DownloadItem item)
        {
            if (item.State != DownloadState.Paused)
                return;

            item.WantedToPause = false;
            item.WantedToResume = true;
            item.RenewCancel();

            item.State = DownloadState.Resuming;

            // Item zaten _currentQueue'da duruyor (pause sırasında hiç çıkarılmamıştı).
            // Eşzamanlı indirme limiti doluysa gerçek bir kuyruğa gönderiyoruz,
            // değilse doğrudan pipeline'a sokuyoruz.
            if (_currentDownloads > SettingsService.ConcurrentDownloads)
            {
                _currentQueue.Remove(item);
                _queue.Enqueue(item);
                item.State = DownloadState.Queued;
                InQueueUpdated?.Invoke();
                TryStartNext();
                return;
            }

            _currentDownloads++;
            _ = RunDownloadAsync(item);
        }

        public void CancelDownload(DownloadItem item)
        {
            item.WantedToPause = false;
            item.WantedToResume = false;
            item.WantedToCancel = true;
            item.Cancel();
        }

        public async Task RemoveFromHistory(DownloadItem item)
        {
            await App.DatabaseService.DeleteByGuidIdAsync(item.Id.ToString());
            HistoryQueueUpdated?.Invoke(true);
        }
    }
}

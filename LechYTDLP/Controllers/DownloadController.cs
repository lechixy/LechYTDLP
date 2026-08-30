using LechYTDLP.Classes;
using LechYTDLP.Components;
using LechYTDLP.Services;
using LechYTDLP.Util;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LechYTDLP.Controllers
{
    public sealed class SearchRequest
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string Url { get; }
        public Uri UrlUri { get; }
        public CancellationTokenSource CancellationTokenSource { get; } = new();
        public CancellationToken CancellationToken =>
            CancellationTokenSource.Token;
        public DateTime StartedAt { get; } = DateTime.Now;
        public SearchRequest(string url)
        {
            Url = url;
            if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                UrlUri = uri;
            }
            else
            {
                UrlUri = new Uri("http://invalid.url");
            }
        }

        public SearchRequest(SearchRequest other)
        {
            Id = other.Id;
            Url = other.Url;
            UrlUri = other.UrlUri;
            CancellationTokenSource = other.CancellationTokenSource;
            StartedAt = other.StartedAt;
        }

        public void Cancel()
        {
            CancellationTokenSource.Cancel();
        }
    }

    public class SearchOptions
    {
        public YtDlpData? VideoInfo { get; set; }
        public bool ForceDialog { get; set; }
    }

    public class DownloadController
    {
        public event Action<SearchRequest>? SearchStarted;
        public event Action<SearchRequest>? SearchFinished;
        public event Action<SearchRequest>? SearchCanceled;
        public event Action<SearchRequest, Exception>? SearchFailed;
        public event Action? RequestsChanged;

        private readonly ConcurrentDictionary<Guid, SearchRequest> _requests = new();

        public IReadOnlyCollection<SearchRequest> ActiveRequests =>
            _requests.Values
                .Select(x => new SearchRequest(x))
                .ToArray();

        public int ActiveRequestCount => _requests.Count;

        public async Task<Guid?> SearchAsync(
            string url,
            SearchOptions? searchOptions = null)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;

            var request = new SearchRequest(url);

            if (!_requests.TryAdd(request.Id, request))
                return null;

            SearchStarted?.Invoke(request);
            RequestsChanged?.Invoke();

            _ = ProcessRequestAsync(request, searchOptions);

            return request.Id;
        }


        private async Task ProcessRequestAsync(
            SearchRequest request,
            SearchOptions? searchOptions)
        {
            string url = request.Url;

            try
            {
                request.CancellationToken.ThrowIfCancellationRequested();

                url = await CheckSearchAsync(
                    url,
                    request.CancellationToken);

                request.CancellationToken.ThrowIfCancellationRequested();

                var selectedPreset = SettingsService.SelectedPreset;

                YtDlpData? info = searchOptions?.VideoInfo;

                if (info == null)
                {
                    var ytdlpResult =
                        await App.YtDlp.GetVideoInfoAsync(
                            url,
                            request.CancellationToken);

                    request.CancellationToken.ThrowIfCancellationRequested();

                    if (ytdlpResult.VideoInfo != null &&
                        ytdlpResult.Code == ResultCode.Success)
                    {
                        info = ytdlpResult.VideoInfo;
                    }
                }

                if (info == null)
                {
                    LogService.Add(
                        "Video information could not be retrieved.",
                        LogTag.Warning);

                    return;
                }

                bool showFormatDialog =
                    selectedPreset == SettingsService.Presets.First() ||
                    searchOptions?.ForceDialog == true ||
                    info.Type == InfoType.Playlist;


                if (showFormatDialog)
                {
                    request.CancellationToken.ThrowIfCancellationRequested();

                    var result = await ShowFormatDialogAsync(
                        url,
                        info,
                        request.CancellationToken);

                    request.CancellationToken.ThrowIfCancellationRequested();

                    if (result == null)
                    {
                        Debug.WriteLine(
                            $"User canceled format dialog: {url}");

                        return;
                    }

                    App.DownloadService.Enqueue(
                        result.Url,
                        result.Type,
                        result.VideoInfo,
                        result.SelectedFormats);
                }
                else
                {
                    var selectedFormat = new SelectedFormat
                    {
                        Preset = selectedPreset
                    };

                    App.DownloadService.Enqueue(
                        url,
                        info.Type,
                        info,
                        [selectedFormat]);
                }
            }
            catch (OperationCanceledException)
            {
                var info = new SearchRequest(request);
                Debug.WriteLine($"Search canceled catch: {info.Url}");

                SearchCanceled?.Invoke(info);
            }
            catch (Exception ex)
            {
                var info = new SearchRequest(request);

                SearchFailed?.Invoke(info, ex);

                await KnownErrors.Check(ex);
            }
            finally
            {
                _requests.TryRemove(request.Id, out _);

                RequestsChanged?.Invoke();

                SearchFinished?.Invoke(
                    new SearchRequest(request));

                request.CancellationTokenSource.Dispose();
            }
        }

        public bool Cancel(Guid requestId)
        {
            if (!_requests.TryGetValue(requestId, out var request))
                return false;

            request.Cancel();

            return true;
        }

        public void CancelAll()
        {
            foreach (var request in _requests.Values)
            {
                request.Cancel();
            }
        }

        public void PauseDownload(DownloadItem item)
        {
            App.DownloadService.PauseDownload(item);
        }

        public void ResumeDownload(DownloadItem item)
        {
            App.DownloadService.ResumeDownload(item);
        }

        public void CancelDownload(DownloadItem item)
        {
            App.DownloadService.CancelDownload(item);
        }

        public bool TryGetRequest(
            Guid id,
            out SearchRequest? requestInfo)
        {
            if (_requests.TryGetValue(id, out var request))
            {
                requestInfo = new SearchRequest(request);
                return true;
            }

            requestInfo = null;
            return false;
        }

        private async Task<string> CheckSearchAsync(
            string url,
            CancellationToken cancellationToken)
        {
            if (url.Contains(
                    "youtube",
                    StringComparison.OrdinalIgnoreCase) &&
                url.Contains(
                    "list=",
                    StringComparison.OrdinalIgnoreCase) &&
                url.Contains(
                    "v=",
                    StringComparison.OrdinalIgnoreCase))
            {
                var tcs =
                    new TaskCompletionSource<string>(
                        TaskCreationOptions.RunContinuationsAsynchronously);

                App.UIThreadDispatcherQueue.TryEnqueue(async () =>
                {
                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var radioDialog = new BasicDialog(
                            App.LocalizationService.Get(
                                "VideoOrPlaylistDialogContent"));

                        var dialog =
                            await App.DialogService.ShowAsync(
                                new DialogOptions
                                {
                                    Title = App.LocalizationService.Get(
                                        "VideoOrPlaylistDialog"),

                                    Content = radioDialog,

                                    PrimaryButtonText =
                                        App.LocalizationService.Get(
                                            "Video"),

                                    PrimaryButtonStyle =
                                        Application.Current.Resources[
                                            "AccentButtonStyle"] as Style,

                                    CloseButtonText =
                                        App.LocalizationService.Get(
                                            "Playlist")
                                });

                        cancellationToken.ThrowIfCancellationRequested();

                        if (dialog == DialogResult.Primary)
                        {
                            var videoId =
                                url.Split("v=")[1]
                                   .Split('&')[0];

                            tcs.TrySetResult(
                                "https://www.youtube.com/watch?v=" +
                                videoId);
                        }
                        else
                        {
                            tcs.TrySetResult(url);
                        }
                    }
                    catch (Exception ex)
                    {
                        tcs.TrySetException(ex);
                    }
                });

                return await tcs.Task;
            }

            return url;
        }

        private async Task<FormatSelectionResult?> ShowFormatDialogAsync(
            string url,
            YtDlpData info,
            CancellationToken cancellationToken)
        {
            var tcs =
                new TaskCompletionSource<FormatSelectionResult?>(
                    TaskCreationOptions.RunContinuationsAsynchronously);

            App.UIThreadDispatcherQueue.TryEnqueue(async () =>
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var result =
                        await App.DialogService.ShowAsync(
                            url,
                            info);

                    cancellationToken.ThrowIfCancellationRequested();

                    tcs.TrySetResult(result);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });

            return await tcs.Task;
        }
    }
}

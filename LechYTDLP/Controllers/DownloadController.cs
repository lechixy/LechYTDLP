using LechYTDLP.Classes;
using LechYTDLP.Components;
using LechYTDLP.Services;
using LechYTDLP.Util;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LechYTDLP.Controllers
{
    public class SearchOptions
    {
        public VideoInfo? VideoInfo { get; set; }
        public bool? ForceDialog { get; set; } = false;
    }

    public class DownloadController
    {
        public event Action<bool, string>? BusyChanged;
        public event Action<string, VideoInfo>? VideoInfoReady;

        private bool _isBusy;
        private string _currentUrl = string.Empty;
        private RequestData? _requestData = null;

        public bool IsBusy => _isBusy;
        public string CurrentUrl => _currentUrl;
        public RequestData? RequestData => _requestData;

        public async Task<string> CheckSearch(string url)
        {
            // https://www.youtube.com/watch?v=jJR9v1WwIuI&list=RDjJR9v1WwIuI&start_radio=1
            // We check if the url is from because youtube keeps current video in v param, playlist in list param
            // YTdlp treats this is a playlist url but it is not, so we need to ask user if they want to download the video or the playlist
            if (url.Contains("youtube", StringComparison.OrdinalIgnoreCase) &&
                url.Contains("list=", StringComparison.OrdinalIgnoreCase) &&
                url.Contains("v=", StringComparison.OrdinalIgnoreCase))
            {
                var radioDialog = new BasicDialog(App.LocalizationService.Get("VideoOrPlaylistDialogContent"));
                var dialog = await App.DialogService.ShowAsync(new DialogOptions
                {
                    Title = App.LocalizationService.Get("VideoOrPlaylistDialog"),
                    Content = radioDialog,
                    PrimaryButtonText = App.LocalizationService.Get("Video"),
                    PrimaryButtonStyle = Application.Current.Resources["AccentButtonStyle"] as Style,
                    CloseButtonText = App.LocalizationService.Get("Playlist"),
                });

                // If the user clicks the primary button, we download the video, otherwise we download the playlist
                if (dialog == DialogResult.Primary)
                {
                    return "https://www.youtube.com/watch?v=" + url.Split("v=")[1].Split('&')[0];
                }
                else
                {
                    return url;
                }
            }

            return string.Empty;
        }

        public async Task SearchAsync(string url, SearchOptions? searchOptions = null)
        {
            if (_isBusy) return;

            // TCS ile UI thread işinin bitmesini bekle
            var tcs = new TaskCompletionSource<string>();

            App.UIThreadDispatcherQueue.TryEnqueue(async () =>
            {
                try
                {
                    var checkResult = await CheckSearch(url);
                    tcs.SetResult(checkResult == string.Empty ? url : checkResult);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });

            url = await tcs.Task;
            SetBusy(true, url);

            var videoInfo = searchOptions?.VideoInfo;

            try
            {
                VideoInfo? info = null;

                if (videoInfo == null)
                {
                    var ytdlp = new YTDLP();
                    info = await ytdlp.GetVideoInfoAsync(url);
                }
                else info = videoInfo;

                // If info is null, it means the video information couldn't be retrieved, so we exit the method.
                if (info == null)
                {
                    return;
                }

                VideoInfoReady?.Invoke(url, info);

                // If the selected preset is "illchose" show format dialog service
                if (SettingsService.SelectedPreset == SettingsService.Presets.First() || searchOptions?.ForceDialog == true)
                {
                    // TCS ile UI thread işinin bitmesini bekle
                    var ftcs = new TaskCompletionSource<FormatSelectionResult?>();

                    App.UIThreadDispatcherQueue.TryEnqueue(async () =>
                    {
                        try
                        {
                            var dialogResult = await App.DialogService.ShowAsync(url, info);
                            ftcs.SetResult(dialogResult);
                        }
                        catch (Exception ex)
                        {
                            ftcs.SetException(ex);
                        }
                    });

                    var result = await ftcs.Task;
                    if (result != null)
                    {
                        App.DownloadService.Enqueue(
                            result.Url,
                            result.VideoInfo,
                            result.SelectedFormat);
                    } else
                    {
                        Debug.WriteLine("User canceled the download dialog.");
                    }
                }
                // Otherwise, enqueue the download with the selected preset
                else
                {
                    var selectedFormat = new SelectedFormat
                    {
                        Preset = SettingsService.SelectedPreset,
                    };
                    App.DownloadService.Enqueue(
                        url,
                        info,
                        selectedFormat);
                }
            }
            catch (Exception ex)
            {
                KnownErrors.Check(ex);
            }
            finally
            {
                _requestData = null;
                SetBusy(false, url);
            }
        }

        public void SetBusy(bool value, string Url)
        {
            _isBusy = value;
            _currentUrl = Url;
            BusyChanged?.Invoke(value, Url);
        }
    }

}

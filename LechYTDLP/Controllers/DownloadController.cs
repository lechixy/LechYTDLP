using LechYTDLP.Classes;
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
    public class DownloadController
    {
        public event Action<bool, string>? BusyChanged;
        public event Action<string, VideoInfo>? VideoInfoReady;
        public event Action<string>? ErrorOccured;

        private bool _isBusy;
        private string _currentUrl = string.Empty;
        private RequestData? _requestData = null;

        public bool IsBusy => _isBusy;
        public string CurrentUrl => _currentUrl;
        public RequestData? RequestData => _requestData;

        public async Task SearchAsync(string url, VideoInfo? videoInfo = null)
        {
            if (_isBusy) return;

            SetBusy(true, url);

            try
            {
                VideoInfo? info = null;

                if (videoInfo == null)
                {
                    var ytdlp = new YTDLP();
                    info = await ytdlp.GetVideoInfoAsync(url);
                }
                else info = videoInfo;

                if (info != null)
                {
                    VideoInfoReady?.Invoke(url, info);

                    var result = await App.FormatDialogService.ShowAsync(url, info);

                    if (result != null)
                    {
                        App.DownloadService.Enqueue(
                            result.Url,
                            result.VideoInfo,
                            result.SelectedFormat);

                        AppNotification notification = new AppNotificationBuilder()
                            .AddText("Download Queued")
                            .AddText(result.VideoInfo.Title ?? "Unknown Title")
                            .SetInlineImage(new Uri(WebUtility.HtmlEncode(result.VideoInfo.Thumbnail!)))
                            .BuildNotification();

                        AppNotificationManager.Default.Show(notification);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorOccured?.Invoke(ex.Message);
            }
            finally
            {
                _requestData = null;
                SetBusy(false, url);
            }
        }

        private void SetBusy(bool value, string Url)
        {
            _isBusy = value;
            _currentUrl = Url;
            BusyChanged?.Invoke(value, Url);
        }
    }

}

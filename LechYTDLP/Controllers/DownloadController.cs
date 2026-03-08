using LechYTDLP.Classes;
using LechYTDLP.Util;
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

                // If info is null, it means the video information couldn't be retrieved, so we exit the method.
                if (info == null)
                {
                    return;
                }

                VideoInfoReady?.Invoke(url, info);

                var result = await App.FormatDialogService.ShowAsync(url, info);

                if (result != null)
                {
                    App.DownloadService.Enqueue(
                        result.Url,
                        result.VideoInfo,
                        result.SelectedFormat);
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

        private void SetBusy(bool value, string Url)
        {
            _isBusy = value;
            _currentUrl = Url;
            BusyChanged?.Invoke(value, Url);
        }
    }

}

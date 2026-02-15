using LechYTDLP.Classes;
using LechYTDLP.Components;
using LechYTDLP.Util;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LechYTDLP.Services
{
    public class FormatSelectionResult
    {
        public string Url { get; set; } = default!;
        public VideoInfo VideoInfo { get; set; } = default!;
        public SelectedFormat SelectedFormat { get; set; } = default!;
    }

    public class FormatDialogService
    {
        private readonly Window _window;

        public FormatDialogService(Window window)
        {
            _window = window;
        }

        public async Task<FormatSelectionResult?> ShowAsync(string url, VideoInfo videoInfo)
        {
            var tcs = new TaskCompletionSource<FormatSelectionResult?>();

            _window.DispatcherQueue.TryEnqueue(async () =>
            {
                try
                {
                    var content = new SelectFormat();
                    content.SetData(videoInfo);

                    var dialog = new ContentDialog
                    {
                        Title = "Select format to download",
                        Content = content,
                        PrimaryButtonText = "Save",
                        PrimaryButtonStyle = Application.Current.Resources["AccentButtonStyle"] as Style,
                        CloseButtonText = "Cancel",
                        IsPrimaryButtonEnabled = false,
                        XamlRoot = _window.Content.XamlRoot,
                        Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style
                    };

                    content.IsUserCanSave += canSave =>
                    {
                        dialog.IsPrimaryButtonEnabled = canSave;
                    };

                    var result = await dialog.ShowAsync();

                    if (result == ContentDialogResult.Primary)
                    {
                        tcs.SetResult(new FormatSelectionResult
                        {
                            Url = url,
                            VideoInfo = videoInfo,
                            SelectedFormat = content.SelectedFormat
                        });
                    }
                    else
                    {
                        tcs.SetResult(null);
                    }
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });

            return await tcs.Task;
        }
    }

}

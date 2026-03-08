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

    public class FormatDialogService(Window window)
    {
        private readonly Window _window = window;

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
                        Title = App.LocalizationService.Get("SelectFormat"),
                        Content = content,
                        PrimaryButtonText = App.LocalizationService.Get("Save"),
                        PrimaryButtonStyle = Application.Current.Resources["AccentButtonStyle"] as Style,
                        CloseButtonText = App.LocalizationService.Get("Cancel"),
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
                    Debug.WriteLine(ex);
                    tcs.TrySetResult(null);
                }
            });

            return await tcs.Task;
        }
    }

}

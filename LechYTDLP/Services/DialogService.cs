using CommunityToolkit.WinUI;
using LechYTDLP.Classes;
using LechYTDLP.Components;
using LechYTDLP.Util;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LechYTDLP.Services
{
    public enum DialogResult
    {
        Primary,
        Secondary,
        Close
    }

    public class DialogOptions
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public UserControl? Content { get; set; } = null;

        public string PrimaryButtonText { get; set; } = "OK";
        public Style? PrimaryButtonStyle { get; set; }
        public string? SecondaryButtonText { get; set; }
        public string? CloseButtonText { get; set; }

        public bool IsPrimaryEnabled { get; set; } = true;
        public bool IsSecondaryEnabled { get; set; } = false;

        // Special Dialog
        public bool IsPlayerDialog { get; set; } = false;

        public ContentDialogButton DefaultButton { get; set; } = ContentDialogButton.Primary;
    }

    public class DialogService
    {
        private readonly Window _window;
        private readonly SemaphoreSlim _dialogLock = new(1, 1);

        public DialogService(Window window)
        {
            _window = window;
        }

        public async Task<DialogResult> ShowAsync(DialogOptions options)
        {
            await _dialogLock.WaitAsync();

            try
            {
                var xamlRoot = await GetXamlRootAsync();

                var tcs = new TaskCompletionSource<DialogResult>();

                _window.DispatcherQueue.TryEnqueue(async () =>
                {
                    try
                    {
                        var dialog = new ContentDialog
                        {
                            Title = options.Title,
                            Content = new TextBlock
                            {
                                Text = options.Message,
                                TextWrapping = TextWrapping.Wrap
                            },
                            CloseButtonText = options.CloseButtonText ?? App.LocalizationService.Get("Cancel"),
                            Style = (Style)Application.Current.Resources["DefaultContentDialogStyle"],
                            XamlRoot = xamlRoot
                        };

                        if (options.Content != null)
                        {
                            dialog.Content = options.Content;
                        }

                        if (options.PrimaryButtonStyle != null)
                        {
                            dialog.PrimaryButtonStyle = options.PrimaryButtonStyle;
                        }

                        if (options.IsPrimaryEnabled)
                        {
                            dialog.IsPrimaryButtonEnabled = true;
                            dialog.PrimaryButtonText = options.PrimaryButtonText;
                        }

                        if (options.IsSecondaryEnabled)
                        {
                            dialog.IsSecondaryButtonEnabled = true;
                            dialog.SecondaryButtonText = options.SecondaryButtonText;
                        }

                        // Special handling for player dialog
        //                if (options.IsPlayerDialog)
        //                {
        //                    dialog.Style = (Style)Application.Current.Resources["PlayerDialogStyle"];

        //                    dialog.IsPrimaryButtonEnabled = false;
        //                    dialog.IsSecondaryButtonEnabled = false;
        //                    dialog.CloseButtonText = null;
        //                    dialog.DefaultButton = ContentDialogButton.None;

        //                    dialog.Background = new SolidColorBrush(Colors.Black);

        //                    // 🔥 KRİTİK KISIM
        //                    dialog.Content = new Grid
        //                    {
        //                        HorizontalAlignment = HorizontalAlignment.Stretch,
        //                        VerticalAlignment = VerticalAlignment.Stretch,
        //                        Children =
        //{
        //    new Grid
        //    {
        //        HorizontalAlignment = HorizontalAlignment.Center,
        //        VerticalAlignment = VerticalAlignment.Center,
        //        Children =
        //        {
        //            options.Content // MediaPlayerElement burada
        //        }
        //    }
        //}
        //                    };
        //                }

                        var result = await dialog.ShowAsync();

                        tcs.SetResult(result switch
                        {
                            ContentDialogResult.Primary => DialogResult.Primary,
                            ContentDialogResult.Secondary => DialogResult.Secondary,
                            _ => DialogResult.Close
                        });
                    }
                    catch (Exception ex)
                    {
                        tcs.SetException(ex);
                    }
                });

                return await tcs.Task;
            }
            finally
            {
                _dialogLock.Release();
            }
        }

        private async Task<XamlRoot> GetXamlRootAsync()
        {
            var fe = _window.Content as FrameworkElement;

            if (fe == null)
                throw new InvalidOperationException("Window.Content is not FrameworkElement");

            if (fe.XamlRoot != null)
                return fe.XamlRoot;

            var tcs = new TaskCompletionSource();

            void handler(object s, RoutedEventArgs e)
            {
                fe.Loaded -= handler;
                tcs.SetResult();
            }

            fe.Loaded += handler;

            await tcs.Task;

            return fe.XamlRoot!;
        }
    }
}

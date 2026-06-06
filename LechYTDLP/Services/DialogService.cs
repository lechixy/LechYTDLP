using CommunityToolkit.WinUI;
using LechYTDLP.Classes;
using LechYTDLP.Components;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
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

    public enum DialogType
    {
        Normal,
        Format
    }

    public class DialogOptions
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public UserControl? Content { get; set; }

        public string PrimaryButtonText { get; set; } = "OK";
        public Style? PrimaryButtonStyle { get; set; }
        public string? SecondaryButtonText { get; set; }
        public string? CloseButtonText { get; set; }

        public bool IsPrimaryEnabled { get; set; } = true;
        public bool IsSecondaryEnabled { get; set; } = false;

        public bool IsPlayerDialog { get; set; } = false;

        public ContentDialogButton DefaultButton { get; set; } = ContentDialogButton.Primary;
    }

    public class FormatSelectionResult
    {
        public string Url { get; set; } = default!;
        public VideoInfo VideoInfo { get; set; } = default!;
        public SelectedFormat SelectedFormat { get; set; } = default!;
    }

    public class DialogService
    {
        private readonly Window _window;

        private readonly SemaphoreSlim _processLock = new(1, 1);

        private readonly ConcurrentQueue<Func<Task>> _dialogQueue = new();

        private bool _isProcessing;

        public DialogType? CurrentDialogType { get; private set; }

        public DialogService(Window window)
        {
            _window = window;
        }

        #region Normal Dialog

        public async Task<DialogResult> ShowAsync(DialogOptions options)
        {
            var tcs = new TaskCompletionSource<DialogResult>();

            _dialogQueue.Enqueue(async () =>
            {
                CurrentDialogType = DialogType.Normal;

                try
                {
                    var result = await ShowNormalDialogInternalAsync(options);
                    tcs.TrySetResult(result);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
                finally
                {
                    CurrentDialogType = null;
                }
            });

            await ProcessQueueAsync();

            return await tcs.Task;
        }

        #endregion

        #region Format Dialog

        public async Task<FormatSelectionResult?> ShowAsync(string url, VideoInfo videoInfo)
        {
            var tcs = new TaskCompletionSource<FormatSelectionResult?>();

            _dialogQueue.Enqueue(async () =>
            {
                CurrentDialogType = DialogType.Format;

                try
                {
                    var result = await ShowFormatDialogInternalAsync(url, videoInfo);
                    tcs.TrySetResult(result);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    tcs.TrySetResult(null);
                }
                finally
                {
                    CurrentDialogType = null;
                }
            });

            await ProcessQueueAsync();

            return await tcs.Task;
        }

        #endregion

        #region Queue

        private async Task ProcessQueueAsync()
        {
            await _processLock.WaitAsync();

            try
            {
                if (_isProcessing)
                    return;

                _isProcessing = true;
            }
            finally
            {
                _processLock.Release();
            }

            while (_dialogQueue.TryDequeue(out var dialogTask))
            {
                try
                {
                    await dialogTask();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }

            _isProcessing = false;
        }

        #endregion

        #region Internal Dialog Methods

        private async Task<DialogResult> ShowNormalDialogInternalAsync(DialogOptions options)
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
                        XamlRoot = xamlRoot,
                        DefaultButton = options.DefaultButton
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

                    var result = await dialog.ShowAsync();

                    tcs.TrySetResult(result switch
                    {
                        ContentDialogResult.Primary => DialogResult.Primary,
                        ContentDialogResult.Secondary => DialogResult.Secondary,
                        _ => DialogResult.Close
                    });
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });

            return await tcs.Task;
        }

        private async Task<FormatSelectionResult?> ShowFormatDialogInternalAsync(string url, VideoInfo videoInfo)
        {
            var xamlRoot = await GetXamlRootAsync();

            var tcs = new TaskCompletionSource<FormatSelectionResult?>();

            _window.DispatcherQueue.TryEnqueue(async () =>
            {
                try
                {
                    var content = new SelectFormat(videoInfo);

                    var dialog = new ContentDialog
                    {
                        Title = App.LocalizationService.Get("SelectFormat"),
                        Content = content,
                        PrimaryButtonText = App.LocalizationService.Get("Save"),
                        PrimaryButtonStyle = Application.Current.Resources["AccentButtonStyle"] as Style,
                        CloseButtonText = App.LocalizationService.Get("Cancel"),
                        IsPrimaryButtonEnabled = false,
                        XamlRoot = xamlRoot,
                        Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style
                    };

                    content.IsUserCanSave += canSave =>
                    {
                        dialog.IsPrimaryButtonEnabled = canSave;
                    };

                    var result = await dialog.ShowAsync();

                    if (result == ContentDialogResult.Primary)
                    {
                        tcs.TrySetResult(new FormatSelectionResult
                        {
                            Url = url,
                            VideoInfo = videoInfo,
                            SelectedFormat = content.SelectedFormat
                        });
                    }
                    else
                    {
                        // User cancelled the dialog
                        tcs.TrySetResult(null);
                        // Set current url to cancel the ongoing download
                        //App.DownloadController.CurrentUrl = ;
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

        #endregion

        #region Helpers

        private async Task<XamlRoot> GetXamlRootAsync()
        {
            var fe = _window.Content as FrameworkElement;

            if (fe == null)
                throw new InvalidOperationException("Window.Content is not FrameworkElement");

            if (fe.XamlRoot != null)
                return fe.XamlRoot;

            var tcs = new TaskCompletionSource();

            void Handler(object s, RoutedEventArgs e)
            {
                fe.Loaded -= Handler;
                tcs.SetResult();
            }

            fe.Loaded += Handler;

            await tcs.Task;

            return fe.XamlRoot!;
        }

        #endregion
    }
}
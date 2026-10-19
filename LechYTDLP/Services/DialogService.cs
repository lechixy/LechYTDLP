using CommunityToolkit.WinUI;
using LechYTDLP.Classes;
using LechYTDLP.Components;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

        public double? MaxWidth { get; set; }

        public ContentDialogButton DefaultButton { get; set; } = ContentDialogButton.Primary;
    }

    public class FormatSelectionResult
    {
        public string Url { get; set; } = default!;
        public InfoType Type { get; set; } = default!;
        public YtDlpData VideoInfo { get; set; } = default!;
        public SelectedFormat[] SelectedFormats { get; set; } = [];
    }

    public class DialogService
    {
        private readonly Window _window;

        private readonly SemaphoreSlim _processLock = new(1, 1);

        private readonly LinkedList<Func<Task>> _dialogQueue = new();

        private readonly object _queueLock = new();

        private bool _isProcessing;

        public DialogType? CurrentDialogType { get; private set; }

        public DialogService(Window window)
        {
            _window = window;
        }

        #region Normal Dialog

        public async Task<DialogResult> ShowAsync(DialogOptions options, bool isPriority = false)
        {
            var tcs = new TaskCompletionSource<DialogResult>();

            Func<Task> dialogTask = async () =>
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
            };

            lock (_queueLock)
            {
                if (isPriority)
                    _dialogQueue.AddFirst(dialogTask);
                else
                    _dialogQueue.AddLast(dialogTask);
            }

            await ProcessQueueAsync();

            return await tcs.Task;
        }

        #endregion

        #region Format Dialog

        public async Task<FormatSelectionResult?> ShowAsync(string url, YtDlpData videoInfo, bool isPriority = false)
        {
            var tcs = new TaskCompletionSource<FormatSelectionResult?>();

            Func<Task> dialogTask = async () =>
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
            };

            lock (_queueLock)
            {
                if (isPriority)
                    _dialogQueue.AddFirst(dialogTask);
                else
                    _dialogQueue.AddLast(dialogTask);
            }

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

            while (true)
            {
                Func<Task>? dialogTask = null;

                lock (_queueLock)
                {
                    if (_dialogQueue.Count > 0)
                    {
                        dialogTask = _dialogQueue.First!.Value;
                        _dialogQueue.RemoveFirst();
                    }
                }

                if (dialogTask == null)
                    break;

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

                    ScrollViewer.SetHorizontalScrollBarVisibility(dialog, ScrollBarVisibility.Disabled);

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

        private async Task<FormatSelectionResult?> ShowFormatDialogInternalAsync(string url, YtDlpData videoInfo)
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
                        Title = content.Title,
                        Content = content,
                        PrimaryButtonText = App.LocalizationService.Get("Download"),
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
                            Type = videoInfo.Type,
                            VideoInfo = videoInfo,
                            SelectedFormats = content.SelectedFormats
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
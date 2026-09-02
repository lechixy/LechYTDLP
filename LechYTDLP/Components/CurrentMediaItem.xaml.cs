using LechYTDLP.Classes;
using LechYTDLP.Controllers;
using LechYTDLP.Services;
using LechYTDLP.Util;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Sentry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LechYTDLP.Components
{
    public sealed partial class CurrentMediaItem : UserControl
    {
        private DownloadItem? _currentItem;

        public CurrentMediaItem()
        {
            InitializeComponent();

            Unloaded += CurrentMediaItem_Unloaded;
            }


        public static readonly DependencyProperty ItemProperty =
            DependencyProperty.Register(nameof(Item), typeof(DownloadItem), typeof(CurrentMediaItem), new PropertyMetadata(null, OnItemChanged));

        public DownloadItem Item
        {
            get => (DownloadItem)GetValue(ItemProperty);
            set => SetValue(ItemProperty, value);
        }

        private static void OnItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not CurrentMediaItem control)
                return;

            // Eski item'dan çık
            if (control._currentItem != null)
                control._currentItem.Changed -= control.OnDownloadItemChanged;

            // Yeni item'a geç
            control._currentItem = e.NewValue as DownloadItem;

            if (control._currentItem != null)
            {
                control._currentItem.Changed += control.OnDownloadItemChanged;

                // İlk yükleme
                control.UpdateUI(control._currentItem);
            }
        }

        private void OnDownloadItemChanged(object? sender, EventArgs e)
        {
            if (sender is not DownloadItem item)
                return;

            DispatcherQueue.TryEnqueue(() =>
            {
                try
                {
                    UpdateUI(item);
                }
                catch (Exception ex)
                {
                    LogService.Add($"Error updating UI for item {item.Info.Title}: {ex.Message}", LogTag.Error);
                    SentrySdk.CaptureException(ex);
                }
            });
        }

        private void CurrentMediaItem_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_currentItem != null)
                _currentItem.Changed -= OnDownloadItemChanged;
        }

        private void UpdateUI(DownloadItem item)
        {
            var info = item.Info;

            CurrentVideoStatus.Style = Application.Current.Resources["CaptionTextBlockStyle"] as Style;
            CurrentVideoStatus.Foreground = Application.Current.Resources["AccentTextFillColorPrimaryBrush"] as SolidColorBrush;

            if (item.State == DownloadState.Downloading)
            {
                CurrentVideoProgress.ShowPaused = false;

                PauseOrResumeButton.IsEnabled = true;
                PauseOrResumeButton.Content = new SymbolIcon { Symbol = Symbol.Pause };
                CancelButton.IsEnabled = true;
            }
            else if (item.State == DownloadState.TestingFormat)
            {
                CurrentVideoProgress.ShowPaused = true;
                CurrentVideoStatus.Foreground = Application.Current.Resources["SystemFillColorCautionBrush"] as SolidColorBrush;
            }
            else if (item.State == DownloadState.Paused || (CurrentVideoProgress.ShowPaused && item.State == DownloadState.Queued))
            {
                CurrentVideoProgress.ShowPaused = true;
                CurrentVideoStatus.Foreground = Application.Current.Resources["SystemFillColorCautionBrush"] as SolidColorBrush;

                PauseOrResumeButton.IsEnabled = true;
                PauseOrResumeButton.Content = new SymbolIcon { Symbol = Symbol.Play };

                // TODO: Needs to be enabled to allow canceling a paused download
                CancelButton.IsEnabled = false;
            }
            else
            {
                PauseOrResumeButton.IsEnabled = false;
                CancelButton.IsEnabled = false;
            }

            CurrentMediaContainer.Visibility = Visibility.Visible;

            string thumbUrl = string.IsNullOrEmpty(item.Info.BestThumbnailUrl) ? "https://placehold.co/320x180.png?text=No+Thumbnail" : item.Info.BestThumbnailUrl;
            if (!(CurrentThumbnailImage.Source is BitmapImage bmp && bmp.UriSource != null && bmp.UriSource.ToString() == thumbUrl))
            {
                CurrentThumbnailImage.Source = new BitmapImage(new Uri(thumbUrl));
            }

            CurrentVideoTitle.Text = info.Title ?? App.LocalizationService.Get("UnknownTitle");

            CurrentVideoUploaderAndSavingTo.Blocks.Clear();
            var p = new Paragraph();
            p.Inlines.Add(new Run { Text = $"@{info.Uploader}" ?? App.LocalizationService.Get("UnknownUploader") });
            p.Inlines.Add(new Run { Text = $" • {App.LocalizationService.Get("SavingTo", SettingsService.DownloadPath)}" });
            CurrentVideoUploaderAndSavingTo.Blocks.Add(p);

            //CurrentVideoUploaderAndSavingTo.Text = $"{info.uploader ?? "Unknown Uploader"} - Saving to {SettingsService.DownloadPath}";

            //// Metadata
            //var metadataItem = new List<string>();

            //if (item.Type == InfoType.Playlist)
            //{
            //    metadataItem.Add(App.LocalizationService.Get("DownloadingItemOf", item.Meta.PlaylistCurrentIndex, item.Info.PlaylistCount ?? 0));
            //}

            //if (metadataItem.Count > 0)
            //{
            //    CurrentVideoMetadata.Visibility = Visibility.Visible;
            //    CurrentVideoMetadata.Text = string.Join(" • ", metadataItem);
            //}
            //else CurrentVideoMetadata.Visibility = Visibility.Collapsed;

            CurrentVideoStatus.Text = item.State switch
            {
                DownloadState.Queued => App.LocalizationService.Get("StatusQueued"),
                DownloadState.Downloading => item.Type == InfoType.Video ?
                    App.LocalizationService.Get("StatusDownloading") : App.LocalizationService.Get("StatusDownloadingOf", item.Meta.PlaylistCurrentIndex, item.Info.PlaylistCount ?? 0),
                DownloadState.Completed => App.LocalizationService.Get("StatusCompleted"),
                DownloadState.PartiallyCompleted => App.LocalizationService.Get("StatusPartiallyCompleted"),
                DownloadState.Failed => App.LocalizationService.Get("StatusFailed"),
                DownloadState.Paused => App.LocalizationService.Get("StatusPaused"),
                DownloadState.Resuming => App.LocalizationService.Get("StatusResuming"),
                DownloadState.TestingFormat => App.LocalizationService.Get("StatusTestingFormat"),
                _ => App.LocalizationService.Get("StatusQueued")
            };
            CurrentVideoProgress.Value = item.Progress;
        }

        private async void OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                if (button.Name == "PauseOrResumeButton")
                {
                    Debug.WriteLine($"PauseOrResumeButton clicked for item: {Item.Info.Title}");

                    if (Item.State == DownloadState.Downloading)
                    {
                        App.DownloadController.PauseDownload(Item);
                    }
                    else if (Item.State == DownloadState.Paused)
                    {
                        App.DownloadController.ResumeDownload(Item);
                    }
                }
                else if (button.Name == "CancelButton")
                {
                    Debug.WriteLine($"CancelButton clicked for item: {Item.Info.Title}");
                    App.DownloadController.CancelDownload(Item);
                }
            }
        }
    }
}
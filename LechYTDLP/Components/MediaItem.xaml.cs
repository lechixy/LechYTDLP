using CommunityToolkit.WinUI.Controls;
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
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LechYTDLP.Components
{
    public sealed partial class MediaItem : UserControl
    {
        public MediaItem()
        {
            this.InitializeComponent();
        }

        public static readonly DependencyProperty ItemProperty =
            DependencyProperty.Register(nameof(Item), typeof(DownloadItem), typeof(MediaItem), new PropertyMetadata(null, OnItemChanged));

        public DownloadItem Item
        {
            get => (DownloadItem)GetValue(ItemProperty);
            set => SetValue(ItemProperty, value);
        }

        private static void OnItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MediaItem control && e.NewValue is DownloadItem newItem)
            {
                control.UpdateUI(newItem);
            }
        }

        private void UpdateUI(DownloadItem item)
        {
            if (item?.Info == null) return;

            QueueMediaItemTitle.Text = item.Info.Title ?? App.LocalizationService.Get("UnknownTitle");

            string thumbUrl = item.Info.Thumbnail ?? "https://placehold.co/320x180.png?text=No+Thumbnail";
            if (QueueMediaItemThumbnail.Source is not BitmapImage bmp || bmp.UriSource?.ToString() != thumbUrl)
            {
                QueueMediaItemThumbnail.Source = new BitmapImage(new Uri(thumbUrl));
            }

            string saveStatus = item.State switch
            {
                DownloadState.Completed => App.LocalizationService.Get("SavedTo", item.FilePath),
                DownloadState.Failed => "Failed",
                _ => App.LocalizationService.Get("SavingTo", SettingsService.DownloadPath)
            };

            QueueMediaItemUploaderAndSavingTo.Blocks.Clear();
            var p = new Paragraph();
            p.Inlines.Add(new Run { Text = $"@{item.Info.Uploader}" ?? App.LocalizationService.Get("UnknownUploader") });
            if (item.State != DownloadState.Failed && item.FilePath != null)
            {
                p.Inlines.Add(new Run { Text = $" • {saveStatus}" });
            }
            QueueMediaItemUploaderAndSavingTo.Blocks.Add(p);

            var metadataItem = new List<string>();

            if (item.SelectedFormat.Preset == null)
            {
                if (item.SelectedFormat.SelectedVideo != null)
                {
                    metadataItem.Add(App.LocalizationService.Get("Video"));
                    metadataItem.Add(item.SelectedFormat.SelectedVideo.VideoExt);
                    metadataItem.Add(item.SelectedFormat.SelectedVideo.Resolution);
                }
                else if (item.SelectedFormat.Audio != null)
                {
                    metadataItem.Add(App.LocalizationService.Get("Audio"));
                    metadataItem.Add(item.SelectedFormat.AudioFileExtension);
                    metadataItem.Add(item.SelectedFormat.Audio);
                }
            }
            else
            {
                metadataItem.Add(SettingsService.Presets.FirstOrDefault(p => p.Value == item.SelectedFormat.Preset.Value)?.DisplayName ?? App.LocalizationService.Get("UnknownPreset"));
            }

            QueueMediaItemMetadata.Text = string.Join(" • ", metadataItem);

            QueueMediaItemStatus.Text = item.State switch
            {
                DownloadState.Queued => App.LocalizationService.Get("StatusQueued"),
                DownloadState.Downloading => App.LocalizationService.Get("StatusDownloading"),
                DownloadState.Completed => App.LocalizationService.Get("StatusCompleted"),
                DownloadState.Failed => App.LocalizationService.Get("StatusFailed"),
                DownloadState.Paused => App.LocalizationService.Get("StatusPaused"),
                DownloadState.Resuming => App.LocalizationService.Get("StatusResuming"),
                DownloadState.TestingFormat => App.LocalizationService.Get("StatusTestingFormat"),
                _ => App.LocalizationService.Get("StatusQueued")
            };


            if (item.State == DownloadState.Paused || item.State == DownloadState.TestingFormat)
            {
                QueueMediaItemStatus.Foreground = Application.Current.Resources["SystemFillColorCautionBrush"] as Brush;

                // Hide "Retry" button for other statuses
                QueueMediaItemRetryButton.Visibility = Visibility.Collapsed;
            }
            else if (item.State == DownloadState.Failed)
            {
                QueueMediaItemStatus.Foreground = Application.Current.Resources["SystemFillColorCriticalBrush"] as Brush;

                // Show "Retry" button for failed downloads
                QueueMediaItemRetryButton.Visibility = Visibility.Visible;
            }
            else
            {
                QueueMediaItemStatus.Foreground = Application.Current.Resources["AccentTextFillColorPrimaryBrush"] as Brush;

                // Hide "Retry" button for other statuses
                QueueMediaItemRetryButton.Visibility = Visibility.Collapsed;
            }
        }

        private async void Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                if (button.Name == "QueueMediaItem")
                {
                    var path = Item.FilePath;
                    if (string.IsNullOrEmpty(path) == false)
                    {
                        if (File.Exists(path) == false && Directory.Exists(path) == false)
                        {
                            KnownErrors.ShowGenericError(KnownErrors.GenericError.NoFileOrDirectory);
                            return;
                        }

                        if (SettingsService.OpenFilesInExternalPlayer)
                        {
                            try
                            {
                                Process.Start(new ProcessStartInfo(path)
                                {
                                    UseShellExecute = true
                                });
                            }
                            catch (Exception)
                            {
                                KnownErrors.ShowGenericError(KnownErrors.GenericError.NoFileOrDirectory);
                            }
                            return;
                        }
                        else
                        {
                            PlayerController.PlayMediaItem(Item);
                        }
                    }
                }
                if (button.Name == "QueueMediaItemRetryButton")
                {
                    await App.DownloadController.SearchAsync(Item.Url, new SearchOptions { VideoInfo = Item.Info, ForceDialog = true });
                    App.InfoBarService.Show(new InfoBarMessage
                    {
                        Title = App.LocalizationService.Get("InfoBarRetryTitle"),
                        Message = "",
                        Severity = InfoBarSeverity.Informational
                    });
                }
            }
        }
    }
}

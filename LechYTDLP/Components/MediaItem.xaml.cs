using LechYTDLP.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
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
using Windows.Foundation;
using Windows.Foundation.Collections;

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

            QueueMediaItemTitle.Text = item.Info.Title ?? "Unknown Title";

            string thumbUrl = item.Info.Thumbnail ?? "https://placehold.co/320x180.png?text=No+Thumbnail";
            if (QueueMediaItemThumbnail.Source is not BitmapImage bmp || bmp.UriSource?.ToString() != thumbUrl)
            {
                QueueMediaItemThumbnail.Source = new BitmapImage(new Uri(thumbUrl));
            }

            string uploader = item.Info.Uploader ?? "Unknown Uploader";
            string saveStatus = item.State == DownloadState.Completed
                ? $"Saved to {item.FilePath}"
                : $"Saving to {SettingsService.DownloadPath}";

            QueueMediaItemUploaderAndSavingTo.Text = $"{uploader} • {saveStatus}";

            QueueMediaItemStatus.Text = item.State.ToString();


            if (item.State == DownloadState.Paused)
            {
                QueueMediaItemStatus.Foreground = Application.Current.Resources["SystemFillColorCautionBrush"] as Brush;
            }
            else
            {
                QueueMediaItemStatus.Foreground = Application.Current.Resources["AccentTextFillColorPrimaryBrush"] as Brush;
            }
        }

        //Medya öğesine tıklandığında yapılacak işlemler
        //Örneğin, detay sayfasına yönlendirme veya oynatma işlemi
        private void QueueMediaItem_Click(object sender, RoutedEventArgs e)
        {
            var path = Item.FilePath;
            if (string.IsNullOrEmpty(path) == false)
            {
                try
                {
                    Process.Start(new ProcessStartInfo(path)
                    {
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Dosya açılamadı: {ex.Message}");
                    App.InfoBarService.Show(new InfoBarMessage(
                        "File cannot be opened",
                        $"The file at {path} cannot be opened. It may have been moved or deleted.",
                        InfoBarSeverity.Error,
                        5000,
                        false
                    ));
                }
            }
        }
    }
}

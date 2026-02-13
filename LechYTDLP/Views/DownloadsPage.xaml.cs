using LechYTDLP.Classes;
using LechYTDLP.Components;
using LechYTDLP.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LechYTDLP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DownloadsPage : Page
    {

        public ObservableCollection<DownloadItem> QueueCollection { get; } = [];
        public ObservableCollection<DownloadItem> HistoryCollection { get; } = [];

        public DownloadsPage()
        {
            this.InitializeComponent();

            Unloaded += DownloadsPage_Unloaded;

            if (App.DownloadService != null)
            {
                App.DownloadService.QueueUpdated += QueueUpdated;
                App.DownloadService.CurrentMediaUpdated += CurrentMediaUpdated;
            }

            if (QueueListView != null)
            {
                QueueListView.ItemsSource = QueueCollection;
            }
            if (HistoryListView != null)
            {
                HistoryListView.ItemsSource = HistoryCollection;
            }

            // burda da çağırıyoruz ki sayfa açıldığında güncel veriler gelsin
            UpdateCurrentVideo();
            UpdateCurrentQueue();
            DispatcherQueue.TryEnqueue(async () => await UpdateHistoryQueue(true));
        }

        private void QueueUpdated()
        {
            DispatcherQueue.TryEnqueue(UpdateCurrentQueue);
        }

        private void HistoryUpdated()
        {
            DispatcherQueue.TryEnqueue(() => _ = UpdateHistoryQueue());
        }

        private void CurrentMediaUpdated()
        {
            DispatcherQueue.TryEnqueue(UpdateCurrentVideo);
        }

        public void UpdateCurrentQueue()
        {
            if (App.DownloadService?.Queue == null) return;
            Debug.WriteLine("QUEUE UPDATED");

            try
            {
                // Listeyi kopyalıyoruz (Thread Safety)
                var snapshot = App.DownloadService.Queue.ToList();

                if ((snapshot.Count - 1) > 0) QueueTitleText.Visibility = Visibility.Visible;
                else QueueTitleText.Visibility = Visibility.Collapsed;

                if (snapshot.Count != 0) snapshot.RemoveAt(0); // İlk öğe CurrentMedia olduğu için kaldırıyoruz

                QueueCollection.Clear();
                foreach (var item in snapshot)
                {
                    QueueCollection.Add(item);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Queue Error: {ex.Message}");
            }
        }

        public async Task UpdateHistoryQueue(bool getHistoryFromDatabase = false)
        {
            if (App.DownloadService?.History == null) return;
            Debug.WriteLine("Updating history");

            try
            {
                List<DownloadItem> snapshot;

                if (getHistoryFromDatabase)
                {
                    snapshot = await App.DatabaseService.GetAllAsync();
                    //snapshot.Reverse();
                }
                else
                {
                    snapshot = [.. App.DownloadService.History];
                }

                // If there are items in the history
                if (snapshot.Count > 0)
                {
                    HistoryTitleText.Visibility = Visibility.Visible;
                    ClearHistoryButton.IsEnabled = true;
                    ClearHistoryButton.Visibility = Visibility.Visible;

                    // Show list view again
                    HistoryListView.Visibility = Visibility.Visible;
                }
                else
                {
                    HistoryTitleText.Visibility = Visibility.Collapsed;
                    ClearHistoryButton.IsEnabled = false;
                    ClearHistoryButton.Visibility = Visibility.Collapsed;

                    // Hide list view if no history
                    HistoryListView.Visibility = Visibility.Collapsed;
                }

                HistoryCollection.Clear();
                foreach (var item in snapshot)
                {
                    HistoryCollection.Add(item);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"History Queue Error: {ex.Message}");
            }
        }

        public void UpdateCurrentVideo()
        {
            var currentDownload = App.DownloadService.CurrentMedia;

            if (currentDownload != null)
            {
                var info = currentDownload.Info;

                CurrentVideoStatus.Style = Application.Current.Resources["CaptionTextBlockStyle"] as Style;
                CurrentVideoStatus.Foreground = Application.Current.Resources["AccentTextFillColorPrimaryBrush"] as SolidColorBrush;

                if (currentDownload.State == DownloadState.Downloading)
                {
                    CurrentVideoProgress.ShowPaused = false;

                    PauseOrResumeButton.IsEnabled = true;
                    PauseOrResumeButton.Content = new SymbolIcon(Symbol.Pause);
                }
                else if (currentDownload.State == DownloadState.Paused || (CurrentVideoProgress.ShowPaused && currentDownload.State == DownloadState.Queued))
                {
                    CurrentVideoProgress.ShowPaused = true;
                    CurrentVideoStatus.Foreground = Application.Current.Resources["SystemFillColorCautionBrush"] as SolidColorBrush;

                    PauseOrResumeButton.IsEnabled = true;
                    PauseOrResumeButton.Content = new SymbolIcon(Symbol.Play);
                }
                else
                {
                    PauseOrResumeButton.IsEnabled = false;
                }

                CurrentMediaContainer.Visibility = Visibility.Visible;
                NoQueueContainer.Visibility = Visibility.Collapsed;

                if (!(CurrentThumbnailImage.Source is BitmapImage bmp && bmp.UriSource != null && bmp.UriSource.ToString() == (info.Thumbnail ?? "https://placehold.co/320x180.png?text=No+Thumbnail")))
                {
                    CurrentThumbnailImage.Source = new BitmapImage(new Uri(info.Thumbnail ?? "https://placehold.co/320x180.png?text=No+Thumbnail"));
                }

                CurrentVideoTitle.Text = info.Title ?? "Unknown Title";

                CurrentVideoUploaderAndSavingTo.Blocks.Clear();
                var p = new Paragraph();
                p.Inlines.Add(new Run { Text = info.Uploader ?? "Unknown Uploader", FontWeight = Microsoft.UI.Text.FontWeights.Bold });
                p.Inlines.Add(new Run { Text = $" • Saving to {SettingsService.DownloadPath}" });
                CurrentVideoUploaderAndSavingTo.Blocks.Add(p);

                //CurrentVideoUploaderAndSavingTo.Text = $"{info.uploader ?? "Unknown Uploader"} - Saving to {SettingsService.DownloadPath}";

                CurrentVideoStatus.Text = currentDownload.State.ToString();
                CurrentVideoProgress.Value = currentDownload.Progress;
            }
            else
            {
                CurrentMediaContainer.Visibility = Visibility.Collapsed;
                NoQueueContainer.Visibility = Visibility.Visible;
            }
        }

        private async void PauseOrResumeButton_Click(object sender, RoutedEventArgs e)
        {
            await App.DownloadService.PauseOrResume();
        }

        private async void ClearHistoryButton_Click()
        {
            await App.DatabaseService.ClearAllAsync();
            await UpdateHistoryQueue(true);
        }

        private void HandleButton(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                if (btn.Name == "ClearHistoryButton")
                {
                    ClearHistoryButton_Click();
                } else if (btn.Name == "CurrentMediaContainer")
                {
                    var currentDownload = App.DownloadService.CurrentMedia;
                    if (currentDownload != null)
                    {
                        var filePath = currentDownload.FilePath;
                        Debug.WriteLine(filePath);
                    }
                } else if (btn.Name == "NoQueueContainer")
                {
                    App.NavigationService.Navigate<MainPage>();
                }
            }
        }

        private void DownloadsPage_Unloaded(object sender, RoutedEventArgs e)
        {
            App.DownloadService.QueueUpdated -= QueueUpdated;
            App.DownloadService.CurrentMediaUpdated -= CurrentMediaUpdated;
        }
    }
}

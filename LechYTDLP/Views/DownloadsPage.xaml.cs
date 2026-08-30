using LechYTDLP.Classes;
using LechYTDLP.Components;
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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LechYTDLP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DownloadsPage : Page
    {
        public ObservableCollection<DownloadItem> CurrentQueueCollection { get; } = [];
        public ObservableCollection<DownloadItem> QueueCollection { get; } = [];
        public ObservableCollection<DownloadItem> HistoryCollection { get; } = [];

        public DownloadsPage()
        {
            this.InitializeComponent();

            Unloaded += DownloadsPage_Unloaded;

            if (App.DownloadService != null)
            {
                App.DownloadService.CurrentQueueUpdated += CurrentUpdated;
                App.DownloadService.InQueueUpdated += InUpdated;
                App.DownloadService.HistoryQueueUpdated += HistoryUpdated;
            }

            if (CurrentQueueListView != null)
            {
                CurrentQueueListView.ItemsSource = CurrentQueueCollection;
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
            UpdateCurrentQueue();
            UpdateInQueue();
            DispatcherQueue.TryEnqueue(async () => await UpdateHistoryQueue(true));
        }

        private void CurrentUpdated()
        {
            DispatcherQueue.TryEnqueue(UpdateCurrentQueue);
        }

        private void InUpdated()
        {
            DispatcherQueue.TryEnqueue(UpdateInQueue);
        }

        private void HistoryUpdated(bool getHistoryFromDatabase)
        {
            DispatcherQueue.TryEnqueue(() => _ = UpdateHistoryQueue(getHistoryFromDatabase));
        }

        public void UpdateCurrentQueue()
        {
            var currentDownloads = App.DownloadService.CurrentDownloads;

            if (currentDownloads.Count == 0)
            {
                CurrentQueueListView.Visibility = Visibility.Collapsed;
                NoQueueContainer.Visibility = Visibility.Visible;
            }
            else
            {
                CurrentQueueListView.Visibility = Visibility.Visible;
                NoQueueContainer.Visibility = Visibility.Collapsed;

                try
                {
                    // Listeyi kopyalıyoruz (Thread Safety)
                    var snapshot = App.DownloadService.CurrentDownloads.ToList();

                    CurrentQueueCollection.Clear();
                    foreach (var item in snapshot)
                    {
                        CurrentQueueCollection.Add(item);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Current Queue Error: {ex.Message}");
                }
            }
        }

        public void UpdateInQueue()
        {
            if (App.DownloadService?.Queue == null) return;
            Debug.WriteLine("QUEUE UPDATED");

            try
            {
                // Listeyi kopyalıyoruz (Thread Safety)
                var snapshot = App.DownloadService.Queue.ToList();

                if ((snapshot.Count - 1) > 0) QueueTitleText.Visibility = Visibility.Visible;
                else QueueTitleText.Visibility = Visibility.Collapsed;

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
                    HistoryProgressBar.Visibility = Visibility.Visible;

                    try
                    {
                        snapshot = await Task.Run(() => App.DatabaseService.GetAllAsync());
                    }
                    finally
                    {
                        HistoryProgressBar.Visibility = Visibility.Collapsed;
                    }
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

                var existingIds = HistoryCollection.Select(x => x.Id).ToHashSet();
                var newIds = snapshot.Select(x => x.Id).ToHashSet();

                // Add new ones
                foreach (var item in snapshot)
                {
                    if (!existingIds.Contains(item.Id))
                    {
                        HistoryCollection.Insert(0, item);
                    }
                }

                // Remove removed ones
                for (int i = HistoryCollection.Count - 1; i >= 0; i--)
                {
                    if (!newIds.Contains(HistoryCollection[i].Id))
                    {
                        HistoryCollection.RemoveAt(i);
                    }
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"History Queue Error: {ex.Message}");
            }
        }

        private async void OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem flyout)
            {
                if (flyout.DataContext is DownloadItem dataContext)
                {
                    if (flyout.Name == "DownloadAgain")
                    {
                        try
                        {
                            // Start a new download with the same info but force for dialog to show up
                            await App.DownloadController.SearchAsync(dataContext.Url, new SearchOptions { VideoInfo = dataContext.Info, ForceDialog = true });
                            // App.InfoBarService.Show(new InfoBarMessage("Copied to clipboard", "", InfoBarSeverity.Informational, 3000));
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex);

                            App.InfoBarService.Show(new InfoBarMessage
                            {
                                Title = App.LocalizationService.Get("DownloadFailed"),
                                Message = ex.Message,
                                Severity = InfoBarSeverity.Error,
                                DurationMs = 4000
                            });
                        }
                    }
                    else if (flyout.Name == "OpenInExplorer")
                    {
                        string filePath = dataContext.FilePath;

                        // Show the file in File Explorer
                        if (dataContext.Type == InfoType.Video)
                        {
                            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                            {
                                KnownErrors.ShowGenericError(KnownErrors.GenericError.NoFileOrDirectory);
                                return;
                            }

                            Process.Start(new ProcessStartInfo
                            {
                                FileName = "explorer.exe",
                                Arguments = $"/select,\"{filePath}\"",
                                UseShellExecute = true
                            });
                        }
                        // We show downloaded playlist folder instead
                        else if (dataContext.Type == InfoType.Playlist)
                        {
                            if (string.IsNullOrEmpty(filePath) || !Directory.Exists(filePath))
                            {
                                KnownErrors.ShowGenericError(KnownErrors.GenericError.NoFileOrDirectory);
                                return;
                            }

                            Process.Start(new ProcessStartInfo
                            {
                                FileName = "explorer.exe",
                                Arguments = $"\"{filePath}\"",
                                UseShellExecute = true
                            });
                        }
                    }
                    else if (flyout.Name.StartsWith("Copy"))
                    {
                        var package = new DataPackage();

                        if (flyout.Name == "CopyMedia")
                        {
                            if (!string.IsNullOrEmpty(dataContext.FilePath))
                            {
                                if (dataContext.Type == InfoType.Video)
                                {
                                    var file = Windows.Storage.StorageFile.GetFileFromPathAsync(dataContext.FilePath).GetAwaiter().GetResult();
                                    var fileList = new List<Windows.Storage.IStorageItem> { file };
                                    package.SetStorageItems(fileList);
                                }
                                else if (dataContext.Type == InfoType.Playlist)
                                {
                                    package.SetText(dataContext.FilePath);
                                }
                            }
                        }
                        else if (flyout.Name == "CopyLink") package.SetText(dataContext.Url);
                        else if (flyout.Name == "CopyFilepath") package.SetText(dataContext.FilePath);
                        else if (flyout.Name == "CopyTitle") package.SetText(dataContext.Info.Title ?? App.LocalizationService.Get("UnknownTitle"));

                        App.InfoBarService.Show(new InfoBarMessage
                        {
                            Title = App.LocalizationService.Get("CopiedToClipboard"),
                            Message = "",
                            Severity = InfoBarSeverity.Informational,
                            DurationMs = 3000
                        });
                        Clipboard.SetContent(package);
                    }
                    else if (flyout.Name == "RemoveFromHistory")
                    {
                        try
                        {
                            App.DownloadService.RemoveFromHistory(dataContext);
                            App.InfoBarService.Show(new InfoBarMessage
                            {
                                Title = App.LocalizationService.Get("DeletedFromHistory"),
                                Message = "",
                                Severity = InfoBarSeverity.Informational,
                                DurationMs = 3000
                            });
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex);
                            App.InfoBarService.Show(new InfoBarMessage
                            {
                                Title = App.LocalizationService.Get("DeleteFailed"),
                                Message = ex.Message,
                                Severity = InfoBarSeverity.Error,
                                DurationMs = 4000
                            });
                        }
                    }
                    else if (flyout.Name == "Delete")
                    {
                        try
                        {
                            App.DownloadService.RemoveFromHistory(dataContext);
                            if (File.Exists(dataContext.FilePath)) File.Delete(dataContext.FilePath);
                            App.InfoBarService.Show(new InfoBarMessage
                            {
                                Title = App.LocalizationService.Get("DeletedFile"),
                                Message = "",
                                Severity = InfoBarSeverity.Informational,
                                DurationMs = 3000
                            });
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex);
                            App.InfoBarService.Show(new InfoBarMessage
                            {
                                Title = App.LocalizationService.Get("DeleteFailed"),
                                Message = ex.Message,
                                Severity = InfoBarSeverity.Error,
                                DurationMs = 4000
                            });
                        }
                    }
                }
            }
            else if (sender is Button btn)
            {
                if (btn.Name == "ClearHistoryButton")
                {
                    await ClearHistoryButton_Click();
                }
                else if (btn.Name == "NoQueueContainer")
                {
                    App.NavigationService.Navigate<MainPage>();
                }
            }

        }
        private async Task ClearHistoryButton_Click()
        {
            await App.DatabaseService.ClearAllAsync();
            await UpdateHistoryQueue(true);
            App.InfoBarService.Show(new InfoBarMessage
            {
                Title = App.LocalizationService.Get("HistoryCleared"),
                Message = "",
                Severity = InfoBarSeverity.Informational,
                DurationMs = 3000
            });
        }

        private void DownloadsPage_Unloaded(object sender, RoutedEventArgs e)
        {
            App.DownloadService.CurrentQueueUpdated -= CurrentUpdated;
            App.DownloadService.InQueueUpdated -= InUpdated;
            App.DownloadService.HistoryQueueUpdated -= HistoryUpdated;
        }
    }
}

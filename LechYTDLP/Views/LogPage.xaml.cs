using CommunityToolkit.WinUI;
using LechYTDLP.Services;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LechYTDLP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LogPage : Page
    {
        public ObservableCollection<LogItem> UiLogs { get; } = [];

        public LogPage()
        {
            InitializeComponent();

            LogListView.ItemsSource = UiLogs;

            // Sayfa açılırken mevcut logları yükle
            foreach (var log in LogService.GetAll())
                UiLogs.Add(log);

            LogService.LogAdded += OnLogAdded;
            LogService.LogUpdated += OnLogUpdated;
        }

        private async void OnLogAdded(LogItem item)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                UiLogs.Add(item);
                LogListView.ScrollIntoView(item);
            });
        }

        private async void OnLogUpdated(LogItem item)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                LogListView.ScrollIntoView(item);
            });
        }

        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem menuItem && menuItem.DataContext is LogItem logItem)
            {
                if (menuItem.Name == "Copy")
                {
                    var package = new DataPackage();
                    package.SetText(logItem.Message);
                    Clipboard.SetContent(package);
                }
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            LogService.ResetLog();
        }


        private void LogPage_Unloaded(object sender, RoutedEventArgs e)
        {
            LogService.LogAdded -= OnLogAdded;
            LogService.LogUpdated -= OnLogUpdated;
        }
    }
}

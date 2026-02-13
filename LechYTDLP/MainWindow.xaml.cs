using LechYTDLP.Views;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics;
using WinRT.Interop;
using LechYTDLP.Services;
using System.Collections;
using System;
using System.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LechYTDLP
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public int InfoBadgeOpacity = 1;

        public MainWindow()
        {
            this.InitializeComponent();

            // Window sizing
            var hWnd = WindowNative.GetWindowHandle(this);
            var windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
            AppWindow appWindow = AppWindow.GetFromWindowId(windowId);

            appWindow.Resize(new Windows.Graphics.SizeInt32(900, 600));

            // Hide the default system title bar.
            ExtendsContentIntoTitleBar = true;
            // Replace system title bar with the WinUI TitleBar.
            AppTitleBar.Subtitle = $"v{App.AppVersion}";
            SetTitleBar(AppTitleBar);

            App.InfoBarService.Register(GlobalInfoBar);

            App.NavigationService.Initialize(AppFrame, NavView);
            // App.NavigationService.Navigate<MainPage>();

            LogService.Add("The logs will be appear here, mate <3");
            LogService.BadgeChanged += UpdateLogBadge;
            DownloadsService.OnBadgeChanged += UpdateLogBadge;
        }

        public void NavigateToPage(string pageTag)
        {
            var item = NavView.MenuItems
                .OfType<NavigationViewItem>()
                .FirstOrDefault(x => (string)x.Tag == pageTag);
            if (item != null)
            {
                NavView.SelectedItem = item;
            }
        }

        private void UpdateLogBadge(int count, string whichBadge)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                if (whichBadge == "Log")
                {
                    LogBadge.Value = count;
                    LogBadge.Visibility =
                        count > 0 ? Visibility.Visible : Visibility.Collapsed;
                }
                else if (whichBadge == "Downloads")
                {
                    DownloadsBadge.Value = count;
                    DownloadsBadge.Visibility =
                        count > 0 ? Visibility.Visible : Visibility.Collapsed;
                }
            });
        }


        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItemContainer != null)
            {
                switch (args.SelectedItemContainer.Tag)
                {
                    case "MainPage":
                        App.NavigationService.Navigate<MainPage>();
                        break;
                    case "DownloadsPage":
                        App.NavigationService.Navigate<DownloadsPage>();
                        break;
                    case "OptionsPage":
                        App.NavigationService.Navigate<OptionsPage>();
                        break;
                    case "LogPage":
                        App.NavigationService.Navigate<LogPage>();
                        break;
                    case "Settings":
                        App.NavigationService.Navigate<SettingsPage>();
                        break;
                }
            }
        }
    }
}
using LechYTDLP.Services;
using LechYTDLP.Views;
using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Resources;
using WinRT;
using WinRT.Interop;
using static LechYTDLP.Views.SettingsPage;

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

            // Apply theme to app and listen to events
            AppThemeChanged(SettingsService.AppTheme);
            App.SettingsService.AppThemeChanged += AppThemeChanged;

            // Hide the default system title bar.
            ExtendsContentIntoTitleBar = true;
            // Replace system title bar with the WinUI TitleBar.
            AppTitleBar.Subtitle = $"v{App.GetAppVersion()}";
            SetTitleBar(AppTitleBar);

            App.InfoBarService.Register(GlobalInfoBar);

            App.NavigationService.Initialize(AppFrame, NavView);
            // App.NavigationService.Navigate<MainPage>();

            LogService.BadgeChanged += UpdateLogBadge;
            DownloadsService.OnBadgeChanged += UpdateLogBadge;

            // Apply backdrop and listen to events
            AppBackdropChanged(SettingsService.AppBackdrop, true);
            App.SettingsService.AppBackdropChanged += (backdrop) => AppBackdropChanged(backdrop);
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

        public void AppThemeChanged(ThemeItem newTheme)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                var root = (FrameworkElement)this.Content;

                ElementTheme newThemeValue = newTheme.Value switch
                {
                    "light" => ElementTheme.Light,
                    "dark" => ElementTheme.Dark,
                    "system" => ElementTheme.Default,
                    _ => ElementTheme.Default
                };

                bool isChanged = root.RequestedTheme != newThemeValue;

                if (isChanged)
                {
                    root.RequestedTheme = newThemeValue;
                    // string format = LocalizationService.Get("DownloadCount.Text");

                    App.InfoBarService.Show(new InfoBarMessage
                    {
                        Title = App.LocalizationService.GetString("ThemeChanged", newTheme.DisplayName),
                        Message = "",
                        Severity = InfoBarSeverity.Success,
                        DurationMs = 3000,
                        IsCancelable = true
                    });
                }
            });
        }
        public void AppBackdropChanged(ThemeItem newBackdrop, bool _isInitializingTheme = false)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                string currentBackdrop = SystemBackdrop switch
                {
                    MicaBackdrop mica => mica.Kind switch
                    {
                        MicaKind.BaseAlt => "micaalt",
                        _ => "mica"
                    },
                    DesktopAcrylicBackdrop => "acrylic",
                    null => "none",
                    _ => "unknown"
                };
                bool isChanged = currentBackdrop != newBackdrop.Value;

                switch (newBackdrop.Value)
                {
                    case "mica":
                        TrySetMicaBackdrop(false);
                        break;
                    case "micaalt":
                        TrySetMicaBackdrop(true);
                        break;
                    case "acrylic":
                        TrySetDesktopAcrylicBackdrop();
                        break;
                    default:
                        SystemBackdrop = null;
                        break;
                }
                if (isChanged && !_isInitializingTheme)
                    App.InfoBarService.Show(new InfoBarMessage
                    {
                        Title = isChanged
                       ? App.LocalizationService.GetString("BackdropChanged", newBackdrop.DisplayName)
                       : App.LocalizationService.GetString("BackdropChangedFailed", newBackdrop.DisplayName),
                        Message = "",
                        Severity = isChanged ? InfoBarSeverity.Success : InfoBarSeverity.Error,
                        DurationMs = 3000,
                        IsCancelable = true
                    });
            });
        }

        public bool TrySetMicaBackdrop(bool useMicaAlt)
        {
            bool IsSupported = MicaController.IsSupported();
            if (IsSupported)
            {
                MicaBackdrop micaBackdrop = new()
                {
                    Kind = useMicaAlt ? MicaKind.BaseAlt : MicaKind.Base
                };
                SystemBackdrop = micaBackdrop;

                return true; // Succeeded.
            }

            return false; // Mica is not supported on this system.
        }

        public bool TrySetDesktopAcrylicBackdrop()
        {
            if (DesktopAcrylicController.IsSupported())
            {
                DesktopAcrylicBackdrop DesktopAcrylicBackdrop = new();
                SystemBackdrop = DesktopAcrylicBackdrop;

                return true; // Succeeded.
            }

            return false; // DesktopAcrylic is not supported on this system.
        }
    }
}
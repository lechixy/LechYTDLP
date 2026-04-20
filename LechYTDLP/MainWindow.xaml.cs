using CommunityToolkit.WinUI.Animations;
using LechYTDLP.Classes;
using LechYTDLP.Components;
using LechYTDLP.Controllers;
using LechYTDLP.Services;
using LechYTDLP.Views;
using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.System;
using Windows.UI.Notifications;
using WinRT;
using WinRT.Interop;
using static LechYTDLP.Views.SettingsPage;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        public bool IsInitTheme = true;

        public MainWindow()
        {
            InitializeComponent();
            App.DialogService = new(this);

            App.NavigationService.Initialize(AppFrame, NavView);

            // Window sizing
            var hWnd = WindowNative.GetWindowHandle(this);
            var windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
            AppWindow appWindow = AppWindow.GetFromWindowId(windowId);
            appWindow.Resize(new Windows.Graphics.SizeInt32(800, 600));

            // Apply theme to app and listen to events
            AppThemeChanged(SettingsService.AppTheme);
            App.SettingsService.AppThemeChanged += AppThemeChanged;

            // Hide the default system title bar.
            ExtendsContentIntoTitleBar = true;
            // Replace system title bar with the WinUI TitleBar.
            AppTitleBar.Subtitle = $"v{App.GetAppVersion()}";
#if DEBUG
            AppTitleBar.Subtitle += $" ({App.LocalizationService.Get("DevelopmentVersion")} • {App.LocalizationService.Get("Debug")})";
#endif
            SetTitleBar(AppTitleBar);
            ExtendsContentIntoTitleBar = true;

            App.InfoBarService.Register(GlobalInfoBar);
            // App.NavigationService.Navigate<MainPage>();

            LogService.BadgeChanged += UpdateLogBadge;
            DownloadsService.OnBadgeChanged += UpdateLogBadge;

            // Apply backdrop and listen to events
            AppBackdropChanged(SettingsService.AppBackdrop, true);
            App.SettingsService.AppBackdropChanged += (backdrop) => AppBackdropChanged(backdrop);

            _ = CheckForUpdatesOnStartupAsync();

            RootGrid.AddHandler(UIElement.KeyDownEvent, new KeyEventHandler(Global_KeyDown), true);
        }

        private void Global_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            var ctrl = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Control)
                .HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);

            var shift = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift)
                .HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);

            if (e.Key == VirtualKey.F1)
            {
                App.NavigationService.Navigate<SettingsPage>();
                e.Handled = true;
            }

            // CTRL + V → özel yapıştır (mesela link parse et)
            if (ctrl && e.Key == VirtualKey.V)
            {
                HandlePaste();
                e.Handled = true;
            }
        }

        private async void HandlePaste()
        {
            var data = Clipboard.GetContent();

            if (data.Contains(StandardDataFormats.Text))
            {
                var text = await data.GetTextAsync();
                Debug.WriteLine("Pasted text: " + text);
                await App.DownloadController.SearchAsync(text);
            }
        }

        private async Task CheckForUpdatesOnStartupAsync()
        {
            try
            {
                var result = await UpdateChecker.CheckForUpdatesAsync(SettingsService._LastKnownYTdlpToolVersion);

                if (result.Success && result.IsUpdateAvailable)
                {
                    var updateDialog = new UpdateDialog(result);
                    var dialog = await App.DialogService.ShowAsync(new DialogOptions
                    {
                        Title = App.LocalizationService.Get("YTdlpUpdateAvailable"),
                        Content = updateDialog,
                        PrimaryButtonText = App.LocalizationService.Get("Update"),
                        PrimaryButtonStyle = Application.Current.Resources["AccentButtonStyle"] as Style,
                        CloseButtonText = App.LocalizationService.Get("MaybeLater")
                    });

                    // If user chooses to update, start the update process.
                    if (dialog != DialogResult.Primary) return;

                    // Show info bar that update is starting.
                    App.InfoBarService.Show(new InfoBarMessage
                    {
                        Title = App.LocalizationService.Get("YTdlpCheckingForUpdates"),
                        Message = "",
                        Severity = InfoBarSeverity.Informational,
                        IsCancelable = false
                    });

                    var ytdlp = new YTDLP();
                    var update = await ytdlp.CheckAndDownloadUpdate();

                    if (update.Status == UpdateStatus.Updated)
                    {
                        App.InfoBarService.Show(new InfoBarMessage
                        {
                            Title = App.LocalizationService.Get("YTdlpUpdatedSuccessfully"),
                            Message = App.LocalizationService.Get("YTdlpUpdatedSuccessfullyMsg", result.NewestVersion),
                            Severity = InfoBarSeverity.Success,
                            IsCancelable = true
                        });
                    }
                    else if (update.Status == UpdateStatus.Failed)
                    {
                        App.InfoBarService.Show(new InfoBarMessage
                        {
                            Title = App.LocalizationService.Get("YTdlpUpdateFailed"),
                            Message = "",
                            Severity = InfoBarSeverity.Error,
                            IsCancelable = true
                        });
                    }
                    else // UpToDate
                    {
                        App.InfoBarService.Show(new InfoBarMessage
                        {
                            Title = App.LocalizationService.Get("YTdlpIsUpToDate"),
                            Message = "",
                            Severity = InfoBarSeverity.Success,
                            IsCancelable = true
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error checking for updates: " + ex.Message);
            }
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

        public void AppThemeChanged(Setting newTheme)
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

                    // Show info bar only when theme is changed and not during initialization to avoid showing it on app launch.
                    if (IsInitTheme)
                    {
                        IsInitTheme = false;
                        return;
                    }

                    App.InfoBarService.Show(new InfoBarMessage
                    {
                        Title = App.LocalizationService.Get("ThemeChanged", newTheme.DisplayName),
                        Message = "",
                        Severity = InfoBarSeverity.Success,
                        DurationMs = 3000,
                        IsCancelable = true
                    });
                }
            });
        }
        public void AppBackdropChanged(Setting newBackdrop, bool _isInitializingTheme = false)
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
                       ? App.LocalizationService.Get("BackdropChanged", newBackdrop.DisplayName)
                       : App.LocalizationService.Get("BackdropChangedFailed", newBackdrop.DisplayName),
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
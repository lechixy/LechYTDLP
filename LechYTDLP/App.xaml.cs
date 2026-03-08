using LechYTDLP.Classes;
using LechYTDLP.Controllers;
using LechYTDLP.Core;
using LechYTDLP.Services;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Microsoft.Windows.ApplicationModel.Resources;
using Microsoft.Windows.AppLifecycle;
using Microsoft.Windows.Globalization;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;
using static System.Runtime.InteropServices.JavaScript.JSType;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LechYTDLP
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        public static Window? Window { get; private set; }

        /// <summary>
        /// Application version string in the format of "Major.Minor.Build", e.g. "1.0.0"
        /// </summary>
        public static string GetAppVersion()
        {
            var v = Package.Current.Id.Version;
            return $"{v.Major}.{v.Minor}.{v.Build}";
        }
        // Api Server for browser extension
        public static LocalApiServer ApiServer { get; private set; } = null!;

        // Services
        public static SettingsService SettingsService => ServiceContainer.Get<SettingsService>();
        public static DownloadsService DownloadService => ServiceContainer.Get<DownloadsService>();
        public static NavigationService NavigationService => ServiceContainer.Get<NavigationService>();
        public static InfoBarService InfoBarService => ServiceContainer.Get<InfoBarService>();
        public static DatabaseService DatabaseService => ServiceContainer.Get<DatabaseService>();
        public static FormatDialogService FormatDialogService { get; private set; } = null!;
        public static LocalizationService LocalizationService => ServiceContainer.Get<LocalizationService>();

        // Controllers
        public static DownloadController DownloadController { get; } = new();


        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            ServiceContainer.Configure();
        }

        private async void BrowserAddedMediaHandler(RequestData data)
        {
            LogService.Add(LocalizationService.GetString("ExtensionAddedMediaLog", data.ExtensionBrowser, data.Url), LogTag.ApiServer);
            InfoBarService.Show(new InfoBarMessage
            {
                Title = LocalizationService.GetString("ExtensionAddedMediaInfoBarMsg", data.ExtensionBrowser),
                Message = data.Url,
                Severity = InfoBarSeverity.Success,
            });
            await DownloadController.SearchAsync(data.Url);
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            Window = new MainWindow();

            var activatedArgs = Microsoft.Windows.AppLifecycle.AppInstance.GetCurrent().GetActivatedEventArgs();
            var activationKind = activatedArgs.Kind;
            if (activationKind == ExtendedActivationKind.Protocol)
            {
                var protocolArgs = activatedArgs.Data as IProtocolActivatedEventArgs;
                Uri? uri = protocolArgs?.Uri;
                if (uri == null) return;
                LogService.Add($"{LocalizationService.Get("ProtocolActivation")}: {uri}", LogTag.Lechixy);
                if (uri.Host == "open")
                {
                    // The URL to open is in the query parameter "url", e.g. lechytdlp://open?url=https%3A%2F%2Fwww.youtube.com%2Fwatch%3Fv%3DdQw4w9WgXcQ&browser=Chrome&version=1.0.0
                    var url = System.Web.HttpUtility.ParseQueryString(uri.Query).Get("url");
                    // If the url parameter is not empty, add it to the download list
                    if (!string.IsNullOrEmpty(url) )
                    {
                        var extensionBrowser = System.Web.HttpUtility.ParseQueryString(uri.Query).Get("browser") ?? LocalizationService.Get("UnknownBrowser");
                        var extensionVersion = System.Web.HttpUtility.ParseQueryString(uri.Query).Get("version") ?? LocalizationService.Get("UnknownVersion");
                        BrowserAddedMediaHandler(new RequestData
                        {
                            Url = url,
                            ExtensionBrowser = extensionBrowser,
                            ExtensionVersion = extensionVersion,
                         });
                    }
                }
            }

            Window.Activate();

            FormatDialogService = new FormatDialogService(Window);
            _ = DatabaseService.InitializeAsync();

            LocalizationService.SetDefaultLanguageBasedOnSystem();

            // Ensure tools are available
            ToolPathService.Ensure(ToolPathService.Tool.YtDlp);
            ToolPathService.Ensure(ToolPathService.Tool.FFmpeg);

            ApiServer = new LocalApiServer();
            ApiServer.Start();

            LogService.Add(LocalizationService.Get("FirstLog"), LogTag.Lechixy, false);

            // Listen for download requests from the browser extension
            ApiServer.DownloadRequested += (data) => BrowserAddedMediaHandler(data);

            // If it's the first run, show a welcome message and ask if the user wants to import settings from the old settings file.
            if (SettingsService._IsFirstRun)
            {
                // YOU reference because of Joe Goldberg ofc.
                LogService.Add(LocalizationService.Get("FirstRun"), LogTag.Lechixy, false);

                // Check if old settings file exists for importing settings
                var oldSettingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "LechYTDLP_Settings.json");
                // If the old settings file exists, show an info bar to ask the user if they want to import settings
                if (File.Exists(oldSettingsPath))
                {
                    LogService.Add(LocalizationService.Get("ImportOldSettingsNotice"), LogTag.Lechixy, false);
                    InfoBarService.Show(new InfoBarMessage
                    {
                        Title = LocalizationService.Get("ImportOldSettings"),
                        Message = LocalizationService.Get("ImportOldSettingsMsg"),
                        Severity = InfoBarSeverity.Informational,
                        ActionButton = new InfoBarButton
                        {
                            Content = LocalizationService.Get("Import"),
                            ClickAction = () =>
                            {
                                SettingsService.ImportSettingsFromFile(oldSettingsPath);
                                LogService.Add("Settings imported successfully", LogTag.Lechixy, false);
                            }
                        },
                        IsCancelable = true,
                        DurationMs = 0
                    });
                }

                SettingsService._IsFirstRun = false;
            }
        }

        public static async Task<string?> PickFileAsync(string[] FilterExtensions, Window window)
        {
            var picker = new FileOpenPicker();

            for (int i = 0; i < FilterExtensions.Length; i++)
            {
                var filter = FilterExtensions[i].Trim().ToLower();
                if (!filter.StartsWith('.'))
                {
                    filter = "." + filter;
                }
                picker.FileTypeFilter.Add(filter);
            }

            var hwnd = WindowNative.GetWindowHandle(window);
            InitializeWithWindow.Initialize(picker, hwnd);

            StorageFile file = await picker.PickSingleFileAsync();

            if (file != null)
            {
                return file.Path;
            }

            return null;
        }
    }

    public partial class LogTagToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value switch
            {
                LogTag.Lechixy => new SolidColorBrush(Colors.Aqua),
                LogTag.YTDLP => new SolidColorBrush(Colors.MediumVioletRed),
                LogTag.Warning => new SolidColorBrush(Colors.Gold),
                LogTag.Error => new SolidColorBrush(Colors.IndianRed),
                LogTag.ApiServer => new SolidColorBrush(Colors.MediumPurple),
                LogTag.App => new SolidColorBrush(Colors.BlueViolet),
                _ => new SolidColorBrush(Colors.LightGray)
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }

}

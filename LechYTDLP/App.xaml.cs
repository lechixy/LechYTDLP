using LechYTDLP.Classes;
using LechYTDLP.Controllers;
using LechYTDLP.Core;
using LechYTDLP.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Microsoft.Windows.ApplicationModel.Resources;
using Microsoft.Windows.AppLifecycle;
using Microsoft.Windows.Globalization;
using Sentry;
using Sentry.Profiling;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;
using static LechYTDLP.Views.SettingsPage;
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
        public static DispatcherQueue UIThreadDispatcherQueue { get; private set; }

        /// <summary>
        /// Application version string in the format of "Major.Minor.Build", e.g. "1.0.0"
        /// </summary>
        /// https://stackoverflow.com/questions/28635208/retrieve-the-current-app-version-from-package
        public static string GetAppVersion()
        {
            try
            {
                // 1. Try to get the MSIX package version (Packaged environment)
                Package package = Package.Current;
                PackageId packageId = package.Id;
                PackageVersion version = packageId.Version;

                return $"{version.Major}.{version.Minor}.{version.Build}";
            }
            catch (InvalidOperationException)
            {
                // 2. Fallback to Assembly version (Unpackaged environment / Debugging)
                Version? assemblyVersion = Assembly.GetEntryAssembly()?.GetName().Version;

                if (assemblyVersion != null)
                {
                    return $"{assemblyVersion.Major}.{assemblyVersion.Minor}.{assemblyVersion.Build}";
                }

                return "1.0.0";
            }
        }

        public Action? WindowInitialized;

        // Api Server for browser extension
        public static LocalApiServer ApiServer { get; private set; } = null!;

        // Services
        public static SettingsService SettingsService => ServiceContainer.Get<SettingsService>();
        public static DownloadsService DownloadService => ServiceContainer.Get<DownloadsService>();
        public static NavigationService NavigationService => ServiceContainer.Get<NavigationService>();
        public static InfoBarService InfoBarService => ServiceContainer.Get<InfoBarService>();
        public static DatabaseService DatabaseService => ServiceContainer.Get<DatabaseService>();
        public static DialogService DialogService { get; set; } = null!;
        public static LogService LogService => ServiceContainer.Get<LogService>();
        public static LocalizationService LocalizationService => ServiceContainer.Get<LocalizationService>();

        // Controllers
        public static DownloadController DownloadController { get; } = new();

        // Configuration
        private static readonly IConfiguration Configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.Local.json", optional: false, reloadOnChange: false)
        .Build();

        // Sentry
        private static readonly bool IsSentryEnabled = Configuration.GetValue<bool>("Sentry:Enabled");
        private static readonly string DefaultSentryDsn = Configuration.GetValue<string>("Sentry:Dsn") ?? "";

        // Helpers
        public static JsonElement SampleJson { get; private set; }

        [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
        public static JsonSerializerOptions JsonSerializerOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver()
        };

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            ServiceContainer.Configure();
        }


        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            Window = new MainWindow();

            // Init Sentry if it is enabled and app in release mode
#if !DEBUG
            if (IsSentryEnabled && !string.IsNullOrEmpty(DefaultSentryDsn))
            {
                Debug.WriteLine("Initializing Sentry...");
                try
                {
                    SentrySdk.Init(options =>
                    {
                        // A Sentry Data Source Name (DSN) is required.
                        // See https://docs.sentry.io/product/sentry-basics/dsn-explainer/
                        // You can set it in the SENTRY_DSN environment variable, or you can set it in code here.

                        options.Dsn = DefaultSentryDsn;

                        // When debug is enabled, the Sentry client will emit detailed debugging information to the console.
                        // This might be helpful, or might interfere with the normal operation of your application.
                        // We enable it here for demonstration purposes when first trying Sentry.
                        // You shouldn't do this in your applications unless you're troubleshooting issues with Sentry.
                        options.Debug = true;

                        // This option is recommended. It enables Sentry's "Release Health" feature.
                        options.AutoSessionTracking = true;

                        // Set TracesSampleRate to 1.0 to capture 100%
                        // of transactions for tracing.
                        // We recommend adjusting this value in production.
                        options.TracesSampleRate = 1.0;

                        // Sample rate for profiling, applied on top of othe TracesSampleRate,
                        // e.g. 0.2 means we want to profile 20 % of the captured transactions.
                        // We recommend adjusting this value in production.
                        options.ProfilesSampleRate = 1.0;
                        // Requires NuGet package: Sentry.Profiling
                        // Note: By default, the profiler is initialized asynchronously. This can
                        // be tuned by passing a desired initialization timeout to the constructor.
                        options.AddIntegration(new ProfilingIntegration(
                            // During startup, wait up to 500ms to profile the app startup code.
                            // This could make launching the app a bit slower so comment it out if you
                            // prefer profiling to start asynchronously
                            TimeSpan.FromMilliseconds(500)
                        ));
                        // Enable logs to be sent to Sentry
                        options.EnableLogs = true;
                    });

                    AppDomain.CurrentDomain.UnhandledException += (_, e) =>
                    {
                        if (e.ExceptionObject is Exception ex)
                        {
                            SentrySdk.CaptureException(ex);
                            SentrySdk.Flush(TimeSpan.FromSeconds(3));
                        }
                    };
                    TaskScheduler.UnobservedTaskException += (_, e) =>
                    {
                        SentrySdk.CaptureException(e.Exception);
                        SentrySdk.Flush(TimeSpan.FromSeconds(3));
                    };

                    this.UnhandledException += (_, e) =>
                    {
                        SentrySdk.CaptureException(e.Exception);
                        SentrySdk.Flush(TimeSpan.FromSeconds(3));
                    };
                } catch (Exception ex)
                {
                    Debug.WriteLine($"Error initializing Sentry: {ex.Message}");
                } finally
                {
                    Debug.WriteLine("Sentry initialized.");
                }
            }
#endif

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
                    if (!string.IsNullOrEmpty(url))
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
            UIThreadDispatcherQueue = DispatcherQueue.GetForCurrentThread();

            WindowInitialized?.Invoke();

            _ = DatabaseService.InitializeAsync();

            LocalizationService.SetDefaultLanguageBasedOnSystem();

            // Ensure tools are available
            // We don't need to ensure yt-dlp here because will check in MainWindow CheckForUpdatesOnStartupAsync();
            // ToolPathService.Ensure(ToolPathService.Tool.YtDlp);
            ToolPathService.Ensure(ToolPathService.Tool.FFmpeg);

            // Load filename_dumpdata.json for filename template preview in options page
            var path = Path.Combine(AppContext.BaseDirectory, "Assets", "filename_dumpdata.json");
            var jsonString = File.ReadAllText(path);
            SampleJson = JsonDocument.Parse(jsonString).RootElement;

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
                var oldSettingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "LechYTDLP_Settings.xml");
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

            if (!SettingsService._IsFirstRun && GetAppVersion() != SettingsService._LastUsedVersion)
            {
                LogService.Add(LocalizationService.Get("AppUpdated"), LogTag.Lechixy, false);
                InfoBarService.Show(new InfoBarMessage
                {
                    Title = LocalizationService.Get("AppUpdated"),
                    Message = "",
                    Severity = InfoBarSeverity.Informational
                });
                SettingsService._LastUsedVersion = GetAppVersion();
            }
        }

        private async void BrowserAddedMediaHandler(RequestData data)
        {
            LogService.Add(LocalizationService.Get("ExtensionAddedMediaLog", data.ExtensionBrowser, data.Url), LogTag.ApiServer);
            InfoBarService.Show(new InfoBarMessage
            {
                Title = LocalizationService.Get("ExtensionAddedMediaInfoBarMsg", data.ExtensionBrowser),
                Message = data.Url,
                Severity = InfoBarSeverity.Success,
            });
            await DownloadController.SearchAsync(data.Url, new SearchOptions { ForceDialog = true });
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

        public static async Task<string?> PickFolderAsync(Window window)
        {
            var picker = new FolderPicker();
            var hwnd = WindowNative.GetWindowHandle(window);
            InitializeWithWindow.Initialize(picker, hwnd);
            StorageFolder folder = await picker.PickSingleFolderAsync();
            if (folder != null)
            {
                return folder.Path;
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

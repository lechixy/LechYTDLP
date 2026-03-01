using LechYTDLP.Classes;
using LechYTDLP.Controllers;
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
        public static SettingsService SettingsService { get; } = new SettingsService();
        public static DownloadsService DownloadService { get; } = new DownloadsService();
        public static INavigationService NavigationService { get; } = new NavigationService();
        public static InfoBarService InfoBarService { get; } = new();
        public static DatabaseService DatabaseService { get; } = new();
        public static FormatDialogService FormatDialogService { get; private set; } = null!;
        public static LocalizationService LocalizationService { get; private set; } = null!;

        // Controllers
        public static DownloadController DownloadController { get; } = new();


        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            LocalizationService = new LocalizationService();
            if (SettingsService.AppLanguage.Code != "system")
            {
                ApplicationLanguages.PrimaryLanguageOverride = SettingsService.AppLanguage.Code;
                Debug.WriteLine($"App language set to {SettingsService.AppLanguage.Code}");
                LocalizationService.Reload();
            }

            InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            _ = DatabaseService.InitializeAsync();

            // Ensure tools are available
            ToolPathService.Ensure(ToolPathService.Tool.YtDlp);
            ToolPathService.Ensure(ToolPathService.Tool.FFmpeg);

            ApiServer = new LocalApiServer();
            ApiServer.Start();

            Window = new MainWindow();
            Window.Activate();

            FormatDialogService = new FormatDialogService(Window);

            LogService.Add(LocalizationService.Get("FirstLog"), LogTag.Lechixy);

            ApiServer.DownloadRequested += async data =>
            {
                Debug.WriteLine(LocalizationService.GetString("ExtensionAddedMediaLog", data.ExtensionBrowser, data.Url));
                LogService.Add(LocalizationService.GetString("ExtensionAddedMediaLog", data.ExtensionBrowser, data.Url), LogTag.ApiServer);

                InfoBarService.Show(new InfoBarMessage
                {
                    Title = LocalizationService.GetString("ExtensionAddedMediaInfoBarMsg", data.ExtensionBrowser),
                    Message = data.Url,
                    Severity = InfoBarSeverity.Success,
                });

                await DownloadController.SearchAsync(data.Url);
            };
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
                LogTag.LechYTDLP => new SolidColorBrush(Colors.DeepSkyBlue),
                LogTag.Warning => new SolidColorBrush(Colors.Gold),
                LogTag.Error => new SolidColorBrush(Colors.IndianRed),
                LogTag.ApiServer => new SolidColorBrush(Colors.MediumPurple),
                _ => new SolidColorBrush(Colors.LightGray)
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }

}

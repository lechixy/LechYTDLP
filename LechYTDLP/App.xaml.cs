using LechYTDLP.Classes;
using LechYTDLP.Controllers;
using LechYTDLP.Services;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Microsoft.Windows.AppLifecycle;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

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

        public static int Major => 1;
        public static int Year => DateTime.Now.Year;
        public static int Minor => 1;

        /// <summary>
        /// Application version string in the format "Major.Minor.Year".
        /// </summary>
        public static string AppVersion => $"{Major}.{Minor}.{Year}";
        public static string GithubLink => "https://github.com/lechixy/LechYTDLP";

        // Api Server for browser extension
        public static LocalApiServer ApiServer { get; private set; } = null!;

        // Services
        public static DownloadsService DownloadService { get; } = new DownloadsService();
        public static INavigationService NavigationService { get; } = new NavigationService();
        public static InfoBarService InfoBarService { get; } = new();
        public static DatabaseService DatabaseService { get; } = new();
        public static FormatDialogService FormatDialogService { get; private set; } = null!;

        // Controllers
        public static DownloadController DownloadController { get; } = new();


        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            var instance = Microsoft.Windows.AppLifecycle.AppInstance.GetCurrent();
            instance.Activated += OnAppActivated;

            _ = DatabaseService.InitializeAsync();

            ApiServer = new LocalApiServer();
            ApiServer.Start();

            Window = new MainWindow();
            Window.Activate();

            FormatDialogService = new FormatDialogService(Window);

            ApiServer.DownloadRequested += async data =>
            {
                Debug.WriteLine($"{data.ExtensionBrowser} extension added media: {data.Url}");
                LogService.Add($"{data.ExtensionBrowser} extension added media: {data.Url}", LogTag.ApiServer);

                await DownloadController.SearchAsync(data.Url, data);
            };
        }

        private void OnAppActivated(object? sender, AppActivationArguments args)
        {
            if (args.Kind == ExtendedActivationKind.Protocol)
            {
                var protocolArgs = args.Data as ProtocolActivatedEventArgs;
                var uri = protocolArgs?.Uri;

                if (uri?.Host == "open")
                {
                    Debug.WriteLine($"Received protocol activation with URI: {uri}");
                }
                else if (uri?.Host == "download" && uri.Query.Contains("url="))
                {
                    Debug.WriteLine($"Received download protocol activation with URI: {uri}");
                    //var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);
                    //var downloadUrl = queryParams["url"];
                    //if (!string.IsNullOrEmpty(downloadUrl))
                    //{
                    //    Debug.WriteLine($"Received download request for URL: {downloadUrl}");
                    //}
                }
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

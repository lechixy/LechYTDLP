using LechYTDLP.Classes;
using LechYTDLP.Services;
using LechYTDLP.Views;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
        public static INavigationService NavigationService { get; }
        = new NavigationService();
        public static InfoBarService InfoBarService { get; } = new();
        public static DatabaseService DatabaseService { get; } = new();


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
            _ = DatabaseService.InitializeAsync();

            ApiServer = new LocalApiServer();
            ApiServer.Start();

            Window = new MainWindow();
            Window.Activate();
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

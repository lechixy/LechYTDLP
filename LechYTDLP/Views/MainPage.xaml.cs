using LechYTDLP.Classes;
using LechYTDLP.Components;
using LechYTDLP.Services;
using LechYTDLP.Util;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.System;
using Windows.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LechYTDLP.Views;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainPage : Page
{

    public string textboxText = "https://www.instagram.com/p/DTtECW1iPct/";

    public ObservableCollection<string> QueueCollection { get; } = [];

    private bool _isSearching = false;

    public MainPage()
    {
        InitializeComponent();

        // Text initialization
        LinkTextBox.Text = textboxText;
        if (textboxText.Length == 0)
            DownloadButton.IsEnabled = false;
        else
            DownloadButton.IsEnabled = true;

        UpdateTextDependingOnLink();

        DownloadsService DownloadsService = App.DownloadService;
        Debug.WriteLine("Rendered one time: MainPage");
        App.ApiServer.DownloadRequested += DownloadRequested;

        //DownloadsService.DownloadProgressChanged += OnProgressChanged;
        //DownloadsService.DownloadBusyChanged += OnBusyChanged;
    }

    private void DownloadRequested(RequestData data)
    {
        Debug.WriteLine($"{data.ExtensionBrowser} extension added media: {data.Url}");
        DispatcherQueue.TryEnqueue(() =>
        {
            LogService.Add($"{data.ExtensionBrowser} extension added media: {data.Url}", LogTag.ApiServer);
            // Show info bar about browser extension added
            App.InfoBarService.Show(new InfoBarMessage($"{data.ExtensionBrowser} extension added media", "", InfoBarSeverity.Success, 5000, false));

            textboxText = data.Url;
            LinkTextBox.Text = data.Url;
            Download();
        });
    }

    private void Download()
    {
        if (textboxText.Length == 0) return;

        // Clear information stuff
        InfoBar.IsOpen = false;
        DownloadButton.IsEnabled = false;
        DownloadButton.Content = new ProgressRing
        {
            IsActive = true,
            Width = 20,
            Height = 20,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        ShowFormatsDialog();
    }

    private void DownloadButton_Click(object sender, RoutedEventArgs e)
    {
        Download();
    }

    enum DownloadButtonStatus
    {
        Download,
        Reset
    }

    private async void SetDownloadButtonStatus(DownloadButtonStatus status)
    {
        if (status == DownloadButtonStatus.Download)
        {
            _isSearching = true;
        }
        else if (status == DownloadButtonStatus.Reset)
        {
            DownloadButton.Content = new SymbolIcon(Symbol.Download);
            _isSearching = false;
            DownloadButton.IsEnabled = true;
        }
    }

    private async void ShowFormatsDialog()
    {
        try
        {
            string downloadUrl = textboxText;
            Debug.WriteLine("Show formats dialog download url is" + downloadUrl);

            SetDownloadButtonStatus(DownloadButtonStatus.Download);

            YTDLP ytdlp = new();
            var videoInfo = await ytdlp.GetVideoInfoAsync(downloadUrl);

            SetDownloadButtonStatus(DownloadButtonStatus.Reset);
            if (videoInfo == null) return;


            var content = new SelectFormat();
            content.SetData(videoInfo);

            var dialog = new ContentDialog
            {
                Title = "Select format to download",
                Content = content,
                PrimaryButtonText = "Save",
                PrimaryButtonStyle = (Style)Resources["AccentButtonStyle"],
                CloseButtonText = "Cancel",
                IsPrimaryButtonEnabled = false,
                XamlRoot = Content.XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style
            };

            content.IsUserCanSave += canSave =>
            {
                dialog.IsPrimaryButtonEnabled = canSave;
            };

            var result = await dialog.ShowAsync();
            //var result = ContentDialogResult.Primary; // For testing without dialog

            if (result == ContentDialogResult.Primary)
            {
                App.DownloadService.Enqueue(downloadUrl, videoInfo, content.SelectedFormat);

                Debug.WriteLine("Download added to queue.");

                AppNotification notification = new AppNotificationBuilder()
                .AddText("Download Queued")
                .AddText(videoInfo.Title ?? "Unknown Title")
                .SetInlineImage(new Uri(WebUtility.HtmlEncode(videoInfo.Thumbnail!)))
                .BuildNotification();

                //.SetAppLogoOverride(new Uri(WebUtility.HtmlEncode(videoInfo.thumbnail!)), AppNotificationImageCrop.Default)

                AppNotificationManager.Default.Show(notification);
            }
            else
            {
                Debug.WriteLine("Download cancelled.");
            }

            content.IsUserCanSave -= null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            SetDownloadButtonStatus(DownloadButtonStatus.Reset);
            KnownErrors.Check(ex);
        }
    }

    private void UpdateTextDependingOnLink()
    {
        if (textboxText.Contains("youtube", StringComparison.OrdinalIgnoreCase))
        {
            YTDLPText.Foreground = Util.Util.GetAppGradient("youtube");
        }
        else if (textboxText.Contains("tiktok", StringComparison.OrdinalIgnoreCase))
        {
            YTDLPText.Foreground = Util.Util.GetAppGradient("tiktok");
        }
        else if (textboxText.Contains("instagram", StringComparison.OrdinalIgnoreCase))
        {
            YTDLPText.Foreground = Util.Util.GetAppGradient("instagram");
        }
        else YTDLPText.Foreground = App.Current.Resources["AccentTextFillColorPrimaryBrush"] as SolidColorBrush;
    }

    private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            Debug.WriteLine("TextBox text changed: " + textBox.Text);
            textboxText = textBox.Text;

            UpdateTextDependingOnLink();

            if (_isSearching) return;

            if (textboxText.Length == 0)
                DownloadButton.IsEnabled = false;
            else
                DownloadButton.IsEnabled = true;
        }
    }

    private void TextBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            Debug.WriteLine("Enter pressed!");

            Download();
        }
    }

    private async void PasteTextButton_Click(object sender, RoutedEventArgs e)
    {
        var package = Clipboard.GetContent();
        if (package.Contains(StandardDataFormats.Text))
        {
            var text = await package.GetTextAsync();
            LinkTextBox.Text = text;
        }
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        App.ApiServer.DownloadRequested -= DownloadRequested;
    }
}

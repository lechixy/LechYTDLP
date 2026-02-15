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
    private string _subtitleText = "❝Download any video, you like.❞";

    private string _textboxText = "";
    public string Text => _textboxText;

    public string SetText(string text) => _textboxText = text;

    public ObservableCollection<string> QueueCollection { get; } = [];

    public MainPage()
    {
        InitializeComponent();
        Subtitle.Text = _subtitleText;
        LinkTextBox.PlaceholderText = Main.GetDynamicSearchBoxPlaceholder();

        if (App.DownloadController.IsBusy)
        {
            DownloadButton.IsEnabled = false;
            LinkTextBox.Text = App.DownloadController.CurrentUrl;
            LinkTextBox.IsEnabled = false;
            PasteTextButton.IsEnabled = false;
        }
        else
        {
            LinkTextBox.Text = Text;

            if (Text.Length == 0)
                DownloadButton.IsEnabled = false;
            else
                DownloadButton.IsEnabled = true;
        }

        UpdateTextDependingOnLink(Text);

        Debug.WriteLine("Rendered one time: MainPage");

        App.DownloadController.BusyChanged += OnBusyChanged;
        // App.DownloadController.VideoInfoReady += OnVideoInfoReady;
        App.DownloadController.ErrorOccured += OnError;
    }

    private void OnBusyChanged(bool isBusy, string Url)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            DownloadButton.IsEnabled = !isBusy;
            LinkTextBox.IsEnabled = !isBusy;
            LinkTextBox.Text = Url;
            PasteTextButton.IsEnabled = !isBusy;

            if (isBusy)
            {
                DownloadButton.Content = new ProgressRing
                {
                    IsActive = true,
                    Width = 20,
                    Height = 20,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
            }
            else
            {
                DownloadButton.Content = new SymbolIcon(Symbol.Download);
            }
        });
    }

    private void OnError(string errorMessage)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            App.InfoBarService.Show(new InfoBarMessage("Unhandled Error", errorMessage, InfoBarSeverity.Error, 5000, true));
        });
    }

    private async void Download()
    {
        await App.DownloadController.SearchAsync(Text);
    }

    private void DownloadButton_Click(object sender, RoutedEventArgs e)
    {
        Download();
    }

    //enum DownloadButtonStatus
    //{
    //    Download,
    //    Reset
    //}

    //private async void SetDownloadButtonStatus(DownloadButtonStatus status)
    //{
    //    if (status == DownloadButtonStatus.Download)
    //    {
    //        _isSearching = true;
    //    }
    //    else if (status == DownloadButtonStatus.Reset)
    //    {
    //        DownloadButton.Content = new SymbolIcon(Symbol.Download);
    //        _isSearching = false;
    //        DownloadButton.IsEnabled = true;
    //    }
    //}

    private async void ShowFormatsDialog(string Url, VideoInfo videoInfo)
    {
        try
        {
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
                App.DownloadService.Enqueue(Url, videoInfo, content.SelectedFormat);

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
            KnownErrors.Check(ex);
        }
    }

    private void UpdateTextDependingOnLink(string link)
    {
        if (link.Contains("youtube", StringComparison.OrdinalIgnoreCase))
        {
            YTDLPText.Foreground = Util.Main.GetAppGradient("youtube");
        }
        else if (link.Contains("tiktok", StringComparison.OrdinalIgnoreCase))
        {
            YTDLPText.Foreground = Util.Main.GetAppGradient("tiktok");
        }
        else if (link.Contains("instagram", StringComparison.OrdinalIgnoreCase))
        {
            YTDLPText.Foreground = Util.Main.GetAppGradient("instagram");
        }
        else YTDLPText.Foreground = App.Current.Resources["AccentTextFillColorPrimaryBrush"] as SolidColorBrush;
    }

    private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            Debug.WriteLine("TextBox text changed: " + textBox.Text);
            SetText(textBox.Text);
            UpdateTextDependingOnLink(textBox.Text);

            if (App.DownloadController.IsBusy) return;

            if (Text.Length == 0)
                DownloadButton.IsEnabled = false;
            else
                DownloadButton.IsEnabled = true;
        }
    }

    //private void TextBox_KeyDown(object sender, KeyRoutedEventArgs e)
    //{
    //    if (e.Key == VirtualKey.Enter)
    //    {
    //        Download();
    //    }
    //}

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
        App.DownloadController.BusyChanged -= OnBusyChanged;
        // App.DownloadController.VideoInfoReady -= OnVideoInfoReady;
        App.DownloadController.ErrorOccured -= OnError;
    }
}

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
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.System;
using Windows.UI;
using static LechYTDLP.Views.SettingsPage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LechYTDLP.Views;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainPage : Page
{
    // Base
    private bool _initialized = false;
    // Text
    private string _textboxText = "";
    public string Text => _textboxText;
    public string SetText(string text) => _textboxText = text;

    public ObservableCollection<string> QueueCollection { get; } = [];

    public MainPage()
    {
        InitializeComponent();
        LinkTextBox.PlaceholderText = Main.GetDynamicSearchBoxPlaceholder();

        DispatcherQueue.TryEnqueue(() =>
        {
            bool success = LinkTextBox.Focus(FocusState.Programmatic);
            Debug.WriteLine(success);
        });

        PresetComboBox.ItemsSource = SettingsService.Presets;
        PresetComboBox.SelectedItem = SettingsService.SelectedPreset;
        _initialized = true;

        if (App.DownloadController.IsBusy)
        {
            DownloadButton.IsEnabled = false;
            DownloadButton.Content = new ProgressRing
            {
                IsActive = true,
                Width = 20,
                Height = 20,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            LinkTextBox.Text = App.DownloadController.CurrentUrl;
            LinkTextBox.IsEnabled = false;
            PasteTextButton.IsEnabled = false;
            PresetComboBox.IsEnabled = false;
        }
        else
        {
            LinkTextBox.Text = Text;
            PresetComboBox.IsEnabled = true;

            if (Text.Length == 0)
                DownloadButton.IsEnabled = false;
            else
                DownloadButton.IsEnabled = true;
        }

        UpdateTextDependingOnLink(Text);

        App.DownloadController.BusyChanged += OnBusyChanged;
        // App.DownloadController.VideoInfoReady += OnVideoInfoReady;
    }

    private void OnBusyChanged(bool isBusy, string Url)
    {
        Debug.WriteLine("Busy changed: " + isBusy + ", Url: " + Url);
        DispatcherQueue.TryEnqueue(() =>
        {
            DownloadButton.IsEnabled = !isBusy;
            LinkTextBox.IsEnabled = !isBusy;
            LinkTextBox.Text = Url;
            PasteTextButton.IsEnabled = !isBusy;
            PresetComboBox.IsEnabled = !isBusy;

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

    private async void OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button)
            return;

        if (button.Name == "DownloadButton")
        {
            await App.DownloadController.SearchAsync(Text);
        }
        else if (button.Name == "PasteTextButton")
        {
            try
            {
                var package = Clipboard.GetContent();

                if (package.Contains(StandardDataFormats.Text))
                {
                    var text = await package.GetTextAsync();
                    LinkTextBox.Text = text;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error pasting text: " + ex.Message);
            }
        }
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
            SetText(textBox.Text);
            UpdateTextDependingOnLink(textBox.Text);

            if (App.DownloadController.IsBusy) return;

            if (Text.Length == 0)
                DownloadButton.IsEnabled = false;
            else
                DownloadButton.IsEnabled = true;
        }
    }


    private void SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not ComboBox combo || !_initialized) return;

        if (combo.Name == "PresetComboBox")
        {
            var selection = (Setting)e.AddedItems[0];
            var preset = SettingsService.Presets.FirstOrDefault(p => p.Value.Equals(selection.Value));
            if (preset == null)
            {
                Debug.WriteLine("There is no preset like that");
                return;
            }
            SettingsService.SelectedPreset = preset;
        }
    }
    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        App.DownloadController.BusyChanged -= OnBusyChanged;
        // App.DownloadController.VideoInfoReady -= OnVideoInfoReady;
    }

    private void ElementLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            textBox.Focus(FocusState.Programmatic);
        }
    }
}

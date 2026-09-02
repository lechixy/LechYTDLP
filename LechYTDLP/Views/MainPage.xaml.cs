using LechYTDLP.Classes;
using LechYTDLP.Components;
using LechYTDLP.Controllers;
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
using Sentry;
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

    public ObservableCollection<SearchRequest> ActiveSearchRequests { get; } = [];

    public MainPage()
    {
        InitializeComponent();

        LinkTextBox.PlaceholderText = Main.GetDynamicSearchBoxPlaceholder();
        PresetComboBox.ItemsSource = SettingsService.Presets;
        PresetComboBox.SelectedItem = SettingsService.SelectedPreset;
        ProcessingListView.ItemsSource = ActiveSearchRequests;

        _initialized = true;

        LinkTextBox.Text = Text;
        UpdateTextDependingOnLink(Text);

        App.DownloadController.RequestsChanged += OnRequestsChanged;
        App.DownloadController.SearchStarted += OnSearchStarted;
        App.DownloadController.SearchFinished += OnSearchFinished;
        App.DownloadController.SearchCanceled += OnSearchCanceled;
        App.DownloadController.SearchFailed += OnSearchFailed;

        foreach (var request in App.DownloadController.ActiveRequests)
        {
            ActiveSearchRequests.Add(request);
        }

        //var mockRequests = new[]
        //{
        //    new SearchRequest("aadklşgkrw30ık9-6y0w36b906290690246b90*234069b23klsklşhsklşfh"),
        //    new SearchRequest("blskdfhklşisflşikh35byı0*eı0yıopetohlşkdflkşhe6yı*36o35op63o6"),
        //    new SearchRequest("asklhklşsadklşighaslşdkig30*e56350*ykoetrkye6390*6490*390*630c")
        //};

        //foreach (var request in mockRequests)
        //{
        //    ActiveSearchRequests.Add(request);
        //}

        UpdateGlobalInfoBar();

        DispatcherQueue.TryEnqueue(() =>
        {
            try
            {
                LinkTextBox.Focus(FocusState.Programmatic);
            }
            catch { }
        });
    }

    /**
     * Controller Events
     */

    private void OnSearchStarted(
        SearchRequest request)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            try
            {
                if (!ActiveSearchRequests.Any(
                    x => x.Id == request.Id))
                {
                    ActiveSearchRequests.Add(request);
                }

                UpdateGlobalInfoBar();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"Error adding search request: {ex.Message}");
                SentrySdk.CaptureException(ex);
            }
        });
    }

    private void OnSearchFinished(
        SearchRequest request)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            try
            {
                RemoveRequest(request.Id);
                UpdateGlobalInfoBar();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"Error removing search request: {ex.Message}");
                SentrySdk.CaptureException(ex);
            }
        });
    }


    private void OnSearchCanceled(
        SearchRequest request)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            try
            {
                RemoveRequest(request.Id);
                UpdateGlobalInfoBar();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"Error removing canceled search request: {ex.Message}");
                SentrySdk.CaptureException(ex);
            }
        });
    }


    private void OnSearchFailed(
        SearchRequest request,
        Exception exception)
    {
        Debug.WriteLine(
            $"Search failed: {request.Url}");

        Debug.WriteLine(
            exception);

        DispatcherQueue.TryEnqueue(() =>
        {
            try
            {
                RemoveRequest(request.Id);
                UpdateGlobalInfoBar();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"Error removing failed search request: {ex.Message}");
                SentrySdk.CaptureException(ex);
            }
        });
    }

    private void OnRequestsChanged()
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            try
            {
                var requests = App.DownloadController.ActiveRequests;
                ActiveSearchRequests.Clear();

                foreach (var request in requests)
                {
                    ActiveSearchRequests.Add(request);
                }

                UpdateGlobalInfoBar();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"Error updating search requests: {ex.Message}");
                SentrySdk.CaptureException(ex);
            }
        });
    }

    private void RemoveRequest(Guid requestId)
    {
        var request =
            ActiveSearchRequests.FirstOrDefault(
                x => x.Id == requestId);

        if (request != null)
        {
            ActiveSearchRequests.Remove(request);
        }
    }

    private void UpdateGlobalInfoBar()
    {
        int count =
            ActiveSearchRequests.Count;

        if (count <= 0)
        {
            ProcessingButton.Visibility = Visibility.Collapsed;
            ProcessingTeachingTip.IsOpen = false;
            return;
        }

        if (ProcessingButton.Visibility != Visibility.Visible) ProcessingButton.Visibility = Visibility.Visible;

        ProcessingButtonText.Text =
            count == 1
                ? App.LocalizationService.Get("UrlProcessing")
                : App.LocalizationService.Get("UrlsProcessing", count);
    }

    public void CancelSearch(Guid requestId)
    {
        bool canceled =
            App.DownloadController.Cancel(requestId);

        Debug.WriteLine(
            $"Cancel search {requestId}: {canceled}");
    }

    private void CancelAllSearches()
    {
        App.DownloadController.CancelAll();
    }

    /**
     * UI Events
     */

    private void OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button)
            return;

        if (button.Name == "DownloadButton")
        {
            StartSearch();
            return;
        }
        else if (button.Name == "PasteTextButton")
        {
            _ = PasteAndSearchAsync();
        }
        else if (button.Name == "ProcessingButton")
        {
            ProcessingTeachingTip.IsOpen = !ProcessingTeachingTip.IsOpen;
        }
        else if (button.Name == "ProcessingCancelButton")
        {
            CancelSearch(((SearchRequest)button.Tag).Id);
        }
    }

    private void StartSearch()
    {
        var url =
            LinkTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(url))
            return;

        LinkTextBox.Text = string.Empty;

        foreach (var singleUrl in url.Split(' '))
        {
            _ = App.DownloadController.SearchAsync(singleUrl.Trim());
        }
    }

    private async Task PasteAndSearchAsync()
    {
        try
        {
            var package = Clipboard.GetContent();

            if (!package.Contains(StandardDataFormats.Text))
                return;

            var text = await package.GetTextAsync();

            LinkTextBox.Text = text;

            if (SettingsService.DownloadAfterPaste)
            {
                var url = LinkTextBox.Text.Trim();

                if (!string.IsNullOrWhiteSpace(url))
                {
                    LinkTextBox.Text = string.Empty;

                    _ = App.DownloadController.SearchAsync(url);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(
                "Error pasting text: " + ex.Message);
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

    private void TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            if (textBox.Name == "LinkTextBox")
            {
                SetText(textBox.Text);
                UpdateTextDependingOnLink(textBox.Text);

                if (Text.Length == 0)
                    DownloadButton.IsEnabled = false;
                else
                    DownloadButton.IsEnabled = true;
            }
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

    private void ElementLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            if (textBox.Name == "LinkTextBox")
            {
                textBox.Focus(FocusState.Programmatic);
            }
        }
    }

    private void OnKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            if (textBox.Name == "LinkTextBox" && e.Key == VirtualKey.Enter && DownloadButton.IsEnabled)
            {
                e.Handled = true;
                OnClick(DownloadButton, new RoutedEventArgs());
            }
        }
    }
    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);

        App.DownloadController.RequestsChanged -= OnRequestsChanged;
        App.DownloadController.SearchStarted -= OnSearchStarted;
        App.DownloadController.SearchFinished -= OnSearchFinished;
        App.DownloadController.SearchCanceled -= OnSearchCanceled;
        App.DownloadController.SearchFailed -= OnSearchFailed;
    }
}

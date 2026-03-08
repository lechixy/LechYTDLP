using LechYTDLP.Classes;
using LechYTDLP.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LechYTDLP.Views;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class SettingsPage : Page
{

    public class LanguageItem
    {
        public required string DisplayName { get; set; }
        public required string Code { get; set; }
    }
    public class ThemeItem
    {
        public required string DisplayName { get; set; }
        public required string Value { get; set; }
    }
    enum SettingType
    {
        PathYTDLP,
        PathFFMPEG
    }

    private bool _isInitializingTheme = true;

    public SettingsPage()
    {
        InitializeComponent();

        // # Customize YT-DLP
        YTDLPPathSetting.PlaceholderText = SettingsService.YTDLPPath;
        YTDLPPathSetting.Text = SettingsService.YTDLPPath;

        FFmpegPathSetting.PlaceholderText = SettingsService.FFmpegPath;
        FFmpegPathSetting.Text = SettingsService.FFmpegPath;

        var textInfo = new CultureInfo("en-US", false).TextInfo;
        JsRuntimeComboBox.SelectedItem = string.IsNullOrEmpty(SettingsService.JavaScriptRuntime)
            ? "None"
            : textInfo.ToTitleCase(SettingsService.JavaScriptRuntime);

        // # Customization
        _isInitializingTheme = true;
        AppThemeComboBox.ItemsSource = SettingsService.Themes;
        AppThemeComboBox.SelectedIndex = SettingsService.Themes.FindIndex(x => x.Value == SettingsService.AppTheme.Value);
        AppBackdropComboBox.ItemsSource = SettingsService.Backdrops;
        AppBackdropComboBox.SelectedIndex = SettingsService.Backdrops.FindIndex(x => x.Value == SettingsService.AppBackdrop.Value);

        // # Developer Options
        #if DEBUG
        AppLanguageSetting.IsEnabled = true;
        #endif
        AppLanguageComboBox.ItemsSource = SettingsService.Languages;
        AppLanguageComboBox.SelectedIndex = SettingsService.Languages.FindIndex(x => x.Code == SettingsService.AppLanguage.Code);
        _isInitializingTheme = false;
        YTdlpVerboseSettingSwitch.IsOn = SettingsService.UseVerboseLoggingOnYTDLP;
        BlobDataSettingSwitch.IsOn = SettingsService.IsUsingBlobData;
        WriteVideoInfoSettingSwitch.IsOn = SettingsService.WriteVideoInfoJson;

        // # About
        AboutExpander.Description = App.LocalizationService.Get("MadeBy");
        AboutExpanderContent.Text = $"{App.LocalizationService.Get("Version")} {App.GetAppVersion()}";
        #if DEBUG
        AboutExpanderContent.Text += $" ({App.LocalizationService.Get("DevelopmentVersion")} • {App.LocalizationService.Get("Debug")})";
        #endif
        GithubLink.NavigateUri = new Uri(Util.Main.GetLink(Util.Links.GitHubRepository));
    }

    private void ChangeSetting(SettingType Setting)
    {
        if (Setting == SettingType.PathYTDLP)
        {
            if (YTDLPPathSetting.Text.Length == 0)
            {
                SettingsService.ResetSetting(nameof(SettingsService.YTDLPPath));
                YTDLPPathSetting.PlaceholderText = SettingsService.YTDLPPath;
            }
            else
            {
                SettingsService.YTDLPPath = YTDLPPathSetting.Text;
                // Every time even if the letter changes , it can cause a lot of popups
                // App.InfoBarService.Show(new InfoBarMessage("Setting updated", "", InfoBarSeverity.Success));
            }
        }
        else if (Setting == SettingType.PathFFMPEG)
        {
            if (FFmpegPathSetting.Text.Length == 0)
            {
                SettingsService.ResetSetting(nameof(SettingsService.FFmpegPath));
                FFmpegPathSetting.PlaceholderText = SettingsService.FFmpegPath;
            }
            else
            {
                SettingsService.FFmpegPath = FFmpegPathSetting.Text;
                // Every time even if the letter changes , it can cause a lot of popups
                // App.InfoBarService.Show(new InfoBarMessage("Setting updated", "", InfoBarSeverity.Success));
            }

        }
    }

    private void PathSetting_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            if (textBox.Name == "YTDLPPathSetting") ChangeSetting(SettingType.PathYTDLP);
            else if (textBox.Name == "FFmpegPathSetting") ChangeSetting(SettingType.PathFFMPEG);
        }
    }

    private async void CheckVersion()
    {
        ApplicationStatusCheck.Description = App.LocalizationService.Get("ApplicationStatusCheck");
        CheckDependsButton.IsEnabled = false;
        CheckDependsButton.Content = new ProgressRing
        {
            IsActive = true,
            Width = 20,
            Height = 20,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        try
        {
            var YTdlpVersion = await YTDLP.CheckExecutable(YTDLP.CheckExecutableApp.YTDLP);
            ApplicationStatusCheck.Description = $"✅ YT-DLP ({YTdlpVersion}) • {App.LocalizationService.Get("ApplicationStatusCheckCheckingFFmpeg")}";
            var FFmpegVersion = await YTDLP.CheckExecutable(YTDLP.CheckExecutableApp.FFMPEG);
            ApplicationStatusCheck.Description = $"✅ YT-DLP ({YTdlpVersion}) • ✅ FFmpeg ({FFmpegVersion})";
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            var ErroredApp = ex.Message.Split('"')[1] == SettingsService.YTDLPPath ? "YT-DLP" : "FFmpeg";
            ApplicationStatusCheck.Description = $"⚠️ {ErroredApp} {App.LocalizationService.Get("ApplicationStatusCheckNotWorking")}";
        }

        CheckDependsButton.Content = App.LocalizationService.Get("Check");
        CheckDependsButton.IsEnabled = true;
    }

    private void Switch_Toggled(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleSwitch toggleSwitch)
        {
            if (toggleSwitch.Name == "BlobDataSettingSwitch")
            {
                SettingsService.IsUsingBlobData = toggleSwitch.IsOn;
            }
            else if (toggleSwitch.Name == "WriteVideoInfoSettingSwitch")
            {
                SettingsService.WriteVideoInfoJson = toggleSwitch.IsOn;
            }
            else if (toggleSwitch.Name == "YTdlpVerboseSettingSwitch")
            {
                SettingsService.UseVerboseLoggingOnYTDLP = toggleSwitch.IsOn;
            }
        }
    }

    private async void PickFile(SettingType Which)
    {
        if (App.Window == null) return;
        var path = await App.PickFileAsync([".exe"], App.Window);
        if (path == null) return;

        if (Which == SettingType.PathYTDLP)
            YTDLPPathSetting.Text = path;
        else if (Which == SettingType.PathFFMPEG)
            FFmpegPathSetting.Text = path;
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn)
        {
            if (btn.Name == "PickYTdlpButton")
                PickFile(SettingType.PathYTDLP);
            else if (btn.Name == "PickFFmpegButton")
                PickFile(SettingType.PathFFMPEG);
            else if (btn.Name == "CheckDependsButton")
                DispatcherQueue.TryEnqueue(CheckVersion);
            else if (btn.Name == "CheckYTdlpUpdateButton")
            {
                DispatcherQueue.TryEnqueue(async () =>
                {
                    CheckYTdlpUpdateButton.IsEnabled = false;
                    CheckYTdlpUpdateButton.Content = new ProgressRing
                    {
                        IsActive = true,
                        Width = 20,
                        Height = 20,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };

                    var ytdlp = new YTDLP();
                    UpdateResult updateInfo = await ytdlp.CheckForUpdates();

                    if (updateInfo.Status == UpdateStatus.UpToDate)
                    {
                        YTdlpUpdateCheck.Description = $"✅ {App.LocalizationService.Get("YTdlpIsUpToDate")} ({updateInfo.Message})";
                    }
                    else if (updateInfo.Status == UpdateStatus.Updated)
                    {
                        YTdlpUpdateCheck.Description = $"✅ {App.LocalizationService.Get("YTdlpUpdatedSuccessfully")} ({updateInfo.Message})";
                    }
                    else
                    {
                        YTdlpUpdateCheck.Description = $"⚠️ {App.LocalizationService.Get("YTdlpUpdateCheckFailed")}";
                    }

                    CheckYTdlpUpdateButton.IsEnabled = true;
                    CheckYTdlpUpdateButton.Content = App.LocalizationService.Get("Check");
                });
            } else if (btn.Name == "ImportSettingsButton") {
                SettingsService.ImportSettingsFromFile();
            } else if (btn.Name == "ExportSettingsButton")
            {
                SettingsService.ExportSettings();
            }
        }
    }

    private void SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox comboBox)
        {
            if (_isInitializingTheme) return;

            if (comboBox.Name == "JsRuntimeComboBox")
            {
                string selected = (string)comboBox.SelectedItem;

                // Use "None" as a display value for an empty string, but store an empty string in settings
                if (selected == "None") selected = "";
                SettingsService.JavaScriptRuntime = selected.ToLower();
            }
            else if (comboBox.Name == "AppThemeComboBox")
            {
                var selectedTheme = (ThemeItem)comboBox.SelectedItem;
                SettingsService.AppTheme = selectedTheme;
                App.SettingsService.AppThemeChanged?.Invoke(selectedTheme);
            }
            else if (comboBox.Name == "AppBackdropComboBox")
            {
                var selectedBackdrop = (ThemeItem)comboBox.SelectedItem;
                SettingsService.AppBackdrop = selectedBackdrop;
                App.SettingsService.AppBackdropChanged?.Invoke(selectedBackdrop);
            }
            else if (comboBox.Name == "AppLanguageComboBox")
            {
                var selectedLanguage = (LanguageItem)comboBox.SelectedItem;
                LocalizationService.SetLanguage(selectedLanguage);
            }
        }
    }
}

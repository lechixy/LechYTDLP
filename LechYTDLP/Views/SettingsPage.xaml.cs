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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using LechYTDLP.Classes;
using System.Diagnostics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LechYTDLP.Views;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class SettingsPage : Page
{
    enum SettingType
    {
        PathYTDLP,
        PathFFMPEG
    }

    public SettingsPage()
    {
        InitializeComponent();

        AboutExpander.Description = $"🤍 Made by @lechixy";
        AboutExpanderContent.Text = $"Version {App.AppVersion}";
        GithubLink.NavigateUri = new Uri(App.GithubLink);

        YTDLPPathSetting.PlaceholderText = SettingsService.YTDLPPath;
        YTDLPPathSetting.Text = SettingsService.YTDLPPath;

        FFmpegPathSetting.PlaceholderText = SettingsService.FFmpegPath;
        FFmpegPathSetting.Text = SettingsService.FFmpegPath;


        BlobDataSettingSwitch.IsOn = SettingsService.IsUsingBlobData;
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
        ApplicationStatusCheck.Description = "Checking YT-DLP and FFmpeg versions...";
        try
        {
            var YTdlpVersion = await YTDLP.CheckExecutable(YTDLP.CheckExecutableApp.YTDLP);
            ApplicationStatusCheck.Description = $"✅ YT-DLP ({YTdlpVersion}) • Checking FFmpeg version...";
            var FFmpegVersion = await YTDLP.CheckExecutable(YTDLP.CheckExecutableApp.FFMPEG);
            ApplicationStatusCheck.Description = $"✅ YT-DLP ({YTdlpVersion}) • ✅ FFmpeg ({FFmpegVersion})";
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            var ErroredApp = ex.Message.Split('"')[1] == SettingsService.YTDLPPath ? "YT-DLP" : "FFmpeg";
            ApplicationStatusCheck.Description = $"⚠️ {ErroredApp} is not working, check executable path.";
            return;
        }
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
        }
    }

    private void CheckDependencies_Click(object sender, RoutedEventArgs e)
    {
        CheckVersion();
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
        }
    }
}

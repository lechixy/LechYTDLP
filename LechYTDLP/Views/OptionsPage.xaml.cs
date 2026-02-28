using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using LechYTDLP.Services;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LechYTDLP.Views;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class OptionsPage : Page {
    enum OptionType
    {
        CookiesFile
    }

    public OptionsPage()
    {
        InitializeComponent();

        // File
        SaveToTextBox.Text = SettingsService.DownloadPath;
        FilenameTextBox.Text = SettingsService.FilenameTemplate;
        EmbedThumbnailSettingSwitch.IsOn = SettingsService.EmbedThumbnail;
        EmbedSubsSettingSwitch.IsOn = SettingsService.EmbedSubs;

        // Account
        CookiesFileTextBox.Text = SettingsService.CookiesfilePath;

        // Hyperlinks
        FilenameHyperLink.NavigateUri = new Uri(Util.Main.GetLink(Util.Links.OutputTemplate));
        CookiesFileHyperLink.NavigateUri = new Uri(Util.Main.GetLink(Util.Links.WhyINeedToMyPassCookiesHere));
    }

    private void SwitchToggled(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleSwitch toggle){
            if (toggle.Name == "EmbedThumbnailSettingSwitch")
            {
                SettingsService.EmbedThumbnail = toggle.IsOn;
            }
            else if (toggle.Name == "EmbedSubsSettingSwitch")
            {
                SettingsService.EmbedSubs = toggle.IsOn;
            }
        }
    }

    private void TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textbox)
        {
            if (textbox.Name == "FileNameTextBox")
            {
                if (textbox.Text.Length == 0)
                {
                    SettingsService.ResetSetting(nameof(SettingsService.FilenameTemplate));
                    textbox.PlaceholderText = SettingsService.FilenameTemplate;
                }
                else
                    SettingsService.FilenameTemplate = textbox.Text;
            }
            else if (textbox.Name == "SaveToTextBox")
            {
                if (textbox.Text.Length == 0)
                {
                    SettingsService.ResetSetting(nameof(SettingsService.DownloadPath));
                    textbox.PlaceholderText = SettingsService.DownloadPath;
                }
                else
                    SettingsService.DownloadPath = textbox.Text;
            }
            else if (textbox.Name == "CookiesFileTextBox")
            {
                if (textbox.Text.Length == 0)
                {
                    SettingsService.ResetSetting(nameof(SettingsService.CookiesfilePath));
                    textbox.PlaceholderText = SettingsService.CookiesfilePath;
                }
                else
                    SettingsService.CookiesfilePath = textbox.Text;

            }
        }
    }

    private async void PickFile(OptionType Which)
    {
        if (App.Window == null) return;
        var path = await App.PickFileAsync([".txt"], App.Window);
        if (path == null) return;

        if (Which == OptionType.CookiesFile)
            CookiesFileTextBox.Text = path;
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn)
        {
            if (btn.Name == "PickCookiesFileButton") PickFile(OptionType.CookiesFile);
        }
    }
}

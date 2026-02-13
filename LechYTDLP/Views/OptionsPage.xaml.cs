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
public sealed partial class OptionsPage : Page
{
    public OptionsPage()
    {
        InitializeComponent();
        SaveToTextBox.PlaceholderText = SettingsService.DownloadPath;
        SaveToTextBox.Text = SettingsService.DownloadPath;

        FileNameTextBox.PlaceholderText = SettingsService.FilenameTemplate;
        FileNameTextBox.Text = SettingsService.FilenameTemplate;
    }

    private void SaveToTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if(SaveToTextBox.Text.Length == 0)
        {
            SettingsService.ResetSetting(nameof(SettingsService.DownloadPath));
            SaveToTextBox.PlaceholderText = SettingsService.DownloadPath;
        }
        else
            SettingsService.DownloadPath = SaveToTextBox.Text;
    }

    private void FileNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (FileNameTextBox.Text.Length == 0)
        {
            SettingsService.ResetSetting(nameof(SettingsService.FilenameTemplate));
            FileNameTextBox.PlaceholderText = SettingsService.FilenameTemplate;
        }
        else
            SettingsService.FilenameTemplate = FileNameTextBox.Text;
    }
}

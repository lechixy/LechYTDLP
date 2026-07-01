using CommunityToolkit.Mvvm.Input;
using LechYTDLP.Services;
using LechYTDLP.Util;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using static LechYTDLP.Views.SettingsPage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LechYTDLP.Views;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class OptionsPage : Page
{
    enum OptionType
    {
        CookiesFile,
        DownloadPath
    }

    private static readonly Dictionary<string, string> PlaceholderMap = new()
    {
        ["id"] = App.LocalizationService.Get("P_ID"),
        ["title"] = App.LocalizationService.Get("P_Title"),
        ["ext"] = App.LocalizationService.Get("P_Extension"),
        ["uploader"] = App.LocalizationService.Get("P_Uploader"),
        ["channel"] = App.LocalizationService.Get("P_ChannelName"),
        ["creator"] = App.LocalizationService.Get("P_Creator"),
        ["playlist_index"] = App.LocalizationService.Get("P_PlaylistIndex"),
        ["playlist_title"] = App.LocalizationService.Get("P_PlaylistTitle"),
        ["extractor"] = App.LocalizationService.Get("P_Extractor"),
        ["extractor_key"] = App.LocalizationService.Get("P_ExtractorKey"),
        ["fulltitle"] = App.LocalizationService.Get("P_FullTitle"),
        ["upload_date"] = App.LocalizationService.Get("P_UploadDate") + " (YYYYMMDD)",
        ["release_date"] = App.LocalizationService.Get("P_ReleaseDate") + " (YYYYMMDD)",
        ["duration"] = App.LocalizationService.Get("P_Duration"),
        ["channel_id"] = App.LocalizationService.Get("P_ChannelID"),
        ["timestamp"] = App.LocalizationService.Get("P_Timestamp"),
    };

    public ObservableCollection<Setting> Placeholders { get; } =
        new ObservableCollection<Setting>(
            PlaceholderMap.Select(x => new Setting
            {
                Value = x.Key,
                DisplayName = x.Value
            }));

    string LocalFilenameTemplateText = SettingsService.FilenameTemplate;
    ICommand InsertTextCommand => new RelayCommand<string?>(InsertText);

    public bool _isInitializing = true;

    public OptionsPage()
    {
        InitializeComponent();

        // Placeholders menüsünü doldur
        foreach (var item in Placeholders)
        {
            FileNameTemplatePlaceholdersMenuFlyout.Items.Add(new MenuFlyoutItem
            {
                Text = item.DisplayName,
                Command = InsertTextCommand,
                CommandParameter = item.Value
            });
        }
        // Kaydet butonunu devre dışı bırak
        if (LocalFilenameTemplateText != SettingsService.FilenameTemplate)
            FileNameTemplateSaveButton.IsEnabled = true;
        else
            FileNameTemplateSaveButton.IsEnabled = false;

        // Önizleme kısmını güncelle
        var preview = FilenameTemplateHelper.ReplaceFilenameTemplateWithSampleData(SettingsService.FilenameTemplate, App.SampleJson);
        if (preview != null) FileNamePreviewSettingsCard.Header = preview;

        // File
        SaveToTextBox.Text = SettingsService.DownloadPath;
        FileNameTextBox.Text = SettingsService.FilenameTemplate;
        ForceOverwritesSwitch.IsOn = SettingsService.ForceOverwrites;
        SaveLogOfEachDownloadSwitch.IsOn = SettingsService.SaveLogOfEachDownload;
        EmbedThumbnailSettingSwitch.IsOn = SettingsService.EmbedThumbnail;
        EmbedSubsSettingSwitch.IsOn = SettingsService.EmbedSubs;

        // Downloads
        ConcurrentFragmentsSettingSlider.Value = SettingsService.ConcurrentFragments;

        // Account
        CookiesFileTextBox.Text = SettingsService.CookiesfilePath;

        // More
        CustomParamsTextBox.Text = SettingsService.CustomYtDlpParams;

        // Hyperlinks
        FileNameSettingsHyperLink.Content = App.LocalizationService.Get("LearnMore");
        FileNameSettingsHyperLink.NavigateUri = new Uri(Util.Main.GetLink(Util.Links.OutputTemplate));
        CookiesFileHyperLink.NavigateUri = new Uri(Util.Main.GetLink(Util.Links.WhyINeedToMyPassCookiesHere));


        _isInitializing = false;
    }

    private void InsertText(string? text)
    {
        if (FileNameTextBox != null && text != null)
        {
            var selectionStart = FileNameTextBox.SelectionStart;
            FileNameTextBox.Text = FileNameTextBox.Text.Insert(0, $"%({text})s");
            LocalFilenameTemplateText = FileNameTextBox.Text;
            FileNameTextBox.SelectionStart = selectionStart + text.Length + 2; // Move cursor after the inserted text
        }
    }

    private bool IsValidFilenameTemplate(string template, out string? error)
    {
        error = null;

        if (string.IsNullOrWhiteSpace(template))
        {
            error = App.LocalizationService.Get("FilenameTemplateCannotBeEmpty");
            return false;
        }

        // %(...)s pattern
        var matches = YTdlpPlaceholderRegex().Matches(template);

        int index = 0;

        foreach (Match match in matches)
        {
            // Arada bozuk bir şey var mı (örneğin %(title) eksik s)
            var start = match.Index;
            if (start > index)
            {
                var between = template.Substring(index, start - index);
                if (between.Contains("%("))
                {
                    error = App.LocalizationService.Get("FilenameTemplateInvalidPlaceholder");
                    return false;
                }
            }

            var key = match.Groups[1].Value;

            // Boş key kontrolü: %()s
            if (string.IsNullOrWhiteSpace(key))
            {
                error = App.LocalizationService.Get("FileNameEmptyPlaceholder");
                return false;
            }

            // İstersen karakter whitelist koyabilirsin
            if (!Regex.IsMatch(key, @"^[a-zA-Z0-9_]+$"))
            {
                error = App.LocalizationService.Get("FileNameInvalidCharacterPlaceholder", key);
                return false;
            }

            index = start + match.Length;
        }

        // Sonda yarım kalan pattern var mı (%(title gibi)
        if (template.Substring(index).Contains("%("))
        {
            // Yarım kalmış bir placeholder var, bu da geçersiz
            // ENGLISH = "There is a half-finished placeholder, which is also invalid"
            error = App.LocalizationService.Get("FilenameTemplateInvalidHalfPlaceholder");
            return false;
        }

        return true;
    }

    private void SwitchToggled(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleSwitch toggle)
        {
            if (toggle == ForceOverwritesSwitch)
            {
                SettingsService.ForceOverwrites = toggle.IsOn;
            }
            else if (toggle == SaveLogOfEachDownloadSwitch)
            {
                SettingsService.SaveLogOfEachDownload = toggle.IsOn;
            }
            else if (toggle == EmbedThumbnailSettingSwitch)
            {
                SettingsService.EmbedThumbnail = toggle.IsOn;
            }
            else if (toggle == EmbedSubsSettingSwitch)
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
                LocalFilenameTemplateText = textbox.Text;

                // Geçersiz bir şey yazıldıysa kaydetme, hata göster
                if (!IsValidFilenameTemplate(textbox.Text, out var error))
                {
                    error ??= App.LocalizationService.Get("UnknownError");
                    FileNameActionsSettingsCard.Header = error;
                    FileNameTemplateSaveButton.IsEnabled = false;
                }
                else
                {
                    if (LocalFilenameTemplateText != SettingsService.FilenameTemplate)
                    {
                        FileNameActionsSettingsCard.Header = App.LocalizationService.Get("FilenameOkLooksGood");
                        FileNameTemplateSaveButton.IsEnabled = true;
                    }
                    else
                    {
                        FileNameActionsSettingsCard.Header = "";
                        FileNameTemplateSaveButton.IsEnabled = false;
                    }

                    // Önizleme kısmını güncelle
                    var preview = FilenameTemplateHelper.ReplaceFilenameTemplateWithSampleData(LocalFilenameTemplateText, App.SampleJson);
                    if (preview != null) FileNamePreviewSettingsCard.Header = preview;
                }

                //if (textbox.Text.Length == 0)
                //{
                //    SettingsService.ResetSetting(nameof(SettingsService.FilenameTemplate));
                //    LocalFilenameTemplateText = SettingsService.FilenameTemplate;
                //    textbox.PlaceholderText = SettingsService.FilenameTemplate;
                //}
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
            else if (textbox.Name == "CustomParamsTextBox")
            {
                if (textbox.Text.Length == 0)
                {
                    SettingsService.ResetSetting(nameof(SettingsService.CustomYtDlpParams));
                    textbox.PlaceholderText = SettingsService.CustomYtDlpParams;
                }
                else
                    SettingsService.CustomYtDlpParams = textbox.Text;
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

    private async void PickFolder(OptionType Which)
    {
        if (App.Window == null) return;
        var path = await App.PickFolderAsync(App.Window);
        if (path == null) return;
        if (Which == OptionType.DownloadPath)
            SaveToTextBox.Text = path;
    }

    private void Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn)
        {
            if (btn.Name == "PickCookiesFileButton") PickFile(OptionType.CookiesFile);
            else if (btn.Name == "SaveToButton") PickFolder(OptionType.DownloadPath);
            else if (btn.Name == "FileNameTemplateSaveButton")
            {
                if (!IsValidFilenameTemplate(LocalFilenameTemplateText, out var error))
                {
                    error ??= App.LocalizationService.Get("UnknownError");
                    FileNameActionsSettingsCard.Header = error;
                    return;
                }

                SettingsService.FilenameTemplate = LocalFilenameTemplateText;
                FileNameActionsSettingsCard.Header = App.LocalizationService.Get("Saved!");
                FileNameTemplateSaveButton.IsEnabled = false;
            }
        }

        if (sender is DropDownButton dropDownButton)
        {
            if (dropDownButton.Name == "PlaceholdersDropDownButton")
            {
                var menu = dropDownButton.Flyout as MenuFlyout;
                if (menu != null)
                {
                    menu.Items.Clear();
                    foreach (var placeholder in Placeholders)
                    {
                        var item = new MenuFlyoutItem
                        {
                            Text = placeholder.DisplayName,
                            Command = new RelayCommand(() => InsertText(placeholder.Value))
                        };
                        menu.Items.Add(item);
                    }
                }
            }
        }
    }

    [GeneratedRegex(@"%\((.*?)\)s")]
    private static partial Regex YTdlpPlaceholderRegex();

    private void ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        if (_isInitializing) return;

        if (sender is Slider slider)
        {
            if (slider.Name == "ConcurrentFragmentsSettingSlider")
            {
                SettingsService.ConcurrentFragments = (int)slider.Value;
                //ConcurrentFragmentsSettingText.Text = $"{slider.Value}";
            }
        }
    }
}

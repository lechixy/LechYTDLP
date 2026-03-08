using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using LechYTDLP.Util;
using System.IO;
using static LechYTDLP.Views.SettingsPage;
using System.Text.Json;
using System.Diagnostics;
using Microsoft.UI.Xaml.Controls;

namespace LechYTDLP.Services
{
    public class SettingsService
    {

        public Action<ThemeItem>? AppThemeChanged;
        public Action<ThemeItem>? AppBackdropChanged;

        private static ApplicationDataContainer Settings =>
            ApplicationData.Current.LocalSettings;

        private static string BasePath = ApplicationData.Current.LocalFolder.Path;

        // # Logs Page
        public static bool AutoScrollLogs
        {
            get => (bool?)Settings.Values[nameof(AutoScrollLogs)]
                   ?? true;
            set => Settings.Values[nameof(AutoScrollLogs)] = value;
        }

        // # Options
        // File
        public static string FilenameTemplate
        {
            get => (string?)Settings.Values[nameof(FilenameTemplate)]
                   ?? "%(uploader)s_%(id)s.%(ext)s";

            set => Settings.Values[nameof(FilenameTemplate)] = value;
        }

        public static string DownloadPath
        {
            get => (string?)Settings.Values[nameof(DownloadPath)]
                   ?? LechKnownFolders.GetPath(LechKnownFolder.Downloads);

            set => Settings.Values[nameof(DownloadPath)] = value;
        }
        public static bool EmbedThumbnail
        {
            get => (bool?)Settings.Values[nameof(EmbedThumbnail)]
                   ?? false;
            set => Settings.Values[nameof(EmbedThumbnail)] = value;
        }
        public static bool EmbedSubs
        {
            get => (bool?)Settings.Values[nameof(EmbedSubs)]
                   ?? false;
            set => Settings.Values[nameof(EmbedSubs)] = value;
        }
        // Account
        public static string CookiesfilePath
        {
            get => (string?)Settings.Values[nameof(CookiesfilePath)]
                   ?? string.Empty;
            set => Settings.Values[nameof(CookiesfilePath)] = value;
        }

        // # Settings
        // Customize YT-DLP
        public static string YTDLPPath
        {
            get => (string?)Settings.Values[nameof(YTDLPPath)]
                   ?? Path.Combine(BasePath, "Tools", "yt-dlp.exe");

            set => Settings.Values[nameof(YTDLPPath)] = value;
        }
        public static string FFmpegPath
        {
            get => (string?)Settings.Values[nameof(FFmpegPath)]
                   ?? Path.Combine(BasePath, "Tools", "ffmpeg.exe");

            set => Settings.Values[nameof(FFmpegPath)] = value;
        }
        public static string JavaScriptRuntime
        {
            get => (string?)Settings.Values[nameof(JavaScriptRuntime)]
                ?? "";
            set => Settings.Values[nameof(JavaScriptRuntime)] = value;
        }
        // # Customization
        public static List<LanguageItem> Languages = [
            new LanguageItem { DisplayName = App.LocalizationService.Get("System"), Code = "system" },
            new LanguageItem { DisplayName = "English", Code = "en-US" },
            new LanguageItem { DisplayName = "Türkçe", Code = "tr-TR" },
        ];
        public static string? _DefaultSystemLanguageCode
        {
            get
            {
                var value = Settings.Values[nameof(_DefaultSystemLanguageCode)] as string ?? null;
                return value;
            }
            set
            {
                Settings.Values[nameof(_DefaultSystemLanguageCode)] = value;
            }
        }
        public static LanguageItem AppLanguage
        {
            get
            {
                var value = Settings.Values[nameof(AppLanguage)] as string ?? "system";
                return Languages.FirstOrDefault(l => l.Code == value) ?? Languages[0];
            }
            set
            {
                Settings.Values[nameof(AppLanguage)] = value.Code;
            }
        }
        public static List<ThemeItem> Themes { get; } = [
            new ThemeItem { DisplayName = App.LocalizationService.Get("System"), Value = "system" },
            new ThemeItem { DisplayName = App.LocalizationService.Get("Light"), Value = "light" },
            new ThemeItem { DisplayName = App.LocalizationService.Get("Dark"), Value = "dark" },
        ];
        public static ThemeItem AppTheme
        {
            get
            {
                var value = Settings.Values[nameof(AppTheme)] as string ?? "system";
                return Themes.FirstOrDefault(t => t.Value == value) ?? Themes[0];
            }
            set
            {
                Settings.Values[nameof(AppTheme)] = value.Value;
            }
        }
        public static List<ThemeItem> Backdrops { get; } = [
            new ThemeItem { DisplayName = App.LocalizationService.Get("None"), Value = "none" },
            new ThemeItem { DisplayName = "Mica", Value = "mica" },
            new ThemeItem { DisplayName = App.LocalizationService.Get("Acrylic"), Value = "acrylic" },
            new ThemeItem { DisplayName = "MicaAlt", Value = "micaalt" },
        ];
        public static ThemeItem AppBackdrop
        {
            get
            {
                var value = Settings.Values[nameof(AppBackdrop)] as string ?? "micaalt";
                return Backdrops.FirstOrDefault(b => b.Value == value) ?? Backdrops.Last();
            }
            set
            {
                Settings.Values[nameof(AppBackdrop)] = value.Value;
            }
        }
        // # Developer options
        public static bool UseVerboseLoggingOnYTDLP
        {
            get => (bool?)Settings.Values[nameof(UseVerboseLoggingOnYTDLP)]
                   ?? false;
            set => Settings.Values[nameof(UseVerboseLoggingOnYTDLP)] = value;
        }
        public static bool IsUsingBlobData
        {
            get => (bool?)Settings.Values[nameof(IsUsingBlobData)]
                   ?? false;

            set => Settings.Values[nameof(IsUsingBlobData)] = value;
        }
        public static bool WriteVideoInfoJson
        {
            get => (bool?)Settings.Values[nameof(WriteVideoInfoJson)]
                   ?? false;

            set => Settings.Values[nameof(WriteVideoInfoJson)] = value;
        }

        // # Utility
        public static bool _IsFirstRun
        {
            get => (bool?)Settings.Values[nameof(_IsFirstRun)]
                   ?? true;
            set => Settings.Values[nameof(_IsFirstRun)] = value;
        }

        public async static void ImportSettingsFromFile(string? file = null)
        {
            var filePath = file;
            // If no file path is provided, open a file picker to select a JSON file
            if (string.IsNullOrEmpty(file))
            {
                if (App.Window == null) return;
                filePath = await App.PickFileAsync([".json"], App.Window);
            }

            if (!string.IsNullOrEmpty(filePath))
            {
                try
                {
                    var json = File.ReadAllText(filePath);
                    var settingsDict = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                    if (settingsDict != null)
                    {
                        // Remove any keys that start with '_' to avoid importing utility or internal settings
                        settingsDict = settingsDict.Where(kvp => !kvp.Key.StartsWith('_')).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                        ImportSettings(settingsDict);
                        LogService.Add($"{App.LocalizationService.Get("SettingsImportSuccess")}, {App.LocalizationService.GetString("SettingsImportSuccessMsg", settingsDict.Count)}: {filePath}", LogTag.App);
                        App.InfoBarService.Show(new InfoBarMessage
                        {
                            Title = App.LocalizationService.Get("SettingsImportSuccess"),
                            Message = $"{App.LocalizationService.GetString("SettingsImportSuccessMsg", settingsDict.Count)}, {App.LocalizationService.Get("SettingImportSuccessNotice")}",
                            Severity = InfoBarSeverity.Success,
                        });
                    }
                }
                catch (Exception ex)
                {
                    LogService.Add($"{App.LocalizationService.Get("SettingsImportFailed")}: {ex.Message}", LogTag.Error);
                    App.InfoBarService.Show(new InfoBarMessage
                    {
                        Title = App.LocalizationService.Get("SettingsImportFailed"),
                        Message = App.LocalizationService.Get("CheckLogs"),
                        Severity = InfoBarSeverity.Error,
                    });
                    Debug.WriteLine($"Error importing settings: {ex.Message}");
                }
            }
        }
        public static void ImportSettings(Dictionary<string, object> newSettings)
        {
            foreach (var kvp in newSettings)
            {
                var value = kvp.Value;

                if (value is JsonElement element)
                {
                    value = element.ValueKind switch
                    {
                        JsonValueKind.String => element.GetString(),
                        JsonValueKind.Number => element.GetInt32(),
                        JsonValueKind.True => true,
                        JsonValueKind.False => false,
                        _ => null
                    };
                }

                // If the value is null, skip it to avoid overwriting existing settings with null
                if (value == null) continue;

                // Handle specific settings that require additional actions when changed
                if (kvp.Key == nameof(AppLanguage)) LocalizationService.SetLanguage(Languages.First(l => l.Code == (string)value));
                else if (kvp.Key == nameof(AppTheme)) App.SettingsService.AppThemeChanged?.Invoke(Themes.First(t => t.Value == (string)value));
                else if (kvp.Key == nameof(AppBackdrop)) App.SettingsService.AppBackdropChanged?.Invoke(Backdrops.First(b => b.Value == (string)value));

                Settings.Values[kvp.Key] = value;
            }
        }
        public static void ExportSettings()
        {
            try
            {
                var settingsDict = Settings.Values.Where(kvp => !kvp.Key.StartsWith('_'))
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                var json = JsonSerializer.Serialize(settingsDict, new JsonSerializerOptions { WriteIndented = true });
                var exportPath = Path.Combine(LechKnownFolders.GetPath(LechKnownFolder.Documents), $"LechYTDLP_Settings.json");
                File.WriteAllText(exportPath, json);

                LogService.Add($"{App.LocalizationService.Get("SettingsExportSuccess")}: {exportPath}", LogTag.App);
                App.InfoBarService.Show(new InfoBarMessage
                {
                    Title = App.LocalizationService.Get("SettingsExportSuccess"),
                    Message = exportPath,
                    Severity = InfoBarSeverity.Success,
                });
            }
            catch (Exception ex)
            {
                LogService.Add($"{App.LocalizationService.Get("SettingsExportFailed")}: {ex.Message}", LogTag.Error);
                App.InfoBarService.Show(new InfoBarMessage
                {
                    Title = App.LocalizationService.Get("SettingsExportFailed"),
                    Message = App.LocalizationService.Get("CheckLogs"),
                    Severity = InfoBarSeverity.Error,
                });
            }
        }

        public static void ResetSetting(string name) => Settings.Values.Remove(name);
    }
}

using LechYTDLP.Util;
using LechYTDLP.Views;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using Windows.Storage;
using static LechYTDLP.Views.SettingsPage;

namespace LechYTDLP.Services
{
    public class SettingsService
    {

        public Action<Setting>? AppThemeChanged;
        public Action<Setting>? AppBackdropChanged;

        private static ApplicationDataContainer Settings =>
            ApplicationData.Current.LocalSettings;

        private static string BasePath = ApplicationData.Current.LocalFolder.Path;

        // # YT-DLP
        public static List<Setting> Presets =
        [
            new() { DisplayName = App.LocalizationService.Get("PresetsIllChoose"), Value = "illchoose" },
            new() { DisplayName = App.LocalizationService.Get("PresetsBestQuality"), Value = "bestquality" },
            new() { DisplayName = App.LocalizationService.Get("PresetsBestQualityVideo"), Value = "bestvideo" },
            new() { DisplayName = App.LocalizationService.Get("PresetsBestQualityAudio"), Value = "bestaudio" },
            new() { DisplayName = App.LocalizationService.Get("PresetsCompatible720pMP4"), Value = "compatible720pmp4" },
            new() { DisplayName = App.LocalizationService.Get("PresetsCompatible1080pMP4"), Value = "compatible1080pmp4" },
            new() { DisplayName = App.LocalizationService.Get("PresetsExtractAudioMP3"), Value = "extractaudiomp3" },
        ];
        public static Setting SelectedPreset
        {
            get
            {
                var value = Settings.Values[nameof(SelectedPreset)] as string ?? Presets[0].Value;
                return Presets.FirstOrDefault(p => p.Value == value) ?? Presets[0];
            }
            set
            {
                Settings.Values[nameof(SelectedPreset)] = value.Value;
            }
        }

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
        public static bool SaveLogOfEachDownload
        {
            get => (bool?)Settings.Values[nameof(SaveLogOfEachDownload)]
                   ?? true;
            set => Settings.Values[nameof(SaveLogOfEachDownload)] = value;
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
        // Behavior
        public static bool OpenFilesInExternalPlayer
        {
            get => (bool?)Settings.Values[nameof(OpenFilesInExternalPlayer)]
                   ?? true;
            set => Settings.Values[nameof(OpenFilesInExternalPlayer)] = value;
        }
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
        public static List<Setting> Languages = [
            new Setting { DisplayName = App.LocalizationService.Get("System"), Value = "system" },
            new Setting { DisplayName = "English", Value = "en-US" },
            new Setting { DisplayName = "Türkçe", Value = "tr-TR" },
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
        public static Setting AppLanguage
        {
            get
            {
                var value = Settings.Values[nameof(AppLanguage)] as string ?? "system";
                return Languages.FirstOrDefault(l => l.Value == value) ?? Languages[0];
            }
            set
            {
                Settings.Values[nameof(AppLanguage)] = value.Value;
            }
        }
        public static List<Setting> Themes { get; } = [
            new Setting { DisplayName = App.LocalizationService.Get("System"), Value = "system" },
            new Setting { DisplayName = App.LocalizationService.Get("Light"), Value = "light" },
            new Setting { DisplayName = App.LocalizationService.Get("Dark"), Value = "dark" },
        ];
        public static Setting AppTheme
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
        public static List<Setting> Backdrops { get; } = [
            new Setting { DisplayName = App.LocalizationService.Get("None"), Value = "none" },
            new Setting { DisplayName = "Mica", Value = "mica" },
            new Setting { DisplayName = App.LocalizationService.Get("Acrylic"), Value = "acrylic" },
            new Setting { DisplayName = "MicaAlt", Value = "micaalt" },
        ];
        public static Setting AppBackdrop
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

        // # Lech YT-DLP Settings
        // These are settings that don't fit in the above categories but are still important to persist, such as whether it's the first run, or update information
        // They are prefixed with '_' to avoid showing up in the settings export, and to indicate that they are more "internal" settings rather than user-facing ones

        // Update checking stuff
        public static bool ShowYTdlpUpdateNotifications
        {
            get => (bool?)Settings.Values[nameof(ShowYTdlpUpdateNotifications)]
                   ?? true;
            set => Settings.Values[nameof(ShowYTdlpUpdateNotifications)] = value;
        }
        public static bool CheckForUpdatesOnStartup
        {
            get => (bool?)Settings.Values[nameof(CheckForUpdatesOnStartup)]
                   ?? true;
            set => Settings.Values[nameof(CheckForUpdatesOnStartup)] = value;
        }
        public static bool YTdlpAutoUpdate
        {
            get => (bool?)Settings.Values[nameof(YTdlpAutoUpdate)]
                   ?? true;
            set => Settings.Values[nameof(YTdlpAutoUpdate)] = value;
        }

        public static string SelectedUpdateChannel
        {
            get => (string?)Settings.Values[nameof(SelectedUpdateChannel)]
                   ?? "stable";
            set => Settings.Values[nameof(SelectedUpdateChannel)] = value;
        }
        // These are used to cache the last known version and the last time we checked for updates, so that we don't have to hit the API every time and can still show update notifications based on the cached information
        public static string _LastKnownVersion
        {
            get
            {
                var value = (string?)Settings.Values[nameof(_LastKnownVersion)];

                if (string.IsNullOrEmpty(value))
                {
                    value = ReadYTdlpVersionInfo();
                    Settings.Values[nameof(_LastKnownVersion)] = value;
                }

                return value;
            }
            set => Settings.Values[nameof(_LastKnownVersion)] = value;
        }
        public static string _LastKnownYTdlpToolVersion
        {
            get
            {
                var value = (string?)Settings.Values[nameof(_LastKnownYTdlpToolVersion)];

                if (string.IsNullOrEmpty(value))
                {
                    value = ReadYTdlpVersionInfo();
                    Settings.Values[nameof(_LastKnownYTdlpToolVersion)] = value;
                }

                return value;
            }
            set => Settings.Values[nameof(_LastKnownYTdlpToolVersion)] = value;
        }
        public static long _LastUpdateCheckAt
        {
            get => (long?)Settings.Values[nameof(_LastUpdateCheckAt)]
                   ?? 0;
            set => Settings.Values[nameof(_LastUpdateCheckAt)] = value;
        }

        // Other
        public static bool _IsFirstRun
        {
            get => (bool?)Settings.Values[nameof(_IsFirstRun)]
                   ?? true;
            set => Settings.Values[nameof(_IsFirstRun)] = value;
        }

        // It's reads version info from the yt-dlp metadata
        public static string ReadYTdlpVersionInfo()
        {
            ToolPathService.Ensure(ToolPathService.Tool.YtDlp); // Ensure the tool is there before trying to read version info, to avoid errors and to make sure we have the most up-to-date version info if it was just updated or downloaded

            if (File.Exists(YTDLPPath))
            {
                try
                {
                    var versionInfo = FileVersionInfo.GetVersionInfo(YTDLPPath);
                    Debug.WriteLine($"Read yt-dlp version info: {versionInfo.FileVersion}");

                    if (!string.IsNullOrEmpty(versionInfo.FileVersion))
                    {
                        return versionInfo.FileVersion;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error reading yt-dlp version info: {ex.Message}");
                    LogService.Add($"Error reading yt-dlp version info: {ex.Message}", LogTag.Error);
                }
            }

            return "unknown";
        }

        [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
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
                    var settingsDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json, App.JsonSerializerOptions);
                    if (settingsDict != null)
                    {
                        // Remove any keys that start with '_' to avoid importing utility or internal settings
                        settingsDict = settingsDict.Where(kvp => !kvp.Key.StartsWith('_')).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                        ImportSettings(settingsDict);
                        LogService.Add($"{App.LocalizationService.Get("SettingsImportSuccess")}, {App.LocalizationService.Get("SettingsImportSuccessMsg", settingsDict.Count)}: {filePath}", LogTag.App);
                        App.InfoBarService.Show(new InfoBarMessage
                        {
                            Title = App.LocalizationService.Get("SettingsImportSuccess"),
                            Message = $"{App.LocalizationService.Get("SettingsImportSuccessMsg", settingsDict.Count)}, {App.LocalizationService.Get("SettingImportSuccessNotice")}",
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
        public static void ImportSettings(Dictionary<string, JsonElement> newSettings)
        {
            foreach (var kvp in newSettings)
            {
                var element = kvp.Value;

                object? value = element.ValueKind switch
                {
                    JsonValueKind.String => element.GetString(),

                    JsonValueKind.Number =>
                        element.TryGetInt32(out var i) ? i :
                        element.TryGetInt64(out var l) ? l :
                        element.TryGetDouble(out var d) ? d :
                        null,

                    JsonValueKind.True => true,
                    JsonValueKind.False => false,

                    JsonValueKind.Null => null,

                    _ => null
                };

                if (value == null) continue;

                if (value is string strValue)
                {
                    if (kvp.Key == nameof(AppLanguage))
                    {
                        var lang = Languages.FirstOrDefault(l => l.Value == strValue);
                        if (lang != null)
                            LocalizationService.SetLanguage(lang);
                    }
                    else if (kvp.Key == nameof(AppTheme))
                    {
                        var theme = Themes.FirstOrDefault(t => t.Value == strValue);
                        if (theme != null)
                            App.SettingsService.AppThemeChanged?.Invoke(theme);
                    }
                    else if (kvp.Key == nameof(AppBackdrop))
                    {
                        var backdrop = Backdrops.FirstOrDefault(b => b.Value == strValue);
                        if (backdrop != null)
                            App.SettingsService.AppBackdropChanged?.Invoke(backdrop);
                    }
                }

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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using LechYTDLP.Util;
using System.IO;
using static LechYTDLP.Views.SettingsPage;

namespace LechYTDLP.Services
{
    public class SettingsService
    {

        public Action<ThemeItem>? AppThemeChanged;
        public Action<ThemeItem>? AppBackdropChanged;

        private static ApplicationDataContainer Settings =>
            ApplicationData.Current.LocalSettings;

        private static string BasePath = Windows.ApplicationModel.Package.Current.InstalledLocation.Path;

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
                   ?? Path.Combine(BasePath, "Tools", "ytdlp.exe");

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
        // Customization
        public static LanguageItem AppLanguage
        {
            get
            {
                var value = Settings.Values[nameof(AppLanguage)] as string ?? "en-US";
                return value switch
                {
                    "tr-TR" => new LanguageItem { DisplayName = "Türkçe", Code = "tr-TR" },
                    _ => new LanguageItem { DisplayName = "English", Code = "en-US" }
                };
            }
            set
            {
                Settings.Values[nameof(AppLanguage)] = value.Code;
            }
        }
        public static ThemeItem AppTheme
        {
            get
            {
                var value = Settings.Values[nameof(AppTheme)] as string ?? "system";

                return value switch
                {
                    "light" => new ThemeItem { DisplayName = "Light", Value = "light" },
                    "dark" => new ThemeItem { DisplayName = "Dark", Value = "dark" },
                    _ => new ThemeItem { DisplayName = "System", Value = "system" }
                };
            }
            set
            {
                Settings.Values[nameof(AppTheme)] = value.Value;
            }
        }
        public static ThemeItem AppBackdrop
        {
            get
            {
                var value = Settings.Values[nameof(AppBackdrop)] as string ?? "micaalt";
                return value switch
                {
                    "none" => new ThemeItem { DisplayName = "None", Value = "none" },
                    "mica" => new ThemeItem { DisplayName = "Mica", Value = "mica" },
                    "micaalt" => new ThemeItem { DisplayName = "MicaAlt", Value = "micaalt" },
                    "acrylic" => new ThemeItem { DisplayName = "Acrylic", Value = "acrylic" },
                    _ => new ThemeItem { DisplayName = "MicaAlt", Value = "micaalt" }
                };
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

        public static void ResetSetting(string name) => Settings.Values.Remove(name);
    }
}

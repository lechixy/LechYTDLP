using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using LechYTDLP.Util;

namespace LechYTDLP.Services
{
    internal class SettingsService
    {
        private static ApplicationDataContainer Settings =>
            ApplicationData.Current.LocalSettings;

        // # Options
        // File
        public static string FilenameTemplate
        {
            get => (string?)Settings.Values[nameof(FilenameTemplate)]
                   ?? "%(channel)s_%(id)s.%(ext)s";

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
                   ?? "ytdlp";

            set => Settings.Values[nameof(YTDLPPath)] = value;
        }
        public static string FFmpegPath
        {
            get => (string?)Settings.Values[nameof(FFmpegPath)]
                   ?? "ffmpeg";

            set => Settings.Values[nameof(FFmpegPath)] = value;
        }
        public static string JavaScriptRuntime
        {
            get => (string?)Settings.Values[nameof(JavaScriptRuntime)]
                ?? "";
            set => Settings.Values[nameof(JavaScriptRuntime)] = value;
        }
        // Developer options
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

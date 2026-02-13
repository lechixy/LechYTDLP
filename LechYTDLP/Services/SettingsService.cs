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

        // Options
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

        // Settings management
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace LechYTDLP.Services
{
    public class ToolPathService
    {
        private const string YtDlpFileName = "yt-dlp.exe";
        private const string FfmpegFileName = "ffmpeg.exe";

        public static string ToolsDirectory => Path.Combine(ApplicationData.Current.LocalFolder.Path, "Tools");

        public static string YtDlpPath => Path.Combine(ToolsDirectory, YtDlpFileName);
        public static string FFmpegPath => Path.Combine(ToolsDirectory, FfmpegFileName);

        public static string GetYtDlpPathFromSettings()
        {
            if (!string.IsNullOrEmpty(SettingsService.YTDLPPath))
                return SettingsService.YTDLPPath as string;

            return YtDlpPath;
        }

        public static string GetFfmpegPathFromSettings()
        {
            if (!string.IsNullOrEmpty(SettingsService.FFmpegPath))
                return SettingsService.FFmpegPath as string;
            return FFmpegPath;
        }

        public static void EnsureToolsDirectory()
        {
            Directory.CreateDirectory(ToolsDirectory);
        }

        public enum Tool
        {
            YtDlp,
            FFmpeg
        }

        public static void Ensure(Tool tool)
        {
            EnsureToolsDirectory();

            var targetPath = tool switch
            {
                Tool.YtDlp => YtDlpPath,
                Tool.FFmpeg => FFmpegPath,
                _ => throw new ArgumentException("Unsupported tool.")
            };

            if (File.Exists(targetPath))
                return;

            var fileName = tool switch
            {
                Tool.YtDlp => YtDlpFileName,
                Tool.FFmpeg => FfmpegFileName,
                _ => throw new ArgumentException("Unsupported tool.")
            };

            var packagedPath = Path.Combine(AppContext.BaseDirectory, "Tools", fileName);

            if (!File.Exists(packagedPath))
            {
                LogService.Add($"Embedded {tool} not found at {packagedPath}", LogTag.Error);
                throw new FileNotFoundException($"Embedded {tool} not found in /Tools folder.");
            }

            var tempPath = targetPath + ".new";

            // Atomic copy
            File.Copy(packagedPath, tempPath, overwrite: true);

            if (File.Exists(targetPath))
                File.Delete(targetPath);

            File.Move(tempPath, targetPath);
        }
    }
}

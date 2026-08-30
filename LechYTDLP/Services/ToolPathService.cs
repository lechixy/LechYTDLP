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
        private const string GalleryDLFileName = "gallery-dl.exe";

        public static string ToolsDirectory => Path.Combine(ApplicationData.Current.LocalFolder.Path, "Tools");

        public static string YtDlpPath => Path.Combine(ToolsDirectory, YtDlpFileName);
        public static string FFmpegPath => Path.Combine(ToolsDirectory, FfmpegFileName);
        public static string GalleryDLPath => Path.Combine(ToolsDirectory, GalleryDLFileName);

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

        public static string GetGalleryDLPathFromSettings()
        {
            if (!string.IsNullOrEmpty(SettingsService.GalleryDLPath))
                return SettingsService.GalleryDLPath as string;
            return GalleryDLPath;
        }

        public static void EnsureToolsDirectory()
        {
            Directory.CreateDirectory(ToolsDirectory);
        }

        public enum Tool
        {
            YtDlp,
            FFmpeg,
            GalleryDL,
        }

        private static readonly object _ensureLock = new();

        public static bool Ensure(Tool tool)
        {
            lock (_ensureLock)
            {
                EnsureToolsDirectory();

                var (targetPath, fileName) = tool switch
                {
                    Tool.YtDlp => (YtDlpPath, YtDlpFileName),
                    Tool.FFmpeg => (FFmpegPath, FfmpegFileName),
                    Tool.GalleryDL => (GalleryDLPath, GalleryDLFileName),
                    _ => throw new ArgumentException("Unsupported tool.")
                };

                if (File.Exists(targetPath))
                    return true;

                var packagedPath = Path.Combine(
                    AppContext.BaseDirectory,
                    "Tools",
                    fileName);

                if (!File.Exists(packagedPath))
                {
                    LogService.Add(
                        $"Embedded {tool} not found at {packagedPath}",
                        LogTag.Error);

                    throw new FileNotFoundException(
                        $"Embedded {tool} not found in /Tools folder.");
                }

                var tempPath =
                    targetPath + "." +
                    Guid.NewGuid().ToString("N") +
                    ".new";

                try
                {
                    File.Copy(packagedPath, tempPath);

                    File.Move(
                        tempPath,
                        targetPath,
                        overwrite: true);

                    return true;
                }
                finally
                {
                    // If something failed before Move completed,
                    // don't leave the temporary file behind.
                    try
                    {
                        if (File.Exists(tempPath))
                            File.Delete(tempPath);
                    }
                    catch
                    {
                        // Best effort cleanup.
                    }
                }
            }
        }
    }
}

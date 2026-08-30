using LechYTDLP.Services;
using LechYTDLP.Util;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace LechYTDLP.Classes
{
    /// <summary>
    /// Represents a class for handling gallery downloads.
    /// </summary>
    public class GalleryDL : ProcessBase
    {
        private async Task<ProcessResult> ExecuteGalleryDlAsync(
            DlArgs args,
            Action<string>? onOutput = null,
            Action<string>? onError = null,
            CancellationToken cancellationToken = default
        )
        {
            // These args must be included in every gallery-dl process, so we add them by default. User-provided args will be added on top of these.
            var mustHaveArgs = new DlArgs
            {
                Type = DlArgsType.GalleryDl,
                CookiesPath = SettingsService.CookiesfilePath,
                //Verbose = SettingsService.UseVerboseLoggingOnYTDLP,
                NoColor = true,
                NoMTime = true,
                //CustomYtDlpParams = SettingsService.CustomYtDlpParams,
                //ConcurrentFragments = SettingsService.ConcurrentFragments,
            }.BuildArgs();

            var ytdlpArgs = args.BuildArgs();
            // If there is update arg we don't add mustHaveArgs because update process may not work properly with some of those args, and also update process doesn't require those args to work.
            // NEEDS REFACTOR: This is a bit of a hacky solution, we should find a better way to handle this in the future.
            string arguments = args.Update ? ytdlpArgs : $"{ytdlpArgs} {mustHaveArgs}";

            LogService.Add($"🚩 {App.LocalizationService.Get("StartingToolWithLog", App.LocalizationService.Get("GalleryDl"))}:", LogTag.YTDLP);
            LogService.Add($"{SettingsService.GalleryDLPath} {arguments}", LogTag.Normal);

            // Loglamaları sarmalıyoruz ki her çıktı doğrudan LogService'e gitsin.
            Action<string> wrappedOnOutput = data =>
            {
                if (args.DumpSingleJson != null)
                    LogService.Add($"ℹ️ {App.LocalizationService.Get("GettingVideoInfoLog")}...", LogTag.Warning);
                else if (data.StartsWith("P|"))
                    LogService.AddOrUpdate(LogKey.Download, data);
                else
                    LogService.Add(data);

                onOutput?.Invoke(data);
            };

            Action<string> wrappedOnError = data =>
            {
                if (data.StartsWith("[debug]")) return;

                Debug.WriteLine("This error received from gallery-dl process:\n" + data);
                _ = KnownErrors.Check(new Exception(data)); // Async void yapmamak için discard ediyoruz (Fire & Forget)

                onError?.Invoke(data);
            };

            // Base classtaki process çalıştırıcıyı çağır!
            var result = await RunProcessAsync($"\"{SettingsService.GalleryDLPath}\"", arguments, wrappedOnOutput, wrappedOnError, cancellationToken);

            LogService.Add($"🏁 {App.LocalizationService.Get("ToolProcessExitedLog", App.LocalizationService.Get("GalleryDl"))}", LogTag.YTDLP);

            if (result.Code != ResultCode.Success && result.Code != ResultCode.Cancelled)
                LogService.Add($"⤷ {App.LocalizationService.Get("ToolProcessNonZeroLog", App.LocalizationService.Get("GalleryDl"))}", LogTag.Error);

            return result;
        }

        public async Task<VideoInfoResult> GetVideoInfoAsync(string url, CancellationToken cancellationToken = default)
        {
            LogService.Add($"⏳ {App.LocalizationService.Get("GettingVideoInfoLog")}: {url}", LogTag.YTDLP);

            //// If using blob data, read from local file instead
            //if (SettingsService.IsUsingBlobData)
            //{
            //    LogService.Add($"🧪 {App.LocalizationService.Get("ReadingVideoInfoFromBlobDataLog")}...", LogTag.YTDLP);
            //    App.InfoBarService.Show(new InfoBarMessage
            //    {
            //        Title = App.LocalizationService.Get("ReadingVideoInfoFromBlobDataLog"),
            //        Message = "",
            //        Severity = InfoBarSeverity.Informational,
            //    });

            //    string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "lechytdlp_blob.json");
            //    string readContents = await File.ReadAllTextAsync(path, Encoding.UTF8, cancellationToken);
            //    var cachedInfo = JsonSerializer.Deserialize<YtDlpData>(readContents, AppJsonContext.Default.YtDlpData);

            //    return new VideoInfoResult
            //    {
            //        Code = cachedInfo != null ? ResultCode.Success : ResultCode.Error,
            //        Message = cachedInfo != null ? "Success" : "Failed",
            //        YtDlpData = cachedInfo
            //    };
            //}

            var args = new DlArgs
            {
                Type = DlArgsType.GalleryDl,
                Url = url,
                DumpSingleJson = true,
                OutputPath = $"{SettingsService.DownloadPath}\\{SettingsService.FilenameTemplate}"
            };

            YtDlpData? videoInfo = null;

            var processResult = await ExecuteGalleryDlAsync(args, onOutput: data =>
            {
                try
                {
                    videoInfo = JsonSerializer.Deserialize<YtDlpData>(data, AppJsonContext.Default.YtDlpData);
                }
                catch (JsonException) { /* json parse error ignore for other lines */ }
            }, cancellationToken: cancellationToken);

            if (processResult.Code == ResultCode.Success && videoInfo != null)
            {
                return new VideoInfoResult { Code = ResultCode.Success, Message = "Success", VideoInfo = videoInfo };
            }

            return new VideoInfoResult { Code = ResultCode.Error, Message = "Failed to retrieve video info" };
        }

        //public async Task<int> DownloadVideo(DlArgs args, YtDlpData info, Action<string?> onOutput = null, Action<string?> onError = null, CancellationToken cancellationToken = default)
        //{
        //    LogService.Add($"⬇️ {App.LocalizationService.Get("DownloadingVideoLog")}: {args.Url}", LogTag.YTDLP);

        //    List<string> logs = new();

        //    Action<string> handleOutput = data =>
        //    {
        //        //HandleOutput(data);
        //        if (SettingsService.SaveLogOfEachDownload && !data.StartsWith("P|")) logs.Add(data);
        //        onOutput?.Invoke(data);
        //    };

        //    Action<string> handleError = data =>
        //    {
        //        //HandleOutput(data);
        //        if (SettingsService.SaveLogOfEachDownload && !data.StartsWith("P|")) logs.Add(data);
        //        onError?.Invoke(data);
        //    };

        //    var processResult = await ExecuteGalleryDlAsync(
        //        args,
        //        onOutput: handleOutput,
        //        onError: handleError,
        //        cancellationToken: cancellationToken
        //    );

        //    // Save log of each download if setting is enabled, save to Documents/LechYTDLP/Logs folder with file name {video_id}.log
        //    if (SettingsService.SaveLogOfEachDownload && logs.Any())
        //    {
        //        string logPath = Path.Combine(LechKnownFolders.GetLogsPath(), $"{info.Id}.log");
        //        await File.WriteAllLinesAsync(logPath, logs, Encoding.UTF8, cancellationToken);
        //    }

        //    // We need info about the video to set filepath etc.
        //    // Dosya yolu bulma lojiği (Paralel süreçler için ileride App.DownloadService.CurrentMedia yerine Media referansını metoda parametre olarak vermeni öneririm)
        //    string infoJsonPath = Path.Combine(LechKnownFolders.GetPath(LechKnownFolder.Documents), $"LechYTDLP\\Logs\\{info.Id}.info.json");

        //    if (File.Exists(infoJsonPath))
        //    {
        //        if (info.Type == InfoType.Video)
        //        {
        //            try
        //            {
        //                string json = await File.ReadAllTextAsync(infoJsonPath, Encoding.UTF8, cancellationToken);
        //                var videoInfo = JsonSerializer.Deserialize<YtDlpData>(json, AppJsonContext.Default.YtDlpData);
        //                if (videoInfo != null && videoInfo.Filename != null && App.DownloadService.CurrentMedia != null)
        //                {
        //                    App.DownloadService.CurrentMedia.FilePath = videoInfo.Filename;
        //                }
        //            }
        //            catch (JsonException ex) { LogService.Add("Error parsing video info JSON: " + ex.Message, LogTag.Error); }
        //        }
        //        else if (info.Type == InfoType.Playlist && App.DownloadService.CurrentMedia != null)
        //        {
        //            // We handle playlist info differently, because yt-dlp writes every video info to single line json, so we need to read all lines and deserialize them into a list of YtDlpData objects.
        //            // We read all lines from the info json file, and deserialize each line into a YtDlpData object, and add them to a list.

        //            // Solution: We show user to downloaded files folder
        //            App.DownloadService.CurrentMedia.FilePath = SettingsService.DownloadPath;
        //        }
        //    }

        //    // If there is no filepath set, we can try to find the file in the download folder
        //    if (App.DownloadService.CurrentMedia != null && !File.Exists(App.DownloadService.CurrentMedia.FilePath))
        //    {
        //        // Try to find the file in the download folder
        //        var files = Directory.GetFiles(SettingsService.DownloadPath, $"{App.DownloadService.CurrentMedia.Id}.*");
        //        // We sort list by last write time to get the most recent file
        //        if (files.Length > 0)
        //        {
        //            Array.Sort(files, (x, y) => File.GetLastWriteTime(y).CompareTo(File.GetLastWriteTime(x)));
        //            App.DownloadService.CurrentMedia.FilePath = files[0];
        //        }
        //    }

        //    return processResult.Code == ResultCode.Success ? 0 : 1;
        //}
    }
}

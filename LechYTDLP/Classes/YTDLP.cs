using LechYTDLP.Components;
using LechYTDLP.Services;
using LechYTDLP.Util;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LechYTDLP.Classes
{
    public enum DlArgsType
    {
        YTDLP,
        GalleryDl
    }

    /// <summary>
    /// Arguments for YT-DLP and Gallery DL process
    /// </summary>
    public sealed class DlArgs
    {
        // Required for most operations
        public string? Url { get; set; } = null;
        public SelectedFormat? SelectedFormat { get; set; }
        public DlArgsType Type { get; set; } = DlArgsType.YTDLP;

        // Output
        public bool? DumpJson { get; set; }
        public bool? DumpSingleJson { get; set; }
        public string? PrintToFile { get; set; }

        // File
        public string? OutputPath { get; set; }
        public string? FFmpegLocation { get; set; }
        public bool? ForceOverwrites { get; set; }

        // Downloads
        public string? PlaylistItems { get; set; }
        public int? ConcurrentFragments { get; set; }
        public bool? SkipDownload { get; set; }

        // Processing
        public bool? FlatPlaylist { get; set; }

        // Account
        public string? CookiesPath { get; set; }

        // Options
        public bool EmbedMetadata { get; set; } = false;
        public bool EmbedThumbnail { get; set; } = false;
        public bool EmbedSubs { get; set; } = false;
        public bool? NoMTime { get; set; }

        // YT-DLP
        public bool Update { get; set; } = false;
        public string? JavaScriptRuntime { get; set; }

        // Debug, logging etc.
        public bool Verbose { get; set; } = false;
        public bool NoColor { get; set; } = false;
        public bool Newline { get; set; } = false;
        public string? ProgressTemplate { get; set; } = null;

        // More
        public string? CustomYtDlpParams { get; set; }

        public string BuildArgs()
        {
            var args = new List<string>();

            if (Url != null)
            {
                args.Add($"\"{Url}\"");
            }

            if (SelectedFormat != null)
            {
                // If a preset is selected, we assume it already contains the necessary format selection arguments, so we don't add any format-specific arguments here.
                if (SelectedFormat.Preset != null)
                {
                    switch (SelectedFormat.Preset.Value)
                    {
                        case "bestquality":
                            args.Add("-f bestvideo+bestaudio/best");
                            break;

                        case "bestvideo":
                            args.Add("-f bestvideo");
                            break;

                        case "bestaudio":
                            args.Add("-f bestaudio");
                            break;

                        case "letytdlpdecide":
                            // Let yt-dlp decide the best format based on its internal logic
                            break;

                        case "compatible1080pmp4":
                            args.Add("-f bestvideo[height<=1080][ext=mp4]+bestaudio[ext=m4a]/best[height<=1080][ext=mp4]/best");
                            args.Add("--merge-output-format mp4");
                            break;

                        case "compatible720pmp4":
                            args.Add("-f bestvideo[height<=720][ext=mp4]+bestaudio[ext=m4a]/best[height<=720][ext=mp4]/best");
                            args.Add("--merge-output-format mp4");
                            break;

                        case "extractaudiomp3":
                            args.Add("-x --audio-format mp3");
                            break;

                        case "2160p":
                            args.Add("-f bestvideo[height<=2160]+bestaudio/best[height<=2160]");
                            break;

                        case "1440p":
                            args.Add("-f bestvideo[height<=1440]+bestaudio/best[height<=1440]");
                            break;

                        case "1080p":
                            args.Add("-f bestvideo[height<=1080]+bestaudio/best[height<=1080]");
                            break;

                        case "720p":
                            args.Add("-f bestvideo[height<=720]+bestaudio/best[height<=720]");
                            break;

                        case "480p":
                            args.Add("-f bestvideo[height<=480]+bestaudio/best[height<=480]");
                            break;

                        case "360p":
                            args.Add("-f bestvideo[height<=360]+bestaudio/best[height<=360]");
                            break;
                    }
                }
                // If no preset is selected, we build the format selection arguments based on the selected video and audio formats.
                else
                {
                    if (SelectedFormat.VideoId != null && SelectedFormat.AudioId != null)
                    {
                        args.Add($"-f \"{SelectedFormat.VideoId}+{SelectedFormat.AudioId}\"");
                    }
                    else if (SelectedFormat.VideoId != null)
                    {
                        args.Add($"-f \"{SelectedFormat.VideoId}\"");
                    }
                    else if (SelectedFormat.AudioId != null)
                    {
                        args.Add($"-f \"{SelectedFormat.AudioId}\"");
                    }
                }
            }

            if (DumpJson != null)
                args.Add("--dump-json");

            if (DumpSingleJson != null)
                if (Type == DlArgsType.YTDLP)
                    args.Add("--dump-single-json");
                else if (Type == DlArgsType.GalleryDl)
                    args.Add("--resolve-json");

            if (!string.IsNullOrEmpty(FFmpegLocation))
                args.Add($"--ffmpeg-location \"{FFmpegLocation}\"");

            if (!string.IsNullOrEmpty(CookiesPath))
                args.Add($"--cookies \"{CookiesPath}\"");
            if (!string.IsNullOrEmpty(JavaScriptRuntime))
                args.Add($"--js-runtimes {JavaScriptRuntime}");

            if (OutputPath != null)
                if (Type == DlArgsType.YTDLP)
                    args.Add($"-o \"{OutputPath}\"");
                else if (Type == DlArgsType.GalleryDl)
                    args.Add($"-d \"{OutputPath}\"");

            if (EmbedMetadata)
                args.Add("--embed-metadata");

            if (EmbedThumbnail)
                args.Add("--embed-thumbnail");

            if (EmbedSubs)
                args.Add("--embed-subs");

            if (Update)
                args.Add("-U");

            if (Verbose)
                args.Add("--verbose");

            if (NoColor)
                if (Type == DlArgsType.YTDLP)
                    args.Add("--no-color");
                else if (Type == DlArgsType.GalleryDl)
                    args.Add("--no-colors");

            if (Newline)
                args.Add("--newline");

            if (ProgressTemplate != null)
                args.Add($"--progress-template \"{ProgressTemplate}\"");

            if (PrintToFile != null)
                args.Add($"--print-to-file {PrintToFile}");

            if (NoMTime != null)
                args.Add("--no-mtime");

            if (CustomYtDlpParams != null)
            {
                args.Add(SettingsService.CustomYtDlpParams);
            }

            if (ConcurrentFragments != null)
            {
                args.Add($"--concurrent-fragments {ConcurrentFragments}");
            }

            if (ForceOverwrites != null)
            {
                args.Add("--force-overwrites");
            }

            if (SkipDownload != null)
            {
                args.Add("--skip-download");
            }

            if (FlatPlaylist != null)
            {
                args.Add("--flat-playlist");
            }

            if (PlaylistItems != null)
            {
                args.Add($"--playlist-items {PlaylistItems}");
            }

            return string.Join(" ", args);
        }
    }

    /// <summary>
    /// Sadece yt-dlp'ye özel işlemleri barındıran sınıf.
    /// </summary>
    public class YTDLP : ProcessBase
    {
        private async Task<ProcessResult> ExecuteYtDlpAsync(
            DlArgs args,
            Action<string>? onOutput = null,
            Action<string>? onError = null,
            CancellationToken cancellationToken = default)
        {
            // These args must be included in every yt-dlp process, so we add them by default. User-provided args will be added on top of these.
            var mustHaveArgs = new DlArgs
            {
                Type = DlArgsType.YTDLP,
                CookiesPath = SettingsService.CookiesfilePath,
                JavaScriptRuntime = string.IsNullOrEmpty(SettingsService.JavaScriptRuntime) ? "" : SettingsService.JavaScriptRuntime,
                Verbose = SettingsService.UseVerboseLoggingOnYTDLP,
                NoColor = true,
                Newline = true,
                NoMTime = true,
                CustomYtDlpParams = SettingsService.CustomYtDlpParams,
                ConcurrentFragments = SettingsService.ConcurrentFragments,
                ForceOverwrites = SettingsService.ForceOverwrites
            }.BuildArgs();

            var ytdlpArgs = args.BuildArgs();
            // If there is update arg we don't add mustHaveArgs because update process may not work properly with some of those args, and also update process doesn't require those args to work.
            // NEEDS REFACTOR: This is a bit of a hacky solution, we should find a better way to handle this in the future.
            string arguments = args.Update ? ytdlpArgs : $"{ytdlpArgs} {mustHaveArgs}";

            LogService.Add($"🚩 {App.LocalizationService.Get("StartingToolWithLog", App.LocalizationService.Get("YtDlp"))}:", LogTag.YTDLP);
            LogService.Add($"{SettingsService.YTDLPPath} {arguments}", LogTag.Normal);

            // Loglamaları sarmalıyoruz ki her çıktı doğrudan LogService'e gitsin.
            Action<string> wrappedOnOutput = data =>
            {
                if (string.IsNullOrEmpty(data)) return;

                if (args.DumpSingleJson != null)
                    LogService.Add($"ℹ️ {App.LocalizationService.Get("GettingVideoInfoLog")}...", LogTag.Warning);
                else if (data.StartsWith("P|"))
                    LogService.AddOrUpdate(LogKey.Download, data);
                else
                    LogService.Add(data);

                onOutput?.Invoke(data);
            };

            bool NoVideoFound = false;

            Action<string> wrappedOnError = data =>
            {
                if (string.IsNullOrEmpty(data)) return;
                if (data.StartsWith("[debug]")) return;

                if (data.Contains("No video formats found!"))
                {
                    NoVideoFound = true;
                }

                Debug.WriteLine("This error received from yt-dlp process:\n" + data);
                _ = KnownErrors.Check(new Exception(data));

                onError?.Invoke(data);
            };

            // Base classtaki process çalıştırıcıyı çağır
            var result = await RunProcessAsync($"\"{SettingsService.YTDLPPath}\"", arguments, wrappedOnOutput, wrappedOnError, cancellationToken);

            LogService.Add($"🏁 {App.LocalizationService.Get("ToolProcessExitedLog", App.LocalizationService.Get("YtDlp"))}", LogTag.YTDLP);

            if (result.Code != ResultCode.Success && result.Code != ResultCode.Cancelled)
                LogService.Add($"⤷ {App.LocalizationService.Get("ToolProcessNonZeroLog", App.LocalizationService.Get("YtDlp"))}", LogTag.Error);

            if (NoVideoFound)
            {
                result.Reason = ResultReason.NoVideoFound;
            }

            return result;
        }

        public async Task<VideoInfoResult> GetVideoInfoAsync(string url, CancellationToken cancellationToken = default)
        {
            LogService.Add($"⏳ {App.LocalizationService.Get("GettingVideoInfoLog")}: {url}", LogTag.YTDLP);

            // If using blob data, read from local file instead
            if (SettingsService.IsUsingBlobData)
            {
                LogService.Add($"🧪 {App.LocalizationService.Get("ReadingVideoInfoFromBlobDataLog")}...", LogTag.YTDLP);
                App.InfoBarService.Show(new InfoBarMessage
                {
                    Title = App.LocalizationService.Get("ReadingVideoInfoFromBlobDataLog"),
                    Message = "",
                    Severity = InfoBarSeverity.Informational,
                });

                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "lechytdlp_blob.json");
                string readContents = await File.ReadAllTextAsync(path, Encoding.UTF8, cancellationToken);
                var cachedInfo = JsonSerializer.Deserialize<YtDlpData>(readContents, AppJsonContext.Default.YtDlpData);

                return new VideoInfoResult
                {
                    Code = cachedInfo != null ? ResultCode.Success : ResultCode.Error,
                    Message = cachedInfo != null ? "Success" : "Failed",
                    VideoInfo = cachedInfo
                };
            }

            var args = new DlArgs
            {
                Type = DlArgsType.YTDLP,
                Url = url,
                DumpSingleJson = true,
                FlatPlaylist = true,
                OutputPath = $"{SettingsService.DownloadPath}\\{SettingsService.FilenameTemplate}"
            };

            YtDlpData? videoInfo = null;

            var processResult = await ExecuteYtDlpAsync(args, onOutput: data =>
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

            return new VideoInfoResult { Code = ResultCode.Error, Reason = processResult.Reason, Message = "Failed to retrieve video info" };
        }

        public async Task<DownloadResult> DownloadVideo(DlArgs args, DownloadItem item, Action<string?> onOutput = null, Action<string?> onError = null, CancellationToken cancellationToken = default)
        {
            LogService.Add($"⬇️ {App.LocalizationService.Get("DownloadingVideoLog")}: {args.Url}", LogTag.YTDLP);

            var info = item.Info;
            List<string> logs = new();

            Action<string> handleOutput = data =>
            {
                HandleOutput(data);
                if (SettingsService.SaveLogOfEachDownload && !data.StartsWith("P|")) logs.Add(data);
                onOutput?.Invoke(data);
            };

            Action<string> handleError = data =>
            {
                HandleOutput(data);
                if (SettingsService.SaveLogOfEachDownload && !data.StartsWith("P|")) logs.Add(data);
                onError?.Invoke(data);
            };

            var processResult = await ExecuteYtDlpAsync(
                args,
                onOutput: handleOutput,
                onError: handleError,
                cancellationToken: cancellationToken
            );
            var result = new DownloadResult { Code = processResult.Code, Reason = processResult.Reason, Message = processResult.Message };

            // Pause nedeniyle process iptal edildiyse, artık cancellation token ile başka async işlem yapma.
            if (item != null && item.WantedToPause && processResult.Code == ResultCode.Cancelled)
            {
                result.Reason = ResultReason.WantedPause;
                return result;
            }
            // Cancel nedeniyle process iptal edildiyse, artık cancellation token ile başka async işlem yapma.
            else if (item != null && item.WantedToCancel && processResult.Code == ResultCode.Cancelled)
            {
                result.Reason = ResultReason.WantedCancel;
                return result;
            }

            // Save log of each download if setting is enabled, save to Documents/LechYTDLP/Logs folder with file name {video_id}.log
            if (SettingsService.SaveLogOfEachDownload && logs.Any() && cancellationToken.IsCancellationRequested)
            {
                string logPath = Path.Combine(LechKnownFolders.GetLogsPath(), $"{info.Id}.log");
                await File.WriteAllLinesAsync(logPath, logs, Encoding.UTF8, cancellationToken);
            }

            // We need info about the video to set filepath etc.
            // Dosya yolu bulma lojiği (Paralel süreçler için ileride App.DownloadService.CurrentMedia yerine Media referansını metoda parametre olarak vermeni öneririm)
            string infoJsonPath = Path.Combine(LechKnownFolders.GetPath(LechKnownFolder.Documents), $"LechYTDLP\\Logs\\{info.Id}.info.json");

            if (File.Exists(infoJsonPath))
            {
                if (info.Type == InfoType.Video)
                {
                    try
                    {
                        string json = await File.ReadAllTextAsync(infoJsonPath, Encoding.UTF8, cancellationToken);
                        var videoInfo = JsonSerializer.Deserialize(json, AppJsonContext.Default.YtDlpData);
                        if (item != null && videoInfo != null && videoInfo.Filename != null)
                        {
                            item.FilePath = videoInfo.Filename;
                        }
                    }
                    catch (JsonException ex) { LogService.Add("Error parsing video info JSON: " + ex.Message, LogTag.Error); }
                }
                else if (info.Type == InfoType.Playlist && item != null)
                {
                    // We handle playlist info differently, because yt-dlp writes every video info to single line json, so we need to read all lines and deserialize them into a list of YtDlpData objects.
                    // We read all lines from the info json file, and deserialize each line into a YtDlpData object, and add them to a list.

                    // Solution: We show user to downloaded files folder
                    item.FilePath = SettingsService.DownloadPath;
                }
            }

            // If there is no filepath set, we can try to find the file in the download folder
            if (item != null && !File.Exists(item.FilePath))
            {
                if (Directory.Exists(SettingsService.DownloadPath))
                {
                    var files = Directory.GetFiles(
                        SettingsService.DownloadPath,
                        $"{item.Id}.*");

                    // We sort list by last write time to get the most recent file
                    if (files.Length > 0)
                    {
                        Array.Sort(
                            files,
                            (x, y) => File.GetLastWriteTime(y)
                                .CompareTo(File.GetLastWriteTime(x)));

                        item.FilePath = files[0];
                    }
                }
            }

            return result;
        }

        public async Task<UpdateResult> CheckAndDownloadUpdate(CancellationToken cancellationToken = default)
        {
            LogService.Add($"🔍 {App.LocalizationService.Get("YTdlpCheckingForUpdates")}...", LogTag.YTDLP);
            //App.DownloadController.SetBusy(true, $"{App.LocalizationService.Get("YTdlpUpdating")}...");

            var args = new DlArgs { Type = DlArgsType.YTDLP, Update = true };
            UpdateResult finalResult = new() { Status = UpdateStatus.Failed };

            var processResult = await ExecuteYtDlpAsync(args, onOutput: data =>
            {
                if (data.Contains("yt-dlp is up to date"))
                {
                    string newVersion = data.Split('@')[1].Split(' ')[0].Trim();
                    SettingsService._LastKnownYTdlpToolVersion = newVersion;
                    finalResult = new UpdateResult { Status = UpdateStatus.UpToDate, Message = data.Split('(')[1].Split(')')[0] };
                }
                else if (data.Contains("Updated yt-dlp to"))
                {
                    string newVersion = data.Split('@')[1].Split(' ')[0].Trim();
                    SettingsService._LastKnownYTdlpToolVersion = newVersion;
                    finalResult = new UpdateResult { Status = UpdateStatus.Updated, Message = data.Split("to ")[1].Trim() };
                }
            }, cancellationToken: cancellationToken);

            //App.DownloadController.SetBusy(false, "");
            return finalResult;
        }

        private void HandleOutput(string output)
        {
            if (string.IsNullOrWhiteSpace(output)) return;
        }

        public enum CheckExecutableApp { YTDLP, FFMPEG, GALLERYDL }

        public static Task<string> CheckExecutable(CheckExecutableApp executable)
        {
            return executable switch
            {
                CheckExecutableApp.YTDLP => CheckExecutableAsync(SettingsService.YTDLPPath, "--version", "yt-dlp"),
                CheckExecutableApp.FFMPEG => CheckExecutableAsync(SettingsService.FFmpegPath, "-version", "ffmpeg"),
                CheckExecutableApp.GALLERYDL => CheckExecutableAsync(SettingsService.GalleryDLPath, "--version", "gallery-dl"),
                _ => throw new ArgumentOutOfRangeException(nameof(executable), executable, null)
            };
        }
    }
}

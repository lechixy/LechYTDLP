using LechYTDLP.Components;
using LechYTDLP.Services;
using LechYTDLP.Util;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LechYTDLP.Classes
{
    public class VideoInfo
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }
        [JsonPropertyName("formats")]
        public List<VideoFormat>? Formats { get; set; }
        [JsonPropertyName("channel")]
        public string? Channel { get; set; }
        [JsonPropertyName("channel_id")]
        public string? ChannelId { get; set; }
        [JsonPropertyName("uploader")]
        public string? Uploader { get; set; }
        [JsonPropertyName("uploader_id")]
        public string? UploaderId { get; set; }
        [JsonPropertyName("channel_url")]
        public string? ChannelUrl { get; set; }
        [JsonPropertyName("uploader_url")]
        public string? UploaderUrl { get; set; }
        [JsonPropertyName("track")]
        public string? Track { get; set; }
        [JsonPropertyName("artists")]
        public string[]? Artists { get; set; }
        [JsonPropertyName("duration")]
        public double? Duration { get; set; }
        [JsonPropertyName("title")]
        public string? Title { get; set; }
        [JsonPropertyName("description")]
        public string? Description { get; set; }
        [JsonPropertyName("timestamp")]
        public int? Timestamp { get; set; }
        [JsonPropertyName("view_count")]
        public int? ViewCount { get; set; }
        [JsonPropertyName("like_count")]
        public int? LikeCount { get; set; }
        [JsonPropertyName("repost_count")]
        public int? RepostCount { get; set; }
        [JsonPropertyName("comment_count")]
        public int? CommentCount { get; set; }
        [JsonPropertyName("thumbnails")]
        public List<Thumbnail>? Thumbnails { get; set; }
        [JsonPropertyName("webpage_url")]
        public string? WebpageUrl { get; set; }
        [JsonPropertyName("original_url")]
        public string? OriginalUrl { get; set; }
        [JsonPropertyName("webpage_url_basename")]
        public string? WebpageUrlBasename { get; set; }
        [JsonPropertyName("webpage_url_domain")]
        public string? WebpageUrlDomain { get; set; }
        [JsonPropertyName("extractor")]
        public string? Extractor { get; set; }
        [JsonPropertyName("extractor_key")]
        public string? ExtractorKey { get; set; }
        [JsonPropertyName("thumbnail")]
        public string? Thumbnail { get; set; }
        [JsonPropertyName("display_id")]
        public string? DisplayId { get; set; }
        [JsonPropertyName("fulltitle")]
        public string? FullTitle { get; set; }
        [JsonPropertyName("duration_string")]
        public string? DurationString { get; set; }
        [JsonPropertyName("upload_date")]
        public string? UploadDate { get; set; }
        [JsonPropertyName("artist")]
        public string? Artist { get; set; }
        [JsonPropertyName("epoch")]
        public int? Epoch { get; set; }
        [JsonPropertyName("ext")]
        public string? Ext { get; set; }
        [JsonPropertyName("vcodec")]
        public string? VCodec { get; set; }
        [JsonPropertyName("acodec")]
        public string? ACodec { get; set; }
        [JsonPropertyName("format_id")]
        public string? FormatId { get; set; }
        [JsonPropertyName("url")]
        public string? Url { get; set; }
        [JsonPropertyName("format_note")]
        public string? FormatNote { get; set; }
        [JsonPropertyName("preference")]
        public int? Preference { get; set; }
        [JsonPropertyName("width")]
        public int? Width { get; set; }
        [JsonPropertyName("height")]
        public int? Height { get; set; }
        [JsonPropertyName("quality")]
        public double? Quality { get; set; }
        [JsonPropertyName("protocol")]
        public string? Protocol { get; set; }
        [JsonPropertyName("video_ext")]
        public string? VideoExt { get; set; }
        [JsonPropertyName("audio_ext")]
        public string? AudioExt { get; set; }
        [JsonPropertyName("resolution")]
        public string? Resolution { get; set; }
        [JsonPropertyName("dynamic_range")]
        public string? DynamicRange { get; set; }
        [JsonPropertyName("filesize")]
        public long? FileSize { get; set; }
        [JsonPropertyName("filesize_approx")]
        public long? FileSizeApprox { get; set; }
        [JsonPropertyName("cookies")]
        public string? Cookies { get; set; }
        [JsonPropertyName("format")]
        public string? Format { get; set; }
        [JsonPropertyName("filename")]
        public string? Filename { get; set; }
    }


    public class Thumbnail
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }
        [JsonPropertyName("url")]
        public string? Url { get; set; }
    }

    public class VideoFormat
    {
        [JsonPropertyName("ext")]
        public string? Ext { get; set; }
        [JsonPropertyName("vcodec")]
        public string? VCodec { get; set; }
        [JsonPropertyName("acodec")]
        public string? ACodec { get; set; }
        [JsonPropertyName("format_id")]
        public string? FormatId { get; set; }
        [JsonPropertyName("url")]
        public string? Url { get; set; }
        [JsonPropertyName("fps")]
        public double? Fps { get; set; }
        [JsonPropertyName("format_note")]
        public string? FormatNote { get; set; }
        [JsonPropertyName("preference")]
        public int? Preference { get; set; }
        [JsonPropertyName("width")]
        public int? Width { get; set; }
        [JsonPropertyName("height")]
        public int? Height { get; set; }
        [JsonPropertyName("quality")]
        public double? Quality { get; set; }
        [JsonPropertyName("protocol")]
        public string? Protocol { get; set; }
        [JsonPropertyName("video_ext")]
        public string? VideoExt { get; set; }
        [JsonPropertyName("audio_ext")]
        public string? AudioExt { get; set; }
        [JsonPropertyName("resolution")]
        public string? Resolution { get; set; }
        [JsonPropertyName("dynamic_range")]
        public string? DynamicRange { get; set; }
        [JsonPropertyName("filesize")]
        public long? FileSize { get; set; }
        [JsonPropertyName("filesize_approx")]
        public long? FileSizeApprox { get; set; }
        [JsonPropertyName("cookies")]
        public string? Cookies { get; set; }
        [JsonPropertyName("format")]
        public string? Format { get; set; }
    }

    public class MergedVideoFormat
    {
        public int? Width { get; set; }
        public int? Height { get; set; }
        public string? Resolution { get; set; } = string.Empty;
        public string? FormatNote { get; set; } = string.Empty;
        public VideoFormat[]? Formats { get; set; } = [];
    }

    [JsonSerializable(typeof(VideoInfo))]
    [JsonSerializable(typeof(Dictionary<string, JsonElement>))]
    public partial class AppJsonContext : JsonSerializerContext
    {
    }

    public class UpdateResult
    {
        public UpdateStatus Status { get; set; }
        public string? Message { get; set; }
    }

    public enum UpdateStatus
    {
        UpToDate,
        Updated,
        Failed
    }

    public sealed class YTDLPDownloadArgs
    {
        // Required for most operations
        public string? Url { get; set; } = null;
        public SelectedFormat? SelectedFormat { get; set; }

        // Output
        public bool DumpJson { get; set; } = false;
        public string? PrintToFile { get; set; } = null;

        // File
        public string? OutputPath { get; set; }
        public string? FFmpegLocation { get; set; }
        public bool? ForceOverwrites { get; set; }

        // Downloads
        public int? ConcurrentFragments { get; set; }

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
                            args.Add("-f bestvideo+bestaudio");
                            break;
                        case "bestvideo":
                            args.Add("-f bestvideo");
                            break;
                        case "bestaudio":
                            args.Add("-f bestaudio");
                            break;
                        case "compatible720pmp4":
                            args.Add("-f bestvideo[height<=720][ext=mp4]+bestaudio[ext=m4a]/best[height<=720][ext=mp4]/best");
                            args.Add("--merge-output-format mp4");
                            break;
                        case "compatible1080pmp4":
                            args.Add("-f bestvideo[height<=1080][ext=mp4]+bestaudio[ext=m4a]/best[height<=1080][ext=mp4]/best");
                            args.Add("--merge-output-format mp4");
                            break;
                        case "extractaudiomp3":
                            args.Add("-x --audio-format mp3");
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

            if (DumpJson)
                args.Add("--dump-json");

            if (!string.IsNullOrEmpty(FFmpegLocation))
                args.Add($"--ffmpeg-location \"{FFmpegLocation}\"");

            if (!string.IsNullOrEmpty(CookiesPath))
                args.Add($"--cookies \"{CookiesPath}\"");
            if (!string.IsNullOrEmpty(JavaScriptRuntime))
                args.Add($"--js-runtimes {JavaScriptRuntime}");

            if (OutputPath != null)
                args.Add($"-o \"{OutputPath}\"");

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
                args.Add("--no-color");

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

            return string.Join(" ", args);
        }
    }


    internal class YTDLP
    {
        public event Action<string>? OutputReceived;
        public event Action<string>? ErrorReceived;
        public event Action<int>? ProcessExited;

        private Process? _process;

        public async Task<int> StartYTDLP(YTDLPDownloadArgs args)
        {
            var tcs = new TaskCompletionSource<int>();

            if (_process != null && !_process.HasExited)
            {
                tcs.SetException(new InvalidOperationException("Zaten çalışan bir yt-dlp süreci var."));
                return await tcs.Task;
            }

            // These args must be included in every yt-dlp process, so we add them by default. User-provided args will be added on top of these.
            var mustHaveArgs = new YTDLPDownloadArgs
            {
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
            string Arguments = args.Update ? ytdlpArgs : $"{ytdlpArgs} {mustHaveArgs}";

            LogService.Add($"🚩 {App.LocalizationService.Get("StartingYTdlpWithLog")}:", LogTag.YTDLP);
            LogService.Add($"{SettingsService.YTDLPPath} {Arguments}", LogTag.Normal);

            _process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = $"\"{SettingsService.YTDLPPath}\"",
                    Arguments = Arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8,
                    WorkingDirectory = Environment.CurrentDirectory,
                    EnvironmentVariables =
                    {
                        ["PYTHONIOENCODING"] = "utf-8",
                        ["PYTHONUTF8"] = "1"
                    }
                },
                EnableRaisingEvents = true
            };

            _process.OutputDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    if (args.DumpJson)
                    {
                        LogService.Add($"ℹ️ {App.LocalizationService.Get("GettingVideoInfoLog")}...", LogTag.Warning);
                    }
                    else if (e.Data.StartsWith("P|"))
                    {
                        LogService.AddOrUpdate(LogKey.Download, e.Data);
                    }
                    else LogService.Add(e.Data);

                    OutputReceived?.Invoke(e.Data);
                }
            };

            _process.ErrorDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    if (e.Data.StartsWith("[debug]")) return;

                    Debug.WriteLine("This error received from yt-dlp process:");
                    Debug.WriteLine(e.Data);
                    KnownErrors.Check(new Exception(e.Data));
                    ErrorReceived?.Invoke(e.Data);
                }
            };

            _process.Exited += (s, e) =>
            {
                var p = (Process)s!;

                Debug.WriteLine("Process is exited.");
                LogService.Add($"🏁 {App.LocalizationService.Get("YTdlpProcessExitedLog")} ({App.LocalizationService.Get("ExitCode")}: {p.ExitCode})", LogTag.YTDLP);

                if (p.ExitCode != 0)
                {
                    LogService.Add($"⤷ {App.LocalizationService.Get("YTdlpProcessNonZeroLog")}", LogTag.Error);
                }
                tcs.TrySetResult(p.ExitCode);

                ProcessExited?.Invoke(p.ExitCode);
                p.Dispose();
                _process = null;
            };

            try
            {
                _process.Start();
                _process.BeginOutputReadLine();
                _process.BeginErrorReadLine();
            }
            catch (Exception ex)
            {
                LogService.Add($"❌ {App.LocalizationService.Get("FailedToStartYTdlpLog")}: {ex.Message}", LogTag.Error);

                _process = null;
                KnownErrors.Check(ex);
                tcs.SetResult(-1);
            }

            return await tcs.Task;
        }

        public async Task StopYTDLPAsync()
        {
            if (_process == null) return;

            Debug.WriteLine("Stopping yt-dlp process...");
            _process.Kill(true);
        }

        public Task<VideoInfo?> GetVideoInfoAsync(string url)
        {
            var tcs = new TaskCompletionSource<VideoInfo?>();

            var ytdlpArgs = new YTDLPDownloadArgs
            {
                Url = url,
                DumpJson = true,
                OutputPath = $"{SettingsService.DownloadPath}\\{SettingsService.FilenameTemplate}"
            };
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
                string readContents;
                using (StreamReader streamReader = new(path, Encoding.UTF8))
                {
                    readContents = streamReader.ReadToEnd();
                }

                var info = JsonSerializer.Deserialize<VideoInfo>(readContents, AppJsonContext.Default.VideoInfo);

                if (info != null)
                {
                    OutputReceived -= OnOutput;
                    tcs.TrySetResult(info);
                }

                return tcs.Task;
            }

            void OnOutput(string data)
            {
                if (string.IsNullOrWhiteSpace(data))
                    return;

                try
                {
                    var info = JsonSerializer.Deserialize<VideoInfo>(data, AppJsonContext.Default.VideoInfo);

                    if (info != null)
                    {
                        OutputReceived -= OnOutput;

                        // Write video info json to file if setting is enabled, write to desktop\lechytdlp_dump.json
                        if (SettingsService.WriteVideoInfoJson && !SettingsService.IsUsingBlobData)
                        {
                            LogService.Add($"💾 {App.LocalizationService.Get("WritingVideoInfoToJsonLog")}...", LogTag.YTDLP);

                            string outputPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "lechytdlp_dump.json");
                            using StreamWriter writer = new(outputPath, false, Encoding.UTF8);
                            //string jsonString = JsonSerializer.Serialize(info, new JsonSerializerOptions { WriteIndented = true });
                            writer.Write(data);
                        }

                        tcs.TrySetResult(info);
                    }
                }
                catch (JsonException e)
                {
                    LogService.Add("JSON parse error: " + e.Message, LogTag.Error);

                    Debug.WriteLine(data);
                    Debug.WriteLine(e);
                }
                catch (Exception ex)
                {
                    LogService.Add("Error processing output: " + ex.Message, LogTag.Error);
                    Debug.WriteLine(ex);
                }
            }

            void OnProcessExited(int exitCode)
            {
                ProcessExited -= OnProcessExited;
                OutputReceived -= OnOutput;
                tcs.TrySetResult(null);
            }

            OutputReceived += OnOutput;
            ProcessExited += OnProcessExited;

            _ = Task.Run(async () =>
            {
                try
                {
                    await StartYTDLP(ytdlpArgs);
                }
                catch (Exception ex)
                {
                    OutputReceived -= OnOutput;
                    KnownErrors.Check(ex);
                    tcs.TrySetException(ex);
                }
            });

            return tcs.Task;
        }

        public Task<int> DownloadVideo(YTDLPDownloadArgs args, VideoInfo info)
        {
            var tcs = new TaskCompletionSource<int>();

            LogService.Add($"⬇️ {App.LocalizationService.Get("DownloadingVideoLog")}: {args.Url}", LogTag.YTDLP);


            List<string> texts = [];

            void OnOutput(string data)
            {
                if (string.IsNullOrWhiteSpace(data))
                    return;

                HandleOutput(data);
                try
                {
                    // Save log of each download if setting is enabled, save to download folder with file name {video_id}.log
                    if (SettingsService.SaveLogOfEachDownload && !data.StartsWith("P|")) texts.Add(data);
                }
                catch (JsonException)
                {
                    // dump-json dışında bir satır gelirse ignore
                }
            }

            async void OnProcessExited(int exitCode)
            {
                ProcessExited -= ProcessExited;
                OutputReceived -= OnOutput;

                if (SettingsService.SaveLogOfEachDownload)
                {
                    string logPath = Path.Combine(SettingsService.DownloadPath, $"{info.Id}.log");
                    using StreamWriter logWriter = new(logPath, false, Encoding.UTF8);
                    foreach (var text in texts)
                    {
                        logWriter.WriteLine(text);
                    }
                    logWriter.Flush();
                    logWriter.Close();
                }

                // We need info about the video to set filepath etc.
                string infoJsonPath = Path.Combine(LechKnownFolders.GetPath(LechKnownFolder.Documents), $"LechYTDLP\\Logs\\{info.Id}.info.json");

                if (File.Exists(infoJsonPath))
                {
                    string json = await File.ReadAllTextAsync(infoJsonPath, Encoding.UTF8);
                    try
                    {
                        var videoInfo = JsonSerializer.Deserialize<VideoInfo>(json, AppJsonContext.Default.VideoInfo);
                        if (videoInfo != null && videoInfo.Filename != null && App.DownloadService.CurrentMedia != null)
                        {
                            App.DownloadService.CurrentMedia.FilePath = videoInfo.Filename;
                        }
                    }
                    catch (JsonException ex)
                    {
                        LogService.Add("Error parsing video info JSON: " + ex.Message, LogTag.Error);
                    }
                }

                // If there is no filepath set, we can try to find the file in the download folder
                if (App.DownloadService.CurrentMedia != null && !File.Exists(App.DownloadService.CurrentMedia.FilePath))
                {
                    var fileName = App.DownloadService.CurrentMedia.FilePath
                        .Split(Path.DirectorySeparatorChar).LastOrDefault()?
                        .Split('.')[0];

                    // Try to find the file in the download folder
                    var files = Directory.GetFiles(SettingsService.DownloadPath, $"{App.DownloadService.CurrentMedia.Id}.*");
                    // We sort list by last write time to get the most recent file
                    if (files.Length > 0)
                    {
                        Array.Sort(files, (x, y) => File.GetLastWriteTime(y).CompareTo(File.GetLastWriteTime(x)));
                        App.DownloadService.CurrentMedia.FilePath = files[0];
                    }
                }

                tcs.SetResult(exitCode);
            }

            OutputReceived += OnOutput;
            ProcessExited += OnProcessExited;

            _ = Task.Run(async () =>
            {
                try
                {
                    await StartYTDLP(args);
                }
                catch (Exception ex)
                {
                    OutputReceived -= OnOutput;
                    KnownErrors.Check(ex);
                    tcs.TrySetException(ex);
                }
            });

            return tcs.Task;
        }
        public void HandleOutput(string output)
        {
            if (string.IsNullOrWhiteSpace(output) || App.DownloadService.CurrentMedia == null)
                return;

            //if (output.Contains("Downloading") && output.Contains("format(s):") && output.Contains('+'))
            //{

            //}

            //if (output.Contains("Merging formats into"))
            //{
            //    App.DownloadService.CurrentMedia.FilePath = output.Split('"')[1].Split('"')[0];
            //} else if (output.Contains("Destination: "))
            //{
            //    App.DownloadService.CurrentMedia.FilePath = output.Split("Destination: ")[1].Trim();
            //} else if (output.Contains("has already been downloaded"))
            //{
            //    App.DownloadService.CurrentMedia.FilePath = output.Split("[download]")[1].Split("has already been downloaded")[0].Trim();
            //}
        }
        public Task<UpdateResult> CheckAndDownloadUpdate()
        {
            var tcs = new TaskCompletionSource<UpdateResult>();
            LogService.Add($"🔍 {App.LocalizationService.Get("YTdlpCheckingForUpdates")}...", LogTag.YTDLP);
            App.DownloadController.SetBusy(true, $"{App.LocalizationService.Get("YTdlpUpdating")}...");

            var args = new YTDLPDownloadArgs
            {
                Update = true
            };

            void OnOutput(string data)
            {
                if (string.IsNullOrWhiteSpace(data))
                    return;

                try
                {
                    if (data.Contains("yt-dlp is up to date"))
                    {
                        // yt-dlp is up to date (stable@2026.03.17 from yt-dlp/yt-dlp)
                        string newVersion = data.Split('@')[1].Split(' ')[0].Trim();

                        SettingsService._LastKnownYTdlpToolVersion = newVersion;
                        tcs.SetResult(new UpdateResult
                        {
                            Status = UpdateStatus.UpToDate,
                            Message = data.Split('(')[1].Split(')')[0] // Extract new version
                        });
                    }
                    else if (data.Contains("Updated yt-dlp to"))
                    {
                        // Updated yt-dlp to stable@2026.03.17 from yt-dlp/yt-dlp
                        string newVersion = data.Split('@')[1].Split(' ')[0].Trim();

                        SettingsService._LastKnownYTdlpToolVersion = newVersion;
                        tcs.SetResult(new UpdateResult
                        {
                            Status = UpdateStatus.Updated,
                            Message = data.Split("to ")[1].Trim() // Extract new version
                        });
                    }

                    Debug.WriteLine(data);
                }
                catch (JsonException)
                {
                    // dump-json dışında bir satır gelirse ignore
                }
            }
            ;

            void OnExited(int exitCode)
            {
                ProcessExited -= OnExited;
                OutputReceived -= OnOutput;
                App.DownloadController.SetBusy(false, "");
                if (exitCode != 0)
                    tcs.TrySetResult(new UpdateResult
                    {
                        Status = UpdateStatus.Failed,
                        Message = null
                    });
            }

            OutputReceived += OnOutput;
            ProcessExited += OnExited;

            _ = Task.Run(async () =>
            {
                int success = await StartYTDLP(args);
                if (success != 0) tcs.SetResult(new UpdateResult
                {
                    Status = UpdateStatus.Failed,
                    Message = null
                });
            });

            return tcs.Task;
        }

        public enum CheckExecutableApp
        {
            YTDLP,
            FFMPEG
        }

        public static Task<string> CheckExecutable(CheckExecutableApp executable)
        {
            var tcs = new TaskCompletionSource<string>();
            Process? localProcess = null;

            try
            {
                string fileName = executable switch
                {
                    CheckExecutableApp.YTDLP => $"\"{SettingsService.YTDLPPath}\"",
                    CheckExecutableApp.FFMPEG => $"\"{SettingsService.FFmpegPath}\"",
                    _ => throw new ArgumentOutOfRangeException(nameof(executable), executable, null)
                };
                string appName = executable switch
                {
                    CheckExecutableApp.YTDLP => "yt-dlp",
                    CheckExecutableApp.FFMPEG => "ffmpeg",
                    _ => throw new ArgumentOutOfRangeException(nameof(executable), executable, null)
                };
                string args = executable == CheckExecutableApp.YTDLP ? "--version" : "-version";

                localProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = fileName,
                        Arguments = args,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    },
                    EnableRaisingEvents = true
                };


                localProcess.OutputDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        if (e.Data.Contains("ffmpeg version") && executable == CheckExecutableApp.FFMPEG)
                        {
                            var version = e.Data.Split(' ')[2];
                            Debug.WriteLine($"{appName} version: {version}");
                            tcs.SetResult(version);
                        }
                        else if (executable == CheckExecutableApp.YTDLP)
                        {
                            Debug.WriteLine($"{appName} version: {e.Data}");
                            tcs.SetResult(e.Data);
                        }
                    }
                };

                localProcess.ErrorDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        tcs.TrySetException(new Exception($"{appName} {e.Data}"));
                };

                localProcess.Exited += (s, e) =>
                {
                    var p = (Process)s!;
                    if (p.ExitCode != 0)
                        tcs.TrySetException(new Exception($"{appName} exit code: {p.ExitCode}"));

                    p.Dispose();
                    localProcess = null;
                };


                localProcess.Start();
                localProcess.BeginOutputReadLine();
                localProcess.BeginErrorReadLine();
            }
            catch (Exception)
            {
                localProcess = null;
                throw;
            }

            return tcs.Task;
        }
    }
}

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

        // File
        public string? OutputPath { get; set; }
        public string? FFmpegLocation { get; set; }

        // Account
        public string? CookiesPath { get; set; }

        // Options
        public bool EmbedMetadata { get; set; } = false;
        public bool EmbedThumbnail { get; set; } = false;
        public bool EmbedSubs { get; set; } = false;

        // YT-DLP
        public bool Update { get; set; } = false;
        public string? JavaScriptRuntime { get; set; }

        // Debug, logging etc.
        public bool Verbose { get; set; } = false;

        public string BuildArgs()
        {
            var args = new List<string>();

            if (Url != null)
            {
                args.Add($"\"{Url}\"");
            }

            if (SelectedFormat != null)
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

            var mustHaveArgs = new YTDLPDownloadArgs
            {
                CookiesPath = SettingsService.CookiesfilePath,
                JavaScriptRuntime = string.IsNullOrEmpty(SettingsService.JavaScriptRuntime) ? "" : SettingsService.JavaScriptRuntime,
                Verbose = SettingsService.UseVerboseLoggingOnYTDLP
            }.BuildArgs();

            var ytdlpArgs = args.BuildArgs();
            string Arguments = $"{ytdlpArgs} {mustHaveArgs}";
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
                    else if (e.Data.StartsWith("[download]") && e.Data.Contains("of") && e.Data.Contains("at"))
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
                string path = "C:\\Users\\lechi\\Desktop\\a.json";
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

                        // Write video info json to file if setting is enabled, write to desktop\b.json
                        if (SettingsService.WriteVideoInfoJson)
                        {
                            string outputPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "b.json");
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

        public Task<int> DownloadVideo(YTDLPDownloadArgs args)
        {
            var tcs = new TaskCompletionSource<int>();

            LogService.Add($"⬇️ {App.LocalizationService.Get("DownloadingVideoLog")}: {args.Url}", LogTag.YTDLP);

            void OnOutput(string data)
            {
                if (string.IsNullOrWhiteSpace(data))
                    return;

                try
                {

                }
                catch (JsonException)
                {
                    // dump-json dışında bir satır gelirse ignore
                }
            }

            void OnProcessExited(int exitCode)
            {
                ProcessExited -= ProcessExited;
                OutputReceived -= OnOutput;

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

        public Task<UpdateResult> CheckForUpdates()
        {
            var tcs = new TaskCompletionSource<UpdateResult>();
            LogService.Add($"🔍 {App.LocalizationService.Get("CheckingForUpdatesLog")}...", LogTag.YTDLP);

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
                    if (data.Contains("yt-dlp is up to date")) tcs.SetResult(new UpdateResult
                    {
                        Status = UpdateStatus.UpToDate,
                        Message = data.Split('(')[1].Split(')')[0] // Extract new version
                    });
                    else if (data.Contains("Updated yt-dlp to")) tcs.SetResult(new UpdateResult
                    {
                        Status = UpdateStatus.Updated,
                        Message = data.Split("to ")[1].Trim() // Extract new version
                    });

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

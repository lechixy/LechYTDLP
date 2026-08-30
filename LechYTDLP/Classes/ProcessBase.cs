using LechYTDLP.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LechYTDLP.Classes
{
    public abstract class ProcessBase
    {
        /// <summary>
        /// Starts a process with the specified executable path and arguments, and returns the exit code.
        /// </summary>
        protected async Task<ProcessResult> RunProcessAsync(
            string fileName,
            string arguments,
            Action<string>? onOutput = null,
            Action<string>? onError = null,
            CancellationToken cancellationToken = default)
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
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

            process.OutputDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    onOutput?.Invoke(e.Data);
            };

            process.ErrorDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    onError?.Invoke(e.Data);
            };

            try
            {
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await process.WaitForExitAsync(cancellationToken);

                return new ProcessResult
                {
                    Code = process.ExitCode == 0 ? ResultCode.Success : ResultCode.Error,
                    Reason = ResultReason.None,
                    Message = process.ExitCode == 0 ? "Success" : $"Exited with code {process.ExitCode}"
                };
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (!process.HasExited)
                    {
                        process.Kill();
                    }
                }
                catch (Win32Exception ex)
                {
                    Debug.WriteLine($"Kill Win32 Error:");
                    Debug.WriteLine($"Message: {ex.Message}");
                    Debug.WriteLine($"NativeErrorCode: {ex.NativeErrorCode}");
                }
                catch (InvalidOperationException)
                {
                    // Process zaten kapanmış olabilir
                }
                return new ProcessResult { Code = ResultCode.Cancelled, Message = "Process cancelled by user." };
            }
            catch (Exception ex)
            {
                await KnownErrors.Check(ex);
                return new ProcessResult
                {
                    Code = ResultCode.Error,
                    Reason = ResultReason.FailedToStartProcess,
                    Message = ex.Message
                };
            }
        }
        public static async Task<string> CheckExecutableAsync(string fileName, string args, string appName)
        {
            var tcs = new TaskCompletionSource<string>();
            try
            {
                using var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = $"\"{fileName}\"",
                        Arguments = args,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    },
                    EnableRaisingEvents = true
                };

                process.OutputDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        if (appName == "ffmpeg" && e.Data.Contains("ffmpeg version"))
                            tcs.TrySetResult(e.Data.Split(' ')[2]);
                        else if (appName != "ffmpeg")
                            tcs.TrySetResult(e.Data.Trim());
                    }
                };

                process.ErrorDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        tcs.TrySetException(new Exception($"{appName} error: {e.Data}"));
                };

                process.Exited += (s, e) =>
                {
                    if (process.ExitCode != 0)
                        tcs.TrySetException(new Exception($"{appName} exit code: {process.ExitCode}"));
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await process.WaitForExitAsync();
                return await tcs.Task;
            }
            catch
            {
                throw;
            }
        }
    }
}

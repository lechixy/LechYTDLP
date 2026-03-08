using LechYTDLP.Services;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LechYTDLP.Util
{
    public class KnownErrors
    {
        public enum GenericError
        {
            NoFileOrDirectory,
        }

        public static void ShowGenericError(GenericError error)
        {
            if (error == GenericError.NoFileOrDirectory)
            {
                Console.WriteLine($"File cannot be opened.");
                App.InfoBarService.Show(new InfoBarMessage
                {
                    Title = App.LocalizationService.Get("FileCantBeOpened"),
                    Message = App.LocalizationService.Get("FileCantBeOpenedMsg"),
                    Severity = InfoBarSeverity.Error,
                    DurationMs = 5000,
                    IsCancelable = false
                });
            }
        }
        public static void Check(Exception ex)
        {
            // There is no yt-dlp executable.
            if (ex.Message.Contains("An error occurred trying to start process"))
            {
                LogService.Add(App.LocalizationService.GetString("NoYTdlpExecutableLog", ex.Message), LogTag.Error);
                App.InfoBarService.Show(new InfoBarMessage
                {
                    Title = App.LocalizationService.Get("NoYTdlpExecutable"),
                    Message = App.LocalizationService.Get("NoYTdlpExecutableMsg"),
                    Severity = InfoBarSeverity.Error,
                    DurationMs = 0,
                    IsCancelable = true
                });
            }
            // There is no ffmpeg executable.
            else if (ex.Message.Contains("ffmpeg not found") || ex.Message.Contains("ffmpeg-location"))
            {
                LogService.Add(App.LocalizationService.GetString("NoFFmpegExecutableLog", ex.Message), LogTag.Error);
                App.InfoBarService.Show(new InfoBarMessage
                {
                    Title = App.LocalizationService.Get("NoFFmpegExecutable"),
                    Message = App.LocalizationService.Get("NoFFmpegExecutableMsg"),
                    Severity = InfoBarSeverity.Error,
                    DurationMs = 0,
                    IsCancelable = true
                });
            }
            // Link is invalid.
            else if (ex.Message.Contains("is not a valid URL"))
            {
                LogService.Add(App.LocalizationService.GetString("InvalidLinkLog", ex.Message), LogTag.Error);
                App.InfoBarService.Show(new InfoBarMessage
                {
                    Title = App.LocalizationService.Get("InvalidLink"),
                    Message = App.LocalizationService.Get("InvalidLinkMsg"),
                    Severity = InfoBarSeverity.Warning,
                    DurationMs = 5000,
                    IsCancelable = false
                });
            }
            // Server sent an empty media response.
            else if (ex.Message.Contains("sent an empty media response"))
            {
                LogService.Add($"Server sent an empty media response {ex.Message}", LogTag.Error);
                App.InfoBarService.Show(new InfoBarMessage
                {
                    Title = "Empty media response",
                    Message = "Maybe passing your cookies for this website from Settings can solve this.",
                    Severity = InfoBarSeverity.Error,
                    DurationMs = 0,
                    IsCancelable = true
                });
            }
            // Video is DRM protected.
            else if (ex.Message.Contains("use DRM protection") || ex.Message.Contains("video is not DRM protected"))
            {
                if (ex.Message.Contains("Please DO NOT open an issue")) return;

                LogService.Add($"{App.LocalizationService.Get("DrmProtectedLog")} {ex.Message}", LogTag.Warning);
                App.InfoBarService.Show(new InfoBarMessage
                {
                    Title = App.LocalizationService.Get("DrmProtected"),
                    Message = App.LocalizationService.Get("DrmProtectedMsg"),
                    Severity = InfoBarSeverity.Warning
                });
            }
            else if (ex.Message.Contains("No supported JavaScript runtime could be found"))
            {
                LogService.Add(App.LocalizationService.GetString("NoJsRuntimeLog", ex.Message), LogTag.Warning);

                // Add hyperlink to the info bar message to open the lechytdlp wiki page about this issue.
                App.InfoBarService.Show(new InfoBarMessage
                {
                    Title = App.LocalizationService.Get("NoJsRuntime"),
                    Message = App.LocalizationService.Get("NoJsRuntimeMsg"),
                    Severity = InfoBarSeverity.Warning,
                    HyperlinkButton = new InfoBarHyperlinkButton
                    {
                        Content = App.LocalizationService.Get("LearnMore"),
                        NavigateUri = new Uri(Main.GetLink(Links.NoJsRuntime))
                    },
                    DurationMs = 0,
                    IsCancelable = true
                });
            }
            else
            {
                LogService.Add(ex.Message, LogTag.Error);

                // YouTube is forcing SABR streaming for this client.
                // This is not an error, but yt-dlp throws an exception for some reason. We can just ignore it.
                if (ex.Message.Contains("YouTube is forcing SABR streaming for this client.")) return;

                App.InfoBarService.Show(new InfoBarMessage
                {
                    Title = App.LocalizationService.Get("UnhandledError"),
                    Message = ex.Message.Trim(),
                    Severity = InfoBarSeverity.Error,
                    DurationMs = 0,
                    IsCancelable = true
                });
            }
        }
    }
}

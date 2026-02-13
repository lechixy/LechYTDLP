using LechYTDLP.Services;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LechYTDLP.Util
{
    public class KnownErrors
    {
        public static void Check(Exception ex)
        {
            // There is no yt-dlp executable.
            if (ex.Message.Contains("An error occurred trying to start process"))
            {
                LogService.Add($"Error: There is no yt-dlp.exe! Check path because we can't find any executable. Details: {ex.Message}", LogTag.Error);
                App.InfoBarService.Show(new InfoBarMessage(
                    "There is no yt-dlp.exe",
                    "Check yt-dlp path because we can't find any executable.",
                    InfoBarSeverity.Error,
                    0,
                    true
                ));
            }
            // There is no ffmpeg executable.
            else if (ex.Message.Contains("ffmpeg not found") || ex.Message.Contains("ffmpeg-location"))
            {
                LogService.Add($"Error: There is no ffmpeg.exe! Check path because we can't find any executable. Details: {ex.Message}", LogTag.Error);
                App.InfoBarService.Show(new InfoBarMessage(
                    "There is no ffmpeg.exe",
                    "Check ffmpeg path because we can't find any executable.",
                    InfoBarSeverity.Error,
                    0,
                    true
                ));
            }
            // Link is invalid.
            else if (ex.Message.Contains("is not a valid URL"))
            {
                LogService.Add($"Error: The provided link is invalid. Details: {ex.Message}", LogTag.Error);
                App.InfoBarService.Show(new InfoBarMessage(
                    "Invalid link",
                    "The provided link is invalid. Please check and try again.",
                    InfoBarSeverity.Warning,
                    5000,
                    false
                ));
            }
            // Instagram sent an empty media response.
            else if (ex.Message.Contains("Instagram sent an empty media response"))
            {
                LogService.Add($"Error: Instagram sent an empty media response {ex.Message}", LogTag.Error);
                App.InfoBarService.Show(new InfoBarMessage(
                    "Empty media response",
                    "You need to pass cookies to YT-DLP",
                    InfoBarSeverity.Error,
                    0,
                    false
                ));
            }
            else
            {
                LogService.Add(ex.Message, LogTag.Error);
                App.InfoBarService.Show(new InfoBarMessage(
                    "Unhandled error",
                    ex.Message,
                    InfoBarSeverity.Error,
                    0,
                    true
                ));
            }
        }
    }
}

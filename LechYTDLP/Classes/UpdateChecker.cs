using LechYTDLP.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LechYTDLP.Classes;

/// <summary>
/// This class checks for updates by calling the API endpoint and comparing the current version with the newest version returned by the API. It also provides a method to open the update URL in the default browser.
/// Credits to https://github.com/unchihugo/FluentFlyout/blob/master/FluentFlyoutWPF/Classes/UpdateChecker.cs
/// Thanks a lot <3
/// </summary>

public static class UpdateChecker
{
    private static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(10),
        DefaultRequestHeaders =
        {
            { "User-Agent", "LechYTDLP-App" },
            { "Accept", "application/json" }
        }
    };
    private const string ApiEndpoint = "https://api.github.com/repos/yt-dlp/yt-dlp/releases";

    /// <summary>
    /// Result of an update check
    /// </summary>
    public class UpdateCheckResult
    {
        public bool IsUpdateAvailable { get; set; }
        public string NewestVersion { get; set; } = string.Empty;
        public string CurrentVersion { get; set; } = string.Empty;
        public DateTime CheckedAt { get; set; }
        public bool Success { get; set; }
        public bool IsCached { get; set; } = false;
    }

    /// <summary>
    /// Check for updates from the API
    /// </summary>
    /// <param name="currentVersion">The current app version (e.g., "v2.5.0")</param>
    /// <returns>UpdateCheckResult with update information</returns>
    public static async Task<UpdateCheckResult> CheckForUpdatesAsync(string currentVersion)
    {
        // currentVersion = SettingsService._LastKnownYTdlpToolVersion; // this is default tool version is updated every github release

        // If we have a cached version and it was updated less than 10 minutes ago, return the cached result
        // debug only, always return cached result to avoid hitting the API too much during development
        var lastChecked = DateTime.FromBinary(SettingsService._LastUpdateCheckAt);
        if ((DateTime.Now - lastChecked).TotalMinutes < 10)
        {
            LogService.Add(App.LocalizationService.Get("YTdlpUpdateCheckResult", currentVersion, SettingsService._LastKnownVersion), LogTag.App);
            return new UpdateCheckResult
            {
                IsUpdateAvailable = currentVersion != "debug" && IsNewerVersion(currentVersion, SettingsService._LastKnownVersion),
                NewestVersion = SettingsService._LastKnownVersion,
                CurrentVersion = currentVersion,
                CheckedAt = lastChecked,
                Success = true,
                IsCached = true
            };
        }

        var now = DateTime.Now;
        var result = new UpdateCheckResult
        {
            CheckedAt = now
        };
        SettingsService._LastUpdateCheckAt = now.ToBinary();

        try
        {
            var response = await HttpClient.GetStringAsync(ApiEndpoint);
            var json = JsonDocument.Parse(response);
            var root = json.RootElement;

            if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0)
            {
                var first = root[0];
                result.NewestVersion = first.GetProperty("tag_name").GetString() ?? string.Empty;
                result.CurrentVersion = currentVersion;
            }
            result.Success = true;
            SettingsService._LastKnownVersion = result.NewestVersion;

            // Compare versions
            result.IsUpdateAvailable = currentVersion != "debug" && IsNewerVersion(currentVersion, result.NewestVersion);

            LogService.Add(App.LocalizationService.Get("YTdlpUpdateCheckResult", currentVersion, SettingsService._LastKnownVersion), LogTag.App);
        }
        catch (HttpRequestException ex)
        {
            Debug.WriteLine(ex);
            LogService.Add(App.LocalizationService.Get("YTdlpUpdateCheckNoInternet"), LogTag.Error);
            result.Success = false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            LogService.Add(App.LocalizationService.Get("YTdlpUpdateCheckError"), LogTag.Error);
            result.Success = false;
        }

        return result;
    }

    //public static void OpenUpdateUrl(string url)
    //{
    //    if (string.IsNullOrEmpty(url)) return;

    //    try
    //    {
    //        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
    //        {
    //            FileName = url,
    //            UseShellExecute = true
    //        });
    //    }
    //    catch (Exception ex)
    //    {
    //        Logger.Error(ex, "Failed to open update URL");
    //    }
    //}

    private static bool IsNewerVersion(string currentVersion, string newestVersion)
    {
        if (currentVersion == "unknown" || newestVersion == "unknown")
        {
            Debug.WriteLine("Version information is unknown, cannot compare versions.");
            return false;
        }

        try
        {
            var current = Version.Parse(currentVersion);
            var newest = Version.Parse(newestVersion);
            return newest > current;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            return false;
        }
    }
}

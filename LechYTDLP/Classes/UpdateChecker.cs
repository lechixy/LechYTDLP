using LechYTDLP.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
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

    /// <summary>
    /// Güncelleme kontrolü yapılacak kaynak (repo) türü.
    /// </summary>
    public enum UpdateTarget
    {
        App,
        YtDlp
        // İleride: örn. FFmpeg, vs.
    }

    public static string TargetName(UpdateTarget target) => target switch
    {
        UpdateTarget.App => "Lech YT-DLP",
        UpdateTarget.YtDlp => "YT-DLP",
        _ => throw new NotImplementedException()
    };

    /// <summary>
    /// Her bir güncelleme kaynağının API endpoint'i, cache süresi ve
    /// SettingsService üzerindeki karşılık gelen alanlara erişim bilgisini tutar.
    /// </summary>
    private class UpdateSourceConfig
    {
        public required string ApiEndpoint { get; init; }
        public required TimeSpan CacheDuration { get; init; }
        public required Func<long> GetLastCheckAt { get; init; }
        public required Action<long> SetLastCheckAt { get; init; }
        public required Func<string> GetLastKnownVersion { get; init; }
        public required Action<string> SetLastKnownVersion { get; init; }
    }

    private static readonly Dictionary<UpdateTarget, UpdateSourceConfig> SourceConfigs = new()
    {
        [UpdateTarget.App] = new UpdateSourceConfig
        {
            ApiEndpoint = "https://api.github.com/repos/lechixy/LechYTDLP/releases",
            CacheDuration = TimeSpan.FromMinutes(15),
            GetLastCheckAt = () => SettingsService._LastUpdateCheckAt,
            SetLastCheckAt = v => SettingsService._LastUpdateCheckAt = v,
            GetLastKnownVersion = () => SettingsService._LastKnownVersion,
            SetLastKnownVersion = v => SettingsService._LastKnownVersion = v
        },
        [UpdateTarget.YtDlp] = new UpdateSourceConfig
        {
            ApiEndpoint = "https://api.github.com/repos/yt-dlp/yt-dlp/releases",
            CacheDuration = TimeSpan.FromMinutes(10),
            GetLastCheckAt = () => SettingsService._LastYTdlpUpdateCheckAt,
            SetLastCheckAt = v => SettingsService._LastYTdlpUpdateCheckAt = v,
            GetLastKnownVersion = () => SettingsService._LastKnownYTdlpVersion,
            SetLastKnownVersion = v => SettingsService._LastKnownYTdlpVersion = v
        }
    };

    /// <summary>
    /// Result of an update check
    /// </summary>
    public class UpdateCheckResult
    {
        public UpdateTarget Target { get; set; }
        public bool IsUpdateAvailable { get; set; }
        public string NewestVersion { get; set; } = string.Empty;
        public string CurrentVersion { get; set; } = string.Empty;
        public DateTime CheckedAt { get; set; }
        public bool Success { get; set; }
        public bool IsCached { get; set; } = false;
    }

    /// <summary>
    /// Belirtilen hedef (App, YtDlp, vs.) için GitHub üzerinden güncelleme kontrolü yapar.
    /// </summary>
    /// <param name="target">Kontrol edilecek kaynak (App / YtDlp / ...)</param>
    /// <param name="currentVersion">Mevcut versiyon (örn. "v2.5.0")</param>
    public static async Task<UpdateCheckResult> CheckForUpdateAsync(UpdateTarget target, string currentVersion)
    {
        var config = SourceConfigs[target];

        // Cache kontrolü
        var lastChecked = DateTime.FromBinary(config.GetLastCheckAt());
        if ((DateTime.Now - lastChecked) < config.CacheDuration)
        {
            var cachedVersion = config.GetLastKnownVersion();
            LogService.Add(App.LocalizationService.Get("UpdateCheckResult", TargetName(target), currentVersion, cachedVersion), LogTag.App);

            return new UpdateCheckResult
            {
                Target = target,
                IsUpdateAvailable = currentVersion != "debug" && IsNewerVersion(currentVersion, cachedVersion),
                NewestVersion = cachedVersion,
                CurrentVersion = currentVersion,
                CheckedAt = lastChecked,
                Success = true,
                IsCached = true
            };
        }

        var now = DateTime.Now;
        var result = new UpdateCheckResult { CheckedAt = now, Target = target };
        config.SetLastCheckAt(now.ToBinary());

        try
        {
            var json = await FetchReleaseJsonWithRetryAsync(config.ApiEndpoint);

            if (json != null)
            {
                var root = json.RootElement;

                if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0)
                {
                    var first = root[0];
                    result.NewestVersion = first.GetProperty("tag_name").GetString() ?? string.Empty;
                    result.CurrentVersion = currentVersion;
                }

                result.Success = true;
                config.SetLastKnownVersion(result.NewestVersion);

                result.IsUpdateAvailable = currentVersion != "debug" && IsNewerVersion(currentVersion, result.NewestVersion);

                LogService.Add(App.LocalizationService.Get("UpdateCheckResult", TargetName(target), currentVersion, result.NewestVersion), LogTag.App);
            }
            else
            {
                LogService.Add("there is no json response from the API", LogTag.Warning);
            }
        }
        catch (HttpRequestException ex)
        {
            Debug.WriteLine(ex);
            LogService.Add(App.LocalizationService.Get("UpdateCheckNoInternet"), LogTag.Error);
            result.Success = false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            LogService.Add(App.LocalizationService.Get("UpdateCheckError"), LogTag.Error);
            result.Success = false;
        }

        return result;
    }

    private static async Task<JsonDocument?> FetchReleaseJsonWithRetryAsync(string apiEndpoint)
    {
        for (int i = 0; i < 3; i++)
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));

                using var response = await HttpClient.GetAsync(apiEndpoint, cts.Token);
                response.EnsureSuccessStatusCode();

                using var stream = await response.Content.ReadAsStreamAsync();
                return await JsonDocument.ParseAsync(stream);
            }
            catch (Exception ex) when (i < 2)
            {
                Debug.WriteLine(ex);
                Debug.WriteLine("Update check failed, retrying...");
                await Task.Delay((i + 1) * 1000);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                Debug.WriteLine("Update check failed.");
            }
        }

        return null;
    }

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
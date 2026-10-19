using LechYTDLP.Classes;
using LechYTDLP.Components;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace LechYTDLP.Services
{
    public static class ChangelogService
    {
        private static readonly HttpClient HttpClient = new()
        {
            Timeout = TimeSpan.FromSeconds(15),
            DefaultRequestHeaders =
            {
                { "User-Agent", "LechYTDLP-App" },
                { "Accept", "application/json" }
            }
        };

        private static string NormalizeVersion(string? version) =>
            version?.Trim().TrimStart('v', 'V') ?? string.Empty;

        public static bool ShouldShowChangelog()
        {
            var current = NormalizeVersion(App.GetAppVersion());
            var lastUsed = NormalizeVersion(SettingsService._LastUsedVersion);

            if (SettingsService._IsFirstRun)
                return true;

            if (string.IsNullOrEmpty(lastUsed))
                return true;

            return !string.Equals(lastUsed, current, StringComparison.OrdinalIgnoreCase);
        }

        public static async Task<bool> CheckAndShowChangelogOnStartupAsync()
        {
            if (!ShouldShowChangelog())
                return false;

            return await ShowChangelogAsync(isStartup: true);
        }

        public static async Task<bool> ShowChangelogAsync(bool isStartup = false)
        {
            try
            {
                var release = await GetReleaseForCurrentVersionAsync();
                if (release == null)
                {
                    LogService.Add("Could not fetch release notes from GitHub", LogTag.Warning);
                    return false;
                }

                var changelogControl = new ChangelogDialog(release, ChangelogMode.WhatsNew);

                var title = !string.IsNullOrWhiteSpace(release.Name)
                    ? release.Name
                    : release.TagName;

                var okText = App.LocalizationService.Get("OK");
                if (okText == "No translation") okText = "OK";

                var dialogOptions = new DialogOptions
                {
                    Title = title,
                    Content = changelogControl,
                    PrimaryButtonText = okText,
                    PrimaryButtonStyle = Application.Current.Resources["AccentButtonStyle"] as Style,
                    CloseButtonText = string.Empty // Hide close button so only OK is shown
                };

                // Show with isPriority = true so it is placed at the front of the queue
                await App.DialogService.ShowAsync(dialogOptions, isPriority: true);

                // Update last used version and first run flag once displayed
                SettingsService._LastUsedVersion = App.GetAppVersion();
                SettingsService._IsFirstRun = false;

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ChangelogService] Error showing changelog dialog: {ex}");
                LogService.Add($"Changelog error: {ex.Message}", LogTag.Warning);
                return false;
            }
        }

        public static async Task<GitHubRelease?> GetReleaseForCurrentVersionAsync()
        {
            return await GetReleaseForVersionAsync(App.GetAppVersion());
        }

        public static async Task<GitHubRelease?> GetReleaseForVersionAsync(string version)
        {
            try
            {
                using var response = await HttpClient.GetAsync("https://api.github.com/repos/lechixy/LechYTDLP/releases");
                response.EnsureSuccessStatusCode();

                using var stream = await response.Content.ReadAsStreamAsync();
                var releases = await JsonSerializer.DeserializeAsync<List<GitHubRelease>>(stream);

                if (releases == null || releases.Count == 0)
                    return null;

                var targetVersion = NormalizeVersion(version);

                // Match release with current version
                var match = releases.FirstOrDefault(r =>
                    !string.IsNullOrWhiteSpace(r.TagName) &&
                    string.Equals(NormalizeVersion(r.TagName), targetVersion, StringComparison.OrdinalIgnoreCase));

                // Fallback: If current version not found (e.g. unpublished debug/dev build), return the latest published release
                return match ?? releases.FirstOrDefault(r => !r.Draft);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ChangelogService] Error fetching releases: {ex}");
                return null;
            }
        }
    }
}

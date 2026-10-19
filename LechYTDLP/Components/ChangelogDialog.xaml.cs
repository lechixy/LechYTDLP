using LechYTDLP.Classes;
using LechYTDLP.Services;
using LechYTDLP.Util;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.System;

namespace LechYTDLP.Components
{
    public enum ChangelogMode
    {
        WhatsNew,
        UpdateAvailable
    }

    public sealed partial class ChangelogDialog : UserControl
    {
        private readonly GitHubRelease _release;
        private readonly ChangelogMode _mode;

        public ChangelogDialog(GitHubRelease release, ChangelogMode mode = ChangelogMode.WhatsNew, string? currentVersion = null)
        {
            InitializeComponent();
            _release = release;
            _mode = mode;

            if (mode == ChangelogMode.UpdateAvailable)
            {
                var cur = !string.IsNullOrWhiteSpace(currentVersion) ? currentVersion : $"v{App.GetAppVersion()}";
                var next = !string.IsNullOrWhiteSpace(release.TagName) ? release.TagName : "new";
                TagText.Text = $"{cur} → {next}";

                var updateBadge = App.LocalizationService.Get("UpdateAvailableBadge");
                ModeBadgeText.Text = (updateBadge != "No translation") ? updateBadge : "Update Available";

                var notice = App.LocalizationService.Get("UpdateNoticeMsg");
                if (notice != "No translation")
                {
                    UpdateNoticeInfoBar.Message = notice;
                    UpdateNoticeInfoBar.IsOpen = true;
                }
            }
            else
            {
                TagText.Text = !string.IsNullOrWhiteSpace(release.TagName)
                    ? release.TagName
                    : $"v{App.GetAppVersion()}";

                var whatsNew = App.LocalizationService.Get("WhatsNew");
                ModeBadgeText.Text = (whatsNew != "No translation") ? whatsNew : "What's New";
                UpdateNoticeInfoBar.IsOpen = false;
            }

            // Set published date according to application's current culture
            if (release.PublishedAt.HasValue)
            {
                var localTime = release.PublishedAt.Value.ToLocalTime();
                var culture = LocalizationService.CurrentCulture;
                var formattedDate = localTime.ToString("d MMMM yyyy, HH:mm", culture);
                var localized = App.LocalizationService.Get("PublishedAt", formattedDate);
                PublishedAtText.Text = (localized != "No translation") ? localized : formattedDate;
            }
            else
            {
                PublishedAtText.Text = string.Empty;
            }

            // Set GitHub button text
            var githubText = App.LocalizationService.Get("ViewOnGitHub");
            GitHubLinkText.Text = (githubText != "No translation") ? githubText : "GitHub";

            // Render Markdown body
            var content = MarkdownRenderer.Render(release.Body);
            MarkdownContainer.Children.Add(content);
        }

        private async void GitHubLinkButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_release.HtmlUrl) && Uri.TryCreate(_release.HtmlUrl, UriKind.Absolute, out var uri))
            {
                await Launcher.LaunchUriAsync(uri);
            }
        }
    }
}

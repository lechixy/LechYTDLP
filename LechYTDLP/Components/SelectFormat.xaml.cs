using LechYTDLP.Classes;
using LechYTDLP.Services;
using LechYTDLP.Util;
using LechYTDLP.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using static LechYTDLP.Views.SettingsPage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LechYTDLP.Components
{
    public class SelectedFormat
    {
        // # These values used to define predefined presets
        public Setting? Preset { get; set; } = null;
        // # This bottom values used by format dialog service
        public VideoFormat? SelectedVideo { get; set; }
        public VideoFormat? SelectedAudio { get; set; }
        //
        public string? FileExtension { get; set; }
        public string? AudioFileExtension { get; set; }
        public string? FilePath { get; set; }
        //
        public string? VideoId { get; set; }
        public string? Codec { get; set; }
        public string? Audio { get; set; }
        public string? AudioId { get; set; }

        public enum FormatType
        {
            Video,
            Audio,
            Both
        }

        public void Reset(FormatType formatType)
        {
            // Only res:
            // Selected Video
            // Video Id
            // File Extension
            // Res and codec:
            // Codec + others
            if (formatType == FormatType.Video)
            {
                SelectedVideo = null;
                VideoId = null;
                Codec = null;
                FileExtension = null;
            }
            // Selected Audio
            // Audio Id
            // Audio File Extension
            // Audio
            else if (formatType == FormatType.Audio)
            {
                SelectedAudio = null;
                Audio = null;
                AudioId = null;
                AudioFileExtension = null;
            }
            else if (formatType == FormatType.Both)
            {
                Preset = null;
                SelectedVideo = null;
                SelectedAudio = null;
                FileExtension = null;
                AudioFileExtension = null;
                FilePath = null;
                VideoId = null;
                Codec = null;
                Audio = null;
                AudioId = null;
            }
        }
    }

    public class FilteredVideoFormat
    {
        public string? Text { get; set; }
        public string? FormatId { get; set; }
    }

    public class ComboOption
    {
        public string? Text { get; set; }
        public string? FormatId { get; set; }
        public string? ACodec { get; set; }
        public string? VCodec { get; set; }
        public string? Resolution { get; set; }
        public string? FormatNote { get; set; }
    }


    public sealed partial class SelectFormat : UserControl
    {
        public VideoInfo videoData = null!;
        public ObservableCollection<MergedVideoFormat> MergedFormats { get; } = [];
        public ObservableCollection<FilteredVideoFormat> FilteredFormats { get; } = [];
        public ObservableCollection<ComboOption> Presets { get; } = [];
        public ObservableCollection<ComboOption> Resolutions { get; } = [];
        public ObservableCollection<ComboOption> Codecs { get; } = [];
        public ObservableCollection<ComboOption> Audios { get; } = [];
        public SelectedFormat SelectedFormat = new();
        private Storyboard _loadingStoryboard = null!;
        public event Action<bool>? IsUserCanSave;

        public SelectFormat(VideoInfo info)
        {
            InitializeComponent();

            videoData = info;

            ThumbnailImage.Source = new BitmapImage(new Uri(info.Thumbnail ?? "https://placehold.co/320x180.png?text=No+Thumbnail"));
            VideoTitle.Text = info.Title ?? App.LocalizationService.Get("UnknownTitle");

            VideoUploaderAndExtractor.Blocks.Clear();
            var p = new Paragraph();
            p.Inlines.Add(new Run { Text = $"@{info.Uploader}" ?? App.LocalizationService.Get("UnknownUploader") });
            p.Inlines.Add(new Run { Text = $" • {info.ExtractorKey}" });
            VideoUploaderAndExtractor.Blocks.Add(p);

            VideoAltInfo.Text = $"{App.LocalizationService.Get("Saving")}: {info.Filename?.Split('.')[0]}.?";

            var VideoFormats = info.Formats!;

            PresetSelect.ItemsSource = Presets;
            ResolutionSelect.ItemsSource = Resolutions;
            CodecSelect.ItemsSource = Codecs;
            AudioSelect.ItemsSource = Audios;

            for (int i = 0; i < SettingsService.Presets.Count; i++)
            {
                var preset = SettingsService.Presets[i];
                Presets.Add(new ComboOption
                {
                    FormatId = preset.Value,
                    Text = preset.DisplayName
                });
            }

            Resolutions.Add(new ComboOption
            {
                FormatId = "no",
                Text = App.LocalizationService.Get("DontIncludeVideo")
            });
            Audios.Add(new ComboOption
            {
                FormatId = "no",
                Text = App.LocalizationService.Get("DontIncludeAudio")
            });
            PresetSelect.SelectedIndex = 0;
            ResolutionSelect.SelectedIndex = 0;
            AudioSelect.SelectedIndex = 0;

            for (int i = 0; i < VideoFormats.Count; i++)
            {
                var currentFormat = VideoFormats[i];
                if (currentFormat.Format == null) continue;
                if (currentFormat.Format.Contains("storyboard")) continue;
                if (currentFormat.Format.Contains("audio only"))
                {
                    var AudioText = $"{currentFormat.ACodec} • {currentFormat.FormatNote}";
                    Audios.Add(new ComboOption
                    {
                        FormatId = currentFormat.FormatId,
                        Text = AudioText,
                        ACodec = currentFormat.ACodec,
                        FormatNote = currentFormat.FormatNote
                    });
                }

                // If this resolution isn't already in the collection, add it
                var existingVideoFormat = MergedFormats.FirstOrDefault(f => f.Resolution == currentFormat.Resolution);
                if (existingVideoFormat == null)
                {
                    MergedFormats.Insert(0, new MergedVideoFormat
                    {
                        Resolution = currentFormat.Resolution,
                        FormatNote = currentFormat.FormatNote,
                        Width = currentFormat.Width,
                        Height = currentFormat.Height,
                        Formats = [.. VideoFormats.Where(f => f.Resolution == currentFormat.Resolution)]
                    });

                    if (currentFormat.Resolution != null && currentFormat.Resolution != "audio only")
                        Resolutions.Add(new ComboOption
                        {
                            FormatId = currentFormat.FormatId,
                            Text = currentFormat.Resolution,
                            Resolution = currentFormat.Resolution
                        });
                }
            }
        }
        private void Format_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox combo)
            {
                CheckIsReadyToSave();

                if (e.AddedItems.Count == 0) return;
                var Selected = (ComboOption)e.AddedItems[0];
                if (Selected == null) return;

                Debug.WriteLine(Selected.ToString());

                if (combo.Name == "PresetSelect")
                {
                    // If the preset is not "illchose"
                    if (Selected.FormatId != SettingsService.Presets[0].Value)
                    {
                        VideoFormatGrid.Visibility = Visibility.Collapsed;
                        AudioFormatGrid.Visibility = Visibility.Collapsed;
                        FormatBorder.Visibility = Visibility.Collapsed;

                        SelectedFormat.Preset = SettingsService.Presets.FirstOrDefault(p => p.Value == Selected.FormatId);
                    }
                    // If the preset is "illchose"
                    else
                    {
                        VideoFormatGrid.Visibility = Visibility.Visible;
                        AudioFormatGrid.Visibility = Visibility.Visible;
                        FormatBorder.Visibility = Visibility.Visible;

                        SelectedFormat.Preset = null;
                    }
                }

                if (combo.Name == "ResolutionSelect")
                {
                    // If "Doesn't include video" is selected, hide codec selection and video info
                    if (Selected.FormatId == "no")
                    {
                        CodecSelect.Visibility = Visibility.Collapsed;
                        VideoInfo.Visibility = Visibility.Collapsed;
                        SelectedFormat.Reset(SelectedFormat.FormatType.Video);
                        return;
                    }

                    // Update SelectedFormat with the selected resolution, video ID, and video object
                    var SelectedVideo = MergedFormats.FirstOrDefault(f => f.Resolution == Selected.Resolution);

                    if (SelectedVideo != null && SelectedVideo.Formats != null)
                    {
                        // Add codecs to the codec combobox
                        Codecs.Clear();

                        for (int i = 0; i < SelectedVideo.Formats.Length; i++)
                        {
                            var VCodec = SelectedVideo.Formats[i].VCodec;
                            string CodecText = VCodec != null ? VCodec!.Split('.')[0] : App.LocalizationService.Get("NoCodecInfo");

                            Codecs.Add(new ComboOption
                            {
                                FormatId = SelectedVideo.Formats[i].FormatId,
                                Text = CodecText,
                                VCodec = SelectedVideo.Formats[i].VCodec,
                                Resolution = SelectedVideo.Resolution
                            });
                        }

                        //// If all formats have the same video codec
                        //bool AllFormatsHaveSameCodec = SelectedVideo.Formats.All(f => f.VCodec == SelectedVideo.Formats[0].VCodec);
                        //if (AllFormatsHaveSameCodec)
                        //{
                        //    NewCodecs[0].Text += " (worst)";
                        //    NewCodecs[^1].Text += " (best)";
                        //}

                        // This is where default codec selection happens 
                        // Update SelectedFormat with the first format's ID and object
                        SelectedFormat.VideoId = SelectedVideo.Formats[0].FormatId;
                        SelectedFormat.SelectedVideo = SelectedVideo.Formats[0];

                        SelectedFormat.Codec = Codecs[0].VCodec;
                        SelectedFormat.FileExtension = SelectedVideo.Formats[0].Ext;
                        LogService.Add($"{App.LocalizationService.Get("SelectedVideoLog")}: {SelectedFormat.VideoId} - {SelectedFormat.Codec}", LogTag.YTDLP);

                        // Set default codec selection to the first codec because it's usually the best one
                        //if (AllFormatsHaveSameCodec) CodecSelect.SelectedIndex = NewCodecs.Count - 1;
                        //else CodecSelect.SelectedIndex = 0;
                        CodecSelect.SelectedIndex = 0;
                        CodecSelect.Opacity = 1;

                        var ThereIsOnlyOneFormat = SelectedVideo.Formats.Length == 1;
                        var DecideFileSize = SelectedFormat.SelectedVideo.FileSize != null ?
                            SelectedFormat.SelectedVideo.FileSize : SelectedFormat.SelectedVideo.FileSizeApprox;

                        // If the selected resolution has one format, hide codec suggestion
                        if (ThereIsOnlyOneFormat)
                        {
                            VideoInfo.Visibility = Visibility.Visible;
                            CodecSelect.IsEnabled = false;
                            VideoInfo.Text = $"{DownloadSuggester.FormatFileSize(DecideFileSize)} • {App.LocalizationService.Get("OnlyOneCodec")}";
                            return;
                        }

                        CodecSelect.IsEnabled = true;
                        if (SelectedFormat.Codec != null)
                        {
                            var suggestedFormat = DownloadSuggester.FormatTextSuggestion(SelectedFormat.Codec);
                            VideoInfo.Text = $"{DownloadSuggester.FormatFileSize(DecideFileSize)} • {suggestedFormat}";
                        }
                        else VideoInfo.Text = $"{DownloadSuggester.FormatFileSize(DecideFileSize)} • {App.LocalizationService.Get("NotSuggested")}";
                    }
                }
                else if (combo.Name == "CodecSelect")
                {
                    // If "Doesn't include video" is selected
                    if (Selected.FormatId == "no") return;

                    // Update SelectedFormat with the selected codec and video object
                    var SelectedRes = MergedFormats
                        .First(f => f.Resolution == Selected.Resolution);
                    var SelectedVideo = SelectedRes.Formats!.First(f => f.FormatId == Selected.FormatId);

                    SelectedFormat.SelectedVideo = SelectedVideo;
                    SelectedFormat.Codec = Selected.VCodec;
                    SelectedFormat.FileExtension = SelectedVideo.Ext;
                    LogService.Add($"{App.LocalizationService.Get("SelectedVideoLog")}: {SelectedFormat.VideoId} - {SelectedFormat.Codec}", LogTag.YTDLP);

                    var DecideFileSize = SelectedVideo.FileSize != null ?
                        SelectedVideo.FileSize : SelectedVideo.FileSizeApprox;

                    CodecSelect.Visibility = Visibility.Visible;
                    VideoInfo.Visibility = Visibility.Visible;

                    if (SelectedFormat.Codec != null)
                    {
                        var suggestedFormat = DownloadSuggester.FormatTextSuggestion(SelectedFormat.Codec);
                        VideoInfo.Text = $"{DownloadSuggester.FormatFileSize(DecideFileSize)} • {suggestedFormat}";
                    }
                    else VideoInfo.Text = $"{DownloadSuggester.FormatFileSize(DecideFileSize)} • {App.LocalizationService.Get("NotSuggested")}";
                }
                else if (combo.Name == "AudioSelect")
                {
                    // If "Doesn't include video" is selected
                    if (Selected.FormatId == "no")
                    {
                        AudioInfo.Visibility = Visibility.Collapsed;
                        SelectedFormat.Reset(SelectedFormat.FormatType.Audio);
                        return;
                    }

                    SelectedFormat.Audio = Selected.ACodec;

                    // Update SelectedFormat with the selected audio ID and audio object
                    var SelectedAudio = MergedFormats
                        .First(f => f.Resolution == "audio only")
                        .Formats!.First(f => f.ACodec == Selected.ACodec && f.FormatNote == Selected.FormatNote);

                    SelectedFormat.SelectedAudio = SelectedAudio;
                    SelectedFormat.AudioId = SelectedAudio.FormatId;
                    SelectedFormat.AudioFileExtension = SelectedAudio.Ext;
                    LogService.Add($"{App.LocalizationService.Get("SelectedAudioLog")}: {SelectedFormat.AudioId} - {SelectedFormat.Audio}", LogTag.YTDLP);

                    AudioInfo.Visibility = Visibility.Visible;
                    var DecideFileSize = SelectedAudio.FileSize != null ?
                        SelectedAudio.FileSize : SelectedAudio.FileSizeApprox;
                    AudioInfo.Text = $"{DownloadSuggester.FormatFileSize(DecideFileSize)}";
                }

                CheckIsReadyToSave();
            }
        }

        private bool CheckIsReadyToSave(bool downloadFormat = false)
        {
            var video = ResolutionSelect.SelectedItem as ComboOption;
            var audio = AudioSelect.SelectedItem as ComboOption;
            var codecSelected = CodecSelect.SelectedItem != null;

            bool presetValid = SelectedFormat.Preset != null && SelectedFormat.Preset.Value != SettingsService.Presets[0].Value;

            bool videoValid =
                video?.FormatId is string videoText &&
                !videoText.Contains("no", StringComparison.OrdinalIgnoreCase) &&
                codecSelected;

            bool audioValid =
                audio?.FormatId is string audioText &&
                !audioText.Contains("no", StringComparison.OrdinalIgnoreCase);

            bool isReady = presetValid || videoValid || audioValid;

            IsUserCanSave?.Invoke(isReady);
            return isReady;
        }

        private void ThumbnailImageBorder_Loaded(object sender, RoutedEventArgs e)
        {
            var animation = new DoubleAnimation
            {
                From = 0.3,
                To = 0.8,
                Duration = TimeSpan.FromSeconds(0.8),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };

            Storyboard.SetTarget(animation, ThumbnailImageBorder);
            Storyboard.SetTargetProperty(animation, "Opacity");

            _loadingStoryboard = new Storyboard();
            _loadingStoryboard.Children.Add(animation);
            _loadingStoryboard.Begin();
        }

        private void ThumbnailImage_ImageOpened(object sender, RoutedEventArgs e)
        {
            _loadingStoryboard?.Stop();
            ThumbnailImageBorder.Opacity = 1;
        }
    }
}

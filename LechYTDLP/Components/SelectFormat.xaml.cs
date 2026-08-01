using CommunityToolkit.WinUI;
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
        public int Index { get; set; } = 0;
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
        // Main info
        public VideoInfo videoData = null!;
        // Selected format
        public ObservableCollection<MergedVideoFormat> MergedFormats { get; } = [];
        public ObservableCollection<FilteredVideoFormat> FilteredFormats { get; } = [];
        // Select format options
        public ObservableCollection<ComboOption> Presets { get; } = [];
        public ObservableCollection<ComboOption> Resolutions { get; } = [];
        public ObservableCollection<ComboOption> Codecs { get; } = [];
        public ObservableCollection<ComboOption> Audios { get; } = [];
        // Selected format
        public SelectedFormat SelectedFormat = new();
        public SelectedFormat[] SelectedFormats = [];

        // Etc.
        private Storyboard _loadingStoryboard = null!;
        public event Action<bool>? IsUserCanSave;

        // Dialog
        public string? Title { get; set; }

        // Flags
        private bool _ListViewInit = false;
        private bool _AllPresetWorking = false;

        public SelectFormat(VideoInfo info)
        {
            InitializeComponent();

            videoData = info;

            if (info.Type == InfoType.Video)
            {
                Title = App.LocalizationService.Get("SelectFormat");
                ThumbnailImage.Source = new BitmapImage(new Uri(info.Thumbnail ?? "https://placehold.co/320x180.png?text=No+Thumbnail"));

                VideoAltInfo.Text = $"{App.LocalizationService.Get("Saving")}: {SettingsService.DownloadPath}";
            }
            else if (info.Type == InfoType.Playlist)
            {
                Title = App.LocalizationService.Get("DownloadPlaylist");

                ThumbnailImage.Source = new BitmapImage(new Uri(info.Thumbnails?.Last()?.Url ?? "https://placehold.co/320x180.png?text=No+Thumbnail"));

                VideoAltInfo.Text = $"{videoData.Entries?.Length ?? 0} videos";
            }


            VideoTitle.Text = info.Title ?? App.LocalizationService.Get("UnknownTitle");

            VideoUploaderAndExtractor.Blocks.Clear();
            var p = new Paragraph();
            p.Inlines.Add(new Run { Text = $"@{info.Uploader}" ?? App.LocalizationService.Get("UnknownUploader") });
            p.Inlines.Add(new Run { Text = $" • {(info.ExtractorKey == "YoutubeTab" ? "YouTube" : info.ExtractorKey == "Youtube" ? "YouTube" : info.ExtractorKey)}" });
            VideoUploaderAndExtractor.Blocks.Add(p);


            // If the type is Video
            if (videoData.Type == InfoType.Video)
            {
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
            else if (videoData.Type == InfoType.Playlist)
            {
                PresetsGrid.Visibility = Visibility.Collapsed;
                VideoFormatGrid.Visibility = Visibility.Collapsed;
                AudioFormatGrid.Visibility = Visibility.Collapsed;

                PlaylistVideoSelectionGrid.Visibility = Visibility.Visible;
                VideoSelectionOptionsGrid.Visibility = Visibility.Visible;
                PlaylistOptionsBorder.Visibility = Visibility.Visible;

                _ListViewInit = true;

                // Adding Indexes to the title
                for (int i = 0; i < videoData.Entries?.Length; i++)
                {
                    videoData.Entries[i].Index = i;
                    videoData.Entries[i].Presets = Presets;
                }

                PlaylistVideoSelection.ItemsSource = videoData.Entries;

                foreach (var video in videoData.Entries!)
                {
                    video.SelectedPresetChanged += PlaylistVideoPresetChanged;
                }

                // Deselect all videos by default
                PlaylistVideoSelection.DeselectAll();
                _ListViewInit = false;
                SelectAllButton.Content = App.LocalizationService.Get("SelectAll");

                ChangeAllPresetsComboBox.ItemsSource = Presets;

                for (int i = 0; i < SettingsService.Presets.Count; i++)
                {
                    var preset = SettingsService.Presets[i];
                    if (preset.Value == "illchoose") continue;

                    Presets.Add(new ComboOption
                    {
                        FormatId = preset.Value,
                        Text = preset.DisplayName
                    });
                }
            }

        }

        private void PlaylistVideoPresetChanged(object? sender, EventArgs e)
        {
            if (_AllPresetWorking)
                return;

            ChangeAllPresetsComboBox.SelectedItem = null;
            CheckIsReadyToSave();
        }

        private void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox combo)
            {
                if (videoData.Type == InfoType.Video)
                {
                    CheckIsReadyToSave();

                    if (e.AddedItems.Count == 0) return;
                    var Selected = (ComboOption)e.AddedItems[0];
                    if (Selected == null) return;

                    if (combo.Name == "PresetSelect")
                    {
                        // If the preset is not "illchoose"
                        if (Selected.FormatId != SettingsService.Presets[0].Value)
                        {
                            VideoFormatGrid.Visibility = Visibility.Collapsed;
                            AudioFormatGrid.Visibility = Visibility.Collapsed;
                            FormatBorder.Visibility = Visibility.Collapsed;

                            SelectedFormat.Preset = SettingsService.Presets.FirstOrDefault(p => p.Value == Selected.FormatId);
                        }
                        // If the preset is "illchoose"
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
                else if (videoData.Type == InfoType.Playlist)
                {
                    // Sadece ListView içindeki ComboBox'lar için çalışsın
                    if (combo.Name == "PresetComboBox" && !_AllPresetWorking)
                    {
                        if (ChangeAllPresetsComboBox.SelectedItem != null) ChangeAllPresetsComboBox.SelectedItem = null;
                        CheckIsReadyToSave();
                        Debug.WriteLine("PresetComboBox selection changed in playlist context.");
                    }

                    if (e.AddedItems.Count == 0) return;
                    var Selected = (ComboOption)e.AddedItems[0];
                    if (Selected == null) return;

                    if (combo.Name == "ChangeAllPresetsComboBox")
                    {
                        _AllPresetWorking = true;
                        if (videoData.Entries == null) return;
                        foreach (var video in videoData.Entries)
                        {
                            video.SelectedPreset = Selected;
                        }
                        _AllPresetWorking = false;

                        CheckIsReadyToSave();
                    }
                }
            }
            else if (sender is ListView listView)
            {
                //if (e.AddedItems.Count == 0) return;
                if (_ListViewInit) return;

                if (listView.Name == "PlaylistVideoSelection")
                {
                    foreach (var selectedItem in e.AddedItems)
                    {
                        var Selected = (PlaylistVideoInfo)selectedItem;
                        Selected.IsSelectEnabled = true;
                        if (Selected.SelectedPreset == null)
                        {
                            // If no preset is selected, set the default preset (best quality video + audio)
                            Selected.SelectedPreset = Presets.FirstOrDefault(p => p.FormatId == Presets[0].FormatId);
                        }
                    }

                    foreach (var unselectedItem in e.RemovedItems)
                    {
                        var Unselected = (PlaylistVideoInfo)unselectedItem;
                        Unselected.IsSelectEnabled = false;
                    }

                    CheckIsReadyToSave();
                }
            }
        }

        private bool CheckIsReadyToSave(bool downloadFormat = false)
        {
            if (videoData.Type == InfoType.Video)
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

                if (isReady)
                {
                    SelectedFormats = [SelectedFormat];
                }

                IsUserCanSave?.Invoke(isReady);
                return isReady;
            }
            else if (videoData.Type == InfoType.Playlist)
            {
                bool atLeastOneSelected = videoData.Entries?.Any(v => v.IsSelectEnabled) ?? false;
                bool atLeastOnePresetSelected = videoData.Entries?.Any(v => v.IsSelectEnabled && v.SelectedPreset != null && v.SelectedPreset.FormatId != SettingsService.Presets[0].Value) ?? false;
                bool selectedFormatValid = videoData.Entries?.Any(v => v.IsSelectEnabled && v.SelectedPreset != null) ?? false;

                bool isReady = atLeastOneSelected && atLeastOnePresetSelected && selectedFormatValid;

                if (isReady && videoData.Entries != null)
                {
                    // We convert all selected presets to a list of Preset objects for saving
                    SelectedFormats = videoData.Entries
                        .Where(v => v.IsSelectEnabled && v.SelectedPreset != null)
                        .Select(v => new SelectedFormat
                        {
                            Preset = new Setting
                            {
                                DisplayName = v.SelectedPreset!.Text!,
                                Value = v.SelectedPreset!.FormatId!
                            },
                            Index = v.Index
                        })
                        .ToArray();
                }

                IsUserCanSave?.Invoke(isReady);
                return isReady;
            }
            else
            {
                IsUserCanSave?.Invoke(false);
                return false;
            }
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

        private void Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                if (button.Name == "SelectAllButton")
                {
                    if (PlaylistVideoSelection.SelectedItems.Count != PlaylistVideoSelection.Items.Count)
                    {
                        PlaylistVideoSelection.SelectAll();
                        SelectAllButton.Content = App.LocalizationService.Get("DeselectAll");
                    }
                    else
                    {
                        PlaylistVideoSelection.DeselectAll();
                        SelectAllButton.Content = App.LocalizationService.Get("SelectAll");
                    }

                    // If every video selected same preset, set the ChangeAllPresetsComboBox to that preset
                    if (videoData.Entries != null && videoData.Entries.All(v => v.SelectedPreset != null && v.SelectedPreset.FormatId == videoData.Entries[0].SelectedPreset?.FormatId))
                    {
                        ChangeAllPresetsComboBox.SelectedItem = videoData.Entries[0].SelectedPreset;
                    }
                }
            }
        }

        private void Dialog_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_loadingStoryboard != null)
            {
                _loadingStoryboard.Stop();
            }

            if (videoData.Entries != null)
            {
                foreach (var video in videoData.Entries)
                {
                    video.SelectedPresetChanged -= PlaylistVideoPresetChanged;
                }
            }
        }
    }
}

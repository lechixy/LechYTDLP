using LechYTDLP.Components;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LechYTDLP.Classes
{
    public class DownloadResult
    {
        public ResultCode Code { get; set; }
        public ResultReason Reason { get; set; } = ResultReason.None;
        public string Message { get; set; } = string.Empty;
    }

    public class VideoInfoResult
    {
        public ResultCode Code { get; set; }
        public ResultReason Reason { get; set; } = ResultReason.None;
        public string Message { get; set; } = string.Empty;
        public YtDlpData? VideoInfo { get; set; } = null;
    }

    public class YtDlpData
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
        [JsonPropertyName("playlist_count")]
        public int? PlaylistCount { get; set; }
        [JsonPropertyName("entries")]
        public PlaylistVideoInfo[]? Entries { get; set; }
        [JsonPropertyName("_type")]
        public InfoType Type { get; set; }
        public string BestThumbnailUrl
        {
            get
            {
                if (Type == InfoType.Video)
                {
                    return Thumbnail ?? string.Empty;
                }
                else if (Type == InfoType.Playlist)
                {
                    return Thumbnails?.LastOrDefault()?.Url ?? string.Empty;
                }

                return string.Empty;
            }
        }
    }

    [JsonConverter(typeof(JsonStringEnumConverter<InfoType>))]
    public enum InfoType
    {
        [JsonStringEnumMemberName("video")]
        Video,

        [JsonStringEnumMemberName("playlist")]
        Playlist,

        [JsonStringEnumMemberName("unknown")]
        Unknown
    }

    public class PlaylistVideoInfo : INotifyPropertyChanged
    {
        // INotify implementation
        public event PropertyChangedEventHandler? PropertyChanged;


        // About the video
        [JsonPropertyName("title")]
        public string? Title { get; set; }
        [JsonPropertyName("thumbnails")]
        public Thumbnail[]? Thumbnails { get; set; }
        public string? BestThumbnailUrl => Thumbnails?.LastOrDefault()?.Url;
        [JsonPropertyName("duration")]
        public double? Duration { get; set; }
        [JsonPropertyName("timestamp")]
        public long? Timestamp { get; set; }
        [JsonPropertyName("ie_key")]
        public string? IeKey { get; set; }
        [JsonPropertyName("id")]
        public string? Id { get; set; }
        [JsonPropertyName("url")]
        public string? Url { get; set; }

        // For binding
        private bool _isSelectEnabled = false;
        public bool IsSelectEnabled
        {
            get => _isSelectEnabled;
            set
            {
                if (_isSelectEnabled == value)
                    return;

                _isSelectEnabled = value;
                PropertyChanged?.Invoke(this,
                    new PropertyChangedEventArgs(nameof(IsSelectEnabled)));
            }
        }

        public ObservableCollection<ComboOption> Presets { get; set; } = [];
        private ComboOption? _selectedPreset;
        public ComboOption? SelectedPreset
        {
            get => _selectedPreset;
            set
            {
                if (_selectedPreset == value)
                    return;
                _selectedPreset = value;
                PropertyChanged?.Invoke(this,
                    new PropertyChangedEventArgs(nameof(SelectedPreset)));
                SelectedPresetChanged?.Invoke(this, EventArgs.Empty);
            }
        }


        public int Index { get; set; }
        public string NumberedTitle => $"{Index + 1} - {Title}";

        public event EventHandler? SelectedPresetChanged;
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

    [JsonSerializable(typeof(YtDlpData))]
    [JsonSerializable(typeof(Dictionary<string, JsonElement>))]
    public partial class AppJsonContext : JsonSerializerContext
    {
    }

    public enum ResultCode
    {
        Success = 0,
        Error = 1,
        Cancelled = 2
    }

    public enum ResultReason
    {
        None = 0,
        FailedToStartProcess = 1,
        NoVideoFound = 2,
        WantedPause = 3,
        WantedCancel = 4,
    }

    public class ProcessResult
    {
        public ResultCode Code { get; set; }
        public ResultReason Reason { get; set; }
        public string Message { get; set; } = string.Empty;
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
}

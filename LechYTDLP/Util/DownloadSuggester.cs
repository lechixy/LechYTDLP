using LechYTDLP.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LechYTDLP.Util
{
    public enum VideoCodec
    {
        AV1,
        VP9,
        AVC1
    }

    internal class DownloadSuggester
    {
        public static string FormatFileSize(long? bytes)
        {
            if (bytes == null) return "Unknown filesize";
            if (bytes < 0) return "—";
            if (bytes == 0) return "0 B";

            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = (long)bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }

            return len % 1 == 0
                ? $"{(int)len} {sizes[order]}"
                : $"{len:0.##} {sizes[order]}";
        }

        public static string FormatTextSuggestion(string VCodec)
        {
            // H.264 (AVC1) is widely supported
            if (Map(VCodec) == VideoCodec.AVC1) 
            {
                return "Most compatible • Larger file size";
            } else if (Map(VCodec) == VideoCodec.VP9) 
            {
                return "Moderately compatible • Medium file size";
            } 
            else if (Map(VCodec) == VideoCodec.AV1) 
            {
                return "Less compatible • Smaller file size";
            }

            return "unknown";
        }

        public static VideoFormat? SuggestBestFormat(MergedVideoFormat merged)
        {
            if (merged.Formats == null || merged.Formats.Length == 0)
                return null;

            return merged.Formats
                .Select(f => new
                {
                    Format = f,
                    Score = ScoreFormat(f, merged.Height ?? 0)
                })
                .OrderByDescending(x => x.Score)
                .First()
                .Format;
        }

        public static VideoCodec Map(string? codec)
        {
            if (codec == null) return VideoCodec.AVC1;

            if (codec.StartsWith("av01"))
                return VideoCodec.AV1;

            if (codec.StartsWith("vp9"))
                return VideoCodec.VP9;

            if (codec.StartsWith("avc1"))
                return VideoCodec.AVC1;

            return VideoCodec.AVC1;
        }

        private static int ScoreFormat(VideoFormat format, int height)
        {
            int score = 0;

            var codec = Map(format.VCodec);

            score += ScoreCodec(codec, height);
            score += ScoreFps((int?)Convert.ToInt64(format.Fps));
            score += ScoreFileSize(format.FileSize, height);

            return score;
        }


        private static int ScoreCodec(VideoCodec codec, int height)
        {
            return codec switch
            {
                VideoCodec.AV1 => height >= 1440 ? 45 : 25,
                VideoCodec.VP9 => height >= 1080 ? 35 : 25,
                VideoCodec.AVC1 => 20,
                _ => 0
            };
        }

        private static int ScoreFps(int? fps)
        {
            if (fps >= 60) return 15;
            if (fps >= 30) return 10;
            return 5;
        }

        private static int ScoreFileSize(long? sizeBytes, int height)
        {
            if (sizeBytes == null) return 0;

            long ideal =
                height >= 2160 ? 300_000_000 :
                height >= 1440 ? 180_000_000 :
                height >= 1080 ? 100_000_000 :
                50_000_000;

            if (sizeBytes <= ideal) return 20;
            if (sizeBytes <= ideal * 1.5) return 12;
            if (sizeBytes <= ideal * 2) return 6;
            return 2;
        }
    }
}

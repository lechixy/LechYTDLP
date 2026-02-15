using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;

namespace LechYTDLP.Util
{
    public class Main
    {
        public static string GetDynamicSearchBoxPlaceholder()
        {
            string[] placeholders = [
                "Paste video link here...",
                "Try me Beyonce... ✨",
                "I want to download that cute cat video I saw yesterday...",
                "Share me a playlist link...",
                "I bet you have some cool videos to download...",
                "Don't be shy, paste a link...",
                "Your video link goes here...",
                "Got a video in mind? Paste it here...",
                "Ready to download? Paste the link...",
                "I can download videos from over 1000 sites, try me with any link...",
                "Looking for something?..",
            ];

            Random rnd = new();
            int index = rnd.Next(placeholders.Length);

            return placeholders[index];
        }
        public static LinearGradientBrush GetAppGradient(string App)
        {
            if (App.Contains("instagram", StringComparison.OrdinalIgnoreCase))
            {
                //new LinearGradientBrush
                //{
                //    StartPoint = new Point(0, 0),
                //    EndPoint = new Point(1, 0),
                //    GradientStops = {
                //    new GradientStop { Color = Color.FromArgb(255, 193, 53, 132), Offset = 0.0 }, // mor
                //    new GradientStop { Color = Color.FromArgb(255, 253, 29, 29),  Offset = 0.5 }, // kırmızı
                //    new GradientStop { Color = Color.FromArgb(255, 252, 175, 69), Offset = 1.0 }  // sarı
                //}
                //};
                return new LinearGradientBrush
                {
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(1, 0),
                    GradientStops = {
                        new GradientStop { Color = Color.FromArgb(255, 64, 93, 230),  Offset = 0.0 },  // #405de6
                        new GradientStop { Color = Color.FromArgb(255, 91, 81, 216),  Offset = 0.12 }, // #5b51d8
                        new GradientStop { Color = Color.FromArgb(255, 131, 58, 180), Offset = 0.25 }, // #833ab4
                        new GradientStop { Color = Color.FromArgb(255, 193, 53, 132), Offset = 0.38 }, // #c13584
                        new GradientStop { Color = Color.FromArgb(255, 225, 48, 108), Offset = 0.50 }, // #e1306c
                        new GradientStop { Color = Color.FromArgb(255, 253, 29, 29),  Offset = 0.62 }, // #fd1d1d
                        new GradientStop { Color = Color.FromArgb(255, 245, 96, 64),  Offset = 0.74 }, // #f56040
                        new GradientStop { Color = Color.FromArgb(255, 247, 119, 55), Offset = 0.84 }, // #f77737
                        new GradientStop { Color = Color.FromArgb(255, 252, 175, 69), Offset = 0.92 }, // #fcaf45
                        new GradientStop { Color = Color.FromArgb(255, 255, 220, 128),Offset = 1.0 }   // #ffdc80
                    }
                };
            }
            else if (App.Contains("youtube", StringComparison.OrdinalIgnoreCase))
            {
                return new LinearGradientBrush
                {
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(1, 0),
                    GradientStops = {
                        new GradientStop { Color = Color.FromArgb(255, 255, 0, 0), Offset = 0.0 },
                        new GradientStop { Color = Color.FromArgb(255, 255, 0, 51), Offset = 1.0 }
                    }
                };
            }
            else if (App.Contains("tiktok", StringComparison.OrdinalIgnoreCase))
            {
                return new LinearGradientBrush
                {
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(1, 0),
                    GradientStops = {
                        new GradientStop { Color = Color.FromArgb(255, 255, 0, 80), Offset = 0.0 },
                        new GradientStop { Color = Color.FromArgb(255, 0, 242, 234), Offset = 1 },
                    }
                };
            }

            return new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 0),
                GradientStops = {
                    new GradientStop { Color = Color.FromArgb(255, 255, 255, 255), Offset = 0.0 },
                    new GradientStop { Color = Color.FromArgb(255, 255, 255, 255), Offset = 1.0 }
                }
            };
        }
    }
}

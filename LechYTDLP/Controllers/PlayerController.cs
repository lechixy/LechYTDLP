using LechYTDLP.Components;
using LechYTDLP.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Core;

namespace LechYTDLP.Controllers
{
    internal class PlayerController
    {
        public static async void PlayMediaItem(DownloadItem item)
        {
            var path = item.FilePath;

            var mediaPlayer = new LechMediaPlayer(path);
            var result = await App.DialogService.ShowAsync(new DialogOptions
            {
                Content = mediaPlayer,
                IsPlayerDialog = true,
            });

            mediaPlayer.Dispose();
        }
    }
}

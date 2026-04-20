using LechYTDLP.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LechYTDLP.Components
{
    public sealed partial class LechMediaPlayer : UserControl
    {
        public LechMediaPlayer(string filePath)
        {
            InitializeComponent();

            MediaPlayer.Source = MediaSource.CreateFromUri(new Uri(filePath));
            MediaPlayer.MediaPlayer.Volume = 0.1;
        }

        public void Dispose()
        {
            MediaPlayer.Source = null;
        }
    }
}

using LechYTDLP.Services;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
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
using static LechYTDLP.Classes.UpdateChecker;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LechYTDLP.Components
{
    public sealed partial class UpdateDialog : UserControl
    {
        public UpdateDialog(UpdateCheckResult result)
        {
            InitializeComponent();

            YTdlpVersionText.Blocks.Clear();
            var paragraph = new Paragraph();
            paragraph.Inlines.Add(new Run { Text = result.CurrentVersion, FontWeight = FontWeights.Bold, Foreground = (Brush)Application.Current.Resources["ControlStrongFillColorDefaultBrush"] });
            paragraph.Inlines.Add(new Run { Text = " > ", FontWeight = FontWeights.Bold });
            paragraph.Inlines.Add(new Run { Text = result.NewestVersion, FontWeight = FontWeights.Bold, Foreground = (Brush)Application.Current.Resources["AccentTextFillColorPrimaryBrush"] });
            YTdlpVersionText.Blocks.Add(paragraph);

            YTdlpUpdateNotice.Text = $"{App.LocalizationService.Get("YTdlpUpdateAvailableMsg")}";
            // YTdlpAutoUpdateCheckBox.IsChecked = SettingsService.YTdlpAutoUpdate;
        }

        private void CheckChanged(object sender, RoutedEventArgs e)
        {
            if (sender is not CheckBox checkBox) return;

            if (checkBox.IsChecked == true)
            {
                SettingsService.YTdlpAutoUpdate = true;
            }
            else
            {
                SettingsService.YTdlpAutoUpdate = false;
            }
        }
    }
}

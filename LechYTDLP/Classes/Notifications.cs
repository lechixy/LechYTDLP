using LechYTDLP.Services;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LechYTDLP.Classes
{
    static internal class Notifications
    {
        public static void ShowUpdateAvailableNotification(string newVersion)
        {
            if (!SettingsService.ShowYTdlpUpdateNotifications) return;

            try
            {
                AppNotification notification = new AppNotificationBuilder()
                .AddText(App.LocalizationService.Get("YTdlpUpdateAvailableNotificationTitle"))
                .AddText(App.LocalizationService.Get("UpdateAvailableNotificationMessage", newVersion))
                .SetAudioEvent(AppNotificationSoundEvent.IM)
                .BuildNotification();

                AppNotificationManager.Default.Show(notification);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}

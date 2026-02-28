using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.Globalization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.ApplicationModel.Resources.Core;
using static LechYTDLP.Views.SettingsPage;

namespace LechYTDLP.Services
{
    public class LocalizationService
    {
        private readonly ResourceLoader _loader;

        private string _defaultValue = "No translation";

        public LocalizationService()
        {
            _loader = ResourceLoader.GetForViewIndependentUse();
        }

        public string Get(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return _defaultValue;

            string value = _loader.GetString(key);
            return string.IsNullOrEmpty(value) ? _defaultValue : value;
        }

        public string GetString(string key, params object[] args)
        {
            if (string.IsNullOrWhiteSpace(key))
                return _defaultValue;

            string value = _loader.GetString(key);
            if (string.IsNullOrEmpty(value))
                value = _defaultValue;

            if (args != null && args.Length > 0)
                value = string.Format(value, args);

            return value;
        }

        public static string SetLanguage(LanguageItem? language)
        {
            if (language == null)
            {
                SettingsService.ResetSetting(nameof(SettingsService.AppLanguage));
                return "system";
            }

            App.InfoBarService.Show(new InfoBarMessage
            {
                Title = App.LocalizationService.GetString("LanguageChangedInfoBarMsg", language.DisplayName),
                Message = "",
                Severity = InfoBarSeverity.Success,
                DurationMs = 3000,
                IsCancelable = false
            });
            SettingsService.AppLanguage = language;
            return language.Code;
        }
    }
}

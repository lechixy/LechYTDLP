using LechYTDLP.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.AppLifecycle;
using Microsoft.Windows.Globalization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private ResourceLoader _loader;

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

        public string Get(string key, params object[] args)
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

        public static void SetDefaultLanguageBasedOnSystem()
        {
            // If the default system language code is not set, set it to the current system language
            if (SettingsService._DefaultSystemLanguageCode == null)
            {
                var systemLanguageCode = Windows.Globalization.ApplicationLanguages.Languages[0];
                SettingsService._DefaultSystemLanguageCode = systemLanguageCode;
                Debug.WriteLine($"Default system language code is not set. Setting it to the current system language: {systemLanguageCode}");
            }
        }

        public static string SetLanguage(Setting language)
        {
            if (language.Value == "system")
            {
                var defaultSystemLanguageCode = SettingsService._DefaultSystemLanguageCode;
                App.InfoBarService.Show(new InfoBarMessage
                {
                    Title = App.LocalizationService.Get("LanguageChangedInfoBar", App.LocalizationService.Get("System")),
                    Message = App.LocalizationService.Get("LanguageChangedInfoBarMsg"),
                    Severity = InfoBarSeverity.Success,
                    DurationMs = 3000,
                    IsCancelable = false
                });

                var languageAvailable = SettingsService.Languages.FirstOrDefault(l => l.Value == defaultSystemLanguageCode);
                // If the current language is not in the available languages, fallback to system default
                if (languageAvailable == null)
                {
                    Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = string.Empty;
                    SettingsService.ResetSetting(nameof(SettingsService.AppLanguage));
                    Debug.WriteLine($"Current language '{defaultSystemLanguageCode}' is not in the available languages. Falling back to system default.");
                    OnLanguageChanged();
                    return "system";
                }

                // If the current language is in the available languages, set it as the primary language override
                Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = languageAvailable.Value;
                SettingsService.AppLanguage = languageAvailable;
                Debug.WriteLine($"Current language '{defaultSystemLanguageCode}' is set as the primary language override.");
				OnLanguageChanged();
				return languageAvailable.Value;
            }

            App.InfoBarService.Show(new InfoBarMessage
            {
                Title = App.LocalizationService.Get("LanguageChangedInfoBar", language.DisplayName),
                Message = App.LocalizationService.Get("LanguageChangedInfoBarMsg"),
                Severity = InfoBarSeverity.Success,
                DurationMs = 3000,
                IsCancelable = false
            });
            Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = language.Value;
            SettingsService.AppLanguage = language;
            OnLanguageChanged();
			return language.Value;
        }

		private static async void OnLanguageChanged()
		{
			var result = await App.DialogService.ShowAsync(new DialogOptions
			{
				Title = App.LocalizationService.Get("DialogLanguageChanged"),
				Message = App.LocalizationService.Get("DialogLanguageChangedNotice"),
				PrimaryButtonText = App.LocalizationService.Get("Restart"),
                PrimaryButtonStyle = Application.Current.Resources["AccentButtonStyle"] as Style,
				CloseButtonText = App.LocalizationService.Get("Later")
			});

			if (result == DialogResult.Primary)
			{
                AppInstance.Restart("");
			}
		}
	}
}

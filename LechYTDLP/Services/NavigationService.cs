using LechYTDLP.Views;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LechYTDLP.Services
{
    public interface INavigationService
    {
        void Initialize(Frame frame, NavigationView navView);
        void Navigate<T>(object? parameter = null) where T : Page;
        void GoBack();
        bool CanGoBack { get; }
    }


    public class NavigationService : INavigationService
    {
        private Frame? _frame;
        private NavigationView? _navView;

        // Page ↔ NavItem eşlemesi
        private readonly Dictionary<Type, string> _pageKeyMap = new()
    {
        { typeof(MainPage), "MainPage" },
        { typeof(DownloadsPage), "DownloadsPage" },
        { typeof(SettingsPage), "Settings"  },
        { typeof(OptionsPage), "OptionsPage" },
        { typeof(LogPage), "LogPage" },
    };

        public void Initialize(Frame frame, NavigationView navView)
        {
            _frame = frame;
            _navView = navView;
            // Set initial page
            _navView.SelectedItem = _navView.MenuItems
                .OfType<NavigationViewItem>()
                .FirstOrDefault(i => i.Tag?.ToString() == "MainPage");  

            _frame.Navigated += OnNavigated;
        }

        public bool CanGoBack => _frame?.CanGoBack ?? false;

        public void GoBack()
        {
            if (CanGoBack)
                _frame!.GoBack();
        }

        public void Navigate<T>(object? parameter = null) where T : Page
        {
            _frame?.Navigate(typeof(T), parameter);
        }
        private void OnNavigated(object sender, NavigationEventArgs e)
        {
            if (_navView == null || e.SourcePageType == null)
                return;

            if (_pageKeyMap.TryGetValue(e.SourcePageType, out var key))
            {
                var item = _navView.MenuItems
                    .OfType<NavigationViewItem>()
                    .FirstOrDefault(i => i.Tag?.ToString() == key);

                if (item != null)
                    _navView.SelectedItem = item;
            }
        }
    }
}

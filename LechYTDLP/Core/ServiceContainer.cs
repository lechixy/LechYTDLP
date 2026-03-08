using LechYTDLP.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace LechYTDLP.Core
{
    public static class ServiceContainer
    {
        public static IServiceCollection Collection = null!;
        public static IServiceProvider Services { get; private set; } = null!;

        public static void Configure()
        {
            Collection = new ServiceCollection();

            Collection.AddSingleton<SettingsService>();
            Collection.AddSingleton<DatabaseService>();
            Collection.AddSingleton<DownloadsService>();
            Collection.AddSingleton<InfoBarService>();
            Collection.AddSingleton<LocalizationService>();
            Collection.AddSingleton<LogService>();
            Collection.AddSingleton<NavigationService>();
            Collection.AddSingleton<SettingsService>();
            Collection.AddSingleton<ToolPathService>();

            Services = Collection.BuildServiceProvider();

        }
#pragma warning disable CS8603 // Possible null reference return.
        public static T Get<T>() => Services.GetService<T>();
#pragma warning restore CS8603 // Possible null reference return.
    }
}

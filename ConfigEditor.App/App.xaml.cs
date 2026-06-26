using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using ConfigEditor.App.Services;
using ConfigEditor.App.ViewModels;
using ConfigEditor.Core.Interfaces;
using ConfigEditor.Formats;
using ConfigEditor.Infrastructure;

namespace ConfigEditor.App;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;

    public App()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Core/Infrastructure Services
        services.AddSingleton<LogService>();
        services.AddSingleton<IFileService, FileService>();
        services.AddSingleton<IBackupService, BackupService>();

        // App Services
        services.AddSingleton<AppSettingsService>();
        services.AddSingleton<RecentFileService>();
        services.AddSingleton<DialogService>();
        services.AddSingleton<ConfigParserFactory>();

        // ViewModels
        services.AddSingleton<MainViewModel>();

        // Main Window View
        services.AddSingleton<MainWindow>();
    }

    public void ChangeTheme(string themeName)
    {
        try
        {
            var themeUri = new Uri($"Theme/{themeName}Theme.xaml", UriKind.Relative);
            // Replace the resource dictionary at index 0 (which is the color palette dictionary)
            Resources.MergedDictionaries[0] = new ResourceDictionary { Source = themeUri };
        }
        catch
        {
            // Fail silently or fallback
        }
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var logService = _serviceProvider.GetRequiredService<LogService>();
        logService.LogInfo("Application starting...");

        var appSettings = _serviceProvider.GetRequiredService<AppSettingsService>();
        ChangeTheme(appSettings.Settings.Theme);

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }
}

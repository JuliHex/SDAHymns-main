using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SDAHymns.Core.Data;
using SDAHymns.Core.Services;
using SDAHymns.Desktop.ViewModels;
using SDAHymns.Desktop.Views;
using Velopack;

namespace SDAHymns.Desktop;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Velopack startup hook - MUST be called first
        VelopackApp.Build().Run();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit.
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();

            // Setup Dependency Injection
            var services = new ServiceCollection();

            // Database - locate hymns.db in Resources folder
            var dbPath = Path.Combine(AppContext.BaseDirectory, "Resources", "hymns.db");
            services.AddDbContext<HymnsContext>(options =>
            {
                options.UseSqlite($"Data Source={dbPath}");
            });

            // Configure update options
            services.Configure<SDAHymns.Core.Services.UpdateOptions>(options =>
            {
                options.GitHubRepoUrl = "https://github.com/ThorSPB/SDAHymns";
            });

            // Services
            services.AddScoped<IHymnDisplayService, HymnDisplayService>();
            services.AddScoped<ISearchService, SearchService>();
            services.AddScoped<IDisplayProfileService, DisplayProfileService>();
            services.AddSingleton<IUpdateService, UpdateService>();
            services.AddSingleton<IHotKeyManager, HotKeyManager>();
            services.AddSingleton<IAudioPlayerService, AudioPlayerService>();
            services.AddScoped<ISettingsService, SettingsService>();
            services.AddScoped<IAudioLibraryService, AudioLibraryService>();
            services.AddScoped<IAudioDownloadService, AudioDownloadService>();
            services.AddSingleton<HttpClient>();  // For AudioDownloadService

            // ViewModels
            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<SettingsWindowViewModel>();
            services.AddTransient<RemoteWidgetViewModel>();

            _serviceProvider = services.BuildServiceProvider();

            // Check launch mode - default to RemoteWidget
            var args = Environment.GetCommandLineArgs();
            bool advancedMode = args.Contains("--advanced");

            if (advancedMode)
            {
                // Launch full MainWindow in advanced mode
                var hotKeyManager = _serviceProvider.GetRequiredService<IHotKeyManager>();
                var mainWindow = new MainWindow(hotKeyManager)
                {
                    DataContext = _serviceProvider.GetRequiredService<MainWindowViewModel>()
                };

                // Provide service provider to main window for creating child windows
                mainWindow.SetServiceProvider(_serviceProvider);

                desktop.MainWindow = mainWindow;
            }
            else
            {
                // Launch compact RemoteWidget (default)
                var remoteWidget = new RemoteWidget
                {
                    DataContext = _serviceProvider.GetRequiredService<RemoteWidgetViewModel>()
                };

                // Provide service provider for creating child windows
                remoteWidget.SetServiceProvider(_serviceProvider);

                desktop.MainWindow = remoteWidget;
            }

            var mainWin = desktop.MainWindow;

            // Check for updates in background (non-blocking)
            Task.Run(async () =>
            {
                // Create a proper DI scope for the background task
                await using var scope = _serviceProvider.CreateAsyncScope();
                var logger = scope.ServiceProvider.GetService<ILogger<App>>();

                try
                {
                    var updateService = scope.ServiceProvider.GetRequiredService<IUpdateService>();
                    var updateInfo = await updateService.CheckForUpdatesAsync();

                    if (updateInfo != null)
                    {
                        // Dispatch to UI thread to show notification
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            // Handle both RemoteWidget (default) and MainWindow (advanced mode)
                            if (mainWin.DataContext is MainWindowViewModel mainViewModel)
                            {
                                mainViewModel.ShowUpdateNotification(updateInfo);
                            }
                            else if (mainWin is RemoteWidget && mainWin.DataContext is RemoteWidgetViewModel)
                            {
                                // For RemoteWidget, show a simple status message
                                // (full update UI would be in MainWindow)
                                // TODO: Add update notification to RemoteWidget status bar
                                // For now, we'll just log it - user can see updates in advanced mode
                                logger?.LogInformation("Update available: {Version}", updateInfo.TargetFullRelease.Version);
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    // Log the error but don't crash the app
                    logger?.LogWarning(ex, "Background update check failed during app startup");
                }
            });
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}

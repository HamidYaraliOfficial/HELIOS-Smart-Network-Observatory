using Helios.UI.Services;
using Microsoft.UI.Xaml;
using Microsoft.Extensions.DependencyInjection;

namespace Helios.UI;

/// <summary>
/// Application entry point. Wires up dependency injection for the entire
/// HELIOS engine stack (discovery, topology, telemetry, rules, storage, AI,
/// security, simulation) and hosts the single MainWindow shell.
/// </summary>
public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;
    public static Window? MainAppWindow { get; private set; }

    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var services = new ServiceCollection();
        AppServiceRegistration.Register(services);
        Services = services.BuildServiceProvider();

        var themeService = Services.GetRequiredService<ThemeService>();
        var localizationService = Services.GetRequiredService<LocalizationService>();
        themeService.ApplyPersistedTheme();
        localizationService.ApplyPersistedLanguage();

        MainAppWindow = new MainWindow();
        MainAppWindow.Activate();
    }
}

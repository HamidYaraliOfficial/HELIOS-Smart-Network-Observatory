using Helios.Alerts;
using Helios.Analytics;
using Helios.Core.Interfaces;
using Helios.Discovery;
using Helios.Events;
using Helios.Graph;
using Helios.Infrastructure;
using Helios.Networking.Discovery;
using Helios.Rules;
using Helios.Security;
using Helios.Simulation;
using Helios.Storage;
using Helios.Telemetry;
using Helios.Topology;
using Helios.Traffic;
using Helios.UI.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Helios.UI;

/// <summary>Central composition root - every subsystem is registered here as a singleton
/// so the whole app shares one live graph, one alert center, one job queue, etc.</summary>
public static class AppServiceRegistration
{
    public static void Register(IServiceCollection services)
    {
        // Storage
        var dbPath = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Helios", "helios.db");
        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(dbPath)!);
        services.AddSingleton(_ => HeliosDbContext.CreateAndEnsureReady(dbPath));
        services.AddSingleton(_ => new RetentionManager(HeliosDbContext.CreateAndEnsureReady(dbPath)));

        // Networking / Discovery
        services.AddSingleton<INetworkDiscoveryEngine, NetworkDiscoveryEngine>();
        services.AddSingleton<GraphEngine>();
        services.AddSingleton<ITopologyEngine>(sp => sp.GetRequiredService<GraphEngine>());
        services.AddSingleton<DiscoveryOrchestrator>();
        services.AddSingleton<GraphViewState>();
        services.AddSingleton<GraphFocusEngine>();

        // Telemetry / Traffic / Events
        services.AddSingleton<TrafficObservationEngine>();
        services.AddSingleton<ProtocolStatisticsService>();
        services.AddSingleton<TrafficPatternAnalyzer>();
        services.AddSingleton<BaselineEngine>();
        services.AddSingleton<AnomalyEngine>();
        services.AddSingleton<RealTimeTrafficAggregator>();
        services.AddSingleton<EventCorrelationEngine>();
        services.AddSingleton<NetworkTimelineService>();

        // Health / Rules / Alerts
        services.AddSingleton<IHealthEngine, HealthEngine>();
        services.AddSingleton<IAlertService, AlertCenterService>();
        services.AddSingleton<IRuleEngine, RuleEngine>();
        services.AddSingleton<IncidentWorkspaceService>();
        services.AddSingleton<NotificationService>();
        services.AddSingleton<MaintenanceModeService>();
        services.AddSingleton<MonitoringScheduleService>();

        // Simulation / Security / Infra
        services.AddSingleton<ScenarioSimulator>();
        services.AddSingleton<CredentialVault>();
        services.AddSingleton<AuditLogService>();
        services.AddSingleton<SecurityCenterService>();
        services.AddSingleton<BackgroundJobCenter>();
        services.AddSingleton<MetricsSelfMonitor>();
        services.AddSingleton<AvailabilityEstimator>();
        services.AddSingleton<SubnetExplorerService>();

        // UI services
        services.AddSingleton<ThemeService>();
        services.AddSingleton<LocalizationService>();
        services.AddSingleton<MainViewModel>();
    }
}

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Helios.Core.Enums;
using Helios.Core.Interfaces;
using Helios.Core.Models;
using Helios.Discovery;
using Helios.Graph;
using Helios.Infrastructure;
using Helios.Rules;
using Helios.Topology;
using Helios.Topology.Layouts;

namespace Helios.UI.ViewModels;

/// <summary>
/// Root view model bound by MainWindow. Coordinates discovery jobs, keeps the
/// observable Nodes/Edges/Alerts collections in sync with the live engines,
/// and exposes the Monitoring Schedule status shown in the status bar.
/// </summary>
public sealed partial class MainViewModel : ObservableObject
{
    private readonly GraphEngine _graph;
    private readonly GraphViewState _viewState;
    private readonly DiscoveryOrchestrator _discoveryOrchestrator;
    private readonly BackgroundJobCenter _jobCenter;
    private readonly IAlertService _alertService;
    private readonly IHealthEngine _healthEngine;
    private readonly MonitoringScheduleService _scheduleService;
    private readonly ForceDirectedLayout _forceLayout = new();

    public ObservableCollection<NetworkNode> Nodes { get; } = new();
    public ObservableCollection<NetworkEdge> Edges { get; } = new();
    public ObservableCollection<Alert> ActiveAlerts { get; } = new();

    [ObservableProperty] private NetworkNode? selectedNode;
    [ObservableProperty] private string cidrRangeInput = "192.168.1.0/24";
    [ObservableProperty] private bool isDiscoveryRunning;
    [ObservableProperty] private string scheduleStatusText = string.Empty;

    public MonitoringSchedule ActiveSchedule { get; } = new() { Name = "Default", Enabled = false };

    public MainViewModel(
        GraphEngine graph, GraphViewState viewState, DiscoveryOrchestrator discoveryOrchestrator,
        BackgroundJobCenter jobCenter, IAlertService alertService, IHealthEngine healthEngine,
        MonitoringScheduleService scheduleService)
    {
        _graph = graph;
        _viewState = viewState;
        _discoveryOrchestrator = discoveryOrchestrator;
        _jobCenter = jobCenter;
        _alertService = alertService;
        _healthEngine = healthEngine;
        _scheduleService = scheduleService;

        _graph.NodeChanged += node => App.MainAppWindow?.DispatcherQueue.TryEnqueue(() => UpsertNode(node));
        _graph.EdgeChanged += edge => App.MainAppWindow?.DispatcherQueue.TryEnqueue(() => UpsertEdge(edge));

        RefreshScheduleStatus();
    }

    [RelayCommand]
    private void StartDiscovery()
    {
        IsDiscoveryRunning = true;
        _jobCenter.Enqueue(JobKind.Discovery, $"Discover {CidrRangeInput}", async token =>
        {
            var scope = new DiscoveryScope { CidrRange = CidrRangeInput };
            await _discoveryOrchestrator.RunAsync(scope, null, token);
            App.MainAppWindow?.DispatcherQueue.TryEnqueue(() => IsDiscoveryRunning = false);
        });
    }

    [RelayCommand]
    private void RunLayout()
    {
        _forceLayout.Apply(Nodes.ToList(), Edges.ToList(), 1400, 900);
    }

    [RelayCommand]
    private void SelectNode(NetworkNode node)
    {
        SelectedNode = node;
        var health = _healthEngine.Evaluate(node, out var reason);
        node.Health = health;
        node.HealthReason = reason;
    }

    public void RefreshScheduleStatus()
    {
        var status = _scheduleService.Evaluate(ActiveSchedule);
        ScheduleStatusText = status.IsCurrentlyActive
            ? $"Active - closes in {Format(status.TimeUntilNextTransition)}"
            : $"Inactive - opens in {Format(status.TimeUntilNextTransition)}";
    }

    private static string Format(TimeSpan? span)
    {
        if (span is null) return "unknown";
        var s = span.Value;
        return s.TotalHours >= 1 ? $"{(int)s.TotalHours}h {s.Minutes}m" : $"{s.Minutes}m";
    }

    private void UpsertNode(NetworkNode node)
    {
        var existing = Nodes.FirstOrDefault(n => n.Id == node.Id);
        if (existing is null) Nodes.Add(node);
    }

    private void UpsertEdge(NetworkEdge edge)
    {
        var existing = Edges.FirstOrDefault(e => e.Id == edge.Id);
        if (existing is null) Edges.Add(edge);
    }
}

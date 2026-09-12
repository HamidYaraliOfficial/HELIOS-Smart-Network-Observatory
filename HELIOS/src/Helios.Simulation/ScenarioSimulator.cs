using Helios.Core.Enums;
using Helios.Core.Models;

namespace Helios.Simulation;

public enum ScenarioActionKind { NodeDown, NodeDegradedLatency, EdgeRemoved, EdgeLatencyIncrease }

public sealed class ScenarioAction
{
    public ScenarioActionKind Kind { get; set; }
    public Guid TargetNodeId { get; set; }
    public Guid? TargetEdgeId { get; set; }
    public double? SimulatedLatencyMs { get; set; }
}

public sealed class ScenarioResult
{
    public const string SimulationBanner = "SIMULATION - no changes were applied to the live network or its configuration.";
    public List<NetworkNode> ProjectedAffectedNodes { get; set; } = new();
    public string Summary { get; set; } = string.Empty;
}

/// <summary>
/// Runs "what-if" scenarios entirely against an in-memory clone of the graph.
/// The live topology, storage and rules are never touched - every result is
/// clearly labelled SIMULATION so it can never be confused with real data.
/// </summary>
public sealed class ScenarioSimulator
{
    public ScenarioResult Run(
        IReadOnlyList<NetworkNode> liveNodes, IReadOnlyList<NetworkEdge> liveEdges, ScenarioAction action)
    {
        // Deep-ish clone so the simulation cannot mutate live objects by reference.
        var clonedNodes = liveNodes.Select(Clone).ToList();
        var clonedEdges = liveEdges.Select(CloneEdge).ToList();

        var target = clonedNodes.FirstOrDefault(n => n.Id == action.TargetNodeId);
        if (target is null)
            return new ScenarioResult { Summary = "Target node not found in current topology." };

        switch (action.Kind)
        {
            case ScenarioActionKind.NodeDown:
                target.IsOnline = false;
                target.Health = HealthState.Critical;
                break;
            case ScenarioActionKind.NodeDegradedLatency:
                target.LastLatencyMs = action.SimulatedLatencyMs ?? 999;
                target.Health = HealthState.Degraded;
                break;
        }

        var analyzer = new Topology.DependencyImpactAnalyzer();
        var impact = analyzer.Analyze(clonedNodes, clonedEdges, target.Id);

        return new ScenarioResult
        {
            ProjectedAffectedNodes = impact.DirectlyDependentNodes.Concat(impact.TransitivelyDependentNodes).ToList(),
            Summary = $"{ScenarioResult.SimulationBanner} If '{target.DisplayName}' experiences this condition, " +
                      $"{impact.DirectlyDependentNodes.Count} directly dependent and " +
                      $"{impact.TransitivelyDependentNodes.Count} transitively dependent components may be affected."
        };
    }

    private static NetworkNode Clone(NetworkNode n) => new()
    {
        Id = n.Id, DisplayName = n.DisplayName, Kind = n.Kind, Hostname = n.Hostname,
        IPv4Address = n.IPv4Address, IsOnline = n.IsOnline, Health = n.Health,
        LastLatencyMs = n.LastLatencyMs, SegmentId = n.SegmentId
    };

    private static NetworkEdge CloneEdge(NetworkEdge e) => new()
    {
        Id = e.Id, SourceNodeId = e.SourceNodeId, TargetNodeId = e.TargetNodeId,
        ViewKind = e.ViewKind, IsDependency = e.IsDependency
    };
}

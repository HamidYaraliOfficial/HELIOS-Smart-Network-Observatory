using Helios.Core.Enums;
using Helios.Core.Models;

namespace Helios.Topology;

public sealed class ImpactEstimate
{
    public NetworkNode SourceNode { get; init; } = null!;
    public List<NetworkNode> DirectlyDependentNodes { get; set; } = new();
    public List<NetworkNode> TransitivelyDependentNodes { get; set; } = new();
    public string Disclaimer { get; } =
        "This is an Observed Dependency / Impact Estimate derived from modeled relationships, not a guaranteed outage forecast.";
}

/// <summary>
/// Walks the Dependency graph view to determine what else might be affected if
/// a given node degrades. Always frames output as an estimate, never a certainty.
/// </summary>
public sealed class DependencyImpactAnalyzer
{
    public ImpactEstimate Analyze(IReadOnlyList<NetworkNode> nodes, IReadOnlyList<NetworkEdge> edges, Guid nodeId)
    {
        var source = nodes.First(n => n.Id == nodeId);
        var estimate = new ImpactEstimate { SourceNode = source };

        var dependencyEdges = edges.Where(e => e.IsDependency).ToList();

        var direct = dependencyEdges
            .Where(e => e.TargetNodeId == nodeId)
            .Select(e => nodes.FirstOrDefault(n => n.Id == e.SourceNodeId))
            .Where(n => n is not null)
            .Cast<NetworkNode>()
            .ToList();

        estimate.DirectlyDependentNodes = direct;

        var visited = new HashSet<Guid>(direct.Select(n => n.Id)) { nodeId };
        var frontier = new Queue<Guid>(direct.Select(n => n.Id));
        var transitive = new List<NetworkNode>();

        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();
            var upstream = dependencyEdges.Where(e => e.TargetNodeId == current);
            foreach (var edge in upstream)
            {
                if (visited.Contains(edge.SourceNodeId)) continue;
                visited.Add(edge.SourceNodeId);
                var node = nodes.FirstOrDefault(n => n.Id == edge.SourceNodeId);
                if (node is not null)
                {
                    transitive.Add(node);
                    frontier.Enqueue(node.Id);
                }
            }
        }

        estimate.TransitivelyDependentNodes = transitive;
        return estimate;
    }
}

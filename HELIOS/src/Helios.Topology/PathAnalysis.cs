using Helios.Core.Enums;
using Helios.Core.Models;

namespace Helios.Topology;

public sealed record PathHop(NetworkNode Node, NetworkEdge? IncomingEdge, double CumulativeLatencyMs);

public sealed class PathAnalysisResult
{
    public bool PathFound { get; set; }
    public List<PathHop> Hops { get; set; } = new();
    public double TotalLatencyMs { get; set; }
}

/// <summary>
/// Dijkstra-based shortest observed path between two nodes, weighted by observed
/// latency where available (falls back to hop count).
/// </summary>
public sealed class PathAnalysisTool
{
    public PathAnalysisResult FindPath(
        IReadOnlyList<NetworkNode> nodes, IReadOnlyList<NetworkEdge> edges,
        Guid fromNodeId, Guid toNodeId, GraphViewKind view)
    {
        var distances = nodes.ToDictionary(n => n.Id, _ => double.PositiveInfinity);
        var previous = new Dictionary<Guid, (Guid NodeId, NetworkEdge Edge)?>();
        var visited = new HashSet<Guid>();

        distances[fromNodeId] = 0;
        var priorityQueue = new SortedSet<(double Dist, Guid Id)>(Comparer<(double, Guid)>.Create((a, b) =>
            a.Item1 != b.Item1 ? a.Item1.CompareTo(b.Item1) : a.Item2.CompareTo(b.Item2)));
        priorityQueue.Add((0, fromNodeId));

        while (priorityQueue.Count > 0)
        {
            var (dist, current) = priorityQueue.Min;
            priorityQueue.Remove(priorityQueue.Min);
            if (!visited.Add(current)) continue;
            if (current == toNodeId) break;

            var outgoing = edges.Where(e => e.ViewKind == view && (e.SourceNodeId == current || e.TargetNodeId == current));
            foreach (var edge in outgoing)
            {
                var neighborId = edge.SourceNodeId == current ? edge.TargetNodeId : edge.SourceNodeId;
                if (visited.Contains(neighborId)) continue;

                double weight = edge.ObservedLatencyMs ?? 1.0;
                double newDist = dist + weight;
                if (newDist < distances.GetValueOrDefault(neighborId, double.PositiveInfinity))
                {
                    distances[neighborId] = newDist;
                    previous[neighborId] = (current, edge);
                    priorityQueue.Add((newDist, neighborId));
                }
            }
        }

        var result = new PathAnalysisResult();
        if (!distances.TryGetValue(toNodeId, out var totalDist) || double.IsPositiveInfinity(totalDist))
            return result;

        var chain = new List<Guid> { toNodeId };
        var cursor = toNodeId;
        while (previous.TryGetValue(cursor, out var prev) && prev is not null)
        {
            chain.Add(prev.Value.NodeId);
            cursor = prev.Value.NodeId;
        }
        chain.Reverse();

        result.PathFound = true;
        result.TotalLatencyMs = totalDist;

        double running = 0;
        foreach (var nodeId in chain)
        {
            var node = nodes.First(n => n.Id == nodeId);
            NetworkEdge? incoming = previous.TryGetValue(nodeId, out var p) && p is not null ? p.Value.Edge : null;
            if (incoming?.ObservedLatencyMs is { } lat) running += lat;
            result.Hops.Add(new PathHop(node, incoming, running));
        }

        return result;
    }
}

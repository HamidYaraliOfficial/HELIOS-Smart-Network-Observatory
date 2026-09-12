using Helios.Core.Models;

namespace Helios.Topology.Layouts;

/// <summary>
/// Simple breadth-first layered layout: picks the highest-degree node (or the
/// first router/gateway found) as the root and arranges nodes into horizontal
/// tiers by BFS depth - good for showing a clear "core to edge" hierarchy.
/// </summary>
public sealed class HierarchicalLayout
{
    public void Apply(IReadOnlyList<NetworkNode> nodes, IReadOnlyList<NetworkEdge> edges, double width, double rowHeight = 140)
    {
        if (nodes.Count == 0) return;

        var adjacency = BuildAdjacency(nodes, edges);
        var root = nodes.OrderByDescending(n => adjacency[n.Id].Count).First();

        var depth = new Dictionary<Guid, int> { [root.Id] = 0 };
        var queue = new Queue<Guid>();
        queue.Enqueue(root.Id);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var neighbor in adjacency[current])
            {
                if (depth.ContainsKey(neighbor)) continue;
                depth[neighbor] = depth[current] + 1;
                queue.Enqueue(neighbor);
            }
        }

        int maxDepth = depth.Values.DefaultIfEmpty(0).Max();
        foreach (var node in nodes.Where(n => !depth.ContainsKey(n.Id)))
            depth[node.Id] = maxDepth + 1;

        var byDepth = nodes.GroupBy(n => depth[n.Id]).OrderBy(g => g.Key);
        foreach (var tier in byDepth)
        {
            var items = tier.ToList();
            double spacing = width / (items.Count + 1);
            for (int i = 0; i < items.Count; i++)
            {
                items[i].CanvasX = spacing * (i + 1);
                items[i].CanvasY = tier.Key * rowHeight + 60;
            }
        }
    }

    private static Dictionary<Guid, HashSet<Guid>> BuildAdjacency(IReadOnlyList<NetworkNode> nodes, IReadOnlyList<NetworkEdge> edges)
    {
        var adjacency = nodes.ToDictionary(n => n.Id, _ => new HashSet<Guid>());
        foreach (var edge in edges)
        {
            if (adjacency.ContainsKey(edge.SourceNodeId)) adjacency[edge.SourceNodeId].Add(edge.TargetNodeId);
            if (adjacency.ContainsKey(edge.TargetNodeId)) adjacency[edge.TargetNodeId].Add(edge.SourceNodeId);
        }
        return adjacency;
    }
}

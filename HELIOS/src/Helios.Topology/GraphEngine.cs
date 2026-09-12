using System.Collections.Concurrent;
using Helios.Core.Enums;
using Helios.Core.Interfaces;
using Helios.Core.Models;

namespace Helios.Topology;

/// <summary>
/// In-memory, thread-safe topology graph. Backed durably by Helios.Storage;
/// this class is the fast working copy used by the UI, Rule Engine and
/// Analytics layers.
/// </summary>
public sealed class GraphEngine : ITopologyEngine
{
    private readonly ConcurrentDictionary<Guid, NetworkNode> _nodes = new();
    private readonly ConcurrentDictionary<Guid, NetworkEdge> _edges = new();

    public IReadOnlyCollection<NetworkNode> Nodes => _nodes.Values.ToList();
    public IReadOnlyCollection<NetworkEdge> Edges => _edges.Values.ToList();

    public event Action<NetworkNode>? NodeChanged;
    public event Action<NetworkEdge>? EdgeChanged;

    public NetworkNode AddOrUpdateNode(NetworkNode node)
    {
        _nodes[node.Id] = node;
        NodeChanged?.Invoke(node);
        return node;
    }

    public NetworkEdge AddOrUpdateEdge(NetworkEdge edge)
    {
        _edges[edge.Id] = edge;
        EdgeChanged?.Invoke(edge);
        return edge;
    }

    public void RemoveNode(Guid nodeId)
    {
        _nodes.TryRemove(nodeId, out _);
        foreach (var edge in _edges.Values.Where(e => e.SourceNodeId == nodeId || e.TargetNodeId == nodeId).ToList())
            _edges.TryRemove(edge.Id, out _);
    }

    public void RemoveEdge(Guid edgeId) => _edges.TryRemove(edgeId, out _);

    public IEnumerable<NetworkNode> GetNeighbors(Guid nodeId, GraphViewKind view)
    {
        var neighborIds = _edges.Values
            .Where(e => e.ViewKind == view && (e.SourceNodeId == nodeId || e.TargetNodeId == nodeId))
            .Select(e => e.SourceNodeId == nodeId ? e.TargetNodeId : e.SourceNodeId)
            .ToHashSet();

        return _nodes.Values.Where(n => neighborIds.Contains(n.Id));
    }

    /// <summary>Finds the node whose IPv4 or hostname matches, used to merge discovery results.</summary>
    public NetworkNode? FindByAddress(string? ipv4, string? hostname)
    {
        return _nodes.Values.FirstOrDefault(n =>
            (!string.IsNullOrEmpty(ipv4) && n.IPv4Address == ipv4) ||
            (!string.IsNullOrEmpty(hostname) && n.Hostname == hostname));
    }
}

using Helios.Core.Models;

namespace Helios.Topology;

public sealed class TopologyDiffEngine
{
    public TopologyDiffResult Diff(NetworkSnapshot a, NetworkSnapshot b)
    {
        var result = new TopologyDiffResult();

        var nodesA = a.Nodes.ToDictionary(n => n.Id);
        var nodesB = b.Nodes.ToDictionary(n => n.Id);

        result.AddedNodes = b.Nodes.Where(n => !nodesA.ContainsKey(n.Id)).ToList();
        result.RemovedNodes = a.Nodes.Where(n => !nodesB.ContainsKey(n.Id)).ToList();

        foreach (var idOverlap in nodesA.Keys.Intersect(nodesB.Keys))
        {
            var na = nodesA[idOverlap];
            var nb = nodesB[idOverlap];

            CompareField(result, idOverlap, nameof(NetworkNode.IsOnline), na.IsOnline.ToString(), nb.IsOnline.ToString());
            CompareField(result, idOverlap, nameof(NetworkNode.Health), na.Health.ToString(), nb.Health.ToString());
            CompareField(result, idOverlap, nameof(NetworkNode.IPv4Address), na.IPv4Address ?? "", nb.IPv4Address ?? "");
            CompareField(result, idOverlap, nameof(NetworkNode.MacAddress), na.MacAddress ?? "", nb.MacAddress ?? "");
        }

        var edgeKeyA = a.Edges.ToDictionary(EdgeKey);
        var edgeKeyB = b.Edges.ToDictionary(EdgeKey);

        result.AddedEdges = b.Edges.Where(e => !edgeKeyA.ContainsKey(EdgeKey(e))).ToList();
        result.RemovedEdges = a.Edges.Where(e => !edgeKeyB.ContainsKey(EdgeKey(e))).ToList();

        return result;
    }

    private static string EdgeKey(NetworkEdge e) => $"{e.SourceNodeId}:{e.TargetNodeId}:{e.ViewKind}";

    private static void CompareField(TopologyDiffResult result, Guid nodeId, string field, string oldValue, string newValue)
    {
        if (oldValue != newValue)
            result.ChangedFields.Add((nodeId, field, oldValue, newValue));
    }
}

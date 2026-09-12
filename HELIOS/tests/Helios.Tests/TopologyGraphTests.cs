using Helios.Core.Enums;
using Helios.Core.Models;
using Helios.Topology;
using Xunit;

namespace Helios.Tests;

public class TopologyGraphTests
{
    [Fact]
    public void AddOrUpdateNode_AddsNewNode()
    {
        var graph = new GraphEngine();
        var node = new NetworkNode { DisplayName = "Router-1", Kind = NodeKind.Router };
        graph.AddOrUpdateNode(node);
        Assert.Single(graph.Nodes);
    }

    [Fact]
    public void RemoveNode_AlsoRemovesConnectedEdges()
    {
        var graph = new GraphEngine();
        var a = graph.AddOrUpdateNode(new NetworkNode { DisplayName = "A" });
        var b = graph.AddOrUpdateNode(new NetworkNode { DisplayName = "B" });
        var edge = graph.AddOrUpdateEdge(new NetworkEdge { SourceNodeId = a.Id, TargetNodeId = b.Id });

        graph.RemoveNode(a.Id);

        Assert.DoesNotContain(graph.Edges, e => e.Id == edge.Id);
    }

    [Fact]
    public void PathAnalysis_FindsShortestWeightedPath()
    {
        var a = new NetworkNode { DisplayName = "A" };
        var b = new NetworkNode { DisplayName = "B" };
        var c = new NetworkNode { DisplayName = "C" };
        var edgeAB = new NetworkEdge { SourceNodeId = a.Id, TargetNodeId = b.Id, ObservedLatencyMs = 10 };
        var edgeBC = new NetworkEdge { SourceNodeId = b.Id, TargetNodeId = c.Id, ObservedLatencyMs = 5 };
        var edgeAC = new NetworkEdge { SourceNodeId = a.Id, TargetNodeId = c.Id, ObservedLatencyMs = 50 };

        var tool = new PathAnalysisTool();
        var result = tool.FindPath(new[] { a, b, c }, new[] { edgeAB, edgeBC, edgeAC }, a.Id, c.Id, GraphViewKind.Physical);

        Assert.True(result.PathFound);
        Assert.Equal(15, result.TotalLatencyMs);
        Assert.Equal(3, result.Hops.Count);
    }
}

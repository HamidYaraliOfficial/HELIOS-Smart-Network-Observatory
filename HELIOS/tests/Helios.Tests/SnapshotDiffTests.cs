using Helios.Core.Models;
using Helios.Topology;
using Xunit;

namespace Helios.Tests;

public class SnapshotDiffTests
{
    [Fact]
    public void Diff_DetectsAddedAndRemovedNodes()
    {
        var nodeA = new NetworkNode { DisplayName = "A" };
        var nodeB = new NetworkNode { DisplayName = "B" };

        var snapshotBefore = new NetworkSnapshot { Nodes = { nodeA } };
        var snapshotAfter = new NetworkSnapshot { Nodes = { nodeA, nodeB } };

        var diff = new TopologyDiffEngine().Diff(snapshotBefore, snapshotAfter);

        Assert.Single(diff.AddedNodes);
        Assert.Empty(diff.RemovedNodes);
    }

    [Fact]
    public void Diff_DetectsHealthChange()
    {
        var nodeBefore = new NetworkNode { DisplayName = "A", Health = Core.Enums.HealthState.Healthy };
        var nodeAfter = new NetworkNode { Id = nodeBefore.Id, DisplayName = "A", Health = Core.Enums.HealthState.Critical };

        var diff = new TopologyDiffEngine().Diff(
            new NetworkSnapshot { Nodes = { nodeBefore } },
            new NetworkSnapshot { Nodes = { nodeAfter } });

        Assert.Contains(diff.ChangedFields, f => f.Field == "Health");
    }
}

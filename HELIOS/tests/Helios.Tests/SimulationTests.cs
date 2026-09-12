using Helios.Core.Enums;
using Helios.Core.Models;
using Helios.Simulation;
using Xunit;

namespace Helios.Tests;

public class SimulationTests
{
    [Fact]
    public void Run_NeverMutatesLiveNodes()
    {
        var liveNode = new NetworkNode { DisplayName = "Core-Router", IsOnline = true, Health = HealthState.Healthy };
        var simulator = new ScenarioSimulator();

        var action = new ScenarioAction { Kind = ScenarioActionKind.NodeDown, TargetNodeId = liveNode.Id };
        var result = simulator.Run(new[] { liveNode }, Array.Empty<NetworkEdge>(), action);

        Assert.True(liveNode.IsOnline); // live object untouched
        Assert.Contains("SIMULATION", result.Summary);
    }
}

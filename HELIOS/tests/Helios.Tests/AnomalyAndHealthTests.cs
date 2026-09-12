using Helios.Analytics;
using Helios.Core.Models;
using Helios.Telemetry;
using Xunit;

namespace Helios.Tests;

public class AnomalyAndHealthTests
{
    [Fact]
    public void HealthEngine_ReturnsUnknown_WhenNoMetricsObserved()
    {
        var engine = new HealthEngine();
        var node = new NetworkNode { IsOnline = true };

        var state = engine.Evaluate(node, out var reason);

        Assert.Equal(Core.Enums.HealthState.Unknown, state);
        Assert.NotEmpty(reason);
    }

    [Fact]
    public void HealthEngine_ReturnsCritical_WhenOffline()
    {
        var engine = new HealthEngine();
        var node = new NetworkNode { IsOnline = false };

        var state = engine.Evaluate(node, out _);

        Assert.Equal(Core.Enums.HealthState.Critical, state);
    }

    [Fact]
    public void BaselineEngine_ReportsLowConfidence_WithFewSamples()
    {
        var engine = new BaselineEngine();
        var baseline = engine.BuildBaseline(new List<double> { 10, 12, 11 });

        Assert.Equal(BaselineConfidence.Low, baseline.Confidence);
    }

    [Fact]
    public void AnomalyEngine_FlagsTrafficSpike_WhenFarFromBaseline()
    {
        var engine = new AnomalyEngine();
        var history = Enumerable.Range(0, 50).Select(_ => 1000.0).ToList();

        var anomaly = engine.CheckTrafficSpike(Guid.NewGuid(), history, currentBps: 50000);

        Assert.NotNull(anomaly);
        Assert.Contains("Observed Anomaly", anomaly!.Title);
    }
}

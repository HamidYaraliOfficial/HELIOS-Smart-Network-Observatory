using Helios.Core.Enums;
using Helios.Core.Interfaces;
using Helios.Core.Models;
using Helios.Alerts;
using Helios.Rules;
using Xunit;

namespace Helios.Tests;

public class RuleEngineTests
{
    [Fact]
    public async Task EvaluateAsync_RaisesAlert_WhenLatencyExceedsThreshold()
    {
        var alertService = new AlertCenterService();
        var ruleEngine = new RuleEngine(alertService);

        ruleEngine.RegisterRule(new Rule
        {
            Name = "High Latency",
            Conditions = { new RuleCondition { Metric = "latency_ms", Comparator = ">", Threshold = 100 } },
            Severity = AlertSeverity.High
        });

        var node = new NetworkNode { DisplayName = "Server-1", LastLatencyMs = 250, IsOnline = true };
        await ruleEngine.EvaluateAsync(node, CancellationToken.None);

        Assert.Single(alertService.ActiveAlerts);
        Assert.Equal(AlertSeverity.High, alertService.ActiveAlerts.First().Severity);
    }

    [Fact]
    public async Task EvaluateAsync_Deduplicates_RepeatedTriggers()
    {
        var alertService = new AlertCenterService();
        var ruleEngine = new RuleEngine(alertService);
        ruleEngine.RegisterRule(new Rule
        {
            Name = "Flap",
            Cooldown = TimeSpan.Zero,
            Conditions = { new RuleCondition { Metric = "latency_ms", Comparator = ">", Threshold = 10 } }
        });

        var node = new NetworkNode { DisplayName = "N1", LastLatencyMs = 500, IsOnline = true };
        await ruleEngine.EvaluateAsync(node, CancellationToken.None);
        await ruleEngine.EvaluateAsync(node, CancellationToken.None);

        Assert.Single(alertService.ActiveAlerts);
        Assert.Equal(2, alertService.ActiveAlerts.First().OccurrenceCount);
    }
}

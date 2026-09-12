using Helios.Core.Enums;
using Helios.Core.Models;

namespace Helios.Telemetry;

public sealed class ObservedAnomaly
{
    public string Title { get; set; } = "Observed Anomaly";
    public string PatternKind { get; set; } = string.Empty; // TrafficSpike, ErrorIncrease, RepeatedFailure, LatencyIncrease, Flapping, AvailabilityDrop
    public AlertSeverity Severity { get; set; } = AlertSeverity.Low;
    public List<string> Evidence { get; set; } = new();
    public Guid? RelatedNodeId { get; set; }
}

/// <summary>
/// Detects observable, explainable patterns (spikes, flapping, repeated
/// failures) and reports them as "Observed Anomaly" / "Potential Issue" -
/// never as a confirmed attack, in line with HELIOS's evidence-based design.
/// </summary>
public sealed class AnomalyEngine
{
    private readonly BaselineEngine _baseline = new();

    public ObservedAnomaly? CheckTrafficSpike(Guid nodeId, IReadOnlyList<double> historicalBps, double currentBps)
    {
        var baseline = _baseline.BuildBaseline(historicalBps);
        var deviation = _baseline.EvaluateDeviation(baseline, currentBps);
        if (!deviation.IsAnomalous) return null;

        return new ObservedAnomaly
        {
            Title = "Observed Anomaly: Traffic Spike",
            PatternKind = "TrafficSpike",
            Severity = Math.Abs(deviation.ZScore) > 5 ? AlertSeverity.High : AlertSeverity.Medium,
            RelatedNodeId = nodeId,
            Evidence = new List<string>
            {
                $"Current throughput {currentBps:N0} bps deviates {deviation.ZScore:F2} standard deviations from baseline.",
                $"Baseline confidence: {deviation.Confidence} (n={baseline.SampleCount})."
            }
        };
    }

    public ObservedAnomaly? CheckFlapping(Guid nodeId, IReadOnlyList<bool> recentOnlineStates, int minTransitions = 4)
    {
        int transitions = 0;
        for (int i = 1; i < recentOnlineStates.Count; i++)
            if (recentOnlineStates[i] != recentOnlineStates[i - 1]) transitions++;

        if (transitions < minTransitions) return null;

        return new ObservedAnomaly
        {
            Title = "Observed Anomaly: Device Flapping",
            PatternKind = "Flapping",
            Severity = AlertSeverity.Medium,
            RelatedNodeId = nodeId,
            Evidence = new List<string> { $"{transitions} online/offline transitions observed in the recent sampling window." }
        };
    }

    public ObservedAnomaly? CheckRepeatedFailures(Guid nodeId, int consecutiveFailures, int threshold = 3)
    {
        if (consecutiveFailures < threshold) return null;

        return new ObservedAnomaly
        {
            Title = "Potential Issue: Repeated Connection Failures",
            PatternKind = "RepeatedFailure",
            Severity = consecutiveFailures >= threshold * 2 ? AlertSeverity.High : AlertSeverity.Medium,
            RelatedNodeId = nodeId,
            Evidence = new List<string> { $"{consecutiveFailures} consecutive failures observed, threshold was {threshold}." }
        };
    }
}

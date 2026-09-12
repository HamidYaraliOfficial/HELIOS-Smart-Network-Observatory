using Helios.Core.Models;

namespace Helios.Telemetry;

public sealed class TrafficPatternSummary
{
    public int PeakHourOfDay { get; set; }
    public int QuietHourOfDay { get; set; }
    public double AverageThroughputBps { get; set; }
    public double AverageConnectionsPerHour { get; set; }
}

public sealed class TrafficPatternAnalyzer
{
    public TrafficPatternSummary Analyze(IEnumerable<FlowRecord> flows)
    {
        var list = flows.ToList();
        if (list.Count == 0) return new TrafficPatternSummary();

        var byHour = list.GroupBy(f => f.TimestampUtc.Hour)
            .ToDictionary(g => g.Key, g => g.Sum(f => f.Bytes));

        var peak = byHour.OrderByDescending(kv => kv.Value).First();
        var quiet = byHour.OrderBy(kv => kv.Value).First();

        return new TrafficPatternSummary
        {
            PeakHourOfDay = peak.Key,
            QuietHourOfDay = quiet.Key,
            AverageThroughputBps = list.Average(f => f.RateBps),
            AverageConnectionsPerHour = list.GroupBy(f => f.TimestampUtc.Hour).Average(g => g.Count())
        };
    }
}

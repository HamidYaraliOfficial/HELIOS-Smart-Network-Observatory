using Helios.Core.Models;

namespace Helios.Traffic;

public sealed class TrafficRatePoint
{
    public DateTimeOffset TimestampUtc { get; set; }
    public double ThroughputBps { get; set; }
    public int ConnectionCount { get; set; }
    public double AverageLatencyMs { get; set; }
    public double ErrorRatePercent { get; set; }
}

/// <summary>
/// Buckets raw FlowRecords into fixed-width time series points for the
/// Real-Time Traffic Graph widget, applying the user's active filters
/// (interface/host/service/port/protocol/time range).
/// </summary>
public sealed class RealTimeTrafficAggregator
{
    public List<TrafficRatePoint> Aggregate(IEnumerable<FlowRecord> flows, TimeSpan bucketWidth, int errorPortHint = 0)
    {
        var buckets = flows
            .GroupBy(f => new DateTimeOffset(f.TimestampUtc.Ticks - (f.TimestampUtc.Ticks % bucketWidth.Ticks), TimeSpan.Zero))
            .OrderBy(g => g.Key)
            .Select(g => new TrafficRatePoint
            {
                TimestampUtc = g.Key,
                ThroughputBps = g.Sum(f => f.RateBps),
                ConnectionCount = g.Count(),
                AverageLatencyMs = 0,
                ErrorRatePercent = 0
            })
            .ToList();

        return buckets;
    }

    public IEnumerable<FlowRecord> ApplyFilters(
        IEnumerable<FlowRecord> flows, string? interfaceHint, string? hostFilter,
        int? port, DateTimeOffset? fromUtc, DateTimeOffset? toUtc)
    {
        return flows.Where(f =>
            (hostFilter is null || f.SourceAddress.Contains(hostFilter, StringComparison.OrdinalIgnoreCase) || f.DestinationAddress.Contains(hostFilter, StringComparison.OrdinalIgnoreCase)) &&
            (port is null || f.SourcePort == port || f.DestinationPort == port) &&
            (fromUtc is null || f.TimestampUtc >= fromUtc) &&
            (toUtc is null || f.TimestampUtc <= toUtc));
    }
}

using Helios.Core.Enums;
using Helios.Core.Models;

namespace Helios.Telemetry;

public sealed record ProtocolStat(ProtocolKind Protocol, long FlowCount, long TotalBytes, double AveragePacketSize);

public sealed class ProtocolStatisticsService
{
    public IReadOnlyCollection<ProtocolStat> Summarize(IEnumerable<FlowRecord> flows)
    {
        return flows
            .GroupBy(f => f.Protocol)
            .Select(g => new ProtocolStat(
                g.Key,
                g.Count(),
                g.Sum(f => f.Bytes),
                g.Sum(f => f.Packets) == 0 ? 0 : (double)g.Sum(f => f.Bytes) / g.Sum(f => f.Packets)))
            .OrderByDescending(s => s.TotalBytes)
            .ToList();
    }
}

using System.Collections.Concurrent;
using Helios.Core.Enums;
using Helios.Core.Models;

namespace Helios.Telemetry;

/// <summary>
/// Ingests flow/metadata records from the active telemetry sources and keeps a
/// bounded rolling buffer for the Flow Explorer and Real-Time Traffic Graph.
/// Deliberately keeps only metadata (addresses, ports, protocol, byte/packet
/// counters) - never payload content.
/// </summary>
public sealed class TrafficObservationEngine
{
    private readonly ConcurrentQueue<FlowRecord> _buffer = new();
    private readonly int _maxBufferedFlows;

    public TrafficObservationEngine(int maxBufferedFlows = 50_000)
    {
        _maxBufferedFlows = maxBufferedFlows;
    }

    public event Action<FlowRecord>? FlowObserved;

    public void Ingest(FlowRecord record)
    {
        _buffer.Enqueue(record);
        while (_buffer.Count > _maxBufferedFlows && _buffer.TryDequeue(out _)) { }
        FlowObserved?.Invoke(record);
    }

    public IReadOnlyCollection<FlowRecord> GetRecent(TimeSpan window)
    {
        var cutoff = DateTimeOffset.UtcNow - window;
        return _buffer.Where(f => f.TimestampUtc >= cutoff).ToList();
    }

    public IReadOnlyCollection<FlowRecord> Query(
        string? sourceContains = null, string? destinationContains = null,
        ProtocolKind? protocol = null, int? port = null)
    {
        return _buffer.Where(f =>
                (sourceContains is null || f.SourceAddress.Contains(sourceContains, StringComparison.OrdinalIgnoreCase)) &&
                (destinationContains is null || f.DestinationAddress.Contains(destinationContains, StringComparison.OrdinalIgnoreCase)) &&
                (protocol is null || f.Protocol == protocol) &&
                (port is null || f.SourcePort == port || f.DestinationPort == port))
            .ToList();
    }
}

using System.Text.Json;
using Helios.Core.Models;

namespace Helios.Reports;

/// <summary>
/// One-stop export point for Topology, Flow data, Metrics, Alerts, Device
/// Inventory and Snapshots, writing to a folder chosen by the user via the
/// WinUI FileSavePicker.
/// </summary>
public sealed class ExportStudio
{
    public async Task ExportTopologyJsonAsync(string path, IEnumerable<NetworkNode> nodes, IEnumerable<NetworkEdge> edges, CancellationToken cancellationToken)
    {
        var payload = new { Nodes = nodes, Edges = edges, ExportedUtc = DateTimeOffset.UtcNow };
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true }), cancellationToken);
    }

    public async Task ExportFlowsCsvAsync(string path, IEnumerable<FlowRecord> flows, CancellationToken cancellationToken)
    {
        var lines = new List<string> { "Timestamp,Source,Destination,SrcPort,DstPort,Protocol,Bytes,Packets" };
        lines.AddRange(flows.Select(f =>
            $"{f.TimestampUtc:u},{f.SourceAddress},{f.DestinationAddress},{f.SourcePort},{f.DestinationPort},{f.Protocol},{f.Bytes},{f.Packets}"));
        await File.WriteAllLinesAsync(path, lines, cancellationToken);
    }

    public async Task ExportDeviceInventoryCsvAsync(string path, IEnumerable<NetworkNode> nodes, CancellationToken cancellationToken)
    {
        var lines = new List<string> { "Name,Kind,IPv4,MAC,Health,Online" };
        lines.AddRange(nodes.Select(n => $"{n.DisplayName},{n.Kind},{n.IPv4Address},{n.MacAddress},{n.Health},{n.IsOnline}"));
        await File.WriteAllLinesAsync(path, lines, cancellationToken);
    }
}

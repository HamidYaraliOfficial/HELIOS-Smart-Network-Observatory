using System.Net.NetworkInformation;
using Helios.Core.Models;

namespace Helios.Networking.Interfaces;

/// <summary>
/// Samples per-interface throughput over time using IPv4InterfaceStatistics,
/// exposed by .NET on top of official Windows networking counters.
/// </summary>
public sealed class InterfaceMonitor
{
    private readonly Dictionary<string, (long Sent, long Received, DateTimeOffset At)> _lastSample = new();

    public IEnumerable<NetworkInterfaceInfo> Sample(IEnumerable<NetworkInterfaceInfo> current)
    {
        foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
        {
            var match = current.FirstOrDefault(c => c.Id == nic.Id);
            if (match is null) continue;

            IPv4InterfaceStatistics? stats;
            try { stats = nic.GetIPv4Statistics(); }
            catch { continue; }

            var now = DateTimeOffset.UtcNow;
            if (_lastSample.TryGetValue(nic.Id, out var previous))
            {
                var elapsed = (now - previous.At).TotalSeconds;
                if (elapsed > 0.05)
                {
                    match.BytesSentPerSecond = Math.Max(0, (stats.BytesSent - previous.Sent) / elapsed);
                    match.BytesReceivedPerSecond = Math.Max(0, (stats.BytesReceived - previous.Received) / elapsed);
                }
            }

            _lastSample[nic.Id] = (stats.BytesSent, stats.BytesReceived, now);
            yield return match;
        }
    }
}

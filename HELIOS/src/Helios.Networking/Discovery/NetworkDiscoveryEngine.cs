using System.Net;
using Helios.Core.Enums;
using Helios.Core.Interfaces;
using Helios.Core.Models;

namespace Helios.Networking.Discovery;

/// <summary>
/// Real implementation of INetworkDiscoveryEngine. Combines interface enumeration,
/// a bounded ICMP subnet sweep restricted to the user-selected scope, and ARP
/// cache lookups for MAC address association plus best-effort reverse DNS for
/// hostnames. No exploitation, credential access, or port-scanning of remote
/// hosts is performed by this engine.
/// </summary>
public sealed class NetworkDiscoveryEngine : INetworkDiscoveryEngine
{
    private readonly SubnetScanner _scanner = new();
    private readonly ArpResolver _arp = new();
    private readonly InterfaceEnumerator _interfaces = new();

    public Task<List<NetworkInterfaceInfo>> GetLocalInterfacesAsync(CancellationToken cancellationToken)
        => Task.FromResult(_interfaces.GetInterfaces());

    public async IAsyncEnumerable<NetworkNode> DiscoverAsync(
        DiscoveryScope scope,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(scope.CidrRange))
            yield break;

        var arpTable = scope.ResolveArp ? _arp.ReadArpTable() : new Dictionary<string, string>();

        await foreach (var pingResult in _scanner.ScanAsync(scope.CidrRange, scope.MaxConcurrency, scope.TimeoutMs, cancellationToken))
        {
            if (!pingResult.Responded) continue;

            string? hostname = null;
            if (scope.ResolveHostnames)
            {
                try
                {
                    var entry = await Dns.GetHostEntryAsync(pingResult.IpAddress, cancellationToken);
                    hostname = entry.HostName;
                }
                catch { /* resolution failure is expected and non-fatal */ }
            }

            arpTable.TryGetValue(pingResult.IpAddress, out var mac);

            yield return new NetworkNode
            {
                DisplayName = hostname ?? pingResult.IpAddress,
                Kind = NodeKind.Unknown,
                Hostname = hostname,
                IPv4Address = pingResult.IpAddress,
                MacAddress = mac,
                IsOnline = true,
                IsDiscovered = true,
                LastSeenUtc = DateTimeOffset.UtcNow,
                LastLatencyMs = pingResult.RoundtripMs,
                Health = HealthState.Healthy
            };
        }
    }
}

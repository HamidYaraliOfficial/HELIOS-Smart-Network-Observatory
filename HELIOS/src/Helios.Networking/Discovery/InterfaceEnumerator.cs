using System.Net.NetworkInformation;
using Helios.Core.Models;

namespace Helios.Networking.Discovery;

/// <summary>
/// Wraps System.Net.NetworkInformation to enumerate local network interfaces
/// (adapters), their addresses, gateways and DNS servers - all via official
/// .NET / Windows APIs, no third-party dependency.
/// </summary>
public sealed class InterfaceEnumerator
{
    public List<NetworkInterfaceInfo> GetInterfaces()
    {
        var results = new List<NetworkInterfaceInfo>();

        foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (nic.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                continue;

            var props = nic.GetIPProperties();
            var info = new NetworkInterfaceInfo
            {
                Id = nic.Id,
                Name = nic.Name,
                Description = nic.Description,
                InterfaceType = nic.NetworkInterfaceType.ToString(),
                IsUp = nic.OperationalStatus == OperationalStatus.Up,
                SpeedBps = SafeSpeed(nic),
                MacAddress = FormatMac(nic.GetPhysicalAddress())
            };

            foreach (var addr in props.UnicastAddresses)
            {
                if (addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    info.IPv4Addresses.Add(addr.Address.ToString());
                    info.SubnetMask ??= addr.IPv4Mask?.ToString();
                }
                else if (addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                {
                    info.IPv6Addresses.Add(addr.Address.ToString());
                }
            }

            var gateway = props.GatewayAddresses.FirstOrDefault();
            info.GatewayAddress = gateway?.Address?.ToString();

            foreach (var dns in props.DnsAddresses)
                info.DnsServers.Add(dns.ToString());

            results.Add(info);
        }

        return results;
    }

    private static long SafeSpeed(NetworkInterface nic)
    {
        try { return nic.Speed; } catch { return 0; }
    }

    private static string? FormatMac(PhysicalAddress address)
    {
        var bytes = address.GetAddressBytes();
        if (bytes.Length == 0) return null;
        return string.Join(":", bytes.Select(b => b.ToString("X2")));
    }
}

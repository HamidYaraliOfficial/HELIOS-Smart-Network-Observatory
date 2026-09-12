using System.Diagnostics;
using Helios.Core.Models;

namespace Helios.Networking.Interfaces;

/// <summary>
/// Reads current Wi-Fi association metadata (SSID / signal / channel) using the
/// official "netsh wlan show interfaces" command, which is the supported,
/// documented way to surface this information without native WLAN API interop.
/// Read-only - never changes Wi-Fi configuration.
/// </summary>
public sealed class WifiObservatory
{
    public async Task<NetworkInterfaceInfo?> GetCurrentWifiStateAsync(CancellationToken cancellationToken)
    {
        try
        {
            var psi = new ProcessStartInfo("netsh", "wlan show interfaces")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process is null) return null;

            var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);

            return ParseNetshOutput(output);
        }
        catch
        {
            return null;
        }
    }

    private static NetworkInterfaceInfo ParseNetshOutput(string output)
    {
        var info = new NetworkInterfaceInfo { Name = "Wi-Fi", InterfaceType = "Wireless80211" };

        foreach (var rawLine in output.Split('\n'))
        {
            var line = rawLine.Trim();
            if (line.StartsWith("SSID", StringComparison.OrdinalIgnoreCase) && !line.StartsWith("BSSID"))
                info.WifiSsid = ValueAfterColon(line);
            else if (line.StartsWith("Signal", StringComparison.OrdinalIgnoreCase))
            {
                var raw = ValueAfterColon(line).Replace("%", string.Empty).Trim();
                if (int.TryParse(raw, out var pct)) info.WifiSignalPercent = pct;
            }
            else if (line.StartsWith("Channel", StringComparison.OrdinalIgnoreCase))
                info.WifiChannel = ValueAfterColon(line);
            else if (line.StartsWith("Authentication", StringComparison.OrdinalIgnoreCase))
                info.WifiAuthAlgorithm = ValueAfterColon(line);
            else if (line.StartsWith("State", StringComparison.OrdinalIgnoreCase))
                info.IsUp = ValueAfterColon(line).Contains("connected", StringComparison.OrdinalIgnoreCase);
        }

        return info;
    }

    private static string ValueAfterColon(string line)
    {
        var idx = line.IndexOf(':');
        return idx >= 0 ? line[(idx + 1)..].Trim() : string.Empty;
    }
}

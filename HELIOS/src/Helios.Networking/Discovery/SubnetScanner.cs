using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Channels;

namespace Helios.Networking.Discovery;

public sealed record SubnetHostResult(string IpAddress, bool Responded, long? RoundtripMs, string? MacAddress);

/// <summary>
/// Performs a bounded, concurrent ICMP sweep across a user-approved CIDR range.
/// This is a standard, low-risk discovery technique (identical in spirit to
/// "ping sweep" utilities built into most OS toolkits) and is only ever run
/// against a scope the user explicitly selected in the UI.
/// </summary>
public sealed class SubnetScanner
{
    public async IAsyncEnumerable<SubnetHostResult> ScanAsync(
        string cidr,
        int maxConcurrency,
        int timeoutMs,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var addresses = ExpandCidr(cidr).ToList();
        var channel = Channel.CreateUnbounded<SubnetHostResult>();
        using var throttle = new SemaphoreSlim(maxConcurrency);

        var producer = Task.Run(async () =>
        {
            var tasks = new List<Task>();
            foreach (var address in addresses)
            {
                if (cancellationToken.IsCancellationRequested) break;
                await throttle.WaitAsync(cancellationToken);
                tasks.Add(PingOneAsync(address, timeoutMs, channel, throttle, cancellationToken));
            }
            await Task.WhenAll(tasks);
            channel.Writer.Complete();
        }, cancellationToken);

        await foreach (var item in channel.Reader.ReadAllAsync(cancellationToken))
            yield return item;

        await producer;
    }

    private static async Task PingOneAsync(
        IPAddress address, int timeoutMs, Channel<SubnetHostResult> channel,
        SemaphoreSlim throttle, CancellationToken cancellationToken)
    {
        try
        {
            using var ping = new Ping();
            var reply = await ping.SendPingAsync(address, timeoutMs);
            var result = new SubnetHostResult(
                address.ToString(),
                reply.Status == IPStatus.Success,
                reply.Status == IPStatus.Success ? reply.RoundtripTime : null,
                null);
            await channel.Writer.WriteAsync(result, cancellationToken);
        }
        catch
        {
            await channel.Writer.WriteAsync(new SubnetHostResult(address.ToString(), false, null, null), cancellationToken);
        }
        finally
        {
            throttle.Release();
        }
    }

    /// <summary>Expands a CIDR notation (e.g. 192.168.1.0/24) into its usable host addresses.</summary>
    public static IEnumerable<IPAddress> ExpandCidr(string cidr)
    {
        var parts = cidr.Split('/');
        if (parts.Length != 2 || !IPAddress.TryParse(parts[0], out var baseAddress))
            yield break;

        if (!int.TryParse(parts[1], out var prefixLength) || prefixLength is < 0 or > 32)
            yield break;

        uint baseAddr = ToUInt32(baseAddress);
        uint mask = prefixLength == 0 ? 0 : 0xFFFFFFFF << (32 - prefixLength);
        uint network = baseAddr & mask;
        uint broadcast = network | ~mask;

        // Guard against accidentally scanning huge ranges (e.g. /8).
        uint hostCount = broadcast - network;
        if (hostCount > 65536) yield break;

        for (uint host = network + 1; host < broadcast; host++)
            yield return FromUInt32(host);
    }

    private static uint ToUInt32(IPAddress address)
    {
        var bytes = address.GetAddressBytes();
        if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
        return BitConverter.ToUInt32(bytes, 0);
    }

    private static IPAddress FromUInt32(uint value)
    {
        var bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
        return new IPAddress(bytes);
    }
}

using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace Helios.Networking.Discovery;

/// <summary>
/// Reads the Windows ARP/neighbor cache via the official IP Helper API.
/// Only exposes entries that Windows itself already resolved through normal
/// network activity (e.g. after a successful ping) - HELIOS never injects,
/// spoofs, or poisons ARP traffic.
/// </summary>
public sealed class ArpResolver
{
    public Dictionary<string, string> ReadArpTable()
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        int bufferSize = 0;
        NativeMethods.GetIpNetTable(null, ref bufferSize, false);
        if (bufferSize <= 0)
            return result;

        var buffer = new byte[bufferSize];
        int ret = NativeMethods.GetIpNetTable(buffer, ref bufferSize, false);
        if (ret != NativeMethods.NO_ERROR)
            return result;

        int entryCount = BitConverter.ToInt32(buffer, 0);
        int rowSize = Marshal.SizeOf<NativeMethods.MIB_IPNETROW>();
        int offset = 4;

        for (int i = 0; i < entryCount; i++)
        {
            if (offset + rowSize > buffer.Length) break;

            int dwAddr = BitConverter.ToInt32(buffer, offset + 8);
            byte[] macBytes = new byte[6];
            Array.Copy(buffer, offset + 4, macBytes, 0, 6);

            var ip = new IPAddress(BitConverter.GetBytes(dwAddr)).ToString();
            var mac = string.Join(":", macBytes.Select(b => b.ToString("X2")));

            if (mac != "00:00:00:00:00:00")
                result[ip] = mac;

            offset += rowSize;
        }

        return result;
    }
}

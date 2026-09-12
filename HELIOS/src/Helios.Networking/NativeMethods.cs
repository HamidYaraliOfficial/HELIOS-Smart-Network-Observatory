using System.Runtime.InteropServices;

namespace Helios.Networking;

/// <summary>
/// Thin, well-documented P/Invoke wrappers around official Windows IP Helper
/// (iphlpapi.dll) functions. These are the same public APIs used by tools such
/// as "arp -a" and are not exploit primitives - they only read the local ARP /
/// neighbor cache that Windows already maintains.
/// </summary>
internal static class NativeMethods
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct MIB_IPNETROW
    {
        public int dwIndex;
        public int dwPhysAddrLen;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] bPhysAddr;
        public int dwAddr;
        public int dwType;
    }

    [DllImport("iphlpapi.dll", SetLastError = true)]
    internal static extern int GetIpNetTable(byte[]? pIpNetTable, ref int pdwSize, bool bOrder);

    internal const int NO_ERROR = 0;
    internal const int ERROR_INSUFFICIENT_BUFFER = 122;
}

using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;

namespace Helios.Security;

/// <summary>
/// Encrypts secrets (cloud AI API keys, agent tokens) at rest using Windows
/// DPAPI (ProtectedData), scoped to the current user, so secrets never appear
/// in plain text in the SQLite database or log files.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class CredentialVault
{
    private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("Helios.CredentialVault.v1");

    public string Protect(string plainText)
    {
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var protectedBytes = ProtectedData.Protect(plainBytes, Entropy, DataProtectionScope.CurrentUser);
        return Convert.ToBase64String(protectedBytes);
    }

    public string Unprotect(string protectedBase64)
    {
        var protectedBytes = Convert.FromBase64String(protectedBase64);
        var plainBytes = ProtectedData.Unprotect(protectedBytes, Entropy, DataProtectionScope.CurrentUser);
        return Encoding.UTF8.GetString(plainBytes);
    }
}

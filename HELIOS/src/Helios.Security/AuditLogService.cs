namespace Helios.Security;

public sealed record AuditEntry(DateTimeOffset TimestampUtc, string Actor, string Action, string Details);

/// <summary>
/// Append-only record of security-relevant changes: scope changes, rule edits,
/// alert configuration changes, agent registration, exports, and security
/// setting changes. Never stores credentials or secrets.
/// </summary>
public sealed class AuditLogService
{
    private readonly List<AuditEntry> _entries = new();
    public event Action<AuditEntry>? EntryAdded;

    public void Record(string action, string details, string actor = "local-user")
    {
        var entry = new AuditEntry(DateTimeOffset.UtcNow, actor, action, details);
        _entries.Add(entry);
        EntryAdded?.Invoke(entry);
    }

    public IReadOnlyList<AuditEntry> GetAll() => _entries.AsReadOnly();
}

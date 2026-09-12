namespace Helios.Rules;

public sealed class MaintenanceWindow
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public string Scope { get; set; } = "*"; // node id or segment id
    public DateTimeOffset StartUtc { get; set; }
    public DateTimeOffset EndUtc { get; set; }
    public string Reason { get; set; } = string.Empty;
}

/// <summary>Suppresses alerts for a scope during a user-defined maintenance window.</summary>
public sealed class MaintenanceModeService
{
    private readonly List<MaintenanceWindow> _windows = new();

    public void Schedule(MaintenanceWindow window) => _windows.Add(window);

    public bool IsUnderMaintenance(string scope, DateTimeOffset? nowUtc = null)
    {
        var now = nowUtc ?? DateTimeOffset.UtcNow;
        return _windows.Any(w => (w.Scope == "*" || w.Scope == scope) && now >= w.StartUtc && now <= w.EndUtc);
    }

    public IReadOnlyList<MaintenanceWindow> ActiveWindows(DateTimeOffset? nowUtc = null)
    {
        var now = nowUtc ?? DateTimeOffset.UtcNow;
        return _windows.Where(w => now >= w.StartUtc && now <= w.EndUtc).ToList();
    }
}

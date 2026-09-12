namespace Helios.Security;

public sealed class TrustedAgentStatus
{
    public string AgentId { get; set; } = string.Empty;
    public string Hostname { get; set; } = string.Empty;
    public bool CertificateValid { get; set; }
    public DateTimeOffset? LastSeenUtc { get; set; }
    public int FailedConnectionAttempts { get; set; }
}

/// <summary>
/// Aggregates authentication, trusted-agent, and certificate/token health for
/// the Security Center dashboard.
/// </summary>
public sealed class SecurityCenterService
{
    private readonly Dictionary<string, TrustedAgentStatus> _agents = new();

    public void RegisterAgent(TrustedAgentStatus status) => _agents[status.AgentId] = status;

    public void RecordFailedConnection(string agentId)
    {
        if (_agents.TryGetValue(agentId, out var status))
            status.FailedConnectionAttempts++;
    }

    public IReadOnlyCollection<TrustedAgentStatus> GetAgents() => _agents.Values.ToList();
}

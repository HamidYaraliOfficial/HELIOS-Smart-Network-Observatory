namespace Helios.Server;

/// <summary>
/// Minimal bearer-token + agent-id validator for the Central Observatory
/// Server preview. Production deployments should back this with a proper
/// identity provider and mutual-TLS between agents and the server.
/// </summary>
public sealed class AgentTokenValidator
{
    private readonly HashSet<string> _validTokens;

    public AgentTokenValidator(IConfiguration configuration)
    {
        _validTokens = configuration.GetSection("ValidAgentTokens").Get<string[]>()?.ToHashSet() ?? new HashSet<string>();
    }

    public bool TryValidate(HttpRequest request, out string? agentId)
    {
        agentId = request.Headers["X-Helios-Agent-Id"].FirstOrDefault();
        var auth = request.Headers.Authorization.FirstOrDefault();

        if (string.IsNullOrEmpty(agentId) || string.IsNullOrEmpty(auth) || !auth.StartsWith("Bearer "))
            return false;

        var token = auth["Bearer ".Length..];
        return _validTokens.Count == 0 || _validTokens.Contains(token);
    }
}

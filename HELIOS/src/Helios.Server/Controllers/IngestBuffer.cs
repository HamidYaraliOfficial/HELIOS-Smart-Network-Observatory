using System.Collections.Concurrent;
using System.Text.Json;

namespace Helios.Server;

/// <summary>In-memory holding area for the most recent payload per agent, ready to be
/// persisted by a background consumer into the same Helios.Storage schema used locally.</summary>
public sealed class IngestBuffer
{
    private readonly ConcurrentDictionary<string, JsonElement> _latestByAgent = new();

    public void Add(string agentId, JsonElement payload) => _latestByAgent[agentId] = payload;
    public JsonElement? GetLatest(string agentId) => _latestByAgent.TryGetValue(agentId, out var v) ? v : null;
}

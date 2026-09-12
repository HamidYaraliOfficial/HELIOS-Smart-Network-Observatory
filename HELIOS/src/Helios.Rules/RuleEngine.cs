using System.Collections.Concurrent;
using Helios.Core.Enums;
using Helios.Core.Interfaces;
using Helios.Core.Models;

namespace Helios.Rules;

/// <summary>
/// Evaluates user-defined threshold/condition rules against live node metrics
/// and raises alerts through IAlertService. Honors Duration (must be sustained)
/// and Cooldown (avoid re-firing immediately) semantics.
/// </summary>
public sealed class RuleEngine : IRuleEngine
{
    private readonly ConcurrentDictionary<Guid, Rule> _rules = new();
    private readonly IAlertService _alertService;
    private readonly Dictionary<(Guid RuleId, Guid NodeId), DateTimeOffset> _conditionFirstTrueAt = new();

    public RuleEngine(IAlertService alertService)
    {
        _alertService = alertService;
    }

    public IReadOnlyCollection<Rule> Rules => _rules.Values.ToList();

    public void RegisterRule(Rule rule) => _rules[rule.Id] = rule;
    public void RemoveRule(Guid ruleId) => _rules.TryRemove(ruleId, out _);

    public async Task EvaluateAsync(NetworkNode node, CancellationToken cancellationToken)
    {
        foreach (var rule in _rules.Values.Where(r => r.IsEnabled))
        {
            if (!IsInScope(rule, node)) continue;

            bool allConditionsMet = rule.Conditions.Count > 0 && rule.Conditions.All(c => EvaluateCondition(c, node));

            var key = (rule.Id, node.Id);
            if (allConditionsMet)
            {
                if (!_conditionFirstTrueAt.TryGetValue(key, out var firstTrue))
                {
                    firstTrue = DateTimeOffset.UtcNow;
                    _conditionFirstTrueAt[key] = firstTrue;
                }

                var sustainedFor = DateTimeOffset.UtcNow - firstTrue;
                if (sustainedFor < rule.Duration) continue;

                if (rule.LastTriggeredUtc is { } last && DateTimeOffset.UtcNow - last < rule.Cooldown)
                    continue;

                rule.LastTriggeredUtc = DateTimeOffset.UtcNow;
                rule.ConsecutiveFailureCount++;

                await _alertService.RaiseAsync(new Alert
                {
                    Title = $"{rule.Name}",
                    Description = $"Rule '{rule.Name}' matched on {node.DisplayName}.",
                    Severity = rule.Severity,
                    RelatedNodeId = node.Id,
                    SourceRuleId = rule.Id,
                    DeduplicationKey = $"{rule.Id}:{node.Id}",
                    EvidenceLines = rule.Conditions.Select(c => $"{c.Metric} {c.Comparator} {c.Threshold}").ToList()
                }, cancellationToken);
            }
            else
            {
                _conditionFirstTrueAt.Remove(key);
                rule.ConsecutiveFailureCount = 0;
            }
        }
    }

    private static bool IsInScope(Rule rule, NetworkNode node)
    {
        if (rule.Scope == "*") return true;
        if (rule.Scope == node.SegmentId) return true;
        if (rule.Scope == node.Id.ToString()) return true;
        return node.Tags.Contains(rule.Scope);
    }

    private static bool EvaluateCondition(RuleCondition condition, NetworkNode node)
    {
        double? value = condition.Metric switch
        {
            "latency_ms" => node.LastLatencyMs,
            "packet_loss_pct" => node.PacketLossPercent,
            "availability_pct" => node.IsOnline ? 100.0 : 0.0,
            _ => null
        };

        if (value is null) return false;

        return condition.Comparator switch
        {
            ">" => value > condition.Threshold,
            "<" => value < condition.Threshold,
            ">=" => value >= condition.Threshold,
            "<=" => value <= condition.Threshold,
            "==" => Math.Abs(value.Value - condition.Threshold) < 0.0001,
            "!=" => Math.Abs(value.Value - condition.Threshold) > 0.0001,
            _ => false
        };
    }
}

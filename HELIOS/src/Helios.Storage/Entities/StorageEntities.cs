namespace Helios.Storage.Entities;

/// <summary>EF Core persistence models. Kept intentionally separate from the
/// live Core domain models so the storage schema can evolve independently of
/// the in-memory graph representation.</summary>

public sealed class NodeEntity
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;
    public string? Hostname { get; set; }
    public string? IPv4Address { get; set; }
    public string? MacAddress { get; set; }
    public string? SegmentId { get; set; }
    public string? WorkspaceId { get; set; }
    public string Health { get; set; } = "Unknown";
    public bool IsOnline { get; set; }
    public DateTime? LastSeenUtc { get; set; }
    public DateTime FirstSeenUtc { get; set; }
    public double? LastLatencyMs { get; set; }
    public string TagsCsv { get; set; } = string.Empty;
    public string MetadataJson { get; set; } = "{}";
}

public sealed class EdgeEntity
{
    public string Id { get; set; } = string.Empty;
    public string SourceNodeId { get; set; } = string.Empty;
    public string TargetNodeId { get; set; } = string.Empty;
    public string ViewKind { get; set; } = "Physical";
    public bool IsDependency { get; set; }
    public bool IsUserDefined { get; set; }
    public double? ObservedLatencyMs { get; set; }
}

public sealed class EventEntity
{
    public string Id { get; set; } = string.Empty;
    public DateTime TimestampUtc { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? RelatedNodeId { get; set; }
    public string Severity { get; set; } = "Info";
    public string? CorrelationGroupId { get; set; }
}

public sealed class AlertEntity
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = "Info";
    public string Status { get; set; } = "Active";
    public DateTime CreatedUtc { get; set; }
    public string DeduplicationKey { get; set; } = string.Empty;
    public int OccurrenceCount { get; set; } = 1;
}

public sealed class RuleEntity
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public string ConfigJson { get; set; } = "{}";
}

public sealed class SnapshotEntity
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedUtc { get; set; }
    public string? WorkspaceId { get; set; }
    public string PayloadJson { get; set; } = "{}";
}

public sealed class MetricSampleEntity
{
    public long RowId { get; set; }
    public string NodeId { get; set; } = string.Empty;
    public string MetricName { get; set; } = string.Empty;
    public double Value { get; set; }
    public DateTime TimestampUtc { get; set; }
}

public sealed class AuditLogEntity
{
    public long RowId { get; set; }
    public DateTime TimestampUtc { get; set; }
    public string Actor { get; set; } = "local-user";
    public string Action { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
}

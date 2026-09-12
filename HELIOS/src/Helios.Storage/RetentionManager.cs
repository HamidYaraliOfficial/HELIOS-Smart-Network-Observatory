using Microsoft.EntityFrameworkCore;

namespace Helios.Storage;

public sealed class RetentionPolicy
{
    public int MetricRetentionDays { get; set; } = 30;
    public int EventRetentionDays { get; set; } = 90;
    public bool DownsampleOldMetrics { get; set; } = true;
    public int DownsampleAfterDays { get; set; } = 7;
    public TimeSpan DownsampleBucket { get; set; } = TimeSpan.FromMinutes(15);
}

/// <summary>
/// Prevents unbounded database growth: deletes metrics/events beyond the
/// user-configured retention window, and can downsample older raw samples
/// into periodic averages before they're purged.
/// </summary>
public sealed class RetentionManager
{
    private readonly HeliosDbContext _db;

    public RetentionManager(HeliosDbContext db)
    {
        _db = db;
    }

    public async Task ApplyAsync(RetentionPolicy policy, CancellationToken cancellationToken)
    {
        var metricCutoff = DateTime.UtcNow.AddDays(-policy.MetricRetentionDays);
        var eventCutoff = DateTime.UtcNow.AddDays(-policy.EventRetentionDays);

        if (policy.DownsampleOldMetrics)
        {
            var downsampleCutoff = DateTime.UtcNow.AddDays(-policy.DownsampleAfterDays);
            await DownsampleAsync(downsampleCutoff, policy.DownsampleBucket, cancellationToken);
        }

        await _db.MetricSamples.Where(m => m.TimestampUtc < metricCutoff).ExecuteDeleteAsync(cancellationToken);
        await _db.Events.Where(e => e.TimestampUtc < eventCutoff).ExecuteDeleteAsync(cancellationToken);
    }

    private async Task DownsampleAsync(DateTime cutoff, TimeSpan bucket, CancellationToken cancellationToken)
    {
        var stale = await _db.MetricSamples.Where(m => m.TimestampUtc < cutoff).ToListAsync(cancellationToken);
        if (stale.Count == 0) return;

        var grouped = stale.GroupBy(m => (m.NodeId, m.MetricName,
            Bucket: new DateTime(m.TimestampUtc.Ticks - (m.TimestampUtc.Ticks % bucket.Ticks), DateTimeKind.Utc)));

        _db.MetricSamples.RemoveRange(stale);

        foreach (var group in grouped)
        {
            _db.MetricSamples.Add(new Entities.MetricSampleEntity
            {
                NodeId = group.Key.NodeId,
                MetricName = group.Key.MetricName,
                TimestampUtc = group.Key.Bucket,
                Value = group.Average(m => m.Value)
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}

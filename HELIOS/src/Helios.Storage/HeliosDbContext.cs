using Helios.Storage.Entities;
using Microsoft.EntityFrameworkCore;

namespace Helios.Storage;

/// <summary>
/// EF Core context for HELIOS's local SQLite database. Versioned via EF Core
/// migrations (see Migrations folder once `dotnet ef migrations add` has been
/// run in a full dev environment). Uses EnsureCreated() as a zero-friction
/// fallback for first-run / portfolio demo scenarios.
/// </summary>
public sealed class HeliosDbContext : DbContext
{
    private readonly string _databasePath;

    public HeliosDbContext(string databasePath)
    {
        _databasePath = databasePath;
    }

    public DbSet<NodeEntity> Nodes => Set<NodeEntity>();
    public DbSet<EdgeEntity> Edges => Set<EdgeEntity>();
    public DbSet<EventEntity> Events => Set<EventEntity>();
    public DbSet<AlertEntity> Alerts => Set<AlertEntity>();
    public DbSet<RuleEntity> Rules => Set<RuleEntity>();
    public DbSet<SnapshotEntity> Snapshots => Set<SnapshotEntity>();
    public DbSet<MetricSampleEntity> MetricSamples => Set<MetricSampleEntity>();
    public DbSet<AuditLogEntity> AuditLog => Set<AuditLogEntity>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={_databasePath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NodeEntity>().HasKey(n => n.Id);
        modelBuilder.Entity<EdgeEntity>().HasKey(e => e.Id);
        modelBuilder.Entity<EventEntity>().HasKey(e => e.Id);
        modelBuilder.Entity<AlertEntity>().HasKey(a => a.Id);
        modelBuilder.Entity<RuleEntity>().HasKey(r => r.Id);
        modelBuilder.Entity<SnapshotEntity>().HasKey(s => s.Id);
        modelBuilder.Entity<MetricSampleEntity>().HasKey(m => m.RowId);
        modelBuilder.Entity<MetricSampleEntity>().Property(m => m.RowId).ValueGeneratedOnAdd();
        modelBuilder.Entity<AuditLogEntity>().HasKey(a => a.RowId);
        modelBuilder.Entity<AuditLogEntity>().Property(a => a.RowId).ValueGeneratedOnAdd();

        modelBuilder.Entity<MetricSampleEntity>().HasIndex(m => new { m.NodeId, m.MetricName, m.TimestampUtc });
        modelBuilder.Entity<EventEntity>().HasIndex(e => e.TimestampUtc);
    }

    public static HeliosDbContext CreateAndEnsureReady(string databasePath)
    {
        var context = new HeliosDbContext(databasePath);
        context.Database.EnsureCreated();
        return context;
    }
}

using Helios.Storage;
using Helios.Storage.Entities;
using Xunit;

namespace Helios.Tests;

public class StorageTests : IDisposable
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"helios-test-{Guid.NewGuid():N}.db");

    [Fact]
    public void CreateAndEnsureReady_CreatesDatabaseFile()
    {
        using var context = HeliosDbContext.CreateAndEnsureReady(_dbPath);
        Assert.True(File.Exists(_dbPath));
    }

    [Fact]
    public void CanInsertAndQueryNode()
    {
        using var context = HeliosDbContext.CreateAndEnsureReady(_dbPath);
        context.Nodes.Add(new NodeEntity { Id = "n1", DisplayName = "Test Node", FirstSeenUtc = DateTime.UtcNow });
        context.SaveChanges();

        Assert.Single(context.Nodes);
    }

    public void Dispose()
    {
        if (File.Exists(_dbPath)) File.Delete(_dbPath);
    }
}

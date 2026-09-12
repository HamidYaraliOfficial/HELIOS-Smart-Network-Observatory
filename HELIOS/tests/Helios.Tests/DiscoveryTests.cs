using Helios.Networking.Discovery;
using Xunit;

namespace Helios.Tests;

public class DiscoveryTests
{
    [Fact]
    public void ExpandCidr_ProducesExpectedHostCount_For24Subnet()
    {
        var hosts = SubnetScanner.ExpandCidr("192.168.1.0/24").ToList();
        Assert.Equal(254, hosts.Count); // 256 total - network address - broadcast address
    }

    [Fact]
    public void ExpandCidr_ReturnsEmpty_ForInvalidInput()
    {
        var hosts = SubnetScanner.ExpandCidr("not-a-cidr").ToList();
        Assert.Empty(hosts);
    }

    [Fact]
    public void ExpandCidr_GuardsAgainstHugeRanges()
    {
        var hosts = SubnetScanner.ExpandCidr("10.0.0.0/8").ToList();
        Assert.Empty(hosts); // guarded: too large to safely sweep
    }
}

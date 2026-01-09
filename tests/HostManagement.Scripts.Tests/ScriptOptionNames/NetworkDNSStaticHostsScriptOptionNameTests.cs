using FluentAssertions;
using Xunit;
using HostManagement.Scripts.ScriptOptionNames;

namespace HostManagement.Scripts.Tests.ScriptOptionNames;

public class NetworkDNSStaticHostsScriptOptionNameTests
{
    [Fact]
    public void Enabled_ShouldReturnExpectedValue()
        => NetworkDNSStaticHostsScriptOptionName.Enabled.ToString().Should().Be("--enabled");

    [Fact]
    public void IPAddress_ShouldReturnExpectedValue()
        => NetworkDNSStaticHostsScriptOptionName.IPAddress.ToString().Should().Be("--ip-address");

    [Fact]
    public void Hostname_ShouldReturnExpectedValue()
        => NetworkDNSStaticHostsScriptOptionName.Hostname.ToString().Should().Be("--hostname");
}

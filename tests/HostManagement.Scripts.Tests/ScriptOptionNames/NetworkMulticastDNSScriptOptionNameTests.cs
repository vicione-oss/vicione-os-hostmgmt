using FluentAssertions;
using Xunit;
using HostManagement.Scripts.ScriptOptionNames;

namespace HostManagement.Scripts.Tests.ScriptOptionNames;

public class NetworkMulticastDNSScriptOptionNameTests
{
    [Fact]
    public void MulticastDNSEnabled_ShouldReturnExpectedValue()
        => NetworkMulticastDNSScriptOptionName.MulticastDNSEnabled.ToString().Should().Be("--multicast-dns-enabled");
}

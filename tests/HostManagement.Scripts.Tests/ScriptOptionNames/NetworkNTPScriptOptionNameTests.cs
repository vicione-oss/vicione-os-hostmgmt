using FluentAssertions;
using Xunit;
using HostManagement.Scripts.ScriptOptionNames;

namespace HostManagement.Scripts.Tests.ScriptOptionNames;

public class NetworkNTPScriptOptionNameTests
{
    [Fact]
    public void NTPServersEnabled_ShouldReturnExpectedValue()
        => NetworkNTPScriptOptionName.NTPServersEnabled.ToString().Should().Be("--ntpservers-enabled");

    [Fact]
    public void NTPServers_ShouldReturnExpectedValue()
        => NetworkNTPScriptOptionName.NTPServers.ToString().Should().Be("--ntpservers");
}

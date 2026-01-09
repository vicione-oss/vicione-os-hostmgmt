using FluentAssertions;
using Xunit;
using HostManagement.Scripts.ScriptOptionNames;

namespace HostManagement.Scripts.Tests.ScriptOptionNames;

public class ProxyScriptOptionNameTests
{
    [Fact]
    public void Enabled_ShouldReturnExpectedValue()
        => ProxyScriptOptionName.Enabled.ToString().Should().Be("--enabled");

    [Fact]
    public void URL_ShouldReturnExpectedValue()
        => ProxyScriptOptionName.URL.ToString().Should().Be("--url");

    [Fact]
    public void Port_ShouldReturnExpectedValue()
        => ProxyScriptOptionName.Port.ToString().Should().Be("--port");

    [Fact]
    public void Username_ShouldReturnExpectedValue()
        => ProxyScriptOptionName.Username.ToString().Should().Be("--username");

    [Fact]
    public void Password_ShouldReturnExpectedValue()
        => ProxyScriptOptionName.Password.ToString().Should().Be("--password");
}

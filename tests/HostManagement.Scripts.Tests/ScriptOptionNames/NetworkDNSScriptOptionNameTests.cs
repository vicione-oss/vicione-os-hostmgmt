using FluentAssertions;
using Xunit;
using HostManagement.Scripts.ScriptOptionNames;

namespace HostManagement.Scripts.Tests.ScriptOptionNames;

public class NetworkDNSScriptOptionNameTests
{
    [Fact]
    public void NameServersEnabled_ShouldReturnExpectedValue()
        => NetworkDNSScriptOptionName.NameServersEnabled.ToString().Should().Be("--nameservers-enabled");

    [Fact]
    public void NameServers_ShouldReturnExpectedValue()
        => NetworkDNSScriptOptionName.NameServers.ToString().Should().Be("--nameservers");

    [Fact]
    public void DNSSuffixEnabled_ShouldReturnExpectedValue()
        => NetworkDNSScriptOptionName.DNSSuffixEnabled.ToString().Should().Be("--dns-suffix-enabled");

    [Fact]
    public void DNSSuffix_ShouldReturnExpectedValue()
        => NetworkDNSScriptOptionName.DNSSuffix.ToString().Should().Be("--dns-suffix");

    [Fact]
    public void SearchDomainsEnabled_ShouldReturnExpectedValue()
        => NetworkDNSScriptOptionName.SearchDomainsEnabled.ToString().Should().Be("--search-domains-enabled");

    [Fact]
    public void SearchDomains_ShouldReturnExpectedValue()
        => NetworkDNSScriptOptionName.SearchDomains.ToString().Should().Be("--search-domains");
}

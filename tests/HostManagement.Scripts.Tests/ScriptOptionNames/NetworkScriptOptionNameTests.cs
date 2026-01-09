using FluentAssertions;
using Xunit;
using HostManagement.Scripts.ScriptOptionNames;

namespace HostManagement.Scripts.Tests.ScriptOptionNames;

public class NetworkScriptOptionNameTests
{
    [Fact]
    public void VLANEnabled_ShouldReturnExpectedValue()
        => NetworkScriptOptionName.VLANEnabled.ToString().Should().Be("--vlan-enabled");

    [Fact]
    public void VLANID_ShouldReturnExpectedValue()
        => NetworkScriptOptionName.VLANID.ToString().Should().Be("--vlan-id");

    [Fact]
    public void InterfaceName_ShouldReturnExpectedValue()
        => NetworkScriptOptionName.InterfaceName.ToString().Should().Be("--interface-name");

    [Fact]
    public void MACAddress_ShouldReturnExpectedValue()
        => NetworkScriptOptionName.MACAddress.ToString().Should().Be("--mac-address");

    [Fact]
    public void InterfaceEnabled_ShouldReturnExpectedValue()
        => NetworkScriptOptionName.Enabled.ToString().Should().Be("--enabled");

    [Fact]
    public void InterfaceDHCPEnabled_ShouldReturnExpectedValue()
        => NetworkScriptOptionName.DHCPEnabled.ToString().Should().Be("--dhcp-enabled");

    [Fact]
    public void InterfaceIPAddress_ShouldReturnExpectedValue()
        => NetworkScriptOptionName.IPAddress.ToString().Should().Be("--ip-address");

    [Fact]
    public void InterfaceGateway_ShouldReturnExpectedValue()
        => NetworkScriptOptionName.Gateway.ToString().Should().Be("--gateway");
}

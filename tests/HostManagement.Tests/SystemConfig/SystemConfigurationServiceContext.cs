using HostManagement.Shared.Capabilities;
using HostManagement.SystemConfig;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace HostManagement.Tests.SystemConfig;

internal sealed class SystemConfigurationServiceContext
{
    internal IPersistence Persistence { get; } = Substitute.For<IPersistence>();
    internal IScriptHandler ScriptHandler { get; } = Substitute.For<IScriptHandler>();
    internal ILogger Logger { get; } = Substitute.For<ILogger>();
    internal SupportedSettings SupportedSettings { get; } = new();
    internal SystemConfigurationService Sut { get; }

#pragma warning disable CA1506
    internal SystemConfigurationServiceContext(string interfaceName, string hostname)
    {
        const string ServiceName = "test.service";

        var rawInterfacesString = $"{interfaceName}            UP             00:50:56:36:6d:33 <BROADCAST,MULTICAST,UP,LOWER_UP>";

        ScriptHandler.ExecuteIsNetworkInterfaceAvailableScript(Arg.Any<string>()).Returns(Result.SuccessResult);
        ScriptHandler.ExecuteGetOriginalPhysicalAddressScript(Arg.Any<string>()).Returns((Result.SuccessResult, "00:50:56:36:6d:aa"));
        ScriptHandler.ExecuteGetNetworkInterfacesScript().ReturnsForAnyArgs((Result.SuccessResult, rawInterfacesString));
        ScriptHandler.ExecuteGetNetworkInterfacesCommonInformationScript(Arg.Any<ScriptOptions>()).ReturnsForAnyArgs((Result.SuccessResult, []));
        ScriptHandler.ExecuteGetInterfaceSettingsScript(Arg.Any<string>()).ReturnsForAnyArgs((Result.SuccessResult, new(), new()));
        ScriptHandler.ExecuteGetInterfaceIPv4DHCPLeaseScript(Arg.Any<string>()).ReturnsForAnyArgs((Result.SuccessResult, new()));
        ScriptHandler.ExecuteGetNetworkDNSSettingsScript().ReturnsForAnyArgs((Result.SuccessResult, new() { Hostname = hostname }));
        ScriptHandler.ExecuteGetNetworkProxySettingsScript().ReturnsForAnyArgs((Result.SuccessResult, new()));
        ScriptHandler.ExecuteGetNetworkNTPSettingsScript().ReturnsForAnyArgs((Result.SuccessResult, new()));
        ScriptHandler.ExecuteGetSystemdUnitStateScript(Arg.Any<ScriptOptions>()).ReturnsForAnyArgs((Result.SuccessResult, new() { Name = ServiceName }));
        ScriptHandler.ExecuteSetInterfaceSettingsScript(Arg.Any<ScriptOptions>()).ReturnsForAnyArgs(Result.SuccessResult);
        ScriptHandler.ExecuteSetNetworkDNSSettingsScript(Arg.Any<ScriptOptions>()).ReturnsForAnyArgs(Result.SuccessResult);
        ScriptHandler.ExecuteSetStaticHostsScript(Arg.Any<ScriptOptions>()).ReturnsForAnyArgs(Result.SuccessResult);
        ScriptHandler.ExecuteSetNetworkMulticastDNSSettingsScript(Arg.Any<ScriptOptions>()).ReturnsForAnyArgs(Result.SuccessResult);
        ScriptHandler.ExecuteSetHostnameScript(Arg.Any<string>()).ReturnsForAnyArgs(Result.SuccessResult);
        ScriptHandler.ExecuteSetNetworkProxySettingsScript(Arg.Any<ScriptOptions>()).ReturnsForAnyArgs(Result.SuccessResult);
        ScriptHandler.ExecuteSetNetworkNTPSettingsScript(Arg.Any<ScriptOptions>()).ReturnsForAnyArgs(Result.SuccessResult);
        ScriptHandler.ExecuteSetSystemdUnitStateScript(Arg.Any<ScriptOptions>()).ReturnsForAnyArgs(Result.SuccessResult);
        ScriptHandler.ExecuteApplyNetplanConfigurationScript().ReturnsForAnyArgs(Result.SuccessResult);
        ScriptHandler.ExecuteGetSyslogSettingsScript().ReturnsForAnyArgs((Result.SuccessResult, new()));
        ScriptHandler.ExecuteSetSyslogSettingsScript(Arg.Any<ScriptOptions>()).ReturnsForAnyArgs(Result.SuccessResult);

        SupportedSettings.NetworkInterfaces.Capability = CapabilityStatus.Enabled;
        SupportedSettings.NetworkInterfaces.NetworkInterfaceDetail.IPv4.Capability = CapabilityStatus.Enabled;
        SupportedSettings.NetworkInterfaces.NetworkInterfaceDetail.VLAN.Capability = CapabilityStatus.Enabled;

        SupportedSettings.DNS.Capability = CapabilityStatus.Enabled;
        SupportedSettings.DNS.Hostname.Capability = CapabilityStatus.Enabled;
        SupportedSettings.DNS.MulticastDNS.Capability = CapabilityStatus.Enabled;
        SupportedSettings.DNS.StaticHosts.Capability = CapabilityStatus.Enabled;
        SupportedSettings.DNS.NameServers.Capability = CapabilityStatus.Enabled;
        SupportedSettings.DNS.PrimaryDNSSuffix.Capability = CapabilityStatus.Enabled;
        SupportedSettings.DNS.SearchDomains.Capability = CapabilityStatus.Enabled;

        SupportedSettings.Proxy.Capability = CapabilityStatus.Enabled;
        SupportedSettings.NTP.Capability = CapabilityStatus.Enabled;
        SupportedSettings.Syslog.Capability = CapabilityStatus.Enabled;
        SupportedSettings.Services.Capability = CapabilityStatus.Enabled;

        Sut = new(Persistence, ScriptHandler, Logger, SupportedSettings);

        Environment.SetEnvironmentVariable("CONFIGURABLE_SERVICES", ServiceName);
        Environment.SetEnvironmentVariable("SYSTEM_CONFIGURATION_DIRECTORY", "/mnt/persistent/hostmanagement");

        Logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
    }
#pragma warning restore CA1506
}

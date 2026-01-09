using HostManagement.Common.Utilities;
using HostManagement.Scripts.ScriptOptionNames;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Extensions;

namespace HostManagement.SystemConfig.Network;

/// <summary>
/// Represents the network interfaces configuration adapter.
/// </summary>
/// <param name="currentConfig">The current system configuration.</param>
/// <param name="scriptHandler">An <see cref="IScriptHandler"/> implementation.</param>
/// <param name="bypassHasChanges">
/// <inheritdoc cref="ConfigurationAdapter(bool, bool)" path="/param[@name='bypassHasChanges']"/>
/// </param>
/// <param name="bypassApplyConfig">
/// <inheritdoc cref="ConfigurationAdapter(bool, bool)" path="/param[@name='bypassApplyConfig']"/>
/// </param>
internal sealed class NetworkInterfacesConfigurationAdapter(
    SystemConfiguration currentConfig,
    IScriptHandler scriptHandler,
    bool bypassHasChanges,
    bool bypassApplyConfig)
        : ConfigurationAdapter<NetworkConfigurationAdapter>(bypassHasChanges, bypassApplyConfig)
{
    private readonly SystemConfiguration _currentConfig = currentConfig;
    private readonly IScriptHandler _scriptHandler = scriptHandler;
    private readonly bool _bypassApplyConfig = bypassApplyConfig;

    protected internal override async Task<OperationStatus> GetConfiguration(
        SystemConfiguration config,
        CancellationToken cancellationToken = default)
    {
        var hasWarning = false;

        var (status, networkInterfacesCommonInformation) = await _scriptHandler
            .ExecuteGetNetworkInterfacesCommonInformationScript()
            .ConfigureAwait(false);

        if (status == OperationStatus.Error)
            return OperationStatus.Error;

        if (status == OperationStatus.Warning)
            hasWarning = true;

        var networkInterfaces = new List<NetworkInterfaceDetail>();

        foreach (var commonInformation in networkInterfacesCommonInformation)
        {
            var interfaceName = commonInformation.Name;

            (status, var loadedIPv4Settings) =
                await GetInterfaceIPv4Settings(interfaceName).ConfigureAwait(false);

            if (status == OperationStatus.Error)
                return OperationStatus.Error;

            var currentOrLoadedIPv4Details = loadedIPv4Settings.IPv4Details;
            var currentOrLoadedGateway = loadedIPv4Settings.Gateway;

            if (_currentConfig.GetNetworkInterface(interfaceName) is NetworkInterfaceDetail currentInterface)
            {
                // Don't discard already configured but disabled settings
                if (loadedIPv4Settings.DHCPEnabled)
                {
                    if (currentInterface.IPv4.IPv4Details.Count > 0)
                        currentOrLoadedIPv4Details = currentInterface.IPv4.IPv4Details;

                    if (currentInterface.IPv4.Gateway is not null)
                        currentOrLoadedGateway = currentInterface.IPv4.Gateway;
                }
            }

            networkInterfaces.Add(new NetworkInterfaceDetail()
            {
                CommonInformation = commonInformation,
                IPv4 = new IPv4Settings()
                {
                    Gateway = currentOrLoadedGateway,
                    DHCPEnabled = loadedIPv4Settings.DHCPEnabled,
                    DHCPLease = loadedIPv4Settings.DHCPLease,
                    IPv4Details = currentOrLoadedIPv4Details,
                    VLANEnabled = loadedIPv4Settings.VLANEnabled,
                    VLANID = loadedIPv4Settings.VLANID
                }
            });
        }

        config.NetworkInterfacesSettings = new NetworkInterfacesSettings()
        {
            NetworkInterfaces = networkInterfaces
        };

        return hasWarning ? OperationStatus.Warning : OperationStatus.Success;
    }

    private protected override bool HasChangesInternal(
        SystemConfiguration currentConfig,
        SystemConfiguration proposedConfig)
    {
        // TODO Improve code/scripts to not call `NetworkDNSConfigurationAdapter` and
        // `NetworkInterfacesConfigurationAdapter`
        //
        // Currently both adapters are executed. Therefor both scripts write in the corresponding
        // interfaces' yaml file. That is because the DNS script depends on a clean file
        // (meaning no DNS info configured).
        return
            !currentConfig.NetworkInterfacesSettings.NetworkInterfaces
                .SequenceEqual(proposedConfig.NetworkInterfacesSettings.NetworkInterfaces) ||
            !currentConfig.NetworkDNSSettings.Equals(proposedConfig.NetworkDNSSettings);
    }

    private protected override async Task<OperationStatus> PrepareConfiguration(
        SystemConfiguration config,
        CancellationToken cancellationToken = default)
    {
        OperationStatus status;
        var hasWarnings = false;

        foreach (var proposedInterface in config.NetworkInterfacesSettings.NetworkInterfaces)
        {
            // TODO Workaround for DNS script (see HasChanges method)
            //if (currentInterface.Equals(proposedInterface))
            //    continue;

            var scriptOptions = new ScriptOptions();

            if (proposedInterface.IPv4.VLANEnabled)
            {
                scriptOptions.Add(
                    NetworkScriptOptionName.VLANID,
                    proposedInterface.IPv4.VLANID);
            }

            scriptOptions.Add(
                NetworkScriptOptionName.InterfaceName,
                proposedInterface.CommonInformation.Name);

            proposedInterface.CommonInformation.PhysicalAddress =
                MACUtilities.ConvertToColonNotation(
                    proposedInterface.CommonInformation.PhysicalAddress);

            // Only set MAC address if bypass to apply config is not active
            if (!_bypassApplyConfig)
            {
                // Only set MAC address if not empty
                if (proposedInterface.CommonInformation.PhysicalAddress != string.Empty)
                {
                    scriptOptions.Add(
                        NetworkScriptOptionName.MACAddress,
                        proposedInterface.CommonInformation.PhysicalAddress);
                }
            }

            scriptOptions.Add(
                NetworkScriptOptionName.InterfaceEnabled,
                proposedInterface.CommonInformation.Enabled);

            scriptOptions.Add(
                NetworkScriptOptionName.InterfaceDHCPEnabled,
                proposedInterface.IPv4.DHCPEnabled);

            if (!proposedInterface.IPv4.DHCPEnabled)
            {
                for (var i = 0; i < proposedInterface.IPv4.IPv4Details.Count; i++)
                {
                    scriptOptions.Add(
                        NetworkScriptOptionName.InterfaceIPAddress,
                        IPUtilities.ConvertToCIDR(
                            proposedInterface.IPv4.IPv4Details[i].IPAddress,
                            proposedInterface.IPv4.IPv4Details[i].Netmask));

                    if (i == 0 &&
                        proposedInterface.IPv4.Gateway is not null)
                    {
                        scriptOptions.Add(
                            NetworkScriptOptionName.InterfaceGateway,
                            proposedInterface.IPv4.Gateway);
                    }
                }
            }

            status = await _scriptHandler
                .ExecuteSetInterfaceIPv4SettingsScript(scriptOptions)
                .ConfigureAwait(false);

            if (status == OperationStatus.Error)
                return OperationStatus.Error;

            if (status == OperationStatus.Warning)
                hasWarnings = true;
        }

        return hasWarnings ? OperationStatus.Warning : OperationStatus.Success;
    }

    private async Task<(OperationStatus, IPv4Settings)> GetInterfaceIPv4Settings(
        string interfaceName)
    {
        var (status, ipv4) = await _scriptHandler
            .ExecuteGetInterfaceIPv4SettingsScript(interfaceName)
            .ConfigureAwait(false);

        if (status == OperationStatus.Error)
            return (OperationStatus.Error, IPv4Settings.Empty);

        if (ipv4.DHCPEnabled)
        {
            (status, var dhcpLease) = await _scriptHandler
                .ExecuteGetInterfaceIPv4DHCPLeaseScript(interfaceName)
                .ConfigureAwait(false);

            if (status == OperationStatus.Error)
                ipv4.DHCPLease = DHCPLease.Empty;
            else
                ipv4.DHCPLease = dhcpLease;
        }

        return (status, ipv4);
    }
}

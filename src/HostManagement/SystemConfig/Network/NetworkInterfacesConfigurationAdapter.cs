using HostManagement.Common.Utilities;
using HostManagement.Scripts;
using HostManagement.Shared.Communication.Enums;
using HostManagement.SystemConfig.Contracts;

namespace HostManagement.SystemConfig.Network;

/// <summary>
/// Represents the network interfaces configuration adapter.
/// </summary>
/// <param name="scriptHandler">An <see cref="IScriptHandler"/> implementation.</param>
/// <param name="bypassHasChanges">
/// <inheritdoc cref="ConfigurationAdapter(bool, bool)" path="/param[@name='bypassHasChanges']"/>
/// </param>
/// <param name="bypassApplyConfig">
/// <inheritdoc cref="ConfigurationAdapter(bool, bool)" path="/param[@name='bypassApplyConfig']"/>
/// </param>
internal sealed class NetworkInterfacesConfigurationAdapter(
    IScriptHandler scriptHandler,
    bool bypassHasChanges,
    bool bypassApplyConfig)
        : ConfigurationAdapter<NetworkConfigurationAdapter>(bypassHasChanges, bypassApplyConfig)
{
    private readonly IScriptHandler _scriptHandler = scriptHandler;
    private readonly bool _bypassApplyConfig = bypassApplyConfig;

    protected internal override async Task<OperationStatus> GetConfiguration(
        SystemConfiguration config,
        CancellationToken cancellationToken = default)
    {
        var hasWarning = false;

        var (status, rawInterfacesString) = await _scriptHandler
            .ExecuteGetNetworkInterfacesScript()
            .ConfigureAwait(false);

        if (status is OperationStatus.Error)
            return OperationStatus.Error;

        var scriptOptions = new ScriptOptions();
        scriptOptions.Add(ScriptOptionNames.Network.Interfaces, rawInterfacesString);

        (status, var networkInterfacesCommonInformation) = await _scriptHandler
            .ExecuteGetNetworkInterfacesCommonInformationScript(scriptOptions)
            .ConfigureAwait(false);

        if (status == OperationStatus.Error)
            return OperationStatus.Error;

        if (status == OperationStatus.Warning)
            hasWarning = true;

        var networkInterfaces = new List<NetworkInterfaceDetail>();

        foreach (var commonInformation in networkInterfacesCommonInformation)
        {
            var interfaceName = commonInformation.Name;

            (status, var ipv4Settings) =
                await GetInterfaceIPv4Settings(interfaceName).ConfigureAwait(false);

            if (status == OperationStatus.Error)
                return OperationStatus.Error;

            networkInterfaces.Add(new NetworkInterfaceDetail()
            {
                CommonInformation = commonInformation,
                IPv4 = ipv4Settings,
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
            !EqualityComparer<NetworkInterfaceDetail>.Default.SequenceEqual(currentConfig.NetworkInterfacesSettings?.NetworkInterfaces,
                proposedConfig.NetworkInterfacesSettings?.NetworkInterfaces) ||
            !EqualityComparer<NetworkDNSSettings>.Default.Equals(currentConfig.NetworkDNSSettings, proposedConfig.NetworkDNSSettings);
    }

    private protected override async Task<OperationStatus> PrepareConfiguration(
        SystemConfiguration config,
        CancellationToken cancellationToken = default)
    {
        OperationStatus status;
        var hasWarnings = false;

        if (config.NetworkInterfacesSettings?.NetworkInterfaces?.Count > 0)
        {
            foreach (var proposedInterface in config.NetworkInterfacesSettings.NetworkInterfaces)
            {
                var interfaceName = proposedInterface.CommonInformation?.Name ?? string.Empty;

                status = await _scriptHandler
                    .ExecuteIsNetworkInterfaceAvailableScript(interfaceName)
                    .ConfigureAwait(false);

                if (status is OperationStatus.Warning or OperationStatus.Error)
                    return OperationStatus.Error;

                var scriptOptions = new ScriptOptions();

                AddVLANScriptOptions(scriptOptions, proposedInterface.IPv4);
                AddInterfaceNameScriptOption(scriptOptions, interfaceName);

                if (!string.IsNullOrWhiteSpace(proposedInterface.CommonInformation?.PhysicalAddress))
                {
                    proposedInterface.CommonInformation.PhysicalAddress = MACUtilities
                        .ConvertToColonNotation(proposedInterface.CommonInformation.PhysicalAddress);
                }

                AddMACAddressScriptOption(scriptOptions, proposedInterface.CommonInformation?.PhysicalAddress);
                AddInterfaceEnabledScriptOption(scriptOptions, proposedInterface.CommonInformation?.Enabled);
                AddIPv4ScriptOptions(scriptOptions, proposedInterface.IPv4);

                status = await _scriptHandler
                    .ExecuteSetInterfaceIPv4SettingsScript(scriptOptions)
                    .ConfigureAwait(false);

                if (status == OperationStatus.Error)
                    return OperationStatus.Error;

                if (status == OperationStatus.Warning)
                    hasWarnings = true;
            }
        }

        return hasWarnings ? OperationStatus.Warning : OperationStatus.Success;
    }

    private static void AddVLANScriptOptions(
        ScriptOptions scriptOptions,
        IPv4Settings? ipv4Settings)
    {
        if (ipv4Settings is not null && ipv4Settings.VLANEnabled.HasValue && ipv4Settings.VLANID.HasValue)
        {
            if (ipv4Settings.VLANEnabled.Value)
            {
                // TODO Remove "if check" around VLANEnabled and VLANID script options when implemented and uncomment VLANEnabled
                //scriptOptions.Add(
                //    ScriptOptionNames.Network.VLANEnabled,
                //    ipv4Settings.VLANEnabled);

                scriptOptions.Add(
                    ScriptOptionNames.Network.VLANID,
                    ipv4Settings.VLANID.Value);
            }
        }
    }

    private static void AddInterfaceNameScriptOption(ScriptOptions scriptOptions, string interfaceName)
    {
        scriptOptions.Add(
            ScriptOptionNames.Network.InterfaceName,
            interfaceName);
    }

    private void AddMACAddressScriptOption(ScriptOptions scriptOptions, string? physicalAddress)
    {
        if (_bypassApplyConfig)
            return;

        if (string.IsNullOrWhiteSpace(physicalAddress))
            return;

        scriptOptions.Add(
            ScriptOptionNames.Network.MACAddress,
            physicalAddress);
    }

    private static void AddInterfaceEnabledScriptOption(
        ScriptOptions scriptOptions,
        bool? enabled)
    {
        if (enabled.HasValue)
        {
            scriptOptions.Add(
                ScriptOptionNames.Network.Enabled,
                enabled.Value);
        }
    }

    private static void AddIPv4ScriptOptions(
        ScriptOptions scriptOptions,
        IPv4Settings? ipv4Settings)
    {
        if (ipv4Settings is not null && ipv4Settings.DHCPEnabled.HasValue)
        {
            scriptOptions.Add(
                ScriptOptionNames.Network.DHCPEnabled,
                ipv4Settings.DHCPEnabled.Value);

            for (var i = 0; i < ipv4Settings.IPv4Details?.Count; i++)
            {
                scriptOptions.Add(
                    ScriptOptionNames.Network.IPAddress,
                    IPUtilities.ConvertToCIDR(
                        ipv4Settings.IPv4Details[i].IPAddress,
                        ipv4Settings.IPv4Details[i].Netmask));

                if (i == 0 &&
                    ipv4Settings.Gateway is not null)
                {
                    scriptOptions.Add(
                        ScriptOptionNames.Network.Gateway,
                        ipv4Settings.Gateway);
                }
            }
        }
    }

    private async Task<(OperationStatus, IPv4Settings?)> GetInterfaceIPv4Settings(
        string interfaceName)
    {
        var (status, ipv4) = await _scriptHandler
            .ExecuteGetInterfaceIPv4SettingsScript(interfaceName)
            .ConfigureAwait(false);

        if (status == OperationStatus.Error)
            return (OperationStatus.Error, default);

        if (ipv4.DHCPEnabled.HasValue && ipv4.DHCPEnabled.Value)
        {
            (status, var dhcpLease) = await _scriptHandler
                .ExecuteGetInterfaceIPv4DHCPLeaseScript(interfaceName)
                .ConfigureAwait(false);

            if (status != OperationStatus.Error)
                ipv4.DHCPLease = dhcpLease;
        }

        return (status, ipv4);
    }
}

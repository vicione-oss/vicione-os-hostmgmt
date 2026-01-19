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

    protected internal override async Task<Result> GetConfiguration(
        SystemConfiguration config,
        CancellationToken cancellationToken)
    {
        var combinedResult = Result.SuccessResult;

        var (result, rawInterfacesString) = await _scriptHandler
            .ExecuteGetNetworkInterfacesScript()
            .ConfigureAwait(false);

        combinedResult.AddResult(result);

        if (combinedResult.OperationStatus is OperationStatus.Error)
            return combinedResult;

        var scriptOptions = new ScriptOptions();
        scriptOptions.Add(ScriptOptionNames.Network.Interfaces, rawInterfacesString);

        (result, var networkInterfacesCommonInformation) = await _scriptHandler
            .ExecuteGetNetworkInterfacesCommonInformationScript(scriptOptions)
            .ConfigureAwait(false);

        combinedResult.AddResult(result);

        if (combinedResult.OperationStatus is OperationStatus.Error)
            return combinedResult;

        var networkInterfaces = new List<NetworkInterfaceDetail>();

        foreach (var commonInformation in networkInterfacesCommonInformation)
        {
            var interfaceName = commonInformation.Name;

            (result, var ipv4Settings) = await GetInterfaceIPv4Settings(interfaceName).ConfigureAwait(false);

            combinedResult.AddResult(result);

            if (combinedResult.OperationStatus is OperationStatus.Error)
                return combinedResult;

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

        return combinedResult;
    }

    private protected override bool HasConfigurationChanges(
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
            SystemConfigurationChangeDetector.HasSequenceChanges(
                currentConfig.NetworkInterfacesSettings?.NetworkInterfaces,
                proposedConfig.NetworkInterfacesSettings?.NetworkInterfaces) ||
            SystemConfigurationChangeDetector.HasChanges(
                currentConfig.NetworkDNSSettings,
                proposedConfig.NetworkDNSSettings);
    }

    private protected override async Task<Result> PrepareConfiguration(
        SystemConfiguration config,
        CancellationToken cancellationToken)
    {
        if (config.NetworkInterfacesSettings?.NetworkInterfaces is null ||
            config.NetworkInterfacesSettings.NetworkInterfaces.Count < 1)
        {
            return Result.SuccessResult;
        }

        var combinedResult = Result.SuccessResult;

        foreach (var proposedInterface in config.NetworkInterfacesSettings.NetworkInterfaces)
        {
            var interfaceName = proposedInterface.CommonInformation?.Name ?? string.Empty;

            var result = await _scriptHandler.ExecuteIsNetworkInterfaceAvailableScript(interfaceName)
                .ConfigureAwait(false);

            if (result.OperationStatus is OperationStatus.Warning or OperationStatus.Error)
            {
                combinedResult.AddError(result.FormattedMessages);
                return combinedResult;
            }

            var scriptOptions = new ScriptOptions();

            AddVLANScriptOptions(scriptOptions, proposedInterface.VLAN);
            AddInterfaceNameScriptOption(scriptOptions, interfaceName);

            if (!string.IsNullOrWhiteSpace(proposedInterface.CommonInformation?.PhysicalAddress))
            {
                proposedInterface.CommonInformation.PhysicalAddress = MACUtilities
                    .ConvertToColonNotation(proposedInterface.CommonInformation.PhysicalAddress);
            }

            AddMACAddressScriptOption(scriptOptions, proposedInterface.CommonInformation?.PhysicalAddress);
            AddInterfaceEnabledScriptOption(scriptOptions, proposedInterface.CommonInformation?.Enabled);
            AddIPv4ScriptOptions(scriptOptions, proposedInterface.IPv4);

            combinedResult.AddResult(
                await _scriptHandler.ExecuteSetInterfaceIPv4SettingsScript(scriptOptions)
                    .ConfigureAwait(false));

            if (combinedResult.OperationStatus is OperationStatus.Error)
                return combinedResult;
        }

        return combinedResult;
    }

    private static void AddVLANScriptOptions(ScriptOptions scriptOptions, VLANSettings? vlanSettings)
    {
        if (vlanSettings is null)
            return;

        if (vlanSettings.Enabled.HasValue && vlanSettings.ID.HasValue)
        {
            if (vlanSettings.Enabled.Value)
            {
                // TODO Remove "if check" around Enabled and ID script options when implemented and uncomment
                // scriptOptions.Add(ScriptOptionNames.Network.VLANEnabled, vlanSettings.Enabled.Value);

                scriptOptions.Add(ScriptOptionNames.Network.VLANID, vlanSettings.ID.Value);
            }
        }
    }

    private static void AddInterfaceNameScriptOption(ScriptOptions scriptOptions, string interfaceName)
        => scriptOptions.Add(ScriptOptionNames.Network.InterfaceName, interfaceName);

    private void AddMACAddressScriptOption(ScriptOptions scriptOptions, string? physicalAddress)
    {
        if (_bypassApplyConfig)
            return;

        if (string.IsNullOrWhiteSpace(physicalAddress))
            return;

        scriptOptions.Add(ScriptOptionNames.Network.MACAddress, physicalAddress);
    }

    private static void AddInterfaceEnabledScriptOption(ScriptOptions scriptOptions, bool? enabled)
    {
        if (enabled.HasValue)
            scriptOptions.Add(ScriptOptionNames.Network.Enabled, enabled.Value);
    }

    private static void AddIPv4ScriptOptions(ScriptOptions scriptOptions, IPv4Settings? ipv4Settings)
    {
        if (ipv4Settings is not null && ipv4Settings.DHCPEnabled.HasValue)
        {
            scriptOptions.Add(ScriptOptionNames.Network.DHCPEnabled, ipv4Settings.DHCPEnabled.Value);

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
                    scriptOptions.Add(ScriptOptionNames.Network.Gateway, ipv4Settings.Gateway);
                }
            }
        }
    }

    private async Task<(Result, IPv4Settings?)> GetInterfaceIPv4Settings(string interfaceName)
    {
        var (result, ipv4) = await _scriptHandler
            .ExecuteGetInterfaceIPv4SettingsScript(interfaceName)
            .ConfigureAwait(false);

        if (result.OperationStatus is OperationStatus.Error)
            return (result, default);

        return (result, ipv4);
    }
}

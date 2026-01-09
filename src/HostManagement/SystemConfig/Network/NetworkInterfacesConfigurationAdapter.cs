using HostManagement.Scripts.ScriptOptionNames;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Common.Utilities;

namespace HostManagement.SystemConfig.Network;

/// <summary>
/// Represents the network interfaces configuration adapter.
/// </summary>
internal sealed class NetworkInterfacesConfigurationAdapter
    : ConfigurationAdapter<NetworkConfigurationAdapter>
{
    private readonly Result _configurationResult;
    private readonly IScriptHandler _scriptHandler;

    /// <summary>
    /// Initializes a new <see cref="NetworkInterfacesConfigurationAdapter"/>.
    /// </summary>
    /// <param name="configurationResult">The configuration result.</param>
    /// <param name="scriptHandler">An <see cref="IScriptHandler"/> implementation.</param>
    /// <param name="bypassHasChangesInternal">If set to <see langword="true"/> bypasses the
    /// internal check for changes and returns <see langword="true"/> when
    /// <see cref="ConfigurationAdapter.HasChanges(SystemConfiguration, SystemConfiguration)"/>
    /// is called.</param>
    public NetworkInterfacesConfigurationAdapter(
        Result configurationResult,
        IScriptHandler scriptHandler,
        bool bypassHasChangesInternal = false)
        : base(bypassHasChangesInternal)
    {
        _configurationResult = configurationResult;
        _scriptHandler = scriptHandler;
    }

    private protected override bool HasChangesInternal(SystemConfiguration currentConfig, SystemConfiguration proposedConfig)
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

    protected internal override async Task<OperationStatus> LoadConfiguration(
        SystemConfiguration config,
        CancellationToken cancellationToken)
    {
        try
        {
            var hasWarning = false;

            var (status, networkInterfacesCommonInformation) =
                await _scriptHandler.ExecuteGetNetworkInterfacesCommonInformationScript();

            if (status == OperationStatus.Error)
                return OperationStatus.Error;

            if (status == OperationStatus.Warning)
                hasWarning = true;

            var networkInterfaces = new List<NetworkInterfaceDetail>();

            foreach (var commonInformation in networkInterfacesCommonInformation)
            {
                (status, var ipv4) = await GetInterfaceIPv4Settings(commonInformation.Name);

                if (status == OperationStatus.Error)
                    return OperationStatus.Error;

                networkInterfaces.Add(new NetworkInterfaceDetail()
                {
                    CommonInformation = commonInformation,
                    IPv4 = ipv4
                });
            }

            config.NetworkInterfacesSettings = new NetworkInterfacesSettings()
            {
                NetworkInterfaces = networkInterfaces
            };

            return hasWarning ? OperationStatus.Warning : OperationStatus.Success;
        }
        catch (Exception ex)
        {
            _configurationResult.AddError(ex.Message);
            return OperationStatus.Error;
        }
    }

    protected internal override async Task<OperationStatus> ApplyConfiguration(
        SystemConfiguration proposed,
        CancellationToken cancellationToken)
    {
        try
        {
            OperationStatus status;
            var hasWarnings = false;

            foreach (var proposedInterface in proposed.NetworkInterfacesSettings.NetworkInterfaces)
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

                status = await _scriptHandler.ExecuteSetInterfaceIPv4SettingsScript(scriptOptions);

                if (status == OperationStatus.Error)
                    return OperationStatus.Error;

                if (status == OperationStatus.Warning)
                    hasWarnings = true;
            }

            return hasWarnings ? OperationStatus.Warning : OperationStatus.Success;
        }
        catch (Exception ex)
        {
            _configurationResult.AddError(ex.Message);
            return OperationStatus.Error;
        }
    }

    private async Task<(OperationStatus, IPv4Settings)> GetInterfaceIPv4Settings(
        string interfaceName)
    {
        var (status, ipv4) = await _scriptHandler.ExecuteGetInterfaceIPv4SettingsScript(interfaceName);

        if (status == OperationStatus.Error)
            return (OperationStatus.Error, IPv4Settings.Empty);

        if (ipv4.DHCPEnabled)
        {
            (status, var dhcpLease) = await _scriptHandler.ExecuteGetInterfaceIPv4DHCPLeaseScript(interfaceName);

            if (status == OperationStatus.Error)
                ipv4.DHCPLease = DHCPLease.Empty;
            else
                ipv4.DHCPLease = dhcpLease;
        }

        return (status, ipv4);
    }
}

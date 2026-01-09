using System.Text.Json;
using System.Text.Json.Serialization;
using HostManagement.Scripts;
using HostManagement.Scripts.Enums;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Contracts.Service;

namespace HostManagement;

[JsonSerializable(typeof(List<NetworkInterfaceCommonInformation>))]
internal partial class NetworkInterfacesCommonInformationContext : JsonSerializerContext;

/// <inheritdoc cref="IScriptHandler"/>
internal sealed class ScriptHandler : IScriptHandler
{
    private readonly Result _result;

    /// <summary>
    /// Initializes a new <see cref="ScriptHandler"/>.
    /// </summary>
    /// <param name="result">The result of the operation.</param>
    public ScriptHandler(Result result)
    {
        _result = result;
    }

    public async Task<OperationStatus> ExecuteApplyNetplanConfigurationScript()
    {
        var scriptResponse = await ScriptRunner.Run(ScriptIdentifier.ApplyNetplanConfiguration);

        if (scriptResponse.HasErrors)
        {
            _result.AddScriptError(
                ScriptIdentifier.ApplyNetplanConfiguration,
                scriptResponse.StandardError);

            return OperationStatus.Error;
        }

        return OperationStatus.Success;
    }

    public async Task<(OperationStatus, List<NetworkInterfaceCommonInformation>)> ExecuteGetNetworkInterfacesCommonInformationScript()
    {
        var hasWarning = false;
        var networkInterfacesCommonInformation = new List<NetworkInterfaceCommonInformation>();
        var scriptResponse = await ScriptRunner.Run(ScriptIdentifier.NetworkInterfacesCommonInformationGet);

        if (scriptResponse.HasErrors)
        {
            _result.AddScriptError(
                ScriptIdentifier.NetworkInterfacesCommonInformationGet,
                scriptResponse.StandardError);

            return (OperationStatus.Error, networkInterfacesCommonInformation);
        }

        if (scriptResponse.HasWarnings)
        {
            _result.AddScriptWarning(
                ScriptIdentifier.NetworkInterfacesCommonInformationGet,
                scriptResponse.StandardError);

            hasWarning = true;
        }

        try
        {
            networkInterfacesCommonInformation = JsonSerializer
                .Deserialize(
                    scriptResponse.StandardOutput,
                    NetworkInterfacesCommonInformationContext.Default.ListNetworkInterfaceCommonInformation)
                ?? throw new InvalidOperationException(
                    GetInvalidOperationExceptionMessage(
                        nameof(NetworkInterfaceCommonInformation),
                        ScriptIdentifier.NetworkInterfacesCommonInformationGet,
                        scriptResponse.StandardOutput));

            if (hasWarning)
                return (OperationStatus.Warning, networkInterfacesCommonInformation);

            return (OperationStatus.Success, networkInterfacesCommonInformation);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                GetInvalidOperationExceptionMessage(
                    nameof(NetworkInterfaceCommonInformation),
                    ScriptIdentifier.NetworkInterfacesCommonInformationGet,
                    ex.Message));
        }
    }

    public async Task<(OperationStatus, IPv4Settings)> ExecuteGetInterfaceIPv4SettingsScript(
        string interfaceName)
    {
        var scriptResponse = await ScriptRunner.Run(
            ScriptIdentifier.NetworkInterfaceIPv4SettingsGet,
            interfaceName);

        if (scriptResponse.HasErrors)
        {
            _result.AddScriptError(
                ScriptIdentifier.NetworkInterfaceIPv4SettingsGet,
                scriptResponse.StandardError);

            return (OperationStatus.Error, IPv4Settings.Empty);
        }

        try
        {
            var iPv4Settings = JsonSerializer
                .Deserialize(
                    scriptResponse.StandardOutput,
                    SourceGenerationContext.Default.IPv4Settings)
                ?? throw new InvalidOperationException(
                    GetInvalidOperationExceptionMessage(
                        nameof(IPv4Settings),
                        ScriptIdentifier.NetworkInterfaceIPv4SettingsGet,
                        scriptResponse.StandardOutput));

            return (OperationStatus.Success, iPv4Settings);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                GetInvalidOperationExceptionMessage(
                    nameof(IPv4Settings),
                    ScriptIdentifier.NetworkInterfaceIPv4SettingsGet,
                    ex.Message));
        }
    }

    public async Task<OperationStatus> ExecuteSetInterfaceIPv4SettingsScript(ScriptOptions scriptOptions)
    {
        var scriptResponse = await ScriptRunner.Run(
            ScriptIdentifier.NetworkInterfaceIPv4SettingsSet,
            scriptOptions.ToArray());

        if (scriptResponse.HasWarnings)
        {
            _result.AddScriptWarning(
                ScriptIdentifier.NetworkInterfaceIPv4SettingsSet,
                scriptResponse.StandardError);

            return OperationStatus.Warning;
        }

        if (scriptResponse.HasErrors)
        {
            _result.AddScriptError(
                ScriptIdentifier.NetworkInterfaceIPv4SettingsSet,
                scriptResponse.StandardError);

            return OperationStatus.Error;
        }

        return OperationStatus.Success;
    }

    public async Task<(OperationStatus, DHCPLease)> ExecuteGetInterfaceIPv4DHCPLeaseScript(
        string interfaceName)
    {
        var scriptResponse = await ScriptRunner.Run(
            ScriptIdentifier.NetworkInterfaceIPv4DHCPLeaseGet,
            interfaceName);

        if (scriptResponse.HasErrors)
        {
            _result.AddScriptError(
                ScriptIdentifier.NetworkInterfaceIPv4DHCPLeaseGet,
                scriptResponse.StandardError);

            return (OperationStatus.Error, DHCPLease.Empty);
        }

        try
        {
            var dhcpLease = JsonSerializer
                .Deserialize(
                    scriptResponse.StandardOutput,
                    SourceGenerationContext.Default.DHCPLease)
                ?? throw new InvalidOperationException(
                    GetInvalidOperationExceptionMessage(
                        nameof(DHCPLease),
                        ScriptIdentifier.NetworkInterfaceIPv4DHCPLeaseGet,
                        scriptResponse.StandardOutput));

            return (OperationStatus.Success, dhcpLease);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                GetInvalidOperationExceptionMessage(
                    nameof(DHCPLease),
                    ScriptIdentifier.NetworkInterfaceIPv4DHCPLeaseGet,
                    ex.Message));
        }
    }

    public async Task<(OperationStatus, NetworkDNSSettings)> ExecuteGetNetworkDNSSettingsScript()
    {
        var scriptResponse = await ScriptRunner.Run(ScriptIdentifier.NetworkDNSSettingsGet);

        if (scriptResponse.HasErrors)
        {
            _result.AddScriptError(
                ScriptIdentifier.NetworkDNSSettingsGet,
                scriptResponse.StandardError);

            return (OperationStatus.Error, NetworkDNSSettings.Empty);
        }

        try
        {
            var dnsSettings = JsonSerializer
                .Deserialize(
                    scriptResponse.StandardOutput,
                    SourceGenerationContext.Default.NetworkDNSSettings)
                ?? throw new InvalidOperationException(
                    GetInvalidOperationExceptionMessage(
                        nameof(NetworkDNSSettings),
                        ScriptIdentifier.NetworkDNSSettingsGet,
                        scriptResponse.StandardOutput));

            return (OperationStatus.Success, dnsSettings);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                GetInvalidOperationExceptionMessage(
                    nameof(NetworkDNSSettings),
                    ScriptIdentifier.NetworkDNSSettingsGet,
                    ex.Message));
        }
    }

    public async Task<OperationStatus> ExecuteSetStaticHostsScript(ScriptOptions scriptOptions)
    {
        var scriptResponse = await ScriptRunner.Run(
            ScriptIdentifier.NetworkStaticHostsSet,
            scriptOptions.ToArray());

        if (scriptResponse.HasWarnings)
        {
            _result.AddScriptWarning(
                ScriptIdentifier.NetworkStaticHostsSet,
                scriptResponse.StandardError);

            return OperationStatus.Warning;
        }

        if (scriptResponse.HasErrors)
        {
            _result.AddScriptError(
                ScriptIdentifier.NetworkStaticHostsSet,
                scriptResponse.StandardError);

            return OperationStatus.Error;
        }

        return OperationStatus.Success;
    }

    public async Task<OperationStatus> ExecuteSetNetworkDNSSettingsScript(ScriptOptions scriptOptions)
    {
        var scriptResponse = await ScriptRunner.Run(
            ScriptIdentifier.NetworkDNSSettingsSet,
            scriptOptions.ToArray());

        if (scriptResponse.HasWarnings)
        {
            _result.AddScriptWarning(
                ScriptIdentifier.NetworkDNSSettingsSet,
                scriptResponse.StandardError);

            return OperationStatus.Warning;
        }

        if (scriptResponse.HasErrors)
        {
            _result.AddScriptError(
                ScriptIdentifier.NetworkDNSSettingsSet,
                scriptResponse.StandardError);

            return OperationStatus.Error;
        }

        return OperationStatus.Success;
    }

    public async Task<OperationStatus> ExecuteSetNetworkMulticastDNSSettingsScript(ScriptOptions scriptOptions)
    {
        var scriptResponse = await ScriptRunner.Run(
            ScriptIdentifier.NetworkMulticastDNSSettingsSet,
            scriptOptions.ToArray());

        if (scriptResponse.HasErrors)
        {
            _result.AddError($"{ScriptIdentifier.NetworkMulticastDNSSettingsSet}: {scriptResponse.StandardError}");
            return OperationStatus.Error;
        }

        return OperationStatus.Success;
    }

    public async Task<(OperationStatus, NetworkNTPSettings)> ExecuteGetNetworkNTPSettingsScript()
    {
        var scriptResponse = await ScriptRunner.Run(ScriptIdentifier.NetworkNTPSettingsGet);

        if (scriptResponse.HasErrors)
        {
            _result.AddScriptError(
                ScriptIdentifier.NetworkNTPSettingsGet,
                scriptResponse.StandardError);

            return (OperationStatus.Error, NetworkNTPSettings.Empty);
        }

        try
        {
            var ntpSettings = JsonSerializer
                .Deserialize(
                    scriptResponse.StandardOutput,
                    SourceGenerationContext.Default.NetworkNTPSettings)
                ?? throw new InvalidOperationException(
                    GetInvalidOperationExceptionMessage(
                        nameof(NetworkNTPSettings),
                        ScriptIdentifier.NetworkNTPSettingsGet,
                        scriptResponse.StandardOutput));

            return (OperationStatus.Success, ntpSettings);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                GetInvalidOperationExceptionMessage(
                    nameof(NetworkNTPSettings),
                    ScriptIdentifier.NetworkNTPSettingsGet,
                    ex.Message));
        }
    }

    public async Task<OperationStatus> ExecuteSetNetworkNTPSettingsScript(ScriptOptions scriptOptions)
    {
        var scriptResponse = await ScriptRunner.Run(
            ScriptIdentifier.NetworkNTPSettingsSet,
            scriptOptions.ToArray());

        if (scriptResponse.HasWarnings)
        {
            _result.AddScriptWarning(
                ScriptIdentifier.NetworkNTPSettingsSet,
                scriptResponse.StandardError);

            return OperationStatus.Warning;
        }

        if (scriptResponse.HasErrors)
        {
            _result.AddScriptError(
                ScriptIdentifier.NetworkNTPSettingsSet,
                scriptResponse.StandardError);

            return OperationStatus.Error;
        }

        return OperationStatus.Success;
    }

    public async Task<OperationStatus> ExecuteRestartNtpServiceScript()
    {
        var scriptResponse = await ScriptRunner.Run(ScriptIdentifier.RestartNtpService);

        if (scriptResponse.HasWarnings)
        {
            _result.AddWarning($"{ScriptIdentifier.RestartNtpService}: {scriptResponse.StandardError}");
            return OperationStatus.Warning;
        }

        if (scriptResponse.HasErrors)
        {
            _result.AddError($"{ScriptIdentifier.RestartNtpService}: {scriptResponse.StandardError}");
            return OperationStatus.Error;
        }

        return OperationStatus.Success;
    }

    public async Task<(OperationStatus, NetworkProxySettings)> ExecuteGetNetworkProxySettingsScript()
    {
        var scriptResponse = await ScriptRunner.Run(ScriptIdentifier.NetworkProxySettingsGet);

        if (scriptResponse.HasErrors)
        {
            _result.AddScriptError(
                ScriptIdentifier.NetworkProxySettingsGet,
                scriptResponse.StandardError);

            return (OperationStatus.Error, new NetworkProxySettings());
        }

        try
        {
            var networkProxySettings = JsonSerializer
                .Deserialize(
                    scriptResponse.StandardOutput,
                    SourceGenerationContext.Default.NetworkProxySettings)
                ?? throw new InvalidOperationException(
                    GetInvalidOperationExceptionMessage(
                        nameof(NetworkProxySettings),
                        ScriptIdentifier.NetworkProxySettingsGet,
                        scriptResponse.StandardOutput));

            return (OperationStatus.Success, networkProxySettings);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                GetInvalidOperationExceptionMessage(
                    nameof(NetworkProxySettings),
                    ScriptIdentifier.NetworkProxySettingsGet,
                    ex.Message));
        }
    }

    public async Task<OperationStatus> ExecuteSetNetworkProxySettingsScript(ScriptOptions scriptOptions)
    {
        var scriptResponse = await ScriptRunner.Run(
            ScriptIdentifier.NetworkProxySettingsSet,
            scriptOptions.ToArray());

        if (scriptResponse.HasErrors)
        {
            _result.AddScriptError(
                ScriptIdentifier.NetworkProxySettingsSet,
                scriptResponse.StandardError);

            return OperationStatus.Error;
        }

        return OperationStatus.Success;
    }

    public async Task<(OperationStatus, ServiceDetail)> ExecuteGetServiceStateScript(ScriptOptions scriptOptions)
    {
        var scriptResponse = await ScriptRunner.Run(
            ScriptIdentifier.ServiceStateGet,
            scriptOptions.ToArray());

        if (scriptResponse.HasErrors)
        {
            _result.AddScriptError(
                ScriptIdentifier.ServiceStateGet,
                scriptResponse.StandardError);

            return (OperationStatus.Error, ServiceDetail.Empty);
        }

        try
        {
            var serviceDetail = JsonSerializer
                .Deserialize(
                    scriptResponse.StandardOutput,
                    SourceGenerationContext.Default.ServiceDetail)
                ?? throw new InvalidOperationException(
                    GetInvalidOperationExceptionMessage(
                        nameof(ServiceDetail),
                        ScriptIdentifier.ServiceStateGet,
                        scriptResponse.StandardOutput));

            return (OperationStatus.Success, serviceDetail);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                GetInvalidOperationExceptionMessage(
                    nameof(ServiceDetail),
                    ScriptIdentifier.ServiceStateGet,
                    ex.Message));
        }
    }

    public async Task<OperationStatus> ExecuteSetServiceStateScript(ScriptOptions scriptOptions)
    {
        var scriptResponse = await ScriptRunner.Run(
            ScriptIdentifier.ServiceStateSet,
            scriptOptions.ToArray());

        if (scriptResponse.HasErrors)
        {
            _result.AddScriptError(
                ScriptIdentifier.ServiceStateSet,
                scriptResponse.StandardError);

            return OperationStatus.Error;
        }

        return OperationStatus.Success;
    }

    private static string GetInvalidOperationExceptionMessage(
        string nameOfType,
        ScriptIdentifier scriptIdentifier,
        string message)
    {
        return
            $"Standard output could not be deserialized to {nameOfType} " +
            $"for script with identifier '{scriptIdentifier}'." +
            Environment.NewLine +
            message;
    }
}

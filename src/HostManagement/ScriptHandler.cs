using System.Text.Json;
using System.Text.Json.Serialization;
using HostManagement.Scripts;
using HostManagement.Scripts.Enums;
using HostManagement.Shared.Communication.Enums;
using HostManagement.SystemConfig.Contracts;

namespace HostManagement;

[JsonSourceGenerationOptions(
    RespectNullableAnnotations = true,
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    UseStringEnumConverter = true)]
[JsonSerializable(typeof(List<NetworkInterfaceCommonInformation>))]
internal partial class ScriptHandlerJsonSerialization : JsonSerializerContext;

/// <inheritdoc cref="IScriptHandler"/>
internal sealed class ScriptHandler(Result? result = default) : IScriptHandler
{
    private Result _result = result ?? new();

    public void UseResult(Result result) => _result = result;

    public async Task<OperationStatus> ExecuteApplyNetplanConfigurationScript()
    {
        var scriptResponse = await ScriptRunner
            .GetInstance()
            .Run(ScriptIdentifier.ApplyNetplanConfiguration);

        if (scriptResponse.HasErrors)
        {
            _result.AddScriptError(
                ScriptIdentifier.ApplyNetplanConfiguration,
                scriptResponse.StandardError);

            return OperationStatus.Error;
        }

        return OperationStatus.Success;
    }

    public async Task<(OperationStatus, string)> ExecuteGetArchitectureScript()
    {
        var scriptResponse = await ScriptRunner
            .GetInstance()
            .Run(ScriptIdentifier.CommonSystemArchitectureGet);

        if (scriptResponse.HasErrors)
        {
            _result.AddScriptError(
                ScriptIdentifier.CommonSystemArchitectureGet,
                scriptResponse.StandardError);

            return (OperationStatus.Error, string.Empty);
        }

        return (OperationStatus.Success, scriptResponse.StandardOutput);
    }

    public async Task<(OperationStatus, List<NetworkInterfaceCommonInformation>)> ExecuteGetNetworkInterfacesCommonInformationScript()
    {
        var hasWarning = false;
        var networkInterfacesCommonInformation = new List<NetworkInterfaceCommonInformation>();
        var scriptResponse = await ScriptRunner
            .GetInstance()
            .Run(ScriptIdentifier.NetworkInterfacesCommonInformationGet);

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
                    ScriptHandlerJsonSerialization.Default.ListNetworkInterfaceCommonInformation)
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

    public async Task<(OperationStatus, string)> ExecuteGetOriginalPhysicalAddressScript(
        string interfaceName)
    {
        var scriptResponse = await ScriptRunner
            .GetInstance()
            .Run(ScriptIdentifier.OriginalPhysicalAddressGet, interfaceName);

        if (scriptResponse.HasErrors)
        {
            _result.AddScriptError(
                ScriptIdentifier.OriginalPhysicalAddressGet,
                scriptResponse.StandardError);

            return (OperationStatus.Error, string.Empty);
        }

        return (OperationStatus.Success, scriptResponse.StandardOutput);
    }

    public async Task<(OperationStatus, IPv4Settings)> ExecuteGetInterfaceIPv4SettingsScript(
        string interfaceName)
    {
        var scriptResponse = await ScriptRunner
            .GetInstance()
            .Run(ScriptIdentifier.NetworkInterfaceIPv4SettingsGet, interfaceName);

        if (scriptResponse.HasErrors)
        {
            _result.AddScriptError(
                ScriptIdentifier.NetworkInterfaceIPv4SettingsGet,
                scriptResponse.StandardError);

            return (OperationStatus.Error, new());
        }

        try
        {
            var iPv4Settings = JsonSerializer
                .Deserialize(
                    scriptResponse.StandardOutput,
                    SystemConfigurationJsonSerializerContext.Default.IPv4Settings)
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
        var scriptResponse = await ScriptRunner
            .GetInstance()
            .Run(ScriptIdentifier.NetworkInterfaceIPv4SettingsSet, scriptOptions.Options);

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
        var scriptResponse = await ScriptRunner
            .GetInstance()
            .Run(ScriptIdentifier.NetworkInterfaceIPv4DHCPLeaseGet, interfaceName);

        if (scriptResponse.HasErrors)
        {
            _result.AddScriptError(
                ScriptIdentifier.NetworkInterfaceIPv4DHCPLeaseGet,
                scriptResponse.StandardError);

            return (OperationStatus.Error, new());
        }

        try
        {
            var dhcpLease = JsonSerializer
                .Deserialize(
                    scriptResponse.StandardOutput,
                    SystemConfigurationJsonSerializerContext.Default.DHCPLease)
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

    public async Task<OperationStatus> ExecuteRenewDHCPLeaseScript(string interfaceName)
    {
        var scriptResponse = await ScriptRunner
            .GetInstance()
            .Run(ScriptIdentifier.RenewDHCPLease, interfaceName);

        if (scriptResponse.HasErrors)
        {
            _result.AddScriptError(
                ScriptIdentifier.RenewDHCPLease,
                scriptResponse.StandardError);

            return OperationStatus.Error;
        }

        return OperationStatus.Success;
    }

    public async Task<(OperationStatus, NetworkDNSSettings)> ExecuteGetNetworkDNSSettingsScript()
    {
        var scriptResponse = await ScriptRunner
            .GetInstance()
            .Run(ScriptIdentifier.NetworkDNSSettingsGet);

        if (scriptResponse.HasErrors)
        {
            _result.AddScriptError(
                ScriptIdentifier.NetworkDNSSettingsGet,
                scriptResponse.StandardError);

            return (OperationStatus.Error, new());
        }

        try
        {
            var dnsSettings = JsonSerializer
                .Deserialize(
                    scriptResponse.StandardOutput,
                    SystemConfigurationJsonSerializerContext.Default.NetworkDNSSettings)
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
        var scriptResponse = await ScriptRunner
            .GetInstance()
            .Run(ScriptIdentifier.NetworkStaticHostsSet, scriptOptions.Options);

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
        var scriptResponse = await ScriptRunner
            .GetInstance()
            .Run(ScriptIdentifier.NetworkDNSSettingsSet, scriptOptions.Options);

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
        var scriptResponse = await ScriptRunner
            .GetInstance()
            .Run(ScriptIdentifier.NetworkMulticastDNSSettingsSet, scriptOptions.Options);

        if (scriptResponse.HasErrors)
        {
            _result.AddError($"{ScriptIdentifier.NetworkMulticastDNSSettingsSet}: {scriptResponse.StandardError}");
            return OperationStatus.Error;
        }

        return OperationStatus.Success;
    }

    public async Task<OperationStatus> ExecuteSetHostnameScript(string hostname)
    {
        var scriptResponse = await ScriptRunner
            .GetInstance()
            .Run(ScriptIdentifier.HostnameSet, hostname);

        if (scriptResponse.HasErrors)
        {
            _result.AddError($"{ScriptIdentifier.HostnameSet}: {scriptResponse.StandardError}");
            return OperationStatus.Error;
        }

        return OperationStatus.Success;
    }

    public async Task<(OperationStatus, IEnumerable<string>)> ExecuteGetNetworkNTPFallbackInformationScript()
    {
        var scriptResponse = await ScriptRunner
            .GetInstance()
            .Run(ScriptIdentifier.NetworkNTPFallbackInformationGet);

        if (scriptResponse.HasErrors)
        {
            _result.AddScriptError(
                ScriptIdentifier.NetworkNTPFallbackInformationGet,
                scriptResponse.StandardError);

            return (OperationStatus.Error, []);
        }

        return (OperationStatus.Success, scriptResponse.StandardOutput.Split(' ').AsEnumerable());
    }

    public async Task<(OperationStatus, NetworkNTPSettings)> ExecuteGetNetworkNTPSettingsScript()
    {
        var scriptResponse = await ScriptRunner
            .GetInstance()
            .Run(ScriptIdentifier.NetworkNTPSettingsGet);

        if (scriptResponse.HasErrors)
        {
            _result.AddScriptError(
                ScriptIdentifier.NetworkNTPSettingsGet,
                scriptResponse.StandardError);

            return (OperationStatus.Error, new());
        }

        try
        {
            var ntpSettings = JsonSerializer
                .Deserialize(
                    scriptResponse.StandardOutput,
                    SystemConfigurationJsonSerializerContext.Default.NetworkNTPSettings)
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
        var scriptResponse = await ScriptRunner
            .GetInstance()
            .Run(ScriptIdentifier.NetworkNTPSettingsSet, scriptOptions.Options);

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

    public async Task<(OperationStatus, NetworkProxySettings)> ExecuteGetNetworkProxySettingsScript()
    {
        var scriptResponse = await ScriptRunner
            .GetInstance()
            .Run(ScriptIdentifier.NetworkProxySettingsGet);

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
                    SystemConfigurationJsonSerializerContext.Default.NetworkProxySettings)
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
        var scriptResponse = await ScriptRunner
            .GetInstance()
            .Run(ScriptIdentifier.NetworkProxySettingsSet, scriptOptions.Options);

        if (scriptResponse.HasErrors)
        {
            _result.AddScriptError(
                ScriptIdentifier.NetworkProxySettingsSet,
                scriptResponse.StandardError);

            return OperationStatus.Error;
        }

        return OperationStatus.Success;
    }

    public async Task<(OperationStatus, string)> ExecuteGetSystemdUnitActiveStateScript(ScriptOptions scriptOptions)
    {
        var scriptResponse = await ScriptRunner
            .GetInstance()
            .Run(ScriptIdentifier.SystemdUnitActiveStateGet, scriptOptions.Options);

        if (scriptResponse.HasErrors)
        {
            _result.AddScriptError(ScriptIdentifier.SystemdUnitActiveStateGet, scriptResponse.StandardError);
            return (OperationStatus.Error, "unknown");
        }

        return (OperationStatus.Success, scriptResponse.StandardOutput);
    }

    public async Task<(OperationStatus, ServiceDetail)> ExecuteGetSystemdUnitStateScript(ScriptOptions scriptOptions)
    {
        var scriptResponse = await ScriptRunner
            .GetInstance()
            .Run(ScriptIdentifier.SystemdUnitStateGet, scriptOptions.Options);

        if (scriptResponse.HasErrors)
        {
            _result.AddScriptError(ScriptIdentifier.SystemdUnitStateGet, scriptResponse.StandardError);
            return (OperationStatus.Error, new() { Name = "Empty", State = ServiceState.Unknown, });
        }

        try
        {
            var serviceDetail = JsonSerializer
                .Deserialize(
                    scriptResponse.StandardOutput,
                    SystemConfigurationJsonSerializerContext.Default.ServiceDetail)
                ?? throw new InvalidOperationException(
                    GetInvalidOperationExceptionMessage(
                        nameof(ServiceDetail),
                        ScriptIdentifier.SystemdUnitStateGet,
                        scriptResponse.StandardOutput));

            return (OperationStatus.Success, serviceDetail);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                GetInvalidOperationExceptionMessage(
                    nameof(ServiceDetail),
                    ScriptIdentifier.SystemdUnitStateGet,
                    ex.Message));
        }
    }

    public async Task<OperationStatus> ExecuteSetSystemdUnitStateScript(ScriptOptions scriptOptions)
    {
        var scriptResponse = await ScriptRunner
            .GetInstance()
            .Run(ScriptIdentifier.SystemdUnitStateSet, scriptOptions.Options);

        if (scriptResponse.HasErrors)
        {
            _result.AddScriptError(ScriptIdentifier.SystemdUnitStateSet, scriptResponse.StandardError);
            return OperationStatus.Error;
        }

        return OperationStatus.Success;
    }

    public async Task<OperationStatus> ExecuteInstallDebianPackageScript(string filePath)
    {
        var scriptOptions = new ScriptOptions();

        scriptOptions.Add(
            ScriptOptionNames.InstallDebianPackage.DebianPackage,
            filePath);

        var scriptResponse = await ScriptRunner
            .GetInstance()
            .Run(ScriptIdentifier.InstallDebianPackage, scriptOptions.Options);

        if (scriptResponse.HasErrors)
        {
            _result.AddScriptError(
                ScriptIdentifier.InstallDebianPackage,
                scriptResponse.StandardError);

            return OperationStatus.Error;
        }

        return OperationStatus.Success;
    }

    public async Task<OperationStatus> ExecuteUpdateSystemScript(string filePath)
    {
        var scriptOptions = new ScriptOptions();

        scriptOptions.Add(
            ScriptOptionNames.UpdateSystem.FilePath,
            filePath);

        var scriptResponse = await ScriptRunner
            .GetInstance()
            .Run(ScriptIdentifier.UpdateSystem, scriptOptions.Options);

        if (scriptResponse.HasErrors)
        {
            _result.AddScriptError(
                ScriptIdentifier.UpdateSystem,
                scriptResponse.StandardError);

            return OperationStatus.Error;
        }

        return OperationStatus.Success;
    }

    public async Task<OperationStatus> ExecuteResetSystemScript()
    {
        var scriptResponse = await ScriptRunner
            .GetInstance()
            .Run(ScriptIdentifier.ResetSystem);

        if (scriptResponse.HasErrors)
        {
            _result.AddScriptError(
                ScriptIdentifier.ResetSystem,
                scriptResponse.StandardError);

            return OperationStatus.Error;
        }

        return OperationStatus.Success;
    }

    public async Task<OperationStatus> ExecuteRestartSystemScript()
    {
        var scriptResponse = await ScriptRunner
            .GetInstance()
            .Run(ScriptIdentifier.RestartSystem);

        if (scriptResponse.HasErrors)
        {
            _result.AddScriptError(
                ScriptIdentifier.RestartSystem,
                scriptResponse.StandardError);

            return OperationStatus.Error;
        }

        return OperationStatus.Success;
    }

    public async Task<OperationStatus> ExecuteShutdownSystemScript()
    {
        var scriptResponse = await ScriptRunner
            .GetInstance()
            .Run(ScriptIdentifier.ShutdownSystem);

        if (scriptResponse.HasErrors)
        {
            _result.AddScriptError(
                ScriptIdentifier.ShutdownSystem,
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

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using HostManagement.Scripts;
using HostManagement.Scripts.Enums;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Contracts.NetworkStatus;
using HostManagement.SystemConfig.Contracts;
using SharedCommunicationSourceGenerationContext = HostManagement.Shared.Communication.Contracts.SourceGenerationContext;

namespace HostManagement;

/// <summary>
/// JSON source generation context for <see cref="List{T}"/> of <see cref="NetworkInterfaceCommonInformation"/>.
/// </summary>
[JsonSourceGenerationOptions(
    RespectNullableAnnotations = true,
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    UseStringEnumConverter = true)]
[JsonSerializable(typeof(List<NetworkInterfaceCommonInformation>))]
internal partial class CommonInformationListJsonSerializerContext : JsonSerializerContext;

/// <inheritdoc cref="IScriptHandler"/>
internal sealed class ScriptHandler(Result? result = default) : IScriptHandler
{
    private Result _result = result ?? new();

    public void UseResult(Result result) => _result = result;

    public async Task<OperationStatus> ExecuteApplyNetplanConfigurationScript()
        => await ExecuteScript(ScriptIdentifier.ApplyNetplanConfiguration, includeWarnings: false);

    public async Task<OperationStatus> ExecuteIsNetworkInterfaceAvailableScript(string interfaceName)
        => await ExecuteScript(ScriptIdentifier.IsNetworkInterfaceAvailable, interfaceName);

    public async Task<(OperationStatus, string)> ExecuteGetArchitectureScript()
        => await ExecuteScriptAndReturnRawStandardOutput(ScriptIdentifier.CommonSystemArchitectureGet);

    public async Task<(OperationStatus, string)> ExecuteGetNetworkInterfacesScript(string interfaceType = "")
    {
        return string.IsNullOrWhiteSpace(interfaceType)
            ? await ExecuteScriptAndReturnRawStandardOutput(ScriptIdentifier.NetworkInterfacesGet)
            : await ExecuteScriptAndReturnRawStandardOutput(ScriptIdentifier.NetworkInterfacesGet, interfaceType);
    }

    public async Task<(OperationStatus, List<NetworkInterfaceCommonInformation>)> ExecuteGetNetworkInterfacesCommonInformationScript(
        ScriptOptions scriptOptions)
        => await ExecuteScriptAndReturnDeserializedType(
            CommonInformationListJsonSerializerContext.Default.ListNetworkInterfaceCommonInformation,
            ScriptIdentifier.NetworkInterfacesCommonInformationGet,
            scriptOptions.Options);

    public async Task<(OperationStatus, NetworkStatusInformation)> ExecuteGetNetworkStatusInformationScript(
        ScriptOptions scriptOptions)
        => await ExecuteScriptAndReturnDeserializedType(
            SharedCommunicationSourceGenerationContext.Default.NetworkStatusInformation,
            ScriptIdentifier.NetworkStatusInformationGet,
            scriptOptions.Options,
            () => NetworkStatusInformation.Empty);

    public async Task<(OperationStatus, string)> ExecuteGetOriginalPhysicalAddressScript(string interfaceName)
        => await ExecuteScriptAndReturnRawStandardOutput(ScriptIdentifier.OriginalPhysicalAddressGet, interfaceName);

    public async Task<(OperationStatus, IPv4Settings)> ExecuteGetInterfaceIPv4SettingsScript(string interfaceName)
        => await ExecuteScriptAndReturnDeserializedType(
            SystemConfigurationJsonSerializerContext.Default.IPv4Settings,
            ScriptIdentifier.NetworkInterfaceIPv4SettingsGet,
            interfaceName);

    public async Task<OperationStatus> ExecuteSetInterfaceIPv4SettingsScript(ScriptOptions scriptOptions)
        => await ExecuteScript(ScriptIdentifier.NetworkInterfaceIPv4SettingsSet, scriptOptions.Options);

    public async Task<(OperationStatus, DHCPLease)> ExecuteGetInterfaceIPv4DHCPLeaseScript(string interfaceName)
        => await ExecuteScriptAndReturnDeserializedType(
            SystemConfigurationJsonSerializerContext.Default.DHCPLease,
            ScriptIdentifier.NetworkInterfaceIPv4DHCPLeaseGet,
            interfaceName);

    public async Task<OperationStatus> ExecuteRenewDHCPLeaseScript(string interfaceName)
        => await ExecuteScript(ScriptIdentifier.RenewDHCPLease, interfaceName);

    public async Task<(OperationStatus, NetworkDNSSettings)> ExecuteGetNetworkDNSSettingsScript()
        => await ExecuteScriptAndReturnDeserializedType(
            SystemConfigurationJsonSerializerContext.Default.NetworkDNSSettings,
            ScriptIdentifier.NetworkDNSSettingsGet);

    public async Task<OperationStatus> ExecuteSetStaticHostsScript(ScriptOptions scriptOptions)
        => await ExecuteScript(ScriptIdentifier.NetworkStaticHostsSet, scriptOptions.Options);

    public async Task<OperationStatus> ExecuteSetNetworkDNSSettingsScript(ScriptOptions scriptOptions)
        => await ExecuteScript(ScriptIdentifier.NetworkDNSSettingsSet, scriptOptions.Options);

    public async Task<OperationStatus> ExecuteSetNetworkMulticastDNSSettingsScript(ScriptOptions scriptOptions)
        => await ExecuteScript(ScriptIdentifier.NetworkMulticastDNSSettingsSet, scriptOptions.Options);

    public async Task<OperationStatus> ExecuteSetHostnameScript(string hostname)
        => await ExecuteScript(ScriptIdentifier.HostnameSet, hostname);

    public async Task<(OperationStatus, IEnumerable<string>)> ExecuteGetNetworkNTPFallbackInformationScript()
        => await ExecuteScriptAndReturnSplittedStandardOutput(ScriptIdentifier.NetworkNTPFallbackInformationGet);

    public async Task<(OperationStatus, NetworkNTPSettings)> ExecuteGetNetworkNTPSettingsScript()
        => await ExecuteScriptAndReturnDeserializedType(
            SystemConfigurationJsonSerializerContext.Default.NetworkNTPSettings,
            ScriptIdentifier.NetworkNTPSettingsGet);

    public async Task<OperationStatus> ExecuteSetNetworkNTPSettingsScript(ScriptOptions scriptOptions)
        => await ExecuteScript(ScriptIdentifier.NetworkNTPSettingsSet, scriptOptions.Options);

    public async Task<(OperationStatus, NetworkProxySettings)> ExecuteGetNetworkProxySettingsScript()
        => await ExecuteScriptAndReturnDeserializedType(
            SystemConfigurationJsonSerializerContext.Default.NetworkProxySettings,
            ScriptIdentifier.NetworkProxySettingsGet);

    public async Task<OperationStatus> ExecuteSetNetworkProxySettingsScript(ScriptOptions scriptOptions)
        => await ExecuteScript(ScriptIdentifier.NetworkProxySettingsSet, scriptOptions.Options);

    public async Task<(OperationStatus, string)> ExecuteGetSystemdUnitActiveStateScript(ScriptOptions scriptOptions)
        => await ExecuteScriptAndReturnRawStandardOutput(
            ScriptIdentifier.SystemdUnitActiveStateGet,
            scriptOptions.Options);

    public async Task<(OperationStatus, ServiceDetail)> ExecuteGetSystemdUnitStateScript(ScriptOptions scriptOptions)
        => await ExecuteScriptAndReturnDeserializedType(
            SystemConfigurationJsonSerializerContext.Default.ServiceDetail,
            ScriptIdentifier.SystemdUnitStateGet,
            scriptOptions.Options,
            () => new() { Name = "Empty", State = ServiceState.Unknown, });

    public async Task<OperationStatus> ExecuteSetSystemdUnitStateScript(ScriptOptions scriptOptions)
        => await ExecuteScript(ScriptIdentifier.SystemdUnitStateSet, scriptOptions.Options);

    public async Task<OperationStatus> ExecuteInstallDebianPackageScript(ScriptOptions scriptOptions)
        => await ExecuteScript(
            ScriptIdentifier.InstallDebianPackage,
            scriptOptions.Options,
            timeoutInMilliseconds: 180_000);

    public async Task<OperationStatus> ExecuteUpdateSystemScript(ScriptOptions scriptOptions)
        => await ExecuteScript(ScriptIdentifier.UpdateSystem, scriptOptions.Options);

    public async Task<OperationStatus> ExecuteResetSystemScript()
        => await ExecuteScript(ScriptIdentifier.ResetSystem);

    public async Task<OperationStatus> ExecuteShutdownSystemScript(ScriptOptions scriptOptions)
        => await ExecuteScript(ScriptIdentifier.ShutdownSystem, scriptOptions.Options);

    private static string GetInvalidOperationExceptionMessage(
        string nameOfType,
        ScriptIdentifier scriptIdentifier,
        string message)
    {
        return
            $"Standard output could not be deserialized to '{nameOfType}' " +
            $"for script with identifier '{scriptIdentifier}'." +
            Environment.NewLine +
            message;
    }

    private static IEnumerable<string> ToArgs(string? arg) => arg is null ? [] : [arg];

    private async Task<(OperationStatus, T)> ExecuteScriptAndReturnDeserializedType<T>(
        JsonTypeInfo<T> jsonTypeInfo,
        ScriptIdentifier scriptIdentifier,
        string? scriptArgument = null,
        bool includeWarnings = true) where T : class, new()
        => await ExecuteScriptAndReturnDeserializedType(
            jsonTypeInfo,
            scriptIdentifier,
            ToArgs(scriptArgument),
            includeWarnings);

    private async Task<(OperationStatus, T)> ExecuteScriptAndReturnDeserializedType<T>(
        JsonTypeInfo<T> jsonTypeInfo,
        ScriptIdentifier scriptIdentifier,
        IEnumerable<string> scriptArguments,
        bool includeWarnings = true) where T : class, new()
        => await ExecuteScriptAndReturnDeserializedType(
            jsonTypeInfo,
            scriptIdentifier,
            scriptArguments,
            () => new T(),
            includeWarnings);

    private async Task<(OperationStatus, T)> ExecuteScriptAndReturnDeserializedType<T>(
        JsonTypeInfo<T> jsonTypeInfo,
        ScriptIdentifier scriptIdentifier,
        IEnumerable<string> scriptArguments,
        Func<T> newValueFactory,
        bool includeWarnings = true) where T : class
    {
        var (status, scriptResponse) = await RunScriptAsync(scriptIdentifier, scriptArguments, includeWarnings);

        if (status is OperationStatus.Error)
            return (OperationStatus.Error, newValueFactory());

        try
        {
            var result = JsonSerializer
                .Deserialize(scriptResponse.StandardOutput, jsonTypeInfo)
                ?? throw new InvalidOperationException(
                    GetInvalidOperationExceptionMessage(
                        typeof(T).Name,
                        scriptIdentifier,
                        scriptResponse.StandardOutput));

            return (status, result);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                GetInvalidOperationExceptionMessage(typeof(T).Name, scriptIdentifier, ex.Message));
        }
    }

    private async Task<OperationStatus> ExecuteScript(
        ScriptIdentifier scriptIdentifier,
        string? scriptArgument = null,
        bool includeWarnings = true,
        int? timeoutInMilliseconds = null)
        => await ExecuteScript(scriptIdentifier, ToArgs(scriptArgument), includeWarnings, timeoutInMilliseconds);

    private async Task<OperationStatus> ExecuteScript(
        ScriptIdentifier scriptIdentifier,
        IEnumerable<string> scriptArguments,
        bool includeWarnings = true,
        int? timeoutInMilliseconds = null)
    {
        var (status, _) = await RunScriptAsync(scriptIdentifier, scriptArguments, includeWarnings, timeoutInMilliseconds);
        return status;
    }

    private async Task<(OperationStatus, string)> ExecuteScriptAndReturnRawStandardOutput(
        ScriptIdentifier scriptIdentifier,
        string? scriptArgument = null,
        bool includeWarnings = true)
        => await ExecuteScriptAndReturnRawStandardOutput(scriptIdentifier, ToArgs(scriptArgument), includeWarnings);

    private async Task<(OperationStatus, string)> ExecuteScriptAndReturnRawStandardOutput(
        ScriptIdentifier scriptIdentifier,
        IEnumerable<string> scriptArguments,
        bool includeWarnings = true)
    {
        var (status, scriptResponse) = await RunScriptAsync(scriptIdentifier, scriptArguments, includeWarnings);

        if (status is OperationStatus.Error)
            return (OperationStatus.Error, string.Empty);

        return (status, scriptResponse.StandardOutput);
    }

    private async Task<(OperationStatus, IEnumerable<string>)> ExecuteScriptAndReturnSplittedStandardOutput(
        ScriptIdentifier scriptIdentifier,
        string? scriptArgument = null,
        char separator = ' ',
        bool includeWarnings = true)
        => await ExecuteScriptAndReturnSplittedStandardOutput(
            scriptIdentifier,
            ToArgs(scriptArgument),
            separator,
            includeWarnings);

    private async Task<(OperationStatus, IEnumerable<string>)> ExecuteScriptAndReturnSplittedStandardOutput(
        ScriptIdentifier scriptIdentifier,
        IEnumerable<string> scriptArguments,
        char separator = ' ',
        bool includeWarnings = true)
    {
        var (status, scriptResponse) = await RunScriptAsync(scriptIdentifier, scriptArguments, includeWarnings);

        if (status is OperationStatus.Error)
            return (OperationStatus.Error, []);

        var values = (scriptResponse.StandardOutput ?? string.Empty)
            .Split(separator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .AsEnumerable();

        return (status, values);
    }

    private async Task<(OperationStatus Status, ScriptResponse ScriptResponse)> RunScriptAsync(
        ScriptIdentifier scriptIdentifier,
        IEnumerable<string> scriptArguments,
        bool includeWarnings,
        int? timeoutInMilliseconds = null)
    {
        var scriptResponse = timeoutInMilliseconds.HasValue
            ? await ScriptRunner.GetInstance().Run(timeoutInMilliseconds.Value, scriptIdentifier, scriptArguments)
            : await ScriptRunner.GetInstance().Run(scriptIdentifier, scriptArguments);

        if (scriptResponse.HasErrors)
        {
            _result.AddScriptError(scriptIdentifier, scriptResponse.StandardError);
            return (OperationStatus.Error, scriptResponse);
        }

        if (includeWarnings && scriptResponse.HasWarnings)
        {
            _result.AddScriptWarning(scriptIdentifier, scriptResponse.StandardError);
            return (OperationStatus.Warning, scriptResponse);
        }

        return (OperationStatus.Success, scriptResponse);
    }
}

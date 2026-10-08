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
    UseStringEnumConverter = true,
    Converters = [typeof(PhysicalAddressConverter)])]
[JsonSerializable(typeof(List<NetworkInterfaceCommonInformation>))]
internal partial class CommonInformationListJsonSerializerContext : JsonSerializerContext;

/// <inheritdoc cref="IScriptHandler"/>
internal sealed class ScriptHandler : IScriptHandler
{
    private readonly Func<int?, ScriptIdentifier, IEnumerable<string>, Task<ScriptResponse>> _runScript;

    internal ScriptHandler() : this(RunScriptUsingDefaultRunnerAsync) { }

    internal ScriptHandler(Func<int?, ScriptIdentifier, IEnumerable<string>, Task<ScriptResponse>> runScript)
        => _runScript = runScript
        ?? throw new ArgumentNullException(nameof(runScript));

    public async Task<Result> ExecuteApplyNetplanConfigurationScript()
        => await ExecuteScript(ScriptIdentifier.ApplyNetplanConfiguration, includeWarnings: false);

    public async Task<Result> ExecuteIsNetworkInterfaceAvailableScript(string interfaceName)
        => await ExecuteScript(ScriptIdentifier.IsNetworkInterfaceAvailable, interfaceName);

    public async Task<(Result, string)> ExecuteGetNetworkInterfacesScript(string interfaceType = "")
    {
        return string.IsNullOrWhiteSpace(interfaceType)
            ? await ExecuteScriptAndReturnRawStandardOutput(ScriptIdentifier.NetworkInterfacesGet)
            : await ExecuteScriptAndReturnRawStandardOutput(ScriptIdentifier.NetworkInterfacesGet, interfaceType);
    }

    public async Task<(Result, List<NetworkInterfaceCommonInformation>)>
        ExecuteGetNetworkInterfacesCommonInformationScript(ScriptOptions scriptOptions)
            => await ExecuteScriptAndReturnDeserializedType(
                CommonInformationListJsonSerializerContext.Default.ListNetworkInterfaceCommonInformation,
                ScriptIdentifier.NetworkInterfacesCommonInformationGet,
                scriptOptions.Options);

    public async Task<(Result, NetworkStatusInformation)> ExecuteGetNetworkStatusInformationScript(
        ScriptOptions scriptOptions)
        => await ExecuteScriptAndReturnDeserializedType(
            SharedCommunicationSourceGenerationContext.Default.NetworkStatusInformation,
            ScriptIdentifier.NetworkStatusInformationGet,
            scriptOptions.Options,
            () => NetworkStatusInformation.Empty);

    public async Task<(Result, string)> ExecuteGetOriginalPhysicalAddressScript(string interfaceName)
        => await ExecuteScriptAndReturnRawStandardOutput(ScriptIdentifier.OriginalPhysicalAddressGet, interfaceName);

    public async Task<(Result, IPv4Settings, VLANSettings)> ExecuteGetInterfaceSettingsScript(string interfaceName)
        => await ExecuteScriptAndReturnDeserializedTypes(
            SystemConfigurationJsonSerializerContext.Default.IPv4Settings,
            SystemConfigurationJsonSerializerContext.Default.VLANSettings,
            ScriptIdentifier.NetworkInterfaceSettingsGet,
            interfaceName);

    public async Task<Result> ExecuteSetInterfaceSettingsScript(ScriptOptions scriptOptions)
        => await ExecuteScript(ScriptIdentifier.NetworkInterfaceSettingsSet, scriptOptions.Options);

    public async Task<(Result, DHCPLease)> ExecuteGetInterfaceIPv4DHCPLeaseScript(string interfaceName)
        => await ExecuteScriptAndReturnDeserializedType(
            SystemConfigurationJsonSerializerContext.Default.DHCPLease,
            ScriptIdentifier.NetworkInterfaceIPv4DHCPLeaseGet,
            interfaceName);

    public async Task<Result> ExecuteRenewDHCPLeaseScript(string interfaceName)
        => await ExecuteScript(ScriptIdentifier.RenewDHCPLease, interfaceName);

    public async Task<(Result, NetworkDNSSettings)> ExecuteGetNetworkDNSSettingsScript()
        => await ExecuteScriptAndReturnDeserializedType(
            SystemConfigurationJsonSerializerContext.Default.NetworkDNSSettings,
            ScriptIdentifier.NetworkDNSSettingsGet);

    public async Task<Result> ExecuteSetStaticHostsScript(ScriptOptions scriptOptions)
        => await ExecuteScript(ScriptIdentifier.NetworkStaticHostsSet, scriptOptions.Options);

    public async Task<Result> ExecuteSetNetworkDNSSettingsScript(ScriptOptions scriptOptions)
        => await ExecuteScript(ScriptIdentifier.NetworkDNSSettingsSet, scriptOptions.Options);

    public async Task<Result> ExecuteSetNetworkMulticastDNSSettingsScript(ScriptOptions scriptOptions)
        => await ExecuteScript(ScriptIdentifier.NetworkMulticastDNSSettingsSet, scriptOptions.Options);

    public async Task<Result> ExecuteSetHostnameScript(string hostname)
        => await ExecuteScript(ScriptIdentifier.HostnameSet, hostname);

    public async Task<(Result, IEnumerable<string>)> ExecuteGetNetworkNTPFallbackInformationScript()
        => await ExecuteScriptAndReturnSplittedStandardOutput(ScriptIdentifier.NetworkNTPFallbackInformationGet);

    public async Task<(Result, NetworkNTPSettings)> ExecuteGetNetworkNTPSettingsScript()
        => await ExecuteScriptAndReturnDeserializedType(
            SystemConfigurationJsonSerializerContext.Default.NetworkNTPSettings,
            ScriptIdentifier.NetworkNTPSettingsGet);

    public async Task<Result> ExecuteSetNetworkNTPSettingsScript(ScriptOptions scriptOptions)
        => await ExecuteScript(ScriptIdentifier.NetworkNTPSettingsSet, scriptOptions.Options);

    public async Task<(Result, NetworkProxySettings)> ExecuteGetNetworkProxySettingsScript()
        => await ExecuteScriptAndReturnDeserializedType(
            SystemConfigurationJsonSerializerContext.Default.NetworkProxySettings,
            ScriptIdentifier.NetworkProxySettingsGet);

    public async Task<Result> ExecuteSetNetworkProxySettingsScript(ScriptOptions scriptOptions)
        => await ExecuteScript(ScriptIdentifier.NetworkProxySettingsSet, scriptOptions.Options);

    public async Task<(Result, SyslogSettings)> ExecuteGetSyslogSettingsScript()
        => await ExecuteScriptAndReturnDeserializedType(
            SystemConfigurationJsonSerializerContext.Default.SyslogSettings,
            ScriptIdentifier.SyslogSettingsGet);

    public async Task<Result> ExecuteSetSyslogSettingsScript(ScriptOptions scriptOptions)
        => await ExecuteScript(ScriptIdentifier.SyslogSettingsSet, scriptOptions.Options);

    public async Task<(Result, string)> ExecuteGetSystemdUnitActiveStateScript(ScriptOptions scriptOptions)
        => await ExecuteScriptAndReturnRawStandardOutput(
            ScriptIdentifier.SystemdUnitActiveStateGet,
            scriptOptions.Options);

    public async Task<(Result, ServiceDetail)> ExecuteGetSystemdUnitStateScript(ScriptOptions scriptOptions)
        => await ExecuteScriptAndReturnDeserializedType(
            SystemConfigurationJsonSerializerContext.Default.ServiceDetail,
            ScriptIdentifier.SystemdUnitStateGet,
            scriptOptions.Options,
            () => new() { Name = "Empty", State = ServiceState.Unknown });

    public async Task<Result> ExecuteSetSystemdUnitStateScript(ScriptOptions scriptOptions)
        => await ExecuteScript(ScriptIdentifier.SystemdUnitStateSet, scriptOptions.Options);

    public async Task<Result> ExecuteInstallDebianPackageScript(ScriptOptions scriptOptions)
        => await ExecuteScript(
            ScriptIdentifier.InstallDebianPackage,
            scriptOptions.Options,
            timeoutInMilliseconds: 180_000);

    public async Task<Result> ExecuteUpdateSystemScript(ScriptOptions scriptOptions)
        => await ExecuteScript(ScriptIdentifier.UpdateSystem, scriptOptions.Options);

    public async Task<Result> ExecuteResetSystemScript()
        => await ExecuteScript(ScriptIdentifier.ResetSystem);

    public async Task<Result> ExecuteShutdownSystemScript(ScriptOptions scriptOptions)
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

    private async Task<(Result, T)> ExecuteScriptAndReturnDeserializedType<T>(
        JsonTypeInfo<T> jsonTypeInfo,
        ScriptIdentifier scriptIdentifier,
        string? scriptArgument = null,
        bool includeWarnings = true) where T : class, new()
            => await ExecuteScriptAndReturnDeserializedType(
                jsonTypeInfo,
                scriptIdentifier,
                ToArgs(scriptArgument),
                includeWarnings);

    private async Task<(Result, T)> ExecuteScriptAndReturnDeserializedType<T>(
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

    private async Task<(Result, T)> ExecuteScriptAndReturnDeserializedType<T>(
        JsonTypeInfo<T> jsonTypeInfo,
        ScriptIdentifier scriptIdentifier,
        IEnumerable<string> scriptArguments,
        Func<T> newValueFactory,
        bool includeWarnings = true)
            where T : class
    {
        var (result, scriptResponse) = await RunScriptAsync(scriptIdentifier, scriptArguments, includeWarnings);

        if (result.OperationStatus is OperationStatus.Error)
            return (result, newValueFactory());

        try
        {
            var typeResult = JsonSerializer
                .Deserialize(scriptResponse.StandardOutput, jsonTypeInfo)
                ?? throw new InvalidOperationException(
                    GetInvalidOperationExceptionMessage(
                        typeof(T).Name,
                        scriptIdentifier,
                        scriptResponse.StandardOutput));

            return (result, typeResult);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                GetInvalidOperationExceptionMessage(typeof(T).Name, scriptIdentifier, ex.Message));
        }
    }

    private async Task<(Result, TFirst, TSecond)> ExecuteScriptAndReturnDeserializedTypes<TFirst, TSecond>(
        JsonTypeInfo<TFirst> firstJsonTypeInfo,
        JsonTypeInfo<TSecond> secondJsonTypeInfo,
        ScriptIdentifier scriptIdentifier,
        string? scriptArgument = null,
        bool includeWarnings = true)
            where TFirst : class, new()
            where TSecond : class, new()
            => await ExecuteScriptAndReturnDeserializedTypes(
                firstJsonTypeInfo,
                secondJsonTypeInfo,
                scriptIdentifier,
                ToArgs(scriptArgument),
                includeWarnings);

    private async Task<(Result, TFirst, TSecond)> ExecuteScriptAndReturnDeserializedTypes<TFirst, TSecond>(
        JsonTypeInfo<TFirst> firstJsonTypeInfo,
        JsonTypeInfo<TSecond> secondJsonTypeInfo,
        ScriptIdentifier scriptIdentifier,
        IEnumerable<string> scriptArguments,
        bool includeWarnings = true)
            where TFirst : class, new()
            where TSecond : class, new()
    {
        var (result, scriptResponse) =
            await RunScriptAsync(scriptIdentifier, scriptArguments, includeWarnings);

        if (result.OperationStatus is OperationStatus.Error)
            return (result, new TFirst(), new TSecond());

        try
        {
            using var document = JsonDocument.Parse(scriptResponse.StandardOutput);
            var rootElement = document.RootElement;

            var firstResult = DeserializeStandardOutputSection(rootElement, firstJsonTypeInfo);
            var secondResult = DeserializeStandardOutputSection(rootElement, secondJsonTypeInfo);

            return (result, firstResult, secondResult);
        }
        catch (JsonException ex)
        {
            var typeNames = $"{typeof(TFirst).Name} and {typeof(TSecond).Name}";

            throw new InvalidOperationException(
                GetInvalidOperationExceptionMessage(typeNames, scriptIdentifier, ex.Message),
                ex);
        }
    }

    private async Task<Result> ExecuteScript(
        ScriptIdentifier scriptIdentifier,
        string? scriptArgument = null,
        bool includeWarnings = true,
        int? timeoutInMilliseconds = null)
            => await ExecuteScript(scriptIdentifier, ToArgs(scriptArgument), includeWarnings, timeoutInMilliseconds);

    private async Task<Result> ExecuteScript(
        ScriptIdentifier scriptIdentifier,
        IEnumerable<string> scriptArguments,
        bool includeWarnings = true,
        int? timeoutInMilliseconds = null)
    {
        var (result, _) = await RunScriptAsync(scriptIdentifier, scriptArguments, includeWarnings, timeoutInMilliseconds);
        return result;
    }

    private async Task<(Result, string)> ExecuteScriptAndReturnRawStandardOutput(
        ScriptIdentifier scriptIdentifier,
        string? scriptArgument = null,
        bool includeWarnings = true)
            => await ExecuteScriptAndReturnRawStandardOutput(scriptIdentifier, ToArgs(scriptArgument), includeWarnings);

    private async Task<(Result, string)> ExecuteScriptAndReturnRawStandardOutput(
        ScriptIdentifier scriptIdentifier,
        IEnumerable<string> scriptArguments,
        bool includeWarnings = true)
    {
        var (result, scriptResponse) = await RunScriptAsync(scriptIdentifier, scriptArguments, includeWarnings);

        if (result.OperationStatus is OperationStatus.Error)
            return (result, string.Empty);

        return (result, scriptResponse.StandardOutput);
    }

    private async Task<(Result, IEnumerable<string>)> ExecuteScriptAndReturnSplittedStandardOutput(
        ScriptIdentifier scriptIdentifier,
        string? scriptArgument = null,
        char separator = ' ',
        bool includeWarnings = true)
            => await ExecuteScriptAndReturnSplittedStandardOutput(
            scriptIdentifier,
            ToArgs(scriptArgument),
            separator,
            includeWarnings);

    private async Task<(Result, IEnumerable<string>)> ExecuteScriptAndReturnSplittedStandardOutput(
        ScriptIdentifier scriptIdentifier,
        IEnumerable<string> scriptArguments,
        char separator = ' ',
        bool includeWarnings = true)
    {
        var (result, scriptResponse) = await RunScriptAsync(scriptIdentifier, scriptArguments, includeWarnings);

        if (result.OperationStatus is OperationStatus.Error)
            return (result, []);

        var values = scriptResponse.StandardOutput
            .Split(separator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .AsEnumerable();

        return (result, values);
    }

    private static async Task<ScriptResponse> RunScriptUsingDefaultRunnerAsync(
        int? timeoutInMilliseconds,
        ScriptIdentifier scriptIdentifier,
        IEnumerable<string> scriptArguments)
    {
        return timeoutInMilliseconds.HasValue
            ? await ScriptRunner.GetInstance()
                .Run(timeoutInMilliseconds.Value, scriptIdentifier, scriptArguments)
                .ConfigureAwait(false)
            : await ScriptRunner.GetInstance()
                .Run(scriptIdentifier, scriptArguments)
                .ConfigureAwait(false);
    }

    private static T DeserializeStandardOutputSection<T>(
        JsonElement rootElement,
        JsonTypeInfo<T> jsonTypeInfo)
            where T : class
    {
        var sectionName = GetStandardOutputSectionName<T>();

        if (!rootElement.TryGetProperty(sectionName, out var section))
            throw new JsonException($"Required JSON property '{sectionName}' was not found.");

        return section.Deserialize(jsonTypeInfo)
            ?? throw new JsonException($"JSON property '{sectionName}' contains null.");
    }

    private static string GetStandardOutputSectionName<T>()
        => typeof(T).Name.EndsWith("Settings", StringComparison.Ordinal)
            ? typeof(T).Name[..^"Settings".Length]
            : typeof(T).Name;

    private async Task<(Result, ScriptResponse)> RunScriptAsync(
        ScriptIdentifier scriptIdentifier,
        IEnumerable<string> scriptArguments,
        bool includeWarnings,
        int? timeoutInMilliseconds = null)
    {
        var scriptResponse = await _runScript(timeoutInMilliseconds, scriptIdentifier, scriptArguments)
            .ConfigureAwait(false);

        var result = new Result();

        if (scriptResponse.HasErrors)
            result.AddScriptError(scriptIdentifier, scriptResponse.StandardError);

        if (includeWarnings && scriptResponse.HasWarnings)
            result.AddScriptWarning(scriptIdentifier, scriptResponse.StandardError);

        return (result, scriptResponse);
    }
}

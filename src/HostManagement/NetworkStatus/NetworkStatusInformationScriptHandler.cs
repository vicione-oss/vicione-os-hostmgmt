using System.Text.Json;
using HostManagement.Scripts;
using HostManagement.Scripts.Enums;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.NetworkStatus;

namespace HostManagement.NetworkStatus;

/// <inheritdoc cref="INetworkStatusInformationScriptHandler"/>
internal sealed class NetworkStatusInformationScriptHandler : INetworkStatusInformationScriptHandler
{
    private readonly Result _result;

    /// <summary>
    /// Initializes a new <see cref="NetworkStatusInformationScriptHandler"/>.
    /// </summary>
    /// <param name="result">The result of the operation.</param>
    public NetworkStatusInformationScriptHandler(Result result)
    {
        _result = result;
    }

    public async Task<(OperationStatus, NetworkStatusInformation)> ExecuteGetNetworkStatusInformationScript(
        ScriptOptions scriptOptions)
    {
        var hasWarning = false;
        var scriptResponse = await ScriptRunner.GetInstance().Run(
            ScriptIdentifier.NetworkStatusInformationGet,
            scriptOptions.Options);

        if (scriptResponse.HasErrors)
        {
            _result.AddScriptError(
                ScriptIdentifier.NetworkStatusInformationGet,
                scriptResponse.StandardError);

            return (OperationStatus.Error, NetworkStatusInformation.Empty);
        }

        if (scriptResponse.HasWarnings)
        {
            _result.AddScriptWarning(
                ScriptIdentifier.NetworkStatusInformationGet,
                scriptResponse.StandardError);

            hasWarning = true;
        }

        try
        {
            var networkStatusInformation = JsonSerializer
                .Deserialize(
                    scriptResponse.StandardOutput,
                    SourceGenerationContext.Default.NetworkStatusInformation)
                ?? throw new InvalidOperationException(
                    GetInvalidOperationExceptionMessage(
                        nameof(NetworkStatusInformation),
                        ScriptIdentifier.NetworkStatusInformationGet,
                        scriptResponse.StandardOutput));

            if (hasWarning)
                return (OperationStatus.Warning, networkStatusInformation);

            return (OperationStatus.Success, networkStatusInformation);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                GetInvalidOperationExceptionMessage(
                    nameof(NetworkStatusInformation),
                    ScriptIdentifier.NetworkStatusInformationGet,
                    ex.Message));
        }
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

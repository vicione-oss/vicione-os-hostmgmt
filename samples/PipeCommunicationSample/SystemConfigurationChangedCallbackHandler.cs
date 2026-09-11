using System.Text.Json.Serialization.Metadata;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.NamedPipe.Client;
using HostManagement.Shared.Communication.Serialization;

namespace PipeCommunicationSample;

/// <summary>
/// Handles notifications about changes to the system configuration.
/// </summary>
internal sealed class SystemConfigurationChangedCallbackHandler : ICallbackHandler<GetSystemConfigurationResult>
{
    /// <inheritdoc/>
    public string Topic => EventTopics.SystemConfigurationChanged;

    /// <inheritdoc/>
    public JsonTypeInfo<GetSystemConfigurationResult> TypeInfo
        => SourceGenerationContext.Default.GetSystemConfigurationResult;

    /// <inheritdoc/>
    public Task HandleAsync(
        GetSystemConfigurationResult message,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        Console.WriteLine();
        Console.WriteLine($"Server event received: '{Topic}'");
        Console.WriteLine($"Correlation ID: {correlationId}");

        var serializedResult = SourceGeneratedTypeSerializer.Serialize(message, TypeInfo);

        Console.WriteLine(serializedResult);

        return Task.CompletedTask;
    }
}

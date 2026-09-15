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
    public Task HandleAsync(
        GetSystemConfigurationResult message,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        Console.WriteLine();
        Console.WriteLine($"Server event received: '{PipeEvents.SystemConfigurationChanged.Topic}'");
        Console.WriteLine($"Correlation ID: {correlationId}");

        var serializedResult = SourceGeneratedTypeSerializer.Serialize(
            message,
            PipeEvents.SystemConfigurationChanged.MessageTypeInfo);

        Console.WriteLine(serializedResult);

        return Task.CompletedTask;
    }
}

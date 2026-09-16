using HostManagement.Shared;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.NamedPipe.Client;
using HostManagement.Shared.Serialization;

namespace PipeCommunicationSample;

/// <summary>
/// Handles notifications about changes to the system configuration.
/// </summary>
/// <remarks>
/// A typed handler only declares its topic and how to handle the message. The JSON metadata for
/// <see cref="GetSystemConfigurationResult"/> is resolved through the <see cref="MessageTypeRegistry"/>.
/// </remarks>
internal sealed class SystemConfigurationChangedCallbackHandler : ICallbackHandler<GetSystemConfigurationResult>
{
    /// <inheritdoc/>
    public string Topic => EventTopics.SystemConfigurationChanged;

    /// <inheritdoc/>
    public Task HandleAsync(
        GetSystemConfigurationResult message,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        Console.WriteLine();
        Console.WriteLine($"Server event received: '{Topic}'");
        Console.WriteLine($"Correlation ID: {correlationId}");
        Console.WriteLine(MessageSerializer.Serialize(message));

        return Task.CompletedTask;
    }
}

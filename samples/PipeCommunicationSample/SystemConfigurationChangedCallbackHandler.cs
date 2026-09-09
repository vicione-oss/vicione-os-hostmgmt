using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.NamedPipe.Client;

namespace PipeCommunicationSample;

/// <summary>
/// Handles notifications about changes to the system configuration.
/// </summary>
internal sealed class SystemConfigurationChangedCallbackHandler : ICallbackHandler
{
    /// <inheritdoc/>
    public string Topic => EventTopics.SystemConfigurationChanged;

    /// <inheritdoc/>
    public Task HandleAsync(string messageContent, Guid correlationId, CancellationToken cancellationToken)
    {
        Console.WriteLine();
        Console.WriteLine($"Server event received: '{Topic}'");
        Console.WriteLine($"Correlation ID: {correlationId}");
        Console.WriteLine(messageContent);

        return Task.CompletedTask;
    }
}

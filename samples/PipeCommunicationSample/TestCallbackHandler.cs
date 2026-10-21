using HostManagement.Shared.Communication.NamedPipe.Client;

namespace PipeCommunicationSample;

internal sealed class TestCallbackHandler(KnownActionTracker knownActionTracker) : ICallbackHandler
{
    public string Topic => "test";

    public Task HandleRawAsync(string messageContent, Guid correlationId, CancellationToken cancellationToken)
    {
        var isKnownAction = knownActionTracker.Contains(correlationId);

        Console.WriteLine(
            $"{(isKnownAction ? "Known" : "Unexpected")} event " +
            $"[{correlationId}]: {messageContent}");

        return Task.CompletedTask;
    }
}

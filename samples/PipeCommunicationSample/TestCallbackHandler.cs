namespace PipeCommunicationSample;

internal sealed class TestCallbackHandler(KnownActionTracker knownActionTracker)
{
    public Task HandleRawAsync(string messageContent, Guid correlationId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var isKnownAction = knownActionTracker.Contains(correlationId);

        Console.WriteLine(
            $"{(isKnownAction ? "Known" : "Unexpected")} event " +
            $"[{correlationId}]: {messageContent}");

        return Task.CompletedTask;
    }
}

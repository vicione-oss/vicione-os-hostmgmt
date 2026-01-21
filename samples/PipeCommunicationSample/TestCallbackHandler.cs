using HostManagement.Shared.Communication.NamedPipe.Client;

namespace PipeCommunicationSample;

internal sealed class TestCallbackHandler : ICallbackHandler
{
    public string Topic => "test";

    public Task HandleAsync(string messageContent, CancellationToken cancellationToken)
    {
        Console.WriteLine(messageContent);
        return Task.CompletedTask;
    }
}

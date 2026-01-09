
namespace PipeCommunicationSample;

internal sealed class TestCallbackHandler : ICallbackHandler
{
    public string Topic => "test";

    public Task Handle(string messageContent, CancellationToken cancellationToken)
    {
        Console.WriteLine(messageContent);
        return Task.CompletedTask;
    }
}

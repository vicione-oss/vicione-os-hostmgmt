using HostManagement.Shared.Communication.Contracts;

namespace HostManagement.Communication.NamedPipes.Client;

internal sealed class ExpectedResponse
{
    public TaskCompletionSource<NamedPipeMessage> TaskCompletionSource { get; } =
        new(TaskCreationOptions.RunContinuationsAsynchronously);
}

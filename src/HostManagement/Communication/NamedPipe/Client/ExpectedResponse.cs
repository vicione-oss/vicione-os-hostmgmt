using HostManagement.Shared.Communication.Contracts;

namespace HostManagement.Communication.NamedPipe.Client;

internal sealed class ExpectedResponse
{
    public TaskCompletionSource<NamedPipeMessage> TaskCompletionSource { get; } =
        new(TaskCreationOptions.RunContinuationsAsynchronously);
}

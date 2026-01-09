using HostManagement.Shared.Communication.Contracts;

namespace PipeCommunicationSample;

internal sealed class ExpectedResponse
{
    public TaskCompletionSource<NamedPipeMessage> TaskCompletionSource { get; } =
        new(TaskCreationOptions.RunContinuationsAsynchronously);
}

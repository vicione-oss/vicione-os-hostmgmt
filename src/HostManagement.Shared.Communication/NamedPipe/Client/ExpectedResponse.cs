using HostManagement.Shared.Communication.Contracts;

namespace HostManagement.Shared.Communication.NamedPipe.Client;

/// <summary>
/// Represents an expected response used by the <see cref="PipeClient"/>.
/// </summary>
public sealed class ExpectedResponse
{
    /// <summary>
    /// The task completion source of the expected response with async continuations.
    /// </summary>
    public TaskCompletionSource<NamedPipeMessage> TaskCompletionSource { get; } =
        new(TaskCreationOptions.RunContinuationsAsynchronously);
}

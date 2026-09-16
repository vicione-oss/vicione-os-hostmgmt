using HostManagement.Communication.NamedPipe.Extensions;

namespace HostManagement.Communication.NamedPipe.Server;

/// <summary>
/// Represents the default implementation for initializing a <see cref="PipeServer"/>.
/// </summary>
internal sealed class DefaultPipeServerInitializer : IPipeServerInitializer
{
    /// <inheritdoc/>
    public async Task InitializeAsync(PipeServer pipeServer, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await pipeServer.SetExecutePermission().ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        await pipeServer.SetOwnership().ConfigureAwait(false);
    }
}

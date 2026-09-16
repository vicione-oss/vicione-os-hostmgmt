namespace HostManagement.Communication.NamedPipe.Server;

/// <summary>
/// Initializes a <see cref="PipeServer"/> after the pipe has been created.
/// </summary>
internal interface IPipeServerInitializer
{
    /// <summary>
    /// Initializes the specified <paramref name="pipeServer"/>.
    /// </summary>
    /// <param name="pipeServer">The pipe server.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task InitializeAsync(PipeServer pipeServer, CancellationToken cancellationToken);
}

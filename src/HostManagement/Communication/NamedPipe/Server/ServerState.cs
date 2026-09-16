namespace HostManagement.Communication.NamedPipe.Server;

/// <summary>
/// Represents the current state of the pipe server.
/// </summary>
internal enum ServerState
{
    NotStarted,
    Starting,
    Started,

    /// <summary>
    /// Indicates that the pipe server is running in a degraded state.
    /// </summary>
    /// <remarks>
    /// At least one listener instance is temporarily unable to continue listening, for example because pipe creation
    /// failed or pipe execute permission and ownership could not be initialized.
    /// The server returns to <see cref="Started"/> once all degraded listener instances have recovered.
    /// </remarks>
    Degraded,
    Stopping,
    Stopped,
    Disposing,
    Disposed
}

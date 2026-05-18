using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Enums;

namespace HostManagement.Communication.NamedPipes.Client;

/// <summary>
/// Represents a named pipe client.
/// </summary>
internal interface IPipeClient : IDisposable
{
    /// <summary>
    /// The state of the pipe connection.
    /// </summary>
    PipeState State { get; }

    /// <summary>
    /// Tries to connect to a pipe.
    /// </summary>
    /// <returns><see langword="true"/> if successful, otherwise <see langword="false"/></returns>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<bool> TryConnect(CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a request with the specified <paramref name="topic"/>, <paramref name="content"/> and
    /// <paramref name="cancellationToken"/>.
    /// </summary>
    /// <param name="topic">The topic of the request used to identify a request handler 
    /// (see <see cref="Topics"/>).</param>
    /// <param name="content">The JSON serialized message content.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><see cref="NamedPipeMessage"/></returns>
    Task<NamedPipeMessage> SendRequest(string topic, string content = "", CancellationToken cancellationToken = default);
}

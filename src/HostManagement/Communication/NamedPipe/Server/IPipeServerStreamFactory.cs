using System.IO.Pipes;
using HostManagement.Shared.Communication;
using Microsoft.Extensions.Logging;

namespace HostManagement.Communication.NamedPipe.Server;

/// <summary>
/// Creates named pipe server streams and wrappers for pipe server connections.
/// </summary>
internal interface IPipeServerStreamFactory
{
    /// <summary>
    /// Creates a new <see cref="NamedPipeServerStream"/>.
    /// </summary>
    /// <param name="pipeName">The name of the pipe.</param>
    /// <param name="maxNumberOfServerInstances">The maximum number of server instances.</param>
    /// <param name="options">The pipe options.</param>
    /// <returns>The created <see cref="NamedPipeServerStream"/>.</returns>
    NamedPipeServerStream Create(string pipeName, int maxNumberOfServerInstances, PipeOptions? options);

    /// <summary>
    /// Creates a wrapper for the specified <paramref name="pipeServerStream"/>.
    /// </summary>
    /// <param name="pipeServerStream">The pipe server stream.</param>
    /// <param name="logger">An <see cref="ILogger"/> implementation.</param>
    /// <returns>The created <see cref="IPipeStreamWrapper"/>.</returns>
    IPipeStreamWrapper CreateWrapper(NamedPipeServerStream pipeServerStream, ILogger logger);
}

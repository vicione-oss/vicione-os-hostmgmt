using System.IO.Pipes;
using HostManagement.Shared.Communication;
using Microsoft.Extensions.Logging;

namespace HostManagement.Communication.NamedPipe.Server;

/// <summary>
/// Represents the default implementation for creating named pipe server streams and wrappers.
/// </summary>
internal sealed class DefaultPipeServerStreamFactory : IPipeServerStreamFactory
{
    /// <inheritdoc/>
    public NamedPipeServerStream Create(string pipeName, int maxNumberOfServerInstances, PipeOptions? options)
    {
        var effectiveOptions = options == null
            ? PipeOptions.Asynchronous
            : options.Value | PipeOptions.Asynchronous;

        return new NamedPipeServerStream(
            pipeName,
            PipeDirection.InOut,
            maxNumberOfServerInstances,
            PipeTransmissionMode.Byte,
            effectiveOptions);
    }

    /// <inheritdoc/>
    public IPipeStreamWrapper CreateWrapper(NamedPipeServerStream pipeServerStream, ILogger logger)
        => new PipeStreamWrapper(pipeServerStream, logger);
}

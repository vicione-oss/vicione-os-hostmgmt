using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Serialization;

namespace HostManagement.Communication.NamedPipe.RequestHandlers;

/// <summary>
/// Represents the abstract base class for a strongly typed request handler.
/// </summary>
/// <typeparam name="T">The strongly typed response type.</typeparam>
/// <remarks>
/// The JSON metadata for <typeparamref name="T"/> is resolved through the <see cref="MessageTypeRegistry"/>,
/// so a handler only declares its topic and how to handle the request.
/// </remarks>
internal abstract class RequestHandler<T> : IRequestHandler where T : OperationResult, IOperationResultFactory<T>
{
    /// <inheritdoc/>
    public abstract string Topic { get; }

    /// <inheritdoc/>
    public abstract Task<string> HandleAsync(
        string messageContent,
        Guid correlationId,
        CancellationToken cancellationToken);

    /// <inheritdoc/>
    public virtual string CreateErrorResponseContent(string message)
        => MessageSerializer.Serialize(T.CreateErrorResult(message));
}

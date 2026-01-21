using System.Text.Json.Serialization.Metadata;
using HostManagement.Shared.Communication.Contracts;

namespace HostManagement.Communication.NamedPipe.RequestHandlers;

/// <summary>
/// Represents the abstract base class for a strongly typed request handler.
/// </summary>
/// <typeparam name="T">The strongly typed response type.</typeparam>
internal abstract class RequestHandler<T> : IRequestHandler<T> where T : OperationResult, IOperationResultFactory<T>
{
    public abstract string Topic { get; }

    public abstract JsonTypeInfo<T> TypedJsonTypeInfo { get; }

    public JsonTypeInfo JsonTypeInfo => TypedJsonTypeInfo;

    public abstract Task<string> HandleAsync(string messageContent, CancellationToken cancellationToken);
    public virtual T CreateTypedErrorInstance(string message) => T.CreateErrorResult(message);
    OperationResult IRequestHandler.CreateErrorInstance(string message) => CreateTypedErrorInstance(message);
}

using System.Text.Json.Serialization.Metadata;
using HostManagement.Shared.Communication.Contracts;

namespace HostManagement.Communication.NamedPipes.RequestHandlers;

/// <summary>
/// Represents a generic request handler with a strongly typed response.
/// </summary>
/// <typeparam name="T">The strongly typed response type.</typeparam>
internal interface IRequestHandler<T> : IRequestHandler where T : OperationResult
{
    /// <summary>
    /// The JSON type info for <typeparamref name="T"/>.
    /// </summary>
    JsonTypeInfo<T> TypedJsonTypeInfo { get; }

    /// <summary>
    /// Creates an error instance of <typeparamref name="T"/>.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <returns>The instance.</returns>
    T CreateTypedErrorInstance(string message);
}

/// <summary>
/// Represents a request handler.
/// </summary>
internal interface IRequestHandler
{
    /// <summary>
    /// The unique message topic to be handled.
    /// </summary>
    string Topic { get; }

    /// <summary>
    /// The JSON type info.
    /// </summary>
    JsonTypeInfo JsonTypeInfo { get; }

    /// <summary>
    /// Creates an error instance of the handler's response type in a type-safe, AOT-friendly way.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <returns><see cref="OperationResult"/></returns>
    OperationResult CreateErrorInstance(string message);

    /// <summary>
    /// The method handling a message.
    /// </summary>
    /// <param name="messageContent">The JSON serialized message content.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The result to be sent as a response to the caller.</returns>
    Task<string> Handle(string messageContent, CancellationToken cancellationToken);
}

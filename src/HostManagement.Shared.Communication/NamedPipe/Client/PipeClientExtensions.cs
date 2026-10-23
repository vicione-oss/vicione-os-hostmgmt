using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Serialization;

namespace HostManagement.Shared.Communication.NamedPipe.Client;

/// <summary>
/// Provides strongly typed request methods for an <see cref="IPipeClient"/>.
/// </summary>
/// <remarks>
/// The JSON metadata for the request and response contracts is resolved through the
/// <see cref="MessageTypeRegistry"/>, so callers never have to name a JsonTypeInfo.
/// <para>
/// Raw text is sent and received unchanged by using <see cref="string"/> as the contract type, or by using
/// <see cref="IPipeClient.SendRequestAsync(string, string, Guid?, CancellationToken)"/> directly.
/// </para>
/// </remarks>
public static class PipeClientExtensions
{
    // CA1034: The analyzer reports the type the compiler generates for the extension block. There is no nested type
    // in the source, and the generated one is not part of the public API surface.
#pragma warning disable CA1034
    extension(IPipeClient client)
#pragma warning restore CA1034
    {
        /// <summary>
        /// Sends a request with the specified <paramref name="topic"/> and raw <paramref name="content"/>
        /// and deserializes the response content into a <typeparamref name="TResponse"/>.
        /// </summary>
        /// <typeparam name="TResponse">The response contract type.</typeparam>
        /// <param name="topic">
        /// The topic of the request used to identify a request handler (see <see cref="Topics"/>).
        /// </param>
        /// <param name="content">The raw request content.</param>
        /// <param name="correlationId">
        /// The optional correlation ID to use as the request's conversation ID. It can be used to associate
        /// subsequent events with the request. When omitted, a new ID is generated. A provided value must not be
        /// <see cref="Guid.Empty"/>.
        /// </param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The deserialized response.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="correlationId"/> is empty.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the pipe client is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the client is not connected and connecting fails, when a duplicate conversation identifier is
        /// detected, when the serialized request payload exceeds the configured pipe message size limit, or when no
        /// registered context provides metadata for <typeparamref name="TResponse"/>.
        /// </exception>
        /// <exception cref="IOException">
        /// Thrown when the pipe faults while awaiting the response,
        /// or when sending the request fails due to an I/O error.
        /// </exception>
        /// <exception cref="JsonException">
        /// Thrown when the response content is neither valid JSON for <typeparamref name="TResponse"/>
        /// nor a plain <see cref="Response"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// Thrown when the client or the underlying connection has been disposed.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        /// Thrown when the operation is canceled via <paramref name="cancellationToken"/>,
        /// or when the pipe is closed while awaiting the response.
        /// </exception>
        /// <exception cref="PipeRequestFailedException">
        /// Thrown when HostManagement answered with a plain <see cref="Response"/>, because the request failed on its
        /// side. The exception message is the reason reported by HostManagement.
        /// </exception>
        public async Task<TResponse> SendRequestAsync<TResponse>(
            string topic,
            string content = "",
            Guid? correlationId = null,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(client);

            var response = await client
                .SendRequestAsync(topic, content, correlationId, cancellationToken)
                .ConfigureAwait(false);

            try
            {
                return MessageSerializer.Deserialize<TResponse>(response.Content);
            }
            catch (JsonException ex) when (TryReadFailureReason(response.Content, out var reason))
            {
                throw new PipeRequestFailedException(response.Topic, response.ConversationId, reason, ex);
            }
        }

        /// <summary>
        /// Serializes <paramref name="content"/>, sends it as a request with the specified <paramref name="topic"/>
        /// and deserializes the response content into a <typeparamref name="TResponse"/>.
        /// </summary>
        /// <typeparam name="TRequest">The request contract type.</typeparam>
        /// <typeparam name="TResponse">The response contract type.</typeparam>
        /// <param name="topic">
        /// The topic of the request used to identify a request handler (see <see cref="Topics"/>).
        /// </param>
        /// <param name="content">The request content.</param>
        /// <param name="correlationId">
        /// The optional correlation ID to use as the request's conversation ID. It can be used to associate
        /// subsequent events with the request. When omitted, a new ID is generated. A provided value must not be
        /// <see cref="Guid.Empty"/>.
        /// </param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The deserialized response.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="correlationId"/> is empty.</exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the pipe client or <paramref name="content"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the client is not connected and connecting fails, when a duplicate conversation identifier is
        /// detected, when the serialized request payload exceeds the configured pipe message size limit, or when no
        /// registered context provides metadata for <typeparamref name="TRequest"/>
        /// or <typeparamref name="TResponse"/>.
        /// </exception>
        /// <exception cref="IOException">
        /// Thrown when the pipe faults while awaiting the response,
        /// or when sending the request fails due to an I/O error.
        /// </exception>
        /// <exception cref="JsonException">
        /// Thrown when the response content is neither valid JSON for <typeparamref name="TResponse"/>
        /// nor a plain <see cref="Response"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// Thrown when the client or the underlying connection has been disposed.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        /// Thrown when the operation is canceled via <paramref name="cancellationToken"/>,
        /// or when the pipe is closed while awaiting the response.
        /// </exception>
        /// <exception cref="PipeRequestFailedException">
        /// Thrown when HostManagement answered with a plain <see cref="Response"/>, because the request failed on its
        /// side. The exception message is the reason reported by HostManagement.
        /// </exception>
        public async Task<TResponse> SendRequestAsync<TRequest, TResponse>(
            string topic,
            TRequest content,
            Guid? correlationId = null,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(client);

            return await client
                .SendRequestAsync<TResponse>(
                    topic,
                    MessageSerializer.Serialize(content),
                    correlationId,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Sends a request with the specified <paramref name="topic"/> and raw <paramref name="content"/> and returns
        /// the <typeparamref name="TResponse"/> operation result.
        /// </summary>
        /// <typeparam name="TResponse">The operation result type.</typeparam>
        /// <param name="topic">
        /// The topic of the request used to identify a request handler (see <see cref="Topics"/>).
        /// </param>
        /// <param name="content">The raw request content.</param>
        /// <param name="correlationId">
        /// The optional correlation ID to use as the request's conversation ID. It can be used to associate
        /// subsequent events with the request. When omitted, a new ID is generated. A provided value must not be
        /// <see cref="Guid.Empty"/>.
        /// </param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>
        /// The deserialized operation result, or an error result created by
        /// <see cref="IOperationResultFactory{T}.CreateErrorResult"/> if the response content could not be deserialized.
        /// </returns>
        /// <remarks>
        /// Unlike the typed request methods this method does not throw when the response content is malformed or
        /// when HostManagement answered with a plain <see cref="Response"/> because the request failed on its side.
        /// The error result then carries the deserialization error or the reason reported by HostManagement.
        /// Failures to send the request, and a missing contract registration for <typeparamref name="TResponse"/>,
        /// are still propagated.
        /// </remarks>
        /// <exception cref="ArgumentException">Thrown when <paramref name="correlationId"/> is empty.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the pipe client is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the client is not connected and connecting fails, when a duplicate conversation identifier is
        /// detected, when the serialized request payload exceeds the configured pipe message size limit, or when no
        /// registered context provides metadata for <typeparamref name="TResponse"/>.
        /// </exception>
        /// <exception cref="IOException">
        /// Thrown when the pipe faults while awaiting the response,
        /// or when sending the request fails due to an I/O error.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// Thrown when the client or the underlying connection has been disposed.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        /// Thrown when the operation is canceled via <paramref name="cancellationToken"/>,
        /// or when the pipe is closed while awaiting the response.
        /// </exception>
        public async Task<TResponse> SendOperationAsync<TResponse>(
            string topic,
            string content = "",
            Guid? correlationId = null,
            CancellationToken cancellationToken = default)
                where TResponse : OperationResult, IOperationResultFactory<TResponse>
        {
            ArgumentNullException.ThrowIfNull(client);

            var response = await client
                .SendRequestAsync(topic, content, correlationId, cancellationToken)
                .ConfigureAwait(false);

            try
            {
                return MessageSerializer.Deserialize<TResponse>(response.Content);
            }
            // A missing contract registration is a programming error and must stay loud, so it is not caught here.
            catch (Exception ex) when (ex is JsonException or NotSupportedException)
            {
                return TResponse.CreateErrorResult(
                    TryReadFailureReason(response.Content, out var reason)
                        ? reason
                        : $"Named pipe message content could not be deserialized for topic '{response.Topic}' " +
                          $"(Conversation ID: {response.ConversationId}). {ex.Message}");
            }
        }

        /// <summary>
        /// Serializes <paramref name="content"/>, sends it as a request with the specified <paramref name="topic"/>
        /// and returns the <typeparamref name="TResponse"/> operation result.
        /// </summary>
        /// <typeparam name="TRequest">The request contract type.</typeparam>
        /// <typeparam name="TResponse">The operation result type.</typeparam>
        /// <param name="topic">
        /// The topic of the request used to identify a request handler (see <see cref="Topics"/>).
        /// </param>
        /// <param name="content">The request content.</param>
        /// <param name="correlationId">
        /// The optional correlation ID to use as the request's conversation ID. It can be used to associate
        /// subsequent events with the request. When omitted, a new ID is generated. A provided value must not be
        /// <see cref="Guid.Empty"/>.
        /// </param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>
        /// The deserialized operation result, or an error result created by
        /// <see cref="IOperationResultFactory{T}.CreateErrorResult"/> if the response content could not be deserialized.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="correlationId"/> is empty.</exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the pipe client or <paramref name="content"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the client is not connected and connecting fails, when a duplicate conversation identifier is
        /// detected, when the serialized request payload exceeds the configured pipe message size limit, or when no
        /// registered context provides metadata for <typeparamref name="TRequest"/>
        /// or <typeparamref name="TResponse"/>.
        /// </exception>
        /// <exception cref="IOException">
        /// Thrown when the pipe faults while awaiting the response,
        /// or when sending the request fails due to an I/O error.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// Thrown when the client or the underlying connection has been disposed.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        /// Thrown when the operation is canceled via <paramref name="cancellationToken"/>,
        /// or when the pipe is closed while awaiting the response.
        /// </exception>
        /// <inheritdoc cref="SendOperationAsync{TResponse}(IPipeClient, string, string, Guid?, CancellationToken)" path="/remarks"/>
        public async Task<TResponse> SendOperationAsync<TRequest, TResponse>(
            string topic,
            TRequest content,
            Guid? correlationId = null,
            CancellationToken cancellationToken = default)
                where TResponse : OperationResult, IOperationResultFactory<TResponse>
        {
            ArgumentNullException.ThrowIfNull(client);

            return await client
                .SendOperationAsync<TResponse>(
                    topic,
                    MessageSerializer.Serialize(content),
                    correlationId,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Sends a request on the specified <paramref name="topic"/> with raw <paramref name="content"/>
        /// and deserializes the response content into the topic's response contract.
        /// </summary>
        /// <typeparam name="TResponse">The response contract type, inferred from <paramref name="topic"/>.</typeparam>
        /// <param name="topic">The topic and its contracts (see <see cref="TopicContracts"/>).</param>
        /// <param name="content">The raw request content.</param>
        /// <param name="correlationId">
        /// The optional correlation ID to use as the request's conversation ID. It can be used to associate
        /// subsequent events with the request. When omitted, a new ID is generated. A provided value must not be
        /// <see cref="Guid.Empty"/>.
        /// </param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The deserialized response.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the pipe client or <paramref name="topic"/> is <see langword="null"/>.
        /// </exception>
        /// <inheritdoc cref="SendRequestAsync{TResponse}(IPipeClient, string, string, Guid?, CancellationToken)" path="/exception"/>
        public async Task<TResponse> SendRequestAsync<TResponse>(
            TopicContract<TResponse> topic,
            string content = "",
            Guid? correlationId = null,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(topic);

            return await client
                .SendRequestAsync<TResponse>(topic.Topic, content, correlationId, cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Serializes <paramref name="content"/>, sends it on the specified <paramref name="topic"/> and deserializes
        /// the response content into the topic's response contract.
        /// </summary>
        /// <typeparam name="TRequest">The request contract type, inferred from <paramref name="topic"/>.</typeparam>
        /// <typeparam name="TResponse">The response contract type, inferred from <paramref name="topic"/>.</typeparam>
        /// <param name="topic">The topic and its contracts (see <see cref="TopicContracts"/>).</param>
        /// <param name="content">The request content.</param>
        /// <param name="correlationId">
        /// The optional correlation ID to use as the request's conversation ID. It can be used to associate
        /// subsequent events with the request. When omitted, a new ID is generated. A provided value must not be
        /// <see cref="Guid.Empty"/>.
        /// </param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The deserialized response.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the pipe client, <paramref name="topic"/> or <paramref name="content"/>
        /// is <see langword="null"/>.
        /// </exception>
        /// <inheritdoc cref="SendRequestAsync{TResponse}(IPipeClient, string, string, Guid?, CancellationToken)" path="/exception"/>
        public async Task<TResponse> SendRequestAsync<TRequest, TResponse>(
            TopicContract<TRequest, TResponse> topic,
            TRequest content,
            Guid? correlationId = null,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(topic);

            return await client
                .SendRequestAsync<TRequest, TResponse>(topic.Topic, content, correlationId, cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Sends a request on the specified <paramref name="topic"/> with raw <paramref name="content"/>
        /// and returns the topic's operation result.
        /// </summary>
        /// <typeparam name="TResponse">The operation result type, inferred from <paramref name="topic"/>.</typeparam>
        /// <param name="topic">The topic and its contracts (see <see cref="TopicContracts"/>).</param>
        /// <param name="content">The raw request content.</param>
        /// <param name="correlationId">
        /// The optional correlation ID to use as the request's conversation ID. It can be used to associate
        /// subsequent events with the request. When omitted, a new ID is generated. A provided value must not be
        /// <see cref="Guid.Empty"/>.
        /// </param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>
        /// The deserialized operation result, or an error result created by
        /// <see cref="IOperationResultFactory{T}.CreateErrorResult"/> if the response content could not be deserialized.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the pipe client or <paramref name="topic"/> is <see langword="null"/>.
        /// </exception>
        /// <inheritdoc cref="SendOperationAsync{TResponse}(IPipeClient, string, string, Guid?, CancellationToken)" path="/remarks|/exception"/>
        public async Task<TResponse> SendOperationAsync<TResponse>(
            TopicContract<TResponse> topic,
            string content = "",
            Guid? correlationId = null,
            CancellationToken cancellationToken = default)
                where TResponse : OperationResult, IOperationResultFactory<TResponse>
        {
            ArgumentNullException.ThrowIfNull(topic);

            return await client
                .SendOperationAsync<TResponse>(topic.Topic, content, correlationId, cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Serializes <paramref name="content"/>, sends it on the specified <paramref name="topic"/>
        /// and returns the topic's operation result.
        /// </summary>
        /// <typeparam name="TRequest">The request contract type, inferred from <paramref name="topic"/>.</typeparam>
        /// <typeparam name="TResponse">The operation result type, inferred from <paramref name="topic"/>.</typeparam>
        /// <param name="topic">The topic and its contracts (see <see cref="TopicContracts"/>).</param>
        /// <param name="content">The request content.</param>
        /// <param name="correlationId">
        /// The optional correlation ID to use as the request's conversation ID. It can be used to associate
        /// subsequent events with the request. When omitted, a new ID is generated. A provided value must not be
        /// <see cref="Guid.Empty"/>.
        /// </param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>
        /// The deserialized operation result, or an error result created by
        /// <see cref="IOperationResultFactory{T}.CreateErrorResult"/> if the response content could not be deserialized.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the pipe client, <paramref name="topic"/> or <paramref name="content"/>
        /// is <see langword="null"/>.
        /// </exception>
        /// <inheritdoc cref="SendOperationAsync{TResponse}(IPipeClient, string, string, Guid?, CancellationToken)" path="/remarks|/exception"/>
        public async Task<TResponse> SendOperationAsync<TRequest, TResponse>(
            TopicContract<TRequest, TResponse> topic,
            TRequest content,
            Guid? correlationId = null,
            CancellationToken cancellationToken = default)
                where TResponse : OperationResult, IOperationResultFactory<TResponse>
        {
            ArgumentNullException.ThrowIfNull(topic);

            return await client
                .SendOperationAsync<TRequest, TResponse>(topic.Topic, content, correlationId, cancellationToken)
                .ConfigureAwait(false);
        }
    }

    // HostManagement answers with a plain 'Response' that carries only the reason when the request failed on its side,
    // e.g. when no handler is registered for the topic or the handler threw. Unknown members are skipped, so a
    // response contract with a 'Message' would deserialize as one too; only content without a 'ResultType' is one.
    private static bool TryReadFailureReason(string content, [NotNullWhen(true)] out string? reason)
    {
        reason = null;

        try
        {
            using var document = JsonDocument.Parse(content);

            if (document.RootElement.ValueKind is not JsonValueKind.Object ||
                document.RootElement.TryGetProperty(nameof(OperationResult.ResultType), out _))
            {
                return false;
            }

            reason = MessageSerializer.Deserialize<Response>(content).Message;
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}

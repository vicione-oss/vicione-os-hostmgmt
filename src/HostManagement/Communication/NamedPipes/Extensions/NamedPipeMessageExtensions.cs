using System.Text.Json;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Enums;
using SourceGenerationContext = HostManagement.Shared.Communication.Contracts.SourceGenerationContext;

namespace HostManagement.Communication.NamedPipes.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="NamedPipeMessage"/>.
/// </summary>
internal static class NamedPipeMessageExtensions
{
    /// <summary>
    /// Creates a default response.
    /// </summary>
    /// <param name="request">The named pipe message request.</param>
    /// <param name="message">The response message.</param>
    /// <returns><see cref="NamedPipeMessage"/></returns>
    public static NamedPipeMessage CreateDefaultResponse(this NamedPipeMessage request, string message)
        => request.CreateResponse(
            JsonSerializer.Serialize(new Response { Message = message }, SourceGenerationContext.Default.Response));

    /// <summary>
    /// Creates a response.
    /// </summary>
    /// <param name="request">The named pipe message request.</param>
    /// <param name="content">The content.</param>
    /// <returns></returns>
    public static NamedPipeMessage CreateResponse(this NamedPipeMessage request, string content)
        => new()
        {
            Topic = request.Topic,
            ConversationId = request.ConversationId,
            Type = MessageType.Response,
            Content = content
        };
}

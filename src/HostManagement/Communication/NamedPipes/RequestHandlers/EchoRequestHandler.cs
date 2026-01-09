using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.Logging;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Contracts;

namespace HostManagement.Communication.NamedPipes.RequestHandlers;

/// <summary>
/// Represents a <see cref="EchoRequestHandler"/>.
/// </summary>
/// <param name="server">The pipe server.</param>
/// <param name="logger">The logger.</param>
internal sealed partial class EchoRequestHandler(PipeServer server, ILogger logger) : IRequestHandler
{
    /// <summary>
    /// The topic of the <see cref="EchoRequestHandler"/>.
    /// </summary>
    public string Topic => Topics.Echo;

    public JsonTypeInfo JsonTypeInfo => throw new NotImplementedException();

    private static async Task FireEventMessages(PipeServer server)
    {
        const int Duration = 3;

        for (var i = 0; i < Duration; i++)
        {
            await Task.Delay(1000);
            await server.PublishEvent("test", $"{Duration - i}...");
        }

        await server.PublishEvent("test", "tadaaaaa!");
    }

    public OperationResult CreateErrorInstance(string message) => throw new NotImplementedException();

    /// <summary>
    /// Handles the echo request.
    /// </summary>
    /// <param name="messageContent">The message content.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The response.</returns>
    public Task<string> Handle(string messageContent, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(messageContent, nameof(messageContent));

        HandlingEchoRequest(logger, messageContent);

        switch (messageContent)
        {
            case "ping":
                return Task.FromResult("pong");
            case "trigger":
                _ = FireEventMessages(server);
                return Task.FromResult("Wait for it!");
            default:
                return Task.FromResult("No!");
        }
    }

    [LoggerMessage(LogLevel.Information, "Handling echo request with content '{MessageContent}'.")]
    private static partial void HandlingEchoRequest(ILogger logger, string messageContent);
}

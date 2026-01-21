using System.Text.Json.Serialization.Metadata;
using HostManagement.Communication.NamedPipe.Server;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Contracts;
using Microsoft.Extensions.Logging;

namespace HostManagement.Communication.NamedPipe.RequestHandlers;

/// <summary>
/// Represents a <see cref="EchoRequestHandler"/>.
/// </summary>
/// <param name="server">The pipe server.</param>
/// <param name="logger">The logger.</param>
internal sealed partial class EchoRequestHandler(PipeServer server, ILogger logger) : IRequestHandler
{
    private const string TestTopic = "test"; // Must be processed by the client (pipe sample in this case)

    /// <summary>
    /// The topic of the <see cref="EchoRequestHandler"/>.
    /// </summary>
    public string Topic => Topics.Echo;

    public JsonTypeInfo JsonTypeInfo => throw new NotImplementedException();

    /// <summary>
    /// Handles the echo request.
    /// </summary>
    /// <inheritdoc/>
    public Task<string> HandleAsync(string messageContent, CancellationToken cancellationToken = default)
    {
        switch (messageContent)
        {
            case "ping":
                return Task.FromResult("pong");
            case "trigger":
                _ = FireEventMessages(cancellationToken);
                return Task.FromResult("Wait for it!");
            default:
                return Task.FromResult("No!");
        }
    }

    private async Task FireEventMessages(CancellationToken cancellationToken)
    {
        try
        {
            const int Duration = 3;

            for (var i = 0; i < Duration; i++)
            {
                await Task.Delay(1000, cancellationToken);
                await server.PublishEvent(TestTopic, $"{Duration - i}...", cancellationToken);
            }

            await server.PublishEvent(TestTopic, "tadaaaaa!", cancellationToken);
        }
        catch (OperationCanceledException)
        {
            FireEventMessagesCancelled(logger);
        }
        catch (Exception ex)
        {
            FireEventMessagesFailed(logger, ex);
        }
    }

    public OperationResult CreateErrorInstance(string message) => throw new NotImplementedException();

    [LoggerMessage(LogLevel.Debug, "Fire event messages cancelled.")]
    private static partial void FireEventMessagesCancelled(ILogger logger);

    [LoggerMessage(LogLevel.Error, "Fire event messages failed.")]
    private static partial void FireEventMessagesFailed(ILogger logger, Exception exception);
}

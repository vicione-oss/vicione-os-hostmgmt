using System.Text.Json.Serialization.Metadata;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Contracts;
using Microsoft.Extensions.Logging;

namespace HostManagement.Communication.NamedPipe.RequestHandlers;

/// <summary>
/// Represents a <see cref="EchoRequestHandler"/>.
/// </summary>
/// <param name="logger">The logger.</param>
internal sealed partial class EchoRequestHandler(ILogger logger) : IRequestHandler, INotifyPublishEventRequested
{
    private const string TestTopic = "test"; // Must be processed by the client (pipe sample in this case)

    public event EventHandler<PublishEventRequestedEventArgs>? PublishEventRequested;

    /// <summary>
    /// The topic of the <see cref="EchoRequestHandler"/>.
    /// </summary>
    public string Topic => Topics.Echo;

    public JsonTypeInfo JsonTypeInfo => throw new NotImplementedException();

    /// <summary>
    /// Handles the echo request.
    /// </summary>
    /// <inheritdoc/>
    public Task<string> HandleAsync(
        string messageContent,
        Guid correlationId,
        CancellationToken cancellationToken = default)
    {
        switch (messageContent)
        {
            case "ping":
                return Task.FromResult("pong");
            case "trigger":
                _ = FireEventMessages(correlationId, cancellationToken);
                return Task.FromResult("Wait for it!");
            default:
                if (messageContent.Contains("stress"))
                    return Task.FromResult($"{messageContent}-response");
                else
                    return Task.FromResult("No!");
        }
    }

    private async Task FireEventMessages(Guid correlationId, CancellationToken cancellationToken)
    {
        try
        {
            const int Duration = 3;

            for (var i = 0; i < Duration; i++)
            {
                await Task.Delay(1000, cancellationToken);

                PublishEventRequested?.Invoke(
                    this,
                    new PublishEventRequestedEventArgs(TestTopic, $"{Duration - i}...", correlationId));
            }

            PublishEventRequested?.Invoke(
                this,
                new PublishEventRequestedEventArgs(TestTopic, "tadaaaaa!", correlationId));
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

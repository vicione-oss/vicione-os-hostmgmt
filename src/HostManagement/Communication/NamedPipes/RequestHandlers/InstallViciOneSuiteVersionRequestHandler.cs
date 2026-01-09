using System.Text.Json;
using HostManagement.ViciOneSuite;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Serialization;
using Microsoft.Extensions.Logging;

namespace HostManagement.Communication.NamedPipes.RequestHandlers;

/// <summary>
/// Represents the <see cref="InstallViciOneSuiteVersionRequestHandler"/>.
/// </summary>
internal sealed class InstallViciOneSuiteVersionRequestHandler : IRequestHandler
{
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new <see cref="InstallViciOneSuiteVersionRequestHandler"/>.
    /// </summary>
    /// <param name="logger">An <see cref="ILogger"/> implementation.</param>
    public InstallViciOneSuiteVersionRequestHandler(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// The topic of the <see cref="InstallViciOneSuiteVersionRequestHandler"/>.
    /// </summary>
    public string Topic => Topics.InstallViciOneSuiteVersion;

    /// <summary>
    /// Handles the install suite version request.
    /// </summary>
    /// <param name="messageContent">The message content.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The response.</returns>
    /// <exception cref="InvalidOperationException"/>
    public async Task<string> Handle(string messageContent, CancellationToken cancellationToken)
    {
        var isCliCall = false;

        try
        {
            var cliMessageContent = SourceGeneratedTypeSerializer
                .Deserialize(messageContent, CliSourceGenerationContext.Default.CliMessageContent);

            isCliCall = true;
            messageContent = cliMessageContent.MessageContent;
        }
        catch (JsonException)
        {
            // Call did not come from CLI, no action required here.
        }

        try
        {
            var manager = new ViciOneSuiteManager(_logger);
            var result = await manager.InstallViciOneSuiteVersion(
                messageContent,
                cancellationToken,
                isCliCall);

            return SourceGeneratedTypeSerializer
                .Serialize(result, SourceGenerationContext.Default.InstallViciOneSuiteVersionResult);
        }
        catch (JsonException)
        {
            throw new InvalidOperationException(
                $"{nameof(InstallViciOneSuiteVersionResult)} could not be serialized into a string.");
        }
    }
}

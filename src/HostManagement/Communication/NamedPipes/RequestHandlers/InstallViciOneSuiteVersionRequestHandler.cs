using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.Logging;
using HostManagement.ViciOneSuite;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Serialization;

namespace HostManagement.Communication.NamedPipes.RequestHandlers;

/// <summary>
/// Represents the <see cref="InstallViciOneSuiteVersionRequestHandler"/>.
/// </summary>
/// <param name="logger">An <see cref="ILogger"/> implementation.</param>
internal sealed class InstallViciOneSuiteVersionRequestHandler(ILogger logger)
    : RequestHandler<InstallViciOneSuiteVersionResult>
{
    /// <summary>
    /// The topic of the <see cref="InstallViciOneSuiteVersionRequestHandler"/>.
    /// </summary>
    public override string Topic => Topics.InstallViciOneSuiteVersion;

    public override JsonTypeInfo<InstallViciOneSuiteVersionResult> TypedJsonTypeInfo
        => SourceGenerationContext.Default.InstallViciOneSuiteVersionResult;

    /// <summary>
    /// Handles the install suite version request.
    /// </summary>
    /// <param name="messageContent">The message content.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The response.</returns>
    /// <exception cref="InvalidOperationException"/>
    public override async Task<string> Handle(string messageContent, CancellationToken cancellationToken)
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
            var manager = new ViciOneSuiteManager(logger);
            var result = await manager.InstallViciOneSuiteVersion(
                messageContent,
                cancellationToken,
                isCliCall);

            return SourceGeneratedTypeSerializer.Serialize(result, TypedJsonTypeInfo);
        }
        catch (JsonException)
        {
            throw new InvalidOperationException(
                $"{nameof(InstallViciOneSuiteVersionResult)} could not be serialized into a string.");
        }
    }
}

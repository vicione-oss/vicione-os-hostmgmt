using System.Text.Json;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Serialization;
using HostManagement.SystemConfig;

namespace HostManagement.Communication.NamedPipes.RequestHandlers;

/// <summary>
/// Represents the <see cref="LoadSystemConfigurationRequestHandler"/>.
/// </summary>
internal sealed class LoadSystemConfigurationRequestHandler : IRequestHandler
{
    /// <summary>
    /// The topic of the <see cref="LoadSystemConfigurationRequestHandler"/>.
    /// </summary>
    public string Topic => Topics.LoadSystemConfiguration;

    /// <summary>
    /// Handles the load system configuration request.
    /// </summary>
    /// <param name="messageContent">The message content.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The response.</returns>
    /// <exception cref="InvalidOperationException"/>
    public async Task<string> Handle(string messageContent, CancellationToken cancellationToken)
    {
        try
        {
            var manager = new SystemConfigurationManager();
            var result = await manager.LoadConfiguration(cancellationToken);

            return SourceGeneratedTypeSerializer
                .Serialize(result, SourceGenerationContext.Default.LoadSystemConfigurationResult);
        }
        catch (JsonException)
        {
            throw new InvalidOperationException(
                $"{nameof(LoadSystemConfigurationResult)} could not be serialized into a string.");
        }
    }
}

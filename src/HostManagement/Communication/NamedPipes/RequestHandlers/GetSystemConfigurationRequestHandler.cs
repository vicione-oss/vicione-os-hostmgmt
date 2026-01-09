using System.IO.Abstractions;
using System.Text.Json;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Serialization;
using HostManagement.SystemConfig;

namespace HostManagement.Communication.NamedPipes.RequestHandlers;

/// <summary>
/// Represents the <see cref="GetSystemConfigurationRequestHandler"/>.
/// </summary>
internal sealed class GetSystemConfigurationRequestHandler : IRequestHandler
{
    /// <summary>
    /// The topic of the <see cref="GetSystemConfigurationRequestHandler"/>.
    /// </summary>
    public string Topic => Topics.GetSystemConfiguration;

    /// <summary>
    /// Handles the get system configuration request.
    /// </summary>
    /// <param name="messageContent">The message content.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The response.</returns>
    /// <exception cref="InvalidOperationException"/>
    public async Task<string> Handle(string messageContent, CancellationToken cancellationToken)
    {
        try
        {
            var configurationService = new SystemConfigurationService(new FileSystem());

            var result = await configurationService.GetConfiguration(cancellationToken);

            return SourceGeneratedTypeSerializer
                .Serialize(result, SourceGenerationContext.Default.GetSystemConfigurationResult);
        }
        catch (JsonException)
        {
            throw new InvalidOperationException(
                $"{nameof(GetSystemConfigurationResult)} could not be serialized into a string.");
        }
    }
}

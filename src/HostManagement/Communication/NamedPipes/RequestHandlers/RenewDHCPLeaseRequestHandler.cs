using System.Text.Json;
using HostManagement.RenewDHCPLease;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Serialization;
using Microsoft.Extensions.Logging;

namespace HostManagement.Communication.NamedPipes.RequestHandlers;

/// <summary>
/// Represents the <see cref="RenewDHCPLeaseRequestHandler"/>.
/// </summary>
internal sealed class RenewDHCPLeaseRequestHandler : IRequestHandler
{
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new <see cref="RenewDHCPLeaseRequestHandler"/>.
    /// </summary>
    /// <param name="logger">An <see cref="ILogger"/> implementation.</param>
    public RenewDHCPLeaseRequestHandler(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// The topic of the <see cref="RenewDHCPLeaseRequestHandler"/>.
    /// </summary>
    public string Topic => Topics.RenewDHCPLease;

    /// <summary>
    /// Handles the renew DHCP lease request.
    /// </summary>
    /// <param name="messageContent">The message content.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The response.</returns>
    /// <exception cref="InvalidOperationException"/>
    public async Task<string> Handle(string messageContent, CancellationToken cancellationToken)
    {
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(messageContent, nameof(messageContent));

            var manager = new RenewDHCPLeaseManager(_logger);
            var result = await manager.RenewDHCPLease(messageContent, cancellationToken);

            return SourceGeneratedTypeSerializer
                .Serialize(result, SourceGenerationContext.Default.RenewDHCPLeaseResult);
        }
        catch (JsonException)
        {
            throw new InvalidOperationException(
                $"{nameof(RenewDHCPLeaseResult)} could not be serialized into a string.");
        }
    }
}

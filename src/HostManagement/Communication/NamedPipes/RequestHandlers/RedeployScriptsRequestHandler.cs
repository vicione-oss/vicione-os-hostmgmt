using System.Text.Json;
using HostManagement.Scripts;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Communication.Serialization;

namespace HostManagement.Communication.NamedPipes.RequestHandlers;

/// <summary>
/// Represents the <see cref="RedeployScriptsRequestHandler"/>.
/// </summary>
internal sealed class RedeployScriptsRequestHandler : IRequestHandler
{
    /// <summary>
    /// The topic of the <see cref="RedeployScriptsRequestHandler"/>.
    /// </summary>
    public string Topic => Topics.RedeployScripts;

    /// <summary>
    /// Handles the redeploy scripts request.
    /// </summary>
    /// <param name="messageContent">The message content.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The response.</returns>
    /// <exception cref="InvalidOperationException"/>
    public Task<string> Handle(string messageContent, CancellationToken cancellationToken)
    {
        RedeployScriptsResult result;

        try
        {
            ResourceManager.GetInstance().RedeployScripts();

            result = new RedeployScriptsResult { Status = OperationStatus.Success, Message = null };
        }
        catch (Exception ex)
        {
            result = new RedeployScriptsResult { Status = OperationStatus.Error, Message = ex.Message };
        }

        try
        {
            var serializeResult = SourceGeneratedTypeSerializer
                .Serialize(result, SourceGenerationContext.Default.RedeployScriptsResult);

            return Task.FromResult(serializeResult);
        }
        catch (JsonException)
        {
            throw new InvalidOperationException(
                $"{nameof(RedeployScriptsResult)} could not be serialized into a string.");
        }
    }
}

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using HostManagement.Scripts;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Communication.Serialization;

namespace HostManagement.Communication.NamedPipes.RequestHandlers;

/// <summary>
/// Represents the <see cref="RedeployScriptsRequestHandler"/>.
/// </summary>
internal sealed class RedeployScriptsRequestHandler : RequestHandler<RedeployScriptsResult>
{
    /// <summary>
    /// The topic of the <see cref="RedeployScriptsRequestHandler"/>.
    /// </summary>
    public override string Topic => Topics.RedeployScripts;

    public override JsonTypeInfo<RedeployScriptsResult> TypedJsonTypeInfo
        => SourceGenerationContext.Default.RedeployScriptsResult;

    /// <summary>
    /// Handles the redeploy scripts request.
    /// </summary>
    /// <param name="messageContent">The message content.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The response.</returns>
    /// <exception cref="InvalidOperationException"/>
    public override Task<string> Handle(string messageContent, CancellationToken cancellationToken)
    {
        RedeployScriptsResult result;

        try
        {
            var wasDeploymentSuccessful = ResourceManager
                .GetInstance()
                .DeployScripts(isRedeployment: true);

            if (wasDeploymentSuccessful)
            {
                result = new RedeployScriptsResult
                {
                    Status = OperationStatus.Success,
                    Message = "Scripts deployed successfully."
                };
            }
            else
            {
                result = new RedeployScriptsResult
                {
                    Status = OperationStatus.Error,
                    Message = "Script deployment failed."
                };
            }
        }
        catch (Exception ex)
        {
            result = new RedeployScriptsResult
            {
                Status = OperationStatus.Error,
                Message = ex.Message
            };
        }

        try
        {
            var serializeResult = SourceGeneratedTypeSerializer.Serialize(result, TypedJsonTypeInfo);

            return Task.FromResult(serializeResult);
        }
        catch (JsonException)
        {
            throw new InvalidOperationException(
                $"{nameof(RedeployScriptsResult)} could not be serialized into a string.");
        }
    }
}

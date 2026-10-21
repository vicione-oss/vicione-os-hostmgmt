using HostManagement.Scripts;
using HostManagement.Shared;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Serialization;

namespace HostManagement.Communication.NamedPipe.RequestHandlers;

/// <summary>
/// Represents the <see cref="RedeployScriptsRequestHandler"/>.
/// </summary>
internal sealed class RedeployScriptsRequestHandler : RequestHandler<RedeployScriptsResult>
{
    /// <summary>
    /// The topic of the <see cref="RedeployScriptsRequestHandler"/>.
    /// </summary>
    public override string Topic => Topics.RedeployScripts;

    /// <summary>
    /// Handles the redeploy scripts request.
    /// </summary>
    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException"/>
    public override Task<string> HandleAsync(
        string messageContent,
        Guid correlationId,
        CancellationToken cancellationToken)
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

        var serializeResult = MessageSerializer.Serialize(result);

        return Task.FromResult(serializeResult);
    }
}

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
    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException"/>
    public override Task<string> HandleAsync(string messageContent, CancellationToken cancellationToken)
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

        var serializeResult = SourceGeneratedTypeSerializer.Serialize(result, TypedJsonTypeInfo);

        return Task.FromResult(serializeResult);
    }
}

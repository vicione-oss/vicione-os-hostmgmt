using System.CommandLine;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;

namespace HostManagement.Communication.CommandLine.Commands.Deployment.Redeploy;

/// <summary>
/// Represents the redeploy command.
/// </summary>
internal sealed class RedeployCommand : Command
{
    /// <summary>
    /// Initializes a new <see cref="DeploymentCommand"/>.
    /// </summary>
    public RedeployCommand()
        : base(CliStrings.Deployment.RedeployCommandName, CliStrings.Deployment.RedeployCommandDescription)
        => SetAction((parseResult, cancellationToken) => RedeployHandler(cancellationToken));

    private static async Task RedeployHandler(CancellationToken cancellationToken)
    {
        if (!await NamedPipeRequestHelper.RedeployScripts(cancellationToken))
            return;

        SimpleOutput.WriteLine(CliStrings.Deployment.ScriptsRedeployed);
    }
}

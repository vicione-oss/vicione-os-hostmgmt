using System.CommandLine;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Communication.CommandLine.Output;

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
        : base(
            CommandLineStrings_Deployment.RedeployCommandName,
            CommandLineStrings_Deployment.RedeployCommandDescription)
        => SetAction((parseResult, cancellationToken) => RedeployHandler(cancellationToken));

    private static async Task RedeployHandler(CancellationToken cancellationToken)
    {
        if (!await NamedPipeRequestHelper.RedeployScripts(cancellationToken))
            return;

        SimpleOutput.WriteLine(CommandLineStrings_Deployment.ScriptsDeployed);
    }
}

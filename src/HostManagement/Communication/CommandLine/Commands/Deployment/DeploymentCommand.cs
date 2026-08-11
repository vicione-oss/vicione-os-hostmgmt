using System.CommandLine;
using HostManagement.Communication.CommandLine.Commands.Deployment.Redeploy;

namespace HostManagement.Communication.CommandLine.Commands.Deployment;

/// <summary>
/// Represents the deployment command.
/// </summary>
internal sealed class DeploymentCommand : Command
{
    /// <summary>
    /// Initializes a new <see cref="DeploymentCommand"/>.
    /// </summary>
    public DeploymentCommand()
        : base(
            CommandLineStrings_Deployment.DeploymentCommandName,
            CommandLineStrings_Deployment.DeploymentCommandDescription)
        => Add(new RedeployCommand());
}

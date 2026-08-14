using System.CommandLine;
using HostManagement.Communication.CommandLine.Commands.Deployment.Redeploy;
using HostManagement.Communication.CommandLine.Resources;

namespace HostManagement.Communication.CommandLine.Commands.Deployment;

/// <summary>
/// Represents the deployment command.
/// </summary>
internal sealed class DeploymentCommand : Command
{
    /// <summary>
    /// Initializes a new <see cref="DeploymentCommand"/>.
    /// </summary>
    public DeploymentCommand() : base(CliStrings.Deployment.CommandName, CliStrings.Deployment.CommandDescription)
        => Add(new RedeployCommand());
}

using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Communication.CommandLine.Output;

namespace HostManagement.Communication.CommandLine.Commands.Deployment;

/// <summary>
/// Represents the script deployment command.
/// </summary>
internal sealed class ScriptDeploymentCommand : CommandBase
{
    /// <summary>
    /// Initializes a new <see cref="ScriptDeploymentCommand"/>.
    /// </summary>
    public ScriptDeploymentCommand() : base(
        CommandLineStrings_Deployment.DeploymentCommandName,
        CommandLineStrings_Deployment.DeploymentCommandDescription)
    {
        Add(
            CommandLineStrings_Deployment.RedeployCommandName,
            CommandLineStrings_Deployment.RedeployCommandDescription,
            RedeployHandler);
    }

    private static async Task RedeployHandler()
    {
        if (!await NamedPipeRequestHelper.RedeployScripts())
            return;

        SimpleOutput.WriteLine(CommandLineStrings_Deployment.ScriptsDeployed);
    }
}

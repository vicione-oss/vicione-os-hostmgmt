using System.CommandLine;
using HostManagement.Communication.CommandLine.Extensions;
using HostManagement.Communication.CommandLine.Helper;

namespace HostManagement.Communication.CommandLine.Commands.Deployment;

/// <summary>
/// Represents the <see cref="ScriptDeploymentCommandGroupProvider"/>.
/// </summary>
internal static class ScriptDeploymentCommandGroupProvider
{
    private static readonly Command s_redeployCommand = CommandHelper.ConstructCommand(
        "redeploy",
        "Redeploys the application scripts.",
        RedeployHandler);

    /// <summary>
    /// Adds the script deployment command group.
    /// </summary>
    /// <param name="parentCommand">The parent command.</param>
    internal static void AddScriptDeploymentCommandGroup(this Command parentCommand)
    {
        var scriptDeploymentCommand = parentCommand.AddCommand(
            "deployment",
            "Functions for script deployment.");

        scriptDeploymentCommand.AddCommand(s_redeployCommand);
    }

    private static async Task RedeployHandler()
    {
        if (!await NamedPipeRequestHelper.RedeployScripts())
            return;

        Console.WriteLine("Scripts redeployed successfully.");
    }
}

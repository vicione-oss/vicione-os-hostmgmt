using System.CommandLine;
using HostManagement.Communication.CommandLine.Commands.Deployment;
using HostManagement.Communication.CommandLine.Commands.Network;
using HostManagement.Communication.CommandLine.Commands.Service;
using HostManagement.Communication.CommandLine.Commands.ViciOneSuite;

namespace HostManagement.Communication.CommandLine;

/// <summary>
/// Represents the <see cref="CommandInterface"/>.
/// </summary>
internal static class CommandInterface
{
    /// <summary>
    /// Gets the full command structure of the app.
    /// </summary>
    /// <returns><see cref="RootCommand"/></returns>
    public static RootCommand GetCommandStructure()
    {
        var rootCommand = new RootCommand("ViciOne Host Management Tool");
        rootCommand.AddNetworkCommandGroup();
        rootCommand.AddServiceCommandGroup();
        rootCommand.AddSuiteCommandGroup();
        rootCommand.AddScriptDeploymentCommandGroup();

        return rootCommand;
    }
}

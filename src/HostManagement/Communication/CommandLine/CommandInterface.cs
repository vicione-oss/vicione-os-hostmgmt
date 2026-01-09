using System.CommandLine;
using HostManagement.Communication.CommandLine.Commands.Deployment;
using HostManagement.Communication.CommandLine.Commands.Network;
using HostManagement.Communication.CommandLine.Commands.Service;
using HostManagement.Communication.CommandLine.Commands.System;
using HostManagement.Communication.CommandLine.Commands.ViciOneSuite;

namespace HostManagement.Communication.CommandLine;

/// <summary>
/// Represents the <see cref="CommandInterface"/>.
/// </summary>
internal sealed class CommandInterface
{
    /// <summary>
    /// Creates the full command structure of the app.
    /// </summary>
    /// <returns><see cref="RootCommand"/></returns>
    public static RootCommand CreateRootCommand()
    {
        return new RootCommand("ViciOne Host Management Tool")
        {
            new NetworkCommand(),
            new ServiceCommand(),
            new ViciOneSuiteCommand(),
            new ScriptDeploymentCommand(),
            new SystemCommand()
        };
    }
}

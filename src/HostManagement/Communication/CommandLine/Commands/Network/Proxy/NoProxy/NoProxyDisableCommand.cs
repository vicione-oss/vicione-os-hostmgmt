using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.Proxy.NoProxy;

/// <summary>
/// Represents the no-proxy disable command.
/// </summary>
internal sealed class NoProxyDisableCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="NoProxyDisableCommand"/>.
    /// </summary>
    public NoProxyDisableCommand()
        : base(
            CommandLineStrings_Common.DisableCommandName,
            CommandLineStrings_Proxy.NoProxyListDisableCommandDescription)
        => SetAction((parseResult, cancellationToken) => DisableHandler(cancellationToken));

    private static async Task DisableHandler(CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (!config.NetworkProxySettings.NoProxy.Enabled)
        {
            SimpleOutput.WriteLineInformation(CommandLineStrings_Proxy.NoProxyListAlreadyDisabled);
            return;
        }

        config.NetworkProxySettings.NoProxy.Enabled = false;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        SimpleOutput.WriteLine(CommandLineStrings_Proxy.NoProxyListDisabled);
    }
}

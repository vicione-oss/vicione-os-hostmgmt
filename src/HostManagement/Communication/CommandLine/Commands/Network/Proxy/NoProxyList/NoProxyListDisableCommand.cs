using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.Proxy.NoProxyList;

/// <summary>
/// Represents the no-proxy list disable command.
/// </summary>
internal sealed class NoProxyListDisableCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="NoProxyListDisableCommand"/>.
    /// </summary>
    public NoProxyListDisableCommand() : base(
        CommandLineStrings_Common.DisableCommandName,
        CommandLineStrings_Proxy.NoProxyListDisableCommandDescription)
    {
        SetAction((parseResult, cancellationToken) => DisableHandler(cancellationToken));
    }

    private static async Task DisableHandler(CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (!config.NetworkProxySettings.DoNotProxyListEnabled)
        {
            SimpleOutput.WriteLineInformation(CommandLineStrings_Proxy.NoProxyListAlreadyDisabled);
            return;
        }

        config.NetworkProxySettings.DoNotProxyListEnabled = false;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        SimpleOutput.WriteLine(CommandLineStrings_Proxy.NoProxyListDisabled);
    }
}

using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.Proxy.NoProxyList;

/// <summary>
/// Represents the no-proxy list clear command.
/// </summary>
internal sealed class NoProxyListClearCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="NoProxyListClearCommand"/>.
    /// </summary>
    public NoProxyListClearCommand() : base(
        CommandLineStrings_Common.ClearCommandName,
        CommandLineStrings_Proxy.NoProxyListClearCommandDescription)
    {
        SetAction((parseResult, cancellationToken) => ClearHandler(cancellationToken));
    }

    private static async Task ClearHandler(CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (config.NetworkProxySettings.DoNotProxyList.Count == 0)
        {
            SimpleOutput.WriteLineInformation(CommandLineStrings_Proxy.NoProxyListNotConfigured);
            return;
        }

        config.NetworkProxySettings.DoNotProxyList.Clear();
        config.NetworkProxySettings.DoNotProxyListEnabled = false;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        SimpleOutput.WriteLine(CommandLineStrings_Proxy.NoProxyListCleared);
    }
}

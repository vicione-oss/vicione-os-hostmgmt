using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.Proxy.NoProxy;

/// <summary>
/// Represents the no-proxy clear command.
/// </summary>
internal sealed class NoProxyClearCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="NoProxyClearCommand"/>.
    /// </summary>
    public NoProxyClearCommand() : base(
        CommandLineStrings_Common.ClearCommandName,
        CommandLineStrings_Proxy.NoProxyListClearCommandDescription)
            => SetAction((parseResult, cancellationToken) => ClearHandler(cancellationToken));

    private static async Task ClearHandler(CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (config.NetworkProxySettings.NoProxy.Entries.Count == 0)
        {
            SimpleOutput.WriteLineInformation(CommandLineStrings_Proxy.NoProxyListNotConfigured);
            return;
        }

        config.NetworkProxySettings.NoProxy.Entries.Clear();
        config.NetworkProxySettings.NoProxy.Enabled = false;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        SimpleOutput.WriteLine(CommandLineStrings_Proxy.NoProxyListCleared);
    }
}

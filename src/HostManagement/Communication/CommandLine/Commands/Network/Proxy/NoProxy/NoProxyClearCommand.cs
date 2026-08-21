using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
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
    public NoProxyClearCommand()
        : base(CliStrings.Common.ClearCommandName, CliStrings.Network.Proxy.NoProxy.ClearCommandDescription)
        => SetAction((parseResult, cancellationToken) => ClearHandler(cancellationToken));

    private static async Task ClearHandler(CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (config.NetworkProxySettings.NoProxy.Entries.Count == 0)
        {
            SimpleOutput.WriteLineInformation(CliStrings.Network.Proxy.NoProxy.NotConfigured);
            return;
        }

        config.NetworkProxySettings.NoProxy.Entries.Clear();
        config.NetworkProxySettings.NoProxy.Enabled = false;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        SimpleOutput.WriteLine(CliStrings.Network.Proxy.NoProxy.Cleared);
    }
}

using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.Proxy.NoProxy;

/// <summary>
/// Represents the no-proxy enable command.
/// </summary>
internal sealed class NoProxyEnableCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="NoProxyEnableCommand"/>.
    /// </summary>
    public NoProxyEnableCommand()
        : base(CliStrings.Common.EnableCommandName, CliStrings.Network.Proxy.NoProxy.EnableCommandDescription)
        => SetAction((parseResult, cancellationToken) => EnableHandler(cancellationToken));

    private static async Task EnableHandler(CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (config.NetworkProxySettings.NoProxy.Enabled)
        {
            SimpleOutput.WriteLineInformation(CliStrings.Network.Proxy.NoProxy.AlreadyEnabled);
            return;
        }

        if (config.NetworkProxySettings.NoProxy.Entries.Count == 0)
        {
            SimpleOutput.WriteLineError(CliStrings.Network.Proxy.NoProxy.NotConfigured);
            return;
        }

        config.NetworkProxySettings.NoProxy.Enabled = true;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        SimpleOutput.WriteLine(CliStrings.Network.Proxy.NoProxy.Enabled);
    }
}

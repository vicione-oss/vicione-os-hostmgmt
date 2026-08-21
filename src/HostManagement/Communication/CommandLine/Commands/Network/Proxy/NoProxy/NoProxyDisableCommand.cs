using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
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
        : base(CliStrings.Common.DisableCommandName, CliStrings.Network.Proxy.NoProxy.DisableCommandDescription)
        => SetAction((parseResult, cancellationToken) => DisableHandler(cancellationToken));

    private static async Task DisableHandler(CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (!config.NetworkProxySettings.NoProxy.Enabled)
        {
            SimpleOutput.WriteLineInformation(CliStrings.Network.Proxy.NoProxy.AlreadyDisabled);
            return;
        }

        config.NetworkProxySettings.NoProxy.Enabled = false;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        SimpleOutput.WriteLine(CliStrings.Network.Proxy.NoProxy.Disabled);
    }
}

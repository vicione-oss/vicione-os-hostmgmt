using HostManagement.Communication.CommandLine.Output;
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
        : base(
            CommandLineStrings_Common.EnableCommandName,
            CommandLineStrings_Proxy.NoProxyListEnableCommandDescription)
        => SetAction((parseResult, cancellationToken) => EnableHandler(cancellationToken));

    private static async Task EnableHandler(CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (config.NetworkProxySettings.NoProxy.Enabled)
        {
            SimpleOutput.WriteLineInformation(CommandLineStrings_Proxy.NoProxyListAlreadyEnabled);
            return;
        }

        if (config.NetworkProxySettings.NoProxy.Entries.Count == 0)
        {
            SimpleOutput.WriteLineError(CommandLineStrings_Proxy.NoProxyListNotConfigured);
            return;
        }

        config.NetworkProxySettings.NoProxy.Enabled = true;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        SimpleOutput.WriteLine(CommandLineStrings_Proxy.NoProxyListEnabled);
    }
}

using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.Proxy.NoProxyList;

/// <summary>
/// Represents the no-proxy list enable command.
/// </summary>
internal sealed class NoProxyListEnableCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="NoProxyListEnableCommand"/>.
    /// </summary>
    public NoProxyListEnableCommand() : base(
        CommandLineStrings_Common.EnableCommandName,
        CommandLineStrings_Proxy.NoProxyListEnableCommandDescription)
    {
        SetAction((parseResult, cancellationToken) => EnableHandler(cancellationToken));
    }

    private static async Task EnableHandler(CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (config.NetworkProxySettings.DoNotProxyListEnabled)
        {
            SimpleOutput.WriteLineInformation(CommandLineStrings_Proxy.NoProxyListAlreadyEnabled);
            return;
        }

        if (config.NetworkProxySettings.DoNotProxyList.Count == 0)
        {
            SimpleOutput.WriteLineError(CommandLineStrings_Proxy.NoProxyListNotConfigured);
            return;
        }

        config.NetworkProxySettings.DoNotProxyListEnabled = true;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        SimpleOutput.WriteLine(CommandLineStrings_Proxy.NoProxyListEnabled);
    }
}

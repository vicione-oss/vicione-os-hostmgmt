using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.Proxy.NoProxyList;

/// <summary>
/// Represents the no-proxy list list command.
/// </summary>
internal sealed class NoProxyListListCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="NoProxyListListCommand"/>.
    /// </summary>
    public NoProxyListListCommand() : base(
        CommandLineStrings_Common.ListCommandName,
        CommandLineStrings_Proxy.NoProxyListListCommandDescription)
    {
        SetAction((parseResult, cancellationToken) => ListHandler(cancellationToken));
    }

    private static async Task ListHandler(CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (config.NetworkProxySettings.DoNotProxyList.Count == 0)
        {
            SimpleOutput.WriteLineInformation(CommandLineStrings_Proxy.NoProxyListNotConfigured);
            return;
        }

        new ListOutput()
            .WithTitle(CommandLineStrings_Proxy.NoProxyList, config.NetworkProxySettings.DoNotProxyListEnabled)
            .WithEach(config.NetworkProxySettings.DoNotProxyList.Select(CliOutputValue.Create))
            .WriteLine();
    }
}

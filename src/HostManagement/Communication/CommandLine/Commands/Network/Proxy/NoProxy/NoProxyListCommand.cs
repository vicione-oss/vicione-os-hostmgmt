using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.Proxy.NoProxy;

/// <summary>
/// Represents the no-proxy list command.
/// </summary>
internal sealed class NoProxyListCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="NoProxyListCommand"/>.
    /// </summary>
    public NoProxyListCommand()
        : base(
            CommandLineStrings_Common.ListCommandName,
            CommandLineStrings_Proxy.NoProxyListListCommandDescription)
        => SetAction((parseResult, cancellationToken) => ListHandler(cancellationToken));

    private static async Task ListHandler(CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (config.NetworkProxySettings.NoProxy.Entries.Count == 0)
        {
            SimpleOutput.WriteLineInformation(CommandLineStrings_Proxy.NoProxyListNotConfigured);
            return;
        }

        new ListOutput()
            .WithTitle(CommandLineStrings_Proxy.NoProxyList, config.NetworkProxySettings.NoProxy.Enabled)
            .WithEach(config.NetworkProxySettings.NoProxy.Entries.Select(CliOutputValue.Create))
            .WriteLine();
    }
}

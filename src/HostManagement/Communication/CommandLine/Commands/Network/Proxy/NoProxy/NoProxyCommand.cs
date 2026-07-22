using System.CommandLine;

namespace HostManagement.Communication.CommandLine.Commands.Network.Proxy.NoProxy;

/// <summary>
/// Represents the no-proxy command.
/// </summary>
internal sealed class NoProxyCommand : Command
{
    /// <summary>
    /// Initializes a new <see cref="NoProxyCommand"/>.
    /// </summary>
    public NoProxyCommand() : base(
        CommandLineStrings_Proxy.NoProxyListCommandName,
        CommandLineStrings_Proxy.NoProxyListCommandDescription)
    {
        Add(new NoProxyEnableCommand());
        Add(new NoProxyDisableCommand());
        Add(new NoProxyListCommand());
        Add(new NoProxyAddCommand(CommonOptions.s_itemsOption));
        Add(new NoProxyRemoveCommand(CommonOptions.s_itemsOption));
        Add(new NoProxySetCommand(CommonOptions.s_itemsOption));
        Add(new NoProxyClearCommand());
    }
}

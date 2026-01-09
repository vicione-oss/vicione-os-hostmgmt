using System.CommandLine;

namespace HostManagement.Communication.CommandLine.Commands.Network.Proxy.NoProxyList;

/// <summary>
/// Represents the no-proxy list command.
/// </summary>
internal sealed class NoProxyListCommand : Command
{
    /// <summary>
    /// Initializes a new <see cref="NoProxyListCommand"/>.
    /// </summary>
    public NoProxyListCommand() : base(
        CommandLineStrings_Proxy.NoProxyListCommandName,
        CommandLineStrings_Proxy.NoProxyListCommandDescription)
    {
        Add(new NoProxyListEnableCommand());
        Add(new NoProxyListDisableCommand());
        Add(new NoProxyListListCommand());
        Add(new NoProxyListAddCommand(CommonOptions.s_itemsOption));
        Add(new NoProxyListRemoveCommand(CommonOptions.s_itemsOption));
        Add(new NoProxyListSetCommand(CommonOptions.s_itemsOption));
        Add(new NoProxyListClearCommand());
    }
}

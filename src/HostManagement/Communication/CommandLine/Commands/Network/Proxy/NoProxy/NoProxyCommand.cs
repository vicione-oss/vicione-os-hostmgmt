using System.CommandLine;

namespace HostManagement.Communication.CommandLine.Commands.Network.Proxy.NoProxy;

/// <summary>
/// Represents the no-proxy command.
/// </summary>
internal sealed class NoProxyCommand : Command
{
    private static readonly Argument<string[]> s_noProxyEntriesArgument = new("entries")
    {
        Description = "The list of no-proxy entries.",
        Arity = ArgumentArity.OneOrMore
    };

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
        Add(new NoProxyAddCommand(s_noProxyEntriesArgument));
        Add(new NoProxyRemoveCommand(s_noProxyEntriesArgument));
        Add(new NoProxySetCommand(s_noProxyEntriesArgument));
        Add(new NoProxyClearCommand());
    }
}

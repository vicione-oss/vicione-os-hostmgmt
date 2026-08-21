using System.CommandLine;
using HostManagement.Communication.CommandLine.Resources;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS.StaticHosts;

/// <summary>
/// Represents the static hosts command.
/// </summary>
internal sealed class StaticHostsCommand : Command
{
    private static readonly Option<string> s_hostnameOption = new("--hostname", "-n")
    {
        Description = "The hostname.",
        Required = true,
        Arity = ArgumentArity.ExactlyOne
    };

    /// <summary>
    /// Initializes a new <see cref="StaticHostsCommand"/>.
    /// </summary>
    public StaticHostsCommand()
        : base(CliStrings.Network.DNS.StaticHosts.CommandName, CliStrings.Network.DNS.StaticHosts.CommandDescription)
    {
        Add(new StaticHostsListCommand());
        Add(new StaticHostsEnableCommand());
        Add(new StaticHostsDisableCommand());
        Add(new StaticHostsAddCommand(CommonOptions.s_ipAddressOption, s_hostnameOption));
        Add(new StaticHostsRemoveCommand(CommonOptions.s_ipAddressOption, s_hostnameOption));
        Add(new StaticHostsClearCommand());
    }
}

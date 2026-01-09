using System.CommandLine;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS.StaticHosts;

/// <summary>
/// Represents the static hosts command.
/// </summary>
internal sealed class DNSStaticHostsCommand : Command
{
    private static readonly Option<string> s_hostnameOption = new("--hostname", "-n")
    {
        Description = "The hostname.",
        Required = true,
        Arity = ArgumentArity.ExactlyOne
    };

    /// <summary>
    /// Initializes a new <see cref="DNSStaticHostsCommand"/>.
    /// </summary>
    public DNSStaticHostsCommand() : base(
        CommandLineStrings_DNS.DNSStaticHostsCommandName,
        CommandLineStrings_DNS.DNSStaticHostsCommandDescription)
    {
        Add(new DNSStaticHostsEnableCommand());
        Add(new DNSStaticHostsDisableCommand());
        Add(new DNSStaticHostsListCommand());
        Add(new DNSStaticHostsAddCommand(CommonOptions.s_ipAddressOption, s_hostnameOption));
        Add(new DNSStaticHostsRemoveCommand(CommonOptions.s_ipAddressOption, s_hostnameOption));
        Add(new DNSStaticHostsClearCommand());
    }
}

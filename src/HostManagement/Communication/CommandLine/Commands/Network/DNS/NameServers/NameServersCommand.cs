using System.CommandLine;
using System.Net;
using HostManagement.Communication.CommandLine.Extensions;
using HostManagement.Communication.CommandLine.Resources;
using HostManagement.Communication.CommandLine.Validators;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS.NameServers;

/// <summary>
/// Represents the DNS name servers command.
/// </summary>
internal sealed class NameServersCommand : Command
{
    private static readonly Argument<IPAddress[]> s_ipAddressesArgument = new Argument<IPAddress[]>("addresses")
    {
        Description = "The list of IP addresses.",
        Arity = ArgumentArity.OneOrMore,
        CustomParser = IPAddressParser.ParseMultiple
    }
    .WithValidation(argumentResult => IPAddressValidator.Validate(argumentResult, "IP address"));

    private static readonly Option<IPAddress> s_oldNameServerOption = new Option<IPAddress>("--old-address", "-o")
    {
        Description = "The old IP address.",
        Required = true,
        Arity = ArgumentArity.ExactlyOne,
        CustomParser = IPAddressParser.ParseSingle
    }
    .WithValidation(optionResult => IPAddressValidator.Validate(optionResult, "Old name server"));

    private static readonly Option<IPAddress> s_newNameServerOption = new Option<IPAddress>("--new-address", "-a")
    {
        Description = "The new IP address.",
        Required = true,
        Arity = ArgumentArity.ExactlyOne,
        CustomParser = IPAddressParser.ParseSingle
    }
    .WithValidation(optionResult => IPAddressValidator.Validate(optionResult, "New name server"));

    /// <summary>
    /// Initializes a new <see cref="NameServersCommand"/>.
    /// </summary>
    public NameServersCommand()
        : base(CliStrings.Network.DNS.NameServers.CommandName, CliStrings.Network.DNS.NameServers.CommandDescription)
    {
        Add(new NameServersListCommand());
        Add(new NameServersEnableCommand());
        Add(new NameServersDisableCommand());
        Add(new NameServersAddCommand(s_ipAddressesArgument));
        Add(new NameServersRemoveCommand(s_ipAddressesArgument));
        Add(new NameServersSetCommand(s_ipAddressesArgument));
        Add(new NameServersClearCommand());
        Add(new NameServersModifyCommand(s_oldNameServerOption, s_newNameServerOption));
    }
}

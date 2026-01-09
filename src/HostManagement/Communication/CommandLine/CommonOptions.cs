using System.CommandLine;
using System.Net;
using HostManagement.Communication.CommandLine.Extensions;
using HostManagement.Communication.CommandLine.Validators;

namespace HostManagement.Communication.CommandLine;

/// <summary>
/// Provides common options for the command line interface.
/// </summary>
internal static class CommonOptions
{
    internal static readonly Option<string> s_interfaceNameOption = new(["-n", "--interface-name"])
    {
        Description = "The name of the network interface.",
        IsRequired = true,
        Arity = ArgumentArity.ExactlyOne
    };

    internal static readonly Option<IPAddress> s_oldIpAddressOption = new Option<IPAddress>(["-o", "--old-ip-address"])
    {
        Description = "The old IP address.",
        IsRequired = true,
        Arity = ArgumentArity.ExactlyOne
    }
    .WithValidation(optionResult => IPAddressValidator.Validate(optionResult, "Old IP address"));

    internal static readonly Option<IPAddress> s_ipAddressOption = new Option<IPAddress>(["-a", "--ip-address"])
    {
        Description = "The IP address.",
        IsRequired = true,
        Arity = ArgumentArity.ExactlyOne
    }
    .WithValidation(optionResult => IPAddressValidator.Validate(optionResult, "IP address"));

    internal static readonly Option<IPAddress> s_netmaskOption = new Option<IPAddress>(["-m", "--netmask"])
    {
        Description = "The subnet mask of the specified IP address.",
        IsRequired = true,
        Arity = ArgumentArity.ExactlyOne
    }
    .WithValidation(optionResult => IPAddressValidator.Validate(optionResult, "Subnet mask"));

    internal static readonly Option<string[]> s_itemsOption = new(["-i", "--items"])
    {
        Description = "The list of items.",
        IsRequired = true,
        Arity = ArgumentArity.OneOrMore,
        AllowMultipleArgumentsPerToken = true
    };

    internal static readonly Option<IPAddress[]> s_ipAddressItemsOption = new Option<IPAddress[]>(["-i", "--items"])
    {
        Description = "The list of items.",
        IsRequired = true,
        Arity = ArgumentArity.OneOrMore,
        AllowMultipleArgumentsPerToken = true
    }
    .WithValidation(optionResult => IPAddressValidator.Validate(optionResult, "IP address"));
}

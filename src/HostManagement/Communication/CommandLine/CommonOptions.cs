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
    internal static readonly Option<string> s_interfaceNameOption = new("--interface-name", ["-n"])
    {
        Description = "The name of the network interface.",
        Required = true,
        Arity = ArgumentArity.ExactlyOne
    };

    internal static readonly Option<IPAddress> s_oldIpAddressOption = new Option<IPAddress>("--old-ip-address", ["-o"])
    {
        Description = "The old IP address.",
        Required = true,
        Arity = ArgumentArity.ExactlyOne,
        CustomParser = IPAddressParser.ParseSingle
    }
    .WithValidation(optionResult => IPAddressValidator.Validate(optionResult, "Old IP address"));

    internal static readonly Option<IPAddress> s_ipAddressOption = new Option<IPAddress>("--ip-address", ["-a"])
    {
        Description = "The IP address.",
        Required = true,
        Arity = ArgumentArity.ExactlyOne,
        CustomParser = IPAddressParser.ParseSingle
    }
    .WithValidation(optionResult => IPAddressValidator.Validate(optionResult, "IP address"));

    internal static readonly Option<IPAddress> s_netmaskOption = new Option<IPAddress>("--netmask", ["-m"])
    {
        Description = "The subnet mask of the specified IP address.",
        Required = true,
        Arity = ArgumentArity.ExactlyOne,
        CustomParser = IPAddressParser.ParseSingle
    }
    .WithValidation(optionResult => IPAddressValidator.Validate(optionResult, "Subnet mask"));

    internal static readonly Option<string[]> s_itemsOption = new("--items", ["-i"])
    {
        Description = "The list of items.",
        Required = true,
        Arity = ArgumentArity.OneOrMore,
        AllowMultipleArgumentsPerToken = true
    };

    internal static readonly Option<IPAddress[]> s_ipAddressItemsOption = new Option<IPAddress[]>("--items", ["-i"])
    {
        Description = "The list of items.",
        Required = true,
        Arity = ArgumentArity.OneOrMore,
        AllowMultipleArgumentsPerToken = true,
        CustomParser = IPAddressParser.ParseMultiple
    }
    .WithValidation(optionResult => IPAddressValidator.Validate(optionResult, "IP address"));
}

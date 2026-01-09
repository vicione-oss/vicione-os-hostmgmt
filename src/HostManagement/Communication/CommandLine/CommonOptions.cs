using System.CommandLine;
using System.Net;

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

    internal static readonly Option<IPAddress> s_oldIpAddressOption = new(["-o", "--old-ip-address"])
    {
        Description = "The old IP address.",
        IsRequired = true,
        Arity = ArgumentArity.ExactlyOne
    };

    internal static readonly Option<IPAddress> s_ipAddressOption = new(["-a", "--ip-address"])
    {
        Description = "The IP address.",
        IsRequired = true,
        Arity = ArgumentArity.ExactlyOne
    };

    internal static readonly Option<IPAddress> s_netmaskOption = new(["-m", "--netmask"])
    {
        Description = "The subnet mask of the specified IP address.",
        IsRequired = true,
        Arity = ArgumentArity.ExactlyOne
    };

    internal static readonly Option<string[]> s_itemsOption = new(["-i", "--items"])
    {
        Description = "The list of items.",
        IsRequired = true,
        Arity = ArgumentArity.OneOrMore,
        AllowMultipleArgumentsPerToken = true
    };

    internal static readonly Option<IPAddress[]> s_ipAddressItemsOption = new(["-i", "--items"])
    {
        Description = "The list of items.",
        IsRequired = true,
        Arity = ArgumentArity.OneOrMore,
        AllowMultipleArgumentsPerToken = true
    };
}

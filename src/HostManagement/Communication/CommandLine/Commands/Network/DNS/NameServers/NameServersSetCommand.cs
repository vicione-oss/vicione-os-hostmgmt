using System.CommandLine;
using System.Net;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS.NameServers;

/// <summary>
/// Represents the DNS name servers set command.
/// </summary>
internal sealed class NameServersSetCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="NameServersSetCommand"/>.
    /// </summary>
    /// <param name="ipAddressesArgument">The IP addresses argument.</param>
    public NameServersSetCommand(Argument<IPAddress[]> ipAddressesArgument)
        : base(CliStrings.Common.SetCommandName, CliStrings.Network.DNS.NameServers.SetCommandDescription)
    {
        Add(ipAddressesArgument);

        SetAction((parseResult, cancellationToken)
            => SetHandler(parseResult.GetRequiredValue(ipAddressesArgument), cancellationToken));
    }

    private static async Task SetHandler(IPAddress[] addresses, CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        config.NetworkDNSSettings.NameServers.Addresses.Clear();

        var output = new ListOutput();

        foreach (var address in addresses)
        {
            config.NetworkDNSSettings.NameServers.Addresses.Add(address);
            output.With(address);
        }

        if (!await SetConfiguration(config, cancellationToken))
            return;

        var title = output.ItemCount == 1
            ? CliStrings.Network.DNS.NameServers.Set
            : CliStrings.Network.DNS.NameServers.SetMultiple;

        output.WithTitle(title);

        if (!config.NetworkDNSSettings.NameServers.Enabled)
            output.WithWarning(CliStrings.Network.DNS.NameServers.CurrentlyDisabled);

        output.WriteLine();
    }
}

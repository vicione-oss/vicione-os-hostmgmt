using System.CommandLine;
using System.Net;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS.NameServers;

/// <summary>
/// Represents the DNS name servers remove command.
/// </summary>
internal sealed class NameServersRemoveCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="NameServersRemoveCommand"/>.
    /// </summary>
    /// <param name="ipAddressesArgument">The IP addresses argument.</param>
    public NameServersRemoveCommand(Argument<IPAddress[]> ipAddressesArgument) : base(
        CommandLineStrings_Common.RemoveCommandName,
        CommandLineStrings_DNS.DNSNameServersRemoveCommandDescription)
    {
        Add(ipAddressesArgument);

        SetAction((parseResult, cancellationToken) =>
        {
            return RemoveHandler(
                parseResult.GetRequiredValue(ipAddressesArgument),
                cancellationToken);
        });
    }

    private static async Task RemoveHandler(IPAddress[] addresses, CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        var hasRemovedAddress = false;
        var output = new ListOutput();

        foreach (var address in addresses)
        {
            if (!config.NetworkDNSSettings.NameServers.Addresses.Contains(address))
            {
                output.WithWarning(CommandLineStrings_DNS.NameServerNotFound, address);
                continue;
            }

            if (!config.NetworkDNSSettings.NameServers.Addresses.Remove(address))
            {
                output
                    .WithError(CommandLineStrings_DNS.NameServerCouldNotBeRemoved, address)
                    .WriteLine();

                return;
            }

            output.With(address);

            hasRemovedAddress = true;
        }

        if (hasRemovedAddress)
        {
            if (config.NetworkDNSSettings.NameServers.Enabled)
            {
                if (config.NetworkDNSSettings.NameServers.Addresses.Count == 0)
                {
                    config.NetworkDNSSettings.NameServers.Enabled = false;

                    output.WithInformation(CommandLineStrings_DNS.NameServersDisabled);
                }
            }
            else
            {
                if (config.NetworkDNSSettings.NameServers.Addresses.Count > 0)
                {
                    output.WithWarning(CommandLineStrings_DNS.NameServersCurrentlyDisabled);
                }
            }

            if (!await SetConfiguration(config, cancellationToken))
                return;

            var title = output.ItemCount == 1
                ? CommandLineStrings_DNS.NameServerRemoved
                : CommandLineStrings_DNS.NameServersRemoved;

            output.WithTitle(title);
        }

        output.WriteLine();
    }
}

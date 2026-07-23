using System.CommandLine;
using System.Net;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS.NameServers;

/// <summary>
/// Represents the DNS name servers add command.
/// </summary>
internal sealed class NameServersAddCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="NameServersAddCommand"/>.
    /// </summary>
    /// <param name="nameServersOption">The name servers option.</param>
    public NameServersAddCommand(Option<IPAddress[]> nameServersOption) : base(
        CommandLineStrings_Common.AddCommandName,
        CommandLineStrings_DNS.DNSNameServersAddCommandDescription)
    {
        Add(nameServersOption);

        SetAction((parseResult, cancellationToken) =>
        {
            return AddHandler(
                parseResult.GetRequiredValue(nameServersOption),
                cancellationToken);
        });
    }

    private static async Task AddHandler(IPAddress[] addresses, CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        var hasAddedAddress = false;
        var output = new ListOutput();

        foreach (var address in addresses)
        {
            if (config.NetworkDNSSettings.NameServers.Addresses.Contains(address))
            {
                output.WithInformation(CommandLineStrings_DNS.NameServerAlreadyDefined, address);
                continue;
            }

            config.NetworkDNSSettings.NameServers.Addresses.Add(address);
            output.With(address);

            hasAddedAddress = true;
        }

        if (hasAddedAddress)
        {
            if (!await SetConfiguration(config, cancellationToken))
                return;

            var title = output.ItemCount == 1
                ? CommandLineStrings_DNS.NameServerAdded
                : CommandLineStrings_DNS.NameServersAdded;

            output.WithTitle(title);

            if (!config.NetworkDNSSettings.NameServers.Enabled)
                output.WithWarning(CommandLineStrings_DNS.NameServersCurrentlyDisabled);
        }

        output.WriteLine();
    }
}

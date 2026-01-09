using System.Net;
using System.CommandLine;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS.NameServers;

/// <summary>
/// Represents the DNS name servers add command.
/// </summary>
internal sealed class DNSNameServersAddCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="DNSNameServersAddCommand"/>.
    /// </summary>
    /// <param name="nameServersOption">The name servers option.</param>
    public DNSNameServersAddCommand(Option<IPAddress[]> nameServersOption) : base(
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

    private static async Task AddHandler(IPAddress[] nameServers, CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        var hasAddedNameServer = false;
        var output = new ListOutput();

        foreach (var nameServer in nameServers)
        {
            if (config.NetworkDNSSettings.NameServers.Contains(nameServer))
            {
                output.WithInformation(CommandLineStrings_DNS.NameServerAlreadyDefined, nameServer);
                continue;
            }

            config.NetworkDNSSettings.NameServers.Add(nameServer);
            output.With(nameServer);

            hasAddedNameServer = true;
        }

        if (hasAddedNameServer)
        {
            if (!await SetConfiguration(config, cancellationToken))
                return;

            var title = output.ItemCount == 1
                ? CommandLineStrings_DNS.NameServerAdded
                : CommandLineStrings_DNS.NameServersAdded;

            output.WithTitle(title);

            if (!config.NetworkDNSSettings.NameServersEnabled)
                output.WithWarning(CommandLineStrings_DNS.NameServersCurrentlyDisabled);
        }

        output.WriteLine();
    }
}

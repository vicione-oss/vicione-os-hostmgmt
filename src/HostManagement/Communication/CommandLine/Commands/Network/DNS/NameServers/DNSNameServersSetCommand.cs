using System.CommandLine;
using System.Net;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS.NameServers;

/// <summary>
/// Represents the DNS name servers set command.
/// </summary>
internal sealed class DNSNameServersSetCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="DNSNameServersSetCommand"/>.
    /// </summary>
    /// <param name="nameServersOption">The name servers option.</param>
    public DNSNameServersSetCommand(Option<IPAddress[]> nameServersOption) : base(
        CommandLineStrings_Common.SetCommandName,
        CommandLineStrings_DNS.DNSNameServersSetCommandDescription)
    {
        Add(nameServersOption);

        SetAction((parseResult, cancellationToken) =>
        {
            return SetHandler(
                parseResult.GetRequiredValue(nameServersOption),
                cancellationToken);
        });
    }

    private static async Task SetHandler(IPAddress[] nameServers, CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        config.NetworkDNSSettings.NameServers.Clear();

        var output = new ListOutput();

        foreach (var nameServer in nameServers)
        {
            config.NetworkDNSSettings.NameServers.Add(nameServer);
            output.With(nameServer);
        }

        if (!await SetConfiguration(config, cancellationToken))
            return;

        var title = output.ItemCount == 1
            ? CommandLineStrings_DNS.NameServerSet
            : CommandLineStrings_DNS.NameServersSet;

        output.WithTitle(title);

        if (!config.NetworkDNSSettings.NameServersEnabled)
            output.WithWarning(CommandLineStrings_DNS.NameServersCurrentlyDisabled);

        output.WriteLine();
    }
}

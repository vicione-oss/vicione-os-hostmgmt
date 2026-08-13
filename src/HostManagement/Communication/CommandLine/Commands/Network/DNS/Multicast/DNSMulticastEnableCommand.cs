using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS.Multicast;

/// <summary>
/// Represents the DNS multicast enable command.
/// </summary>
internal sealed class DNSMulticastEnableCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="DNSMulticastEnableCommand"/>.
    /// </summary>
    public DNSMulticastEnableCommand()
        : base(
            CliStrings.Common.EnableCommandName,
            CommandLineStrings_DNS.DNSMulticastEnableCommandDescription)
        => SetAction((parseResult, cancellationToken) => EnableHandler(cancellationToken));

    private static async Task EnableHandler(CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (config.NetworkDNSSettings.MulticastDNSEnabled)
        {
            SimpleOutput.WriteLineInformation(CommandLineStrings_DNS.MulticastDNSAlreadyEnabled);
            return;
        }

        config.NetworkDNSSettings.MulticastDNSEnabled = true;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        SimpleOutput.WriteLine(CommandLineStrings_DNS.MulticastDNSEnabled);
    }
}

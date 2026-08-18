using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS.Multicast;

/// <summary>
/// Represents the DNS multicast disable command.
/// </summary>
internal sealed class DNSMulticastDisableCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="DNSMulticastDisableCommand"/>.
    /// </summary>
    public DNSMulticastDisableCommand()
        : base(
            CommandLineStrings_Common.DisableCommandName,
            CommandLineStrings_DNS.DNSMulticastDisableCommandDescription)
        => SetAction((parseResult, cancellationToken) => DisableHandler(cancellationToken));

    private static async Task DisableHandler(CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (!config.NetworkDNSSettings.MulticastDNSEnabled)
        {
            SimpleOutput.WriteLineInformation(CommandLineStrings_DNS.MulticastDNSAlreadyDisabled);
            return;
        }

        config.NetworkDNSSettings.MulticastDNSEnabled = false;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        SimpleOutput.WriteLine(CommandLineStrings_DNS.MulticastDNSDisabled);
    }
}

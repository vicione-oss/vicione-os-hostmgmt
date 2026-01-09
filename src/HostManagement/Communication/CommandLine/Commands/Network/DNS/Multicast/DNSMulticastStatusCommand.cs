using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS.Multicast;

/// <summary>
/// Represents the DNS multicast status command.
/// </summary>
internal sealed class DNSMulticastStatusCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="DNSMulticastStatusCommand"/>.
    /// </summary>
    public DNSMulticastStatusCommand() : base(
        CommandLineStrings_Common.StatusCommandName,
        CommandLineStrings_DNS.DNSMulticastStatusCommandDescription)
    {
        SetAction((parseResult, cancellationToken) => StatusHandler(cancellationToken));
    }

    private static async Task StatusHandler(CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        var enableMessage = config.NetworkDNSSettings.MulticastDNSEnabled
            ? CommandLineStrings_DNS.MulticastDNSIsEnabled
            : CommandLineStrings_DNS.MulticastDNSIsDisabled;

        SimpleOutput.WriteLine(enableMessage);
    }
}

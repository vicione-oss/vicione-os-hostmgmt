using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
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
    public DNSMulticastStatusCommand()
        : base(CliStrings.Common.StatusCommandName, CliStrings.Network.DNS.Multicast.StatusCommandDescription)
        => SetAction((parseResult, cancellationToken) => StatusHandler(cancellationToken));

    private static async Task StatusHandler(CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        var enableMessage = config.NetworkDNSSettings.MulticastDNSEnabled
            ? CliStrings.Network.DNS.Multicast.IsEnabled
            : CliStrings.Network.DNS.Multicast.IsDisabled;

        SimpleOutput.WriteLine(enableMessage);
    }
}

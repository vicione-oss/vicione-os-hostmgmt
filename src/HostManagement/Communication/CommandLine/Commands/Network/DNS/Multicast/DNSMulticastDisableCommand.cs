using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
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
        : base(CliStrings.Common.DisableCommandName, CliStrings.Network.DNS.Multicast.DisableCommandDescription)
        => SetAction((parseResult, cancellationToken) => DisableHandler(cancellationToken));

    private static async Task DisableHandler(CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (!config.NetworkDNSSettings.MulticastDNSEnabled)
        {
            SimpleOutput.WriteLineInformation(CliStrings.Network.DNS.Multicast.AlreadyDisabled);
            return;
        }

        config.NetworkDNSSettings.MulticastDNSEnabled = false;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        SimpleOutput.WriteLine(CliStrings.Network.DNS.Multicast.Disabled);
    }
}

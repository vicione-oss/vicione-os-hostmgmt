using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS.NameServers;

/// <summary>
/// Represents the DNS name servers clear command.
/// </summary>
internal sealed class DNSNameServersClearCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="DNSNameServersClearCommand"/>.
    /// </summary>
    public DNSNameServersClearCommand() : base(
        CommandLineStrings_Common.ClearCommandName,
        CommandLineStrings_DNS.DNSNameServersClearCommandDescription)
    {
        SetAction((parseResult, cancellationToken) => ClearHandler(cancellationToken));
    }

    private static async Task ClearHandler(CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (config.NetworkDNSSettings.NameServers.Count == 0)
        {
            SimpleOutput.WriteLineInformation(CommandLineStrings_DNS.NameServersNotConfigured);
            return;
        }

        config.NetworkDNSSettings.NameServers.Clear();
        config.NetworkDNSSettings.NameServersEnabled = false;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        SimpleOutput.WriteLineInformation(CommandLineStrings_DNS.NameServersDisabled);
        SimpleOutput.WriteLine(CommandLineStrings_DNS.NameServersCleared);
    }
}

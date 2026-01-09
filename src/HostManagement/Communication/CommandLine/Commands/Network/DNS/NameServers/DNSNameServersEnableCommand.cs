using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS.NameServers;

/// <summary>
/// Represents the DNS name servers enable command.
/// </summary>
internal sealed class DNSNameServersEnableCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="DNSNameServersEnableCommand"/>.
    /// </summary>
    public DNSNameServersEnableCommand() : base(
        CommandLineStrings_Common.EnableCommandName,
        CommandLineStrings_DNS.DNSNameServersEnableCommandDescription)
    {
        SetAction((parseResult, cancellationToken) => EnableHandler(cancellationToken));
    }

    private static async Task EnableHandler(CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (config.NetworkDNSSettings.NameServersEnabled)
        {
            SimpleOutput.WriteLineInformation(CommandLineStrings_DNS.NameServersAlreadyEnabled);
            return;
        }

        if (config.NetworkDNSSettings.NameServers.Count == 0)
        {
            SimpleOutput.WriteLineError(CommandLineStrings_DNS.NameServersNotConfigured);
            return;
        }

        config.NetworkDNSSettings.NameServersEnabled = true;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        SimpleOutput.WriteLine(CommandLineStrings_DNS.NameServersEnabled);
    }
}

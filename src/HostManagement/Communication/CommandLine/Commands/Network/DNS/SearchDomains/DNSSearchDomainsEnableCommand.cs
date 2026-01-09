using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS.SearchDomains;

/// <summary>
/// Represents the DNS search domains enable command.
/// </summary>
internal sealed class DNSSearchDomainsEnableCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="DNSSearchDomainsEnableCommand"/>.
    /// </summary>
    public DNSSearchDomainsEnableCommand() : base(
        CommandLineStrings_Common.EnableCommandName,
        CommandLineStrings_DNS.DNSSearchDomainsEnableCommandDescription)
    {
        SetAction((parseResult, cancellationToken) => EnableHandler(cancellationToken));
    }

    private static async Task EnableHandler(CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (config.NetworkDNSSettings.SearchDomainsEnabled)
        {
            SimpleOutput.WriteLineInformation(CommandLineStrings_DNS.SearchDomainsAlreadyEnabled);
            return;
        }

        if (config.NetworkDNSSettings.SearchDomains.Count == 0)
        {
            SimpleOutput.WriteLineError(CommandLineStrings_DNS.SearchDomainsNotConfigured);
            return;
        }

        config.NetworkDNSSettings.SearchDomainsEnabled = true;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        SimpleOutput.WriteLine(CommandLineStrings_DNS.SearchDomainsEnabled);
    }
}

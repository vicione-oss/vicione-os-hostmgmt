using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS.SearchDomains;

/// <summary>
/// Represents the DNS search domains disable command.
/// </summary>
internal sealed class DNSSearchDomainsDisableCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="DNSSearchDomainsDisableCommand"/>.
    /// </summary>
    public DNSSearchDomainsDisableCommand() : base(
        CommandLineStrings_Common.DisableCommandName,
        CommandLineStrings_DNS.DNSSearchDomainsDisableCommandDescription)
    {
        SetAction((parseResult, cancellationToken) => DisableHandler(cancellationToken));
    }

    private static async Task DisableHandler(CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (!config.NetworkDNSSettings.SearchDomainsEnabled)
        {
            SimpleOutput.WriteLineInformation(CommandLineStrings_DNS.SearchDomainsAlreadyDisabled);
            return;
        }

        config.NetworkDNSSettings.SearchDomainsEnabled = false;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        SimpleOutput.WriteLine(CommandLineStrings_DNS.SearchDomainsDisabled);
    }
}

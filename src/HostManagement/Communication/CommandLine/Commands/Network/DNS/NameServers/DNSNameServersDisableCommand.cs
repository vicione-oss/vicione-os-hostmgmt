using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS.NameServers;

/// <summary>
/// Represents the DNS name servers disable command.
/// </summary>
internal sealed class DNSNameServersDisableCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="DNSNameServersDisableCommand"/>.
    /// </summary>
    public DNSNameServersDisableCommand() : base(
        CommandLineStrings_Common.DisableCommandName,
        CommandLineStrings_DNS.DNSNameServersDisableCommandDescription)
    {
        SetAction((parseResult, cancellationToken) => DisableHandler(cancellationToken));
    }

    private static async Task DisableHandler(CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (!config.NetworkDNSSettings.NameServersEnabled)
        {
            SimpleOutput.WriteLineInformation(CommandLineStrings_DNS.NameServersAlreadyDisabled);
            return;
        }

        config.NetworkDNSSettings.NameServersEnabled = false;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        SimpleOutput.WriteLine(CommandLineStrings_DNS.NameServersDisabled);
    }
}

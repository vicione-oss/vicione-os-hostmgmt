using System.CommandLine;
using System.Net;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands.Network.DNS.NameServers;

/// <summary>
/// Represents the DNS name servers modify command.
/// </summary>
internal sealed class NameServersModifyCommand : ConfigCommandBase
{
    /// <summary>
    /// Initializes a new <see cref="NameServersModifyCommand"/>.
    /// </summary>
    /// <param name="oldNameServerOption">The old name server option.</param>
    /// <param name="newNameServerOption">The new name server option.</param>
    public NameServersModifyCommand(
        Option<IPAddress> oldNameServerOption,
        Option<IPAddress> newNameServerOption)
        : base(
            CliStrings.Common.ModifyCommandName,
            CommandLineStrings_DNS.DNSNameServersModifyCommandDescription)
    {
        Add(oldNameServerOption);
        Add(newNameServerOption);

        SetAction((parseResult, cancellationToken)
            => ModifyHandler(
                parseResult.GetRequiredValue(oldNameServerOption),
                parseResult.GetRequiredValue(newNameServerOption),
                cancellationToken));
    }

    private static async Task ModifyHandler(
        IPAddress oldAddress,
        IPAddress newAddress,
        CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        var addresses = config.NetworkDNSSettings.NameServers.Addresses;

        if (addresses.Contains(newAddress))
        {
            SimpleOutput.WriteLineInformation(CommandLineStrings_DNS.NameServerAlreadyDefined, newAddress);
            return;
        }

        var index = addresses.IndexOf(oldAddress);

        if (index == -1)
        {
            SimpleOutput.WriteLineError(CommandLineStrings_DNS.NameServerNotFound, oldAddress);
            return;
        }

        addresses[index] = newAddress;

        if (!await SetConfiguration(config, cancellationToken))
            return;

        if (!config.NetworkDNSSettings.NameServers.Enabled)
            SimpleOutput.WriteLineWarning(CommandLineStrings_DNS.NameServersCurrentlyDisabled);

        SimpleOutput.WriteLine(CommandLineStrings_DNS.NameServerModified, oldAddress, newAddress);
    }
}

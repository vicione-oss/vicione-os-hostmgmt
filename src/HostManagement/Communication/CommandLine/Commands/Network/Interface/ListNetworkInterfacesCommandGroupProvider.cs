using System.CommandLine;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;

namespace HostManagement.Communication.CommandLine.Commands.Network.Interface;

/// <summary>
/// Represents the <see cref="ListNetworkInterfacesCommandGroupProvider"/>.
/// </summary>
internal static class ListNetworkInterfacesCommandGroupProvider
{
    private const int IPv4FixedOutputWidth = 15;

    private static readonly Option<bool> s_showNamesOnlyOption = new(["-n", "--name"])
    {
        Description = "List all network interface names.",
        Arity = ArgumentArity.ZeroOrOne
    };

    private static readonly Command s_listInterfacesCommand = CommandHelper.ConstructCommand(
        "list-interfaces",
        "List all network interfaces with their configuration.",
        s_showNamesOnlyOption,
        Handler);

    /// <summary>
    /// Adds the command group to list network interfaces.
    /// </summary>
    /// <param name="parentCommand">The parent command.</param>
    /// <exception cref="InvalidOperationException"></exception>
    internal static void AddListNetworkInterfacesCommandGroup(this Command parentCommand)
        => parentCommand.Add(s_listInterfacesCommand);

    private static async Task Handler(bool showNamesOnly)
    {
        if (await NamedPipeRequestHelper.LoadConfiguration() is not SystemConfiguration config)
            return;

        foreach (var networkInterface in config.NetworkInterfacesSettings.NetworkInterfaces)
        {
            var builder = new CommandLineStringBuilder();

            builder.AppendLine(networkInterface.CommonInformation.Name);

            if (showNamesOnly)
            {
                Console.Write(builder.ToString());
                continue;
            }

            AppendOutputForCommonInformation(builder, 1, networkInterface.CommonInformation);
            AppendOutputForIPv4(builder, 1, networkInterface.IPv4, config.NetworkDNSSettings);

            Console.WriteLine(builder.Output);
        }
    }

    private static void AppendOutputForCommonInformation(
        CommandLineStringBuilder builder,
        int indentationLevel,
        NetworkInterfaceCommonInformation commonInformation)
    {
        builder.AppendLine(indentationLevel, "Common Information");
        builder.AppendLine(indentationLevel + 1, $"Enabled: {commonInformation.Enabled}");
        builder.AppendLine(indentationLevel + 1, $"Physical address: {commonInformation.PhysicalAddress}");
    }

    private static void AppendOutputForIPv4(
        CommandLineStringBuilder builder,
        int indentationLevel,
        IPv4Settings ipv4Settings,
        NetworkDNSSettings networkDNSSettings)
    {
        builder.AppendLine(indentationLevel, "IPv4");
        builder.AppendLine(indentationLevel + 1, $"DHCP: {ipv4Settings.DHCPEnabled}");

        if (ipv4Settings.DHCPEnabled)
        {
            AppendOutputForDHCPLease(builder, indentationLevel + 1, ipv4Settings.DHCPLease);
        }
        else
        {
            builder.AppendLine(
                indentationLevel + 1,
                $"Gateway: {ipv4Settings.Gateway?.ToString() ?? CommandLineStrings.NotSet}");

            if (ipv4Settings.IPv4Details.Count > 0)
            {
                builder.AppendLine(indentationLevel + 1, "IP addresses");

                foreach (var ipv4Detail in ipv4Settings.IPv4Details)
                {
                    builder.Append(
                        indentationLevel + 2,
                        $"IP address: {ipv4Detail.IPAddress,-IPv4FixedOutputWidth}");

                    builder.AppendLine(1, $"Subnet mask: {ipv4Detail.Netmask}");
                }
            }
        }

        builder.AppendLine(indentationLevel + 1, "DNS");

        AppendOutputForDNSSuffix(builder, indentationLevel + 2, networkDNSSettings);
        AppendOutputForSearchDomains(builder, indentationLevel + 2, networkDNSSettings);
        AppendOutputForNameServers(builder, indentationLevel + 2, networkDNSSettings);
    }

    private static void AppendOutputForDHCPLease(
        CommandLineStringBuilder builder,
        int indentationLevel,
        DHCPLease? lease)
    {
        builder.AppendLine(indentationLevel, $"DHCP Lease");

        if (lease is null)
        {
            builder.AppendLine(indentationLevel + 1, "No DHCP lease information available.");
            return;
        }

        var leaseObtained = lease.LeaseObtained?.ToString() ?? CommandLineStrings.NotSet;
        var leaseExpires = lease.LeaseExpires?.ToString() ?? CommandLineStrings.NotSet;
        var gateway = lease.Gateway?.ToString() ?? CommandLineStrings.NotSet;
        var ip = lease.IPv4Detail?.IPAddress.ToString() ?? CommandLineStrings.NotSet;
        var netmask = lease.IPv4Detail?.Netmask.ToString() ?? CommandLineStrings.NotSet;

        builder.AppendLine(indentationLevel + 1, $"Lease obtained: {leaseObtained}");
        builder.AppendLine(indentationLevel + 1, $"Lease expires: {leaseExpires}");
        builder.AppendLine(indentationLevel + 1, $"Gateway: {gateway}");
        builder.AppendLine(indentationLevel + 1, $"IP address: {ip,-IPv4FixedOutputWidth}  Subnet mask: {netmask}");

        builder.AppendLine(indentationLevel + 1, $"DNS");

        if (lease.NetworkDNSSettings is null)
        {
            builder.AppendLine(indentationLevel + 2, "No DNS lease information available.");
            return;
        }

        AppendOutputForDNSSuffix(builder, indentationLevel + 2, lease.NetworkDNSSettings);
        AppendOutputForSearchDomains(builder, indentationLevel + 2, lease.NetworkDNSSettings);
        AppendOutputForNameServers(builder, indentationLevel + 2, lease.NetworkDNSSettings);
    }

    private static void AppendOutputForDNSSuffix(
        CommandLineStringBuilder builder,
        int indentationLevel,
        NetworkDNSSettings networkDNSSettings)
    {
        var dnsSuffixEnabledString = networkDNSSettings.DNSSuffixEnabled
            ? CommandLineStrings.enabled
            : CommandLineStrings.disabled;

        var dnsSuffix = string.IsNullOrWhiteSpace(networkDNSSettings.DNSSuffix)
            ? CommandLineStrings.NotSet
            : networkDNSSettings.DNSSuffix;

        builder.AppendLine(indentationLevel, $"Primary DNS Suffix ({dnsSuffixEnabledString})");
        builder.AppendLine(indentationLevel + 1, dnsSuffix);
    }

    private static void AppendOutputForSearchDomains(
        CommandLineStringBuilder builder,
        int indentationLevel,
        NetworkDNSSettings networkDNSSettings)
    {
        var searchDomainsEnabledString = networkDNSSettings.SearchDomainsEnabled
            ? CommandLineStrings.enabled
            : CommandLineStrings.disabled;

        builder.AppendLine(indentationLevel, $"Search Domains ({searchDomainsEnabledString})");

        if (networkDNSSettings.SearchDomains.Count == 0)
        {
            builder.AppendLine(indentationLevel + 1, CommandLineStrings.NotSet);
            return;
        }

        foreach (var searchDomain in networkDNSSettings.SearchDomains)
            builder.AppendLine(indentationLevel + 1, searchDomain);
    }

    private static void AppendOutputForNameServers(
        CommandLineStringBuilder builder,
        int indentationLevel,
        NetworkDNSSettings networkDNSSettings)
    {
        var nameServersEnabledString = networkDNSSettings.NameServersEnabled
            ? CommandLineStrings.enabled
            : CommandLineStrings.disabled;

        builder.AppendLine(indentationLevel, $"Name Servers ({nameServersEnabledString})");

        if (networkDNSSettings.NameServers.Count == 0)
        {
            builder.AppendLine(indentationLevel + 1, CommandLineStrings.NotSet);
            return;
        }

        foreach (var nameServer in networkDNSSettings.NameServers)
            builder.AppendLine(indentationLevel + 1, nameServer.ToString());
    }
}

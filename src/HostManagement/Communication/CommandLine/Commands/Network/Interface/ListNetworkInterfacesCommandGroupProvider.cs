using System.CommandLine;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;

namespace HostManagement.Communication.CommandLine.Commands.Network.Interface;

/// <summary>
/// Represents the <see cref="ListNetworkInterfacesCommandGroupProvider"/>.
/// </summary>
internal static class ListNetworkInterfacesCommandGroupProvider
{
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
        if (await NamedPipeRequestHelper.GetConfiguration() is not SystemConfiguration config)
            return;

        if (config.NetworkInterfacesSettings.NetworkInterfaces.Count == 0)
        {
            SimpleOutput.WriteLine(CommandLineStrings.NetworkInterfacesNotFound);

            return;
        }

        var output = new ListOutput();

        foreach (var networkInterface in config.NetworkInterfacesSettings.NetworkInterfaces)
        {
            var listOutput = output
                .AddListOutput()
                .WithTitle(networkInterface.CommonInformation.Name);

            if (showNamesOnly)
                continue;

            AppendOutputForCommonInformation(listOutput, 1, networkInterface.CommonInformation);
            AppendOutputForIPv4(listOutput, 1, networkInterface.IPv4, config.NetworkDNSSettings);
        }

        output.WriteLine();
    }

    private static void AppendOutputForCommonInformation(
        CliComplexOutputBase output,
        int indentationLevel,
        NetworkInterfaceCommonInformation commonInformation)
    {
        output
            .AddKeyValueOutput(indentationLevel)
                .WithTitle(CommandLineStrings.CommonInfo)
                .With(new(CommandLineStrings.Enabled, commonInformation.Enabled))
                .With(new(CommandLineStrings.PhysicalAddress, commonInformation.PhysicalAddress));
    }

    private static void AppendOutputForIPv4(
        CliComplexOutputBase output,
        int indentationLevel,
        IPv4Settings ipv4Settings,
        NetworkDNSSettings networkDNSSettings)
    {
        var ipv4Output = output.AddKeyValueOutput(indentationLevel)
            .WithTitle(CommandLineStrings.IPv4)
            .WithSingleItemOnNewLine()
            .With(new(CommandLineStrings.DHCP, ipv4Settings.DHCPEnabled));

        if (ipv4Settings.DHCPEnabled)
        {
            AppendOutputForDHCPLease(ipv4Output, indentationLevel + 1, ipv4Settings.DHCPLease);
        }
        else
        {
            ipv4Output
                .AddKeyValueOutput(indentationLevel + 1)
                .With(new(CommandLineStrings.Gateway, ipv4Settings.Gateway));

            if (ipv4Settings.IPv4Details.Count > 0)
            {
                var ipv4DetailsOutput = ipv4Output
                    .AddMultiKeyValueOutput(indentationLevel + 1)
                    .WithTitle(CommandLineStrings.IPAddresses)
                    .WithSingleItemOnNewLine();

                foreach (var ipv4Detail in ipv4Settings.IPv4Details)
                {
                    ipv4DetailsOutput.With(
                        new(CommandLineStrings.IPAddress, ipv4Detail.IPAddress),
                        new(CommandLineStrings.SubnetMask, ipv4Detail.Netmask));
                }
            }
        }

        var dnsOutput = output
            .AddListOutput(indentationLevel + 1)
            .WithTitle(CommandLineStrings.DNS);

        AppendOutputForDNSSuffix(dnsOutput, indentationLevel + 2, networkDNSSettings);
        AppendOutputForSearchDomains(dnsOutput, indentationLevel + 2, networkDNSSettings);
        AppendOutputForNameServers(dnsOutput, indentationLevel + 2, networkDNSSettings);
    }

    private static void AppendOutputForDHCPLease(
        CliComplexOutputBase output,
        int indentationLevel,
        DHCPLease? lease)
    {
        if (lease is null)
        {
            output
                .AddListOutput(indentationLevel)
                .WithTitle(CommandLineStrings.DHCPLease)
                .With(CommandLineStrings.DHCPLeaseNotAvailable);

            return;
        }

        output
            .AddKeyValueOutput(indentationLevel)
            .WithTitle(CommandLineStrings.DHCPLease)
            .With(new(CommandLineStrings.DHCPLeaseObtained, lease.LeaseObtained))
            .With(new(CommandLineStrings.DHCPLeaseExpires, lease.LeaseExpires))
            .With(new(CommandLineStrings.Gateway, lease.Gateway))
            .With(new(CommandLineStrings.IPAddress, lease.IPv4Detail?.IPAddress))
            .With(new(CommandLineStrings.SubnetMask, lease.IPv4Detail?.Netmask));

        var dnsOutput = output
            .AddListOutput(indentationLevel + 1)
            .WithTitle(CommandLineStrings.DNS);

        if (lease.NetworkDNSSettings is null)
        {
            dnsOutput.With(CommandLineStrings.DHCPLeaseNotAvailable);
        }
        else
        {
            AppendOutputForDNSSuffix(dnsOutput, indentationLevel + 2, lease.NetworkDNSSettings);
            AppendOutputForSearchDomains(dnsOutput, indentationLevel + 2, lease.NetworkDNSSettings);
            AppendOutputForNameServers(dnsOutput, indentationLevel + 2, lease.NetworkDNSSettings);
        }
    }

    private static void AppendOutputForDNSSuffix(
        CliComplexOutputBase output,
        int indentationLevel,
        NetworkDNSSettings networkDNSSettings)
    {
        var dnsSuffixOutput = output
            .AddListOutput(indentationLevel)
            .WithTitle(CommandLineStrings.DNSSuffix, networkDNSSettings.DNSSuffixEnabled);

        if (string.IsNullOrWhiteSpace(networkDNSSettings.DNSSuffix))
            dnsSuffixOutput.With(CliOutputBase.NotSetValue);
        else
            dnsSuffixOutput.With(networkDNSSettings.DNSSuffix);
    }

    private static void AppendOutputForSearchDomains(
        CliComplexOutputBase output,
        int indentationLevel,
        NetworkDNSSettings networkDNSSettings)
    {
        var searchDomainsOutput = output
            .AddListOutput(indentationLevel)
            .WithTitle(CommandLineStrings.DNSSearchDomains, networkDNSSettings.SearchDomainsEnabled);

        if (networkDNSSettings.SearchDomains.Count == 0)
            searchDomainsOutput.With(CliOutputBase.NotSetValue);
        else
            networkDNSSettings.SearchDomains.ForEach(domain => searchDomainsOutput.With(domain));
    }

    private static void AppendOutputForNameServers(
        CliComplexOutputBase output,
        int indentationLevel,
        NetworkDNSSettings networkDNSSettings)
    {
        var nameServersOutput = output
            .AddListOutput(indentationLevel)
            .WithTitle(CommandLineStrings.DNSNameServers, networkDNSSettings.NameServersEnabled);

        if (networkDNSSettings.NameServers.Count == 0)
            nameServersOutput.With(CliOutputBase.NotSetValue);
        else
            networkDNSSettings.NameServers.ForEach(server => nameServersOutput.With(server));
    }
}

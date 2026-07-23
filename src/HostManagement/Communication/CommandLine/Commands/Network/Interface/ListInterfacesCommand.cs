using System.CommandLine;
using HostManagement.Communication.CommandLine.Commands.Network.DNS;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;

namespace HostManagement.Communication.CommandLine.Commands.Network.Interface;

/// <summary>
/// Represents the list interfaces command.
/// </summary>
internal sealed class ListInterfacesCommand : ConfigCommandBase
{
    private static readonly Option<bool> s_showNamesOnlyOption = new("--name", "-n")
    {
        Description = "List all network interface names.",
        Arity = ArgumentArity.ZeroOrOne
    };

    /// <summary>
    /// Initializes a new <see cref="ListInterfacesCommand"/>.
    /// </summary>
    public ListInterfacesCommand() : base(
        CommandLineStrings_NetworkInterfaces.ListInterfacesCommandName,
        CommandLineStrings_NetworkInterfaces.ListInterfacesCommandDescription)
    {
        Add(s_showNamesOnlyOption);

        SetAction((parseResult, cancellationToken) =>
        {
            return ListHandler(
                parseResult.GetValue(s_showNamesOnlyOption),
                cancellationToken);
        });
    }

    private static async Task ListHandler(bool showNamesOnly, CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (config.NetworkInterfacesSettings.NetworkInterfaces.Count == 0)
        {
            SimpleOutput.WriteLine(CommandLineStrings_NetworkInterfaces.InterfacesNotFound);
            return;
        }

        var output = new ListOutput();
        output.WithTitle(CommandLineStrings_NetworkInterfaces.Interfaces);

        foreach (var networkInterface in config.NetworkInterfacesSettings.NetworkInterfaces)
        {
            var listOutput = output
                .AddListOutput(1)
                .WithTitle(networkInterface.CommonInformation.Name, useTitleDelimiter: !showNamesOnly);

            if (showNamesOnly)
                continue;

            await AppendOutputForCommonInformation(listOutput, 2, networkInterface.CommonInformation);

            DHCPLease? dhcpLease = null;

            if (networkInterface.IPv4.DHCPEnabled)
            {
                dhcpLease = await NamedPipeRequestHelper
                    .GetDHCPLeaseInformation(networkInterface.CommonInformation.Name, cancellationToken);
            }

            AppendOutputForVLAN(listOutput, 2, networkInterface.IPv4.VLANEnabled, networkInterface.IPv4.VLANID);
            AppendOutputForIPv4(listOutput, 2, networkInterface.IPv4, dhcpLease);
        }

        if (!showNamesOnly)
            AppendOutputForDNS(output, config.NetworkDNSSettings);

        output.WriteLine();
    }

    private static async Task AppendOutputForCommonInformation(
        CliComplexOutputBase output,
        int indentationLevel,
        NetworkInterfaceCommonInformation commonInformation)
    {
        var physicalAddress = commonInformation.PhysicalAddress;
        var originalPhysicalAddress =
            await NamedPipeRequestHelper.GetOriginalPhysicalAddress(commonInformation.Name, CancellationToken.None)
            ?? CommandLineStrings_NetworkInterfaces.Unknown;

        var multiKeyValueOutput = output
            .AddMultiKeyValueOutput(indentationLevel)
            .WithTitle(CommandLineStrings_NetworkInterfaces.CommonInfo)
            .With(new(CommandLineStrings_NetworkInterfaces.Enabled, commonInformation.Enabled));

        if (physicalAddress.Equals(originalPhysicalAddress))
        {
            multiKeyValueOutput.With(new(CommandLineStrings_NetworkInterfaces.PhysicalAddress, commonInformation.PhysicalAddress));
        }
        else
        {
            multiKeyValueOutput.With(
                new(CommandLineStrings_NetworkInterfaces.PhysicalAddress, physicalAddress),
                new(CommandLineStrings_NetworkInterfaces.OriginalPhysicalAddress, originalPhysicalAddress));
        }
    }

    private static void AppendOutputForIPv4(
        CliComplexOutputBase output,
        int indentationLevel,
        IPv4Settings ipv4Settings,
        DHCPLease? dhcpLease)
    {
        var ipv4Output = output.AddKeyValueOutput(indentationLevel)
            .WithTitle(CommandLineStrings_NetworkInterfaces.IPv4)
            .WithSingleItemOnNewLine()
            .With(new(CommandLineStrings_NetworkInterfaces.DHCP, ipv4Settings.DHCPEnabled));

        if (ipv4Settings.DHCPEnabled)
        {
            AppendOutputForDHCPLease(ipv4Output, indentationLevel + 1, dhcpLease);
        }
        else
        {
            ipv4Output
                .AddKeyValueOutput(indentationLevel + 1)
                .With(new(CommandLineStrings_NetworkInterfaces.Gateway, ipv4Settings.Gateway));

            if (ipv4Settings.IPv4Details.Count > 0)
            {
                var ipv4DetailsOutput = ipv4Output
                    .AddMultiKeyValueOutput(indentationLevel + 1)
                    .WithTitle(CommandLineStrings_NetworkInterfaces.IPAddresses)
                    .WithSingleItemOnNewLine();

                foreach (var ipv4Detail in ipv4Settings.IPv4Details)
                {
                    ipv4DetailsOutput.With(
                        new(CommandLineStrings_NetworkInterfaces.IPAddress, ipv4Detail.IPAddress),
                        new(CommandLineStrings_NetworkInterfaces.SubnetMask, ipv4Detail.Netmask));
                }
            }
        }
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
                .WithTitle(CommandLineStrings_NetworkInterfaces.DHCPLease)
                .With(CommandLineStrings_NetworkInterfaces.DHCPLeaseNotAvailable);

            return;
        }

        output
            .AddKeyValueOutput(indentationLevel)
            .WithTitle(CommandLineStrings_NetworkInterfaces.DHCPLease)
            .With(new(CommandLineStrings_NetworkInterfaces.DHCPLeaseObtained, lease.LeaseObtained))
            .With(new(CommandLineStrings_NetworkInterfaces.DHCPLeaseExpires, lease.LeaseExpires))
            .With(new(CommandLineStrings_NetworkInterfaces.Gateway, lease.Gateway))
            .With(new(CommandLineStrings_NetworkInterfaces.IPAddress, lease.IPv4Detail?.IPAddress))
            .With(new(CommandLineStrings_NetworkInterfaces.SubnetMask, lease.IPv4Detail?.Netmask));

        var dnsOutput = output
            .AddListOutput(indentationLevel + 1)
            .WithTitle(CommandLineStrings_DNS.DNS);

        if (lease.NetworkDNSSettings is null)
        {
            dnsOutput.With(CommandLineStrings_NetworkInterfaces.DHCPLeaseNotAvailable);
        }
        else
        {
            AppendOutputForDNSSuffix(dnsOutput, indentationLevel + 2, lease.NetworkDNSSettings);
            AppendOutputForSearchDomains(dnsOutput, indentationLevel + 2, lease.NetworkDNSSettings);
            AppendOutputForNameServers(dnsOutput, indentationLevel + 2, lease.NetworkDNSSettings.NameServers);
        }
    }

    private static void AppendOutputForDNS(
        CliComplexOutputBase output,
        NetworkDNSSettings networkDNSSettings)
    {
        var dnsOutput = output
            .AddListOutput()
            .WithTitle(CommandLineStrings_DNS.DNS);

        AppendOutputForDNSSuffix(dnsOutput, 1, networkDNSSettings);
        AppendOutputForSearchDomains(dnsOutput, 1, networkDNSSettings);
        AppendOutputForNameServers(dnsOutput, 1, networkDNSSettings.NameServers);
    }

    private static void AppendOutputForDNSSuffix(
        CliComplexOutputBase output,
        int indentationLevel,
        NetworkDNSSettings networkDNSSettings)
    {
        var dnsSuffixOutput = output
            .AddListOutput(indentationLevel)
            .WithTitle(CommandLineStrings_DNS.Suffix, networkDNSSettings.DNSSuffixEnabled);

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
            .WithTitle(CommandLineStrings_DNS.SearchDomains, networkDNSSettings.SearchDomainsEnabled);

        if (networkDNSSettings.SearchDomains.Count == 0)
            searchDomainsOutput.With(CliOutputBase.NotSetValue);
        else
            networkDNSSettings.SearchDomains.ForEach(domain => searchDomainsOutput.With(domain));
    }

    private static void AppendOutputForNameServers(
        CliComplexOutputBase output,
        int indentationLevel,
        NameServersSettings nameServersSettings)
    {
        var nameServersOutput = output
            .AddListOutput(indentationLevel)
            .WithTitle(CommandLineStrings_DNS.NameServers, nameServersSettings.Enabled);

        if (nameServersSettings.Addresses.Count == 0)
            nameServersOutput.With(CliOutputBase.NotSetValue);
        else
            nameServersSettings.Addresses.ForEach(address => nameServersOutput.With(address));
    }

    private static void AppendOutputForVLAN(
        CliComplexOutputBase output,
        int indentationLevel,
        bool vlanEnabled,
        int vlanID)
    {
        // If vlanID is '0' it is set to null to interpret as 'not set' value.
        int? vlanIdValue = vlanID == 0 ? null : vlanID;

        output.AddKeyValueOutput(indentationLevel)
            .WithTitle(CommandLineStrings_NetworkInterfaces.VLAN)
            .With(new(CommandLineStrings_NetworkInterfaces.Enabled, vlanEnabled))
            .With(new(CommandLineStrings_NetworkInterfaces.VLANID, vlanIdValue));
    }
}

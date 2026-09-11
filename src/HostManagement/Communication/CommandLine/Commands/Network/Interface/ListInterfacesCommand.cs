using System.CommandLine;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Communication.CommandLine.Output;
using HostManagement.Communication.CommandLine.Resources;
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
    public ListInterfacesCommand()
        : base(
            CliStrings.Network.Interface.ListInterfacesCommandName,
            CliStrings.Network.Interface.ListInterfacesCommandDescription)
    {
        Add(s_showNamesOnlyOption);

        SetAction((parseResult, cancellationToken)
            => ListHandler(parseResult.GetValue(s_showNamesOnlyOption), cancellationToken));
    }

    private static async Task ListHandler(bool showNamesOnly, CancellationToken cancellationToken)
    {
        if (await GetConfiguration(cancellationToken) is not SystemConfiguration config)
            return;

        if (config.NetworkInterfaces.Count == 0)
        {
            SimpleOutput.WriteLine(CliStrings.Network.Interface.InterfacesNotFound);
            return;
        }

        var output = new ListOutput();
        output.WithTitle(CliStrings.Network.Interface.Interfaces);

        foreach (var networkInterface in config.NetworkInterfaces)
        {
            var listOutput = output
                .AddListOutput(1)
                .WithTitle(
                    networkInterface.CommonInformation.Name,
                    networkInterface.CommonInformation.Enabled,
                    useTitleDelimiter: !showNamesOnly);

            if (showNamesOnly)
                continue;

            await AppendOutputForMACAddress(listOutput, 2, networkInterface.CommonInformation);

            DHCPLease? dhcpLease = null;

            if (networkInterface.IPv4.DHCPEnabled)
            {
                dhcpLease = await NamedPipeRequestHelper
                    .GetDHCPLeaseInformation(networkInterface.CommonInformation.Name, cancellationToken);
            }

            AppendOutputForVLAN(listOutput, 2, networkInterface.VLAN);
            AppendOutputForIPv4(listOutput, 2, networkInterface.IPv4, dhcpLease);
        }

        if (!showNamesOnly)
            AppendOutputForDNS(output, config.NetworkDNSSettings);

        output.WriteLine();
    }

    private static async Task AppendOutputForMACAddress(
        CliComplexOutputBase output,
        int indentationLevel,
        NetworkInterfaceCommonInformation commonInformation)
    {
        var userDefinedMACAddressEnabled = commonInformation.UserDefinedMACAddress.Enabled;
        var userDefinedMACAddress = commonInformation.UserDefinedMACAddress.Address;

        var originalMACAddress = await NamedPipeRequestHelper
            .GetOriginalPhysicalAddress(commonInformation.Name, CancellationToken.None)
            ?? CliStrings.Network.Interface.Unknown;

        output.AddListOutput(indentationLevel)
            .WithTitle("MAC")
            .AddListOutput(indentationLevel + 1)
                .WithTitle(CliStrings.Network.Interface.MAC.UserDefinedAddress, userDefinedMACAddressEnabled)
                .With(userDefinedMACAddress)
            .AddListOutput(indentationLevel + 1)
                .WithTitle(CliStrings.Network.Interface.MAC.OriginalAddress, !userDefinedMACAddressEnabled)
                .With(originalMACAddress);
    }

    private static void AppendOutputForIPv4(
        CliComplexOutputBase output,
        int indentationLevel,
        IPv4Settings ipv4Settings,
        DHCPLease? dhcpLease)
    {
        var ipv4Output = output.AddKeyValueOutput(indentationLevel)
            .WithTitle(CliStrings.Network.Interface.IPv4.Label)
            .WithSingleItemOnNewLine()
            .With(new(CliStrings.Network.Interface.DHCP.Label, ipv4Settings.DHCPEnabled));

        if (ipv4Settings.DHCPEnabled)
        {
            AppendOutputForDHCPLease(ipv4Output, indentationLevel + 1, dhcpLease);
        }
        else
        {
            ipv4Output
                .AddKeyValueOutput(indentationLevel + 1)
                .With(new(CliStrings.Network.Interface.Gateway.Label, ipv4Settings.Gateway));

            if (ipv4Settings.IPv4Details.Count > 0)
            {
                var ipv4DetailsOutput = ipv4Output
                    .AddMultiKeyValueOutput(indentationLevel + 1)
                    .WithTitle(CliStrings.Network.Interface.IP.IPAddresses)
                    .WithSingleItemOnNewLine();

                foreach (var ipv4Detail in ipv4Settings.IPv4Details)
                {
                    ipv4DetailsOutput.With(
                        new(CliStrings.Network.Interface.IP.IPAddress, ipv4Detail.IPAddress),
                        new(CliStrings.Network.Interface.IP.SubnetMask, ipv4Detail.Netmask));
                }
            }
        }
    }

    private static void AppendOutputForDHCPLease(CliComplexOutputBase output, int indentationLevel, DHCPLease? lease)
    {
        if (lease is null)
        {
            output
                .AddListOutput(indentationLevel)
                .WithTitle(CliStrings.Network.Interface.DHCP.Lease)
                .With(CliStrings.Network.Interface.DHCP.LeaseNotAvailable);

            return;
        }

        output
            .AddKeyValueOutput(indentationLevel)
            .WithTitle(CliStrings.Network.Interface.DHCP.Lease)
            .With(new(CliStrings.Network.Interface.DHCP.LeaseObtained, lease.LeaseObtained))
            .With(new(CliStrings.Network.Interface.DHCP.LeaseExpires, lease.LeaseExpires))
            .With(new(CliStrings.Network.Interface.Gateway.Label, lease.Gateway))
            .With(new(CliStrings.Network.Interface.IP.IPAddress, lease.IPv4Detail?.IPAddress))
            .With(new(CliStrings.Network.Interface.IP.SubnetMask, lease.IPv4Detail?.Netmask));

        var dnsOutput = output
            .AddListOutput(indentationLevel + 1)
            .WithTitle(CliStrings.Network.DNS.Label);

        if (lease.NetworkDNSSettings is null)
        {
            dnsOutput.With(CliStrings.Network.Interface.DHCP.LeaseNotAvailable);
        }
        else
        {
            AppendOutputForPrimaryDNSSuffix(dnsOutput, indentationLevel + 2, lease.NetworkDNSSettings.PrimaryDNSSuffix);
            AppendOutputForSearchDomains(dnsOutput, indentationLevel + 2, lease.NetworkDNSSettings.SearchDomains);
            AppendOutputForNameServers(dnsOutput, indentationLevel + 2, lease.NetworkDNSSettings.NameServers);
        }
    }

    private static void AppendOutputForDNS(CliComplexOutputBase output, NetworkDNSSettings networkDNSSettings)
    {
        var dnsOutput = output
            .AddListOutput()
            .WithTitle(CliStrings.Network.DNS.Label);

        AppendOutputForPrimaryDNSSuffix(dnsOutput, 1, networkDNSSettings.PrimaryDNSSuffix);
        AppendOutputForSearchDomains(dnsOutput, 1, networkDNSSettings.SearchDomains);
        AppendOutputForNameServers(dnsOutput, 1, networkDNSSettings.NameServers);
    }

    private static void AppendOutputForPrimaryDNSSuffix(
        CliComplexOutputBase output,
        int indentationLevel,
        PrimaryDNSSuffixSettings primaryDnsSuffixSettings)
    {
        var dnsSuffixOutput = output
            .AddListOutput(indentationLevel)
            .WithTitle(CliStrings.Network.DNS.Suffix.Label, primaryDnsSuffixSettings.Enabled);

        if (string.IsNullOrWhiteSpace(primaryDnsSuffixSettings.Suffix))
            dnsSuffixOutput.With(CliOutputBase.NotSetValue);
        else
            dnsSuffixOutput.With(primaryDnsSuffixSettings.Suffix);
    }

    private static void AppendOutputForSearchDomains(
        CliComplexOutputBase output,
        int indentationLevel,
        SearchDomainsSettings searchDomainsSettings)
    {
        var searchDomainsOutput = output
            .AddListOutput(indentationLevel)
            .WithTitle(CliStrings.Network.DNS.SearchDomains.Label, searchDomainsSettings.Enabled);

        if (searchDomainsSettings.Domains.Count == 0)
            searchDomainsOutput.With(CliOutputBase.NotSetValue);
        else
            searchDomainsSettings.Domains.ForEach(domain => searchDomainsOutput.With(domain));
    }

    private static void AppendOutputForNameServers(
        CliComplexOutputBase output,
        int indentationLevel,
        NameServersSettings nameServersSettings)
    {
        var nameServersOutput = output
            .AddListOutput(indentationLevel)
            .WithTitle(CliStrings.Network.DNS.NameServers.Label, nameServersSettings.Enabled);

        if (nameServersSettings.Addresses.Count == 0)
            nameServersOutput.With(CliOutputBase.NotSetValue);
        else
            nameServersSettings.Addresses.ForEach(address => nameServersOutput.With(address));
    }

    private static void AppendOutputForVLAN(
        CliComplexOutputBase output,
        int indentationLevel,
        VLANSettings vlanSettings)
    {
        // If VLAN ID is '0', it is set to 'null' to interpret as 'not set' value.
        int? vlanIdValue = vlanSettings.ID == 0 ? null : vlanSettings.ID;

        output.AddKeyValueOutput(indentationLevel)
            .WithTitle(CliStrings.Network.Interface.VLAN.Label)
            .With(new(CliStrings.Common.EnabledLabel, vlanSettings.Enabled))
            .With(new(CliStrings.Network.Interface.VLAN.ID, vlanIdValue));
    }
}

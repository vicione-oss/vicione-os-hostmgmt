using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Validation.Utils;

namespace HostManagement.Shared.Validation.Validators.DNS;

/// <summary>
/// Represents the validator for a <see cref="NetworkDNSSettings.NameServers"/>.
/// </summary>
internal sealed class NameServersValidator
{
    public static Task Validate(
        bool nameServersEnabled,
        List<IPAddress> nameServers,
        ValidationResult result)
    {
        if (!nameServersEnabled)
            return Task.CompletedTask;

        if (HasDuplicateNameServers(nameServers, result))
            return Task.CompletedTask;

        if (HasLinkLocalNameServers(nameServers, result))
            return Task.CompletedTask;

        if (HasMulticastNameServers(nameServers, result))
            return Task.CompletedTask;

        if (HasBroadcastNameServers(nameServers, result))
            return Task.CompletedTask;

        return Task.CompletedTask;
    }

    private static bool HasDuplicateNameServers(
        List<IPAddress> nameServers,
        ValidationResult result)
    {
        var duplicateNameServers = nameServers
            .GroupBy(address => address)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        if (duplicateNameServers.Count > 0)
        {
            result.AddErrorMessage(
                string.Format(
                    CultureInfo.InvariantCulture,
                    CompositeFormat.Parse(ValidationResultMessages.DNSNameServersNotUnique),
                    string.Join(
                        ", ",
                        duplicateNameServers.Select(address => $"'{address}'"))),
                ErrorCodes.ValidationFailed);

            return true;
        }

        return false;
    }

    private static bool HasLinkLocalNameServers(
        List<IPAddress> nameServers,
        ValidationResult result)
    {
        var hasLinkLocalNameServers = false;

        foreach (var nameServer in nameServers)
        {
            if (nameServer.AddressFamily == AddressFamily.InterNetwork)
            {
                if (IPAddressValidatorUtils.IsIPv4LinkLocal(nameServer))
                {
                    hasLinkLocalNameServers = true;

                    result.AddErrorMessage(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            CompositeFormat.Parse(
                                ValidationResultMessages.DNSNameServerCanNotBeLinkLocal),
                            nameServer.ToString(),
                            IPAddressValidatorUtils.IPv4LinkLocalNetworkCIDR),
                    ErrorCodes.ValidationFailed);

                    continue;
                }
            }

            if (nameServer.AddressFamily == AddressFamily.InterNetworkV6)
            {
                if (IPAddressValidatorUtils.IsIPv6LinkLocal(nameServer))
                {
                    hasLinkLocalNameServers = true;

                    result.AddErrorMessage(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            CompositeFormat.Parse(
                                ValidationResultMessages.DNSNameServerCanNotBeLinkLocal),
                            nameServer.ToString(),
                            IPAddressValidatorUtils.IPv6LinkLocalNetworkCIDR),
                    ErrorCodes.ValidationFailed);

                    continue;
                }
            }
        }

        return hasLinkLocalNameServers;
    }

    private static bool HasMulticastNameServers(
        List<IPAddress> nameServers,
        ValidationResult result)
    {
        var hasMulticastNameServers = false;

        foreach (var nameServer in nameServers)
        {
            if (nameServer.AddressFamily == AddressFamily.InterNetwork)
            {
                if (IPAddressValidatorUtils.IsIPv4Multicast(nameServer))
                {
                    hasMulticastNameServers = true;

                    result.AddErrorMessage(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            CompositeFormat.Parse(
                                ValidationResultMessages.DNSNameServerCanNotBeMulticast),
                            nameServer.ToString(),
                            IPAddressValidatorUtils.IPv4MulticastNetworkCIDR),
                    ErrorCodes.ValidationFailed);

                    continue;
                }
            }

            if (nameServer.AddressFamily == AddressFamily.InterNetworkV6)
            {
                if (IPAddressValidatorUtils.IsIPv6Multicast(nameServer))
                {
                    hasMulticastNameServers = true;

                    result.AddErrorMessage(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            CompositeFormat.Parse(
                                ValidationResultMessages.DNSNameServerCanNotBeMulticast),
                            nameServer.ToString(),
                            IPAddressValidatorUtils.IPv6MulticastNetworkCIDR),
                    ErrorCodes.ValidationFailed);

                    continue;
                }
            }
        }

        return hasMulticastNameServers;
    }

    private static bool HasBroadcastNameServers(
        List<IPAddress> nameServers,
        ValidationResult result)
    {
        var hasBroadcastNameServers = false;

        foreach (var nameServer in nameServers)
        {
            if (nameServer.AddressFamily == AddressFamily.InterNetwork)
            {
                if (IPAddressValidatorUtils.IsIPv4Broadcast(nameServer))
                {
                    hasBroadcastNameServers = true;

                    result.AddErrorMessage(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            CompositeFormat.Parse(
                                ValidationResultMessages.DNSNameServerCanNotBeBroadcast),
                            nameServer.ToString(),
                            IPAddressValidatorUtils.IPv4BroadcastNetworkCIDR),
                    ErrorCodes.ValidationFailed);

                    continue;
                }
            }
        }

        return hasBroadcastNameServers;
    }
}

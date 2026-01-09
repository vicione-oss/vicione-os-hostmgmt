using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Validation.Utils;

namespace HostManagement.Shared.Validation.Validators.DNS;

/// <summary>
/// Represents the validator for a <see cref="NetworkDNSSettings.StaticHosts"/>.
/// </summary>
internal sealed class StaticHostsValidator
{
    private const string CustomDomainName = "Static hosts hostname";

    public static Task Validate(
        bool staticHostsEnabled,
        List<StaticHostDetail> staticHosts,
        ValidationResult result)
    {
        if (!staticHostsEnabled)
            return Task.CompletedTask;

        if (HasDuplicateStaticHosts(staticHosts, result))
            return Task.CompletedTask;

        // Should aliases be allowed in the future, we need to add validation
        // for max. line length of etc/hosts (1024 characters).

        foreach (var staticHost in staticHosts)
        {
            ValidateIPAddress(staticHost.IPAddress, result);
            ValidateHostname(staticHost.Hostname, result);
        }

        return Task.CompletedTask;
    }

    private static bool HasDuplicateStaticHosts(
        List<StaticHostDetail> staticHosts,
        ValidationResult result)
    {
        var duplicateStaticHosts = staticHosts
            .GroupBy(staticHostDetail => staticHostDetail)
            .Where(group => group.Count() > 1)
            .SelectMany(group => group.Skip(1)) // Exclude the first instance, keep duplicates
            .ToList();

        if (duplicateStaticHosts.Count > 0)
        {
            var duplicateStaticHostsString = string.Join(
                ", ",
                duplicateStaticHosts.Select(staticHost => $"'{staticHost.IPAddress}'  '{staticHost.Hostname}'"));

            result.AddErrorMessage(
                string.Format(
                    CultureInfo.InvariantCulture,
                    CompositeFormat.Parse(
                        ValidationResultMessages.DNSStaticHostsHaveDuplicateEntries),
                    duplicateStaticHostsString),
            ErrorCodes.ValidationFailed);

            return true;
        }

        return false;
    }

    private static void ValidateIPAddress(IPAddress ipAddress, ValidationResult result)
    {
        // Loopback, link-local, multicast and broadcast addresses are in principle possible
        // but should be avoided and are for now treated as invalid.

        if (IsLoopbackAddress(ipAddress, result))
            return;

        if (IsLinkLocalAddress(ipAddress, result))
            return;

        if (IsMulticastAddress(ipAddress, result))
            return;

        if (IsBroadcastAddress(ipAddress, result))
            return;
    }

    private static bool IsLoopbackAddress(IPAddress ipAddress, ValidationResult result)
    {
        if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
        {
            if (IPAddressValidatorUtils.IsIPv4Loopback(ipAddress))
            {
                result.AddErrorMessage(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        CompositeFormat.Parse(
                            ValidationResultMessages.DNSStaticHostIPAddressCanNotBeLoopback),
                        ipAddress.ToString(),
                        IPAddressValidatorUtils.IPv4LoopbackNetworkCIDR),
                ErrorCodes.ValidationFailed);

                return true;
            }
        }

        if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
        {
            if (IPAddressValidatorUtils.IsIPv6Loopback(ipAddress))
            {
                result.AddErrorMessage(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        CompositeFormat.Parse(
                            ValidationResultMessages.DNSStaticHostIPAddressCanNotBeLoopback),
                        ipAddress.ToString(),
                        IPAddressValidatorUtils.IPv6LoopbackNetworkCIDR),
                ErrorCodes.ValidationFailed);

                return true;
            }
        }

        return false;
    }

    private static bool IsLinkLocalAddress(IPAddress ipAddress, ValidationResult result)
    {
        if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
        {
            if (IPAddressValidatorUtils.IsIPv4LinkLocal(ipAddress))
            {
                result.AddErrorMessage(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        CompositeFormat.Parse(
                            ValidationResultMessages.DNSStaticHostIPAddressCanNotBeLinkLocal),
                        ipAddress.ToString(),
                        IPAddressValidatorUtils.IPv4LinkLocalNetworkCIDR),
                ErrorCodes.ValidationFailed);

                return true;
            }
        }

        if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
        {
            if (IPAddressValidatorUtils.IsIPv6LinkLocal(ipAddress))
            {
                result.AddErrorMessage(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        CompositeFormat.Parse(
                            ValidationResultMessages.DNSStaticHostIPAddressCanNotBeLinkLocal),
                        ipAddress.ToString(),
                        IPAddressValidatorUtils.IPv6LinkLocalNetworkCIDR),
                ErrorCodes.ValidationFailed);

                return true;
            }
        }

        return false;
    }

    private static bool IsMulticastAddress(IPAddress ipAddress, ValidationResult result)
    {
        if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
        {
            if (IPAddressValidatorUtils.IsIPv4Multicast(ipAddress))
            {
                result.AddErrorMessage(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        CompositeFormat.Parse(
                            ValidationResultMessages.DNSStaticHostIPAddressCanNotBeMulticast),
                        ipAddress.ToString(),
                        IPAddressValidatorUtils.IPv4MulticastNetworkCIDR),
                ErrorCodes.ValidationFailed);

                return true;
            }
        }

        if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
        {
            if (IPAddressValidatorUtils.IsIPv6Multicast(ipAddress))
            {
                result.AddErrorMessage(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        CompositeFormat.Parse(
                            ValidationResultMessages.DNSStaticHostIPAddressCanNotBeMulticast),
                        ipAddress.ToString(),
                        IPAddressValidatorUtils.IPv6MulticastNetworkCIDR),
                ErrorCodes.ValidationFailed);

                return true;
            }
        }

        return false;
    }

    private static bool IsBroadcastAddress(IPAddress ipAddress, ValidationResult result)
    {
        if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
        {
            if (IPAddressValidatorUtils.IsIPv4Broadcast(ipAddress))
            {
                result.AddErrorMessage(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        CompositeFormat.Parse(
                            ValidationResultMessages.DNSStaticHostIPAddressCanNotBeBroadcast),
                        ipAddress.ToString(),
                        IPAddressValidatorUtils.IPv4BroadcastNetworkCIDR),
                ErrorCodes.ValidationFailed);

                return true;
            }
        }

        return false;
    }

    private static void ValidateHostname(string hostname, ValidationResult result)
    {
        if (hostname.Contains('.'))
            FQDNValidator.Validate(hostname, result, CustomDomainName);
        else
            DomainLabelValidator.Validate(hostname, result, CustomDomainName);
    }
}

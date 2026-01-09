using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Validation.Utils;

namespace HostManagement.Shared.Validation.Validators.DNS;

// TODO Add unit tests
/// <summary>
/// Represents the validator for a <see cref="NetworkDNSSettings.StaticHosts"/>.
/// </summary>
internal sealed class StaticHostsValidator : IValidator
{
    public Task Validate(
        SystemConfiguration current,
        SystemConfiguration proposed,
        ValidationResult result)
    {
        var duplicateStaticHosts = proposed.NetworkDNSSettings.StaticHosts
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
        }

        foreach (var staticHost in proposed.NetworkDNSSettings.StaticHosts)
        {
            ValidateIPAddress(staticHost.IPAddress, result);

            if (staticHost.Hostname.Contains('.'))
                FQDNValidator.Validate(staticHost.Hostname, result, "Static hosts hostname");
            else
                DomainLabelValidator.Validate(staticHost.Hostname, result, "Static host hostname");
        }

        return Task.CompletedTask;
    }

    internal static void ValidateIPAddress(IPAddress ipAddress, ValidationResult result)
    {
        if (IsLoopbackAddress(ipAddress, result))
            return;

        if (IsLinkLocalAddress(ipAddress, result))
            return;

        if (IsMulticastAddress(ipAddress, result))
            return;
    }

    internal static bool IsLoopbackAddress(IPAddress ipAddress, ValidationResult result)
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

    internal static bool IsLinkLocalAddress(IPAddress ipAddress, ValidationResult result)
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

    internal static bool IsMulticastAddress(IPAddress ipAddress, ValidationResult result)
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
}

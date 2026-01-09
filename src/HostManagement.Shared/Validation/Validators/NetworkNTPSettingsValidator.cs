using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Validation.Utils;
using HostManagement.Shared.Validation.Validators.DNS;

namespace HostManagement.Shared.Validation.Validators;

internal sealed class NetworkNTPSettingsValidator : IValidator
{
    public Task Validate(
        SystemConfiguration current,
        SystemConfiguration proposed,
        ValidationResult result)
    {
        if (HasNonUniqueNTPServer(proposed.NetworkNTPSettings.NTPServers, result))
            return Task.CompletedTask;

        foreach (var ntpServer in proposed.NetworkNTPSettings.NTPServers)
            ValidateNTPServer(ntpServer, result);

        return Task.CompletedTask;
    }

    internal static bool HasNonUniqueNTPServer(List<string> ntpServers, ValidationResult result)
    {
        var duplicateNTPServers = ntpServers
            .GroupBy(ntpServer => ntpServer)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key);

        if (duplicateNTPServers.Any())
        {
            var duplicateNTPServersString =
                string.Join(", ", duplicateNTPServers.Select(ntpServer => $"'{ntpServer}'"));

            result.AddErrorMessage(
                string.Format(
                    CultureInfo.InvariantCulture,
                    CompositeFormat.Parse(
                        ValidationResultMessages.NTPServersAreNotUnique),
                    duplicateNTPServersString),
                ErrorCodes.ValidationFailed);

            return true;
        }

        return false;
    }

    internal static void ValidateNTPServer(string ntpServer, ValidationResult result)
    {
        if (IsNTPServerEmpty(ntpServer, result))
            return;

        if (IsPotentialIPv4Address(ntpServer))
        {
            if (!IPAddress.TryParse(ntpServer, out var ipv4Address))
            {
                result.AddErrorMessage(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        CompositeFormat.Parse(
                            ValidationResultMessages.NTPServerIsNoValidIPv4Address),
                        ntpServer),
                    ErrorCodes.ValidationFailed);

                return;
            }

            if (IsInvalidIPAddress(ipv4Address, result))
                return;
        }
        else if (IPAddress.TryParse(ntpServer, out var ipv6Address) &&
                 ipv6Address.AddressFamily == AddressFamily.InterNetworkV6)
        {
            if (IsInvalidIPAddress(ipv6Address, result))
                return;
        }
        else
        {
            FQDNValidator.Validate(ntpServer, result, "NTP server");
            return;
        }
    }

    internal static bool IsNTPServerEmpty(string ntpServer, ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(ntpServer))
        {
            result.AddErrorMessage(
                ValidationResultMessages.NTPServerCanNotBeEmpty,
                ErrorCodes.ValidationFailed);

            return true;
        }

        return false;
    }

    internal static bool IsPotentialIPv4Address(string ntpServer)
        => IPAddressValidatorUtils.IsPotentialIPv4Address(ntpServer);

    internal static bool IsInvalidIPAddress(IPAddress ntpServerAddress, ValidationResult result)
    {
        if (ntpServerAddress.AddressFamily == AddressFamily.InterNetwork)
        {
            if (IPAddressValidatorUtils.IsIPv4LinkLocal(ntpServerAddress))
            {
                result.AddErrorMessage(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        CompositeFormat.Parse(
                            ValidationResultMessages.NTPServerCanNotBeLinkLocal),
                        ntpServerAddress.ToString(),
                        IPAddressValidatorUtils.IPv4LinkLocalNetworkCIDR),
                    ErrorCodes.ValidationFailed);

                return true;
            }

            if (IPAddressValidatorUtils.IsIPv4Multicast(ntpServerAddress))
            {
                result.AddErrorMessage(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        CompositeFormat.Parse(
                            ValidationResultMessages.NTPServerCanNotBeMulticast),
                        ntpServerAddress.ToString(),
                        IPAddressValidatorUtils.IPv4MulticastNetworkCIDR),
                    ErrorCodes.ValidationFailed);

                return true;
            }

            return false;
        }
        else
        {
            if (IPAddressValidatorUtils.IsIPv6LinkLocal(ntpServerAddress))
            {
                result.AddErrorMessage(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        CompositeFormat.Parse(
                            ValidationResultMessages.NTPServerCanNotBeLinkLocal),
                        ntpServerAddress.ToString(),
                        IPAddressValidatorUtils.IPv6LinkLocalNetworkCIDR),
                    ErrorCodes.ValidationFailed);

                return true;
            }

            if (IPAddressValidatorUtils.IsIPv6Multicast(ntpServerAddress))
            {
                result.AddErrorMessage(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        CompositeFormat.Parse(
                            ValidationResultMessages.NTPServerCanNotBeMulticast),
                        ntpServerAddress.ToString(),
                        IPAddressValidatorUtils.IPv6MulticastNetworkCIDR),
                    ErrorCodes.ValidationFailed);

                return true;
            }

            return false;
        }
    }
}

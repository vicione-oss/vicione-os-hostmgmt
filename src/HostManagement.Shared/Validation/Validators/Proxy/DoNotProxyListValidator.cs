using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Validation.Utils;
using HostManagement.Shared.Validation.Validators.DNS;

namespace HostManagement.Shared.Validation.Validators.Proxy;

internal sealed class DoNotProxyListValidator
{
    public static Task Validate(
        List<NetworkInterfaceDetail> interfaces,
        bool doNotProxyListEnabled,
        List<string> doNotProxyList,
        ValidationResult result)
    {
        if (!doNotProxyListEnabled)
            return Task.CompletedTask;

        if (HasEmptyDoNotProxyItems(doNotProxyList, result))
            return Task.CompletedTask;

        foreach (var doNotProxyItem in doNotProxyList)
            ValidateDoNotProxyItem(interfaces, doNotProxyItem, result);

        return Task.CompletedTask;
    }

    private static bool HasEmptyDoNotProxyItems(List<string> doNotProxyItems, ValidationResult result)
    {
        if (doNotProxyItems.Any(string.IsNullOrWhiteSpace))
        {
            result.AddErrorMessage(
                ValidationResultMessages.DoNotProxyItemCanNotBeEmpty,
                ErrorCodes.ValidationFailed);

            return true;
        }

        return false;
    }

    private static void ValidateDoNotProxyItem(
        List<NetworkInterfaceDetail> interfaces,
        string doNotProxyItem,
        ValidationResult result)
    {
        if (IPAddressValidatorUtils.IsPotentialIPv4Address(doNotProxyItem))
            ValidateIPv4Address(doNotProxyItem, result);
        else if (IPAddressValidatorUtils.IsPotentialIPv6Address(doNotProxyItem))
            ValidateIPv6Address(doNotProxyItem, result, out var _);
        else
            ValidateComplexDoNotProxyItem(interfaces, doNotProxyItem, result);
    }

    private static void ValidateIPv4Address(string ipv4String, ValidationResult result)
    {
        if (!IPAddress.TryParse(ipv4String, out var ipv4Address) ||
            ipv4Address.AddressFamily != AddressFamily.InterNetwork)
        {
            result.AddErrorMessage(
                string.Format(
                    CultureInfo.InvariantCulture,
                    CompositeFormat.Parse(
                        ValidationResultMessages.DoNotProxyItemIsNoValidIPv4Address),
                    ipv4String),
                ErrorCodes.ValidationFailed);

            return;
        }

        if (IsInvalidIPAddress(ipv4Address, result))
            return;
    }

    private static void ValidateIPv6Address(

        string ipv6String,
        ValidationResult result,
        out IPAddress? ipv6Address)
    {
        if (!IPAddress.TryParse(ipv6String, out var parsedIPv6Address) ||
            parsedIPv6Address.AddressFamily != AddressFamily.InterNetworkV6)
        {
            result.AddErrorMessage(
                string.Format(
                    CultureInfo.InvariantCulture,
                    CompositeFormat.Parse(
                        ValidationResultMessages.DoNotProxyItemIsNoValidIPv6Address),
                    ipv6String),
                ErrorCodes.ValidationFailed);

            ipv6Address = null;
            return;
        }

        if (IsInvalidIPAddress(parsedIPv6Address, result))
        {
            ipv6Address = null;
            return;
        }

        ipv6Address = parsedIPv6Address;
    }

    private static void ValidateComplexDoNotProxyItem(
        List<NetworkInterfaceDetail> interfaces,
        string doNotProxyItem,
        ValidationResult result)
    {
        if (doNotProxyItem.StartsWith('[') &&
            doNotProxyItem.Contains(']'))
        {
            // Must be IPv6 with port

            if (doNotProxyItem.EndsWith(']') ||
                doNotProxyItem.EndsWith("]:"))
            {
                result.AddErrorMessage(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        CompositeFormat.Parse(
                            ValidationResultMessages.DoNotProxyItemHasMissingPort),
                        doNotProxyItem),
                    ErrorCodes.ValidationFailed);

                return;
            }

            if (doNotProxyItem.Contains('%'))
                ValidateIPv6WithZoneIdentiferAndPort(interfaces, doNotProxyItem, result);
            else
                ValidateIPv6WithPort(doNotProxyItem, result);
        }
        else if (doNotProxyItem.Contains(':'))
        {
            // Must be IPv6 with zone identifier or IPv4 with port

            if (doNotProxyItem.Contains('%'))
                ValidateIPv6WithZoneIdentifer(interfaces, doNotProxyItem, result);
            else
                ValidateIPv4WithPort(doNotProxyItem, result);
        }
        else if (doNotProxyItem.Contains('.'))
        {
            // Must be Fully Qualified Domain Name

            /*
             * Allow a leading dot (e.g. '.company.com') to indicate that the "DoNotProxy" item
             * applies to the entire domain and all its subdomains.
             * 
             * This is a slight extension of strict FQDN validation but is common in "DoNotProxy" usage.
             */
            FQDNValidator.Validate(doNotProxyItem, result, "DoNotProxy item", shouldAllowLeadingDot: true);
        }
        else
        {
            // Simple hostname
            DomainLabelValidator.Validate(doNotProxyItem, result, "DoNotProxy item");
        }
    }

    private static void ValidateIPv4WithPort(string doNotProxyItem, ValidationResult result)
    {
        // IPv4 address with port - IPv4:PORT

        var parts = doNotProxyItem.Split(':');
        var ipv4String = parts[0];
        var portString = parts[1];

        ValidateIPv4Address(ipv4String, result);
        ValidateNoProxyPort(portString, result);
    }

    private static void ValidateIPv6WithPort(string doNotProxyItem, ValidationResult result)
    {
        // IPv6 address with port - [IPv6]:PORT

        var parts = doNotProxyItem.Split("]:");
        var ipv6String = parts[0].TrimStart('[');
        var portString = parts[1];

        ValidateIPv6Address(ipv6String, result, out var _);
        ValidateNoProxyPort(portString, result);
    }

    private static void ValidateIPv6WithZoneIdentifer(
        List<NetworkInterfaceDetail> interfaces,
        string doNotProxyItem,
        ValidationResult result)
    {
        // Potential link-local IPv6 address with zone ID - IPv6%ZONE_ID

        var parts = doNotProxyItem.Split('%');
        var ipv6String = parts[0];
        var zoneId = parts[1];

        ValidateIPv6Address(ipv6String, result, out var parsedIPv6Address);

        if (parsedIPv6Address is not null &&
            IPAddressValidatorUtils.IsIPv6LinkLocal(parsedIPv6Address))
        {
            ValidateZoneIdentifier(interfaces, doNotProxyItem, zoneId, result);
        }
    }

    private static void ValidateIPv6WithZoneIdentiferAndPort(
        List<NetworkInterfaceDetail> interfaces,
        string doNotProxyItem,
        ValidationResult result)
    {
        // Potential link-local IPv6 address with zone ID and port - [IPv6%ZONE_ID]:PORT

        var parts = doNotProxyItem.Split("]:");
        var subParts = parts[0].TrimStart('[').Split('%');
        var ipv6String = subParts[0];
        var zoneId = subParts[1];
        var portString = parts[1];

        ValidateIPv6Address(ipv6String, result, out var parsedIPv6Address);

        if (parsedIPv6Address is not null &&
            IPAddressValidatorUtils.IsIPv6LinkLocal(parsedIPv6Address))
        {
            ValidateZoneIdentifier(interfaces, doNotProxyItem, zoneId, result);
        }

        ValidateNoProxyPort(portString, result);
    }

    private static bool IsInvalidIPAddress(IPAddress ipAddress, ValidationResult result)
    {
        /* Link-Local
         * 
         * Addresses such as IPv4’s 169.254.x.x or IPv6’s fe80::/10 are valid. However, with IPv6 a
         * zone identifier is required (like %eth0). 
         *
         * Multicast
         * 
         * IPv4 addresses (typically 224.0.0.0/4) or IPv6 addresses (starting with ff00::/8) are in
         * principle valid. Since they're not generally intended for proxy-based traffic though,
         * it's best to leave them out of the DoNotProxy list (unless a very specific scenario exists).
         *
         * Broadcast
         * 
         * IPv4 addresses (255.255.255.255 or e.g. in 192.168.1.0/24 network 192.168.1.255) will be
         * treated as nonsensical and invalid although theoretically possible.
         */

        if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
        {
            if (IPAddressValidatorUtils.IsIPv4Multicast(ipAddress))
            {
                result.AddErrorMessage(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        CompositeFormat.Parse(
                            ValidationResultMessages.DoNotProxyItemCanNotBeMulticast),
                        ipAddress.ToString(),
                        IPAddressValidatorUtils.IPv4MulticastNetworkCIDR),
                    ErrorCodes.ValidationFailed);

                return true;
            }

            if (IPAddressValidatorUtils.IsIPv4Broadcast(ipAddress))
            {
                result.AddErrorMessage(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        CompositeFormat.Parse(
                            ValidationResultMessages.DoNotProxyItemCanNotBeBroadcast),
                        ipAddress.ToString(),
                        IPAddressValidatorUtils.IPv4BroadcastNetworkCIDR),
                    ErrorCodes.ValidationFailed);

                return true;
            }

            return false;
        }
        else
        {
            if (IPAddressValidatorUtils.IsIPv6Multicast(ipAddress))
            {
                result.AddErrorMessage(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        CompositeFormat.Parse(
                            ValidationResultMessages.DoNotProxyItemCanNotBeMulticast),
                        ipAddress.ToString(),
                        IPAddressValidatorUtils.IPv6MulticastNetworkCIDR),
                    ErrorCodes.ValidationFailed);

                return true;
            }

            return false;
        }
    }

    private static void ValidateNoProxyPort(string portString, ValidationResult result)
        => ProxyPortValidator.Validate(portString, result);

    private static void ValidateZoneIdentifier(
        List<NetworkInterfaceDetail> interfaces,
        string doNotProxyItem,
        string zoneIdentifer,
        ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(zoneIdentifer))
        {
            result.AddErrorMessage(
                string.Format(
                    CultureInfo.InvariantCulture,
                    CompositeFormat.Parse(
                        ValidationResultMessages.DoNotProxyItemHasEmptyZoneIdentifier),
                    doNotProxyItem),
                ErrorCodes.ValidationFailed);

            return;
        }

        if (zoneIdentifer.Length > 15)
        {
            result.AddErrorMessage(
                string.Format(
                    CultureInfo.InvariantCulture,
                    CompositeFormat.Parse(
                        ValidationResultMessages.DoNotProxyItemZoneIdentifierInvalidCharacterCount),
                    zoneIdentifer,
                    doNotProxyItem),
                ErrorCodes.ValidationFailed);

            return;
        }

        foreach (var character in zoneIdentifer)
        {
            if (!char.IsAsciiLetterOrDigit(character) &&
                character != '_' &&
                character != '-')
            {
                result.AddErrorMessage(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        CompositeFormat.Parse(
                            ValidationResultMessages.DoNotProxyItemHasZoneIdentifierWithInvalidCharacter),
                        zoneIdentifer,
                        doNotProxyItem),
                    ErrorCodes.ValidationFailed);

                return;
            }
        }

        /*
         * The IPv6 zone identifier must match the interface name as reported by the system.
         * 
         * Because we only have the virtual interface name here, we have to add the physical
         * interface name later. So the following check must suffice.
         */
        if (!interfaces.Any(d => d.CommonInformation.Name.Equals(zoneIdentifer, StringComparison.OrdinalIgnoreCase)))
        {
            result.AddErrorMessage(
                string.Format(
                    CultureInfo.InvariantCulture,
                    CompositeFormat.Parse(
                        ValidationResultMessages.DoNotProxyItemHasZoneIdentifierWithUnknownInterfaceName),
                    zoneIdentifer,
                    doNotProxyItem),
                ErrorCodes.ValidationFailed);

            return;
        }

        return;
    }
}

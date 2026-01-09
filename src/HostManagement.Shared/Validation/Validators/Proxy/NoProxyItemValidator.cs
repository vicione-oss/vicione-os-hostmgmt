using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Validation.Utils;
using HostManagement.Shared.Validation.Validators.DNS;

namespace HostManagement.Shared.Validation.Validators.Proxy;

/// <summary>
/// Represents the validator for a no-proxy item.
/// </summary>
public sealed class NoProxyItemValidator : IValidator<string, IEnumerable<NetworkInterfaceDetail>>
{
    private const string NoProxyItemText = "No-proxy item";

    /// <summary>
    /// Validates a no-proxy item asynchronously.
    /// </summary>
    /// <param name="noProxyItem">The no-proxy item.</param>
    /// <param name="interfaces">The list of network interfaces.</param>
    /// <param name="cancellationToken">A token to cancel the validation.</param>
    /// <inheritdoc/>
    public async Task<ValidationResult> ValidateAsync(
        string noProxyItem,
        IEnumerable<NetworkInterfaceDetail> interfaces,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(noProxyItem))
                return ValidationResult.CreateWithError(ProxyValidationResults.NoProxyItemCanNotBeEmpty);

            if (IPAddressValidatorUtils.IsPotentialIPv4Address(noProxyItem))
            {
                // IPv4

                return ValidateIPv4Address(noProxyItem);
            }
            else if (IPAddressValidatorUtils.IsPotentialIPv6Address(noProxyItem))
            {
                // IPv6

                return ValidateIPv6Address(noProxyItem, out var _);
            }
            else if (noProxyItem.StartsWith('[') && noProxyItem.Contains(']'))
            {
                // [IPv6]:PORT - IPv6 with port
                // [IPv6%ID]:PORT - IPv6 with zone identifier and port

                return await ValidateIPv6AddressWithPort(noProxyItem, interfaces, cancellationToken)
                    .ConfigureAwait(false);
            }
            else if (noProxyItem.Contains(':') && noProxyItem.Contains('%'))
            {
                // IPv6%ID - IPv6 with zone identifier

                return await ValidateIPv6AddressWithZoneIdentifier(noProxyItem, interfaces, cancellationToken)
                    .ConfigureAwait(false);
            }
            else if (noProxyItem.Contains(':'))
            {
                // IPv4:PORT - IPv4 with port

                return await ValidateIPv4AddressWithPort(noProxyItem, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                // Simple domain or Fully Qualified Domain Name (FQDN)

                /*
                 * Allow a leading dot (e.g. '.company.com') to indicate that the no-proxy item
                 * applies to the entire domain and all its subdomains.
                 * 
                 * This is a slight extension of strict FQDN validation but is common in no-proxy usage.
                 */

                return await new DomainNameValidator(NoProxyItemText, allowLeadingDot: true)
                    .ValidateAsync(noProxyItem, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            return ValidationResult.CreateWithError(ValidationResults.ValidationCancelled);
        }
    }

    private static ValidationResult ValidateIPv4Address(string ipv4String)
    {
        if (!IPAddress.TryParse(ipv4String, out var ipv4Address) ||
            ipv4Address.AddressFamily != AddressFamily.InterNetwork)
        {
            return ValidationResult.CreateWithError(
                string.Format(
                    CultureInfo.InvariantCulture,
                    CompositeFormat.Parse(
                        ProxyValidationResults.NoProxyItemIsNoValidIPv4Address),
                    ipv4String));
        }

        return ValidateIPAddress(ipv4Address);
    }

    private static ValidationResult ValidateIPv6Address(
        string ipv6String,
        [NotNullWhen(true)] out IPAddress? ipv6Address)
    {
        ipv6Address = null;

        if (!IPAddress.TryParse(ipv6String, out var parsedIPv6Address) ||
            parsedIPv6Address.AddressFamily != AddressFamily.InterNetworkV6)
        {
            return ValidationResult.CreateWithError(
                string.Format(
                    CultureInfo.InvariantCulture,
                    CompositeFormat.Parse(
                        ProxyValidationResults.NoProxyItemIsNoValidIPv6Address),
                    ipv6String));
        }

        ipv6Address = parsedIPv6Address;
        return ValidateIPAddress(parsedIPv6Address);
    }

    private static async Task<ValidationResult> ValidateIPv6AddressWithPort(
        string noProxyItem,
        IEnumerable<NetworkInterfaceDetail> interfaces,
        CancellationToken cancellationToken = default)
    {
        if (noProxyItem.EndsWith(']') ||
            noProxyItem.EndsWith("]:"))
        {
            return ValidationResult.CreateWithError(
                string.Format(
                    CultureInfo.InvariantCulture,
                    CompositeFormat.Parse(
                        ProxyValidationResults.NoProxyItemHasMissingPort),
                    noProxyItem));
        }

        if (noProxyItem.Contains('%'))
        {
            return await ValidateIPv6WithZoneIdentifierAndPort(noProxyItem, interfaces, cancellationToken)
                .ConfigureAwait(false);
        }
        else
        {
            // IPv6 address with port - [IPv6]:PORT

            var parts = noProxyItem.Split("]:");
            var ipv6String = parts[0].TrimStart('[');
            var portString = parts[1];

            var result = new ValidationResult();

            result.AddResult(ValidateIPv6Address(ipv6String, out var _));
            result.AddResult(await ValidatePort(portString, cancellationToken).ConfigureAwait(false));

            return result;
        }
    }

    private static async Task<ValidationResult> ValidateIPv6WithZoneIdentifierAndPort(
        string noProxyItem,
        IEnumerable<NetworkInterfaceDetail> interfaces,
        CancellationToken cancellationToken = default)
    {
        // Potential link-local IPv6 address with zone ID and port - [IPv6%ZONE_ID]:PORT

        var parts = noProxyItem.Split("]:");
        var subParts = parts[0].TrimStart('[').Split('%');
        var ipv6String = subParts[0];
        var zoneIdentifier = subParts[1];
        var portString = parts[1];

        var result = new ValidationResult();

        result.AddResult(ValidateIPv6Address(ipv6String, out var parsedIPv6Address));

        if (parsedIPv6Address is not null &&
            IPAddressValidatorUtils.IsIPv6LinkLocal(parsedIPv6Address))
        {
            result.AddResult(
                await new ZoneIdentifierValidator(noProxyItem)
                    .ValidateAsync(zoneIdentifier, interfaces, cancellationToken)
                    .ConfigureAwait(false));
        }

        result.AddResult(
            await ValidatePort(portString, cancellationToken)
                .ConfigureAwait(false));

        return result;
    }

    private static async Task<ValidationResult> ValidateIPv4AddressWithPort(
        string noProxyItem,
        CancellationToken cancellationToken = default)
    {
        // IPv4 address with port - IPv4:PORT

        var parts = noProxyItem.Split(':');
        var ipv4String = parts[0];
        var portString = parts[1];

        var result = new ValidationResult();

        result.AddResult(ValidateIPv4Address(ipv4String));
        result.AddResult(await ValidatePort(portString, cancellationToken).ConfigureAwait(false));

        return result;
    }

    private static async Task<ValidationResult> ValidateIPv6AddressWithZoneIdentifier(
        string noProxyItem,
        IEnumerable<NetworkInterfaceDetail> interfaces,
        CancellationToken cancellationToken = default)
    {
        // Potential link-local IPv6 address with zone ID - IPv6%ZONE_ID

        var parts = noProxyItem.Split('%');
        var ipv6String = parts[0];
        var zoneIdentifier = parts[1];

        var result = ValidateIPv6Address(ipv6String, out var parsedIPv6Address);

        if (!result.IsValid)
            return result;

        if (parsedIPv6Address is not null &&
            IPAddressValidatorUtils.IsIPv6LinkLocal(parsedIPv6Address))
        {
            return await new ZoneIdentifierValidator(noProxyItem)
                .ValidateAsync(zoneIdentifier, interfaces, cancellationToken)
                .ConfigureAwait(false);
        }

        return ValidationResult.Valid;
    }

    private static ValidationResult ValidateIPAddress(IPAddress ipAddress)
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
         * it's best to leave them out of the No-proxy list (unless a very specific scenario exists).
         *
         * Broadcast
         * 
         * IPv4 addresses (255.255.255.255 or e.g. in 192.168.1.0/24 network 192.168.1.255) will be
         * treated as nonsensical and invalid although theoretically possible.
         */

        if (IPAddressValidatorUtils.IsMulticast(ipAddress, out var multicastCIDRString))
        {
            return ValidationResult.CreateWithError(
                string.Format(
                    CultureInfo.InvariantCulture,
                    CompositeFormat.Parse(
                        ProxyValidationResults.NoProxyItemCanNotBeMulticast),
                    ipAddress.ToString(),
                    multicastCIDRString));
        }

        if (IPAddressValidatorUtils.IsBroadcast(ipAddress, out var broadcastCIDRString))
        {
            return ValidationResult.CreateWithError(
                string.Format(
                    CultureInfo.InvariantCulture,
                    CompositeFormat.Parse(
                        ProxyValidationResults.NoProxyItemCanNotBeBroadcast),
                    ipAddress.ToString(),
                    broadcastCIDRString));
        }

        return ValidationResult.Valid;
    }

    private static async Task<ValidationResult> ValidatePort(
        string portString,
        CancellationToken cancellationToken = default)
    {
        if (!int.TryParse(portString, out var port))
        {
            return ValidationResult.CreateWithError(
                string.Format(
                    CultureInfo.InvariantCulture,
                    CompositeFormat.Parse(
                        ProxyValidationResults.PortMustBeNumber),
                    portString));
        }

        return await new ProxyPortValidator()
            .ValidateAsync(port, cancellationToken)
            .ConfigureAwait(false);
    }
}

using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using HostManagement.Shared.Validation.Utils;
using HostManagement.Shared.Validation.Validators.DNS;

namespace HostManagement.Shared.Validation.Validators.NTP;

/// <summary>
/// Represents the validator for an NTP server.
/// </summary>
public sealed class NTPServerValidator : IValidator<string>
{
    private const string NtpServerText = "NTP server";

    /// <summary>
    /// Validates an NTP server asynchronously.
    /// </summary>
    /// <param name="ntpServer">The NTP server.</param>
    /// <param name="cancellationToken">A token to cancel the validation.</param>
    /// <inheritdoc/>
    public async Task<ValidationResult> ValidateAsync(
        string ntpServer,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(ntpServer))
                return ValidationResult.CreateWithError(NTPValidationResults.ServerCanNotBeEmpty);

            if (IPAddressValidatorUtils.IsPotentialIPv4Address(ntpServer))
            {
                return ValidateIPv4Address(ntpServer);
            }
            else if (IPAddressValidatorUtils.IsPotentialIPv6Address(ntpServer))
            {
                return ValidateIPv6Address(ntpServer);
            }
            else
            {
                return await new FQDNValidator(NtpServerText)
                    .ValidateAsync(ntpServer, cancellationToken)
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
                        NTPValidationResults.ServerIsNoValidIPv4Address),
                    ipv4String));
        }

        return ValidateIPAddress(ipv4Address);
    }

    private static ValidationResult ValidateIPv6Address(string ipv6String)
    {
        if (!IPAddress.TryParse(ipv6String, out var parsedIPv6Address) ||
            parsedIPv6Address.AddressFamily != AddressFamily.InterNetworkV6)
        {
            return ValidationResult.CreateWithError(
                string.Format(
                    CultureInfo.InvariantCulture,
                    CompositeFormat.Parse(
                        NTPValidationResults.ServerIsNoValidIPv6Address),
                    ipv6String));
        }

        return ValidateIPAddress(parsedIPv6Address);
    }

    private static ValidationResult ValidateIPAddress(IPAddress ipAddress)
    {
        if (IPAddressValidatorUtils.IsLinkLocal(ipAddress, out var linkLocalCIDRString))
        {
            return ValidationResult.CreateWithError(
                string.Format(
                    CultureInfo.InvariantCulture,
                    CompositeFormat.Parse(
                        NTPValidationResults.ServerCanNotBeLinkLocal),
                    ipAddress.ToString(),
                    linkLocalCIDRString));
        }

        if (IPAddressValidatorUtils.IsMulticast(ipAddress, out var multicastCIDRString))
        {
            return ValidationResult.CreateWithError(
                string.Format(
                    CultureInfo.InvariantCulture,
                    CompositeFormat.Parse(
                        NTPValidationResults.ServerCanNotBeMulticast),
                    ipAddress.ToString(),
                    multicastCIDRString));
        }

        if (IPAddressValidatorUtils.IsBroadcast(ipAddress, out var broadcastCIDRString))
        {
            return ValidationResult.CreateWithError(
                string.Format(
                    CultureInfo.InvariantCulture,
                    CompositeFormat.Parse(
                        NTPValidationResults.ServerCanNotBeBroadcast),
                    ipAddress.ToString(),
                    broadcastCIDRString));
        }

        return ValidationResult.Valid;
    }
}

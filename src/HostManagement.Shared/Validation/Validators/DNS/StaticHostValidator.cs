using System.Globalization;
using System.Net;
using System.Text;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Validation.Utils;

namespace HostManagement.Shared.Validation.Validators.DNS;

/// <summary>
/// Represents the validator for a static host.
/// </summary>
public sealed class StaticHostValidator : IValidator<StaticHostDetail>
{
    private const string StaticHostHostnameText = "Static host hostname";

    /// <summary>
    /// Validates a static host asynchronously.
    /// </summary>
    /// <param name="staticHost">The static host.</param>
    /// <param name="cancellationToken">A token to cancel the validation.</param>
    /// <inheritdoc/>
    public async Task<ValidationResult> ValidateAsync(
        StaticHostDetail staticHost,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Should aliases be allowed in the future, we need to add validation
            // for max. line length of 'etc/hosts' file (1024 characters).

            var result = new ValidationResult();

            result.AddResult(ValidateIPAddress(staticHost.IPAddress));

            result.AddResult(
                await new DomainNameValidator(StaticHostHostnameText)
                    .ValidateAsync(staticHost.Hostname, cancellationToken)
                    .ConfigureAwait(false));

            return result;
        }
        catch (OperationCanceledException)
        {
            return ValidationResult.CreateWithError(ValidationResults.ValidationCancelled);
        }
    }

    private static ValidationResult ValidateIPAddress(IPAddress ipAddress)
    {
        // Loopback, link-local, multicast and broadcast addresses are in principle possible
        // but should be avoided and are for now treated as invalid.

        if (IPAddressValidatorUtils.IsLoopback(ipAddress, out var loopbackCIDRString))
        {
            return ValidationResult.CreateWithError(
                string.Format(
                    CultureInfo.InvariantCulture,
                    CompositeFormat.Parse(
                        DNSValidationResults.StaticHostIPAddressCanNotBeLoopback),
                    ipAddress.ToString(),
                    loopbackCIDRString));
        }

        if (IPAddressValidatorUtils.IsLinkLocal(ipAddress, out var linkLocalCIDRString))
        {
            return ValidationResult.CreateWithError(
                string.Format(
                    CultureInfo.InvariantCulture,
                    CompositeFormat.Parse(
                        DNSValidationResults.StaticHostIPAddressCanNotBeLinkLocal),
                    ipAddress.ToString(),
                    linkLocalCIDRString));
        }

        if (IPAddressValidatorUtils.IsMulticast(ipAddress, out var multicastCIDRString))
        {
            return ValidationResult.CreateWithError(
                string.Format(
                    CultureInfo.InvariantCulture,
                    CompositeFormat.Parse(
                        DNSValidationResults.StaticHostIPAddressCanNotBeMulticast),
                    ipAddress.ToString(),
                    multicastCIDRString));
        }

        if (IPAddressValidatorUtils.IsBroadcast(ipAddress, out var broadcastCIDRString))
        {
            return ValidationResult.CreateWithError(
                string.Format(
                    CultureInfo.InvariantCulture,
                    CompositeFormat.Parse(
                        DNSValidationResults.StaticHostIPAddressCanNotBeBroadcast),
                    ipAddress.ToString(),
                    broadcastCIDRString));
        }

        return ValidationResult.Valid;
    }
}

using System.Globalization;
using System.Net;
using System.Text;
using HostManagement.Shared.Validation.Utils;

namespace HostManagement.Shared.Validation.Validators.DNS;

/// <summary>
/// Represents the validator for a name server.
/// </summary>
public sealed class NameServerValidator : IValidator<IPAddress>
{
    /// <summary>
    /// Validates a name server asynchronously.
    /// </summary>
    /// <param name="nameServer">The name server.</param>
    /// <param name="cancellationToken">A token to cancel the validation.</param>
    /// <inheritdoc/>
    public Task<ValidationResult> ValidateAsync(
        IPAddress nameServer,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (IPAddressValidatorUtils.IsLinkLocal(nameServer, out var linkLocalCIDRString))
            {
                return Task.FromResult(
                    ValidationResult.CreateWithError(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            CompositeFormat.Parse(
                                DNSValidationResults.NameServerCanNotBeLinkLocal),
                            nameServer.ToString(),
                            linkLocalCIDRString)));
            }

            if (IPAddressValidatorUtils.IsMulticast(nameServer, out var multicastCIDRString))
            {
                return Task.FromResult(
                    ValidationResult.CreateWithError(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            CompositeFormat.Parse(
                                DNSValidationResults.NameServerCanNotBeMulticast),
                            nameServer.ToString(),
                            multicastCIDRString)));
            }

            if (IPAddressValidatorUtils.IsBroadcast(nameServer, out var broadcastCIDRString))
            {
                return Task.FromResult(
                    ValidationResult.CreateWithError(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            CompositeFormat.Parse(
                                DNSValidationResults.NameServerCanNotBeBroadcast),
                            nameServer.ToString(),
                            broadcastCIDRString)));
            }

            return Task.FromResult(ValidationResult.Valid);
        }
        catch (OperationCanceledException)
        {
            return Task.FromResult(
                ValidationResult.CreateWithError(
                    ValidationResults.ValidationCancelled));
        }
    }
}

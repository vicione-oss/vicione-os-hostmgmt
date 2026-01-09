using System.Globalization;
using System.Text;
using HostManagement.Shared.Contracts.Network;

namespace HostManagement.Shared.Validation.Validators.Proxy;

/// <summary>
/// Represents the validator for a no-proxy item's zone identifier.
/// </summary>
internal sealed class ZoneIdentifierValidator : IValidator<string, IEnumerable<NetworkInterfaceDetail>>
{
    private const string CustomNoProxyItemText = "no-proxy item";

    private readonly string _customNoProxyItemText;

    /// <summary>
    /// Initializes a new <see cref="ZoneIdentifierValidator"/>.
    /// </summary>
    /// <param name="noProxyItem">The no-proxy item, to be used in the validation result message.</param>
    public ZoneIdentifierValidator(string? noProxyItem = null)
    {
        if (string.IsNullOrWhiteSpace(noProxyItem))
            _customNoProxyItemText = CustomNoProxyItemText;
        else
            _customNoProxyItemText = CustomNoProxyItemText + $" '{noProxyItem}'";
    }

    /// <summary>
    /// Validates a zone identifier asynchronously.
    /// </summary>
    /// <param name="zoneIdentifier">The zone identifier.</param>
    /// <param name="interfaces">The list of network interfaces.</param>
    /// <param name="cancellationToken">A token to cancel the validation.</param>
    /// <inheritdoc/>
    public Task<ValidationResult> ValidateAsync(
        string zoneIdentifier,
        IEnumerable<NetworkInterfaceDetail> interfaces,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(zoneIdentifier))
            {
                return Task.FromResult(
                    ValidationResult.CreateWithError(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            CompositeFormat.Parse(
                                ProxyValidationResults.NoProxyItemZoneIdentifierEmpty),
                            _customNoProxyItemText)));
            }

            if (zoneIdentifier.Length > 15)
            {
                return Task.FromResult(
                    ValidationResult.CreateWithError(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            CompositeFormat.Parse(
                                ProxyValidationResults.NoProxyItemZoneIdentifierInvalidCharacterCount),
                            zoneIdentifier,
                            _customNoProxyItemText)));
            }

            foreach (var character in zoneIdentifier)
            {
                if (!char.IsAsciiLetterOrDigit(character) &&
                    character != '_' &&
                    character != '-')
                {
                    return Task.FromResult(
                        ValidationResult.CreateWithError(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                CompositeFormat.Parse(
                                    ProxyValidationResults.NoProxyItemZoneIdentifierHasInvalidCharacter),
                                zoneIdentifier,
                                _customNoProxyItemText)));
                }
            }

            /*
             * The IPv6 zone identifier must match the interface name as reported by the system
             * (without '@ens36' etc.).
             */
            if (!interfaces.Any(interfaceDetail => interfaceDetail.CommonInformation.Name.Equals(zoneIdentifier)))
            {
                return Task.FromResult(
                    ValidationResult.CreateWithError(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            CompositeFormat.Parse(
                                ProxyValidationResults.NoProxyItemZoneIdentifierHasUnknownInterfaceName),
                            zoneIdentifier,
                            _customNoProxyItemText)));
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

using System.Globalization;
using System.Text;

namespace HostManagement.Shared.Validation.Validators.Proxy;

/// <summary>
/// Represents the validator for a proxy port.
/// </summary>
internal sealed class ProxyPortValidator : IValidator<int>
{
    internal const int MinPort = 1;
    internal const int MaxPort = 65_535;

    /// <summary>
    /// Validates a proxy port asynchronously.
    /// </summary>
    /// <remarks>
    /// Port 0 is reserved by the system and cannot be used as a fixed proxy port. It is often used
    /// to indicate that the system should choose an available port dynamically.
    /// <para>
    /// Ports 1-1023 are known as well-known or system ports.They are typically reserved for
    /// standard services (e.g.: HTTP on 80, HTTPS on 443, FTP on 21) and usually require
    /// administrative privileges to bind to. Using these for a proxy can lead to conflicts or
    /// permission issues.
    /// </para>
    /// </remarks>
    /// <param name="port">The proxy port.</param>
    /// <param name="cancellationToken">A token to cancel the validation.</param>
    /// <inheritdoc/>
    public Task<ValidationResult> ValidateAsync(int port, CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (port < MinPort)
            {
                return Task.FromResult(
                    ValidationResult.CreateWithError(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            CompositeFormat.Parse(
                                ProxyValidationResults.PortCanNotBeSmallerThanMinimum),
                            port,
                            MinPort)));
            }

            if (port > MaxPort)
            {
                return Task.FromResult(
                    ValidationResult.CreateWithError(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            CompositeFormat.Parse(
                                ProxyValidationResults.PortCanNotBeGreaterThanMaximum),
                            port,
                            MaxPort)));
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

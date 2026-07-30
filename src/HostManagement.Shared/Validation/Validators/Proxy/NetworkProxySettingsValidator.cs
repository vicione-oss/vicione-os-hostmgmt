using HostManagement.Shared.Contracts.Network;

namespace HostManagement.Shared.Validation.Validators.Proxy;

/// <summary>
/// Represents the validator for <see cref="NetworkProxySettings"/>.
/// </summary>
public sealed class NetworkProxySettingsValidator
    : IValidator<NetworkProxySettings, IReadOnlyList<NetworkInterfaceDetail>>
{
    /// <summary>
    /// Validates the specified <see cref="NetworkProxySettings"/> asynchronously.
    /// </summary>
    /// <param name="proxySettings">The <see cref="NetworkProxySettings"/>.</param>
    /// <param name="interfaces">The list of network interfaces.</param>
    /// <param name="cancellationToken">A token to cancel the validation.</param>
    /// <inheritdoc/>
    public async Task<ValidationResult> ValidateAsync(
        NetworkProxySettings proxySettings,
        IReadOnlyList<NetworkInterfaceDetail> interfaces,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result = new ValidationResult();
            var proxyValidator = new ProxyValidator();

            result.AddResult(
                await proxyValidator
                    .ValidateAsync(proxySettings.HTTP, ProxyType.HTTP, cancellationToken)
                    .ConfigureAwait(false));

            result.AddResult(
                await proxyValidator
                    .ValidateAsync(proxySettings.HTTPS, ProxyType.HTTPS, cancellationToken)
                    .ConfigureAwait(false));

            result.AddResult(
                await proxyValidator
                    .ValidateAsync(proxySettings.FTP, ProxyType.FTP, cancellationToken)
                    .ConfigureAwait(false));

            result.AddResult(
                await proxyValidator
                    .ValidateAsync(proxySettings.SFTP, ProxyType.SFTP, cancellationToken)
                    .ConfigureAwait(false));

            result.AddResult(
                await proxyValidator
                    .ValidateAsync(proxySettings.SOCKS, ProxyType.SOCKS, cancellationToken)
                    .ConfigureAwait(false));

            if (proxySettings.NoProxy.Enabled)
            {
                result.AddResult(
                    await new NoProxyListValidator()
                        .ValidateAsync(proxySettings.NoProxy.Entries, interfaces, cancellationToken)
                        .ConfigureAwait(false));
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            return ValidationResult.CreateWithError(ValidationResults.ValidationCancelled);
        }
    }
}

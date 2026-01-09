using HostManagement.Shared.Contracts.Network;

namespace HostManagement.Shared.Validation.Validators.Proxy;

/// <summary>
/// Represents the validator for a <see cref="NetworkProxyDetail"/>.
/// </summary>
public sealed class ProxyValidator : IValidator<NetworkProxyDetail, ProxyType>
{
    /// <summary>
    /// Validates the specified proxy asynchronously.
    /// </summary>
    /// <param name="proxyDetail">The proxy detail.</param>
    /// <param name="proxyType">The type of the proxy (transport protocol).</param>
    /// <param name="cancellationToken">A token to cancel the validation.</param>
    /// <inheritdoc/>
    public async Task<ValidationResult> ValidateAsync(
        NetworkProxyDetail proxyDetail,
        ProxyType proxyType = ProxyType.Unspecified,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (proxyDetail.Equals(NetworkProxyDetail.Unconfigured))
                return ValidationResult.Valid;

            var result = new ValidationResult();

            if (proxyDetail.Server is not string server)
                return ValidationResult.CreateWithError(ProxyValidationResults.ServerCanNotBeNull);

            result.AddResult(
                await new ProxyServerValidator()
                    .ValidateAsync(server, proxyType, cancellationToken)
                    .ConfigureAwait(false));

            if (proxyDetail.Port is not int port)
                return ValidationResult.CreateWithError(ProxyValidationResults.PortCanNotBeNull);

            result.AddResult(
                await new ProxyPortValidator()
                    .ValidateAsync(port, cancellationToken)
                    .ConfigureAwait(false));

            if (proxyDetail.Username is null && proxyDetail.Password is null)
                return result;

            if (proxyDetail.Username is not string username)
                return ValidationResult.CreateWithError(ProxyValidationResults.UserCanNotBeNull);

            if (proxyDetail.Password is not string password)
                return ValidationResult.CreateWithError(ProxyValidationResults.PasswordCanNotBeNull);

            result.AddResult(
                await new ProxyAuthenticationValidator()
                    .ValidateAsync(username, password, cancellationToken)
                    .ConfigureAwait(false));

            return result;
        }
        catch (OperationCanceledException)
        {
            return ValidationResult.CreateWithError(ValidationResults.ValidationCancelled);
        }
    }
}

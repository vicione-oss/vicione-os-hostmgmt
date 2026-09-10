using System.Globalization;
using System.Text;

namespace HostManagement.Shared.Validation.Validators.Proxy;

/// <summary>
/// Represents the validator for a proxy server.
/// </summary>
public sealed class ProxyServerValidator : IValidator<string, ProxyType>
{
    private static readonly HashSet<string> s_transportProtocols = ["http", "https", "ftp", "sftp"];

    /// <summary>
    /// Validates the specified proxy server asynchronously.
    /// </summary>
    /// <param name="proxyServer">The proxy server.</param>
    /// <param name="proxyType">The type of the proxy (transport protocol).</param>
    /// <param name="cancellationToken">A token to cancel the validation.</param>
    /// <inheritdoc/>
    public Task<ValidationResult> ValidateAsync(
        string proxyServer,
        ProxyType proxyType = ProxyType.Unspecified,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(proxyServer))
            {
                return Task.FromResult(
                    ValidationResult.CreateWithError(
                        ProxyValidationResults.ServerCanNotBeEmpty));
            }

            if (proxyServer.Count(character => character == ':') > 1)
            {
                return Task.FromResult(
                    ValidationResult.CreateWithError(
                        ProxyValidationResults.ServerInvalid));
            }

            if (!proxyServer.Contains("://"))
            {
                return Task.FromResult(
                    ValidationResult.CreateWithError(
                        ProxyValidationResults.ServerInvalid));
            }

            var transportProtocol = proxyServer.Split(':')[0];

            if (transportProtocol is "")
            {
                return Task.FromResult(
                    ValidationResult.CreateWithError(
                        ProxyValidationResults.ServerTransportProtocolNotSpecified));
            }

            switch (proxyType)
            {
                case ProxyType.HTTP:
                case ProxyType.HTTPS:
                case ProxyType.FTP:
                case ProxyType.SFTP:

                    if (!transportProtocol.Equals(proxyType.ToString().ToLowerInvariant()))
                    {
                        return Task.FromResult(
                            ValidationResult.CreateWithError(
                                string.Format(
                                    CultureInfo.InvariantCulture,
                                    CompositeFormat.Parse(
                                        ProxyValidationResults.ServerTransportProtocolInvalid),
                                    proxyType.ToString().ToLowerInvariant(),
                                    transportProtocol)));
                    }

                    break;

                case ProxyType.Unspecified:
                default:

                    if (!s_transportProtocols.Contains(transportProtocol))
                    {
                        return Task.FromResult(
                            ValidationResult.CreateWithError(
                                string.Format(
                                    CultureInfo.InvariantCulture,
                                    CompositeFormat.Parse(
                                        ProxyValidationResults.ServerTransportProtocolInvalid),
                                    string.Join(", ", s_transportProtocols),
                                    transportProtocol)));
                    }

                    break;
            }

            if (!Uri.IsWellFormedUriString(proxyServer, UriKind.Absolute))
            {
                return Task.FromResult(
                    ValidationResult.CreateWithError(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            CompositeFormat.Parse(
                                ProxyValidationResults.ServerNotWellFormedUri),
                            proxyServer)));
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

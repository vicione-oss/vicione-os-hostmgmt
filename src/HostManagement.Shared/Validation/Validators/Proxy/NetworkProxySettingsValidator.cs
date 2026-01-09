using System.Globalization;
using System.Text;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;

namespace HostManagement.Shared.Validation.Validators.Proxy;

/// <summary>
/// Represents the validator for a <see cref="NetworkProxySettings"/> object.
/// </summary>
internal sealed class NetworkProxySettingsValidator : IValidator
{
    public Task Validate(
        SystemConfiguration current,
        SystemConfiguration proposed,
        ValidationResult result)
    {
        ValidateNetworkProxyDetail(["http"], proposed.NetworkProxySettings.HTTP, result);
        ValidateNetworkProxyDetail(["https"], proposed.NetworkProxySettings.HTTPS, result);
        ValidateNetworkProxyDetail(["ftp"], proposed.NetworkProxySettings.FTP, result);
        ValidateNetworkProxyDetail(["sftp"], proposed.NetworkProxySettings.SFTP, result);
        ValidateNetworkProxyDetail(["socks", "socks5"], proposed.NetworkProxySettings.SOCKS, result);

        DoNotProxyListValidator.Validate(
            proposed.NetworkInterfacesSettings.NetworkInterfaces,
            proposed.NetworkProxySettings.DoNotProxyListEnabled,
            proposed.NetworkProxySettings.DoNotProxyList,
            result);

        return Task.CompletedTask;
    }

    internal static void ValidateNetworkProxyDetail(
        string[] transportProtocol,
        NetworkProxyDetail networkProxyDetail,
        ValidationResult result)
    {
        if (IsProxyUnconfigured(networkProxyDetail))
            return;

        ValidateServer(transportProtocol, networkProxyDetail.Server, result);
        ProxyPortValidator.Validate(networkProxyDetail.Port, result);
        ValidateAuthentication(networkProxyDetail.Username, networkProxyDetail.Password, result);
    }

    internal static bool IsProxyUnconfigured(NetworkProxyDetail networkProxyDetail)
        => networkProxyDetail.Equals(NetworkProxyDetail.Empty);

    internal static void ValidateServer(
        string[] transportProtocol,
        string? server,
        ValidationResult result)
    {
        if (server is null)
        {
            result.AddErrorMessage(
                ValidationResultMessages.ProxyServerCanNotBeNull,
                ErrorCodes.ValidationFailed);

            return;
        }

        if (string.IsNullOrWhiteSpace(server))
        {
            result.AddErrorMessage(
                ValidationResultMessages.ProxyServerCanNotBeEmpty,
                ErrorCodes.ValidationFailed);

            return;
        }

        if (!server.Contains("://"))
        {
            result.AddErrorMessage(
                ValidationResultMessages.ProxyServerURLInvalid,
                ErrorCodes.ValidationFailed);

            return;
        }

        var proxyType = server.Split(':')[0];

        if (proxyType is "")
        {
            result.AddErrorMessage(
                ValidationResultMessages.ProxyServerTransportProtocolNotSpecified,
                ErrorCodes.ValidationFailed);

            return;
        }

        if (!transportProtocol.Contains(proxyType))
        {
            result.AddErrorMessage(
                string.Format(
                    CultureInfo.InvariantCulture,
                    CompositeFormat.Parse(ValidationResultMessages.ProxyServerTransportProtocolInvalid),
                    string.Join(", ", transportProtocol),
                    proxyType),
                ErrorCodes.ValidationFailed);

            return;
        }

        if (!Uri.IsWellFormedUriString(server, UriKind.Absolute))
        {
            result.AddErrorMessage(
                ValidationResultMessages.ProxyServerNotWellFormedUri,
                ErrorCodes.ValidationFailed);

            return;
        }
    }

    internal static void ValidateAuthentication(
        string? username,
        string? password,
        ValidationResult result)
    {
        if (username is null)
        {
            if (password is not null)
            {
                result.AddErrorMessage(
                    ValidationResultMessages.ProxyUserCanNotBeNullWhenPasswordGiven,
                    ErrorCodes.ValidationFailed);
            }

            return;
        }

        if (password is null)
        {
            result.AddErrorMessage(
                ValidationResultMessages.ProxyPasswordCanNotBeNullWhenUserGiven,
                ErrorCodes.ValidationFailed);

            return;
        }

        if (username.Length == 0 ||
            string.IsNullOrWhiteSpace(username))
        {
            result.AddErrorMessage(
                ValidationResultMessages.ProxyUserCanNotBeEmpty,
                ErrorCodes.ValidationFailed);
        }

        if (password.Length == 0 ||
            string.IsNullOrWhiteSpace(password))
        {
            result.AddErrorMessage(
                ValidationResultMessages.ProxyPasswordCanNotBeEmpty,
                ErrorCodes.ValidationFailed);
        }

        // TODO additional validation
        // special characters ?
    }
}

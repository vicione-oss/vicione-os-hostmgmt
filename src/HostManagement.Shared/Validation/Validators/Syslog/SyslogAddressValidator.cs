using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using HostManagement.Shared.Contracts.Syslog;
using HostManagement.Shared.Validation.Utils;
using HostManagement.Shared.Validation.Validators.DNS;

namespace HostManagement.Shared.Validation.Validators.Syslog;

/// <summary>
/// Represents the validator for a syslog destination address.
/// </summary>
public sealed class SyslogAddressValidator : IValidator<string, SyslogTransportProtocol>
{
    private const int MinPort = 1;
    private const int MaxPort = 65_535;
    private const string SyslogServerText = "Syslog server";

    /// <summary>
    /// Validates the specified syslog destination address asynchronously.
    /// </summary>
    /// <param name="address">The syslog destination in HOST:PORT or [IPv6]:PORT format.</param>
    /// <param name="transportProtocol">The syslog transport protocol.</param>
    /// <param name="cancellationToken">A token to cancel the validation.</param>
    /// <inheritdoc/>
    public async Task<ValidationResult> ValidateAsync(
        string address,
        SyslogTransportProtocol transportProtocol,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(address))
                return ValidationResult.CreateWithError(SyslogValidationResults.AddressCanNotBeEmpty);

            if (address.Any(char.IsControl))
            {
                return ValidationResult.CreateWithError(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        CompositeFormat.Parse(SyslogValidationResults.AddressHasControlCharacters),
                        address));
            }

            if (HasLeadingOrTrailingWhiteSpace(address) ||
                !TrySplitAddress(address, out var host, out var portString))
            {
                return CreateInvalidAddressResult(address);
            }

            if (!int.TryParse(portString, NumberStyles.None, CultureInfo.InvariantCulture, out var port) ||
                port is < MinPort or > MaxPort)
            {
                return ValidationResult.CreateWithError(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        CompositeFormat.Parse(SyslogValidationResults.AddressPortInvalid),
                        portString,
                        MinPort,
                        MaxPort));
            }

            var hasBrackets = address.StartsWith('[');

            if (IPAddress.TryParse(host, out var ipAddress))
                return ValidateIPAddress(address, ipAddress, hasBrackets, transportProtocol);

            if (hasBrackets ||
                IPAddressValidatorUtils.IsPotentialIPv4Address(host) ||
                host.Contains(':'))
            {
                return CreateInvalidAddressResult(address);
            }

            return await new DomainNameValidator(SyslogServerText)
                .ValidateAsync(host, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return ValidationResult.CreateWithError(ValidationResults.ValidationCancelled);
        }
    }

    private static bool HasLeadingOrTrailingWhiteSpace(string address)
        => !address.Equals(address.Trim(), StringComparison.Ordinal);

    private static bool TrySplitAddress(string address, out string host, out string port)
    {
        host = string.Empty;
        port = string.Empty;

        if (address.StartsWith('['))
        {
            var closingBracketIndex = address.IndexOf(']');

            if (closingBracketIndex <= 1 ||
                closingBracketIndex + 2 >= address.Length ||
                address[closingBracketIndex + 1] != ':')
            {
                return false;
            }

            host = address[1..closingBracketIndex];
            port = address[(closingBracketIndex + 2)..];

            return !host.Contains(']') && !port.Contains(':');
        }

        var portSeparatorIndex = address.LastIndexOf(':');

        if (portSeparatorIndex <= 0 || portSeparatorIndex == address.Length - 1)
            return false;

        host = address[..portSeparatorIndex];
        port = address[(portSeparatorIndex + 1)..];

        return !host.Contains(':');
    }

    private static ValidationResult ValidateIPAddress(
        string address,
        IPAddress ipAddress,
        bool hasBrackets,
        SyslogTransportProtocol transportProtocol)
    {
        var isIpv6 = ipAddress.AddressFamily == AddressFamily.InterNetworkV6;

        if (isIpv6 != hasBrackets)
            return CreateInvalidAddressResult(address);

        if (IPAddressValidatorUtils.IsMulticast(ipAddress, out _) &&
            Enum.IsDefined(transportProtocol) &&
            transportProtocol != SyslogTransportProtocol.Udp)
        {
            return ValidationResult.CreateWithError(
                string.Format(
                    CultureInfo.InvariantCulture,
                    CompositeFormat.Parse(SyslogValidationResults.MulticastRequiresUdp),
                    address));
        }

        return ValidationResult.Valid;
    }

    private static ValidationResult CreateInvalidAddressResult(string address)
        => ValidationResult.CreateWithError(
            string.Format(
                CultureInfo.InvariantCulture,
                CompositeFormat.Parse(SyslogValidationResults.AddressInvalidFormat),
                address));

}

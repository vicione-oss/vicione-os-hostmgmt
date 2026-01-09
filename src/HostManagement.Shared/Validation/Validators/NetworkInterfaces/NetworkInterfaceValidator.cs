using System.Globalization;
using System.Net;
using System.Text;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Validation.Utils;

namespace HostManagement.Shared.Validation.Validators.NetworkInterfaces;

/// <summary>
/// Represents the validator for a network interface.
/// </summary>
public sealed class NetworkInterfaceValidator : IValidator<NetworkInterfaceDetail>
{
    /// <summary>
    /// Validates a network interface asynchronously.
    /// </summary>
    /// <param name="networkInterface">The network interface.</param>
    /// <param name="cancellationToken">A token to cancel the validation.</param>
    /// <inheritdoc/>
    public async Task<ValidationResult> ValidateAsync(
        NetworkInterfaceDetail networkInterface,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result = new ValidationResult();

            // TODO Is this check required anymore?
            // When "enabled" is implemented this needs to be addressed!
            if (IsMacAddressConfigured(networkInterface.CommonInformation.PhysicalAddress))
            {
                result.AddResult(
                    await new MACAddressValidator(networkInterface.CommonInformation.Name)
                        .ValidateAsync(networkInterface.CommonInformation.PhysicalAddress, cancellationToken)
                        .ConfigureAwait(false));
            }

            if (IsLoopbackInterface(networkInterface))
                result.AddError(NetworkInterfacesValidationResults.LoopbackInterfaceCanNotBeSpecified);

            if (networkInterface.IPv4.VLANEnabled)
            {
                result.AddResult(
                    await new VLANIDValidator()
                        .ValidateAsync(networkInterface.IPv4.VLANID, cancellationToken)
                        .ConfigureAwait(false));
            }

            if (HasInvalidDHCPConfig(networkInterface))
            {
                result.AddError(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        CompositeFormat.Parse(
                            NetworkInterfacesValidationResults.InterfaceHasNoIPAddresses),
                        networkInterface.CommonInformation.Name));

                return result;
            }

            result.AddResult(
                await ValidateIPv4Details(networkInterface, cancellationToken).ConfigureAwait(false));

            return result;
        }
        catch (OperationCanceledException)
        {
            return ValidationResult.CreateWithError(ValidationResults.ValidationCancelled);
        }
    }

    private static bool IsMacAddressConfigured(string macAddress)
        => !string.IsNullOrEmpty(macAddress);

    private static bool IsLoopbackInterface(NetworkInterfaceDetail networkInterface)
        => networkInterface.CommonInformation.Name.ToLowerInvariant().Equals("lo");

    private static bool HasInvalidDHCPConfig(NetworkInterfaceDetail networkInterface)
        => !networkInterface.IPv4.DHCPEnabled && networkInterface.IPv4.IPv4Details.Count == 0;

    private static async Task<ValidationResult> ValidateIPv4Details(
        NetworkInterfaceDetail networkInterface,
        CancellationToken cancellationToken = default)
    {
        var result = new ValidationResult();
        var ipv4Details = networkInterface.IPv4.IPv4Details;

        var ipv4DetailValidator = new IPv4DetailValidator(networkInterface.CommonInformation.Name);

        for (var i = 0; i < ipv4Details.Count; i++)
        {
            result.AddResult(
                await ipv4DetailValidator
                    .ValidateAsync(ipv4Details[i], cancellationToken)
                    .ConfigureAwait(false));

            if (i == 0)
            {
                if (networkInterface.IPv4.Gateway is IPAddress gateway)
                {
                    var gatewayValidatorResult = await new GatewayValidator(networkInterface.CommonInformation.Name)
                        .ValidateAsync(gateway, cancellationToken)
                        .ConfigureAwait(false);

                    if (!gatewayValidatorResult.IsValid)
                    {
                        result.AddResult(gatewayValidatorResult);
                        continue;
                    }

                    var ip = ipv4Details[i].IPAddress;
                    var netmask = ipv4Details[i].Netmask;

                    if (gateway.Equals(ip))
                    {
                        result.AddError(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                CompositeFormat.Parse(
                                    NetworkInterfacesValidationResults.GatewayIdenticalWithIP),
                                networkInterface.CommonInformation.Name));
                    }

                    var isGatewayInDifferentSubnet = IPAddressValidatorUtils
                        .IsInDifferentSubnet(
                            ip,
                            gateway,
                            netmask,
                            out var ipNetworkAddressString,
                            out var gatewayNetworkAddressString);

                    if (isGatewayInDifferentSubnet)
                    {
                        result.AddError(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                CompositeFormat.Parse(
                                    NetworkInterfacesValidationResults.GatewayInDifferentSubnet),
                                networkInterface.CommonInformation.Name,
                                ipNetworkAddressString,
                                gatewayNetworkAddressString));
                    }

                    var isGatewaySubnetNetworkAddress = IPAddressValidatorUtils
                        .IsSubnetNetworkAddress(
                            gateway,
                            ip,
                            netmask,
                            out var networkAddressString);

                    if (isGatewaySubnetNetworkAddress)
                    {
                        result.AddError(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                CompositeFormat.Parse(
                                    NetworkInterfacesValidationResults.GatewayCanNotBeSubnetNetworkAddress),
                                networkInterface.CommonInformation.Name,
                                networkAddressString));
                    }

                    var isGatewaySubnetBroadcastAddress = IPAddressValidatorUtils
                        .IsSubnetBroadcastAddress(
                            gateway,
                            ip,
                            netmask,
                            out var broadcastAddressString);

                    if (isGatewaySubnetBroadcastAddress)
                    {
                        result.AddError(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                CompositeFormat.Parse(
                                    NetworkInterfacesValidationResults.GatewayCanNotBeSubnetBroadcastAddress),
                                networkInterface.CommonInformation.Name,
                                broadcastAddressString));
                    }
                }
            }
        }

        return result;
    }
}

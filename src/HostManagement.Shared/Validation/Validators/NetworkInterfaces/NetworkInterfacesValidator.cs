using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using HostManagement.Shared.Contracts.Network;

namespace HostManagement.Shared.Validation.Validators.NetworkInterfaces;

/// <summary>
/// Represents the validator for a list of network interfaces.
/// </summary>
public sealed class NetworkInterfacesValidator : IValidator<IReadOnlyList<NetworkInterfaceDetail>>
{
    /// <summary>
    /// Validates a list of network interfaces asynchronously.
    /// </summary>
    /// <param name="networkInterfaces">The list of network interfaces.</param>
    /// <param name="cancellationToken">A token to cancel the validation.</param>
    /// <inheritdoc/>
    public async Task<ValidationResult> ValidateAsync(
        IReadOnlyList<NetworkInterfaceDetail> networkInterfaces,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (networkInterfaces.Count == 0)
                return ValidationResult.CreateWithError(NetworkInterfacesValidationResults.NoNetworkInterfaces);

            if (HasDuplicateInterfaceNames(networkInterfaces, out var duplicateInterfaceNamesString))
            {
                return ValidationResult.CreateWithError(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        CompositeFormat.Parse(
                            NetworkInterfacesValidationResults.InterfacesHaveDuplicateNames),
                        duplicateInterfaceNamesString));
            }

            if (HasDuplicateMACAddresses(networkInterfaces, out var duplicateMACAddressesString))
            {
                return ValidationResult.CreateWithError(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        CompositeFormat.Parse(
                            NetworkInterfacesValidationResults.InterfacesHaveDuplicateMACs),
                        duplicateMACAddressesString));
            }

            if (HasMultipleDefaultGateways(networkInterfaces, out var interfacesWithDefaultGatewayString))
            {
                return ValidationResult.CreateWithError(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        CompositeFormat.Parse(
                            NetworkInterfacesValidationResults.InterfaceNonUniqueGateway),
                        interfacesWithDefaultGatewayString));
            }

            if (HasIPAddressesConflictingWithGateway(networkInterfaces, out var gatewayString, out var interfacesString))
            {
                return ValidationResult.CreateWithError(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        CompositeFormat.Parse(
                            NetworkInterfacesValidationResults.InterfacesHaveConflictingIPs),
                        gatewayString,
                        interfacesString));
            }

            if (HasDuplicateIPAddresses(networkInterfaces, out var duplicateIPAddressesString))
            {
                return ValidationResult.CreateWithError(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        CompositeFormat.Parse(
                            NetworkInterfacesValidationResults.InterfacesHaveDuplicateIPs),
                        duplicateIPAddressesString));
            }

            if (HasDuplicateVLANIDs(networkInterfaces, out var interfacesWithDuplicateVLANIDString))
            {
                return ValidationResult.CreateWithError(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        CompositeFormat.Parse(
                            NetworkInterfacesValidationResults.InterfacesHaveDuplicateVLANID),
                        interfacesWithDuplicateVLANIDString));
            }

            var result = new ValidationResult();
            var networkInterfaceValidator = new NetworkInterfaceValidator();

            foreach (var networkInterface in networkInterfaces)
            {
                result.AddResult(
                    await networkInterfaceValidator
                        .ValidateAsync(networkInterface, cancellationToken)
                        .ConfigureAwait(false));
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            return ValidationResult.CreateWithError(ValidationResults.ValidationCancelled);
        }
    }

    private static bool HasDuplicateInterfaceNames(
        IReadOnlyList<NetworkInterfaceDetail> networkInterfaces,
        [NotNullWhen(true)] out string? duplicateInterfaceNamesString)
    {
        duplicateInterfaceNamesString = null;

        var duplicateInterfaces = networkInterfaces
            .GroupBy(detail => detail.CommonInformation.Name)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        if (duplicateInterfaces.Count > 0)
        {
            duplicateInterfaceNamesString = string.Join(
                ", ",
                duplicateInterfaces.Select(name => $"'{name}'"));

            return true;
        }

        return false;
    }

    private static bool HasDuplicateMACAddresses(
        IReadOnlyList<NetworkInterfaceDetail> networkInterfaces,
        [NotNullWhen(true)] out string? duplicateMACAddressesString)
    {
        duplicateMACAddressesString = null;

        var duplicateMACAddresses = networkInterfaces
            .Where(detail => detail.CommonInformation.PhysicalAddress != string.Empty)
            .GroupBy(detail => detail.CommonInformation.PhysicalAddress)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        if (duplicateMACAddresses.Count > 0)
        {
            duplicateMACAddressesString = string.Join(
                ", ",
                duplicateMACAddresses.Select(address => $"'{address}'"));

            return true;
        }

        return false;
    }

    private static bool HasMultipleDefaultGateways(
        IReadOnlyList<NetworkInterfaceDetail> networkInterfaces,
        [NotNullWhen(true)] out string? interfacesWithDefaultGatewayString)
    {
        interfacesWithDefaultGatewayString = null;

        var interfacesWithGateway = networkInterfaces
            .Where(detail => detail.IPv4.Gateway is not null)
            .ToList();

        if (interfacesWithGateway.Count > 1)
        {
            interfacesWithDefaultGatewayString = string.Join(
                ", ",
                interfacesWithGateway.Select(@interface =>
                    $"'{@interface.CommonInformation.Name}' ({@interface.IPv4.Gateway})"));

            return true;
        }

        return false;
    }

    private static bool HasIPAddressesConflictingWithGateway(
        IReadOnlyList<NetworkInterfaceDetail> networkInterfaces,
        [NotNullWhen(true)] out string? defaultGatewayString,
        [NotNullWhen(true)] out string? conflictingInterfacesString)
    {
        defaultGatewayString = null;
        conflictingInterfacesString = null;

        var defaultGateway = networkInterfaces
            .Select(detail => detail.IPv4.Gateway)
            .FirstOrDefault(gateway => gateway is not null);

        if (defaultGateway is null)
            return false;

        defaultGatewayString = defaultGateway.ToString();

        var conflictingInterfaces = networkInterfaces
            .Where(detail => detail.IPv4.IPv4Details
                .Any(ipv4Detail => ipv4Detail.IPAddress.Equals(defaultGateway)))
            .ToList();

        if (conflictingInterfaces.Count > 0)
        {
            conflictingInterfacesString = string.Join(
                ", ",
                conflictingInterfaces.Select(detail => $"'{detail.CommonInformation.Name}'"));

            return true;
        }

        return false;
    }

    private static bool HasDuplicateIPAddresses(
        IReadOnlyList<NetworkInterfaceDetail> networkInterfaces,
        [NotNullWhen(true)] out string? duplicateIPAddressesString)
    {
        duplicateIPAddressesString = null;

        var duplicateIPAddresses = networkInterfaces
            .SelectMany(detail => detail.IPv4.IPv4Details)
            .GroupBy(ipv4Detail => ipv4Detail.IPAddress)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        if (duplicateIPAddresses.Count > 0)
        {
            duplicateIPAddressesString = string.Join(
                ", ",
                duplicateIPAddresses.Select(address => $"'{address}'"));

            return true;
        }

        return false;
    }

    private static bool HasDuplicateVLANIDs(
        IReadOnlyList<NetworkInterfaceDetail> networkInterfaces,
        [NotNullWhen(true)] out string? interfacesWithDuplicateVLANIDString)
    {
        interfacesWithDuplicateVLANIDString = null;

        var duplicateInterfaces = networkInterfaces
            .Where(detail => detail.IPv4.VLANEnabled)
            .GroupBy(detail => detail.IPv4.VLANID)
            .Where(group => group.Count() > 1)
            .SelectMany(group => group)
            .ToList();

        if (duplicateInterfaces.Count > 0)
        {
            interfacesWithDuplicateVLANIDString = string.Join(
                ", ",
                duplicateInterfaces.Select(detail => $"'{detail.CommonInformation.Name}'"));

            return true;
        }

        return false;
    }
}

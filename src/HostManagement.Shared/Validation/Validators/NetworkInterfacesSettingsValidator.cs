using System.Globalization;
using System.Net;
using System.Text;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;

namespace HostManagement.Shared.Validation.Validators;

/// <summary>
/// Represents the validator for a <see cref="NetworkInterfacesSettings"/> object.
/// </summary>
internal sealed class NetworkInterfacesSettingsValidator : IValidator
{
    public Task Validate(
        SystemConfiguration current,
        SystemConfiguration proposed,
        ValidationResult result)
    {
        var currentInterfaces = current.NetworkInterfacesSettings.NetworkInterfaces;
        var proposedInterfaces = proposed.NetworkInterfacesSettings.NetworkInterfaces;

        if (HasNoInterfaces(proposedInterfaces, result))
            return Task.CompletedTask;

        if (HasLoopbackInterfaces(proposedInterfaces, result))
            return Task.CompletedTask;

        if (HasUnknownInterfaces(currentInterfaces, proposedInterfaces, result))
            return Task.CompletedTask;

        if (HasNonUniqueInterfaceNames(proposedInterfaces, result))
            return Task.CompletedTask;

        if (HasNonUniqueIPAddresses(proposedInterfaces, result))
            return Task.CompletedTask;

        if (HasInvalidDHCPConfig(proposedInterfaces, result))
            return Task.CompletedTask;

        if (HasMultipleDefaultGateways(proposedInterfaces, result))
            return Task.CompletedTask;

        foreach (var proposedInterface in proposedInterfaces)
        {
            ValidateIPv4Details(proposedInterface, result);
            ValidateVLANID(proposedInterface, result);
        }

        return Task.CompletedTask;
    }

    internal static bool HasNoInterfaces(
        List<NetworkInterfaceDetail> proposedInterfaces,
        ValidationResult result)
    {
        if (proposedInterfaces.Count == 0)
        {
            result.AddErrorMessage(
                ValidationResultMessages.NoNetworkInterfaces,
                ErrorCodes.ValidationFailed);

            return true;
        }

        return false;
    }

    internal static bool HasLoopbackInterfaces(
        List<NetworkInterfaceDetail> proposedInterfaces,
        ValidationResult result)
    {
        var hasLoopbackInterface = proposedInterfaces
            .Any(detail => detail.CommonInformation.Name.ToLowerInvariant().Equals("lo"));

        if (hasLoopbackInterface)
        {
            result.AddErrorMessage(
                ValidationResultMessages.LoopbackInterfaceCanNotBeSpecified,
                ErrorCodes.ValidationFailed);

            return true;
        }

        return false;
    }

    internal static bool HasUnknownInterfaces(
        List<NetworkInterfaceDetail> currentInterfaces,
        List<NetworkInterfaceDetail> proposedInterfaces,
        ValidationResult result)
    {
        var hasUnknownInterfaces = false;

        foreach (var proposedInterface in proposedInterfaces)
        {
            var currentInterface = currentInterfaces
                .FirstOrDefault(detail =>
                    detail.CommonInformation.Name.Equals(proposedInterface.CommonInformation.Name));

            if (currentInterface is null)
            {
                result.AddErrorMessage(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        CompositeFormat.Parse(ValidationResultMessages.NetworkInterfaceNotFound),
                        proposedInterface.CommonInformation.Name),
                    ErrorCodes.ValidationFailed);

                hasUnknownInterfaces = true;
            }
        }

        return hasUnknownInterfaces;
    }

    internal static bool HasNonUniqueInterfaceNames(
        List<NetworkInterfaceDetail> proposedInterfaces,
        ValidationResult result)
    {
        var duplicateNames = proposedInterfaces
            .GroupBy(detail => detail.CommonInformation.Name)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        if (duplicateNames.Count > 0)
        {
            var duplicateNamesString = string.Join(", ", duplicateNames.Select(name => $"'{name}'"));

            result.AddErrorMessage(
                string.Format(
                    CultureInfo.InvariantCulture,
                    CompositeFormat.Parse(ValidationResultMessages.NetworkInterfaceNonUniqueName),
                    duplicateNamesString),
                ErrorCodes.ValidationFailed);

            return true;
        }

        return false;
    }

    internal static bool HasNonUniqueIPAddresses(
        List<NetworkInterfaceDetail> proposedInterfaces,
        ValidationResult result)
    {
        var duplicateIPAddresses = proposedInterfaces
            .SelectMany(detail => detail.IPv4.IPv4Details)
            .GroupBy(ipv4Detail => ipv4Detail.IPAddress)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        if (duplicateIPAddresses.Count > 0)
        {
            var duplicateIPAddressesString =
                string.Join(", ", duplicateIPAddresses.Select(address => $"'{address}'"));

            result.AddErrorMessage(
                string.Format(
                    CultureInfo.InvariantCulture,
                    CompositeFormat.Parse(ValidationResultMessages.NetworkInterfaceNonUniqueIP),
                    duplicateIPAddressesString),
                ErrorCodes.ValidationFailed);
        }

        return false;
    }

    // TODO Add unit tests
    internal static bool HasInvalidDHCPConfig(
        List<NetworkInterfaceDetail> proposedInterfaces,
        ValidationResult result)
    {
        var hasInvalidDHCPConfig = false;

        foreach (var proposedInterface in proposedInterfaces)
        {
            if (proposedInterface.IPv4.DHCPEnabled)
            {
                if (proposedInterface.IPv4.Gateway is not null)
                {
                    result.AddErrorMessage(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            CompositeFormat.Parse(ValidationResultMessages.NetworkInterfaceCanNotSetGatewayWhenDHCPEnabled),
                            proposedInterface.CommonInformation.Name),
                        ErrorCodes.ValidationFailed);

                    hasInvalidDHCPConfig = true;
                }

                if (proposedInterface.IPv4.IPv4Details.Count > 0)
                {
                    result.AddErrorMessage(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            CompositeFormat.Parse(ValidationResultMessages.NetworkInterfaceCanNotSetIPWhenDHCPEnabled),
                            proposedInterface.CommonInformation.Name),
                        ErrorCodes.ValidationFailed);

                    hasInvalidDHCPConfig = true;
                }
            }
            else
            {
                if (proposedInterface.IPv4.IPv4Details.Count == 0)
                {
                    result.AddErrorMessage(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            CompositeFormat.Parse(ValidationResultMessages.NetworkInterfaceNoIPAddressesSpecified),
                            proposedInterface.CommonInformation.Name),
                        ErrorCodes.ValidationFailed);

                    hasInvalidDHCPConfig = true;
                }
            }
        }

        return hasInvalidDHCPConfig;
    }

    internal static bool HasMultipleDefaultGateways(
        List<NetworkInterfaceDetail> proposedInterfaces,
        ValidationResult result)
    {
        var interfacesWithGateway = proposedInterfaces
            .Where(detail => detail.IPv4.Gateway is not null)
            .ToList();

        if (interfacesWithGateway.Count > 1)
        {
            var interfacesWithGatewayString =
                string.Join(
                    ", ",
                    interfacesWithGateway
                        .Select(detail => $"'{detail.CommonInformation.Name}' ({detail.IPv4.Gateway})"));

            result.AddErrorMessage(
                string.Format(
                    CultureInfo.InvariantCulture,
                    CompositeFormat.Parse(ValidationResultMessages.NetworkInterfaceNonUniqueGateway),
                    interfacesWithGatewayString),
                ErrorCodes.ValidationFailed);

            return true;
        }

        return false;
    }

    // TODO Add unit tests
    internal static void ValidateIPv4Details(
        NetworkInterfaceDetail networkInterfaceDetail,
        ValidationResult result)
    {
        foreach (var ipv4Detail in networkInterfaceDetail.IPv4.IPv4Details)
        {
            if (HasInvalidIPAddress(ipv4Detail.IPAddress))
            {
                result.AddErrorMessage(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        CompositeFormat.Parse(ValidationResultMessages.NetworkInterfaceInvalidIPAddress),
                        networkInterfaceDetail.CommonInformation.Name,
                        ipv4Detail.IPAddress),
                    ErrorCodes.ValidationFailed);
            }

            if (HasInvalidNetmask(ipv4Detail.IPAddress, ipv4Detail.Netmask))
            {
                result.AddErrorMessage(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        CompositeFormat.Parse(ValidationResultMessages.NetworkInterfaceInvalidNetmask),
                        networkInterfaceDetail.CommonInformation.Name,
                        ipv4Detail.IPAddress),
                    ErrorCodes.ValidationFailed);
            }
        }
    }

    internal static void ValidateVLANID(
        NetworkInterfaceDetail networkInterfaceDetail,
        ValidationResult result)
    {
        if (networkInterfaceDetail.IPv4.VLANID > 4094 ||
            (networkInterfaceDetail.IPv4.VLANEnabled &&
            networkInterfaceDetail.IPv4.VLANID < 1))
        {
            result.AddErrorMessage(
                string.Format(
                    CultureInfo.InvariantCulture,
                    CompositeFormat.Parse(ValidationResultMessages.NetworkInterfaceInvalidVLANID),
                    networkInterfaceDetail.CommonInformation.Name,
                    networkInterfaceDetail.IPv4.VLANID),
                ErrorCodes.ValidationFailed);

            return;
        }
    }

    internal static bool HasInvalidIPAddress(IPAddress ipAddress)
    {
        // TODO Validate IP address
        _ = ipAddress;
        return false;
    }

    internal static bool HasInvalidNetmask(IPAddress ipAddress, IPAddress netmask)
    {
        // TODO Validate netmask against IP address
        _ = ipAddress;
        _ = netmask;
        return false;
    }
}

using System.Globalization;
using System.Text;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Validation;

namespace HostManagement.SystemConfig.InternalValidation;

/// <summary>
/// Represents the internal validator for the list of services.
/// </summary>
internal sealed class ServicesValidatorInternal
{
    public static Task Validate(
        SystemConfiguration proposed,
        IEnumerable<string> configurableServices,
        ValidationResult result)
    {
        const char WildcardChar = '*';

        var configurableServicesWithoutWildcard = configurableServices
            .Where(service => !service.Contains(WildcardChar))
            .ToList();

        var notAllowedServices = proposed.Services
            .Where(serviceDetail => !configurableServicesWithoutWildcard.Contains(serviceDetail.Name))
            .ToList();

        if (notAllowedServices.Count > 0)
        {
            var invalidServicesString = string.Join(
                ", ",
                notAllowedServices.Select(serviceDetail => $"'{serviceDetail.Name}'"));

            result.AddError(
                string.Format(
                    CultureInfo.InvariantCulture,
                    CompositeFormat.Parse(InternalValidationResultMessages.ServicesNotAllowed),
                    invalidServicesString),
                ErrorCodes.ValidationFailed);
        }

        return Task.CompletedTask;
    }
}

using System.Globalization;
using System.Text;
using HostManagement.Shared.Contracts;

namespace HostManagement.SystemConfig.InternalValidation;

/// <summary>
/// Validates that all proposed services are explicitly listed as configurable.
/// Configurable service patterns containing wildcards are excluded from validation.
/// </summary>
internal sealed class ConfigurableServicesValidator
{
    /// <summary>
    /// Adds a validation error for every proposed service that is not explicitly configurable.
    /// </summary>
    /// <param name="proposed">The proposed system configuration.</param>
    /// <param name="configurableServices">The collection of configurable service names.</param>
    /// <param name="result">The validation result to which errors are added.</param>
    internal static void Validate(
        SystemConfiguration proposed,
        IReadOnlyCollection<string> configurableServices,
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
                    invalidServicesString));
        }
    }
}

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
    /// <returns>The validation result.</returns>
    internal static ValidationResult Validate(
        SystemConfiguration proposed,
        IReadOnlyCollection<string> configurableServices)
    {
        var result = new ValidationResult();

        try
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

            return result;
        }
        catch (Exception ex)
        {
            result.AddError(
                InternalValidationResultMessages.InternalValidationFailed +
                Environment.NewLine +
                ex.Message);

            return result;
        }
    }
}

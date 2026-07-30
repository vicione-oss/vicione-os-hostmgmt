using HostManagement.Configuration;
using HostManagement.Shared.Contracts;

namespace HostManagement.SystemConfig.InternalValidation;

/// <summary>
/// Validates a <see cref="SystemConfiguration"/> against HostManagement-specific policies.
/// </summary>
internal static class SystemConfigurationPolicyValidator
{
    /// <summary>
    /// Validates the <paramref name="proposed"/> <see cref="SystemConfiguration"/> against HostManagement-specific policies.
    /// </summary>
    /// <param name="proposed">The proposed <see cref="SystemConfiguration"/>.</param>
    /// <returns><see cref="ValidationResult"/></returns>
    internal static ValidationResult Validate(SystemConfiguration proposed)
    {
        var result = new ValidationResult();

        try
        {
            ConfigurableServicesValidator.Validate(
                proposed,
                EnvironmentVariables.ConfigurableServices.Value,
                result);
        }
        catch (Exception ex)
        {
            result.AddError(
                InternalValidationResultMessages.InternalValidationFailed +
                Environment.NewLine +
                ex.Message);
        }

        return result;
    }
}

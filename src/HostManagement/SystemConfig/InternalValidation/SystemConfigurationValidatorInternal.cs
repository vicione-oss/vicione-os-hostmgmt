using HostManagement.Configuration;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Validation;

namespace HostManagement.SystemConfig.InternalValidation;

/// <summary>
/// Represents the internal validator for a <see cref="SystemConfiguration"/> object.
/// </summary>
internal static class SystemConfigurationValidatorInternal
{
    /// <summary>
    /// Internally validates the <paramref name="proposed"/> <see cref="SystemConfiguration"/>.
    /// </summary>
    /// <param name="proposed">The proposed <see cref="SystemConfiguration"/>.</param>
    /// <returns><see cref="ValidationResult"/></returns>
    public static async Task<ValidationResult> Validate(SystemConfiguration proposed)
    {
        var validationResult = new ValidationResult();

        try
        {
            await ServicesValidatorInternal.Validate(
                proposed,
                EnvironmentVariables.ConfigurableServices,
                validationResult);

            return validationResult;
        }
        catch (Exception ex)
        {
            validationResult.AddErrorMessage(
                InternalValidationResultMessages.InternalValidationFailed +
                Environment.NewLine +
                ex.Message,
                ErrorCodes.ValidationFailed);

            return validationResult;
        }
    }
}

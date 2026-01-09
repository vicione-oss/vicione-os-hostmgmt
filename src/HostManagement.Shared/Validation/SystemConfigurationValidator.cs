using HostManagement.Shared.Contracts;
using HostManagement.Shared.Validation.Validators;
using HostManagement.Shared.Validation.Validators.DNS;

namespace HostManagement.Shared.Validation;

/// <summary>
/// Represents the validator for a <see cref="SystemConfiguration"/> object.
/// </summary>
public static class SystemConfigurationValidator
{
    private static readonly List<IValidator> s_validators =
    [
        new NetworkInterfacesSettingsValidator(),
        new NetworkDNSSettingsValidator(),
        new NetworkProxySettingsValidator(),
        new NetworkNTPSettingsValidator()
    ];

    /// <summary>
    /// Validates the <paramref name="proposed"/> <see cref="SystemConfiguration"/> taking into
    /// account the <paramref name="current"/> <see cref="SystemConfiguration"/>.
    /// </summary>
    /// <param name="current">The current <see cref="SystemConfiguration"/>.</param>
    /// <param name="proposed">The proposed <see cref="SystemConfiguration"/>.</param>
    /// <returns><see cref="ValidationResult"/></returns>
    public static async Task<ValidationResult> Validate(
        SystemConfiguration current,
        SystemConfiguration proposed)
    {
        var validationResult = new ValidationResult();

        foreach (var validator in s_validators)
            await validator.Validate(current, proposed, validationResult);

        return validationResult;
    }
}

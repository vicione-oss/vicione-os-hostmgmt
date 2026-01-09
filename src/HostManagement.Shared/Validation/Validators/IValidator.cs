using HostManagement.Shared.Contracts;

namespace HostManagement.Shared.Validation.Validators;

/// <summary>
/// Represents a validator.
/// </summary>
internal interface IValidator
{
    /// <summary>
    /// Validates the <paramref name="proposed"/> <see cref="SystemConfiguration"/> taking into
    /// account the <paramref name="current"/> <see cref="SystemConfiguration"/>.
    /// </summary>
    /// <param name="current">The current system configuration.</param>
    /// <param name="proposed">The proposed system configuration.</param>
    /// <param name="result">The validation result to append validation messages to.</param>
    Task Validate(SystemConfiguration current, SystemConfiguration proposed, ValidationResult result);
}

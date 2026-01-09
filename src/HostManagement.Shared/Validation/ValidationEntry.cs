namespace HostManagement.Shared.Validation;

/// <summary>
/// Represents a validation entry, used in a <see cref="ValidationResult"/>.
/// </summary>
/// <param name="Message">The message.</param>
/// <param name="Code">The error code.</param>
public record ValidationEntry(string Message, ErrorCodes Code = ErrorCodes.ValidationFailed);

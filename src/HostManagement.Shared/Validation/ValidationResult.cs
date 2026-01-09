namespace HostManagement.Shared.Validation;

/// <summary>
/// Represents the result of a validation.
/// </summary>
public class ValidationResult
{
    private readonly List<ValidationEntry> _entries = [];

    /// <summary>
    /// The list of validation entries.
    /// </summary>
    public IReadOnlyCollection<ValidationEntry> Entries => _entries.AsReadOnly();

    /// <summary>
    /// A string of the validation entries joined with <see cref="Environment.NewLine"/> with the
    /// format "ERROR_CODE: ERROR_MESSAGE".
    /// </summary>
    public string EntriesString => string.Join(
        Environment.NewLine,
        _entries.Select(entry => $"{entry.Code}: {entry.Message}"));

    /// <summary>
    /// Indicates if the validation result is valid.
    /// </summary>
    public bool IsValid { get; private set; } = true;

    /// <summary>
    /// Creates a new <see cref="ValidationResult"/> and sets <see cref="IsValid"/> to <see langword="true"/>.
    /// </summary>
    internal static ValidationResult Valid => new();

    /// <summary>
    /// Creates a new List of type <see cref="ValidationEntry"/> of all validation entries.
    /// </summary>
    public List<ValidationEntry> ToList() => [.. _entries];

    /// <summary>
    /// Adds a new <see cref="ValidationEntry"/> with the specified error message and optional
    /// error code to the list of validation entries and sets <see cref="IsValid"/> to <see langword="false"/>.
    /// </summary>
    /// <param name="message">The error message used for the validation entry.</param>
    /// <param name="code">The error code used for the validation entry.</param>
    public void AddError(string message, ErrorCodes code = ErrorCodes.ValidationFailed)
    {
        _entries.Add(new ValidationEntry(message, code));
        IsValid = false;
    }

    /// <summary>
    /// Adds a new <see cref="ValidationEntry"/> with the specified information message to the list
    /// of validation entries and sets <see cref="IsValid"/> to <see langword="true"/>.
    /// </summary>
    /// <param name="message">The information message used for the validation entry.</param>
    public void AddInformation(string message)
    {
        _entries.Add(new ValidationEntry(message, ErrorCodes.Valid));
        IsValid = true;
    }

    /// <summary>
    /// Creates a new <see cref="ValidationResult"/> and adds the specified error message and
    /// optional error code.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="errorCode">The error code.</param>
    /// <returns><see cref="ValidationResult"/></returns>
    internal static ValidationResult CreateWithError(
        string message,
        ErrorCodes errorCode = ErrorCodes.ValidationFailed)
    {
        var result = new ValidationResult();
        result.AddError(message, errorCode);

        return result;
    }

    /// <summary>
    /// Creates a new <see cref="ValidationResult"/> and adds the specified information message.
    /// </summary>
    /// <param name="information">The information message.</param>
    /// <returns><see cref="ValidationResult"/></returns>
    internal static ValidationResult CreateWithInformation(string information)
    {
        var result = new ValidationResult();
        result.AddInformation(information);

        return result;
    }

    /// <summary>
    /// Adds a <see cref="ValidationResult"/> to the current validation result.
    /// </summary>
    /// <param name="validationResult">The validation result.</param>
    internal void AddResult(ValidationResult validationResult)
    {
        ArgumentNullException.ThrowIfNull(validationResult);

        foreach (var entry in validationResult.Entries)
            _entries.Add(entry);

        IsValid = IsValid && validationResult.IsValid;
    }
}

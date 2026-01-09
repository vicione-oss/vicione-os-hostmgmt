namespace HostManagement.Shared.Validation;

/// <summary>
/// Represents a <see cref="ValidationResult"/>.
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
    public string EntriesString
        => string.Join(Environment.NewLine, _entries.Select(entry => $"{entry.Code}: {entry.Message}"));


    /// <summary>
    /// Indicates if the validation result is valid.
    /// </summary>
    public bool IsValid { get; private set; } = true;

    /// <summary>
    /// Adds a new <see cref="ValidationEntry"/> with the specified error message and error code to
    /// the list of validation entries and sets <see cref="IsValid"/> to <see langword="false"/>.
    /// </summary>
    /// <param name="message">The error message used for the validation entry.</param>
    /// <param name="code">The error code used for the validation entry.</param>
    public void AddErrorMessage(string message, ErrorCodes code)
    {
        _entries.Add(new ValidationEntry(message, code));
        IsValid = false;
    }

    /// <summary>
    /// Adds a new <see cref="ValidationEntry"/> with the specified information message to the list
    /// of validation entries.
    /// </summary>
    /// <param name="message">The information message used for the validation entry.</param>
    /// <remarks>The error code of the <see cref="ValidationEntry"/> will be set to <see cref="ErrorCodes.Valid"/>.</remarks>
    public void AddInformation(string message)
        => _entries.Add(new ValidationEntry(message, ErrorCodes.Valid));

    /// <summary>
    /// Creates a new List of type <see cref="ValidationEntry"/> of all validation entries.
    /// </summary>
    public List<ValidationEntry> ToList() => new(_entries);

}

/// <summary>
/// Represents a <see cref="ValidationEntry"/>.
/// </summary>
/// <param name="Message">The message.</param>
/// <param name="Code">The error code.</param>
public record ValidationEntry(string Message, ErrorCodes Code);

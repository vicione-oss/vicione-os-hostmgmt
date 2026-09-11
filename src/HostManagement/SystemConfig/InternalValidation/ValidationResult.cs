namespace HostManagement.SystemConfig.InternalValidation;

/// <summary>
/// Represents a <see cref="ValidationResult"/>.
/// </summary>
internal sealed class ValidationResult
{
    private readonly List<string> _messages = [];

    /// <summary>
    /// The list of result messages.
    /// </summary>
    internal IReadOnlyCollection<string> Messages => _messages.AsReadOnly();

    /// <summary>
    /// A string of result messages joined with <see cref="Environment.NewLine"/>.
    /// </summary>
    internal string FormattedMessages => string.Join(Environment.NewLine, _messages);

    /// <summary>
    /// Indicates if the validation result is valid.
    /// </summary>
    internal bool IsValid { get; private set; } = true;

    /// <summary>
    /// Adds the specified message to the list of Messages and sets <see cref="IsValid"/> to <see langword="false"/>.
    /// </summary>
    /// <param name="message">The message.</param>
    internal void AddError(string message)
    {
        _messages.Add(message);
        IsValid = false;
    }
}

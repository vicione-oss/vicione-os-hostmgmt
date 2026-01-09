namespace HostManagement.Scripts;

/// <summary>
/// Represents errors that occur during process start.
/// </summary>
[Serializable]
public class ProcessWrapperStartException : Exception
{
    /// <summary>
    /// Initializes a new <see cref="ProcessWrapperStartException"/>.
    /// </summary>
    public ProcessWrapperStartException()
    { }

    /// <summary>
    /// Initializes a new <see cref="ProcessWrapperStartException"/>.
    /// </summary>
    /// <inheritdoc/>
    public ProcessWrapperStartException(string? message)
        : base(message)
    { }

    /// <summary>
    /// Initializes a new <see cref="ProcessWrapperStartException"/>.
    /// </summary>
    /// <inheritdoc/>
    public ProcessWrapperStartException(string? message, Exception? innerException)
        : base(message, innerException)
    { }
}

namespace HostManagement.SystemConfig;

/// <summary>
/// The exception that is thrown when the system configuration is invalid.
/// </summary>
/// <remarks>
/// The system configuration is invalid, if the deserialized JSON config is <see langword="null"/>,
/// the JSON syntax is invalid or the file is defective. 
/// </remarks>
[Serializable]
internal class InvalidSystemConfigurationException : Exception
{
    /// <summary>
    /// Initializes a new <see cref="InvalidSystemConfigurationException"/>.
    /// </summary>
    public InvalidSystemConfigurationException()
    { }

    /// <summary>
    /// Initializes a new <see cref="InvalidSystemConfigurationException"/>.
    /// </summary>
    /// <inheritdoc/>
    public InvalidSystemConfigurationException(string? message)
        : base(message)
    { }

    /// <summary>
    /// Initializes a new <see cref="InvalidSystemConfigurationException"/>.
    /// </summary>
    /// <inheritdoc/>
    public InvalidSystemConfigurationException(string? message, Exception? innerException)
        : base(message, innerException)
    { }
}

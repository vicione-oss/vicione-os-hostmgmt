namespace HostManagement.Configuration;

/// <summary>
/// Represents a generic environment variable.
/// </summary>
/// <typeparam name="T">The type of the environment variable's value.</typeparam>
public interface IEnvironmentVariable<T> : IEnvironmentVariable
{
    /// <summary>
    /// The value of the environment variable, e.g.: "vicione-suite" (for key "HOSTMANAGEMENT:PIPE:GROUP").
    /// </summary>
    T Value { get; }
}

/// <summary>
/// Represents an environment variable.
/// </summary>
public interface IEnvironmentVariable
{
    /// <summary>
    /// The key of the environment variable, e.g.: "HOSTMANAGEMENT:PIPE:GROUP".
    /// </summary>
    string Key { get; }

    /// <summary>
    /// A value indicating whether the current instance is in an invalid state.
    /// </summary>
    bool IsInvalid { get; }

    /// <summary>
    /// A value of the invalid environment variable, with ":" replaced by "__".
    /// </summary>
    /// <remarks>
    /// If the environment variable is indexed, the value contains all invalid parts (one per line).
    /// </remarks>
    string InvalidVariableString { get; }
}

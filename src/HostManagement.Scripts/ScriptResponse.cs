namespace HostManagement.Scripts;

/// <summary>
/// Represents a script response.
/// </summary>
/// <param name="exitCode">The exit code returned from the executed script.</param>
/// <param name="standardOutput">The standard output returned from the executed script.</param>
/// <param name="standardError">The standard error returned from the executed script.</param>
public sealed class ScriptResponse(int exitCode, string standardOutput, string standardError)
{
    /// <summary>
    /// The exit code returned from the script.
    /// </summary>
    public int ExitCode { get; } = exitCode;

    /// <summary>
    /// The standard output returned from the script.
    /// </summary>
    public string StandardOutput { get; } = standardOutput;

    /// <summary>
    /// The standard error returned from the script.
    /// </summary>
    public string StandardError { get; } = standardError;

    /// <summary>
    /// Determines if the script response has errors (<see cref="ExitCode"/> > 0).
    /// </summary>
    public bool HasErrors => ExitCode > 0;

    /// <summary>
    /// Determines if the script response has warnings (<see cref="ExitCode"/> == 0 and
    /// <see cref="StandardError"/> not null or whitespace).
    /// </summary>
    public bool HasWarnings => ExitCode == 0 && !string.IsNullOrWhiteSpace(StandardError);

    /// <summary>
    /// Creates a new <see cref="ScriptResponse"/> with the specified error message and optional exit code.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="exitCode">The exit code.</param>
    /// <returns><see cref="ScriptResponse"/></returns>
    internal static ScriptResponse CreateErrorResponse(string message, int exitCode = 1)
        => new(exitCode, string.Empty, message);
}

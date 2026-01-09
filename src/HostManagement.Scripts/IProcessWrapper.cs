namespace HostManagement.Scripts;

/// <summary>
/// Represents a process wrapper.
/// </summary>
public interface IProcessWrapper
{
    /// <summary>
    /// Starts the process.
    /// </summary>
    /// <param name="fileName">The application to start.</param>
    /// <param name="arguments">The arguments.</param>
    /// <returns><see cref="ScriptResponse"/></returns>
    ScriptResponse Start(string fileName, string arguments);
}

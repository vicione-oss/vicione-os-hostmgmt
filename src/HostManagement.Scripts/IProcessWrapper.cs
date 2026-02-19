using System.Diagnostics;

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

    /// <summary>
    /// Starts the process to execute the provided script.
    /// </summary>
    /// <param name="processStartInfo">The <see cref="ProcessStartInfo"/> that contains the information
    /// that is used to start the process, including the file name and any command-line arguments.</param>
    /// <param name="timeoutInMilliseconds">The timeout in milliseconds.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><see cref="ScriptResponse"/></returns>
    Task<ScriptResponse> StartScriptAsync(
        ProcessStartInfo processStartInfo,
        int timeoutInMilliseconds,
        CancellationToken cancellationToken = default);
}

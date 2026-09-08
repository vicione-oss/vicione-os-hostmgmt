using HostManagement.Scripts;
using HostManagement.Scripts.Enums;

namespace HostManagement.Communication.NamedPipes.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="PipeServer"/>.
/// </summary>
internal static class PipeServerExtensions
{
    /// <summary>
    /// Sets the execute permissions of the named pipe.
    /// </summary>
    /// <param name="pipeServer">The pipe server.</param>
    /// <exception cref="IOException"></exception>
    internal static async Task SetExecutePermission(this PipeServer pipeServer)
    {
        var scriptOptions = new ScriptOptions();
        scriptOptions.Add(ScriptOptionNames.SetExecutePermission.File, pipeServer.PipePath);

        var scriptResponse = await ScriptRunner
            .GetInstance()
            .Run(ScriptIdentifier.SetExecutePermission, scriptOptions.Options);

        if (scriptResponse.HasErrors)
            throw new IOException(scriptResponse.StandardError);
    }

    /// <summary>
    /// Sets the ownership of the named pipe.
    /// </summary>
    /// <param name="pipeServer">The pipe server.</param>
    /// <exception cref="IOException"></exception>
    internal static async Task SetOwnership(this PipeServer pipeServer)
    {
        var scriptOptions = new ScriptOptions();
        scriptOptions.Add(ScriptOptionNames.SetPipeOwnership.File, pipeServer.PipePath);

        var scriptResponse = await ScriptRunner
            .GetInstance()
            .Run(ScriptIdentifier.SetPipeOwnership, scriptOptions.Options);

        if (scriptResponse.HasErrors)
            throw new IOException(scriptResponse.StandardError);
    }
}

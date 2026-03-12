using HostManagement.Configuration;
using HostManagement.Scripts;
using HostManagement.Scripts.Enums;
using Microsoft.Extensions.Logging;

namespace HostManagement.Communication.NamedPipes.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="PipeServer"/>.
/// </summary>
internal static partial class PipeServerExtensions
{
    /// <summary>
    /// Sets the execute permissions of the named pipe.
    /// </summary>
    /// <param name="pipeServer">The pipe server.</param>
    /// <exception cref="IOException"></exception>
    internal static async Task SetExecutePermission(this PipeServer pipeServer)
    {
        var pipePath = Path.Combine(AppPaths.TempPath, $"CoreFxPipe_{pipeServer.PipeName}");
        var scriptOptions = new ScriptOptions();

        scriptOptions.Add(
            ScriptOptionNames.SetExecutePermission.Permission,
            EnvironmentVariables.NamedPipePermission.Value);

        scriptOptions.Add(ScriptOptionNames.SetExecutePermission.File, pipePath);

        var scriptResponse = await ScriptRunner.GetInstance().Run(
            ScriptIdentifier.SetExecutePermission,
            scriptOptions.Options);

        if (scriptResponse.HasErrors)
        {
            throw new IOException(
                $"An error occurred while setting execute permission for '{pipePath}'." +
                Environment.NewLine +
                scriptResponse.StandardError);
        }
    }

    /// <summary>
    /// Sets the ownership of the named pipe.
    /// </summary>
    /// <param name="pipeServer">The pipe server.</param>
    /// <exception cref="IOException"></exception>
    internal static async Task SetOwnership(this PipeServer pipeServer)
    {
        var user = EnvironmentVariables.NamedPipeUser.Value;
        var group = EnvironmentVariables.NamedPipeGroup.Value;

        if (!await ExistsUser(EnvironmentVariables.NamedPipeUser.Value))
        {
            user = "root";

            NamedPipeUserDoesNotExist(pipeServer.Logger, EnvironmentVariables.NamedPipeUser.Value);
        }

        if (!await ExistsGroup(EnvironmentVariables.NamedPipeGroup.Value))
        {
            group = "root";

            NamedPipeGroupDoesNotExist(pipeServer.Logger, EnvironmentVariables.NamedPipeGroup.Value);
        }

        var pipePath = Path.Combine(AppPaths.TempPath, $"CoreFxPipe_{pipeServer.PipeName}");
        var scriptOptions = new ScriptOptions();

        scriptOptions.Add(
            ScriptOptionNames.SetOwnership.User,
            user);

        scriptOptions.Add(
            ScriptOptionNames.SetOwnership.Group,
            group);

        scriptOptions.Add(ScriptOptionNames.SetOwnership.File, pipePath);

        var scriptResponse = await ScriptRunner.GetInstance().Run(
            ScriptIdentifier.SetOwnership,
            scriptOptions.Options);

        if (scriptResponse.HasErrors)
        {
            throw new IOException(
                $"An error occurred while setting ownership for '{pipePath}'." +
                Environment.NewLine +
                scriptResponse.StandardError);
        }
    }

    private static async Task<bool> ExistsUser(string user)
    {
        var scriptOptions = new ScriptOptions();

        scriptOptions.Add(
            ScriptOptionNames.CheckUserExists.User,
            user);

        var scriptResponse = await ScriptRunner.GetInstance().Run(
            ScriptIdentifier.CheckUserExists,
            scriptOptions.Options);

        return !scriptResponse.HasErrors;
    }

    private static async Task<bool> ExistsGroup(string group)
    {
        var scriptOptions = new ScriptOptions();

        scriptOptions.Add(
            ScriptOptionNames.CheckGroupExists.Group,
            group);

        var scriptResponse = await ScriptRunner.GetInstance().Run(
            ScriptIdentifier.CheckGroupExists,
            scriptOptions.Options);

        return !scriptResponse.HasErrors;
    }

    [LoggerMessage(LogLevel.Information, "User '{NamedPipeUser}' does not exist, using 'root' instead.")]
    private static partial void NamedPipeUserDoesNotExist(ILogger logger, string namedPipeUser);

    [LoggerMessage(LogLevel.Information, "Group '{NamedPipeGroup}' does not exist, using 'root' instead.")]
    private static partial void NamedPipeGroupDoesNotExist(ILogger logger, string namedPipeGroup);
}

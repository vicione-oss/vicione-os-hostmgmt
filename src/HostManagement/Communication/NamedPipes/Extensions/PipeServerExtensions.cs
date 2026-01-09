using HostManagement.Configuration;
using HostManagement.Scripts;
using HostManagement.Scripts.Enums;
using HostManagement.Scripts.ScriptOptionNames;
using Microsoft.Extensions.Logging;

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
        var pipePath = Path.Combine(AppPaths.TempPath, $"CoreFxPipe_{pipeServer.PipeName}");
        var scriptOptions = new ScriptOptions();

        scriptOptions.Add(
            SetExecutePermissionScriptOptionName.Permission,
            EnvironmentVariables.NamedPipePermission);

        scriptOptions.Add(SetExecutePermissionScriptOptionName.File, pipePath);

        var scriptResponse = await ScriptRunner.Run(
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
        var user = EnvironmentVariables.NamedPipeUser;
        var group = EnvironmentVariables.NamedPipeGroup;

        if (!await ExistsUser(EnvironmentVariables.NamedPipeUser))
        {
            user = "root";

            pipeServer.Logger.LogInformation(
                $"User '{EnvironmentVariables.NamedPipeUser}' does not exist, using 'root' instead.");
        }

        if (!await ExistsGroup(EnvironmentVariables.NamedPipeGroup))
        {
            group = "root";

            pipeServer.Logger.LogInformation(
                $"Group '{EnvironmentVariables.NamedPipeGroup}' does not exist, using 'root' instead.");
        }

        var pipePath = Path.Combine(AppPaths.TempPath, $"CoreFxPipe_{pipeServer.PipeName}");
        var scriptOptions = new ScriptOptions();

        scriptOptions.Add(
            SetOwnershipScriptOptionName.User,
            user);

        scriptOptions.Add(
            SetOwnershipScriptOptionName.Group,
            group);

        scriptOptions.Add(SetOwnershipScriptOptionName.File, pipePath);

        var scriptResponse = await ScriptRunner.Run(
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
            CheckUserExistsScriptOptionName.User,
            user);

        var scriptResponse = await ScriptRunner.Run(
            ScriptIdentifier.CheckUserExists,
            scriptOptions.Options);

        return !scriptResponse.HasErrors;
    }

    private static async Task<bool> ExistsGroup(string group)
    {
        var scriptOptions = new ScriptOptions();

        scriptOptions.Add(
            CheckGroupExistsScriptOptionName.Group,
            group);

        var scriptResponse = await ScriptRunner.Run(
            ScriptIdentifier.CheckGroupExists,
            scriptOptions.Options);

        return !scriptResponse.HasErrors;
    }
}

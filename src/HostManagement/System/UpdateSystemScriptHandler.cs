using HostManagement.Scripts;
using HostManagement.Scripts.Enums;
using HostManagement.Scripts.ScriptOptionNames;
using HostManagement.Shared.Communication.Enums;

namespace HostManagement.System;

/// <inheritdoc cref="IUpdateSystemScriptHandler"/>
/// <param name="result">The result of the operation.</param>
internal sealed class UpdateSystemScriptHandler(Result result) : IUpdateSystemScriptHandler
{
    private readonly Result _result = result;

    public async Task<OperationStatus> ExecuteUpdateSystemScript(string filePath)
    {
        var scriptOptions = new ScriptOptions();

        scriptOptions.Add(
            UpdateSystemScriptOptionName.FilePath,
            filePath);

        var scriptResponse = await ScriptRunner
            .GetInstance()
            .Run(ScriptIdentifier.UpdateSystem, scriptOptions.Options);

        if (scriptResponse.HasErrors)
        {
            _result.AddScriptError(
                ScriptIdentifier.UpdateSystem,
                scriptResponse.StandardError);

            return OperationStatus.Error;
        }

        return OperationStatus.Success;
    }
}

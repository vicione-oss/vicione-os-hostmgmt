using HostManagement.Scripts;
using HostManagement.Scripts.Enums;
using HostManagement.Shared.Communication.Enums;

namespace HostManagement.System;

/// <inheritdoc cref="IRestartSystemScriptHandler"/>
/// <param name="result">The result of the operation.</param>
internal sealed class RestartSystemScriptHandler(Result result) : IRestartSystemScriptHandler
{
    private readonly Result _result = result;

    public async Task<OperationStatus> ExecuteRestartSystemScript()
    {
        var scriptResponse = await ScriptRunner
            .GetInstance()
            .Run(ScriptIdentifier.RestartSystem);

        if (scriptResponse.HasErrors)
        {
            _result.AddScriptError(
                ScriptIdentifier.RestartSystem,
                scriptResponse.StandardError);

            return OperationStatus.Error;
        }

        return OperationStatus.Success;
    }
}

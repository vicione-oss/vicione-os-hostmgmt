using HostManagement.Scripts;
using HostManagement.Scripts.Enums;
using HostManagement.Shared.Communication.Enums;

namespace HostManagement.System;

/// <inheritdoc cref="IShutdownSystemScriptHandler"/>
/// <param name="result">The result of the operation.</param>
internal sealed class ShutdownSystemScriptHandler(Result result) : IShutdownSystemScriptHandler
{
    private readonly Result _result = result;

    public async Task<OperationStatus> ExecuteShutdownSystemScript()
    {
        var scriptResponse = await ScriptRunner
            .GetInstance()
            .Run(ScriptIdentifier.ShutdownSystem);

        if (scriptResponse.HasErrors)
        {
            _result.AddScriptError(
                ScriptIdentifier.ShutdownSystem,
                scriptResponse.StandardError);

            return OperationStatus.Error;
        }

        return OperationStatus.Success;
    }
}

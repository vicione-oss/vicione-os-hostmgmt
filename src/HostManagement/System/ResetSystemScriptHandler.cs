using HostManagement.Scripts;
using HostManagement.Scripts.Enums;
using HostManagement.Shared.Communication.Enums;

namespace HostManagement.System;

/// <inheritdoc cref="IResetSystemScriptHandler"/>
/// <param name="result">The result of the operation.</param>
internal sealed class ResetSystemScriptHandler(Result result) : IResetSystemScriptHandler
{
    private readonly Result _result = result;

    public async Task<OperationStatus> ExecuteResetSystemScript()
    {
        var scriptResponse = await ScriptRunner.Run(
            ScriptIdentifier.ResetSystem);

        if (scriptResponse.HasErrors)
        {
            _result.AddScriptError(
                ScriptIdentifier.ResetSystem,
                scriptResponse.StandardError);

            return OperationStatus.Error;
        }

        return OperationStatus.Success;
    }
}

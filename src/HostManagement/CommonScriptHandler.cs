using HostManagement.Scripts;
using HostManagement.Scripts.Enums;
using HostManagement.Shared.Communication.Enums;

namespace HostManagement;

/// <inheritdoc cref="ICommonScriptHandler"/>
internal sealed class CommonScriptHandler : ICommonScriptHandler
{
    private readonly Result _result;

    /// <summary>
    /// Initializes a new <see cref="CommonScriptHandler"/>.
    /// </summary>
    /// <param name="result">The result of the operation.</param>
    public CommonScriptHandler(Result result)
    {
        _result = result;
    }

    public async Task<(OperationStatus, string)> ExecuteGetArchitectureScript()
    {
        var scriptResponse = await ScriptRunner.Run(ScriptIdentifier.CommonSystemArchitectureGet);

        if (scriptResponse.HasErrors)
        {
            _result.AddError($"{ScriptIdentifier.CommonSystemArchitectureGet}: {scriptResponse.StandardError}");
            return (OperationStatus.Error, string.Empty);
        }

        return (OperationStatus.Success, scriptResponse.StandardOutput);
    }
}

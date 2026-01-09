using HostManagement.Configuration;
using HostManagement.Scripts;
using HostManagement.Scripts.Enums;
using HostManagement.Scripts.ScriptOptionNames;
using HostManagement.Shared.Communication.Enums;

namespace HostManagement.ViciOneSuite;

/// <inheritdoc cref="IViciOneSuiteInstallScriptHandler"/>
internal sealed class ViciOneSuiteInstallScriptHandler : IViciOneSuiteInstallScriptHandler
{
    private readonly Result _result;

    /// <summary>
    /// Initializes a new <see cref="ViciOneSuiteInstallScriptHandler"/>.
    /// </summary>
    /// <param name="result">The result of the operation.</param>
    public ViciOneSuiteInstallScriptHandler(Result result)
    {
        _result = result;
    }

    public async Task<(OperationStatus, string)> ExecuteGetViciOneSuiteVersionsScript()
    {
        var scriptOptions = new ScriptOptions();

        scriptOptions.Add(
            GetViciOneSuiteVersionsScriptOptionName.SuiteRepositoryEndpoint,
            EnvironmentVariables.SuiteRepositoryEndpoint);
        scriptOptions.Add(
            GetViciOneSuiteVersionsScriptOptionName.SuiteRepositoryUsername,
            EnvironmentVariables.SuiteRepositoryUsername);
        scriptOptions.Add(
            GetViciOneSuiteVersionsScriptOptionName.SuiteRepositoryPassword,
            EnvironmentVariables.SuiteRepositoryPassword);

        var scriptResponse = await ScriptRunner.Run(
            ScriptIdentifier.ViciOneSuiteVersionsGet,
            scriptOptions.ToArray());

        if (scriptResponse.HasErrors)
        {
            _result.AddError($"{ScriptIdentifier.ViciOneSuiteVersionsGet}: {scriptResponse.StandardError}");
            return (OperationStatus.Error, string.Empty);
        }

        return (OperationStatus.Success, scriptResponse.StandardOutput);
    }

    public async Task<(OperationStatus, string)> ExecuteGetInstalledSuiteVersionScript()
    {
        var scriptResponse = await ScriptRunner.Run(
            ScriptIdentifier.ViciOneSuiteInstalledVersionGet);

        return (OperationStatus.Success, scriptResponse.StandardOutput);
    }

    /// <remarks>The .deb file will be downloaded to the
    /// <see cref="EnvironmentVariables.SuiteDownloadDirectory"/>.</remarks>
    public async Task<OperationStatus> ExecuteGetViciOneSuiteDebFileScript(string suiteDebianFile)
    {
        var scriptOptions = new ScriptOptions();

        scriptOptions.Add(
            GetViciOneSuiteDebFileScriptOptionName.SuiteRepositoryEndpoint,
            EnvironmentVariables.SuiteRepositoryEndpoint);
        scriptOptions.Add(
            GetViciOneSuiteDebFileScriptOptionName.SuiteDebianFile,
            suiteDebianFile);
        scriptOptions.Add(
            GetViciOneSuiteDebFileScriptOptionName.SuiteRepositoryUsername,
            EnvironmentVariables.SuiteRepositoryUsername);
        scriptOptions.Add(
            GetViciOneSuiteDebFileScriptOptionName.SuiteRepositoryPassword,
            EnvironmentVariables.SuiteRepositoryPassword);
        scriptOptions.Add(
            GetViciOneSuiteDebFileScriptOptionName.SuiteDebianFileDownloadDirectory,
            EnvironmentVariables.SuiteDownloadDirectory);

        var scriptResponse = await ScriptRunner.Run(
            300_000,
            ScriptIdentifier.ViciOneSuiteDebFileGet,
            scriptOptions.ToArray());

        if (scriptResponse.HasErrors)
        {
            _result.AddError($"{ScriptIdentifier.ViciOneSuiteDebFileGet}: {scriptResponse.StandardError}");
            return OperationStatus.Error;
        }

        return OperationStatus.Success;
    }

    /// <remarks>The .deb file will be used from the
    /// <see cref="EnvironmentVariables.SuiteDownloadDirectory"/>.</remarks>
    public async Task<OperationStatus> ExecuteViciOneSuiteInstallScript(string suiteDebianFile)
    {
        var scriptOptions = new ScriptOptions();

        scriptOptions.Add(
            InstallViciOneSuiteDebFileScriptOptionName.SuiteDebianFileDownloadDirectory,
            EnvironmentVariables.SuiteDownloadDirectory);
        scriptOptions.Add(
            InstallViciOneSuiteDebFileScriptOptionName.SuiteDebianFile,
            suiteDebianFile);
        scriptOptions.Add(InstallViciOneSuiteDebFileScriptOptionName.ForceReinstall);

        var scriptResponse = await ScriptRunner.Run(
            300_000,
            ScriptIdentifier.ViciOneSuiteInstallDebFile,
            scriptOptions.ToArray());

        if (scriptResponse.HasWarnings)
        {
            _result.AddWarning($"{ScriptIdentifier.ViciOneSuiteInstallDebFile}: {scriptResponse.StandardError}");
            return OperationStatus.Warning;
        }

        if (scriptResponse.HasErrors)
        {
            _result.AddError($"{ScriptIdentifier.ViciOneSuiteInstallDebFile}: {scriptResponse.StandardError}");
            return OperationStatus.Error;
        }

        return OperationStatus.Success;
    }
}

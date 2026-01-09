using HostManagement.Configuration;
using HostManagement.Scripts;
using HostManagement.Scripts.Enums;
using HostManagement.Shared.Communication.Enums;

namespace HostManagement.ViciOneSuite;

/// <inheritdoc cref="IViciOneSuiteInstallScriptHandler"/>
/// <param name="result">The result of the operation.</param>
internal sealed class ViciOneSuiteInstallScriptHandler(Result result) : IViciOneSuiteInstallScriptHandler
{
    private readonly Result _result = result;

    public async Task<(OperationStatus, string)> ExecuteGetViciOneSuiteVersionsScript()
    {
        var scriptOptions = new ScriptOptions();

        scriptOptions.Add(
            ScriptOptionNames.GetViciOneSuiteVersions.SuiteRepositoryEndpoint,
            EnvironmentVariables.SuiteRepositoryEndpoint);
        scriptOptions.Add(
            ScriptOptionNames.GetViciOneSuiteVersions.SuiteRepositoryUsername,
            EnvironmentVariables.SuiteRepositoryUsername);
        scriptOptions.Add(
            ScriptOptionNames.GetViciOneSuiteVersions.SuiteRepositoryPassword,
            EnvironmentVariables.SuiteRepositoryPassword);

        var scriptResponse = await ScriptRunner.GetInstance().Run(
            ScriptIdentifier.ViciOneSuiteVersionsGet,
            scriptOptions.Options);

        if (scriptResponse.HasErrors)
        {
            _result.AddError($"{ScriptIdentifier.ViciOneSuiteVersionsGet}: {scriptResponse.StandardError}");
            return (OperationStatus.Error, string.Empty);
        }

        return (OperationStatus.Success, scriptResponse.StandardOutput);
    }

    public async Task<(OperationStatus, string)> ExecuteGetInstalledSuiteVersionScript()
    {
        var scriptResponse = await ScriptRunner.GetInstance().Run(
            ScriptIdentifier.ViciOneSuiteInstalledVersionGet);

        return (OperationStatus.Success, scriptResponse.StandardOutput);
    }

    /// <remarks>The .deb file will be downloaded to the
    /// <see cref="EnvironmentVariables.SuiteDownloadDirectory"/>.</remarks>
    public async Task<OperationStatus> ExecuteGetViciOneSuiteDebFileScript(string suiteDebianFile)
    {
        var scriptOptions = new ScriptOptions();

        scriptOptions.Add(
            ScriptOptionNames.GetViciOneSuiteDebFile.SuiteRepositoryEndpoint,
            EnvironmentVariables.SuiteRepositoryEndpoint);
        scriptOptions.Add(
            ScriptOptionNames.GetViciOneSuiteDebFile.SuiteDebianFile,
            suiteDebianFile);
        scriptOptions.Add(
            ScriptOptionNames.GetViciOneSuiteDebFile.SuiteRepositoryUsername,
            EnvironmentVariables.SuiteRepositoryUsername);
        scriptOptions.Add(
            ScriptOptionNames.GetViciOneSuiteDebFile.SuiteRepositoryPassword,
            EnvironmentVariables.SuiteRepositoryPassword);
        scriptOptions.Add(
            ScriptOptionNames.GetViciOneSuiteDebFile.SuiteDebianFileDownloadDirectory,
            EnvironmentVariables.SuiteDownloadDirectory);

        var scriptResponse = await ScriptRunner.GetInstance().Run(
            3_600_000,
            ScriptIdentifier.ViciOneSuiteDebFileGet,
            scriptOptions.Options);

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
            ScriptOptionNames.InstallViciOneSuiteDebFile.SuiteDebianFileDownloadDirectory,
            EnvironmentVariables.SuiteDownloadDirectory);
        scriptOptions.Add(
            ScriptOptionNames.InstallViciOneSuiteDebFile.SuiteDebianFile,
            suiteDebianFile);
        scriptOptions.Add(ScriptOptionNames.InstallViciOneSuiteDebFile.ForceReinstall);

        var scriptResponse = await ScriptRunner.GetInstance().Run(
            300_000,
            ScriptIdentifier.ViciOneSuiteInstallDebFile,
            scriptOptions.Options);

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

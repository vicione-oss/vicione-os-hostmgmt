using HostManagement.Shared.Communication.Enums;

namespace HostManagement.ViciOneSuite;

/// <inheritdoc cref="IViciOneSuiteVersionHelper"/>
/// <param name="scriptHandler">An <see cref="IScriptHandler"/> implementation.</param>
/// <param name="result">The result of the operation.</param>
internal sealed class ViciOneSuiteVersionHelper(IScriptHandler scriptHandler, Result result)
    : IViciOneSuiteVersionHelper
{
    private const char PreReleaseVersionMarker = '~';
    private const string ViciOneSuiteVersionPrefix = "vicione-suite_";
    private const string VersionMappingFileHeader =
        "HostManagement Version,Suite Debian Package Version";

    private readonly IScriptHandler _scriptHandler = scriptHandler;
    private readonly Result _result = result;

    public Func<string, bool> NonPreReleaseVersionsFilter { get; } = version =>
        version.Count(static character => character == '.') == 2 &&
        !version.Contains(PreReleaseVersionMarker);

    public async Task<(OperationStatus, Dictionary<string, IList<string>>)> GetVersionMapping(
        string versionsCsvString)
    {
        var versionMappings = new Dictionary<string, IList<string>>();

        using (var reader = new StringReader(versionsCsvString))
        {
            var firstLine = await reader.ReadLineAsync();

            if (firstLine is null)
            {
                _result.AddError(ViciOneSuiteResources.VersionMappingFileEmpty);
                return (OperationStatus.Error, versionMappings);
            }

            if (!firstLine.Contains(VersionMappingFileHeader))
            {
                _result.AddError(ViciOneSuiteResources.VersionMappingFileInvalidHeader);
                return (OperationStatus.Error, versionMappings);
            }

            string? line;
            var lineNumber = 2;

            while ((line = await reader.ReadLineAsync()) != null)
            {
                var parts = line.Split(',');

                if (parts.Length < 2 ||
                    parts.Length > 2)
                {
                    _result.AddError(ViciOneSuiteResources.CouldNotParseVersionMappingLine, lineNumber);
                    return (OperationStatus.Error, []);
                }

                var hostManagementVersion = parts[0];

                if (!parts[1].Contains(ViciOneSuiteVersionPrefix))
                {
                    _result.AddError(ViciOneSuiteResources.CouldNotParseVersionMappingLine, lineNumber);
                    return (OperationStatus.Error, []);
                }

                var suiteDebianVersion = parts[1].Replace(ViciOneSuiteVersionPrefix, string.Empty);

                if (versionMappings.TryGetValue(hostManagementVersion, out var suiteDebianVersionList))
                {
                    if (suiteDebianVersionList.Contains(suiteDebianVersion))
                        continue;

                    suiteDebianVersionList.Add(suiteDebianVersion);
                }
                else
                {
                    versionMappings[hostManagementVersion] = [suiteDebianVersion];
                }

                lineNumber++;
            }

            if (versionMappings.Count == 0)
            {
                _result.AddWarning(ViciOneSuiteResources.NoVersionMappingsFound);
                return (OperationStatus.Warning, versionMappings);
            }
        }

        return (OperationStatus.Success, versionMappings);
    }

    public async Task<(OperationStatus, IEnumerable<string>)> GetCompatibleSuiteVersions(
        string hostManagementVersionString,
        string installedSuiteVersionString,
        Dictionary<string, IList<string>> versionMapping)
    {
        if (!versionMapping.TryGetValue(hostManagementVersionString, out var suiteVersions))
        {
            _result.AddWarning(
                ViciOneSuiteResources.NoCompatibleSuiteVersionsForHostManagementFound,
                hostManagementVersionString);

            return (OperationStatus.Warning, Enumerable.Empty<string>());
        }

        var (status, supportedArchitecture) =
            await _scriptHandler.ExecuteGetArchitectureScript();

        if (status == OperationStatus.Error)
            return (OperationStatus.Error, Enumerable.Empty<string>());

        var compatibleArchitectureSuiteVersions = suiteVersions
            .Where(version => version.Contains(supportedArchitecture))
            .ToList();

        if (compatibleArchitectureSuiteVersions.Count == 0)
        {
            _result.AddWarning(
                ViciOneSuiteResources.NoCompatibleSuiteVersionsForArchitectureFound,
                supportedArchitecture);

            return (OperationStatus.Warning, Enumerable.Empty<string>());
        }

        var compatibleSuiteVersions = new List<string>();

        foreach (var versionString in compatibleArchitectureSuiteVersions)
        {
            var parts = versionString.Split('_');

            if (parts.Length is < 2 or > 2)
            {
                _result.AddError(ViciOneSuiteResources.CouldNotParseSuiteVersion, versionString);
                return (OperationStatus.Error, Enumerable.Empty<string>());
            }

            var version = DebianPackageVersion.Parse(parts[0]);
            var installedSuiteVersion = DebianPackageVersion.Parse(installedSuiteVersionString);

            if (version < installedSuiteVersion)
                continue;

            compatibleSuiteVersions.Add(parts[0]);
        }

        if (compatibleSuiteVersions.Count == 0)
        {
            _result.AddWarning(
                ViciOneSuiteResources.NoCompatibleSuiteVersionsForInstalledVersionFound,
                installedSuiteVersionString);

            return (OperationStatus.Warning, Enumerable.Empty<string>());
        }

        return (OperationStatus.Success, compatibleSuiteVersions);
    }

    public Task<(OperationStatus, IEnumerable<string>)> GetFilteredSuiteVersions(
        IEnumerable<string> suiteVersions,
        Func<string, bool> predicate)
    {
        var filteredVersions = suiteVersions
            .Where(predicate)
            .ToList();

        if (filteredVersions.Count == 0)
        {
            _result.AddWarning(ViciOneSuiteResources.NoCompatibleSuiteVersionsFound);
            return Task.FromResult((OperationStatus.Warning, Enumerable.Empty<string>()));
        }

        return Task.FromResult((OperationStatus.Success, filteredVersions.AsEnumerable()));
    }

    public Task<OperationStatus> IsVersionContainedInVersionList(
        string hostManagementVersionString,
        IEnumerable<string> suiteVersions,
        string suiteVersionString)
    {
        if (!suiteVersions.Contains(suiteVersionString))
        {
            _result.AddError(
                ViciOneSuiteResources.InstallingSuiteVersionNotSupportedForHostManagement,
                suiteVersionString,
                hostManagementVersionString);
            return Task.FromResult(OperationStatus.Error);
        }

        return Task.FromResult(OperationStatus.Success);
    }

    public async Task<(OperationStatus, string)> GetSuiteVersionDebianFileName(string suiteVersion)
    {
        string? suiteDebianFileName;

        var (status, architecture) =
            await _scriptHandler.ExecuteGetArchitectureScript();

        if (status == OperationStatus.Error)
            return (OperationStatus.Error, string.Empty);

        try
        {
            suiteDebianFileName = string.Concat(
                ViciOneSuiteVersionPrefix,
                suiteVersion,
                "_",
                architecture,
                ".deb");
        }
        catch (Exception)
        {
            _result.AddError(
                ViciOneSuiteResources.CouldNotCreateSuiteVersionDebianFileName,
                suiteVersion,
                architecture);

            return (OperationStatus.Error, string.Empty);
        }

        return (OperationStatus.Success, suiteDebianFileName);
    }
}

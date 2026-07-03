using System.IO.Abstractions;
using System.Text.Json;
using HostManagement.Configuration;
using HostManagement.SystemConfig.Contracts;
using HostManagement.SystemConfig.Migrations;

namespace HostManagement.SystemConfig;

/// <inheritdoc cref="IPersistence"/>
/// <param name="fileSystem">A reference to the file system.</param>
/// <param name="migrationService">The system configuration migration service.</param>
/// <param name="contractDeserializers">The available system configuration contract deserializers.</param>
internal sealed class SystemConfigurationPersistence(
    IFileSystem fileSystem,
    ISystemConfigurationMigrationService? migrationService = null,
    IEnumerable<ISystemConfigurationContractDeserializer>? contractDeserializers = null)
        : IPersistence
{
    private readonly IFileSystem _fileSystem = fileSystem;

    private readonly SystemConfigurationContractDeserializerRegistry _contractDeserializers = new(
        contractDeserializers ??
        [
            new V1SystemConfigurationContractDeserializer(),
            new CurrentSystemConfigurationContractDeserializer()
        ]);

    private readonly ISystemConfigurationMigrationService _migrationService = migrationService
        ?? new SystemConfigurationMigrationService(
            [
                new V1ToV2SystemConfigurationMigration()
            ]);

    public void DeleteDefectiveMarkedUserConfiguration()
        => _fileSystem.File.Delete(AppPaths.DefectiveSystemConfigurationFilePath);

    public bool ExistsDefectivePreparationConfiguration()
        => _fileSystem.File.Exists(AppPaths.DefectiveDefaultSystemConfigurationFilePath);

    public bool ExistsDefectiveUserConfiguration()
        => _fileSystem.File.Exists(AppPaths.DefectiveSystemConfigurationFilePath);

    public bool ExistsPreparationConfiguration()
        => _fileSystem.File.Exists(AppPaths.DefaultSystemConfigurationFilePath);

    public bool ExistsUserConfiguration()
        => _fileSystem.File.Exists(AppPaths.SystemConfigurationFilePath);

    public string GetDefectivePreparationConfigurationPath()
        => AppPaths.DefectiveDefaultSystemConfigurationFilePath;

    public string GetDefectiveUserConfigurationPath()
        => AppPaths.DefectiveSystemConfigurationFilePath;

    public string GetPreparationConfigurationPath()
        => AppPaths.DefaultSystemConfigurationFilePath;

    public string GetUserConfigurationPath()
        => AppPaths.SystemConfigurationFilePath;

    public Task<SystemConfiguration> LoadPreparationConfiguration(CancellationToken cancellationToken)
        => LoadSystemConfiguration(AppPaths.DefaultSystemConfigurationFilePath, cancellationToken);

    public Task<SystemConfiguration> LoadUserConfiguration(CancellationToken cancellationToken)
        => LoadSystemConfiguration(AppPaths.SystemConfigurationFilePath, cancellationToken);

    public void MarkPreparationConfigurationDefective()
    {
        _fileSystem.File.Move(
            AppPaths.DefaultSystemConfigurationFilePath,
            AppPaths.DefectiveDefaultSystemConfigurationFilePath,
            overwrite: true);
    }

    public void MarkUserConfigurationDefective()
    {
        _fileSystem.File.Move(
            AppPaths.SystemConfigurationFilePath,
            AppPaths.DefectiveSystemConfigurationFilePath,
            overwrite: true);
    }

    public async Task SaveUserConfiguration(SystemConfiguration systemConfiguration, CancellationToken cancellationToken)
    {
        var filePath = AppPaths.SystemConfigurationFilePath;
        var directoryPath = _fileSystem.Path.GetDirectoryName(filePath)
            ?? throw new InvalidOperationException($"Could not get directory name for '{filePath}'.");

        _fileSystem.Directory.CreateDirectory(directoryPath);

        await using var stream = _fileSystem.FileStream.New(filePath, new FileStreamOptions()
        {
            Access = FileAccess.Write,
            Mode = FileMode.Create,
            Options = FileOptions.Asynchronous,
            Share = FileShare.Read,
        });

        await JsonSerializer
            .SerializeAsync(
                stream,
                systemConfiguration,
                SystemConfigurationPersistenceContext.Default.SystemConfiguration,
                cancellationToken)
            .ConfigureAwait(false);
    }

    private static int ReadVersion(JsonElement configuration)
    {
        if (configuration.ValueKind is not JsonValueKind.Object)
            throw new JsonException("System configuration JSON root element must be an object.");

        if (!configuration.TryGetProperty(nameof(SystemConfiguration.Version), out var versionElement))
            throw new JsonException("System configuration JSON is missing required 'Version' property.");

        if (versionElement.ValueKind is not JsonValueKind.Number ||
            !versionElement.TryGetInt32(out var version))
        {
            throw new JsonException("System configuration JSON property 'Version' must be an integer.");
        }

        return version;
    }

    private async Task<SystemConfiguration> LoadSystemConfiguration(
        string configurationFilePath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var stream = _fileSystem.FileStream.New(configurationFilePath, new FileStreamOptions()
            {
                Access = FileAccess.Read,
                Mode = FileMode.Open,
                Options = FileOptions.Asynchronous | FileOptions.SequentialScan,
                Share = FileShare.Read,
            });

            using var document = await JsonDocument
                .ParseAsync(stream, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            var sourceVersion = ReadVersion(document.RootElement);

            if (!_contractDeserializers.TryGetDeserializer(sourceVersion, out var deserializer))
            {
                throw new NotSupportedException(
                    $"System configuration contract version '{sourceVersion}' is not supported.");
            }

            var sourceConfiguration = deserializer.Deserialize(document.RootElement);

            return _migrationService.Migrate(sourceConfiguration);
        }
        catch (JsonException ex)
        {
            // Invalid JSON syntax or defective file

            throw new InvalidSystemConfigurationException(
                $"UTF-8 JSON stream could not be deserialized to '{nameof(SystemConfiguration)}'.",
                ex);
        }
    }
}

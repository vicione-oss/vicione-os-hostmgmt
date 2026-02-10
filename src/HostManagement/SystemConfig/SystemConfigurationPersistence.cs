using System.IO.Abstractions;
using System.Text.Json;
using HostManagement.Configuration;
using HostManagement.SystemConfig.Contracts;

namespace HostManagement.SystemConfig;

/// <summary>
/// Provides persistence for the system configuration, e.g. loading from or saving to a JSON file.
/// </summary>
/// <param name="fileSystem">A reference to the file system.</param>
internal sealed class SystemConfigurationPersistence(IFileSystem fileSystem) : IPersistence
{
    private readonly IFileSystem _fileSystem = fileSystem;

    /// <inheritdoc/>
    public Task<SystemConfiguration> LoadPreparationConfiguration(CancellationToken cancellationToken)
        => LoadSystemConfiguration(AppPaths.DefaultSystemConfigurationFilePath, cancellationToken);

    /// <inheritdoc/>
    public Task<SystemConfiguration> LoadUserConfiguration(CancellationToken cancellationToken)
        => LoadSystemConfiguration(AppPaths.SystemConfigurationFilePath, cancellationToken);

    private async Task<SystemConfiguration> LoadSystemConfiguration(
        string configurationFilePath,
        CancellationToken cancellationToken = default)
    {
        await using var stream = _fileSystem.FileStream.New(configurationFilePath, new FileStreamOptions()
        {
            Access = FileAccess.Read,
            Mode = FileMode.Open,
            Options = FileOptions.Asynchronous | FileOptions.SequentialScan,
            Share = FileShare.Read,
        });

        return await JsonSerializer
            .DeserializeAsync(
                stream,
                SystemConfigurationPersistenceContext.Default.SystemConfiguration,
                cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException($"UTF-8 JSON stream could not be deserialized to '{nameof(SystemConfiguration)}'.");
    }

    /// <inheritdoc/>
    public bool PreparationConfigurationExists()
        => _fileSystem.File.Exists(AppPaths.DefaultSystemConfigurationFilePath);

    /// <inheritdoc/>
    public bool UserConfigurationExists()
        => _fileSystem.File.Exists(AppPaths.SystemConfigurationFilePath);

    /// <inheritdoc/>
    public string GetPreparationConfigurationPath()
        => AppPaths.DefaultSystemConfigurationFilePath;

    /// <inheritdoc/>
    public string GetUserConfigurationPath()
        => AppPaths.SystemConfigurationFilePath;

    /// <inheritdoc/>
    public async Task SaveUserConfiguration(SystemConfiguration systemConfiguration, CancellationToken cancellationToken)
    {
        var filePath = AppPaths.SystemConfigurationFilePath;
        var directoryPath = _fileSystem.Path.GetDirectoryName(filePath)
            ?? throw new InvalidOperationException($"Could not get directory name for '{filePath}'.");

        _fileSystem.Directory.CreateDirectory(directoryPath);

        // reset readonly properties to exclude them from serialization
        if (systemConfiguration.NetworkInterfacesSettings?.NetworkInterfaces is not null)
        {
            foreach (var networkInterface in systemConfiguration.NetworkInterfacesSettings.NetworkInterfaces)
            {
                if (networkInterface.IPv4 is not null)
                    networkInterface.IPv4.DHCPLease = default;
            }
        }
        if (systemConfiguration.NetworkNTPSettings is not null)
            systemConfiguration.NetworkNTPSettings.FallbackNTPServers = default;

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
}

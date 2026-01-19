using System.IO.Abstractions;
using HostManagement.SystemConfig.Contracts;
using System.Text.Json;

namespace HostManagement.SystemConfig;

/// <summary>
/// Provides persistence for the system configuration, e.g. loading from or saving to a JSON file.
/// </summary>
/// <param name="fileSystem">A reference to the file system.</param>
internal sealed class SystemConfigurationPersistence(IFileSystem fileSystem) : IPersistence
{
    private readonly IFileSystem _fileSystem = fileSystem;

    /// <summary>
    /// Loads a <see cref="SystemConfiguration"/> from the specified <paramref name="configurationFilePath"/>.
    /// </summary>
    /// <param name="configurationFilePath">The path to the configuration file.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><see cref="SystemConfiguration"/></returns>
    public async Task<SystemConfiguration> LoadSystemConfiguration(
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

    /// <summary>
    /// Saves the <paramref name="systemConfiguration"/> to the specified <paramref name="configurationFilePath"/>.
    /// </summary>
    /// <param name="systemConfiguration">The <see cref="SystemConfiguration"/>.</param>
    /// <param name="configurationFilePath">The path to the configuration file.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task SaveSystemConfiguration(
        SystemConfiguration systemConfiguration,
        string configurationFilePath,
        CancellationToken cancellationToken = default)
    {
        var directoryPath = _fileSystem.Path.GetDirectoryName(configurationFilePath)
            ?? throw new InvalidOperationException($"Could not get directory name for '{configurationFilePath}'.");

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

        await using var stream = _fileSystem.FileStream.New(configurationFilePath, new FileStreamOptions()
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

using System.IO.Abstractions;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Serialization;

namespace HostManagement.SystemConfig;

/// <summary>
/// Provides persistence for the system configuration, e.g. loading from or saving to a JSON file.
/// </summary>
/// <param name="fileSystem">A reference to the file system.</param>
internal sealed class SystemConfigurationPersistence(IFileSystem fileSystem)
{
    private readonly IFileSystem _fileSystem = fileSystem;

    /// <summary>
    /// Loads a <see cref="SystemConfiguration"/> from the specified <paramref name="configurationFilePath"/>.
    /// </summary>
    /// <param name="configurationFilePath">The path to the configuration file.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><see cref="SystemConfiguration"/></returns>
    internal async Task<SystemConfiguration> LoadSystemConfiguration(
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

        return await SystemConfigurationSerializer
            .DeserializeAsync(
                stream,
                SystemConfigurationPersistenceContext.Default.SystemConfiguration,
                cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Saves the <paramref name="systemConfiguration"/> to the specified <paramref name="configurationFilePath"/>.
    /// </summary>
    /// <param name="systemConfiguration">The <see cref="SystemConfiguration"/>.</param>
    /// <param name="configurationFilePath">The path to the configuration file.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="InvalidOperationException"></exception>
    internal async Task SaveSystemConfiguration(
        SystemConfiguration systemConfiguration,
        string configurationFilePath,
        CancellationToken cancellationToken = default)
    {
        var directoryPath = _fileSystem.Path.GetDirectoryName(configurationFilePath)
            ?? throw new InvalidOperationException($"Could not get directory name for '{configurationFilePath}'.");

        _fileSystem.Directory.CreateDirectory(directoryPath);

        // reset readonly properties to exclude them from serialization
        foreach (var networkInterface in systemConfiguration.NetworkInterfacesSettings.NetworkInterfaces)
            networkInterface.IPv4.DHCPLease = null;

        systemConfiguration.NetworkNTPSettings = new(default, systemConfiguration.NetworkNTPSettings.NTPServers)
        {
            NTPServersEnabled = systemConfiguration.NetworkNTPSettings.NTPServersEnabled
        };

        await using var stream = _fileSystem.FileStream.New(configurationFilePath, new FileStreamOptions()
        {
            Access = FileAccess.Write,
            Mode = FileMode.Create,
            Options = FileOptions.Asynchronous,
            Share = FileShare.Read,
        });

        await SystemConfigurationSerializer
            .SerializeAsync(
                stream,
                systemConfiguration,
                SystemConfigurationPersistenceContext.Default.SystemConfiguration,
                cancellationToken)
            .ConfigureAwait(false);
    }
}

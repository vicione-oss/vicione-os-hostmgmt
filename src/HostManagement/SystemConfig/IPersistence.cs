using HostManagement.SystemConfig.Contracts;

namespace HostManagement.SystemConfig;

internal interface IPersistence
{
    /// <summary>
    /// Loads the configuration that was flashed onto the device to prepare the device for initial use.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The configuration to prepare the device.</returns>
    Task<SystemConfiguration> LoadPreparationConfiguration(CancellationToken cancellationToken);

    /// <summary>
    /// Loads the configuration with user-defined customizations.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The user-defined configuration.</returns>
    Task<SystemConfiguration> LoadUserConfiguration(CancellationToken cancellationToken);

    /// <summary>
    /// Saves the configuration with user-defined customizations.
    /// </summary>
    /// <param name="systemConfiguration">The <see cref="SystemConfiguration"/> to save.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="InvalidOperationException">The directory name could not be determined.</exception>
    Task SaveUserConfiguration(SystemConfiguration systemConfiguration, CancellationToken cancellationToken);

    /// <summary>
    /// Checks if the preparation configuration exists.
    /// </summary>
    /// <returns><see langword="true"/> if the preparation configuration exists; otherwise, <see langword="false"/>.</returns>
    bool PreparationConfigurationExists();

    /// <summary>
    /// Checks if the user-defined configuration exists.
    /// </summary>
    /// <returns><see langword="true"/> if the user-defined configuration exists; otherwise, <see langword="false"/>.</returns>
    bool UserConfigurationExists();

    /// <summary>
    /// Gets the file path of the preparation configuration.
    /// </summary>
    /// <returns>The file path of the preparation configuration.</returns>
    string GetPreparationConfigurationPath();

    /// <summary>
    /// Gets the file path of the user-defined configuration.
    /// </summary>
    /// <returns>The file path of the user-defined configuration.</returns>
    string GetUserConfigurationPath();
}

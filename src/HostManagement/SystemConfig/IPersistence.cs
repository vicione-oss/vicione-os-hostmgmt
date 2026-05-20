using HostManagement.SystemConfig.Contracts;

namespace HostManagement.SystemConfig;

/// <summary>
/// Defines persistence operations for preparation and user-defined system configuration.
/// </summary>
internal interface IPersistence
{
    /// <summary>
    /// Checks whether the preparation configuration exists.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the preparation configuration exists, otherwise <see langword="false"/>.
    /// </returns>
    bool ExistsPreparationConfiguration();

    /// <summary>
    /// Checks whether the user-defined configuration exists.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the user-defined configuration exists, otherwise <see langword="false"/>.
    /// </returns>
    bool ExistsUserConfiguration();

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

    /// <summary>
    /// Loads the preparation configuration that came with the initial device image.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The configuration to prepare the device.</returns>
    Task<SystemConfiguration> LoadPreparationConfiguration(CancellationToken cancellationToken);

    /// <summary>
    /// Loads the user-defined configuration.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The user-defined configuration.</returns>
    Task<SystemConfiguration> LoadUserConfiguration(CancellationToken cancellationToken);

    /// <summary>
    /// Saves the user-defined configuration.
    /// </summary>
    /// <param name="systemConfiguration">The <see cref="SystemConfiguration"/> to save.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="InvalidOperationException">The directory name could not be determined.</exception>
    Task SaveUserConfiguration(SystemConfiguration systemConfiguration, CancellationToken cancellationToken);
}

using HostManagement.SystemConfig.Contracts;

namespace HostManagement.SystemConfig;

/// <summary>
/// Defines persistence operations for preparation and user-defined system configuration.
/// </summary>
internal interface IPersistence
{
    /// <summary>
    /// Deletes the previously marked defective user-defined configuration.
    /// </summary>
    void DeleteDefectiveMarkedUserConfiguration();

    /// <summary>
    /// Checks whether a defective preparation configuration exists.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if a defective preparation configuration exists, otherwise <see langword="false"/>.
    /// </returns>
    bool ExistsDefectivePreparationConfiguration();

    /// <summary>
    /// Checks whether a defective user-defined configuration exists.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if a defective user-defined configuration exists, otherwise <see langword="false"/>.
    /// </returns>
    bool ExistsDefectiveUserConfiguration();

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
    /// Gets the file path of the defective preparation configuration.
    /// </summary>
    /// <returns>The file path of the defective preparation configuration.</returns>
    string GetDefectivePreparationConfigurationPath();

    /// <summary>
    /// Gets the file path of the defective user-defined configuration.
    /// </summary>
    /// <returns>The file path of the defective user-defined configuration.</returns>
    string GetDefectiveUserConfigurationPath();

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
    /// <returns>The loaded preparation configuration together with its source contract version.</returns>
    Task<LoadedSystemConfiguration> LoadPreparationConfiguration(CancellationToken cancellationToken);

    /// <summary>
    /// Loads the user-defined configuration.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The loaded user-defined configuration together with its source contract version.</returns>
    Task<LoadedSystemConfiguration> LoadUserConfiguration(CancellationToken cancellationToken);

    /// <summary>
    /// Marks the preparation configuration as defective.
    /// </summary>
    void MarkPreparationConfigurationDefective();

    /// <summary>
    /// Marks the user-defined configuration as defective.
    /// </summary>
    void MarkUserConfigurationDefective();

    /// <summary>
    /// Saves the user-defined configuration.
    /// </summary>
    /// <param name="systemConfiguration">The <see cref="SystemConfiguration"/> to save.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="InvalidOperationException">The directory name could not be determined.</exception>
    Task SaveUserConfiguration(SystemConfiguration systemConfiguration, CancellationToken cancellationToken);
}

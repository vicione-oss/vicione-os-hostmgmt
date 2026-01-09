using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine;

/// <summary>
/// Represents the abstract base class for a config command.
/// </summary>
/// <param name="name">The name of the command.</param>
/// <param name="description">The description of the command.</param>
internal abstract class ConfigCommandBase(string name, string description)
    : CommandBase(name, description)
{
    /// <inheritdoc cref="NamedPipeRequestHelper.GetConfiguration"/>
    private protected static async Task<SystemConfiguration?> GetConfiguration()
        => await NamedPipeRequestHelper.GetConfiguration();

    /// <inheritdoc cref="NamedPipeRequestHelper.SetConfiguration(SystemConfiguration)"/>
    private protected static async Task<bool> SetConfiguration(SystemConfiguration config)
        => await NamedPipeRequestHelper.SetConfiguration(config);
}

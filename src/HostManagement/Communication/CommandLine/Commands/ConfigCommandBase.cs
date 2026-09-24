using System.CommandLine;
using HostManagement.Communication.CommandLine.Helper;
using HostManagement.Shared.Contracts;

namespace HostManagement.Communication.CommandLine.Commands;

/// <summary>
/// Represents the abstract base class for a config command.
/// </summary>
/// <param name="name">The name of the command.</param>
/// <param name="description">The description of the command.</param>
internal abstract class ConfigCommandBase(string name, string description) : Command(name, description)
{
    /// <inheritdoc cref="NamedPipeRequestHelper.GetConfiguration(CancellationToken)"/>
    private protected static async Task<SystemConfiguration?> GetConfiguration(CancellationToken cancellationToken)
        => await NamedPipeRequestHelper.GetConfiguration(cancellationToken);

    /// <inheritdoc cref="NamedPipeRequestHelper.SetConfiguration(SystemConfiguration, CancellationToken)"/>
    private protected static async Task<bool> SetConfiguration(SystemConfiguration config, CancellationToken cancellationToken)
        => await NamedPipeRequestHelper.SetConfiguration(config, cancellationToken);
}

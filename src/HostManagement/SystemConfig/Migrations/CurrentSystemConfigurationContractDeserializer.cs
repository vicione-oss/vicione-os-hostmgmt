using System.Text.Json.Serialization.Metadata;
using CurrentContracts = HostManagement.SystemConfig.Contracts;

namespace HostManagement.SystemConfig.Migrations;

/// <summary>
/// Deserializes current system configuration contracts.
/// </summary>
internal sealed class CurrentSystemConfigurationContractDeserializer
    : SystemConfigurationContractDeserializer<CurrentContracts.SystemConfiguration>
{
    /// <inheritdoc/>
    public override int Version => CurrentContracts.SystemConfiguration.CurrentVersion;

    /// <inheritdoc/>
    protected override JsonTypeInfo<CurrentContracts.SystemConfiguration> ConfigurationTypeInfo
        => SystemConfigurationPersistenceContext.Default.SystemConfiguration;
}

using System.Text.Json;

namespace HostManagement.SystemConfig.Migrations;

/// <summary>
/// Represents a deserializer for a specific persisted system configuration contract version.
/// </summary>
internal interface ISystemConfigurationContractDeserializer
{
    /// <summary>
    /// Gets the contract version supported by this deserializer.
    /// </summary>
    int Version { get; }

    /// <summary>
    /// Deserializes the given system configuration JSON to the supported contract type.
    /// </summary>
    /// <param name="configuration">The system configuration JSON root element.</param>
    /// <returns>The deserialized system configuration contract.</returns>
    ISystemConfigurationContract Deserialize(JsonElement configuration);
}

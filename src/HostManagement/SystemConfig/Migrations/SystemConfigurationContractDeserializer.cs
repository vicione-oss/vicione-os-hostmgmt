using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace HostManagement.SystemConfig.Migrations;

/// <summary>
/// Provides a typed base implementation for system configuration contract deserializers.
/// </summary>
/// <typeparam name="TConfiguration">The supported configuration contract type.</typeparam>
internal abstract class SystemConfigurationContractDeserializer<TConfiguration>
    : ISystemConfigurationContractDeserializer
        where TConfiguration : class, ISystemConfigurationContract
{
    /// <inheritdoc/>
    public abstract int Version { get; }

    /// <summary>
    /// Gets the JSON type information for the supported configuration contract.
    /// </summary>
    protected abstract JsonTypeInfo<TConfiguration> ConfigurationTypeInfo { get; }

    /// <inheritdoc/>
    public ISystemConfigurationContract Deserialize(JsonElement configuration)
    {
        var typedConfiguration = configuration.Deserialize(ConfigurationTypeInfo)
            ?? throw new InvalidOperationException(
                $"JSON could not be deserialized to '{typeof(TConfiguration).Name}'.");

        if (typedConfiguration.Version != Version)
        {
            throw new InvalidOperationException(
                $"Deserializer '{GetType().Name}' expected version '{Version}' " +
                $"but received version '{typedConfiguration.Version}'.");
        }

        return typedConfiguration;
    }
}

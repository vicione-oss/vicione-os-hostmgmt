using System.Text.Json.Serialization;

namespace HostManagement.Shared.Capabilities;

/// <summary>
/// Describes the support available for a setting.
/// </summary>
public abstract record SettingSupport
{
    /// <summary>
    /// The name of the setting for which support is described.
    /// </summary>
    [JsonPropertyOrder(-2)]
    public string Name { get; set; } = "Unknown";

    /// <summary>
    /// The supported capability for the setting.
    /// </summary>
    [JsonPropertyOrder(-1)]
    public CapabilityStatus Capability { get; set; }
}

/// <summary>
/// Describes support for a setting that can be configured directly.
/// </summary>
public abstract record LeafSettingSupport : SettingSupport;

/// <summary>
/// Describes support for a setting whose capability is derived from its child settings.
/// </summary>
public abstract record CompositeSettingSupport : SettingSupport;

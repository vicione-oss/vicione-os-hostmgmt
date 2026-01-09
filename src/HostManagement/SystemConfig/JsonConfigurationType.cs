namespace HostManagement.SystemConfig;

/// <summary>
/// Represents the JSON configuration type, e.g. 'DefaultSystemConfiguration.json' or
/// 'SystemConfiguration.json'.
/// </summary>
internal enum JsonConfigurationType
{
    /// <summary>
    /// No JSON configuration.
    /// </summary>
    None,

    /// <summary>
    /// The default system configuration ('DefaultSystemConfiguration.json').
    /// </summary>
    Default,

    /// <summary>
    /// The system configuration ('SystemConfiguration.json').
    /// </summary>
    System
}

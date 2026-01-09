namespace HostManagement.Shared.Enums;

/// <summary>
/// Represents the state of a linux service.
/// </summary>
public enum ServiceState
{
    /// <summary>
    /// Represents the unknown state of a linux service.
    /// </summary>
    Unknown,

    /// <summary>
    /// Represents the enabled state of a linux service.
    /// </summary>
    Enabled,

    /// <summary>
    /// Represents the disabled state of a linux service.
    /// </summary>
    Disabled
}

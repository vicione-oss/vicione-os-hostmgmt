namespace HostManagement.Shared.Enums;

/// <summary>
/// Represents the state of a Linux service (systemd unit).
/// </summary>
public enum ServiceState
{
    /// <summary>
    /// The unknown state.
    /// </summary>
    Unknown,

    /// <summary>
    /// The enabled state.
    /// </summary>
    Enabled,

    /// <summary>
    /// The disabled state.
    /// </summary>
    Disabled
}

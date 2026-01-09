namespace HostManagement.Shared.Enums;

/// <summary>
/// Represents the control action of a service e.g. restart.
/// </summary>
public enum ServiceControlAction
{
    /// <summary>
    /// Represents the unknown control action of a service.
    /// </summary>
    Unknown,

    /// <summary>
    /// Represents the start control action of a service.
    /// </summary>
    Start,

    /// <summary>
    /// Represents the stop control action of a service.
    /// </summary>
    Stop,

    /// <summary>
    /// Represents the restart control action of a service.
    /// </summary>
    Restart
}

namespace HostManagement.Shared;

/// <summary>
/// Contains the topics used for server events.
/// </summary>
public static class EventTopics
{
    /// <summary>
    /// The topic used by the pipe server to notify connected clients that the service is stopping.
    /// </summary>
    public const string PipeServerShutdown = "PipeServerShutdown";

    /// <summary>
    /// The topic used to notify connected clients that the system configuration has changed.
    /// </summary>
    public const string SystemConfigurationChanged = "SystemConfigurationChanged";
}

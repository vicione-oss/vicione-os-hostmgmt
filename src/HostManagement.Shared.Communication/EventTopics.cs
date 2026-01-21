namespace HostManagement.Shared.Communication;

/// <summary>
/// Contains the topics used for server events.
/// </summary>
public static class EventTopics
{
    /// <summary>
    /// The topic used by the pipe server to notify connected clients that the service is stopping.
    /// </summary>
    public const string PipeServerShutdown = "PipeServerShutdown";
}

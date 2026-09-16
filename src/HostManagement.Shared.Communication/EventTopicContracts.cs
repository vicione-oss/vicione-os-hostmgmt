using HostManagement.Shared.Communication.Contracts;

namespace HostManagement.Shared.Communication;

/// <summary>
/// Binds every topic in <see cref="EventTopics"/> to its event message contract.
/// </summary>
/// <remarks>
/// Registering a callback with one of these lets the compiler infer the message type, so a handler class is only
/// needed when the handler has state of its own.
/// </remarks>
public static class EventTopicContracts
{
    /// <inheritdoc cref="EventTopics.PipeServerShutdown"/>
    /// <remarks>The message is the raw shutdown reason.</remarks>
    public static EventContract<string> PipeServerShutdown { get; } = new(EventTopics.PipeServerShutdown);

    /// <inheritdoc cref="EventTopics.SystemConfigurationChanged"/>
    public static EventContract<GetSystemConfigurationResult> SystemConfigurationChanged { get; }
        = new(EventTopics.SystemConfigurationChanged);
}

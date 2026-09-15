using HostManagement.Shared.Communication.Contracts;

namespace HostManagement.Shared.Communication.NamedPipe.Client;

/// <summary>
/// Contains strongly typed named pipe event descriptors.
/// </summary>
public static class PipeEvents
{
    /// <summary>
    /// The event raised when the system configuration changes.
    /// </summary>
    public static PipeEvent<GetSystemConfigurationResult> SystemConfigurationChanged { get; } = new(
        EventTopics.SystemConfigurationChanged,
        SourceGenerationContext.Default.GetSystemConfigurationResult);
}

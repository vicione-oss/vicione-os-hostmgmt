using HostManagement.Shared.Communication.Contracts;

namespace HostManagement.Shared.Communication.NamedPipe.Client;

/// <summary>
/// Contains strongly typed named pipe request descriptors for capability-related contracts.
/// </summary>
public static class CapabilityPipeRequests
{
    /// <summary>
    /// Gets the set of supported capabilities.
    /// </summary>
    public static PipeRequest<GetSupportedCapabilitiesResult> GetSupportedCapabilities { get; } = new(
        Topics.GetSupportedCapabilities,
        CapabilitySourceGenerationContext.Default.GetSupportedCapabilitiesResult);
}

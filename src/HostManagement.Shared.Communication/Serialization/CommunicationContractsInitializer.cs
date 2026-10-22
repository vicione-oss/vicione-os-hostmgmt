using System.Runtime.CompilerServices;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Serialization;

namespace HostManagement.Shared.Communication.Serialization;

/// <summary>
/// Registers the communication contracts with the <see cref="MessageTypeRegistry"/>.
/// </summary>
internal static class CommunicationContractsInitializer
{
    /// <summary>
    /// Adds the <see cref="SourceGenerationContext"/> to the <see cref="MessageTypeRegistry"/>.
    /// </summary>
    // CA2255: Module initializers are discouraged in libraries because they run implicitly. That is exactly what is
    // wanted here, so consumers of this package never have to register the contract context themselves.
#pragma warning disable CA2255
    [ModuleInitializer]
#pragma warning restore CA2255
    internal static void Initialize() => MessageTypeRegistry.AddContext(SourceGenerationContext.Default);
}

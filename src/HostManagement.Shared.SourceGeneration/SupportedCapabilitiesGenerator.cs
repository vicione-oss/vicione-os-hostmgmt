using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace HostManagement.Shared.SourceGeneration;

/// <summary>
/// Represents a source generator that generates the <c>SupportedCapabilities</c> root class.
/// </summary>
[Generator]
public partial class SupportedCapabilitiesGenerator : IIncrementalGenerator
{
    private const string SharedCapabilitiesNamespace = "HostManagement.Shared.Capabilities";
    private const string CommunicationCapabilitiesNamespace = "HostManagement.Shared.Communication.Capabilities";

    private const string GeneratedSupportedCapabilitiesTypeName = "SupportedCapabilities";
    private const string SupportedSettingsPropertyName = "Settings";
    private const string SupportedTopicsPropertyName = "Topics";

    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static initializationContext =>
        {
            var (hintName, sourceText) = CodeGeneration.GenerateSupportedCapabilitiesClass();

            initializationContext.AddSource(hintName, SourceText.From(sourceText, Encoding.UTF8));
        });
    }
}

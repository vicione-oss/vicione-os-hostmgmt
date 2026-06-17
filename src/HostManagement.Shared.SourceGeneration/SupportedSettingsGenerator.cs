using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace HostManagement.Shared.SourceGeneration;

/// <summary>
/// Represents a source generator that builds a hierarchical settings model from properties
/// annotated with <c>SupportedSettingAttribute</c> and generates a <c>SupportedSettings</c> class
/// and one setting record per supported setting occurrence.
/// </summary>
[Generator]
public partial class SupportedSettingsGenerator : IIncrementalGenerator
{
    // Types defined in 'HostManagement.Shared' project
    private const string SystemConfigurationTypeName = "SystemConfiguration";
    private const string CapabilitiesNamespace = "HostManagement.Shared.Capabilities";
    private const string SupportedSettingAttributeFullName = CapabilitiesNamespace + ".SupportedSettingAttribute";
    private const string CapabilityTypeName = "Capability";
    private const string LeafSettingTypeName = "LeafSettingSupport";
    private const string CompositeSettingTypeName = "CompositeSettingSupport";

    // Generated types
    private const string GeneratedSupportedSettingsTypeName = "SupportedSettings";
    private const string GeneratedSettingTypeSuffix = "Support";

    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Find all property declarations with the "SupportedSetting" attribute and collect them
        var supportedSettingPropertiesProvider = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                SupportedSettingAttributeFullName,
                static (syntaxNode, _) => syntaxNode is PropertyDeclarationSyntax,
                static (ctx, _) => (IPropertySymbol)ctx.TargetSymbol)
            .Collect();

        context.RegisterSourceOutput(supportedSettingPropertiesProvider, (sourceProductionContext, properties) =>
        {
            // Generate 'SupportedSettings' class even if no properties with the corresponding attribute exist.
            // Consumers can rely on the fact that 'SupportedSettings' class exist.
            if (properties.IsDefault)
                return;

            sourceProductionContext.CancellationToken.ThrowIfCancellationRequested();

            var supportedSettingsModel = SupportedSettingsModelBuilder.Build(properties);

            if (SupportedSettingsModelDiagnostics.Report(sourceProductionContext, supportedSettingsModel))
                return;

            GenerateSources(sourceProductionContext, supportedSettingsModel);
        });
    }

    private static void GenerateSources(SourceProductionContext context, SupportedSettingsModel supportedSettingsModel)
    {
        foreach (var (recordHintName, recordSourceText) in CodeGeneration.GenerateSettingRecords(supportedSettingsModel))
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            context.AddSource(recordHintName, SourceText.From(recordSourceText, Encoding.UTF8));
        }

        var (settingsHintName, settingsSourceText) = CodeGeneration.GenerateSupportedSettingsClass(supportedSettingsModel);
        context.CancellationToken.ThrowIfCancellationRequested();
        context.AddSource(settingsHintName, SourceText.From(settingsSourceText, Encoding.UTF8));
    }
}

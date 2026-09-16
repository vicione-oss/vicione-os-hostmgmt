using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace HostManagement.Shared.SourceGeneration;

/// <summary>
/// Represents a source generator that builds a topics model from fields annotated with
/// <c>SupportedTopicAttribute</c> and generates a <c>SupportedTopics</c> class.
/// </summary>
[Generator]
public partial class SupportedTopicsGenerator : IIncrementalGenerator
{
    private const string CapabilitiesNamespace = "HostManagement.Shared.Capabilities";
    private const string CapabilityTypeName = "CapabilityStatus";

    private const string SupportedTopicAttributeFullName = CapabilitiesNamespace + ".SupportedTopicAttribute";
    private const string GeneratedSupportedTopicsTypeName = "SupportedTopics";

    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Find all variable declarations (fields) with the "SupportedTopic" attribute and collect them
        var supportedTopicFieldsProvider = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                SupportedTopicAttributeFullName,
                static (syntaxNode, _) => syntaxNode is VariableDeclaratorSyntax,
                static (ctx, _) => (IFieldSymbol)ctx.TargetSymbol)
            .Collect();

        context.RegisterSourceOutput(supportedTopicFieldsProvider, (sourceProductionContext, fields) =>
        {
            // Generate 'SupportedTopics' class even if no fields with the corresponding attribute exist.
            // Consumers can rely on the fact that 'SupportedTopics' class exist.
            if (fields.IsDefault)
                return;

            sourceProductionContext.CancellationToken.ThrowIfCancellationRequested();

            var supportedTopicsModel = SupportedTopicsModelBuilder.Build(fields);

            if (SupportedTopicsModelDiagnostics.Report(sourceProductionContext, supportedTopicsModel))
                return;

            GenerateSources(sourceProductionContext, supportedTopicsModel);
        });
    }

    private static void GenerateSources(SourceProductionContext context, SupportedTopicsModel supportedTopicsModel)
    {
        var (topicsHintName, topicsSourceText) = CodeGeneration.GenerateSupportedTopicsClass(supportedTopicsModel);

        context.CancellationToken.ThrowIfCancellationRequested();
        context.AddSource(topicsHintName, SourceText.From(topicsSourceText, Encoding.UTF8));
    }
}

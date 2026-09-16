using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace HostManagement.Shared.SourceGeneration;

/// <summary>
/// Represents a source generator to generate the "GetAllTopics" and "GetTopicsWithRequiredContent"
/// method for the "Topics" class.
/// </summary>
[Generator]
public class TopicsGenerator : IIncrementalGenerator
{
    private const string TopicsClassName = "Topics";
    private const string TopicType = "string";
    private const string RequiresContentAttribute = "RequiresContent";

    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var topicsClasses = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) =>
                {
                    return
                        node is ClassDeclarationSyntax syntax &&
                        syntax.Identifier.Text == TopicsClassName;
                },
                transform: static (context, cancellationToken) => CreateModel(context, cancellationToken))
            .Where(static model => model is not null);

        context.RegisterSourceOutput(topicsClasses, static (context, model) => GenerateSource(context, model!));
    }

    private static TopicsModel? CreateModel(GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
        var syntax = (ClassDeclarationSyntax)context.Node;

        // The generated part has to be declared in the same namespace as the class it completes. Reading the namespace
        // from the symbol instead of hard-coding it means moving the class cannot turn the generated part into a
        // separate class that happens to share its name.
        if (context.SemanticModel.GetDeclaredSymbol(syntax, cancellationToken) is not { } topicsSymbol)
            return null;

        var containingNamespace = topicsSymbol.ContainingNamespace.IsGlobalNamespace
            ? null
            : topicsSymbol.ContainingNamespace.ToDisplayString();

        return new TopicsModel(containingNamespace, GetTopics(syntax));
    }

    private static List<TopicField> GetTopics(ClassDeclarationSyntax syntax)
    {
        return syntax.Members
            .OfType<FieldDeclarationSyntax>()
            .Where(fieldSyntax =>
                fieldSyntax.Modifiers.Any(token => token.IsKind(SyntaxKind.ConstKeyword)) &&
                fieldSyntax.Declaration.Type.ToString() == TopicType)
            .Select(fieldSyntax =>
            {
                var variable = fieldSyntax.Declaration.Variables.First();
                var valueExpr = variable.Initializer?.Value;
                var value = valueExpr is LiteralExpressionSyntax literal && literal.IsKind(SyntaxKind.StringLiteralExpression)
                    ? literal.Token.ValueText
                    : valueExpr?.ToString() ?? "neverNull";

                var hasRequiresContent = fieldSyntax.AttributeLists
                    .SelectMany(attributeSyntax => attributeSyntax.Attributes)
                    .Any(attributeSyntax => attributeSyntax.Name.ToString().Contains(RequiresContentAttribute));

                return new TopicField(value, hasRequiresContent);
            })
            .ToList();
    }

    private static void GenerateSource(SourceProductionContext context, TopicsModel model)
    {
        var source = new StringBuilder();
        var indent = model.ContainingNamespace is null ? string.Empty : "    ";

        source.AppendLine("using System.Collections.Generic;");
        source.AppendLine();

        if (model.ContainingNamespace is not null)
        {
            source.AppendLine($"namespace {model.ContainingNamespace}");
            source.AppendLine("{");
        }

        source.AppendLine($"{indent}public static partial class {TopicsClassName}");
        source.AppendLine($"{indent}{{");
        source.AppendLine($"{indent}    /// <summary>");
        source.AppendLine($"{indent}    /// Gets all topics used to identify a message handler.");
        source.AppendLine($"{indent}    /// </summary>");
        source.AppendLine($"{indent}    /// <returns><see cref=\"IEnumerable{{String}}\"/></returns>");
        source.AppendLine($"{indent}    public static IEnumerable<string> GetAllTopics()");
        source.AppendLine($"{indent}    {{");

        foreach (var topic in model.Topics)
            source.AppendLine($"{indent}        yield return {SymbolDisplay.FormatLiteral(topic.Value, true)};");

        source.AppendLine($"{indent}    }}");
        source.AppendLine();
        source.AppendLine($"{indent}    /// <summary>");
        source.AppendLine($"{indent}    /// Gets all topics used to identify a message handler, that require content passed with the named pipe request.");
        source.AppendLine($"{indent}    /// </summary>");
        source.AppendLine($"{indent}    public static IEnumerable<string> GetTopicsWithRequiredContent()");
        source.AppendLine($"{indent}    {{");

        foreach (var topic in model.Topics.Where(field => field.HasRequiresContent))
            source.AppendLine($"{indent}        yield return {SymbolDisplay.FormatLiteral(topic.Value, true)};");

        source.AppendLine($"{indent}    }}");
        source.AppendLine($"{indent}}}");

        if (model.ContainingNamespace is not null)
            source.AppendLine("}");

        // The hint name carries the namespace so several topic classes in one compilation cannot collide.
        var hintName = model.ContainingNamespace is null
            ? $"{TopicsClassName}.GetAllTopics.g.cs"
            : $"{model.ContainingNamespace}.{TopicsClassName}.GetAllTopics.g.cs";

        context.AddSource(hintName, SourceText.From(source.ToString(), Encoding.UTF8));
    }

    private sealed class TopicField(string value, bool hasRequiresContent)
    {
        public string Value { get; } = value;

        public bool HasRequiresContent { get; } = hasRequiresContent;
    }

    private sealed class TopicsModel(string? containingNamespace, List<TopicField> topics)
    {
        /// <summary>
        /// The namespace the "Topics" class is declared in, or <see langword="null"/> for the global namespace.
        /// </summary>
        public string? ContainingNamespace { get; } = containingNamespace;

        public List<TopicField> Topics { get; } = topics;
    }
}

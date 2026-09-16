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
        var classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) =>
                {
                    return
                        node is ClassDeclarationSyntax syntax &&
                        syntax.Identifier.Text == TopicsClassName;
                },
                transform: static (context, _) => (ClassDeclarationSyntax)context.Node)
            .Where(static syntax => syntax is not null);

        var constFields = classDeclarations.Select((syntax, _) =>
            syntax.Members
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

                    return new
                    {
                        Value = value,
                        HasRequiresContent = fieldSyntax.AttributeLists
                            .SelectMany(attributeSyntax => attributeSyntax.Attributes)
                            .Any(attributeSyntax => attributeSyntax.Name.ToString().Contains(RequiresContentAttribute))
                    };
                })
                .ToList()
        );

        context.RegisterSourceOutput(constFields, (context, topics) =>
        {
            var source = new StringBuilder();

            source.AppendLine("using System.Collections.Generic;");
            source.AppendLine();
            source.AppendLine("namespace HostManagement.Shared.Communication");
            source.AppendLine("{");
            source.AppendLine("    public static partial class Topics");
            source.AppendLine("    {");
            source.AppendLine("        /// <summary>");
            source.AppendLine("        /// Gets all topics used to identify a message handler.");
            source.AppendLine("        /// </summary>");
            source.AppendLine("        /// <returns><see cref=\"IEnumerable{String}\"/></returns>");
            source.AppendLine("        public static IEnumerable<string> GetAllTopics()");
            source.AppendLine("        {");

            foreach (var topic in topics)
                source.AppendLine($"            yield return {SymbolDisplay.FormatLiteral(topic.Value, true)};");

            source.AppendLine("        }");
            source.AppendLine();
            source.AppendLine("        /// <summary>");
            source.AppendLine("        /// Gets all topics used to identify a message handler, that require content passed with the named pipe request.");
            source.AppendLine("        /// </summary>");
            source.AppendLine("        public static IEnumerable<string> GetTopicsWithRequiredContent()");
            source.AppendLine("        {");

            foreach (var topic in topics.Where(field => field.HasRequiresContent))
                source.AppendLine($"            yield return {SymbolDisplay.FormatLiteral(topic.Value, true)};");

            source.AppendLine("        }");
            source.AppendLine("    }");
            source.AppendLine("}");

            context.AddSource("Topics.GetAllTopics.g.cs", SourceText.From(source.ToString(), Encoding.UTF8));
        });
    }
}

using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace HostManagement.Shared.SourceGeneration;

/// <inheritdoc/>
public partial class SupportedTopicsGenerator
{
    private static class SupportedTopicsModelBuilder
    {
        public static SupportedTopicsModel Build(ImmutableArray<IFieldSymbol> fields)
        {
            var topicOccurrences = fields
                .Select(CreateOccurrence)
                .OrderBy(occurrence => occurrence.GeneratedPropertyName, StringComparer.Ordinal)
                .ToImmutableArray();

            return new SupportedTopicsModel(topicOccurrences);
        }

        private static TopicOccurrence CreateOccurrence(IFieldSymbol field)
        {
            var attribute = field
                .GetAttributes()
                .First(attribute => attribute.AttributeClass?.ToDisplayString() == SupportedTopicAttributeFullName);

            var explicitTopicName = attribute.ConstructorArguments.Length > 0
                ? attribute.ConstructorArguments[0].Value as string
                : null;

            var topicName = string.IsNullOrWhiteSpace(explicitTopicName)
                ? field.Name
                : explicitTopicName!;

            return new TopicOccurrence(field, topicName, explicitTopicName, SanitizeIdentifier(topicName));
        }

        private static string SanitizeIdentifier(string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
                return "_";

            var stringBuilder = new StringBuilder(identifier.Length);

            if (!SyntaxFacts.IsIdentifierStartCharacter(identifier[0]))
                stringBuilder.Append('_');

            foreach (var character in identifier)
                stringBuilder.Append(SyntaxFacts.IsIdentifierPartCharacter(character) ? character : '_');

            return stringBuilder.ToString();
        }
    }
}

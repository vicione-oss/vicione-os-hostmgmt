using Microsoft.CodeAnalysis;

namespace HostManagement.Shared.SourceGeneration;

/// <inheritdoc/>
public partial class SupportedTopicsGenerator
{
    private static class SupportedTopicsModelDiagnostics
    {
        private const string UsageCategory = "Usage";

        private static readonly DiagnosticDescriptor s_emptyTopicNameDescriptor = new(
            "STG001",
            "Supported topic name must not be empty",
            "Supported topic name on '{0}' must not be empty or whitespace",
            UsageCategory,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Explicit supported topic names are used as generated property names and must not be empty.");

        private static readonly DiagnosticDescriptor s_duplicateTopicNameDescriptor = new(
            "STG002",
            "Duplicate supported topic name",
            "Supported topic name '{0}' is ambiguous",
            UsageCategory,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Supported topic names are used as generated property names and must be unique.");

        private static readonly DiagnosticDescriptor s_lowercaseTopicNameDescriptor = new(
            "STG003",
            "Supported topic name must not start with a lowercase letter",
            "Supported topic name '{0}' on '{1}' must not start with a lowercase letter",
            UsageCategory,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Explicit supported topic names must not start with a lowercase letter.");

        /// <summary>
        /// Reports all diagnostics found in the supported topics model.
        /// </summary>
        /// <param name="context">The source production context used to report diagnostics.</param>
        /// <param name="model">The supported topics model to validate.</param>
        /// <returns>
        /// <see langword="true"/> if at least one reported diagnostic has error severity, otherwise <see langword="false"/>.
        /// </returns>
        public static bool Report(SourceProductionContext context, SupportedTopicsModel model)
        {
            var hasErrors = false;

            foreach (var diagnostic in Validate(model))
            {
                context.ReportDiagnostic(diagnostic);
                hasErrors |= diagnostic.Severity == DiagnosticSeverity.Error;
            }

            return hasErrors;
        }

        private static IEnumerable<Diagnostic> Validate(SupportedTopicsModel model)
        {
            foreach (var diagnostic in ValidateEmptyExplicitTopicNames(model))
                yield return diagnostic;

            foreach (var diagnostic in ValidateExplicitTopicNamesStartingWithLowercaseLetter(model))
                yield return diagnostic;

            foreach (var diagnostic in ValidateDuplicateTopicNames(model))
                yield return diagnostic;
        }

        private static IEnumerable<Diagnostic> ValidateEmptyExplicitTopicNames(SupportedTopicsModel model)
        {
            foreach (var occurrence in model.TopicOccurrences)
            {
                if (occurrence.ExplicitTopicName is not null &&
                    string.IsNullOrWhiteSpace(occurrence.ExplicitTopicName))
                {
                    var location = GetDiagnosticLocation(occurrence);

                    yield return Diagnostic.Create(s_emptyTopicNameDescriptor, location, occurrence.FieldName);
                }
            }
        }

        private static IEnumerable<Diagnostic> ValidateExplicitTopicNamesStartingWithLowercaseLetter(
            SupportedTopicsModel model)
        {
            foreach (var occurrence in model.TopicOccurrences)
            {
                if (occurrence.ExplicitTopicName is { Length: > 0 } explicitTopicName &&
                    char.IsLower(explicitTopicName[0]))
                {
                    var location = GetDiagnosticLocation(occurrence);

                    yield return Diagnostic.Create(
                        s_lowercaseTopicNameDescriptor,
                        location,
                        explicitTopicName,
                        occurrence.FieldName);
                }
            }
        }

        private static IEnumerable<Diagnostic> ValidateDuplicateTopicNames(SupportedTopicsModel model)
        {
            var duplicateTopicNameGroups = model.TopicOccurrences
                .GroupBy(occurrence => occurrence.GeneratedPropertyName, StringComparer.Ordinal)
                .Where(group => group.Count() > 1)
                .OrderBy(group => group.Key, StringComparer.Ordinal);

            foreach (var duplicateTopicNameGroup in duplicateTopicNameGroups)
            {
                foreach (var occurrence in duplicateTopicNameGroup)
                {
                    var location = GetDiagnosticLocation(occurrence);

                    yield return Diagnostic.Create(s_duplicateTopicNameDescriptor, location, occurrence.TopicName);
                }
            }
        }

        private static Location? GetDiagnosticLocation(TopicOccurrence occurrence)
        {
            return occurrence.FieldSymbol.Locations.FirstOrDefault(location => location.IsInSource)
                ?? occurrence.FieldSymbol.Locations.FirstOrDefault();
        }
    }
}

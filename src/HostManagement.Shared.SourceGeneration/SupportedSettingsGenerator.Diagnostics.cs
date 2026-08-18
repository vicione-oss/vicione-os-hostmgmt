using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace HostManagement.Shared.SourceGeneration;

/// <inheritdoc/>
public partial class SupportedSettingsGenerator
{
    private static class SupportedSettingsModelDiagnostics
    {
        private const string UsageCategory = "Usage";

        private static readonly DiagnosticDescriptor s_emptySettingNameDescriptor = new(
            "SSG001",
            "Supported setting name must not be empty",
            "Supported setting name on '{0}' must not be empty or whitespace",
            UsageCategory,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Explicit supported setting names are used as generated property names and must not be empty.");

        private static readonly DiagnosticDescriptor s_duplicateSettingNameDescriptor = new(
            "SSG002",
            "Duplicate supported setting name in parent scope",
            "Supported setting name '{0}' is ambiguous in '{1}'",
            UsageCategory,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Supported setting names are used as generated property names and must be unique within parent scope.");

        private static readonly DiagnosticDescriptor s_lowercaseSettingNameDescriptor = new(
            "SSG003",
            "Supported setting name must not start with a lowercase letter",
            "Supported setting name '{0}' on '{1}' must not start with a lowercase letter",
            UsageCategory,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Explicit supported setting names must not start with a lowercase letter.");

        /// <summary>
        /// Reports all diagnostics found in the supported settings model.
        /// </summary>
        /// <param name="context">The source production context used to report diagnostics.</param>
        /// <param name="model">The supported settings model to validate.</param>
        /// <returns>
        /// <see langword="true"/> if at least one reported diagnostic has error severity, otherwise <see langword="false"/>.
        /// </returns>
        public static bool Report(SourceProductionContext context, SupportedSettingsModel model)
        {
            var hasErrors = false;

            foreach (var diagnostic in Validate(model))
            {
                context.ReportDiagnostic(diagnostic);
                hasErrors |= diagnostic.Severity == DiagnosticSeverity.Error;
            }

            return hasErrors;
        }

        private static IEnumerable<Diagnostic> Validate(SupportedSettingsModel model)
        {
            foreach (var diagnostic in ValidateEmptyExplicitSettingNames(model))
                yield return diagnostic;

            foreach (var diagnostic in ValidateExplicitSettingNamesStartingWithLowercaseLetter(model))
                yield return diagnostic;

            foreach (var diagnostic in ValidateDuplicateSettingNamesInParentScope(model))
                yield return diagnostic;
        }

        private static IEnumerable<Diagnostic> ValidateEmptyExplicitSettingNames(SupportedSettingsModel model)
        {
            foreach (var occurrence in model.AllOccurrences)
            {
                if (occurrence.ExplicitSettingName is not null &&
                    string.IsNullOrWhiteSpace(occurrence.ExplicitSettingName))
                {
                    var location = GetDiagnosticLocation(occurrence);

                    yield return Diagnostic.Create(s_emptySettingNameDescriptor, location, occurrence.PropertyName);
                }
            }
        }

        private static IEnumerable<Diagnostic> ValidateExplicitSettingNamesStartingWithLowercaseLetter(
            SupportedSettingsModel model)
        {
            foreach (var occurrence in model.AllOccurrences)
            {
                if (occurrence.ExplicitSettingName is { Length: > 0 } explicitSettingName &&
                    char.IsLower(explicitSettingName[0]))
                {
                    var location = GetDiagnosticLocation(occurrence);

                    yield return Diagnostic.Create(
                        s_lowercaseSettingNameDescriptor,
                        location,
                        explicitSettingName,
                        occurrence.PropertyName);
                }
            }
        }

        private static IEnumerable<Diagnostic> ValidateDuplicateSettingNamesInParentScope(SupportedSettingsModel model)
        {
            foreach (var diagnostic in ValidateDuplicateSettingNamesInScope(
                model.RootOccurrences,
                GeneratedSupportedSettingsTypeName))
            {
                yield return diagnostic;
            }

            var settingRecordModels = model.SettingRecordModels.Values
                .OrderBy(settingRecordModel => settingRecordModel.RecordName, StringComparer.Ordinal);

            foreach (var settingRecordModel in settingRecordModels)
            {
                foreach (var diagnostic in ValidateDuplicateSettingNamesInScope(
                    settingRecordModel.ChildOccurrences,
                    settingRecordModel.RecordName))
                {
                    yield return diagnostic;
                }
            }
        }

        private static IEnumerable<Diagnostic> ValidateDuplicateSettingNamesInScope(
            ImmutableArray<SettingOccurrence> occurrences,
            string scopeName)
        {
            var duplicateSettingNameGroups = occurrences
                .GroupBy(occurrence => occurrence.GeneratedPropertyName, StringComparer.Ordinal)
                .Where(group => group.Count() > 1)
                .OrderBy(group => group.Key, StringComparer.Ordinal);

            foreach (var duplicateSettingNameGroup in duplicateSettingNameGroups)
            {
                foreach (var occurrence in duplicateSettingNameGroup)
                {
                    var location = GetDiagnosticLocation(occurrence);

                    yield return Diagnostic.Create(
                        s_duplicateSettingNameDescriptor,
                        location,
                        occurrence.SettingName,
                        scopeName);
                }
            }
        }

        private static Location? GetDiagnosticLocation(SettingOccurrence occurrence)
        {
            return occurrence.PropertySymbol.Locations.FirstOrDefault(location => location.IsInSource)
                ?? occurrence.PropertySymbol.Locations.FirstOrDefault();
        }
    }
}

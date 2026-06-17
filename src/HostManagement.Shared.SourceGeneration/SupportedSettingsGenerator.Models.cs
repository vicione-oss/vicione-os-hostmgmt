using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace HostManagement.Shared.SourceGeneration;

/// <inheritdoc/>
public partial class SupportedSettingsGenerator
{
    private sealed class SettingOccurrence
    {
        public SettingOccurrence(
            IPropertySymbol propertySymbol,
            ITypeSymbol declaringType,
            ITypeSymbol settingType,
            string propertyName,
            string settingName,
            string? explicitSettingName,
            string generatedPropertyName,
            bool isSyntheticCollectionElement)
        {
            PropertySymbol = propertySymbol;
            DeclaringType = declaringType;
            SettingType = settingType;
            PropertyName = propertyName;
            SettingName = settingName;
            ExplicitSettingName = explicitSettingName;
            GeneratedPropertyName = generatedPropertyName;
            IsSyntheticCollectionElement = isSyntheticCollectionElement;
            PropertyPathSegments = ImmutableArray<string>.Empty;
        }

        public IPropertySymbol PropertySymbol { get; }
        public ITypeSymbol DeclaringType { get; }
        public ITypeSymbol SettingType { get; }

        /// <summary>
        /// Original source property name or synthetic element type name. Used to derive stable record type names.
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// Explicit setting name from 'SupportedSettingAttribute' or the original property name,
        /// e.g. "DNS" or "NetworkDNSSettings".
        /// </summary>
        public string SettingName { get; }

        /// <summary>
        /// Raw explicit setting name from 'SupportedSettingAttribute', if one was provided.
        /// </summary>
        public string? ExplicitSettingName { get; }

        /// <summary>
        /// Sanitized setting name used as generated property name.
        /// </summary>
        public string GeneratedPropertyName { get; }

        public bool IsSyntheticCollectionElement { get; }

        /// <summary>
        /// Hierarchical source-property path used to create collision-resistant record names.
        /// </summary>
        public ImmutableArray<string> PropertyPathSegments { get; set; }
    }

    private sealed class SettingRecordModel
    {
        public SettingRecordModel(
            SettingOccurrence occurrence,
            string recordName,
            ImmutableArray<SettingOccurrence> childOccurrences)
        {
            Occurrence = occurrence;
            RecordName = recordName;
            ChildOccurrences = childOccurrences;
        }

        /// <summary>
        /// The setting occurrence that describes the record to generate.
        /// </summary>
        public SettingOccurrence Occurrence { get; }

        /// <summary>
        /// The name of the generated record.
        /// </summary>
        public string RecordName { get; }

        /// <summary>
        /// The child setting occurrences associated with the record.
        /// </summary>
        public ImmutableArray<SettingOccurrence> ChildOccurrences { get; }
    }

    private sealed class SupportedSettingsModel
    {
        public SupportedSettingsModel(
            ImmutableArray<SettingOccurrence> allOccurrences,
            ImmutableArray<SettingOccurrence> rootOccurrences,
            ImmutableDictionary<SettingOccurrence, SettingRecordModel> settingRecordModels)
        {
            AllOccurrences = allOccurrences;
            RootOccurrences = rootOccurrences;
            SettingRecordModels = settingRecordModels;
        }

        public ImmutableArray<SettingOccurrence> AllOccurrences { get; }
        public ImmutableArray<SettingOccurrence> RootOccurrences { get; }
        public ImmutableDictionary<SettingOccurrence, SettingRecordModel> SettingRecordModels { get; }
    }
}

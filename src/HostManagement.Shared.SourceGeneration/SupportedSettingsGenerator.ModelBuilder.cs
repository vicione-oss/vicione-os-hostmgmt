using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace HostManagement.Shared.SourceGeneration;

/// <inheritdoc/>
public partial class SupportedSettingsGenerator
{
    private static class SupportedSettingsModelBuilder
    {
        /// <summary>
        /// Builds the supported settings model from the provided property symbols.
        /// </summary>
        /// <param name="properties">The property symbols annotated with the supported setting attribute.</param>
        /// <returns>The supported settings model created from the provided property symbols.</returns>
        public static SupportedSettingsModel Build(ImmutableArray<IPropertySymbol> properties)
        {
            var propertyOccurrences = CreatePropertyOccurrences(properties);
            var occurrencesByType = GroupByDeclaringType(propertyOccurrences);
            var collectionElements = CreateCollectionElements(propertyOccurrences, occurrencesByType);

            var roots = GetRoots(propertyOccurrences);
            var allOccurrences = propertyOccurrences.AddRange(collectionElements.Values);
            var childrenByOccurrence = CreateChildrenByOccurrence(allOccurrences, occurrencesByType, collectionElements);

            AssignPathSegments(roots, childrenByOccurrence);

            var recordModels = CreateRecordModels(allOccurrences, childrenByOccurrence).ToImmutableDictionary();

            return new SupportedSettingsModel(allOccurrences, roots, recordModels);
        }

        private static ImmutableArray<SettingOccurrence> CreatePropertyOccurrences(
            ImmutableArray<IPropertySymbol> properties)
                => properties.Select(CreatePropertyOccurrence).ToImmutableArray();

        private static SettingOccurrence CreatePropertyOccurrence(IPropertySymbol property)
        {
            var attribute = property
                .GetAttributes()
                .First(attribute => attribute.AttributeClass?.ToDisplayString() == SupportedSettingAttributeFullName);

            var explicitSettingName = attribute.ConstructorArguments.Length > 0
                ? attribute.ConstructorArguments[0].Value as string
                : null;

            var settingName = string.IsNullOrWhiteSpace(explicitSettingName)
                ? property.Name
                : explicitSettingName!;

            return new SettingOccurrence(
                property,
                property.ContainingType,
                property.Type,
                property.Name,
                settingName,
                explicitSettingName,
                SanitizeIdentifier(settingName),
                isSyntheticCollectionElement: false);
        }

        private static ImmutableArray<SettingOccurrence> GetRoots(ImmutableArray<SettingOccurrence> occurrences)
            => occurrences
                .Where(occurrence => occurrence.DeclaringType.Name == SystemConfigurationTypeName)
                .ToImmutableArray();

        private static Dictionary<ITypeSymbol, ImmutableArray<SettingOccurrence>> GroupByDeclaringType(
            ImmutableArray<SettingOccurrence> occurrences)
        {
            var comparer = SymbolEqualityComparer.Default;
            var occurrencesByDeclaringTypeLists = new Dictionary<ITypeSymbol, List<SettingOccurrence>>(comparer);

            foreach (var occurrence in occurrences)
            {
                var declaringType = occurrence.DeclaringType;

                if (!occurrencesByDeclaringTypeLists.TryGetValue(declaringType, out var declaringTypeOccurrences))
                {
                    declaringTypeOccurrences = new List<SettingOccurrence>();
                    occurrencesByDeclaringTypeLists.Add(declaringType, declaringTypeOccurrences);
                }

                declaringTypeOccurrences.Add(occurrence);
            }

            var occurrencesByDeclaringType = new Dictionary<ITypeSymbol, ImmutableArray<SettingOccurrence>>(comparer);

            foreach (var occurrenceByDeclaringType in occurrencesByDeclaringTypeLists)
                occurrencesByDeclaringType[occurrenceByDeclaringType.Key] = occurrenceByDeclaringType.Value.ToImmutableArray();

            return occurrencesByDeclaringType;
        }

        private static Dictionary<SettingOccurrence, SettingOccurrence> CreateCollectionElements(
            ImmutableArray<SettingOccurrence> occurrences,
            Dictionary<ITypeSymbol, ImmutableArray<SettingOccurrence>> occurrencesByType)
        {
            var collectionElementOccurrences = new Dictionary<SettingOccurrence, SettingOccurrence>();

            foreach (var occurrence in occurrences)
            {
                if (!TryGetCollectionElementType(occurrence.SettingType, out var elementType))
                    continue;

                if (!occurrencesByType.ContainsKey(elementType))
                    continue;

                var elementTypeName = elementType.Name;

                collectionElementOccurrences[occurrence] = new SettingOccurrence(
                    occurrence.PropertySymbol,
                    occurrence.SettingType,
                    elementType,
                    elementTypeName,
                    elementTypeName,
                    explicitSettingName: null,
                    SanitizeIdentifier(elementTypeName),
                    isSyntheticCollectionElement: true);
            }

            return collectionElementOccurrences;
        }

        private static Dictionary<SettingOccurrence, ImmutableArray<SettingOccurrence>> CreateChildrenByOccurrence(
            ImmutableArray<SettingOccurrence> occurrences,
            Dictionary<ITypeSymbol, ImmutableArray<SettingOccurrence>> occurrencesByType,
            Dictionary<SettingOccurrence, SettingOccurrence> collectionElements)
        {
            var childOccurrencesByOccurrence = new Dictionary<SettingOccurrence, ImmutableArray<SettingOccurrence>>();

            foreach (var occurrence in occurrences)
            {
                childOccurrencesByOccurrence[occurrence] =
                    collectionElements.TryGetValue(occurrence, out var collectionElement)
                        ? ImmutableArray.Create(collectionElement)
                        : occurrencesByType.TryGetValue(occurrence.SettingType, out var typeChildren)
                            ? typeChildren
                            : ImmutableArray<SettingOccurrence>.Empty;
            }

            return childOccurrencesByOccurrence;
        }

        private static void AssignPathSegments(
            ImmutableArray<SettingOccurrence> roots,
            Dictionary<SettingOccurrence, ImmutableArray<SettingOccurrence>> childrenByOccurrence)
        {
            foreach (var root in roots)
                AssignPath(root, ImmutableArray<string>.Empty, childrenByOccurrence, new HashSet<SettingOccurrence>());
        }

        private static void AssignPath(
            SettingOccurrence occurrence,
            ImmutableArray<string> path,
            Dictionary<SettingOccurrence, ImmutableArray<SettingOccurrence>> childrenByOccurrence,
            HashSet<SettingOccurrence> visited)
        {
            if (!visited.Add(occurrence))
                return;

            var currentPath = path.Add(occurrence.PropertyName);

            occurrence.PropertyPathSegments = currentPath;

            if (!childrenByOccurrence.TryGetValue(occurrence, out var childOccurrences))
                return;

            foreach (var child in childOccurrences)
                AssignPath(child, currentPath, childrenByOccurrence, visited);
        }

        private static Dictionary<SettingOccurrence, SettingRecordModel> CreateRecordModels(
            ImmutableArray<SettingOccurrence> occurrences,
            Dictionary<SettingOccurrence, ImmutableArray<SettingOccurrence>> childrenByOccurrence)
        {
            var recordNames = CreateRecordNames(occurrences);
            var settingRecordModels = new Dictionary<SettingOccurrence, SettingRecordModel>();

            foreach (var occurrence in occurrences)
            {
                var childOccurrences = childrenByOccurrence.TryGetValue(occurrence, out var childOccurrence)
                    ? childOccurrence
                    : ImmutableArray<SettingOccurrence>.Empty;

                settingRecordModels[occurrence] =
                    new SettingRecordModel(occurrence, recordNames[occurrence], childOccurrences);
            }

            return settingRecordModels;
        }

        private static Dictionary<SettingOccurrence, string> CreateRecordNames(
            ImmutableArray<SettingOccurrence> occurrences)
        {
            var comparer = StringComparer.Ordinal;
            var recordNames = new Dictionary<SettingOccurrence, string>();

            var groups = occurrences
                .GroupBy(occurrence => SanitizeIdentifier(occurrence.PropertyName), comparer)
                .OrderBy(group => group.Key, comparer);

            foreach (var group in groups)
            {
                var groupOccurrences = group
                    .OrderBy(occurrence => occurrence.PropertyPathSegments.Length)
                    .ThenBy(occurrence => string.Join(".", occurrence.PropertyPathSegments), comparer)
                    .ToArray();

                if (groupOccurrences.Length == 1)
                {
                    recordNames[groupOccurrences[0]] = group.Key;
                    continue;
                }

                AssignUniqueNamesForCollidingOccurrences(groupOccurrences, recordNames);
            }

            return recordNames;
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

        private static bool TryGetCollectionElementType(ITypeSymbol type, out ITypeSymbol elementType)
        {
            if (type is IArrayTypeSymbol arrayType)
            {
                elementType = arrayType.ElementType;
                return true;
            }

            if (type.SpecialType == SpecialType.System_String || type is not INamedTypeSymbol namedType)
            {
                elementType = type;
                return false;
            }

            foreach (var candidate in namedType.AllInterfaces.Prepend(namedType))
            {
                if (candidate is not INamedTypeSymbol namedCandidate || !IsGenericEnumerable(namedCandidate))
                    continue;

                elementType = namedCandidate.TypeArguments[0];
                return true;
            }

            elementType = type;
            return false;
        }

        private static bool IsGenericEnumerable(INamedTypeSymbol type)
            => type.MetadataName == "IEnumerable`1" &&
               type.ContainingNamespace.ToDisplayString() == "System.Collections.Generic";

        private static void AssignUniqueNamesForCollidingOccurrences(
            SettingOccurrence[] occurrences,
            Dictionary<SettingOccurrence, string> recordNames)
        {
            var usedNames = new HashSet<string>(StringComparer.Ordinal);

            for (var i = 0; i < occurrences.Length; i++)
            {
                var occurrence = occurrences[i];
                var recordName = CreatePathBasedRecordName(occurrence);

                if (!usedNames.Add(recordName))
                {
                    var suffix = 2;

                    while (!usedNames.Add(recordName + suffix.ToString(CultureInfo.InvariantCulture)))
                        suffix++;

                    recordName += suffix.ToString(CultureInfo.InvariantCulture);
                }

                recordNames[occurrence] = recordName;
            }
        }

        private static string CreatePathBasedRecordName(SettingOccurrence occurrence)
            => occurrence.PropertyPathSegments.IsDefaultOrEmpty
                ? SanitizeIdentifier(occurrence.PropertyName)
                : string.Concat(occurrence.PropertyPathSegments.Select(SanitizeIdentifier));
    }
}

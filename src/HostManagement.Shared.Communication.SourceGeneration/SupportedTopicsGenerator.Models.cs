using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace HostManagement.Shared.Communication.SourceGeneration;

/// <inheritdoc/>
public partial class SupportedTopicsGenerator
{
    private sealed class TopicOccurrence
    {
        public IFieldSymbol FieldSymbol { get; }

        /// <summary>
        /// Original source field name.
        /// </summary>
        public string FieldName { get; }

        /// <summary>
        /// Explicit topic name from 'SupportedTopicAttribute' or the original field name,
        /// e.g. "GetNetworkStatus" or "GetNetworkStatusInformation".
        /// </summary>
        public string TopicName { get; }

        /// <summary>
        /// Raw explicit topic name from 'SupportedTopicAttribute', if one was provided.
        /// </summary>
        public string? ExplicitTopicName { get; }

        /// <summary>
        /// Sanitized topic name used as generated property name.
        /// </summary>
        public string GeneratedPropertyName { get; }

        public TopicOccurrence(
            IFieldSymbol fieldSymbol,
            string topicName,
            string? explicitTopicName,
            string generatedPropertyName)
        {
            FieldSymbol = fieldSymbol;
            FieldName = fieldSymbol.Name;
            TopicName = topicName;
            ExplicitTopicName = explicitTopicName;
            GeneratedPropertyName = generatedPropertyName;
        }
    }

    private sealed class SupportedTopicsModel
    {
        public ImmutableArray<TopicOccurrence> TopicOccurrences { get; }

        public SupportedTopicsModel(ImmutableArray<TopicOccurrence> topicOccurrences)
            => TopicOccurrences = topicOccurrences;
    }
}

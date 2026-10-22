using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Contracts.Service;
using SharedConfiguration = HostManagement.Shared.Contracts.SystemConfiguration;

namespace HostManagement.SystemConfig;

/// <summary>
/// Merges explicitly proposed system configuration properties into the current public configuration.
/// </summary>
/// <remarks>
/// The proposed configuration must contain a <c>Version</c> equal to the current contract version.<br/>
/// Omitted properties are preserved.<br/>
/// Existing network interfaces are merged by name, while proposed interfaces that are not
/// currently present are ignored.<br/>
/// Services are merged by name and new services can be added.<br/>
/// IPv4 details, static hosts and supported scalar collections replace their existing lists when submitted.<br/>
/// Unrecognized collection paths are rejected.
/// </remarks>
internal static class SharedSystemConfigurationMerger
{
    /// <summary>
    /// Produces a new configuration containing the properties of the proposed configuration
    /// and all unsubmitted current values.
    /// </summary>
    /// <param name="currentConfiguration">The current configuration to preserve and update.</param>
    /// <param name="proposedConfigurationJson">The proposed configuration JSON.</param>
    /// <returns>A separate, merged configuration instance.</returns>
    /// <exception cref="ArgumentNullException">The current configuration is null.</exception>
    /// <exception cref="JsonException">
    /// The proposed configuration JSON shape, contract version, identity, or a known value is invalid.
    /// </exception>
    /// <exception cref="NotSupportedException">A submitted collection has no defined merge strategy.</exception>
    internal static SharedConfiguration Merge(
        SharedConfiguration currentConfiguration,
        JsonElement proposedConfigurationJson)
    {
        ArgumentNullException.ThrowIfNull(currentConfiguration);

        if (proposedConfigurationJson.ValueKind != JsonValueKind.Object)
            throw new JsonException("Proposed configuration must be a JSON object.");

        ValidateVersion(currentConfiguration.Version, proposedConfigurationJson);

        var mergedJson = JsonSerializer.SerializeToNode(
            currentConfiguration,
            SourceGenerationContext.Default.SystemConfiguration)
            as JsonObject
            ?? throw new InvalidOperationException("Current configuration could not be serialized as a json object.");

        ApplyProperties(mergedJson, proposedConfigurationJson, string.Empty);

        return JsonSerializer.Deserialize(
            mergedJson.ToJsonString(),
            SourceGenerationContext.Default.SystemConfiguration)
            ?? throw new JsonException("The merged configuration could not be deserialized.");
    }

    private static void ValidateVersion(int currentVersion, JsonElement proposed)
    {
        if (!proposed.TryGetProperty(nameof(SharedConfiguration.Version), out var version))
            throw new JsonException("Configuration version is required.");

        if (version.ValueKind != JsonValueKind.Number || !version.TryGetInt32(out var proposedVersion))
            throw new JsonException("Configuration version must be an integer.");

        if (proposedVersion != currentVersion)
            throw new JsonException($"Unsupported configuration version {proposedVersion} (expected {currentVersion}).");
    }

    private static void ApplyProperties(JsonObject current, JsonElement proposed, string basePath)
    {
        var proposedPropertyNames = new HashSet<string>(StringComparer.Ordinal);

        foreach (var property in proposed.EnumerateObject())
        {
            var name = property.Name;
            var value = property.Value;
            var path = $"{basePath}/{name}";

            if (!proposedPropertyNames.Add(name))
                throw new JsonException($"Duplicate configuration property '{path}'.");

            if (basePath.Length == 0 && property.NameEquals(nameof(SharedConfiguration.Version)))
                continue; // Already validated in Merge; current version is never overwritten

            if (!current.ContainsKey(name))
                throw new JsonException($"Unknown configuration property '{path}'.");

            if (value.ValueKind == JsonValueKind.Object)
            {
                if (current[name] is not JsonObject currentObject)
                    throw new JsonException($"Expected an existing configuration object at '{path}'.");

                ApplyProperties(currentObject, value, path);
            }
            else if (value.ValueKind == JsonValueKind.Array)
            {
                current[name] = ReplaceOrMergeCollection(current[name] as JsonArray, value, path);
            }
            else
            {
                current[name] = JsonNode.Parse(value.GetRawText());
            }
        }
    }

    private static JsonArray ReplaceOrMergeCollection(JsonArray? current, JsonElement proposed, string path)
    {
        var collectionType = GetCollectionType(path);

        return collectionType switch
        {
            CollectionType.Values => ReplaceValueCollection(proposed, path),
            CollectionType.IPv4Details or CollectionType.StaticHosts => ReplaceObjectCollection(proposed, collectionType, path),
            CollectionType.NetworkInterfaces or CollectionType.Services => MergeNamedCollection(current, proposed, collectionType, path),
            _ => throw new InvalidOperationException($"Unsupported collection type '{collectionType}'.")
        };
    }

    private static JsonArray ReplaceValueCollection(JsonElement proposed, string path)
    {
        if (proposed.EnumerateArray().Any(static item =>
                item.ValueKind is JsonValueKind.Object or JsonValueKind.Array))
        {
            // Only scalar collections are safely replaceable without stable item identities.
            throw new NotSupportedException($"Object collection '{path}' has no defined identity strategy.");
        }

        return JsonNode.Parse(proposed.GetRawText()) as JsonArray
            ?? throw new JsonException($"Invalid collection '{path}'.");
    }

    private static JsonArray ReplaceObjectCollection(JsonElement proposed, CollectionType collectionType, string path)
    {
        var result = new JsonArray();
        var index = 0;

        foreach (var item in proposed.EnumerateArray())
        {
            var itemPath = $"{path}/{index}";

            if (item.ValueKind != JsonValueKind.Object)
                throw new JsonException($"Expected an object at '{itemPath}'.");

            ApplyProperties(CreateItemTemplate(collectionType), item, itemPath);

            result.Add((JsonNode)(JsonNode.Parse(item.GetRawText()) as JsonObject
                ?? throw new JsonException($"Invalid object at '{itemPath}'.")));

            index++;
        }

        return result;
    }

    private static JsonArray MergeNamedCollection(
        JsonArray? current,
        JsonElement proposed,
        CollectionType collectionType,
        string path)
    {
        // Clone the current collection to preserve the original.
        var result = current is not null ? (JsonArray)current.DeepClone() : [];

        // Build an index of existing collection items.
        var existingIndexes = new Dictionary<string, int>(StringComparer.Ordinal);

        for (var i = 0; i < result.Count; i++)
        {
            if (result[i] is not JsonObject item)
                throw new JsonException($"Expected an object at '{path}/{i}'.");

            var identity = GetIdentity(item, collectionType, $"{path}/{i}");

            if (!existingIndexes.TryAdd(identity, i))
                throw new JsonException($"Duplicate collection identity at '{path}/{i}'.");
        }

        // Apply the submitted collection items.
        var proposedIdentities = new HashSet<string>(StringComparer.Ordinal);

        for (var i = 0; i < proposed.GetArrayLength(); i++)
        {
            var proposedItem = proposed[i];
            var itemPath = $"{path}/{i}";

            if (proposedItem.ValueKind != JsonValueKind.Object)
                throw new JsonException($"Expected an object at '{itemPath}'.");

            var identity = GetIdentity(proposedItem, collectionType, itemPath);

            if (!proposedIdentities.Add(identity))
                throw new JsonException($"Duplicate collection identity at '{itemPath}'.");

            if (existingIndexes.TryGetValue(identity, out var existingIndex))
            {
                // Merge directly into the cloned existing item.
                var existingItem = (JsonObject)result[existingIndex]!;
                ApplyProperties(existingItem, proposedItem, itemPath);
            }
            else
            {
                var template = CreateItemTemplate(collectionType);
                ApplyProperties(template, proposedItem, itemPath);

                if (collectionType == CollectionType.NetworkInterfaces)
                {
                    // Unknown interfaces are validated but not added.
                    _ = JsonSerializer.Deserialize(
                        template.ToJsonString(),
                        SourceGenerationContext.Default.NetworkInterfaceDetail);

                    continue;
                }

                // New services are allowed.
                var newItem = JsonNode.Parse(proposedItem.GetRawText()) as JsonObject
                    ?? throw new JsonException($"Invalid object at '{itemPath}'.");

                result.Add((JsonNode)newItem);
            }
        }

        return result;
    }

    private static CollectionType GetCollectionType(string path)
    {
        if (path == "/NetworkInterfaces")
            return CollectionType.NetworkInterfaces;

        if (path == "/Services")
            return CollectionType.Services;

        if (path.StartsWith("/NetworkInterfaces/", StringComparison.Ordinal) &&
            path.EndsWith("/IPv4/IPv4Details", StringComparison.Ordinal))
        {
            return CollectionType.IPv4Details;
        }

        if (path == "/NetworkDNSSettings/StaticHosts/Hosts")
            return CollectionType.StaticHosts;

        return path switch
        {
            "/NetworkNTPSettings/Servers"
            or "/NetworkDNSSettings/NameServers/Addresses"
            or "/NetworkDNSSettings/SearchDomains/Domains"
            or "/NetworkProxySettings/NoProxy/Entries"
            or "/Syslog/ExcludeSyslogFacilities"
            or "/Syslog/ExcludeSyslogLevels"
                => CollectionType.Values,

            _ => throw new NotSupportedException($"No collection merge strategy defined for '{path}'.")
        };
    }

    private static string GetIdentity(JsonObject jsonObject, CollectionType type, string path)
    {
        var item = jsonObject;

        if (type == CollectionType.NetworkInterfaces)
        {
            item = jsonObject["CommonInformation"] as JsonObject
                ?? throw new JsonException($"Missing or invalid 'CommonInformation' at '{path}'.");
        }

        return GetIdentityComponent(item, "Name", path);
    }

    private static string GetIdentity(JsonElement jsonElement, CollectionType type, string path)
    {
        var item = jsonElement;

        if (type == CollectionType.NetworkInterfaces)
        {
            if (!jsonElement.TryGetProperty("CommonInformation", out var value) || value.ValueKind != JsonValueKind.Object)
                throw new JsonException($"Missing or invalid 'CommonInformation' at '{path}'.");

            item = value;
        }

        return GetIdentityComponent(item, "Name", path);
    }

    private static string GetIdentityComponent(JsonObject item, string name, string path)
    {
        if (item[name] is not JsonValue value ||
            !value.TryGetValue<string>(out var result) ||
            string.IsNullOrWhiteSpace(result))
        {
            throw new JsonException($"Missing or invalid '{name}' at '{path}'.");
        }

        return result;
    }

    private static string GetIdentityComponent(JsonElement item, string name, string path)
    {
        if (!item.TryGetProperty(name, out var value) ||
            value.ValueKind != JsonValueKind.String ||
            string.IsNullOrWhiteSpace(value.GetString()))
        {
            throw new JsonException($"Missing or invalid '{name}' at '{path}'.");
        }

        return value.GetString()!;
    }

    private static JsonObject CreateItemTemplate(CollectionType type)
    {
        return type switch
        {
            CollectionType.NetworkInterfaces => SerializeTemplate(
                new NetworkInterfaceDetail
                {
                    CommonInformation = new() { Name = string.Empty, Enabled = false },
                    IPv4 = new()
                },
                SourceGenerationContext.Default.NetworkInterfaceDetail),

            CollectionType.Services => SerializeTemplate(
                new ServiceDetail { Name = string.Empty, State = default },
                SourceGenerationContext.Default.ServiceDetail),

            CollectionType.IPv4Details => SerializeTemplate(
                new IPv4Detail { IPAddress = IPAddress.Any, Netmask = IPAddress.Any },
                SourceGenerationContext.Default.IPv4Detail),

            CollectionType.StaticHosts => SerializeTemplate(
                new StaticHostDetail { IPAddress = IPAddress.Any, Hostname = string.Empty },
                SourceGenerationContext.Default.StaticHostDetail),

            _ => throw new InvalidOperationException("The collection is not keyed.")
        };
    }

    private static JsonObject SerializeTemplate<T>(T value, JsonTypeInfo<T> typeInfo)
    {
        return JsonSerializer.SerializeToNode(value, typeInfo) as JsonObject
            ?? throw new InvalidOperationException("The collection item template could not be serialized.");
    }

    private enum CollectionType
    {
        Values,
        NetworkInterfaces,
        Services,
        IPv4Details,
        StaticHosts
    }
}

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using HostManagement.Shared.Capabilities;
using HostManagement.Shared.Communication.Capabilities;
using HostManagement.Shared.Communication.Contracts;

namespace HostManagement.Services;

/// <summary>
/// Converts <see cref="SupportedCapabilities"/> between its INI representation and JSON.
/// </summary>
/// <remarks>
/// This converter is specific to <see cref="SupportedCapabilities"/> and supports only sections such as
/// <c>[Settings.DNS]</c> or <c>[Topics]</c>, <c>Key=Value</c> entries, blank lines and full-line comments
/// (<c>#</c> or <c>;</c>). Inline comments, quoted values, escapes and multi-line values are not supported.
/// <para>
/// Settings become nested capability objects; topics become direct capability values. The <c>Capability</c>
/// property is reserved and section capabilities are derived from configured child settings.
/// Template output includes default values as commented entries while section headers remain active.
/// </para>
/// </remarks>
internal static class SupportedCapabilitiesIniConverter
{
    private const char TemplateEntryCommentPrefix = '#';

    private const string NamePropertyName = "Name";
    private const string CapabilityPropertyName = "Capability";
    private const string DisabledCapabilityValue = nameof(CapabilityStatus.Disabled);
    private const string EnabledCapabilityValue = nameof(CapabilityStatus.Enabled);
    private const string SettingsSectionName = nameof(SupportedCapabilities.Settings);
    private const string TopicsSectionName = nameof(SupportedCapabilities.Topics);

    /// <summary>
    /// Converts a 'SupportedCapabilities' INI document to a JSON document
    /// that can be applied to <see cref="SupportedCapabilities"/>.
    /// </summary>
    /// <param name="iniContent">The INI content to convert.</param>
    /// <returns>
    /// A <see cref="JsonDocument"/> containing the equivalent 'SupportedCapabilities' JSON structure.
    /// </returns>
    /// <exception cref="FormatException">
    /// Thrown when the INI content contains an unknown or invalid section, an unknown setting,
    /// an invalid key-value entry, an entry outside a section, a duplicate entry,
    /// an invalid capability value or conflicting scalar and object values.
    /// </exception>
    public static JsonDocument ToJsonDocument(string iniContent)
        => ToJsonDocument(iniContent, CreateDefaultSupportedCapabilitiesJson());

    /// <summary>
    /// Converts a 'SupportedCapabilities' JSON document to the HostManagement INI template.
    /// </summary>
    /// <param name="document">The 'SupportedCapabilities' JSON document to convert.</param>
    /// <returns>The equivalent 'SupportedCapabilities' INI template content.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <paramref name="document"/> is not a JSON object, when a known section is not a JSON object
    /// or when the JSON contains an unsupported top-level section.
    /// </exception>
    public static string ToTemplateIni(JsonDocument document)
    {
        if (document.RootElement.ValueKind != JsonValueKind.Object)
            throw new InvalidOperationException("The 'SupportedCapabilities' JSON root element must be an object.");

        var builder = new StringBuilder();

        AppendTemplateHeader(builder);

        foreach (var section in document.RootElement.EnumerateObject())
        {
            if (section.NameEquals(SettingsSectionName))
                WriteSettingsToIniContext(builder, section.Name, section.Value);
            else if (section.NameEquals(TopicsSectionName))
                WriteTopicsToIniContext(builder, section.Name, section.Value);
            else
                throw new InvalidOperationException($"Invalid 'SupportedCapabilities' JSON section '{section.Name}'.");
        }

        return builder.ToString();
    }

    /// <summary>
    /// Converts a 'SupportedCapabilities' INI document to a JSON document using the provided default capabilities schema.
    /// </summary>
    /// <remarks>
    /// Allows callers to provide an explicit default capabilities schema.
    /// </remarks>
    /// <param name="iniContent">The INI content to convert.</param>
    /// <param name="defaultCapabilitiesJson">The default capabilities schema used to validate settings.</param>
    /// <returns>A <see cref="JsonDocument"/> containing the equivalent JSON structure.</returns>
    internal static JsonDocument ToJsonDocument(string iniContent, JsonObject defaultCapabilitiesJson)
    {
        var parsedRoot = ParseIniDocument(iniContent);
        var supportedCapabilitiesJsonObject = BuildSupportedCapabilitiesJsonObject(
            parsedRoot,
            defaultCapabilitiesJson);

        return CreateJsonDocument(supportedCapabilitiesJsonObject);
    }

    // INI -> JSON
    private static JsonObject ParseIniDocument(string iniContent)
    {
        using var reader = new StringReader(iniContent);

        var parsedRoot = new JsonObject();
        var lineNumber = 0;

        IniSectionContext? currentSectionContext = null;

        while (reader.ReadLine() is string line)
        {
            lineNumber++;

            var trimmedLine = line.Trim();

            if (trimmedLine.Length == 0 || trimmedLine.StartsWith('#') || trimmedLine.StartsWith(';'))
                continue;

            if (TryReadIniSection(trimmedLine, lineNumber, out var sectionName))
            {
                currentSectionContext = CreateSectionContext(parsedRoot, sectionName, lineNumber);
                continue;
            }

            if (currentSectionContext is not { } sectionContext)
                throw new FormatException($"INI entry at line {lineNumber} is not inside a section.");

            var (key, value) = ReadIniEntry(trimmedLine, lineNumber);

            ApplyIniEntry(sectionContext, key, value, lineNumber);
        }

        return parsedRoot;
    }

    private static IniSectionContext CreateSectionContext(JsonObject parsedRoot, string sectionName, int lineNumber)
    {
        if (sectionName.Length == 0)
            throw new FormatException($"Invalid empty INI section at line {lineNumber}.");

        var isTopicsSection = string.Equals(sectionName, TopicsSectionName, StringComparison.Ordinal);
        var section = GetOrCreateSectionJsonObject(parsedRoot, sectionName, lineNumber);

        return new IniSectionContext(isTopicsSection, section);
    }

    private static void ApplyIniEntry(IniSectionContext sectionContext, string key, string value, int lineNumber)
    {
        if (sectionContext.IsTopicsSection)
            ApplyTopicEntry(sectionContext.JsonObject, key, value, lineNumber);
        else
            ApplySettingEntry(sectionContext.JsonObject, key, value, lineNumber);
    }

    private static void ApplyTopicEntry(JsonObject topicsSection, string key, string value, int lineNumber)
    {
        var parsedCapabilityValue = ParseCapabilityValue(value, lineNumber);
        SetUniqueIniPropertyValue(topicsSection, key, parsedCapabilityValue, lineNumber);
    }

    private static void ApplySettingEntry(JsonObject settingsSection, string key, string value, int lineNumber)
    {
        var segments = key.Split('.', StringSplitOptions.TrimEntries);
        var currentSettingSection = settingsSection;

        for (var i = 0; i < segments.Length - 1; i++)
        {
            if (segments[i].Length == 0)
                throw new FormatException($"Invalid INI key '{key}' at line {lineNumber}.");

            currentSettingSection = GetOrCreateJsonObject(currentSettingSection, segments[i], lineNumber);
        }

        var settingName = segments[^1];

        if (settingName.Length == 0)
            throw new FormatException($"Invalid INI key '{key}' at line {lineNumber}.");

        if (string.Equals(settingName, CapabilityPropertyName, StringComparison.Ordinal))
        {
            throw new FormatException(
                $"Unsupported INI property '{CapabilityPropertyName}' at line {lineNumber}. " +
                "Configure child settings explicitly instead.");
        }

        var setting = GetOrCreateJsonObject(currentSettingSection, settingName, lineNumber);
        SetUniqueIniPropertyValue(setting, CapabilityPropertyName, ParseCapabilityValue(value, lineNumber), lineNumber);
    }

    private static JsonObject BuildSupportedCapabilitiesJsonObject(
        JsonObject parsedRoot,
        JsonObject defaultCapabilitiesJson)
    {
        var supportedCapabilitiesJson = new JsonObject();

        if (parsedRoot.TryGetPropertyValue(SettingsSectionName, out var settingsNode) &&
            settingsNode is JsonObject configuredSettings)
        {
            if (defaultCapabilitiesJson[SettingsSectionName] is not JsonObject defaultSettings)
            {
                throw new InvalidOperationException(
                    "The default 'SupportedCapabilities' JSON must contain a 'Settings' object.");
            }

            supportedCapabilitiesJson[SettingsSectionName] = BuildSettingsJson(configuredSettings, defaultSettings);
        }

        if (parsedRoot.TryGetPropertyValue(TopicsSectionName, out var topicsNode) &&
            topicsNode is JsonObject configuredTopics)
        {
            if (defaultCapabilitiesJson[TopicsSectionName] is not JsonObject defaultTopics)
            {
                throw new InvalidOperationException(
                    "The default 'SupportedCapabilities' JSON must contain a 'Topics' object.");
            }

            supportedCapabilitiesJson[TopicsSectionName] = BuildTopicsJson(configuredTopics, defaultTopics);
        }

        return supportedCapabilitiesJson;
    }

    private static JsonObject CreateDefaultSupportedCapabilitiesJson()
    {
        var supportedCapabilitiesJson = JsonSerializer.Serialize(
            new SupportedCapabilities(),
            CapabilitySourceGenerationContext.Default.SupportedCapabilities);

        return JsonNode.Parse(supportedCapabilitiesJson)?.AsObject()
            ?? throw new InvalidOperationException("The default 'SupportedCapabilities' JSON must be an object.");
    }

    private static JsonObject BuildSettingsJson(JsonObject configuredSettings, JsonObject defaultSettings)
    {
        var settingsJson = new JsonObject();

        foreach (var configuredSetting in configuredSettings)
        {
            if (configuredSetting.Value is not JsonObject configuredSettingObject)
                throw new FormatException($"Configured setting '{configuredSetting.Key}' must be an object.");

            if (defaultSettings[configuredSetting.Key] is not JsonObject defaultSetting)
                throw new FormatException($"Unknown supported setting '{configuredSetting.Key}'.");

            settingsJson[configuredSetting.Key] = BuildSettingJson(
                configuredSetting.Key,
                configuredSettingObject,
                defaultSetting);
        }

        return settingsJson;
    }

    private static JsonObject BuildTopicsJson(JsonObject configuredTopics, JsonObject defaultTopics)
    {
        var topicsJson = new JsonObject();

        foreach (var configuredTopic in configuredTopics)
        {
            if (!defaultTopics.ContainsKey(configuredTopic.Key))
                throw new FormatException($"Unknown supported topic '{configuredTopic.Key}'.");

            topicsJson[configuredTopic.Key] = configuredTopic.Value?.DeepClone();
        }

        return topicsJson;
    }

    private static JsonObject BuildSettingJson(
        string settingPath,
        JsonObject configuredSetting,
        JsonObject defaultSetting)
    {
        if (!HasDefaultChildSettings(defaultSetting))
        {
            EnsureLeafSettingHasNoChildSettings(settingPath, configuredSetting);

            var capability = TryGetConfiguredCapability(configuredSetting, out var configuredCapability)
                ? configuredCapability
                : DisabledCapabilityValue;

            return new JsonObject { [CapabilityPropertyName] = capability };
        }

        if (TryGetConfiguredCapability(configuredSetting, out _))
        {
            throw new FormatException(
                $"Setting '{settingPath}' has child settings and cannot be configured directly. " +
                $"Configure child settings in '[{SettingsSectionName}.{settingPath}]'.");
        }

        var settingJson = new JsonObject();
        var hasEnabledChild = false;

        foreach (var configuredChild in configuredSetting)
        {
            if (IsCapabilityProperty(configuredChild.Key))
                continue;

            var childSettingPath = $"{settingPath}.{configuredChild.Key}";

            if (configuredChild.Value is not JsonObject configuredChildSetting)
                throw new FormatException($"Configured setting '{childSettingPath}' must be an object.");

            if (defaultSetting[configuredChild.Key] is not JsonObject defaultChildSetting)
                throw new FormatException($"Unknown setting '{childSettingPath}'.");

            var childSettingJson = BuildSettingJson(childSettingPath, configuredChildSetting, defaultChildSetting);

            if (HasEnabledCapability(childSettingJson))
                hasEnabledChild = true;

            settingJson[configuredChild.Key] = childSettingJson;
        }

        settingJson[CapabilityPropertyName] = hasEnabledChild ? EnabledCapabilityValue : DisabledCapabilityValue;

        return settingJson;
    }

    private static bool HasDefaultChildSettings(JsonObject defaultSetting)
        => defaultSetting.Any(property => !IsSettingMetadataProperty(property.Key));

    private static void EnsureLeafSettingHasNoChildSettings(string settingPath, JsonObject configuredSetting)
    {
        if (configuredSetting.Any(property => !IsSettingMetadataProperty(property.Key)))
            throw new FormatException($"The setting '{settingPath}' does not support a child setting.");
    }

    private static bool IsSettingMetadataProperty(string propertyName)
        => string.Equals(propertyName, NamePropertyName, StringComparison.Ordinal)
        || string.Equals(propertyName, CapabilityPropertyName, StringComparison.Ordinal);

    private static bool TryGetConfiguredCapability(JsonObject setting, out string capability)
    {
        if (setting.TryGetPropertyValue(CapabilityPropertyName, out var capabilityNode) &&
            capabilityNode is JsonValue capabilityValue)
        {
            capability = capabilityValue.GetValue<string>();
            return true;
        }

        capability = string.Empty;
        return false;
    }

    private static bool IsCapabilityProperty(string propertyName)
        => string.Equals(propertyName, CapabilityPropertyName, StringComparison.Ordinal);

    private static bool HasEnabledCapability(JsonObject settingObject)
        => settingObject.TryGetPropertyValue(CapabilityPropertyName, out var capabilityNode)
        && capabilityNode is JsonValue capabilityValue
        && string.Equals(capabilityValue.GetValue<string>(), EnabledCapabilityValue, StringComparison.Ordinal);

    private static JsonDocument CreateJsonDocument(JsonObject jsonObject)
    {
        using var jsonStream = new MemoryStream();

        using (var writer = new Utf8JsonWriter(jsonStream))
        {
            jsonObject.WriteTo(writer);
        }

        jsonStream.Position = 0;

        return JsonDocument.Parse(jsonStream);
    }

    // JSON -> INI
    private static void AppendTemplateHeader(StringBuilder builder)
    {
        builder
            .AppendLine("# This file is managed by ifm HostManagement application.")
            .AppendLine("#")
            .AppendLine("# It describes the supported capabilities exposed by HostManagement.")
            .AppendLine("# Sections are written as [Settings.<path>] or [Topics]. Setting entries")
            .AppendLine("# configure capabilities below the current settings section. Topic entries")
            .AppendLine("# configure topic capabilities directly.")
            .AppendLine("#")
            .AppendLine("# Entries in this file show the compile time defaults. Values can be")
            .AppendLine("# configured by modifying this file.")
            .AppendLine("# Valid values are 'Enabled'/'Disabled' ignoring case.")
            .AppendLine();
    }

    private static void WriteSettingsToIniContext(StringBuilder builder, string sectionPath, JsonElement settingElement)
    {
        EnsureJsonObject(settingElement, sectionPath);

        var sectionBuilder = new StringBuilder();

        foreach (var property in settingElement.EnumerateObject())
        {
            if (property.NameEquals(NamePropertyName))
                continue;

            if (property.NameEquals(CapabilityPropertyName))
                continue;

            if (HasNestedSettingProperties(property.Value))
                continue;

            if (TryGetCapabilityProperty(property.Value, out var capability))
            {
                AppendIniEntry(sectionBuilder, property.Name, ReadCapabilityValue(capability));
                continue;
            }

            AppendIniEntry(sectionBuilder, property.Name, ReadCapabilityValue(property.Value));
        }

        AppendSection(builder, sectionPath, sectionBuilder);

        // Write nested sections after direct entries to preserve INI item order.
        foreach (var property in settingElement.EnumerateObject())
        {
            if (property.NameEquals(CapabilityPropertyName))
                continue;

            if (HasNestedSettingProperties(property.Value))
                WriteSettingsToIniContext(builder, $"{sectionPath}.{property.Name}", property.Value);
        }
    }

    private static void WriteTopicsToIniContext(StringBuilder builder, string sectionName, JsonElement sectionElement)
    {
        EnsureJsonObject(sectionElement, sectionName);

        var sectionBuilder = new StringBuilder();

        foreach (var property in sectionElement.EnumerateObject())
            AppendIniEntry(sectionBuilder, property.Name, ReadCapabilityValue(property.Value));

        AppendSection(builder, sectionName, sectionBuilder);
    }

    private static void EnsureJsonObject(JsonElement element, string sectionPath)
    {
        if (element.ValueKind != JsonValueKind.Object)
            throw new InvalidOperationException($"The '{sectionPath}' section must be a JSON object.");
    }

    private static bool HasNestedSettingProperties(JsonElement settingElement)
    {
        return settingElement.ValueKind == JsonValueKind.Object
            && settingElement.EnumerateObject().Any(property
                => !property.NameEquals(NamePropertyName)
                && !property.NameEquals(CapabilityPropertyName));
    }

    private static bool TryGetCapabilityProperty(JsonElement settingElement, out JsonElement capability)
    {
        if (settingElement.ValueKind == JsonValueKind.Object &&
            settingElement.TryGetProperty(CapabilityPropertyName, out capability))
        {
            return true;
        }

        capability = default;
        return false;
    }

    private static string ReadCapabilityValue(JsonElement capabilityElement)
    {
        if (capabilityElement.ValueKind == JsonValueKind.String &&
            capabilityElement.GetString() is { } capability &&
            (string.Equals(capability, EnabledCapabilityValue, StringComparison.Ordinal) ||
             string.Equals(capability, DisabledCapabilityValue, StringComparison.Ordinal)))
        {
            return capability;
        }

        throw new InvalidOperationException(
            $"Capability values must be '{DisabledCapabilityValue}' or '{EnabledCapabilityValue}'.");
    }

    private static void AppendIniEntry(StringBuilder builder, string key, string value)
    {
        builder.Append(TemplateEntryCommentPrefix);
        builder.AppendLine($"{key}={value}");
    }

    private static void AppendSection(StringBuilder builder, string sectionName, StringBuilder sectionBuilder)
    {
        if (sectionBuilder.Length == 0)
            return;

        builder.AppendLine($"[{sectionName}]");
        builder.Append(sectionBuilder);
        builder.AppendLine();
    }

    // INI parsing helpers
    private static bool TryReadIniSection(string trimmedLine, int lineNumber, out string sectionName)
    {
        var hasSectionStart = trimmedLine.StartsWith('[');
        var hasSectionEnd = trimmedLine.EndsWith(']');

        if (hasSectionStart && hasSectionEnd)
        {
            sectionName = trimmedLine[1..^1].Trim();
            return true;
        }

        if (hasSectionStart || hasSectionEnd)
            throw new FormatException($"Invalid INI section at line {lineNumber}.");

        sectionName = string.Empty;
        return false;
    }

    private static (string key, string value) ReadIniEntry(string trimmedLine, int lineNumber)
    {
        var separatorIndex = trimmedLine.IndexOf('=');

        if (separatorIndex <= 0)
            throw new FormatException($"Invalid INI entry at line {lineNumber}.");

        var key = trimmedLine[..separatorIndex].Trim();
        var value = trimmedLine[(separatorIndex + 1)..].Trim();

        if (key.Length == 0)
            throw new FormatException($"Invalid empty INI key at line {lineNumber}.");

        return (key, value);
    }

    private static string ParseCapabilityValue(string value, int lineNumber)
    {
        if (string.Equals(value, DisabledCapabilityValue, StringComparison.OrdinalIgnoreCase))
            return DisabledCapabilityValue;

        if (string.Equals(value, EnabledCapabilityValue, StringComparison.OrdinalIgnoreCase))
            return EnabledCapabilityValue;

        throw new FormatException($"Invalid capability value '{value}' at line {lineNumber}.");
    }

    private static JsonObject GetOrCreateSectionJsonObject(JsonObject parsedRoot, string sectionName, int lineNumber)
    {
        var section = parsedRoot; // Starting point for resolving the section path

        foreach (var sectionSegment in GetValidatedSectionSegments(sectionName, lineNumber))
            section = GetOrCreateJsonObject(section, sectionSegment, lineNumber);

        return section;
    }

    private static string[] GetValidatedSectionSegments(string sectionName, int lineNumber)
    {
        var segments = sectionName.Split('.', StringSplitOptions.TrimEntries);

        if (segments.Any(static segment => segment.Length == 0))
            throw new FormatException(GetInvalidSectionMessage());

        var topLevelSectionName = segments[0];

        if (string.Equals(topLevelSectionName, SettingsSectionName, StringComparison.Ordinal))
            return segments;

        if (string.Equals(topLevelSectionName, TopicsSectionName, StringComparison.Ordinal))
        {
            if (segments.Length != 1)
                throw new FormatException(GetInvalidSectionMessage());

            return segments;
        }

        throw new FormatException($"Unknown supported capabilities INI section '{sectionName}' at line {lineNumber}.");

        string GetInvalidSectionMessage() => $"Invalid INI section '{sectionName}' at line {lineNumber}.";
    }

    private static JsonObject GetOrCreateJsonObject(JsonObject parent, string propertyName, int lineNumber)
    {
        if (parent.TryGetPropertyValue(propertyName, out var existingNode))
        {
            if (existingNode is JsonObject existingObject)
                return existingObject;

            throw new FormatException(
                $"INI property '{propertyName}' at line {lineNumber} conflicts with an existing scalar value.");
        }

        var createdObject = new JsonObject();
        parent[propertyName] = createdObject;

        return createdObject;
    }

    private static void SetUniqueIniPropertyValue(JsonObject parent, string propertyName, string value, int lineNumber)
    {
        if (parent.ContainsKey(propertyName))
            throw new FormatException($"Duplicate INI property '{propertyName}' at line {lineNumber}.");

        parent[propertyName] = value;
    }

    // Nested types
    private readonly record struct IniSectionContext(bool IsTopicsSection, JsonObject JsonObject);
}

using Microsoft.Extensions.Configuration;

namespace HostManagement.Configuration;

/// <inheritdoc cref="IEnvironmentVariable{T}"/>
public record EnvironmentVariable<T> : IEnvironmentVariable<T> where T : notnull
{
    private static readonly Func<string, string> s_invalidVariableKeyFormatter = key => key.Replace(":", "__");

    /// <summary>
    /// Initializes a new <see cref="EnvironmentVariable{T}"/> with <paramref name="key"/> and
    /// <paramref name="defaultValue"/>.
    /// </summary>
    /// <param name="key">The key of the environment variable.</param>
    /// <param name="defaultValue">The default value of the environment variable.</param>
    /// <param name="isRequired">
    /// Determines whether the environment variable is required and should have a valid value.
    /// </param>
    /// <param name="isIndexed">
    /// Determines whether the environment variable is an indexed variable, e.g. 'KEY:0', 'KEY:1' etc.
    /// </param>
    public EnvironmentVariable(string key, T defaultValue, bool isRequired = false, bool isIndexed = false)
    {
        Key = key;
        Value = GetValue(key, defaultValue, isRequired, isIndexed);
    }

    /// <inheritdoc/>
    public bool IsInvalid { get; private set; }

    /// <inheritdoc/>
    public string InvalidVariableString { get; private set; } = string.Empty;

    /// <inheritdoc/>
    public string Key { get; }

    /// <inheritdoc/>
    public T Value { get; init; }

    private T GetValue(string key, T defaultValue, bool isRequired, bool isIndexed)
    {
        object result;

        if (typeof(T) == typeof(string))
        {
            result = GetStringValue(key, defaultValue, isRequired);
        }
        else if (typeof(T) == typeof(IReadOnlyCollection<string>))
        {
            if (isIndexed)
                result = GetIndexedStringListValue(key, defaultValue, isRequired);
            else
                result = GetStringListValue(key, defaultValue, isRequired);
        }
        else if (typeof(T).IsEnum)
        {
            if (typeof(T) == typeof(TargetEnvironment))
                result = GetTargetEnvironmentEnumValue(key, defaultValue, isRequired);
            else
                result = GetEnumValue(key, defaultValue, isRequired);
        }
        else
        {
            throw new NotSupportedException($"Unsupported environment variable type: {typeof(T)}");
        }

        return (T)result;
    }

    private string GetStringValue(string key, T defaultValue, bool isRequired)
    {
        var defaultStringValue = defaultValue is string s ? s : string.Empty;
        var value = AppConfiguration.Configuration[key] ?? defaultStringValue;

        IsInvalid = isRequired && string.IsNullOrWhiteSpace(value);

        SetInvalidVariableStringIfInvalid(key);

        return value;
    }

    private IReadOnlyCollection<string> GetIndexedStringListValue(string key, T defaultValue, bool isRequired)
    {
        // Indexed environment variables, e.g.:
        // HOSTMANAGEMENT:PUBLIC_KEYS:0
        // HOSTMANAGEMENT:PUBLIC_KEYS:1

        var defaultCollectionValue = defaultValue is IReadOnlyCollection<string> c ? c : [];
        var keyAndColon = key + ":";

        var indexedValues = AppConfiguration.Configuration
            .AsEnumerable()
            .Where(kvp => kvp.Key.StartsWith(keyAndColon, StringComparison.Ordinal))
            .Select(kvp =>
            {
                var hasValidIndex = int.TryParse(kvp.Key[keyAndColon.Length..], out var index) && index >= 0;

                return new { HasValidIndex = hasValidIndex, Index = index, kvp.Key, kvp.Value };
            })
            .ToList();

        var values = indexedValues.Count == 0
            ? defaultCollectionValue
            : [.. indexedValues.Where(x => x.HasValidIndex).OrderBy(x => x.Index).Select(x => x.Value ?? string.Empty)];

        var invalidIndexedKeys = indexedValues
            .Where(x => !x.HasValidIndex || string.IsNullOrWhiteSpace(x.Value))
            .Select(x => x.Key)
            .ToList();

        IsInvalid = isRequired && (invalidIndexedKeys.Count > 0 || values.Count == 0);

        var invalidVariableKeys = GetInvalidVariableKeys();
        SetInvalidVariableStringIfInvalid(invalidVariableKeys);

        return values;

        string[] GetInvalidVariableKeys()
        {
            if (invalidIndexedKeys.Count > 0)
                return [.. invalidIndexedKeys];

            return IsInvalid ? [key] : [];
        }
    }

    private IReadOnlyCollection<string> GetStringListValue(string key, T defaultValue, bool isRequired)
    {
        // Single environment variable, containing comma separated list, e.g.:
        // HOSTMANAGEMENT:CONFIGURABLE_SERVICES='ssh.service,remoteconnect.service'

        var defaultCollectionValue = defaultValue is IReadOnlyCollection<string> c ? c : [];
        var stringValue = AppConfiguration.Configuration[key];

        var values = stringValue is null
            ? defaultCollectionValue
            // Do not treat empty segments as invalid, just remove them (e.g.: "a,,b" -> ["a","b"])
            : [.. stringValue.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)];

        IsInvalid = isRequired && values.Count == 0;

        SetInvalidVariableStringIfInvalid(key);

        return values;
    }

    private T GetEnumValue(string key, T defaultValue, bool isRequired)
    {
        var couldBeParsed = Enum.TryParse(typeof(T), AppConfiguration.Configuration[key], true, out var result);

        if (couldBeParsed)
            return (T)result!;

        IsInvalid = isRequired && !couldBeParsed;

        SetInvalidVariableStringIfInvalid(key);

        return defaultValue;
    }

    private TargetEnvironment GetTargetEnvironmentEnumValue(string key, T defaultValue, bool isRequired)
    {
        _ = defaultValue; // Discard 'defaultValue' because 'FromEnvironmentString' handles all values
        var value = TargetEnvironmentExtensions.FromEnvironmentString(AppConfiguration.Configuration[key]);

        IsInvalid = isRequired && value is TargetEnvironment.Unknown;

        SetInvalidVariableStringIfInvalid(key);

        return value;
    }

    private void SetInvalidVariableStringIfInvalid(params string[] keys)
    {
        if (!IsInvalid)
            return;

        if (keys.Length == 0)
            return;

        if (keys.Length == 1)
        {
            InvalidVariableString = s_invalidVariableKeyFormatter(keys[0]);
            return;
        }

        InvalidVariableString = string.Join(
            Environment.NewLine,
            keys.Select(s_invalidVariableKeyFormatter));
    }
}

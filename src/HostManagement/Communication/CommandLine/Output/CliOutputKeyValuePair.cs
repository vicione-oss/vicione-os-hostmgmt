using System.ComponentModel;

namespace HostManagement.Communication.CommandLine.Output;

/// <summary>
/// Initializes a new <see cref="CliOutputKeyValuePair"/>.
/// </summary>
/// <remarks>
/// If the <paramref name="key"/> is empty the value of the key will be set to
/// <see cref="CliOutputBase.EmptyKey"/>.<br/>
/// If the <paramref name="value"/> is empty it will be set to
/// <see cref="CliOutputBase.EmptyValue"/>.
/// </remarks>
/// <param name="key">The key.</param>
/// <param name="value">The value.</param>
internal sealed class CliOutputKeyValuePair(CliOutputValue key, CliOutputValue value)
{
    /// <summary>
    /// The key.
    /// </summary>
    internal CliOutputValue Key { get; } = string.IsNullOrWhiteSpace(key)
        ? CliOutputBase.EmptyKey
        : key;

    /// <summary>
    /// The value.
    /// </summary>
    internal CliOutputValue Value { get; } = string.IsNullOrWhiteSpace(value)
        ? CliOutputBase.EmptyValue
        : value;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void Deconstruct(out CliOutputValue key, out CliOutputValue value)
    {
        key = Key;
        value = Value;
    }
}

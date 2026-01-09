using System.Text;

namespace HostManagement.Communication.CommandLine.Output;

/// <summary>
/// Represents a <see cref="PropertyListOutput"/>.
/// </summary>
/// <remarks>
/// An optional title and/or enabled can be added to the output.
/// An optional enabled can also be added to each key-value formatted property.
/// <code>
/// Title (enabled):
/// <br/>  Item1    -> Key: Value (enabled)
/// <br/>  Item123  -> Key: Value (enabled)
/// </code>
/// <code>
/// Title (enabled): Item1  -> Key: Value (enabled)
/// </code>
/// </remarks>
internal sealed class PropertyListOutput : CliComplexOutputBase
{
    private readonly List<(string, CliOutputKeyValuePair, bool?)> _items = [];
    private readonly int[] _maxColumnLengths = new int[16];

    /// <summary>
    /// Initializes a new <see cref="PropertyListOutput"/>.
    /// </summary>
    /// <param name="indentationLevel">The indentation level of the output.</param>
    /// <param name="cliOutputs">A collection of CLI outputs.</param>
    /// <param name="stringBuilder">A <see cref="StringBuilder"/>.</param>
    internal PropertyListOutput(
        int indentationLevel = 0,
        ICollection<CliComplexOutputBase>? cliOutputs = null,
        StringBuilder? stringBuilder = null)
        : base(indentationLevel, cliOutputs, stringBuilder) { }

    protected internal override int ItemCount => _items.Count;

    /// <summary>
    /// Adds a value followed by a key-value formatted property to the output.
    /// </summary>
    /// <remarks>
    /// An optional enabled can also be added to the item.
    /// </remarks>
    /// <param name="value">The value.</param>
    /// <param name="property">The key-value formatted property belonging to the value.</param>
    /// <param name="enabled">The enabled.</param>
    /// <returns>This <see cref="PropertyListOutput"/></returns>
    internal PropertyListOutput With(
        string value,
        CliOutputKeyValuePair property,
        bool? enabled = null)
    {
        var nonEmptyValue = string.IsNullOrWhiteSpace(value)
            ? EmptyValue
            : value.ToString();

        if (_maxColumnLengths[0] < nonEmptyValue.Length)
            _maxColumnLengths[0] = nonEmptyValue.Length;

        if (_maxColumnLengths[1] < property.Key.Length)
            _maxColumnLengths[1] = property.Key.Length;

        if (_maxColumnLengths[2] < property.Value.Length)
            _maxColumnLengths[2] = property.Value.Length;

        _items.Add((nonEmptyValue, property, enabled));
        return this;
    }

    private protected override void Format()
    {
        var hasEmptyValue = _items.Any(row => string.IsNullOrWhiteSpace(row.Item1));
        var hasEmptyPropertyKey = _items.Any(row => string.IsNullOrWhiteSpace(row.Item2.Key));
        var hasEmptyPropertyValue = _items.Any(row => string.IsNullOrWhiteSpace(row.Item2.Value));

        foreach (var (value, (propertyKey, propertyValue), enabled) in _items)
        {
            if (enabled is not null)
            {
                AppendLine(
                    GetPaddedValue(value) +
                    PropertyListNameDelimiter +
                    GetPaddedPropertyKey(propertyKey) +
                    KeyValueDelimiter +
                    GetPaddedPropertyValue(propertyValue) +
                    GetEnabledText(enabled));
            }
            else
            {
                AppendLine(
                    GetPaddedValue(value) +
                    PropertyListNameDelimiter +
                    GetPaddedPropertyKey(propertyKey) +
                    KeyValueDelimiter +
                    propertyValue);
            }
        }
    }

    private string GetPaddedValue(string value)
        => value + GetWhiteSpace(_maxColumnLengths[0] - value.Length);

    private string GetPaddedPropertyKey(string key)
        => key + GetLeaderPattern(_maxColumnLengths[1] - key.Length);

    private string GetPaddedPropertyValue(string value)
        => value + GetWhiteSpace(_maxColumnLengths[2] - value.Length);
}

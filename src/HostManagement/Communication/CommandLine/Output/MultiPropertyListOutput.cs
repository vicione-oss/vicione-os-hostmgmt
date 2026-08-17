using System.Text;
using HostManagement.Communication.CommandLine.Resources;

namespace HostManagement.Communication.CommandLine.Output;

/// <summary>
/// Represents a <see cref="MultiPropertyListOutput"/>.
/// </summary>
/// <remarks>
/// An optional title and/or enabled can be added to the output.
/// <code>
/// Title (enabled):
/// <br/>  Item1    -> Key1: Value1  Key2: Value2  Key3: Value3
/// <br/>  Item123  -> Key1: Value1  Key2: Value2  Key3: Value3
/// </code>
/// <code>
/// Title (enabled): Item1  -> Key1: Value1  Key2: Value2  Key3: Value3
/// </code>
/// </remarks>
internal sealed class MultiPropertyListOutput : CliComplexOutputBase
{
    private readonly List<(string, CliOutputKeyValuePair[])> _items = [];
    private readonly int[] _maxColumnLengths = new int[16];

    /// <summary>
    /// Initializes a new <see cref="MultiPropertyListOutput"/>.
    /// </summary>
    /// <param name="indentationLevel">The indentation level of the output.</param>
    /// <param name="cliOutputs">A collection of CLI outputs.</param>
    /// <param name="stringBuilder">A <see cref="StringBuilder"/>.</param>
    internal MultiPropertyListOutput(
        int indentationLevel = 0,
        ICollection<CliComplexOutputBase>? cliOutputs = null,
        StringBuilder? stringBuilder = null)
        : base(indentationLevel, cliOutputs, stringBuilder) { }

    protected internal override int ItemCount => _items.Count;

    /// <summary>
    /// Adds a value followed by two key-value formatted properties to the output.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="property1">The first key-value formatted property belonging to the value.</param>
    /// <param name="property2">The second key-value formatted property belonging to the value.</param>
    /// <returns>This <see cref="MultiPropertyListOutput"/></returns>
    internal MultiPropertyListOutput With(
        string value,
        CliOutputKeyValuePair property1,
        CliOutputKeyValuePair property2)
    {
        var nonEmptyValue = string.IsNullOrWhiteSpace(value)
            ? EmptyValue
            : value.ToString();

        if (_maxColumnLengths[0] < nonEmptyValue.Length)
            _maxColumnLengths[0] = nonEmptyValue.Length;

        if (_maxColumnLengths[1] < property1.Key.Length)
            _maxColumnLengths[1] = property1.Key.Length;

        if (_maxColumnLengths[2] < property1.Value.Length)
            _maxColumnLengths[2] = property1.Value.Length;

        if (_maxColumnLengths[3] < property2.Key.Length)
            _maxColumnLengths[3] = property2.Key.Length;

        if (_maxColumnLengths[4] < property2.Value.Length)
            _maxColumnLengths[4] = property2.Value.Length;

        _items.Add((nonEmptyValue, [property1, property2]));
        return this;
    }

    /// <summary>
    /// Adds a value followed by multiple key-value formatted properties to the output.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="property1">The first key-value formatted property belonging to the value.</param>
    /// <param name="property2">The second key-value formatted property belonging to the value.</param>
    /// <param name="additionalProperties">Up to 5 additional key-value formatted properties
    /// belonging to the value.</param>
    /// <returns>This <see cref="MultiPropertyListOutput"/></returns>
    internal MultiPropertyListOutput With(
        string value,
        CliOutputKeyValuePair property1,
        CliOutputKeyValuePair property2,
        params CliOutputKeyValuePair[] additionalProperties)
    {
        if (additionalProperties.Length > 5)
        {
            throw new ArgumentOutOfRangeException(
                nameof(additionalProperties),
                CliStrings.Output.MultiPropertyListTooManyValues);
        }

        var nonEmptyValue = string.IsNullOrWhiteSpace(value)
            ? EmptyValue
            : value.ToString();

        if (_maxColumnLengths[0] < nonEmptyValue.Length)
            _maxColumnLengths[0] = nonEmptyValue.Length;

        if (_maxColumnLengths[1] < property1.Key.Length)
            _maxColumnLengths[1] = property1.Key.Length;

        if (_maxColumnLengths[2] < property1.Value.Length)
            _maxColumnLengths[2] = property1.Value.Length;

        if (_maxColumnLengths[3] < property2.Key.Length)
            _maxColumnLengths[3] = property2.Key.Length;

        if (_maxColumnLengths[4] < property2.Value.Length)
            _maxColumnLengths[4] = property2.Value.Length;

        for (var i = 0; i < additionalProperties.Length; i++)
        {
            if (_maxColumnLengths[5 + (2 * i)] < additionalProperties[i].Key.Length)
                _maxColumnLengths[5 + (2 * i)] = additionalProperties[i].Key.Length;

            if (_maxColumnLengths[6 + (2 * i)] < additionalProperties[i].Value.Length)
                _maxColumnLengths[6 + (2 * i)] = additionalProperties[i].Value.Length;
        }

        _items.Add((nonEmptyValue, [property1, property2, .. additionalProperties]));
        return this;
    }

    private protected override void Format()
    {
        var hasEmptyValue = _items.Any(tuple => string.IsNullOrWhiteSpace(tuple.Item1));

        var hasEmptyPropertyKey = _items
            .Any(tuple => tuple.Item2
                .Any(property => string.IsNullOrWhiteSpace(property.Key)));

        var hasEmptyPropertyValue = _items
            .Any(tuple => tuple.Item2
                .Any(property => string.IsNullOrWhiteSpace(property.Value)));

        for (var rowIndex = 0; rowIndex < _items.Count; rowIndex++)
        {
            var value = _items[rowIndex].Item1;
            var properties = _items[rowIndex].Item2;

            var propertiesString = string.Empty;

            for (var columnIndex = 0; columnIndex < properties.Length; columnIndex++)
            {
                if (columnIndex < properties.Length - 1)
                {
                    propertiesString +=
                        GetPaddedPropertyKey((2 * columnIndex) + 1, properties[columnIndex].Key) +
                        KeyValueDelimiter +
                        GetPaddedPropertyValue((2 * columnIndex) + 2, properties[columnIndex].Value) +
                        MultiKeyValuePairDelimiter;
                }
                else
                {
                    propertiesString +=
                        GetPaddedPropertyKey((2 * columnIndex) + 1, properties[columnIndex].Key) +
                        KeyValueDelimiter +
                        properties[columnIndex].Value;
                }
            }

            AppendLine(
                GetPaddedValue(value) +
                PropertyListNameDelimiter +
                propertiesString);
        }
    }

    private string GetPaddedValue(string value)
        => value + GetWhiteSpace(_maxColumnLengths[0] - value.Length);

    private string GetPaddedPropertyKey(int index, string key)
        => key + GetLeaderPattern(_maxColumnLengths[index] - key.Length);

    private string GetPaddedPropertyValue(int index, string value)
        => value + GetWhiteSpace(_maxColumnLengths[index] - value.Length);
}

using System.Text;

namespace HostManagement.Communication.CommandLine.Output;

/// <summary>
/// Represents a <see cref="MultiKeyValueOutput"/>.
/// </summary>
/// <remarks>
/// An optional title and/or enabled can be added to the output.
/// <code>
/// Title (enabled):
/// <br/>  Key1.......: Value1         Key2.......: Value2         Key3.......: Value3
/// <br/>  AnotherKey1: AnotherValue1  AnotherKey2: AnotherValue2  AnotherKey3: AnotherValue3
/// </code>
/// <code>
/// Title (enabled): Key1: Value1  Key2: Value2  Key3: Value3
/// </code>
/// </remarks>
internal sealed class MultiKeyValueOutput : CliComplexOutputBase
{
    private readonly List<CliOutputKeyValuePair[]> _items = [];
    private readonly int[] _maxColumnLengths = new int[16];

    /// <summary>
    /// Initializes a new <see cref="MultiKeyValueOutput"/>.
    /// </summary>
    /// <param name="indentationLevel">The indentation level of the output.</param>
    /// <param name="cliOutputs">A collection of CLI outputs.</param>
    /// <param name="stringBuilder">A <see cref="StringBuilder"/>.</param>
    internal MultiKeyValueOutput(
        int indentationLevel = 0,
        ICollection<CliComplexOutputBase>? cliOutputs = null,
        StringBuilder? stringBuilder = null)
        : base(indentationLevel, cliOutputs, stringBuilder) { }

    protected internal override int ItemCount => _items.Count;

    /// <summary>
    /// Adds two key-value pairs to the output.
    /// </summary>
    /// <param name="keyValuePair1">The first key-value pair.</param>
    /// <param name="keyValuePair2">The second key-value pair.</param>
    /// <returns>This <see cref="MultiKeyValueOutput"/></returns>
    internal MultiKeyValueOutput With(
        CliOutputKeyValuePair keyValuePair1,
        CliOutputKeyValuePair keyValuePair2)
    {
        if (_maxColumnLengths[0] < keyValuePair1.Key.Length)
            _maxColumnLengths[0] = keyValuePair1.Key.Length;

        if (_maxColumnLengths[1] < keyValuePair1.Value.Length)
            _maxColumnLengths[1] = keyValuePair1.Value.Length;

        if (_maxColumnLengths[2] < keyValuePair2.Key.Length)
            _maxColumnLengths[2] = keyValuePair2.Key.Length;

        if (_maxColumnLengths[3] < keyValuePair2.Value.Length)
            _maxColumnLengths[3] = keyValuePair2.Value.Length;

        _items.Add([keyValuePair1, keyValuePair2]);
        return this;
    }

    /// <summary>
    /// Adds multiple key-value pairs to the output.
    /// </summary>
    /// <param name="keyValuePair1">The first key-value pair.</param>
    /// <param name="keyValuePair2">The second key-value pair.</param>
    /// <param name="additionalKeyValuePairs">Up to 6 additional key-value pairs.</param>
    /// <returns>This <see cref="MultiKeyValueOutput"/></returns>
    internal MultiKeyValueOutput With(
        CliOutputKeyValuePair keyValuePair1,
        CliOutputKeyValuePair keyValuePair2,
        params CliOutputKeyValuePair[] additionalKeyValuePairs)
    {
        if (additionalKeyValuePairs.Length > 6)
        {
            throw new ArgumentOutOfRangeException(
                nameof(additionalKeyValuePairs),
                "Only up to 6 additional key-value pairs are allowed.");
        }

        if (_maxColumnLengths[0] < keyValuePair1.Key.Length)
            _maxColumnLengths[0] = keyValuePair1.Key.Length;

        if (_maxColumnLengths[1] < keyValuePair1.Value.Length)
            _maxColumnLengths[1] = keyValuePair1.Value.Length;

        if (_maxColumnLengths[2] < keyValuePair2.Key.Length)
            _maxColumnLengths[2] = keyValuePair2.Key.Length;

        if (_maxColumnLengths[3] < keyValuePair2.Value.Length)
            _maxColumnLengths[3] = keyValuePair2.Value.Length;

        for (var i = 0; i < additionalKeyValuePairs.Length; i++)
        {
            if (_maxColumnLengths[4 + (2 * i)] < additionalKeyValuePairs[i].Key.Length)
                _maxColumnLengths[4 + (2 * i)] = additionalKeyValuePairs[i].Key.Length;

            if (_maxColumnLengths[5 + (2 * i)] < additionalKeyValuePairs[i].Value.Length)
                _maxColumnLengths[5 + (2 * i)] = additionalKeyValuePairs[i].Value.Length;
        }

        _items.Add([keyValuePair1, keyValuePair2, .. additionalKeyValuePairs]);
        return this;
    }

    private protected override void Format()
    {
        var hasEmptyKey = _items
            .Any(keyValuePairs => keyValuePairs
                .Any(keyValuePair => string.IsNullOrWhiteSpace(keyValuePair.Key)));

        var hasEmptyValue = _items
            .Any(keyValuePairs => keyValuePairs
                .Any(keyValuePair => string.IsNullOrWhiteSpace(keyValuePair.Value)));

        for (var rowIndex = 0; rowIndex < _items.Count; rowIndex++)
        {
            var keyValuePairs = _items[rowIndex];
            var keyValuePairsString = string.Empty;

            for (var columnIndex = 0; columnIndex < keyValuePairs.Length; columnIndex++)
            {
                if (columnIndex < keyValuePairs.Length - 1)
                {
                    keyValuePairsString +=
                        GetPaddedKey(2 * columnIndex, keyValuePairs[columnIndex].Key) +
                        KeyValueDelimiter +
                        GetPaddedValue((2 * columnIndex) + 1, keyValuePairs[columnIndex].Value) +
                        MultiKeyValuePairDelimiter;
                }
                else
                {
                    keyValuePairsString +=
                        GetPaddedKey(2 * columnIndex, keyValuePairs[columnIndex].Key) +
                        KeyValueDelimiter +
                        keyValuePairs[columnIndex].Value;
                }
            }

            AppendLine(keyValuePairsString);
        }
    }

    private string GetPaddedKey(int index, string key)
        => key + GetLeaderPattern(_maxColumnLengths[index] - key.Length);

    private string GetPaddedValue(int index, string value)
        => value + GetWhiteSpace(_maxColumnLengths[index] - value.Length);
}

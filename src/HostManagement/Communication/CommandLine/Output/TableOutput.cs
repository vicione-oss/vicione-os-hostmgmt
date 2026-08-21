using System.Text;
using HostManagement.Communication.CommandLine.Resources;

namespace HostManagement.Communication.CommandLine.Output;

/// <summary>
/// Represents a <see cref="TableOutput"/>.
/// </summary>
/// <remarks>
/// An optional title and/or enabled can be added to the output.
/// <code>
/// Title (enabled):
/// <br/>  CellA1   CellB1   CellC1
/// <br/>  CellA246 CellB246 CellC246
/// </code>
/// <code>
/// Title (enabled): CellA1 CellB1 CellC1
/// </code>
/// </remarks>
internal sealed class TableOutput : CliComplexOutputBase
{
    private readonly List<CliOutputTableValue[]> _items = [];
    private readonly int[] _maxColumnLengths = new int[16];

    /// <summary>
    /// Initializes a new <see cref="TableOutput"/>.
    /// </summary>
    /// <param name="indentationLevel">The indentation level of the output.</param>
    /// <param name="cliOutputs">A collection of CLI outputs.</param>
    /// <param name="stringBuilder">A <see cref="StringBuilder"/>.</param>
    internal TableOutput(
        int indentationLevel = 0,
        ICollection<CliComplexOutputBase>? cliOutputs = null,
        StringBuilder? stringBuilder = null)
        : base(indentationLevel, cliOutputs, stringBuilder) { }

    protected internal override int ItemCount => _items.Count;

    /// <summary>
    /// Adds two values to the table output.
    /// </summary>
    /// <param name="value1">The first value.</param>
    /// <param name="value2">The second value.</param>
    /// <returns>This <see cref="TableOutput"/></returns>
    internal TableOutput With(CliOutputTableValue value1, CliOutputTableValue value2)
    {
        if (_maxColumnLengths[0] < value1.Length)
            _maxColumnLengths[0] = value1.Length;

        if (_maxColumnLengths[1] < value2.Length)
            _maxColumnLengths[1] = value2.Length;

        _items.Add([value1, value2]);
        return this;
    }

    /// <summary>
    /// Adds multiple values to the table output.
    /// </summary>
    /// <param name="value1">The first value.</param>
    /// <param name="value2">The second value.</param>
    /// <param name="additionalValues">Up to 14 additional values.</param>
    /// <returns>This <see cref="TableOutput"/></returns>
    internal TableOutput With(
        CliOutputTableValue value1,
        CliOutputTableValue value2,
        params CliOutputTableValue[] additionalValues)
    {
        if (additionalValues.Length > 14)
        {
            throw new ArgumentOutOfRangeException(
                nameof(additionalValues),
                CliStrings.Output.TableOutputTooManyValues);
        }

        if (_maxColumnLengths[0] < value1.Length)
            _maxColumnLengths[0] = value1.Length;

        if (_maxColumnLengths[1] < value2.Length)
            _maxColumnLengths[1] = value2.Length;

        for (var i = 0; i < additionalValues.Length; i++)
        {
            if (_maxColumnLengths[i + 2] < additionalValues[i].Length)
                _maxColumnLengths[i + 2] = additionalValues[i].Length;
        }

        _items.Add([value1, value2, .. additionalValues]);
        return this;
    }

    private protected override void Format()
    {
        var maxColumnCount = _items
            .Select(value => value.Length)
            .DefaultIfEmpty(0)
            .Max();

        var hasEmptyValue = _items
            .Any(columns => columns
                .Any(value => string.IsNullOrWhiteSpace(value)));

        for (var rowIndex = 0; rowIndex < _items.Count; rowIndex++)
        {
            var columnsString = string.Empty;

            for (var columnIndex = 0; columnIndex < maxColumnCount; columnIndex++)
            {
                var value = columnIndex < _items[rowIndex].Length
                    ? _items[rowIndex][columnIndex].ToString()
                    : TableCellEmptyValue;

                var whiteSpaceCount = hasEmptyValue
                    ? int.Max(TableCellEmptyValue.Length, _maxColumnLengths[columnIndex]) - value.Length
                    : _maxColumnLengths[columnIndex] - value.Length;

                if (columnIndex < maxColumnCount - 1)
                    columnsString += value + GetWhiteSpace(whiteSpaceCount) + TableColumnDelimiter;
                else
                    columnsString += value;
            }

            AppendLine(columnsString);
        }
    }
}

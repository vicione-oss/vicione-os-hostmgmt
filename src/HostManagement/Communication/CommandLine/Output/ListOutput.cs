using System.Text;

namespace HostManagement.Communication.CommandLine.Output;

/// <summary>
/// Represents a <see cref="ListOutput"/>.
/// </summary>
/// <remarks>
/// An optional title and/or enabled can be added to the output.
/// An optional enabled can also be added to each item.
/// <code>
/// Title (enabled):
/// <br/>  Item1 (enabled)
/// <br/>  Item2 (enabled)
/// </code>
/// <code>
/// Title (enabled): Item1 (enabled)
/// </code>
/// </remarks>
internal sealed class ListOutput : CliComplexOutputBase
{
    private readonly List<(CliOutputValue, bool?)> _items = [];

    /// <summary>
    /// Initializes a new <see cref="ListOutput"/>.
    /// </summary>
    /// <param name="indentationLevel">The indentation level of the output.</param>
    /// <param name="cliOutputs">A collection of CLI outputs.</param>
    /// <param name="stringBuilder">A <see cref="StringBuilder"/>.</param>
    internal ListOutput(
        int indentationLevel = 0,
        ICollection<CliComplexOutputBase>? cliOutputs = null,
        StringBuilder? stringBuilder = null)
        : base(indentationLevel, cliOutputs, stringBuilder) { }

    protected internal override int ItemCount => _items.Count;

    /// <summary>
    /// Adds a value to the output.
    /// </summary>
    /// <remarks>
    /// An optional enabled can also be added to the item.
    /// </remarks>
    /// <param name="value">The value.</param>
    /// <param name="enabled">The enabled.</param>
    /// <returns>This <see cref="ListOutput"/></returns>
    internal ListOutput With(CliOutputValue value, bool? enabled = null)
    {
        var nonEmptyValue = string.IsNullOrWhiteSpace(value)
            ? EmptyValue
            : value.ToString();

        _items.Add((nonEmptyValue, enabled));
        return this;
    }

    /// <summary>
    /// Adds each value to the output.
    /// </summary>
    /// <param name="values">The values.</param>
    /// <returns>This <see cref="ListOutput"/></returns>
    internal ListOutput WithEach(IEnumerable<CliOutputValue> values)
    {
        foreach (var value in values)
            With(value);

        return this;
    }

    private protected override void Format()
    {
        foreach (var item in _items)
        {
            if (item.Item2 is not null)
                AppendLine(item.Item1 + GetEnabledText(item.Item2));
            else
                AppendLine(item.Item1);
        }
    }
}

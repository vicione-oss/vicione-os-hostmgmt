using System.Text;

namespace HostManagement.Communication.CommandLine.Output;

/// <summary>
/// Represents a <see cref="KeyValueOutput"/>.
/// </summary>
/// <remarks>
/// An optional title and/or enabled can be added to the output.
/// <code>
/// Title (enabled):
/// <br/>  Key1......: Value1
/// <br/>  AnotherKey: AnotherValue
/// </code>
/// <code>
/// Title (enabled): Key1: Value1
/// </code>
/// </remarks>
internal sealed class KeyValueOutput : CliComplexOutputBase
{
    private readonly List<CliOutputKeyValuePair> _items = [];

    /// <summary>
    /// Initializes a new <see cref="KeyValueOutput"/>.
    /// </summary>
    /// <param name="indentationLevel">The indentation level of the output.</param>
    /// <param name="cliOutputs">A collection of CLI outputs.</param>
    /// <param name="stringBuilder">A <see cref="StringBuilder"/>.</param>
    internal KeyValueOutput(
        int indentationLevel = 0,
        ICollection<CliComplexOutputBase>? cliOutputs = null,
        StringBuilder? stringBuilder = null)
        : base(indentationLevel, cliOutputs, stringBuilder) { }

    protected internal override int ItemCount => _items.Count;

    /// <summary>
    /// Adds a key-value pair to the output.
    /// </summary>
    /// <param name="keyValuePair">The key-value pair.</param>
    /// <returns>This <see cref="KeyValueOutput"/></returns>
    internal KeyValueOutput With(CliOutputKeyValuePair keyValuePair)
    {
        _items.Add(keyValuePair);
        return this;
    }

    private protected override void Format()
    {
        var maximumKeyLength = _items
            .Select(keyValuePair => keyValuePair.Key.Length)
            .DefaultIfEmpty(0)
            .Max();

        foreach (var (key, value) in _items)
        {
            AppendLine(
                key +
                GetLeaderPattern(maximumKeyLength - key.Length) +
                KeyValueDelimiter +
                value);
        }
    }
}

using System.Text;

namespace HostManagement.Communication.CommandLine.Helper;

/// <summary>
/// Represents a wrapper for the <see cref="StringBuilder"/> class with the possibility to choose
/// the indentation level.
/// </summary>
internal sealed class CommandLineStringBuilder
{
    private const string DefaultIndentation = "  ";

    private readonly string _indentationString;
    private readonly StringBuilder _stringBuilder = new();

    /// <summary>
    /// Initializes a new <see cref="CommandLineStringBuilder"/>.
    /// </summary>
    /// <param name="indentationString">The string used for indentation.</param>
    public CommandLineStringBuilder(string indentationString = DefaultIndentation)
    {
        _indentationString = indentationString;
    }

    /// <summary>
    /// The string representation of all appended values.
    /// </summary>
    internal string Output => _stringBuilder.ToString();

    public override string ToString() => Output;

    /// <summary>
    /// Appends the specified <paramref name="value"/> without indentation followed by the default
    /// line terminator.
    /// </summary>
    /// <param name="value">The value.</param>
    internal void AppendLine(string value = "") => AppendLine(0, value);

    /// <summary>
    /// Appends the specified <paramref name="value"/> using the given
    /// <paramref name="indentationLevel"/> followed by the default line terminator.
    /// </summary>
    /// <param name="indentationLevel">The non negative indentation level.</param>
    /// <param name="value">The value.</param>
    /// <exception cref="ArgumentOutOfRangeException"/>
    internal void AppendLine(int indentationLevel, string value = "")
        => _stringBuilder.AppendLine(GetIndentedOutput(indentationLevel, value));

    /// <summary>
    /// Appends the specified <paramref name="value"/> without indentation.
    /// </summary>
    /// <param name="value">The value.</param>
    internal void Append(string value) => Append(0, value);

    /// <summary>
    /// Appends the specified <paramref name="value"/> using the given <paramref name="indentationLevel"/>.
    /// </summary>
    /// <param name="indentationLevel">The non negative indentation level.</param>
    /// <param name="value">The value.</param>
    /// <exception cref="ArgumentOutOfRangeException"/>
    internal void Append(int indentationLevel, string value)
        => _stringBuilder.Append(GetIndentedOutput(indentationLevel, value));

    private string GetIndentedOutput(int indentationLevel, string output)
    {
        if (indentationLevel < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(indentationLevel),
                CommandLineStrings.IndentationLevelNegative);
        }

        return string.Concat(Enumerable.Repeat(_indentationString, indentationLevel)) + output;
    }
}

namespace HostManagement.Communication.CommandLine.Output;

internal abstract partial class CliOutputBase
{
    /// <summary>
    /// The whitespace character.
    /// </summary>
    internal const char WhiteSpace = ' ';

    /// <summary>
    /// The indentation pattern.
    /// </summary>
    internal const string IndentationPattern = "  ";

    /// <summary>
    /// The title delimiter.
    /// </summary>
    internal const string TitleDelimiter = ":";

    /// <summary>
    /// The delimiter for warning/error messages.
    /// </summary>
    internal const string LogMessageDelimiter = ":";

    #region Output specific configurations
    /// <summary>
    /// The empty key.
    /// </summary>
    internal const string EmptyKey = "'no key'";

    /// <summary>
    /// The empty value.
    /// </summary>
    internal const string EmptyValue = "'no value'";

    /// <summary>
    /// The key-value delimiter.
    /// </summary>
    internal const string KeyValueDelimiter = ": ";

    /// <summary>
    /// The leader pattern.
    /// <para>
    /// A leader in typography is a series of characters, usually lines of dots or dashes,
    /// that are used as a visual aid to connect items.
    /// </para>
    /// </summary>
    internal const string LeaderPattern = ".";

    /// <summary>
    /// The multi key-value pair delimiter.
    /// </summary>
    internal const string MultiKeyValuePairDelimiter = "  ";

    /// <summary>
    /// The not set value (null).
    /// </summary>
    internal const string NotSetValue = "'not set'";

    /// <summary>
    /// The property list name delimiter.
    /// </summary>
    internal const string PropertyListNameDelimiter = "  -> ";

    /// <summary>
    /// The table cell empty value.
    /// </summary>
    internal const string TableCellEmptyValue = "-";

    /// <summary>
    /// The table column delimiter.
    /// </summary>
    internal const string TableColumnDelimiter = "  ";
    #endregion
}

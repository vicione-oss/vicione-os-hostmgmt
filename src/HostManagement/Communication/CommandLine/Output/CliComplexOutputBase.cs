using System.Text;

namespace HostManagement.Communication.CommandLine.Output;

/// <summary>
/// Represents the abstract base class for a complex CLI output.
/// Use a derived type to create a complex CLI output.
/// </summary>
internal abstract class CliComplexOutputBase : CliOutputBase
{
    private readonly int _indentationLevel;
    private readonly ICollection<CliComplexOutputBase> _cliOutputs = [];
    private readonly StringBuilder _stringBuilder = new();

    private readonly List<string> _errorMessages = [];
    private readonly List<string> _informationMessages = [];
    private readonly List<string> _warningMessages = [];

    private string _title = string.Empty;
    private bool _addSingleItemToNewLine;

    /// <summary>
    /// Initializes a new <see cref="CliComplexOutputBase"/>.
    /// </summary>
    /// <param name="indentationLevel">The indentation level.</param>
    /// <param name="cliOutputs">A collection of CLI outputs.</param>
    /// <param name="stringBuilder">A <see cref="StringBuilder"/>.</param>
    public CliComplexOutputBase(
        int indentationLevel = 0,
        ICollection<CliComplexOutputBase>? cliOutputs = null,
        StringBuilder? stringBuilder = null)
    {
        if (indentationLevel < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(indentationLevel),
                CommandLineStrings.IndentationLevelNegative);
        }

        _indentationLevel = indentationLevel;
        _cliOutputs = cliOutputs ?? [this];
        _stringBuilder = stringBuilder ?? new();
    }

    /// <summary>
    /// Gets the item count.
    /// </summary>
    protected internal abstract int ItemCount { get; }

    /// <summary>
    /// Determines whether the output has error messages.
    /// </summary>
    private bool HasErrorMessages => _errorMessages.Count != 0;

    /// <summary>
    /// Determines whether the output has information messages.
    /// </summary>
    private bool HasInformationMessages => _informationMessages.Count != 0;

    /// <summary>
    /// Determines whether the output has a title.
    /// </summary>
    private bool HasTitle => !string.IsNullOrWhiteSpace(_title);

    /// <summary>
    /// Determines whether the output has warning messages.
    /// </summary>
    private bool HasWarningMessages => _warningMessages.Count != 0;

    /// <summary>
    /// Gets a formatted string of the output.
    /// </summary>
    internal string GetOutput()
    {
        try
        {
            foreach (var output in _cliOutputs)
            {
                AppendMessagesIfPresent(output);

                if (output.HasTitle)
                {
                    var indentedTitle = GetIndentedString(output._indentationLevel, output._title);

                    if (!output._addSingleItemToNewLine &&
                        output.ItemCount == 1)
                    {
                        _stringBuilder.Append(indentedTitle);
                    }
                    else
                    {
                        _stringBuilder.AppendLine(indentedTitle);
                    }
                }

                output.Format();
            }

            return _stringBuilder.ToString();
        }
        finally
        {
            _cliOutputs.Clear();
            _stringBuilder.Clear();
        }
    }

    /// <summary>
    /// Writes the output to the standard output stream.
    /// </summary>
    /// <param name="useColoredOutput">Should the output be colored?</param>
    internal void Write(bool useColoredOutput = true)
    {
        if (useColoredOutput)
            DoColoredOutput(GetOutput(), addLineTerminator: false);
        else
            Console.Write(GetOutput());
    }

    /// <summary>
    /// Writes the output, followed by the current line terminator, to the standard output stream.
    /// </summary>
    /// <param name="useColoredOutput">Should the output be colored?</param>
    internal void WriteLine(bool useColoredOutput = true)
    {
        if (useColoredOutput)
            DoColoredOutput(GetOutput(), addLineTerminator: true);
        else
            Console.WriteLine(GetOutput());
    }

    /// <summary>
    /// Prevents single items to be appended to the same line but starting on a new line.
    /// <para>
    /// Use <see cref="CliComplexOutputBaseExtensions.WithSingleItemOnNewLine{T}(T)"/> for fluent design.
    /// </para>
    /// </summary>
    protected internal void AddSingleItemToNewLine() => _addSingleItemToNewLine = true;

    /// <summary>
    /// Adds a title to the output.
    /// <para>
    /// Use <see cref="CliComplexOutputBaseExtensions.WithTitle{T}(T, string, bool?)"/> for fluent design.
    /// </para>
    /// </summary>
    /// <param name="title">The title.</param>
    /// <param name="enabled">An optional enabled to show after the <paramref name="title"/>.</param>
    protected internal void AddTitle(string title, bool? enabled = null)
        => _title = title + GetEnabledText(enabled) + TitleDelimiter;

    /// <summary>
    /// Adds a new <see cref="KeyValueOutput"/> to the output.
    /// </summary>
    /// <param name="indentationLevel">The indentation level.</param>
    /// <returns><see cref="KeyValueOutput"/></returns>
    protected internal KeyValueOutput AddKeyValueOutput(int indentationLevel = 0)
    {
        var output = new KeyValueOutput(indentationLevel, _cliOutputs, _stringBuilder);
        _cliOutputs.Add(output);
        return output;
    }

    /// <summary>
    /// Adds a new <see cref="ListOutput"/> to the output.
    /// </summary>
    /// <param name="indentationLevel">The indentation level.</param>
    /// <returns><see cref="ListOutput"/></returns>
    protected internal ListOutput AddListOutput(int indentationLevel = 0)
    {
        var output = new ListOutput(indentationLevel, _cliOutputs, _stringBuilder);
        _cliOutputs.Add(output);
        return output;
    }

    /// <summary>
    /// Adds a new <see cref="MultiKeyValueOutput"/> to the output.
    /// </summary>
    /// <param name="indentationLevel">The indentation level.</param>
    /// <returns><see cref="MultiKeyValueOutput"/></returns>
    protected internal MultiKeyValueOutput AddMultiKeyValueOutput(int indentationLevel = 0)
    {
        var output = new MultiKeyValueOutput(indentationLevel, _cliOutputs, _stringBuilder);
        _cliOutputs.Add(output);
        return output;
    }

    /// <summary>
    /// Adds a new <see cref="MultiPropertyListOutput"/> to the output.
    /// </summary>
    /// <param name="indentationLevel">The indentation level.</param>
    /// <returns><see cref="MultiPropertyListOutput"/></returns>
    protected internal MultiPropertyListOutput AddMultiPropertyListOutput(int indentationLevel = 0)
    {
        var output = new MultiPropertyListOutput(indentationLevel, _cliOutputs, _stringBuilder);
        _cliOutputs.Add(output);
        return output;
    }

    /// <summary>
    /// Adds a new line to the output.
    /// </summary>
    /// <returns><see cref="NewLineOutput"/></returns>
    protected internal NewLineOutput AddNewLine()
    {
        var output = new NewLineOutput(_cliOutputs, _stringBuilder);
        _cliOutputs.Add(output);
        return output;
    }

    /// <summary>
    /// Adds a new <see cref="PropertyListOutput"/> to the output.
    /// </summary>
    /// <param name="indentationLevel">The indentation level.</param>
    /// <returns><see cref="PropertyListOutput"/></returns>
    protected internal PropertyListOutput AddPropertyListOutput(int indentationLevel = 0)
    {
        var output = new PropertyListOutput(indentationLevel, _cliOutputs, _stringBuilder);
        _cliOutputs.Add(output);
        return output;
    }

    /// <summary>
    /// Adds a new <see cref="TableOutput"/> to the output.
    /// </summary>
    /// <param name="indentationLevel">The indentation level.</param>
    /// <returns><see cref="TableOutput"/></returns>
    protected internal TableOutput AddTableOutput(int indentationLevel = 0)
    {
        var output = new TableOutput(indentationLevel, _cliOutputs, _stringBuilder);
        _cliOutputs.Add(output);
        return output;
    }

    /// <summary>
    /// Adds an error message to the output.
    /// <para>
    /// Use <see cref="CliComplexOutputBaseExtensions.WithError{T}(T, string)"/> for fluent design.
    /// </para>
    /// </summary>
    /// <remarks>
    /// The error message is displayed before the output.
    /// </remarks>
    /// <param name="errorText">The error text.</param>
    protected internal void AddErrorMessage(string errorText)
        => _errorMessages.Add(errorText);

    /// <summary>
    /// Adds an error message to the output using the specified arguments.
    /// <para>
    /// Use <see cref="CliComplexOutputBaseExtensions.WithError{T}(T, string, CliOutputValue[])"/> for fluent design.
    /// </para>
    /// </summary>
    /// <remarks>
    /// The error message is displayed before the output.
    /// </remarks>
    /// <param name="errorText">The error text.</param>
    /// <param name="args">The arguments to replace the placeholders in the error text.</param>
    protected internal void AddErrorMessage(string errorText, params CliOutputValue[]? args)
    {
        AddErrorMessage(
            FormatMessage(
                errorText,
                args,
                CommandLineStrings.ErrorMessageEmpty,
                CommandLineStrings.ErrorMessageInvalidFormat));
    }

    /// <summary>
    /// Adds an information message to the output.
    /// <para>
    /// Use <see cref="CliComplexOutputBaseExtensions.WithInformation{T}(T, string)"/> for fluent design.
    /// </para>
    /// </summary>
    /// <remarks>
    /// The information message is displayed before the output.
    /// </remarks>
    /// <param name="informationText">The information text.</param>
    protected internal void AddInformationMessage(string informationText)
        => _informationMessages.Add(informationText);

    /// <summary>
    /// Adds an information message to the output using the specified arguments.
    /// <para>
    /// Use <see cref="CliComplexOutputBaseExtensions.WithInformation{T}(T, string, CliOutputValue[])"/> for fluent design.
    /// </para>
    /// </summary>
    /// <remarks>
    /// The information message is displayed before the output.
    /// </remarks>
    /// <param name="informationText">The information text.</param>
    /// <param name="args">The arguments to replace the placeholders in the information text.</param>
    protected internal void AddInformationMessage(string informationText, params CliOutputValue[]? args)
    {
        AddInformationMessage(
            FormatMessage(
                informationText,
                args,
                CommandLineStrings.InformationMessageEmpty,
                CommandLineStrings.InformationMessageInvalidFormat));
    }

    /// <summary>
    /// Adds a warning message to the output.
    /// <para>
    /// Use <see cref="CliComplexOutputBaseExtensions.WithWarning{T}(T, string)"/> for fluent design.
    /// </para>
    /// </summary>
    /// <remarks>
    /// The warning message is displayed before the output.
    /// </remarks>
    /// <param name="warningText">The warning text.</param>
    protected internal void AddWarningMessage(string warningText)
        => _warningMessages.Add(warningText);

    /// <summary>
    /// Adds a warning message to the output using the specified arguments.
    /// <para>
    /// Use <see cref="CliComplexOutputBaseExtensions.WithWarning{T}(T, string, CliOutputValue[])"/> for fluent design.
    /// </para>
    /// </summary>
    /// <remarks>
    /// The warning message is displayed before the output.
    /// </remarks>
    /// <param name="warningText">The warning text.</param>
    /// <param name="args">The arguments to replace the placeholders in the warning text.</param>
    protected internal void AddWarningMessage(string warningText, params CliOutputValue[]? args)
    {
        AddWarningMessage(
            FormatMessage(
                warningText,
                args,
                CommandLineStrings.WarningMessageEmpty,
                CommandLineStrings.WarningMessageInvalidFormat));
    }

    /// <summary>
    /// Formats the CLI output's values. For the title see <see cref="AddTitle"/>.
    /// </summary>
    /// <returns><see cref="string"/></returns>
    private protected abstract void Format();

    private protected void AppendLine(string value)
    {
        var indentationLevel = HasTitle ? _indentationLevel + 1 : _indentationLevel;

        if (!_addSingleItemToNewLine &&
            ItemCount == 1)
        {
            var whiteSpace = HasTitle
                ? WhiteSpace.ToString()
                : GetIndentedString(indentationLevel, string.Empty);

            _stringBuilder.AppendLine(whiteSpace + value);
        }
        else
        {
            var indentedValue = GetIndentedString(indentationLevel, value);

            _stringBuilder.AppendLine(indentedValue);
        }
    }

    private protected void AppendLine() => _stringBuilder.AppendLine();

    private protected static string GetEnabledText(bool? enabled)
    {
        if (enabled is true)
            return WhiteSpace + $"({CommandLineStrings.enabledLower})";
        else if (enabled is false)
            return WhiteSpace + $"({CommandLineStrings.disabled})";
        else
            return string.Empty;
    }

    private protected static string GetLeaderPattern(int count)
        => string.Concat(Enumerable.Repeat(LeaderPattern, count));

    private protected static string GetWhiteSpace(int count)
        => string.Concat(Enumerable.Repeat(WhiteSpace, count));

    private void AppendMessagesIfPresent(CliComplexOutputBase output)
    {
        if (output.HasErrorMessages)
            _errorMessages.ForEach(error => _stringBuilder.AppendLine(GetErrorMessage(error)));

        if (output.HasWarningMessages)
            _warningMessages.ForEach(warning => _stringBuilder.AppendLine(GetWarningMessage(warning)));

        if (output.HasInformationMessages)
            _informationMessages.ForEach(info => _stringBuilder.AppendLine(GetInformationMessage(info)));

        if (output.HasWarningMessages ||
            output.HasErrorMessages ||
            output.HasInformationMessages)
        {
            if (output.HasTitle ||
                output.ItemCount > 0)
            {
                _stringBuilder.AppendLine();
            }
        }
    }

    private static void DoColoredOutput(string output, bool addLineTerminator)
    {
        var lines = output.Split(["\r\n", "\n"], StringSplitOptions.None);

        for (var i = 0; i < lines.Length; i++)
        {
            if (lines[i].StartsWith(CommandLineStrings.ERROR))
            {
                DoColoredErrorOutput(lines[i]);
            }
            else if (lines[i].StartsWith(CommandLineStrings.WARNING))
            {
                DoColoredWarningOutput(lines[i]);
            }
            else if (lines[i].StartsWith(CommandLineStrings.INFORMATION))
            {
                DoColoredInformationOutput(lines[i]);
            }
            else
            {
                Console.Write(lines[i]);
            }

            if (i < lines.Length - 1)
                Console.Write(Environment.NewLine);
        }

        if (addLineTerminator)
            Console.Write(Environment.NewLine);
    }

    private string FormatMessage(
        string message,
        CliOutputValue[]? args,
        string emptyMessageResource,
        string invalidFormatResource)
    {
        if (string.IsNullOrWhiteSpace(message))
            return emptyMessageResource;

        if (args is null ||
            args.Length == 0)
        {
            return message;
        }

        try
        {
            return string.Format(message, args);
        }
        catch (FormatException ex)
        {
            return invalidFormatResource + " " + ex.Message;
        }
    }
}

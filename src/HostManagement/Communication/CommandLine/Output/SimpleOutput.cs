namespace HostManagement.Communication.CommandLine.Output;

/// <summary>
/// Represents a <see cref="SimpleOutput"/>.
/// </summary>
/// <remarks>
/// <code>
/// Simple text line
/// </code>
/// </remarks>
internal sealed class SimpleOutput : CliOutputBase
{
    private SimpleOutput() { }

    /// <summary>
    /// Writes a value, followed by the current line terminator, to the standard output stream.
    /// </summary>
    /// <param name="value">The value.</param>
    internal static void WriteLine(CliOutputValue value)
    {
        Console.WriteLine(GetNonEmptyValue(value));
        Console.WriteLine();
    }

    /// <summary>
    /// Writes a formatted string using the specified arguments, followed by the current
    /// line terminator, to the standard output stream.
    /// </summary>
    /// <param name="format">The format string.</param>
    /// <param name="args">The arguments to replace the placeholders in the format string.</param>
    internal static void WriteLine(string format, params CliOutputValue[]? args)
        => WriteLine(GetCliOutputValue(format, args));

    /// <summary>
    /// Writes a value, followed by the current line terminator, to the standard output stream.
    /// <para>
    /// The output is colored.<br/>
    /// The output is formatted as error message, e.g ERROR: MESSAGE.
    /// </para>
    /// </summary>
    /// <param name="value">The value.</param>
    internal static void WriteLineError(CliOutputValue value)
        => WriteLineError(true, value);

    /// <summary>
    /// Writes a value, followed by the current line terminator, to the standard output stream.
    /// <para>
    /// The output is formatted as error message, e.g ERROR: MESSAGE.
    /// </para>
    /// </summary>
    /// <param name="useColoredOutput">Determines whether the output should be colored.</param>
    /// <param name="value">The value.</param>
    internal static void WriteLineError(bool useColoredOutput, CliOutputValue value)
    {
        var nonEmptyValue = GetNonEmptyValue(value);
        var message = GetErrorMessage(nonEmptyValue);

        if (useColoredOutput)
            DoColoredErrorOutput(message + Environment.NewLine);
        else
            Console.WriteLine(message);

        Console.WriteLine();
    }

    /// <summary>
    /// Writes a formatted string using the specified arguments, followed by the current
    /// line terminator, to the standard output stream.
    /// <para>
    /// The output is colored.<br/>
    /// The output is formatted as error message, e.g ERROR: MESSAGE.
    /// </para>
    /// </summary>
    /// <param name="format">The format string.</param>
    /// <param name="args">The arguments to replace the placeholders in the format string.</param>
    internal static void WriteLineError(string format, params CliOutputValue[]? args)
        => WriteLineError(true, format, args);

    /// <summary>
    /// Writes a formatted string using the specified arguments, followed by the current
    /// line terminator, to the standard output stream.
    /// <para>
    /// The output is formatted as error message, e.g ERROR: MESSAGE.
    /// </para>
    /// </summary>
    /// <param name="useColoredOutput">Determines whether the output should be colored.</param>
    /// <param name="format">The format string.</param>
    /// <param name="args">The arguments to replace the placeholders in the format string.</param>
    internal static void WriteLineError(bool useColoredOutput, string format, params CliOutputValue[]? args)
        => WriteLineError(useColoredOutput, GetCliOutputValue(format, args));

    /// <summary>
    /// Writes a value, followed by the current line terminator, to the standard output stream.
    /// <para>
    /// The output is colored.<br/>
    /// The output is formatted as warning message, e.g WARNING: MESSAGE.
    /// </para>
    /// </summary>
    /// <param name="value">The value.</param>
    internal static void WriteLineWarning(CliOutputValue value)
        => WriteLineWarning(true, value);

    /// <summary>
    /// Writes a value, followed by the current line terminator, to the standard output stream.
    /// <para>
    /// The output is formatted as warning message, e.g WARNING: MESSAGE.
    /// </para>
    /// </summary>
    /// <param name="useColoredOutput">Determines whether the output should be colored.</param>
    /// <param name="value">The value.</param>
    internal static void WriteLineWarning(bool useColoredOutput, CliOutputValue value)
    {
        var nonEmptyValue = GetNonEmptyValue(value);
        var message = GetWarningMessage(nonEmptyValue);

        if (useColoredOutput)
            DoColoredWarningOutput(message + Environment.NewLine);
        else
            Console.WriteLine(message);

        Console.WriteLine();
    }

    /// <summary>
    /// Writes a formatted string using the specified arguments, followed by the current
    /// line terminator, to the standard output stream.
    /// <para>
    /// The output is colored.<br/>
    /// The output is formatted as warning message, e.g WARNING: MESSAGE.
    /// </para>
    /// </summary>
    /// <param name="format">The format string.</param>
    /// <param name="args">The arguments to replace the placeholders in the format string.</param>
    internal static void WriteLineWarning(string format, params CliOutputValue[]? args)
        => WriteLineWarning(true, format, args);

    /// <summary>
    /// Writes a formatted string using the specified arguments, followed by the current
    /// line terminator, to the standard output stream.
    /// <para>
    /// The output is formatted as warning message, e.g WARNING: MESSAGE.
    /// </para>
    /// </summary>
    /// <param name="useColoredOutput">Determines whether the output should be colored.</param>
    /// <param name="format">The format string.</param>
    /// <param name="args">The arguments to replace the placeholders in the format string.</param>
    internal static void WriteLineWarning(bool useColoredOutput, string format, params CliOutputValue[]? args)
        => WriteLineWarning(useColoredOutput, GetCliOutputValue(format, args));

    /// <summary>
    /// Writes a value, followed by the current line terminator, to the standard output stream.
    /// <para>
    /// The output is colored.<br/>
    /// The output is formatted as information message, e.g WARNING: MESSAGE.
    /// </para>
    /// </summary>
    /// <param name="value">The value.</param>
    internal static void WriteLineInformation(CliOutputValue value)
        => WriteLineInformation(true, value);

    /// <summary>
    /// Writes a value, followed by the current line terminator, to the standard output stream.
    /// <para>
    /// The output is formatted as information message, e.g INFORMATION: MESSAGE.
    /// </para>
    /// </summary>
    /// <param name="useColoredOutput">Determines whether the output should be colored.</param>
    /// <param name="value">The value.</param>
    internal static void WriteLineInformation(bool useColoredOutput, CliOutputValue value)
    {
        var nonEmptyValue = GetNonEmptyValue(value);
        var message = GetInformationMessage(nonEmptyValue);

        if (useColoredOutput)
            DoColoredInformationOutput(message + Environment.NewLine);
        else
            Console.WriteLine(message);

        Console.WriteLine();
    }

    /// <summary>
    /// Writes a formatted string using the specified arguments, followed by the current
    /// line terminator, to the standard output stream.
    /// <para>
    /// The output is colored.<br/>
    /// The output is formatted as information message, e.g INFORMATION: MESSAGE.
    /// </para>
    /// </summary>
    /// <param name="format">The format string.</param>
    /// <param name="args">The arguments to replace the placeholders in the format string.</param>
    internal static void WriteLineInformation(string format, params CliOutputValue[]? args)
        => WriteLineInformation(true, format, args);

    /// <summary>
    /// Writes a formatted string using the specified arguments, followed by the current
    /// line terminator, to the standard output stream.
    /// <para>
    /// The output is formatted as information message, e.g INFORMATION: MESSAGE.
    /// </para>
    /// </summary>
    /// <param name="useColoredOutput">Determines whether the output should be colored.</param>
    /// <param name="format">The format string.</param>
    /// <param name="args">The arguments to replace the placeholders in the format string.</param>
    internal static void WriteLineInformation(bool useColoredOutput, string format, params CliOutputValue[]? args)
        => WriteLineInformation(useColoredOutput, GetCliOutputValue(format, args));

    private static CliOutputValue GetCliOutputValue(string format, params CliOutputValue[]? args)
    {
        if (args is null ||
            args.Length == 0 ||
            string.IsNullOrWhiteSpace(format))
        {
            return CliOutputValue.Create(format);
        }
        else
        {
            return CliOutputValue.Create(string.Format(format, args));
        }
    }

    private static string GetNonEmptyValue(CliOutputValue value)
        => string.IsNullOrWhiteSpace(value)
            ? EmptyValue
            : value.ToString();
}

namespace HostManagement.Communication.CommandLine.Output;

/// <summary>
/// Represents the abstract base class for a CLI output.
/// </summary>
internal abstract partial class CliOutputBase
{
    private protected static string GetErrorMessage(string errorText)
        => CommandLineStrings.ERROR + LogMessageDelimiter + WhiteSpace + errorText;

    private protected static string GetInformationMessage(string informationText)
        => CommandLineStrings.INFORMATION + LogMessageDelimiter + WhiteSpace + informationText;

    private protected static string GetWarningMessage(string warningText)
        => CommandLineStrings.WARNING + LogMessageDelimiter + WhiteSpace + warningText;

    private protected string GetIndentedString(int indentationLevel, string value)
    {
        var indentation = Enumerable.Repeat(IndentationPattern, indentationLevel);

        return string.Concat(indentation) + value;
    }

    private protected static void DoColoredErrorOutput(string errorMessage)
        => DoColoredMessageOutput(ConsoleColor.Red, CommandLineStrings.ERROR, errorMessage);

    private protected static void DoColoredInformationOutput(string informationMessage)
        => DoColoredMessageOutput(ConsoleColor.Blue, CommandLineStrings.INFORMATION, informationMessage);

    private protected static void DoColoredWarningOutput(string warningMessage)
        => DoColoredMessageOutput(ConsoleColor.Yellow, CommandLineStrings.WARNING, warningMessage);

    private static void DoColoredMessageOutput(ConsoleColor color, string messageIndicator, string message)
    {
        if (Console.IsOutputRedirected)
        {
            Console.Write(message);
        }
        else
        {
            var indicatorLength = messageIndicator.Length + LogMessageDelimiter.Length;
            var indicator = message[..indicatorLength];
            var text = message[indicatorLength..];

            Console.ForegroundColor = color;
            Console.Write(indicator);
            Console.ResetColor();
            Console.Write(text);
        }
    }
}

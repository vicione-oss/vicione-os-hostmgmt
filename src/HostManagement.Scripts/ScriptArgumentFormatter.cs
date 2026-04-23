namespace HostManagement.Scripts;

internal static class ScriptArgumentFormatter
{
    internal static string GetFormattedArguments(params IEnumerable<string> arguments)
    {
        if (!arguments.Any())
            return string.Empty;

        // Escape arguments in case they contain spaces or special characters
        var formattedArguments = arguments.Select(argument => $"\"{argument.Replace("\"", "\\\"")}\"");

        return string.Join(" ", formattedArguments);
    }
}

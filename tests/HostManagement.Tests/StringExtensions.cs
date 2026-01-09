namespace HostManagement.Tests;

internal static class StringExtensions
{
    internal static string UseEnvironmentNewLine(this string input)
        => input
            .Replace("\r\n", "\n")               // normalize Windows newlines to Unix
            .Replace("\r", "\n")                 // normalize old Mac newlines to Unix
            .Replace("\n", Environment.NewLine); // normalize all to system
}

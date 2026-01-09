namespace HostManagement.Scripts.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="string"/> type.
/// </summary>
internal static class StringExtensions
{
    /// <summary>
    /// Replaces all Windows (CRLF) and Mac (CR) line endings with Unix (LF) line endings.
    /// </summary>
    /// <param name="input">The input string.</param>
    /// <returns>A string with only Unix line endings</returns>
    internal static string EnsureLFLineEndings(this string input)
        => input
            .Replace("\r\n", "\n")
            .Replace("\r", "\n");
}

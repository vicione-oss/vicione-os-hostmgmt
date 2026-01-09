using System.CommandLine.Parsing;
using System.Net;

namespace HostManagement.Communication.CommandLine;

/// <summary>
/// Provides parsing for the IPAddress type.
/// </summary>
internal static class IPAddressParser
{
    /// <summary>
    /// Parses a single IP address from the command line argument result.
    /// </summary>
    /// <param name="result">The argument result.</param>
    /// <returns><see cref="IPAddress"/></returns>
    internal static IPAddress ParseSingle(ArgumentResult result)
        => ConvertIPAddressToken(result, result.Tokens.Single()) ?? IPAddress.None;

    /// <summary>
    /// Parses multiple IP addresses from the command line argument result.
    /// </summary>
    /// <param name="result">The argument result.</param>
    /// <returns>An array of <see cref="IPAddress"/></returns>
    internal static IPAddress[] ParseMultiple(ArgumentResult result)
        => result.Tokens
            .Select(token => ConvertIPAddressToken(result, token))
            .Where(ip => ip is not null)
            .Cast<IPAddress>()
            .ToArray();

    private static IPAddress? ConvertIPAddressToken(ArgumentResult result, Token token)
    {
        if (IPAddress.TryParse(token.Value, out var ipValue))
            return ipValue;

        result.AddError($"Invalid IP address: '{token.Value}'.");
        return null;
    }
}

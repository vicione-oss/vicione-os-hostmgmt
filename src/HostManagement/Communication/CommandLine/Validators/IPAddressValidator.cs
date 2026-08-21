using System.CommandLine.Parsing;
using System.Globalization;
using System.Text;
using HostManagement.Communication.CommandLine.Resources;

namespace HostManagement.Communication.CommandLine.Validators;

internal static class IPAddressValidator
{
    /// <summary>
    /// Validates each specified IP address argument and sets an error message if it does not
    /// consist of exactly 4 octets.
    /// </summary>
    /// <param name="argumentResult">The argument result.</param>
    /// <param name="ipAddressDescription">A description of the IP address used for the beginning
    /// of the error message (e.g. "Netmask" -> "Netmask must have...",
    /// "IP address" -> "IP address must have..." etc.).</param>
    internal static void Validate(ArgumentResult argumentResult, string ipAddressDescription)
    {
        var tokens = argumentResult.Tokens;
        var errorMessageBuilder = new StringBuilder();

        foreach (var token in tokens)
        {
            if (string.IsNullOrWhiteSpace(token?.Value))
                continue;

            var parts = token.Value.Split('.');

            if (parts.Length != 4)
            {
                errorMessageBuilder.AppendLine(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        CompositeFormat.Parse(CliStrings.Network.Interface.IPv4.AddressMustHave4Octets),
                        ipAddressDescription,
                        parts.Length));
            }
        }

        if (errorMessageBuilder.Length == 0)
            return;

        argumentResult.AddError(errorMessageBuilder.ToString().TrimEnd('\r', '\n'));
    }

    /// <summary>
    /// Validates each specified IP address argument and sets an error message if it does not
    /// consist of exactly 4 octets.
    /// </summary>
    /// <param name="optionResult">The option result.</param>
    /// <param name="ipAddressDescription">A description of the IP address used for the beginning
    /// of the error message (e.g. "Netmask" -> "Netmask must have...",
    /// "IP address" -> "IP address must have..." etc.).</param>
    internal static void Validate(OptionResult optionResult, string ipAddressDescription)
    {
        var tokens = optionResult.Tokens;
        var errorMessageBuilder = new StringBuilder();

        foreach (var token in tokens)
        {
            if (string.IsNullOrWhiteSpace(token?.Value))
                continue;

            var parts = token.Value.Split('.');

            if (parts.Length != 4)
            {
                errorMessageBuilder.AppendLine(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        CompositeFormat.Parse(CliStrings.Network.Interface.IPv4.AddressMustHave4Octets),
                        ipAddressDescription,
                        parts.Length));
            }
        }

        if (errorMessageBuilder.Length == 0)
            return;

        optionResult.AddError(errorMessageBuilder.ToString().TrimEnd('\r', '\n'));
    }
}

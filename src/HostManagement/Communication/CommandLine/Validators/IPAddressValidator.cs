using System.CommandLine.Parsing;
using System.Globalization;
using System.Text;

namespace HostManagement.Communication.CommandLine.Validators;

internal static class IPAddressValidator
{
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
                        CompositeFormat.Parse(CommandLineStrings.IPAddressMustHave4Octets),
                        ipAddressDescription,
                        parts.Length));
            }
        }

        if (errorMessageBuilder.Length == 0)
            return;

        optionResult.ErrorMessage = errorMessageBuilder.ToString().TrimEnd('\r', '\n');
    }
}

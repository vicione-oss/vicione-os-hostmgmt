using System.Globalization;
using System.Text;

namespace HostManagement.Shared.Validation.Validators.Proxy;

/// <summary>
/// Represents the validator for a proxy port.
/// </summary>
internal sealed class ProxyPortValidator
{
    /* 
     * Port 0 is reserved by the system and cannot be used as a fixed proxy port. It is often used
     * to indicate that the system should choose an available port dynamically.
     * 
     * Ports 1-1023 are known as well-known or system ports. They are typically reserved for
     * standard services (e.g.: HTTP on 80, HTTPS on 443, FTP on 21) and usually require
     * administrative privileges to bind to. Using these for a proxy can lead to conflicts or
     * permission issues.
    */

    internal const int MinPort = 1;
    internal const int MaxPort = 65_535;

    public static Task Validate(string? portString, ValidationResult result)
    {
        if (!int.TryParse(portString, out var port))
        {
            result.AddErrorMessage(
                string.Format(
                    CultureInfo.InvariantCulture,
                    CompositeFormat.Parse(
                        ValidationResultMessages.ProxyPortMustBeNumber),
                    portString),
                ErrorCodes.ValidationFailed);

            return Task.CompletedTask;
        }

        return Validate(port, result);
    }

    public static Task Validate(int? port, ValidationResult result)
    {
        if (port is null)
        {
            result.AddErrorMessage(
                ValidationResultMessages.ProxyPortCanNotBeNull,
                ErrorCodes.ValidationFailed);

            return Task.CompletedTask;
        }

        if (port < MinPort)
        {
            result.AddErrorMessage(
                string.Format(
                    CultureInfo.InvariantCulture,
                    CompositeFormat.Parse(
                        ValidationResultMessages.ProxyPortCanNotBeSmallerThanMinimum),
                    port,
                    MinPort),
                ErrorCodes.ValidationFailed);

            return Task.CompletedTask;
        }

        if (port > MaxPort)
        {
            result.AddErrorMessage(
                string.Format(
                    CultureInfo.InvariantCulture,
                    CompositeFormat.Parse(
                        ValidationResultMessages.ProxyPortCanNotBeGreaterThanMaximum),
                    port,
                    MaxPort),
                ErrorCodes.ValidationFailed);

            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }
}

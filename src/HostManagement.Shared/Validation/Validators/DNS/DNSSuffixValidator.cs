using System.Globalization;
using System.Text;
using HostManagement.Shared.Contracts.Network;

namespace HostManagement.Shared.Validation.Validators.DNS;

/// <summary>
/// Represents the validator for a <see cref="NetworkDNSSettings.DNSSuffix"/>.
/// </summary>
internal sealed class DNSSuffixValidator
{
    private const string CustomDomainName = "Primary DNS suffix";

    public static Task Validate(
        string hostname,
        bool dnsSuffixEnabled,
        string dnsSuffix,
        ValidationResult result)
    {
        if (!dnsSuffixEnabled)
            return Task.CompletedTask;

        if (IsDNSSuffixEmpty(dnsSuffix, result))
            return Task.CompletedTask;

        if (IsHostnameAndSuffixTooLong(hostname, dnsSuffix, result))
            return Task.CompletedTask;

        /*
         * A trailing dot in a DNS suffix is regarded as invalid. Most resolvers automatically
         * append the root dot during DNS queries. Linux typically strips the trailing dot when
         * saving the primary DNS suffix.
         * 
         * Single-label DNS suffixes should only be used for internal networks.
        */

        if (dnsSuffix.Contains('.'))
            FQDNValidator.Validate(dnsSuffix, result, CustomDomainName);
        else
            DomainLabelValidator.Validate(dnsSuffix, result, CustomDomainName);

        return Task.CompletedTask;
    }

    private static bool IsDNSSuffixEmpty(
        string dnsSuffix,
        ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(dnsSuffix))
        {
            result.AddErrorMessage(
                ValidationResultMessages.DNSSuffixCanNotBeEmpty,
                ErrorCodes.ValidationFailed);

            return true;
        }

        return false;
    }

    private static bool IsHostnameAndSuffixTooLong(
        string hostname,
        string dnsSuffix,
        ValidationResult result)
    {
        // Hostname + '.' + DNSSuffix = FQDN must be max. 253 characters
        const int MaxFqdnLength = 253;

        if (hostname.Length + 1 + dnsSuffix.Length > MaxFqdnLength)
        {
            result.AddErrorMessage(
                string.Format(
                    CultureInfo.InvariantCulture,
                    CompositeFormat.Parse(
                        ValidationResultMessages.HostnameAndSuffixHaveTooManyCharacters),
                    hostname,
                    dnsSuffix),
                ErrorCodes.ValidationFailed);

            return true;
        }

        return false;
    }
}

using HostManagement.Shared.Contracts.Network;

namespace HostManagement.Shared.Validation.Validators.DNS;

/// <summary>
/// Represents the validator for a <see cref="NetworkDNSSettings.SearchDomains"/>.
/// </summary>
internal sealed class SearchDomainsValidator
{
    private const string CustomDomainName = "Search domain";

    public static Task Validate(
        bool searchDomainsEnabled,
        List<string> searchDomains,
        ValidationResult result)
    {
        if (!searchDomainsEnabled)
            return Task.CompletedTask;

        foreach (var searchDomain in searchDomains)
            ValidateSearchDomain(searchDomain, result);

        return Task.CompletedTask;
    }

    private static void ValidateSearchDomain(string searchDomain, ValidationResult result)
    {
        if (searchDomain.Contains('.'))
            FQDNValidator.Validate(searchDomain, result, CustomDomainName);
        else
            DomainLabelValidator.Validate(searchDomain, result, CustomDomainName);
    }
}

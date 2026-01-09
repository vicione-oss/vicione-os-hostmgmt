namespace HostManagement.Shared.Validation.Utils;

/// <summary>
/// Provides utility methods to check a Full Qualified Domain Name (FQDN).
/// </summary>
internal static class FQDNValidatorUtils
{
    /// <summary>
    /// Determines if a domain name has more than 253 characters.
    /// </summary>
    /// <param name="domainName">The domain name.</param>
    /// <returns><see langword="true"/> if more than 253 characters found, otherwise <see langword="false"/></returns>
    internal static bool HasDomainNameTooManyCharacters(string domainName)
    {
        if (domainName.Length > 253)
            return true;

        return false;
    }

    /// <summary>
    /// Determines if a domain name has leading or trailing dots.
    /// </summary>
    /// <param name="domainName">The domain name.</param>
    /// <returns><see langword="true"/> if leading or trailing dots found, otherwise <see langword="false"/></returns>
    internal static bool HasDomainNameLeadingOrTrailingDots(string domainName)
    {
        if (domainName.StartsWith('.'))
            return true;

        if (domainName.EndsWith('.'))
            return true;

        return false;
    }

    /// <summary>
    /// Determines if a domain name has consecutive dots.
    /// </summary>
    /// <param name="domainName">The domain name.</param>
    /// <returns><see langword="true"/> if consecutive dots found, otherwise <see langword="false"/></returns>
    internal static bool HasDomainNameConsecutiveDots(string domainName)
    {
        if (domainName.Contains(".."))
            return true;

        return false;
    }

    /// <summary>
    /// Determines if there are less than two domain labels.
    /// </summary>
    /// <param name="domainLabels">The domain labels.</param>
    /// <returns><see langword="true"/> if less than two domain labels found, otherwise
    /// <see langword="false"/></returns>
    internal static bool HasLessThanTwoDomainLabels(string[] domainLabels)
    {
        if (domainLabels.Length < 2)
            return true;

        return false;
    }

    /// <summary>
    /// Determines if there are more than one domain label with only digits.
    /// </summary>
    /// <param name="domainLabels">The domain labels.</param>
    /// <returns><see langword="true"/> if more than one domain label found, otherwise
    /// <see langword="false"/></returns>
    internal static bool HasMoreThanOneNumericLabel(string[] domainLabels)
    {
        var count = 0;

        foreach (var label in domainLabels)
        {
            if (HasDomainLabelOnlyDigits(label))
                count++;
        }

        return count > 1;
    }

    /// <summary>
    /// Determines if a top-level domain has an invalid character count.
    /// </summary>
    /// <remarks>
    /// Top-level domain must have more than 1 and less than 64 characters.
    /// </remarks>
    /// <param name="topLevelDomain">The top-level domain.</param>
    /// <returns><see langword="true"/> if less than 1 or more than 64 characters found, otherwise
    /// <see langword="false"/></returns>
    internal static bool HasTopLevelDomainInvalidCharacterCount(string topLevelDomain)
    {
        if (topLevelDomain.Length < 2)
            return true;

        if (topLevelDomain.Length > 63)
            return true;

        return false;
    }

    /// <summary>
    /// Determines if a domain label has an invalid character count.
    /// </summary>
    /// <remarks>
    /// Domain label must have more than 0 and less than 64 characters.
    /// </remarks>
    /// <param name="domainLabel">The domain label.</param>
    /// <returns><see langword="true"/> if more than 0 and less than 64 characters found, otherwise
    /// <see langword="false"/></returns>
    internal static bool HasDomainLabelInvalidCharacterCount(string domainLabel)
    {
        if (domainLabel.Length < 1)
            return true;

        if (domainLabel.Length > 63)
            return true;

        return false;
    }

    /// <summary>
    /// Determines if a domain label has only digits.
    /// </summary>
    /// <remarks>
    /// Domain label can not have only digits.
    /// </remarks>
    /// <param name="domainLabel">The domain label.</param>
    /// <returns><see langword="true"/> if only digits found, otherwise <see langword="false"/></returns>
    internal static bool HasDomainLabelOnlyDigits(string domainLabel)
    {
        foreach (var character in domainLabel)
        {
            if (!char.IsAsciiDigit(character))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Determines if a domain label has an invalid character.
    /// </summary>
    /// <remarks>
    /// Domain label must start and end with letter or digit; other characters can also be a hyphen.
    /// </remarks>
    /// <param name="domainLabel">The domain label.</param>
    /// <returns><see langword="true"/> if invalid characters found, otherwise <see langword="false"/></returns>
    internal static bool HasDomainLabelInvalidCharacter(string domainLabel)
    {
        if (!char.IsAsciiLetterOrDigit(domainLabel[0]))
            return true;

        if (!char.IsAsciiLetterOrDigit(domainLabel[^1]))
            return true;

        if (domainLabel.Length > 2)
        {
            foreach (var character in domainLabel[1..^1])
            {
                if (!char.IsAsciiLetterOrDigit(character) &&
                    character != '-')
                {
                    return true;
                }
            }
        }

        return false;
    }
}

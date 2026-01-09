using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Text;

namespace HostManagement.Shared.Validation.Validators.DNS;

/// <summary>
/// Represents the validator for a list of name servers.
/// </summary>
public sealed class NameServersValidator : IValidator<IReadOnlyList<IPAddress>>
{
    /// <summary>
    /// Validates a list of name servers asynchronously.
    /// </summary>
    /// <param name="nameServers">The list of name servers.</param>
    /// <param name="cancellationToken">A token to cancel the validation.</param>
    /// <inheritdoc/>
    public async Task<ValidationResult> ValidateAsync(
        IReadOnlyList<IPAddress> nameServers,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (nameServers.Count == 0)
                return ValidationResult.CreateWithError(DNSValidationResults.NoNameServers);

            if (HasDuplicateNameServers(nameServers, out var duplicateNameServersString))
            {
                return ValidationResult.CreateWithError(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        CompositeFormat.Parse(
                            DNSValidationResults.NameServersHaveDuplicateEntries),
                        duplicateNameServersString));
            }

            var result = new ValidationResult();
            var nameServerValidator = new NameServerValidator();

            foreach (var nameServer in nameServers)
            {
                result.AddResult(
                    await nameServerValidator
                        .ValidateAsync(nameServer, cancellationToken)
                        .ConfigureAwait(false));
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            return ValidationResult.CreateWithError(ValidationResults.ValidationCancelled);
        }
    }

    private static bool HasDuplicateNameServers(
        IReadOnlyList<IPAddress> nameServers,
        [NotNullWhen(true)] out string? duplicateNameServersString)
    {
        duplicateNameServersString = null;

        var duplicateNameServers = nameServers
            .GroupBy(address => address)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        if (duplicateNameServers.Count > 0)
        {
            duplicateNameServersString = string.Join(
                ", ",
                duplicateNameServers.Select(address => $"'{address}'"));

            return true;
        }

        return false;
    }
}

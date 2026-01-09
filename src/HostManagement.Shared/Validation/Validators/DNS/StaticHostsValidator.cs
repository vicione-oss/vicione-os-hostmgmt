using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using HostManagement.Shared.Contracts.Network;

namespace HostManagement.Shared.Validation.Validators.DNS;

/// <summary>
/// Represents the validator for a list of static hosts.
/// </summary>
public sealed class StaticHostsValidator : IValidator<IReadOnlyList<StaticHostDetail>>
{
    /// <summary>
    /// Validates a list of static hosts asynchronously.
    /// </summary>
    /// <param name="staticHosts">The list of static hosts.</param>
    /// <param name="cancellationToken">A token to cancel the validation.</param>
    /// <inheritdoc/>
    public async Task<ValidationResult> ValidateAsync(
        IReadOnlyList<StaticHostDetail> staticHosts,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (staticHosts.Count == 0)
                return ValidationResult.CreateWithError(DNSValidationResults.NoStaticHosts);

            if (HasDuplicateStaticHosts(staticHosts, out var duplicateStaticHostsString))
            {
                return ValidationResult.CreateWithError(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        CompositeFormat.Parse(
                            DNSValidationResults.StaticHostsHaveDuplicateEntries),
                        duplicateStaticHostsString));
            }

            var result = new ValidationResult();
            var staticHostValidator = new StaticHostValidator();

            foreach (var staticHost in staticHosts)
            {
                result.AddResult(
                    await staticHostValidator
                        .ValidateAsync(staticHost, cancellationToken)
                        .ConfigureAwait(false));
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            return ValidationResult.CreateWithError(ValidationResults.ValidationCancelled);
        }
    }

    private static bool HasDuplicateStaticHosts(
        IReadOnlyList<StaticHostDetail> staticHosts,
        [NotNullWhen(true)] out string? duplicateStaticHostsString)
    {
        duplicateStaticHostsString = null;

        var duplicateStaticHosts = staticHosts
            .GroupBy(staticHostDetail => staticHostDetail)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        if (duplicateStaticHosts.Count > 0)
        {
            duplicateStaticHostsString = string.Join(
                ", ",
                duplicateStaticHosts.Select(staticHost => $"'{staticHost.IPAddress}'  '{staticHost.Hostname}'"));

            return true;
        }

        return false;
    }
}

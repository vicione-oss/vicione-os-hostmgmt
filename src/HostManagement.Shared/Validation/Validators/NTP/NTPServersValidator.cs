using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace HostManagement.Shared.Validation.Validators.NTP;

/// <summary>
/// Represents the validator for a list of NTP servers.
/// </summary>
public sealed class NTPServersValidator : IValidator<IReadOnlyList<string>>
{
    /// <summary>
    /// Validates a list of NTP servers asynchronously.
    /// </summary>
    /// <param name="ntpServers">The list of NTP servers.</param>
    /// <param name="cancellationToken">A token to cancel the validation.</param>
    /// <inheritdoc/>
    public async Task<ValidationResult> ValidateAsync(
        IReadOnlyList<string> ntpServers,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (ntpServers.Count == 0)
                return ValidationResult.CreateWithError(NTPValidationResults.NoServers);

            if (HasDuplicateNTPServers(ntpServers, out var duplicateNTPServersString))
            {
                return ValidationResult.CreateWithError(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        CompositeFormat.Parse(
                            NTPValidationResults.ServersHaveDuplicateEntries),
                        duplicateNTPServersString));
            }

            var result = new ValidationResult();
            var ntpServerValidator = new NTPServerValidator();

            foreach (var ntpServer in ntpServers)
            {
                result.AddResult(
                    await ntpServerValidator
                        .ValidateAsync(ntpServer, cancellationToken)
                        .ConfigureAwait(false));
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            return ValidationResult.CreateWithError(ValidationResults.ValidationCancelled);
        }
    }

    private static bool HasDuplicateNTPServers(
        IReadOnlyList<string> ntpServers,
        [NotNullWhen(true)] out string? duplicateNTPServersString)
    {
        duplicateNTPServersString = null;

        var duplicateNTPServers = ntpServers
            .GroupBy(ntpServer => ntpServer)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        if (duplicateNTPServers.Count > 0)
        {
            duplicateNTPServersString = string.Join(
                ", ",
                duplicateNTPServers.Select(ntpServer => $"'{ntpServer}'"));

            return true;
        }

        return false;
    }
}

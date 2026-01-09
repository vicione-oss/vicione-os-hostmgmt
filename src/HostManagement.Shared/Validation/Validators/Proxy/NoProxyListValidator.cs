using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using HostManagement.Shared.Contracts.Network;

namespace HostManagement.Shared.Validation.Validators.Proxy;

/// <summary>
/// Represents the validator for a list of no-proxy items.
/// </summary>
public sealed class NoProxyListValidator : IValidator<IReadOnlyList<string>, IEnumerable<NetworkInterfaceDetail>>
{
    /// <summary>
    /// Validates a list of no-proxy items asynchronously.
    /// </summary>
    /// <param name="noProxyItems">The list of no-proxy items.</param>
    /// <param name="interfaces">The list of network interfaces.</param>
    /// <param name="cancellationToken">A token to cancel the validation.</param>
    /// <inheritdoc/>
    public async Task<ValidationResult> ValidateAsync(
        IReadOnlyList<string> noProxyItems,
        IEnumerable<NetworkInterfaceDetail> interfaces,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (noProxyItems.Count == 0)
                return ValidationResult.CreateWithError(ProxyValidationResults.NoNoProxyItems);

            if (HasDuplicateNoProxyItems(noProxyItems, out var duplicateNoProxyItemsString))
            {
                return ValidationResult.CreateWithError(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        CompositeFormat.Parse(
                            ProxyValidationResults.NoProxyItemsHaveDuplicateEntries),
                        duplicateNoProxyItemsString));
            }

            var result = new ValidationResult();
            var noProxyItemValidator = new NoProxyItemValidator();

            foreach (var noProxyItem in noProxyItems)
            {
                result.AddResult(
                    await noProxyItemValidator
                        .ValidateAsync(noProxyItem, interfaces, cancellationToken)
                        .ConfigureAwait(false));
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            return ValidationResult.CreateWithError(ValidationResults.ValidationCancelled);
        }
    }

    private static bool HasDuplicateNoProxyItems(
        IReadOnlyList<string> noProxyItems,
        [NotNullWhen(true)] out string? duplicateNoProxyItemsString)
    {
        duplicateNoProxyItemsString = null;

        var duplicateNoProxyItems = noProxyItems
            .GroupBy(noProxyItem => noProxyItem)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        if (duplicateNoProxyItems.Count > 0)
        {
            duplicateNoProxyItemsString = string.Join(
                ", ",
                duplicateNoProxyItems.Select(noProxyItem => $"'{noProxyItem}'"));

            return true;
        }

        return false;
    }
}

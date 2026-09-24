using System.Globalization;
using System.Text;
using HostManagement.Shared.Contracts.Syslog;

namespace HostManagement.Shared.Validation.Validators.Syslog;

/// <summary>
/// Represents the validator for excluded syslog facilities.
/// </summary>
public sealed class SyslogFacilitiesValidator : IValidator<IReadOnlyList<SyslogFacility>>
{
    /// <summary>
    /// Validates the specified excluded syslog facilities asynchronously.
    /// </summary>
    /// <param name="facilities">The facilities to validate.</param>
    /// <param name="cancellationToken">A token to cancel the validation.</param>
    /// <inheritdoc/>
    public Task<ValidationResult> ValidateAsync(
        IReadOnlyList<SyslogFacility> facilities,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result = new ValidationResult();

            foreach (var facility in facilities.Where(facility => !Enum.IsDefined(facility)).Distinct())
            {
                result.AddError(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        CompositeFormat.Parse(SyslogValidationResults.FacilityInvalid),
                        facility));
            }

            var duplicateFacilities = facilities
                .GroupBy(facility => facility)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToList();

            if (duplicateFacilities.Count > 0)
            {
                result.AddError(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        CompositeFormat.Parse(SyslogValidationResults.FacilitiesHaveDuplicateEntries),
                        string.Join(", ", duplicateFacilities.Select(facility => $"'{facility}'"))));
            }

            return Task.FromResult(result);
        }
        catch (OperationCanceledException)
        {
            return Task.FromResult(
                ValidationResult.CreateWithError(ValidationResults.ValidationCancelled));
        }
    }
}

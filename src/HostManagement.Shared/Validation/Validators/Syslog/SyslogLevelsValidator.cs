using System.Globalization;
using System.Text;
using HostManagement.Shared.Contracts.Syslog;

namespace HostManagement.Shared.Validation.Validators.Syslog;

/// <summary>
/// Represents the validator for excluded syslog levels.
/// </summary>
public sealed class SyslogLevelsValidator : IValidator<IReadOnlyList<SyslogLevel>>
{
    /// <summary>
    /// Validates the specified excluded syslog levels asynchronously.
    /// </summary>
    /// <param name="levels">The levels to validate.</param>
    /// <param name="cancellationToken">A token to cancel the validation.</param>
    /// <inheritdoc/>
    public Task<ValidationResult> ValidateAsync(
        IReadOnlyList<SyslogLevel> levels,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result = new ValidationResult();

            foreach (var level in levels.Where(level => !Enum.IsDefined(level)).Distinct())
            {
                result.AddError(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        CompositeFormat.Parse(SyslogValidationResults.LevelInvalid),
                        level));
            }

            var duplicateLevels = levels
                .GroupBy(level => level)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToList();

            if (duplicateLevels.Count > 0)
            {
                result.AddError(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        CompositeFormat.Parse(SyslogValidationResults.LevelsHaveDuplicateEntries),
                        string.Join(", ", duplicateLevels.Select(level => $"'{level}'"))));
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

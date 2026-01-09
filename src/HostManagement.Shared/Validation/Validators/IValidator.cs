namespace HostManagement.Shared.Validation.Validators;

/// <summary>
/// Represents a generic validator for one value.
/// </summary>
/// <typeparam name="TValue">The type of the value to validate.</typeparam>
public interface IValidator<TValue>
{
    /// <summary>
    /// Executes the validator asynchronously.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="cancellationToken">A token to cancel the validation.</param>
    /// <returns>
    /// A <see cref="Task{ValidationResult}"/> that represents the asynchronous validation.
    /// The task result contains the result of the validation.
    /// </returns>
    Task<ValidationResult> ValidateAsync(TValue value, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a generic validator for two related values.
/// </summary>
/// <typeparam name="TValue">The type of the value to validate.</typeparam>
/// <typeparam name="TRelatedValue">The type of the related value to validate against.</typeparam>
public interface IValidator<TValue, TRelatedValue>
{
    /// <summary>
    /// Executes the validator asynchronously.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="relatedValue">The related value to validate against.</param>
    /// <param name="cancellationToken">A token to cancel the validation.</param>
    /// <returns>
    /// A <see cref="Task{ValidationResult}"/> that represents the asynchronous validation.
    /// The task result contains the result of the validation.
    /// </returns>
    Task<ValidationResult> ValidateAsync(TValue value, TRelatedValue relatedValue, CancellationToken cancellationToken = default);
}

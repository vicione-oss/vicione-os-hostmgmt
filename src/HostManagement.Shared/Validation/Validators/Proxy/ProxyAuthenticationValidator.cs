namespace HostManagement.Shared.Validation.Validators.Proxy;

/// <summary>
/// Represents the validator for the proxy username and password.
/// </summary>
public sealed class ProxyAuthenticationValidator : IValidator<string, string>
{
    /// <summary>
    /// Validates the specified proxy username and password asynchronously.
    /// </summary>
    /// <param name="username">The proxy username.</param>
    /// <param name="password">The proxy password.</param>
    /// <param name="cancellationToken">A token to cancel the validation.</param>
    /// <inheritdoc/>
    public Task<ValidationResult> ValidateAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(username))
            {
                result.AddResult(
                    ValidationResult.CreateWithError(
                        ProxyValidationResults.UserCanNotBeEmpty));
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                result.AddResult(
                    ValidationResult.CreateWithError(
                        ProxyValidationResults.PasswordCanNotBeEmpty));
            }

            // All characters except control characters are allowed.
            // Special characters are later escaped (needed in /etc/environment for tools like wget, curl etc.).
            result.AddResult(ValidateUser(username));
            result.AddResult(ValidatePassword(password));

            return Task.FromResult(result);
        }
        catch (OperationCanceledException)
        {
            return Task.FromResult(
                ValidationResult.CreateWithError(
                    ValidationResults.ValidationCancelled));
        }
    }

    private static ValidationResult ValidateUser(string username)
    {
        if (username.Length > 255)
            return ValidationResult.CreateWithError(ProxyValidationResults.UserInvalidCharacterCount);

        foreach (var character in username)
        {
            if (IsControlCharacter(character))
                return ValidationResult.CreateWithError(ProxyValidationResults.UserCanNotContainControlCharacters);
        }

        return ValidationResult.Valid;
    }

    private static ValidationResult ValidatePassword(string password)
    {
        if (password.Length > 255)
            return ValidationResult.CreateWithError(ProxyValidationResults.PasswordInvalidCharacterCount);

        foreach (var character in password)
        {
            if (IsControlCharacter(character))
                return ValidationResult.CreateWithError(ProxyValidationResults.PasswordCanNotContainControlCharacters);
        }

        return ValidationResult.Valid;
    }

    private static bool IsControlCharacter(char character) => character is <= (char)0x1F or (char)0x7F;
}

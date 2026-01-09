using HostManagement.Shared.Communication.Enums;

namespace HostManagement.Shared.Communication.Contracts;

/// <summary>
/// Represents the result of setting a system configuration.
/// </summary>
public record SetSystemConfigurationResult : OperationResult
{
    /// <summary>
    /// Initializes a new <see cref="SetSystemConfigurationResult"/>.
    /// <para>
    /// Use the static factory methods to create an appropriate result (success, warning or error).
    /// </para>
    /// </summary>
    public SetSystemConfigurationResult()
        : base(nameof(SetSystemConfigurationResult))
    {
    }

    /// <inheritdoc/>
    protected override string ExpectedType => nameof(SetSystemConfigurationResult);

    /// <summary>
    /// Creates a new <see cref="SetSystemConfigurationResult"/> with
    /// <see cref="OperationStatus.Error"/>.
    /// </summary>
    /// <param name="message">The error message</param>
    /// <returns><see cref="SetSystemConfigurationResult"/></returns>
    public static SetSystemConfigurationResult CreateErrorResult(string message)
    {
        return new SetSystemConfigurationResult()
        {
            Status = OperationStatus.Error,
            Message = message
        };
    }

    /// <summary>
    /// Creates a new <see cref="SetSystemConfigurationResult"/> with
    /// <see cref="OperationStatus.Success"/>.
    /// </summary>
    /// <param name="message">The success message.</param>
    /// <returns><see cref="SetSystemConfigurationResult"/></returns>
    public static SetSystemConfigurationResult CreateSuccessResult(string message)
    {
        return new SetSystemConfigurationResult()
        {
            Status = OperationStatus.Success,
            Message = message,
        };
    }

    /// <summary>
    /// Creates a new <see cref="SetSystemConfigurationResult"/> with
    /// <see cref="OperationStatus.Warning"/>.
    /// </summary>
    /// <param name="message">The warning message.</param>
    /// <returns><see cref="SetSystemConfigurationResult"/></returns>
    public static SetSystemConfigurationResult CreateWarningResult(string message)
    {
        return new SetSystemConfigurationResult()
        {
            Status = OperationStatus.Warning,
            Message = message
        };
    }
}

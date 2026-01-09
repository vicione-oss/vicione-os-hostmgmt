using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Contracts;

namespace HostManagement.Shared.Communication.Contracts;

/// <summary>
/// Represents the result of getting a system configuration.
/// </summary>
public record GetSystemConfigurationResult : OperationResult, IOperationResultFactory<GetSystemConfigurationResult>
{
    /// <summary>
    /// Initializes a new <see cref="GetSystemConfigurationResult"/>.
    /// <para>
    /// Use the static factory methods to create an appropriate result (success, warning or error).
    /// </para>
    /// </summary>
    public GetSystemConfigurationResult()
        : base(nameof(GetSystemConfigurationResult))
    {
    }

    /// <summary>
    /// The <see cref="SystemConfiguration"/> returned from the operation.
    /// </summary>
    public required SystemConfiguration? Configuration { get; init; }

    /// <inheritdoc/>
    protected override string ExpectedType => nameof(GetSystemConfigurationResult);

    /// <summary>
    /// Creates a new <see cref="GetSystemConfigurationResult"/> with
    /// <see cref="OperationStatus.Error"/> and no <see cref="SystemConfiguration"/> (null).
    /// </summary>
    /// <param name="message">The error message</param>
    /// <returns><see cref="GetSystemConfigurationResult"/></returns>
    public static GetSystemConfigurationResult CreateErrorResult(string message)
        => new()
        {
            Status = OperationStatus.Error,
            Message = message,
            Configuration = null
        };

    /// <summary>
    /// Creates a new <see cref="GetSystemConfigurationResult"/> with
    /// <see cref="OperationStatus.Success"/> and the <see cref="SystemConfiguration"/>.
    /// </summary>
    /// <param name="message">The success message.</param>
    /// <param name="configuration">The system configuration.</param>
    /// <returns><see cref="GetSystemConfigurationResult"/></returns>
    public static GetSystemConfigurationResult CreateSuccessResult(string message, SystemConfiguration configuration)
        => new()
        {
            Status = OperationStatus.Success,
            Message = message,
            Configuration = configuration
        };

    /// <summary>
    /// Creates a new <see cref="GetSystemConfigurationResult"/> with
    /// <see cref="OperationStatus.Warning"/> and the <see cref="SystemConfiguration"/>.
    /// </summary>
    /// <param name="message">The warning message.</param>
    /// <param name="configuration">The system configuration.</param>
    /// <returns><see cref="GetSystemConfigurationResult"/></returns>
    public static GetSystemConfigurationResult CreateWarningResult(string message, SystemConfiguration configuration)
        => new()
        {
            Status = OperationStatus.Warning,
            Message = message,
            Configuration = configuration
        };
}

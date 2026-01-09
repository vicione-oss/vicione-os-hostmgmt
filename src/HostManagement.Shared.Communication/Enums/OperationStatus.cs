namespace HostManagement.Shared.Communication.Enums;

/// <summary>
/// Represents the status returned from an operation.
/// </summary>
public enum OperationStatus
{
    /// <summary>
    /// Indicates the operation was successful.
    /// </summary>
    Success,

    /// <summary>
    /// Indicates the operation completed with warnings.
    /// </summary>
    Warning,

    /// <summary>
    /// Indicates the operation failed with errors.
    /// </summary>
    Error
}

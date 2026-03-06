using HostManagement.Shared.Communication.Enums;

namespace HostManagement.Shared.Communication.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="OperationStatus"/>.
/// </summary>
public static class OperationStatusExtensions
{
    /// <summary>
    /// Gets the combined <see cref="OperationStatus"/>.
    /// </summary>
    /// <param name="status">This <see cref="OperationStatus"/>.</param>
    /// <param name="otherStatus">The other <see cref="OperationStatus"/>.</param>
    /// <returns>The combined <see cref="OperationStatus"/>.</returns>
    public static OperationStatus GetCombinedStatus(this OperationStatus status, OperationStatus otherStatus)
    {
        if (status is OperationStatus.Error || otherStatus is OperationStatus.Error)
            return OperationStatus.Error;

        if (status is OperationStatus.Warning || otherStatus is OperationStatus.Warning)
            return OperationStatus.Warning;

        return OperationStatus.Success;
    }
}

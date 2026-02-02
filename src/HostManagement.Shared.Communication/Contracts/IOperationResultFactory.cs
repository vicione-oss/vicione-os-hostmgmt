namespace HostManagement.Shared.Communication.Contracts;

/// <summary>
/// Provides static factory methods for <see cref="OperationResult"/> derivatives.
/// </summary>
/// <typeparam name="T">The concrete operation result type.</typeparam>
public interface IOperationResultFactory<T> where T : OperationResult
{
    /// <summary>
    /// Creates an error instance of <typeparamref name="T"/>.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <returns>The error instance.</returns>
    static abstract T CreateErrorResult(string message);
}

using System.Text.Json;
using System.Text.Json.Serialization;
using HostManagement.Shared.Communication.Enums;

namespace HostManagement.Shared.Communication.Contracts;

/// <summary>
/// Represents the result of an operation.
/// </summary>
public abstract record OperationResult : IJsonOnDeserialized
{
    /// <summary>
    /// Initializes a new <see cref="OperationResult"/>.
    /// </summary>
    protected OperationResult(string resultType) => ResultType = resultType;

    /// <summary>
    /// The status returned from the operation.
    /// </summary>
    public required OperationStatus Status { get; init; }

    /// <summary>
    /// The message returned from the operation.
    /// </summary>
    public required string? Message { get; init; }

    /// <summary>
    /// The type of the result.
    /// </summary>
    [JsonRequired]
    public virtual string ResultType { get; init; }

    /// <summary>
    /// The expected type of the result.
    /// </summary>
    protected abstract string ExpectedType { get; }

    /// <inheritdoc/>
    public void OnDeserialized()
    {
        if (string.IsNullOrEmpty(ResultType))
            throw new JsonException("Required property 'ResultType' is missing or empty.");

        if (ResultType != ExpectedType)
            throw new JsonException($"Type mismatch: Expected '{ExpectedType}', but got '{ResultType}'.");
    }
}

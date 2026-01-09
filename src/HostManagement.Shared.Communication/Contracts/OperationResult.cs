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
    /// The type of the result.
    /// </summary>
    [JsonRequired]
    [JsonPropertyOrder(-3)]
    public string ResultType { get; init; }

    /// <summary>
    /// The status returned from the operation.
    /// </summary>
    [JsonPropertyOrder(-2)]
    public required OperationStatus Status { get; init; }

    /// <summary>
    /// The message returned from the operation.
    /// </summary>
    [JsonPropertyOrder(-1)]
    public required string? Message { get; init; }

    /// <summary>
    /// The expected type of the result.
    /// </summary>
    protected abstract string ExpectedType { get; }

    /// <inheritdoc/>
    public void OnDeserialized()
    {
        if (string.IsNullOrWhiteSpace(ResultType))
        {
            throw new JsonException(
                $"Required property '{nameof(ResultType)}' is missing or empty " +
                $"(expected type '{ExpectedType}').");
        }

        if (ResultType != ExpectedType)
        {
            throw new JsonException(
                $"Type mismatch: Expected '{ExpectedType}', but got '{ResultType}'.");
        }
    }
}

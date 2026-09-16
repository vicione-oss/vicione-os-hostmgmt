using System.Text.Json.Serialization.Metadata;

namespace HostManagement.Shared.Communication.NamedPipe.Client;

/// <summary>
/// Describes a named pipe request topic and its response type.
/// </summary>
/// <typeparam name="TResponse">The response type.</typeparam>
/// <param name="Topic">The named pipe request topic.</param>
/// <param name="ResponseTypeInfo">Source-generated JSON metadata for the response type.</param>
public sealed record PipeRequest<TResponse>(string Topic, JsonTypeInfo<TResponse> ResponseTypeInfo);

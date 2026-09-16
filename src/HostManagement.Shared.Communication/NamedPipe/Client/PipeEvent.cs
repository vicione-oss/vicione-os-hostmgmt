using System.Text.Json.Serialization.Metadata;

namespace HostManagement.Shared.Communication.NamedPipe.Client;

/// <summary>
/// Describes a named pipe event topic and its message type.
/// </summary>
/// <typeparam name="TMessage">The event message type.</typeparam>
/// <param name="Topic">The named pipe event topic.</param>
/// <param name="MessageTypeInfo">Source-generated JSON metadata for the event message type.</param>
public sealed record PipeEvent<TMessage>(string Topic, JsonTypeInfo<TMessage> MessageTypeInfo);

namespace HostManagement.Communication.NamedPipe.RequestHandlers;

/// <summary>
/// Represents the arguments for the <see cref="INotifyPublishEventRequested.PublishEventRequested"/> event.
/// </summary>
/// <param name="Topic">The topic of the message used to identify a message callback handler.</param>
/// <param name="Content">The message content (can be JSON serialized).</param>
internal readonly record struct PublishEventRequestedEventArgs(string Topic, string Content);

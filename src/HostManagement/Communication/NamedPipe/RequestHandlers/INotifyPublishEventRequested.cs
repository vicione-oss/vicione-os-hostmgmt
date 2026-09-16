namespace HostManagement.Communication.NamedPipe.RequestHandlers;

/// <summary>
/// Notifies about a requested event to be published. 
/// </summary>
internal interface INotifyPublishEventRequested
{
    /// <summary>
    /// Occurs when a request to publish an event is made.
    /// </summary>
    event EventHandler<PublishEventRequestedEventArgs>? PublishEventRequested;
}

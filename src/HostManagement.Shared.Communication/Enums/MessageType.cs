namespace HostManagement.Shared.Communication.Enums;

/// <summary>
/// Represents a message type.
/// </summary>
public enum MessageType
{
    /// <summary>
    /// A message that will generate a response.
    /// </summary>
    Request = 0,
    /// <summary>
    /// The response to a previous request.
    /// </summary>
    Response = 1,
    /// <summary>
    /// A message generated without previous request.
    /// </summary>
    Event = 2
}

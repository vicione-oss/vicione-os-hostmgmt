namespace HostManagement.Shared.Communication.Contracts;

/// <summary>
/// Represents the <see cref="Response"/> of a request.
/// </summary>
public class Response
{
    /// <summary>
    /// The message.
    /// </summary>
    public required string Message { get; set; }
}

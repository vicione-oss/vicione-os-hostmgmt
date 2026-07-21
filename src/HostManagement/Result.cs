using HostManagement.Shared.Communication.Enums;

namespace HostManagement;

/// <summary>
/// Represents a <see cref="Result"/>.
/// </summary>
internal sealed class Result
{
    private readonly List<string> _messages = [];

    /// <summary>
    /// The list of configuration messages.
    /// </summary>
    public string Messages => string.Join(Environment.NewLine, _messages);

    /// <summary>
    /// The operation status.
    /// </summary>
    public OperationStatus OperationStatus { get; private set; } = OperationStatus.Success;

    /// <summary>
    /// Adds a new message to the list of Messages and sets <see cref="OperationStatus"/> to
    /// <see cref="OperationStatus.Error"/>.
    /// </summary>
    /// <param name="message">The error message.</param>
    public void AddError(string message)
    {
        _messages.Add(message);
        OperationStatus = OperationStatus.Error;
    }

    /// <summary>
    /// Adds a new message to the list of Messages using the specified arguments and sets
    /// <see cref="OperationStatus"/> to <see cref="OperationStatus.Error"/>.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="args">The arguments to replace the placeholders in the message.</param>
    public void AddError(string message, params object?[]? args)
    {
        if (args is null || args.Length == 0)
            AddError(message);
        else
            AddError(string.Format(message, args));
    }

    /// <summary>
    /// Adds a new message to the list of Messages and sets <see cref="OperationStatus"/> to
    /// <see cref="OperationStatus.Warning"/> if not already <see cref="OperationStatus.Error"/>.
    /// </summary>
    /// <param name="message">The warning message.</param>
    public void AddWarning(string message)
    {
        _messages.Add(message);

        if (OperationStatus != OperationStatus.Error)
            OperationStatus = OperationStatus.Warning;
    }

    /// <summary>
    /// Adds a new message to the list of Messages using the specified arguments and sets
    /// <see cref="OperationStatus"/> to <see cref="OperationStatus.Warning"/>.
    /// </summary>
    /// <param name="message">The warning message.</param>
    /// <param name="args">The arguments to replace the placeholders in the message.</param>
    public void AddWarning(string message, params object?[]? args)
    {
        if (args is null || args.Length == 0)
            AddWarning(message);
        else
            AddWarning(string.Format(message, args));
    }
}

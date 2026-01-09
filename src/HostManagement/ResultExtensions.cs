using HostManagement.Scripts.Enums;
using HostManagement.Shared.Communication.Enums;

namespace HostManagement;

internal static class ResultExtensions
{
    /// <summary>
    /// Adds a new message with a message header including the <paramref name="scriptIdentifier"/>
    /// to the list of Messages and sets <see cref="OperationStatus"/> to
    /// <see cref="OperationStatus.Error"/>.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <param name="scriptIdentifier">The script identifier.</param>
    /// <param name="message">The error message.</param>
    public static void AddScriptError(
        this Result result,
        ScriptIdentifier scriptIdentifier,
        string message)
    {
        result.AddError(
            $"Script '{scriptIdentifier}' returned error(s):" +
            Environment.NewLine +
            message);
    }

    /// <summary>
    /// Adds a new message with a message header including the <paramref name="scriptIdentifier"/>
    /// to the list of Messages using the specified arguments and sets
    /// <see cref="OperationStatus"/> to <see cref="OperationStatus.Error"/>.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <param name="scriptIdentifier">The script identifier.</param>
    /// <param name="message">The error message.</param>
    /// <param name="args">The arguments to replace the placeholders in the message.</param>
    public static void AddScriptError(
        this Result result,
        ScriptIdentifier scriptIdentifier,
        string message,
        params object?[]? args)
    {
        if (args is null || args.Length == 0)
            result.AddScriptError(scriptIdentifier, message);
        else
            result.AddScriptError(scriptIdentifier, string.Format(message, args));
    }

    /// <summary>
    /// Adds a new message with a message header including the <paramref name="scriptIdentifier"/>
    /// to the list of Messages and sets <see cref="OperationStatus"/> to
    /// <see cref="OperationStatus.Warning"/>.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <param name="scriptIdentifier">The script identifier.</param>
    /// <param name="message">The error message.</param>
    public static void AddScriptWarning(
        this Result result,
        ScriptIdentifier scriptIdentifier,
        string message)
    {
        result.AddWarning(
            $"Script '{scriptIdentifier}' returned warning(s):" +
            Environment.NewLine +
            message);
    }

    /// <summary>
    /// Adds a new message with a message header including the <paramref name="scriptIdentifier"/>
    /// to the list of Messages using the specified arguments and sets
    /// <see cref="OperationStatus"/> to <see cref="OperationStatus.Warning"/>.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <param name="scriptIdentifier">The script identifier.</param>
    /// <param name="message">The error message.</param>
    /// <param name="args">The arguments to replace the placeholders in the message.</param>
    public static void AddScriptWarning(
        this Result result,
        ScriptIdentifier scriptIdentifier,
        string message,
        params object?[]? args)
    {
        if (args is null || args.Length == 0)
            result.AddScriptWarning(scriptIdentifier, message);
        else
            result.AddScriptWarning(scriptIdentifier, string.Format(message, args));
    }
}

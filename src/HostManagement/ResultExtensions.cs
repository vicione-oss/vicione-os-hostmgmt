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
    /// to the list of Messages and sets <see cref="OperationStatus"/> to
    /// <see cref="OperationStatus.Warning"/>.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <param name="scriptIdentifier">The script identifier.</param>
    /// <param name="message">The warning message.</param>
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
}

using System.CommandLine;

namespace HostManagement.Communication.CommandLine.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="Command"/> class.
/// </summary>
internal static class CommandExtensions
{
    /// <summary>
    /// Adds <see cref="Option"/>s to the command.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="options">The options to add to the command.</param>
    /// <returns><see cref="Command"/></returns>
    public static Command Add(this Command command, params Option[] options)
    {
        foreach (var option in options)
            command.Add(option);

        return command;
    }
}

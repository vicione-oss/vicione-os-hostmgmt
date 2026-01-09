using System.CommandLine;

namespace HostManagement.Communication.CommandLine.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="Command"/> class.
/// </summary>
internal static class CommandExtensions
{
    /// <summary>
    /// Adds a new subcommand with the specified <paramref name="name"/> and
    /// <paramref name="description"/> to the command.
    /// </summary>
    /// <param name="parentCommand">The parent command.</param>
    /// <param name="name">The name of the subcommand.</param>
    /// <param name="description">The description of the subcommand.</param>
    /// <returns><see cref="Command"/></returns>
    public static Command AddCommand(this Command parentCommand, string name, string description)
    {
        var command = new Command(name, description);
        parentCommand.AddCommand(command);
        return command;
    }
}

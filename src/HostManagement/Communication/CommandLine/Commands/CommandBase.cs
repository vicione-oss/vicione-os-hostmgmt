using System.CommandLine;
using HostManagement.Communication.CommandLine.Extensions;

namespace HostManagement.Communication.CommandLine;

/// <summary>
/// Represents the abstract base class for a command.
/// </summary>
/// <param name="name">The name of the command.</param>
/// <param name="description">The description of the command.</param>
internal abstract class CommandBase(string name, string description)
    : Command(name, description)
{
    /// <summary>
    /// Adds a subcommand with the specified name, description and handler function to the command.
    /// </summary>
    /// <param name="subCommandName">The name of the subcommand.</param>
    /// <param name="subCommandDescription">The description of the subcommand.</param>
    /// <param name="handle">The handler function of the subcommand.</param>
    /// <param name="isHidden">Indicates whether the subcommand should be hidden from the CLI help output.</param>
    private protected void Add(
        string subCommandName,
        string subCommandDescription,
        Func<Task> handle,
        bool isHidden = false)
    {
        var command = new Command(subCommandName, subCommandDescription) { IsHidden = isHidden };
        command.SetHandler(handle);

        Add(command);
    }

    /// <summary>
    /// Adds a subcommand with the specified name, description, option and handler function to the command.
    /// </summary>
    /// <param name="subCommandName">The name of the subcommand.</param>
    /// <param name="subCommandDescription">The description of the subcommand.</param>
    /// <param name="option">The option of the subcommand.</param>
    /// <param name="handle">The handler function of the subcommand.</param>
    /// <param name="isHidden">Indicates whether the subcommand should be hidden from the CLI help output.</param>
    private protected void Add<T>(
        string subCommandName,
        string subCommandDescription,
        Option<T> option,
        Func<T, Task> handle,
        bool isHidden = false)
    {
        var command = new Command(subCommandName, subCommandDescription) { IsHidden = isHidden };
        command.Add(option);
        command.SetHandler(handle, option);

        Add(command);
    }

    /// <summary>
    /// Adds a subcommand with the specified name, description, options and handler function to the command.
    /// </summary>
    /// <param name="subCommandName">The name of the subcommand.</param>
    /// <param name="subCommandDescription">The description of the subcommand.</param>
    /// <param name="option1">The first option of the subcommand.</param>
    /// <param name="option2">The second option of the subcommand.</param>
    /// <param name="handle">The handler function of the subcommand.</param>
    /// <param name="isHidden">Indicates whether the subcommand should be hidden from the CLI help output.</param>
    private protected void Add<T1, T2>(
        string subCommandName,
        string subCommandDescription,
        Option<T1> option1,
        Option<T2> option2,
        Func<T1, T2, Task> handle,
        bool isHidden = false)
    {
        var command = new Command(subCommandName, subCommandDescription) { IsHidden = isHidden };
        command.Add(option1, option2);
        command.SetHandler(handle, option1, option2);

        Add(command);
    }

    /// <summary>
    /// Adds a subcommand with the specified name, description, options and handler function to the command.
    /// </summary>
    /// <param name="subCommandName">The name of the subcommand.</param>
    /// <param name="subCommandDescription">The description of the subcommand.</param>
    /// <param name="option1">The first option of the subcommand.</param>
    /// <param name="option2">The second option of the subcommand.</param>
    /// <param name="option3">The third option of the subcommand.</param>
    /// <param name="handle">The handler function of the subcommand.</param>
    /// <param name="isHidden">Indicates whether the subcommand should be hidden from the CLI help output.</param>
    private protected void Add<T1, T2, T3>(
        string subCommandName,
        string subCommandDescription,
        Option<T1> option1,
        Option<T2> option2,
        Option<T3> option3,
        Func<T1, T2, T3, Task> handle,
        bool isHidden = false)
    {
        var command = new Command(subCommandName, subCommandDescription) { IsHidden = isHidden };
        command.Add(option1, option2, option3);
        command.SetHandler(handle, option1, option2, option3);

        Add(command);
    }

    /// <summary>
    /// Adds a subcommand with the specified name, description, options and handler function to the command.
    /// </summary>
    /// <param name="subCommandName">The name of the subcommand.</param>
    /// <param name="subCommandDescription">The description of the subcommand.</param>
    /// <param name="option1">The first option of the subcommand.</param>
    /// <param name="option2">The second option of the subcommand.</param>
    /// <param name="option3">The third option of the subcommand.</param>
    /// <param name="option4">The fourth option of the subcommand.</param>
    /// <param name="handle">The handler function of the subcommand.</param>
    /// <param name="isHidden">Indicates whether the subcommand should be hidden from the CLI help output.</param>
    private protected void Add<T1, T2, T3, T4>(
        string subCommandName,
        string subCommandDescription,
        Option<T1> option1,
        Option<T2> option2,
        Option<T3> option3,
        Option<T4> option4,
        Func<T1, T2, T3, T4, Task> handle,
        bool isHidden = false)
    {
        var command = new Command(subCommandName, subCommandDescription) { IsHidden = isHidden };
        command.Add(option1, option2, option3, option4);
        command.SetHandler(handle, option1, option2, option3, option4);

        Add(command);
    }
}

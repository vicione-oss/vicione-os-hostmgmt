using System.CommandLine;

namespace HostManagement.Communication.CommandLine.Helper;

internal static class CommandHelper
{
    internal static Command ConstructCommand(
        string name,
        string description,
        Func<Task> handle,
        bool isHidden = false)
    {
        var command = new Command(name, description);
        command.IsHidden = isHidden;
        command.SetHandler(handle);

        return command;
    }

    internal static Command ConstructCommand<T>(
        string name,
        string description,
        Option<T> option,
        Func<T, Task> handle,
        bool isHidden = false)
    {
        var command = new Command(name, description);
        command.IsHidden = isHidden;
        command.AddOption(option);
        command.SetHandler(handle, option);

        return command;
    }

    internal static Command ConstructCommand<T1, T2>(
        string name,
        string description,
        Option<T1> option1,
        Option<T2> option2,
        Func<T1, T2, Task> handle,
        bool isHidden = false)
    {
        var command = new Command(name, description);
        command.IsHidden = isHidden;
        command.AddOption(option1);
        command.AddOption(option2);
        command.SetHandler(handle, option1, option2);

        return command;
    }

    internal static Command ConstructCommand<T1, T2, T3>(
        string name,
        string description,
        Option<T1> option1,
        Option<T2> option2,
        Option<T3> option3,
        Func<T1, T2, T3, Task> handle,
        bool isHidden = false)
    {
        var command = new Command(name, description);
        command.IsHidden = isHidden;
        command.AddOption(option1);
        command.AddOption(option2);
        command.AddOption(option3);
        command.SetHandler(handle, option1, option2, option3);

        return command;
    }

    internal static Command ConstructCommand<T1, T2, T3, T4>(
        string name,
        string description,
        Option<T1> option1,
        Option<T2> option2,
        Option<T3> option3,
        Option<T4> option4,
        Func<T1, T2, T3, T4, Task> handle,
        bool isHidden = false)
    {
        var command = new Command(name, description);
        command.IsHidden = isHidden;
        command.AddOption(option1);
        command.AddOption(option2);
        command.AddOption(option3);
        command.AddOption(option4);
        command.SetHandler(handle, option1, option2, option3, option4);

        return command;
    }
}

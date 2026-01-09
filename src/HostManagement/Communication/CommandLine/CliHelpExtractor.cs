using System.CommandLine;

namespace HostManagement.Communication.CommandLine;

/// <summary>
/// Utility class to extract all CLI commands and create a markdown conform document.
/// </summary>
internal static class CliHelpExtractor
{
    private static TextWriter s_output = Console.Out;

    /// <summary>
    /// Sets the <see cref="TextWriter"/>. The default is console output.
    /// </summary>
    /// <param name="writer">The <see cref="TextWriter"/>.</param>
    internal static void SetOutput(TextWriter writer) => s_output = writer;

    /// <summary>
    /// Extracts all CLI commands and creates a markdown conform document.
    /// </summary>
    /// <param name="rootCommand">The <see cref="RootCommand"/> of the CLI.</param>
    internal static void ExtractHelp(RootCommand rootCommand)
    {
        HideDefaultHelpOptionsRecursively(rootCommand);

        // Output description of the Host Management CLI followed by all commands
        s_output.WriteLine(CommandLineStrings_Common.CliHelpExtractorHostManagementDescription, VersionInfo.Version);
        OutputHelpForRootCommand(rootCommand);
        OutputHelpForSubcommands(rootCommand.Subcommands);
    }

    private static void HideDefaultHelpOptionsRecursively(Command command)
    {
        string[] aliases = ["--help", "-h", "/h", "-?", "/?"];

        foreach (var option in command.Options)
        {
            if (aliases.Any(option.Aliases.Contains))
            {
                option.Hidden = true;
                break;
            }
        }

        foreach (var sub in command.Subcommands)
            HideDefaultHelpOptionsRecursively(sub);
    }

    private static void OutputHelpForRootCommand(RootCommand rootCommand)
    {
        s_output.WriteLine("### Root Command");
        OutputCommandDetails(rootCommand);
    }

    private static void OutputHelpForSubcommands(
        IList<Command> subcommands,
        string parentCommandStructure = "")
    {
        foreach (var command in subcommands)
        {
            var commandStructure = string.IsNullOrWhiteSpace(parentCommandStructure)
                ? command.Name
                : parentCommandStructure + " " + command.Name;

            // Use "###" heading for subcommands (like 'network' or 'suite')
            // Use "####" heading for all deeper nested commands (like 'network list-interfaces')
            var commandHeading = string.IsNullOrWhiteSpace(parentCommandStructure)
                ? $"### {char.ToUpper(command.Name[0]) + command.Name[1..]} Commands"
                : $"#### {commandStructure}";

            s_output.WriteLine();
            s_output.WriteLine(commandHeading);

            if (!string.IsNullOrWhiteSpace(parentCommandStructure))
                OutputCommandDetails(command);

            if (command.Subcommands.Count > 0)
                OutputHelpForSubcommands(command.Subcommands, commandStructure);
        }
    }

    private static void OutputCommandDetails(Command command)
    {
        s_output.WriteLine();
        s_output.WriteLine("```text"); // Put commands in fenced code blocks (```) to conform with markdown standards

        command.Parse("-h").Invoke();

        s_output.WriteLine("```");
    }
}

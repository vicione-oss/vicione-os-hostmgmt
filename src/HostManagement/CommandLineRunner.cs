using System.CommandLine;
using HostManagement.Communication.CommandLine;
using HostManagement.Communication.CommandLine.Output;

namespace HostManagement;

/// <summary>
/// Represents the <see cref="CommandLineRunner"/>, which is responsible for invoking and starting
/// the command line interface.
/// </summary>
internal static partial class CommandLineRunner
{
    /// <summary>
    /// Starts the application with a command line interface.
    /// </summary>
    /// <param name="args">The arguments.</param>
    /// <returns>0 if the command succeeded, otherwise 1</returns>
    internal static async Task<int> StartCommandLine(string[] args)
    {
        try
        {
            if (args.Length == 0)
                args = ["--help"];

#if DEBUG
            if (args[0].Equals("--extract-help"))
            {
                CliHelpExtractor.ExtractHelp(CommandInterface.CreateRootCommand());
                return 0;
            }
#endif
            // Per default, if a command is cancelled, it has a termination timeout of 2 seconds.
            // For now we don't want to force termination, if cancellation takes some time.
            var invocationConfig = new InvocationConfiguration() { ProcessTerminationTimeout = null };

            return await CommandInterface
                .CreateRootCommand()
                .Parse(args)
                .InvokeAsync(invocationConfig);
        }
        catch (Exception ex)
        {
            SimpleOutput.WriteLineError(
                "An error occurred while running the command line interface. {0}",
                ex.ToString());

            return 1;
        }
    }
}

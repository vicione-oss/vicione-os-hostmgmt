using System.CommandLine;
using Microsoft.Extensions.Logging;
using HostManagement.Communication.CommandLine;
using HostManagement.Communication.NamedPipes;
using HostManagement.Configuration;
using HostManagement.Scripts;

namespace HostManagement;

/// <summary>
/// The main entry point for the application.
/// </summary>
internal static class Program
{
    private static readonly Action<ILogger, Exception> s_logServiceError = LoggerMessage.Define(LogLevel.Error, 0, "An error occurred while running the service");

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    /// <param name="args">An array of strings containing command-line arguments.</param>
    /// <returns>Exit code of the application.</returns>
    public static async Task<int> Main(string[] args)
    {
        var logger = new ConsoleLogger(EnvironmentVariables.LogLevel);

        if (args.Length == 1 &&
            args[0].Equals("--daemon"))
        {
            return await StartAsService(logger);
        }

        return await StartAsCommandLine(args);
    }

    private static async Task<int> StartAsService(ILogger logger)
    {
        try
        {
            if (EnvironmentVariables.HasInvalidVariables)
            {
                logger.LogError(
                    "The following environment variables were not specified or are invalid:" +
                    Environment.NewLine +
                    string.Join(Environment.NewLine, EnvironmentVariables.InvalidVariables));

                return 1;
            }

            ResourceManager.Initialize(logger);

            // TODO: Temporarily disabled because of an issue that occurs sometimes during system
            // start and leads to an unrecoverable state
            //if (!await ConfigurationHelper.ApplyLastSavedConfiguration())
            //    return 1;

            using ConsoleLifetime lifetime = new(logger);

            await StartNamedPipeServer(logger, lifetime.Token);

            return 0;
        }
        catch (Exception exception)
        {
            s_logServiceError(logger, exception);
            return 1;
        }
    }

    private static async Task StartNamedPipeServer(ILogger logger, CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                using var pipeServer = new PipeServer("HostPipe", logger);

                await pipeServer.WaitForConnection(cancellationToken).ConfigureAwait(false);
                await pipeServer.WaitForRemotePipeClose(cancellationToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        { }
    }

    private static async Task<int> StartAsCommandLine(string[] args)
    {
        if (args.Length == 0)
            args = ["--help"];

#if DEBUG
        if (args[0].Equals("--extract-help"))
        {
            CliHelpExtractor.ExtractHelp(CommandInterface.GetCommandStructure());

            return 0;
        }
#endif
        return await CommandInterface.GetCommandStructure().InvokeAsync(args);
    }
}

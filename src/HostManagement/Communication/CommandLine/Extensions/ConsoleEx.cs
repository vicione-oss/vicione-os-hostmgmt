using HostManagement.Configuration;
using Microsoft.Extensions.Logging;

namespace HostManagement.Communication.CommandLine.Extensions;

/// <summary>
/// Provides static methods for colored console output.
/// </summary>
internal static class ConsoleEx
{
    private static readonly ConsoleLogger s_logger = new(EnvironmentVariables.LogLevel);

    internal static void WriteLineError(string message, params object?[]? args)
        => s_logger.LogError(message, args ?? []);

    internal static void WriteLineWarning(string message, params object?[]? args)
        => s_logger.LogWarning(message, args ?? []);
}

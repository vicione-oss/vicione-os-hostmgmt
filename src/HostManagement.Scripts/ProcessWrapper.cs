using System.Diagnostics;

namespace HostManagement.Scripts;

/// <inheritdoc cref="IProcessWrapper"/>
internal sealed class ProcessWrapper : IProcessWrapper
{
    public ScriptResponse Start(string fileName, string arguments)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden
        };

        using var process = new Process
        {
            StartInfo = processStartInfo
        };

        process.Start();

        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();

        process.WaitForExit();

        return new ScriptResponse(process.ExitCode, output.TrimEnd(), error.TrimEnd());
    }
}

using System.Diagnostics;
using System.Text;

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

    public async Task<ScriptResponse> StartScriptAsync(
        ProcessStartInfo processStartInfo,
        int timeoutInMilliseconds,
        CancellationToken cancellationToken)
    {
        var processStarted = false;
        using var process = new Process { StartInfo = processStartInfo };

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var output = new StringBuilder();
            var error = new StringBuilder();

            process.OutputDataReceived += (_, e) => output.AppendLine(e.Data);
            process.ErrorDataReceived += (_, e) => error.AppendLine(e.Data);

            try
            {
                processStarted = process.Start();
            }
            catch (Exception ex)
            {
                throw new ProcessWrapperStartException(null, ex);
            }

            if (!processStarted)
                throw new ProcessWrapperStartException("No new process was started to execute the script.");

            process.BeginErrorReadLine();
            process.BeginOutputReadLine();

            using var timeoutCTS = new CancellationTokenSource(timeoutInMilliseconds);
            using var linkedCTS = CancellationTokenSource.CreateLinkedTokenSource(timeoutCTS.Token, cancellationToken);

            try
            {
                await process.WaitForExitAsync(linkedCTS.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                if (timeoutCTS.IsCancellationRequested)
                    throw new TimeoutException();
                else
                    throw;
            }

            return new ScriptResponse(process.ExitCode, output.ToString().TrimEnd(), error.ToString().TrimEnd());
        }
        finally
        {
            if (processStarted && !process.HasExited)
                process.Kill(true);
        }
    }
}

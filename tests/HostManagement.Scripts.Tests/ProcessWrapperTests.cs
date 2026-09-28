using System.Diagnostics;
using AwesomeAssertions;
using Xunit;

namespace HostManagement.Scripts.Tests;

public sealed class ProcessWrapperTests
{
    public static bool IsLinux => OperatingSystem.IsLinux();

    public sealed class Start
    {
        [Fact(Skip = "Process execution requires Linux.", SkipType = typeof(ProcessWrapperTests), SkipUnless = nameof(IsLinux))]
        public void ShouldReturnProcessOutput_WhenProcessCompletes()
        {
            // Arrange
            var sut = new ProcessWrapper();

            // Act
            var result = sut.Start(
                "/bin/sh",
                "-c \"echo stdout; echo stderr >&2; exit 42\"");

            // Assert
            result.ExitCode.Should().Be(42);
            result.StandardOutput.Should().Be("stdout");
            result.StandardError.Should().Be("stderr");
        }

        [Fact(Skip = "Process execution requires Linux.", SkipType = typeof(ProcessWrapperTests), SkipUnless = nameof(IsLinux))]
        public void ShouldTrimTrailingWhitespaceFromOutput()
        {
            // Arrange
            var sut = new ProcessWrapper();

            // Act
            var result = sut.Start(
                "/bin/sh",
                "-c \"printf 'stdout   '; printf 'stderr   ' >&2\"");

            // Assert
            result.StandardOutput.Should().Be("stdout");
            result.StandardError.Should().Be("stderr");
        }
    }

    public sealed class StartScriptAsync
    {
        [Fact(Skip = "Process execution requires Linux.", SkipType = typeof(ProcessWrapperTests), SkipUnless = nameof(IsLinux))]
        public async Task ShouldThrowProcessWrapperStartException_WhenProcessCannotBeStarted()
        {
            // Arrange
            var sut = new ProcessWrapper();

            var startInfo = new ProcessStartInfo
            {
                FileName = "/does/not/exist/hostmanagement-test",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            // Act
            var act = () => sut.StartScriptAsync(
                startInfo,
                timeoutInMilliseconds: 5_000,
                CancellationToken.None);

            // Assert
            var exception = await act.Should().ThrowAsync<ProcessWrapperStartException>();
            exception.Which.InnerException.Should().NotBeNull();
        }

        [Fact(Skip = "Process execution requires Linux.", SkipType = typeof(ProcessWrapperTests), SkipUnless = nameof(IsLinux))]
        public async Task ShouldThrowOperationCanceledException_WhenAlreadyCanceled()
        {
            // Arrange
            var sut = new ProcessWrapper();
            var startInfo = CreateProcessStartInfo("exit 0");

            using var cancellationTokenSource = new CancellationTokenSource();

            await cancellationTokenSource.CancelAsync();

            // Act
            var act = () => sut.StartScriptAsync(
                startInfo,
                timeoutInMilliseconds: 5_000,
                cancellationTokenSource.Token);

            // Assert
            await act.Should().ThrowAsync<OperationCanceledException>();
        }

        [Fact(Skip = "Process execution requires Linux.", SkipType = typeof(ProcessWrapperTests), SkipUnless = nameof(IsLinux))]
        public async Task ShouldThrowOperationCanceledException_WhenCanceledWhileProcessRunning()
        {
            // Arrange
            var sut = new ProcessWrapper();
            var startInfo = CreateProcessStartInfo("sleep 3");

            using var cancellationTokenSource = new CancellationTokenSource();

            var task = sut.StartScriptAsync(
                startInfo,
                timeoutInMilliseconds: 5_000,
                cancellationTokenSource.Token);

            // The async method has reached WaitForExitAsync at this point,
            // so the process has already been started.
            await cancellationTokenSource.CancelAsync();

            // Act
            var act = async () => await task;

            // Assert
            await act.Should().ThrowAsync<OperationCanceledException>();
        }

        [Fact(Skip = "Process execution requires Linux.", SkipType = typeof(ProcessWrapperTests), SkipUnless = nameof(IsLinux))]
        public async Task ShouldThrowTimeoutException_WhenProcessExceedsTimeout()
        {
            // Arrange
            var sut = new ProcessWrapper();
            var startInfo = CreateProcessStartInfo("sleep 3");

            // Act
            var act = () => sut.StartScriptAsync(
                startInfo,
                timeoutInMilliseconds: 500,
                CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<TimeoutException>();
        }

        [Fact(Skip = "Process execution requires Linux.", SkipType = typeof(ProcessWrapperTests), SkipUnless = nameof(IsLinux))]
        public async Task ShouldReturnProcessOutput_WhenProcessCompletes()
        {
            // Arrange
            var sut = new ProcessWrapper();
            var startInfo = CreateProcessStartInfo("echo stdout; echo stderr >&2; exit 42");

            // Act
            var result = await sut.StartScriptAsync(
                startInfo,
                timeoutInMilliseconds: 5_000,
                CancellationToken.None);

            // Assert
            result.ExitCode.Should().Be(42);
            result.StandardOutput.Should().Be("stdout");
            result.StandardError.Should().Be("stderr");
        }

        private static ProcessStartInfo CreateProcessStartInfo(string command)
        {
            var result = new ProcessStartInfo
            {
                FileName = "/bin/sh",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            result.ArgumentList.Add("-c");
            result.ArgumentList.Add(command);

            return result;
        }
    }
}

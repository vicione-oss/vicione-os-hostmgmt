using System.Diagnostics;
using System.Runtime.Versioning;
using AwesomeAssertions;
using Xunit;

namespace HostManagement.Scripts.Tests;

[Collection("Non Parallel Tests")]
[SupportedOSPlatform("linux")]
public class PipeScriptsTests
{
    private const string DefaultGroup = "vicione-suite";
    private const string DefaultPermission = "664";
    private const string DefaultUser = "vicione-suite";
    private const string PipeFilePath = "/tmp/CoreFxPipe_HostPipe";
    private const string Group = "configured-group";
    private const string User = "configured-user";

    public static bool IsLinux => OperatingSystem.IsLinux();

    public sealed class SetExecutePermissionScript : PipeScriptsTests
    {
        [Theory(Skip = "Pipe scripts require Linux.", SkipType = typeof(PipeScriptsTests), SkipUnless = nameof(IsLinux))]
        [InlineData("640")]
        [InlineData("664")]
        public async Task ShouldPassConfiguredPermissionAndFile_WhenCalled(string permission)
        {
            // Arrange
            using var context = new ScriptTestContext();

            // Act
            var response = await RunSetExecutePermission(context, permission, PipeFilePath);

            // Assert
            response.HasErrors.Should().BeFalse();
            context.GetCapturedArguments().Should().Equal(permission, PipeFilePath);
        }

        [Fact(Skip = "Pipe scripts require Linux.", SkipType = typeof(PipeScriptsTests), SkipUnless = nameof(IsLinux))]
        public async Task ShouldPassDefaultPermissionAndFile_WhenNotConfigured()
        {
            // Arrange
            using var context = new ScriptTestContext();

            // Act
            var response = await RunSetExecutePermissionWithDefault(context, PipeFilePath);

            // Assert
            response.HasErrors.Should().BeFalse();
            context.GetCapturedArguments().Should().Equal(DefaultPermission, PipeFilePath);
        }

        [Fact(Skip = "Pipe scripts require Linux.", SkipType = typeof(PipeScriptsTests), SkipUnless = nameof(IsLinux))]
        public async Task ShouldReturnError_WhenChmodFails()
        {
            // Arrange
            using var context = new ScriptTestContext();
            context.Environment["HOSTMANAGEMENT_TEST_CHMOD_EXIT_CODE"] = "1";

            // Act
            var response = await RunSetExecutePermission(context, "664", PipeFilePath);

            // Assert
            response.HasErrors.Should().BeTrue();
            response.StandardError.Should().Be($"Setting permissions for '{PipeFilePath}' failed.");
        }
    }

    public sealed class SetOwnershipScript : PipeScriptsTests
    {
        [Fact(Skip = "Pipe scripts require Linux.", SkipType = typeof(PipeScriptsTests), SkipUnless = nameof(IsLinux))]
        public async Task ShouldPassDefaultOwnershipAndFile_WhenNotConfigured()
        {
            // Arrange
            using var context = new ScriptTestContext();

            // Act
            var response = await RunSetOwnershipWithDefaults(context, PipeFilePath);

            // Assert
            response.HasErrors.Should().BeFalse();
            context.GetCapturedArguments().Should().Equal($"{DefaultUser}:{DefaultGroup}", PipeFilePath);
        }

        [Fact(Skip = "Pipe scripts require Linux.", SkipType = typeof(PipeScriptsTests), SkipUnless = nameof(IsLinux))]
        public async Task ShouldPassConfiguredOwnershipAndFile_WhenUserAndGroupExist()
        {
            // Arrange
            using var context = new ScriptTestContext();

            // Act
            var response = await RunSetOwnership(context, User, Group, PipeFilePath);

            // Assert
            response.HasErrors.Should().BeFalse();
            context.GetCapturedArguments().Should().Equal($"{User}:{Group}", PipeFilePath);
        }

        [Fact(Skip = "Pipe scripts require Linux.", SkipType = typeof(PipeScriptsTests), SkipUnless = nameof(IsLinux))]
        public async Task ShouldFallBackToRootUser_WhenUserDoesNotExist()
        {
            // Arrange
            using var context = new ScriptTestContext();
            context.Environment["HOSTMANAGEMENT_TEST_ID_EXIT_CODE"] = "1";

            // Act
            var response = await RunSetOwnership(context, User, Group, PipeFilePath);

            // Assert
            response.HasErrors.Should().BeFalse();
            context.GetCapturedArguments().Should().Equal($"root:{Group}", PipeFilePath);
        }

        [Fact(Skip = "Pipe scripts require Linux.", SkipType = typeof(PipeScriptsTests), SkipUnless = nameof(IsLinux))]
        public async Task ShouldFallBackToRootGroup_WhenGroupDoesNotExist()
        {
            // Arrange
            using var context = new ScriptTestContext();
            context.Environment["HOSTMANAGEMENT_TEST_GETENT_EXIT_CODE"] = "1";

            // Act
            var response = await RunSetOwnership(context, User, Group, PipeFilePath);

            // Assert
            response.HasErrors.Should().BeFalse();
            context.GetCapturedArguments().Should().Equal($"{User}:root", PipeFilePath);
        }

        [Fact(Skip = "Pipe scripts require Linux.", SkipType = typeof(PipeScriptsTests), SkipUnless = nameof(IsLinux))]
        public async Task ShouldFallBackToRootUserAndGroup_WhenUserAndGroupDoNotExist()
        {
            // Arrange
            using var context = new ScriptTestContext();
            context.Environment["HOSTMANAGEMENT_TEST_ID_EXIT_CODE"] = "1";
            context.Environment["HOSTMANAGEMENT_TEST_GETENT_EXIT_CODE"] = "1";

            // Act
            var response = await RunSetOwnership(context, User, Group, PipeFilePath);

            // Assert
            response.HasErrors.Should().BeFalse();
            context.GetCapturedArguments().Should().Equal("root:root", PipeFilePath);
        }

        [Fact(Skip = "Pipe scripts require Linux.", SkipType = typeof(PipeScriptsTests), SkipUnless = nameof(IsLinux))]
        public async Task ShouldReturnError_WhenChownFails()
        {
            // Arrange
            using var context = new ScriptTestContext();
            context.Environment["HOSTMANAGEMENT_TEST_CHOWN_EXIT_CODE"] = "1";

            // Act
            var response = await RunSetOwnership(context, User, Group, PipeFilePath);

            // Assert
            response.HasErrors.Should().BeTrue();
            response.StandardError.Should().Be($"Setting ownership for '{PipeFilePath}' failed.");
        }
    }

    private static Task<ScriptResponse> RunSetExecutePermission(
        ScriptTestContext context,
        string permission,
        string file)
        => context.RunScript("set_execute_permission", "--permission", permission, "--file", file);

    private static Task<ScriptResponse> RunSetExecutePermissionWithDefault(
        ScriptTestContext context,
        string file)
        => RunSetExecutePermission(context, DefaultPermission, file);

    private static async Task<ScriptResponse> RunSetOwnership(
        ScriptTestContext context,
        string user,
        string group,
        string file)
    {
        var userResponse = await context.RunScript("check_user_exists", "--user", user);

        if (userResponse.HasErrors)
            user = "root";

        var groupResponse = await context.RunScript("check_group_exists", "--group", group);

        if (groupResponse.HasErrors)
            group = "root";

        return await context.RunScript(
            "set_ownership", "--user", user, "--group", group, "--file", file);
    }

    private static Task<ScriptResponse> RunSetOwnershipWithDefaults(
        ScriptTestContext context,
        string file)
        => RunSetOwnership(context, DefaultUser, DefaultGroup, file);

    private sealed class ScriptTestContext : IDisposable
    {
        private const string ResourcePrefix =
            "HostManagement.Scripts.Resources.Scripts.targets.debian.Common";

        private const UnixFileMode ExecutableFileMode =
            UnixFileMode.UserRead |
            UnixFileMode.UserWrite |
            UnixFileMode.UserExecute;

        private readonly string _captureFile;
        private readonly string _commandsDirectory;
        private readonly string _testDirectory;

        public ScriptTestContext()
        {
            _testDirectory = Path.Combine(Path.GetTempPath(), $"HostManagement_PipeScriptsTests_{Guid.NewGuid():N}");
            _commandsDirectory = Path.Combine(_testDirectory, "commands");
            _captureFile = Path.Combine(_testDirectory, "captured-arguments");

            Directory.CreateDirectory(_commandsDirectory);

            CreateCommand(
                "chmod",
                """
                #!/usr/bin/env bash
                printf '%s\n' "$@" > "$HOSTMANAGEMENT_TEST_CAPTURE_FILE"
                exit "${HOSTMANAGEMENT_TEST_CHMOD_EXIT_CODE:-0}"
                """);

            CreateCommand(
                "chown",
                """
                #!/usr/bin/env bash
                printf '%s\n' "$@" > "$HOSTMANAGEMENT_TEST_CAPTURE_FILE"
                exit "${HOSTMANAGEMENT_TEST_CHOWN_EXIT_CODE:-0}"
                """);

            CreateCommand(
                "id",
                """
                #!/usr/bin/env bash
                exit "${HOSTMANAGEMENT_TEST_ID_EXIT_CODE:-0}"
                """);

            CreateCommand(
                "getent",
                """
                #!/usr/bin/env bash
                exit "${HOSTMANAGEMENT_TEST_GETENT_EXIT_CODE:-0}"
                """);

            var systemPath = System.Environment.GetEnvironmentVariable("PATH");

            Environment = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["HOSTMANAGEMENT_TEST_CAPTURE_FILE"] = _captureFile,
                ["PATH"] = string.IsNullOrWhiteSpace(systemPath)
                    ? _commandsDirectory
                    : $"{_commandsDirectory}:{systemPath}",
            };
        }

        public Dictionary<string, string> Environment { get; }

        public void Dispose() => Directory.Delete(_testDirectory, true);

        public string[] GetCapturedArguments() => File.ReadAllLines(_captureFile);

        public async Task<ScriptResponse> RunScript(string scriptName, params string[] arguments)
        {
            var scriptPath = await DeployScript(scriptName);
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            processStartInfo.Environment.Remove("HOSTMANAGEMENT__PIPE__CHMOD");
            processStartInfo.Environment.Remove("HOSTMANAGEMENT__PIPE__USER");
            processStartInfo.Environment.Remove("HOSTMANAGEMENT__PIPE__GROUP");

            processStartInfo.ArgumentList.Add(scriptPath);

            foreach (var argument in arguments)
                processStartInfo.ArgumentList.Add(argument);

            foreach (var (key, value) in Environment)
                processStartInfo.Environment[key] = value;

            return await new ProcessWrapper().StartScriptAsync(
                processStartInfo,
                ScriptRunner.DefaultTimeoutInMilliseconds,
                CancellationToken.None);
        }

        private void CreateCommand(string name, string content)
        {
            var commandPath = Path.Combine(_commandsDirectory, name);
            File.WriteAllText(commandPath, content);
            File.SetUnixFileMode(commandPath, ExecutableFileMode);
        }

        private async Task<string> DeployScript(string scriptName)
        {
            var scriptPath = Path.Combine(_testDirectory, $"{scriptName}.sh");
            var resourceName = $"{ResourcePrefix}.{scriptName}.sh";

            await using var resourceStream = new ResourceHelper().GetResourceStream(resourceName);
            await using var scriptStream = File.Create(scriptPath);
            await resourceStream.CopyToAsync(scriptStream);

            return scriptPath;
        }
    }
}

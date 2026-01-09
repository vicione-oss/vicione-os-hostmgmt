using FluentAssertions;
using Xunit;

namespace HostManagement.Scripts.Tests;

[Collection("Non Parallel Tests")]
public class ScriptRunnerTests
{
    private readonly string _testResourceDeployDirectory = Path.Combine("/tmp", "HostManagement_Resources");

    public class GetInstance : ScriptRunnerTests
    {
        [Fact]
        public void ShouldThrow_WhenInstanceIsNull()
        {
            // Arrange
            ScriptRunner.ResetInstance();

            // Act
            var result = ScriptRunner.GetInstance;

            // Assert
            result.Should().Throw<InvalidOperationException>()
                .WithMessage("ScriptRunner is not initialized, use Initialize first.");
        }
    }

    public class Initialize : ScriptRunnerTests
    {
        [Fact]
        public void ShouldInitializeScriptRunner_WhenCalledWithValidParameters()
        {
            // Arrange
            ScriptRunner.ResetInstance();

            // Act
            ScriptRunner.Initialize(_testResourceDeployDirectory);

            // Assert
            ScriptRunner.GetInstance().Should().NotBeNull();
        }
    }

    public class DefaultTimeoutInMillisecondsProperty : ScriptRunnerTests
    {
        [Fact]
        public void ShouldReturnDefaultTimeoutInMilliseconds_WhenCalled()
        {
            // Arrange
            ScriptRunner.ResetInstance();

            // Act
            ScriptRunner.Initialize(_testResourceDeployDirectory);

            // Assert
            ScriptRunner.GetInstance().DefaultTimeoutInMilliseconds.Should().Be(60_000);
        }
    }
}

using AwesomeAssertions;
using Xunit;

namespace HostManagement.Configuration.Tests;

public class AppPathsTests
{
    [Fact]
    public void ShouldReturnSystemConfigurationFile_WhenCalled()
        => AppPaths.SystemConfigurationFile.Should().Be("SystemConfiguration.json");

    [Fact]
    public void ShouldReturnTempPath_WhenCalled()
        => AppPaths.TempPath.Should().Be("/tmp");
}

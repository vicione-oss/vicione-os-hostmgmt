using AwesomeAssertions;
using Xunit;

namespace HostManagement.Tests;

[Collection("Non Parallel Tests")]
public sealed class CommandLineRunnerTests
{
    [Fact]
    public async Task ShouldReturnOne_WhenConfigurationTemplatePathFlagMissing()
    {
        var result = await CommandLineRunner.StartCommandLine(["--validate-configuration-template"]);
        result.Should().Be(1);
    }

    [Fact]
    public async Task ShouldReturnOne_WhenConfigurationTemplatePathValueMissing()
    {
        var result = await CommandLineRunner.StartCommandLine(["--validate-configuration-template", "--path"]);
        result.Should().Be(1);
    }
}

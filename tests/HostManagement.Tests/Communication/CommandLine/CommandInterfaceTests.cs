using AwesomeAssertions;
using AwesomeAssertions.Equivalency;
using Xunit;
using HostManagement.Communication.CommandLine;
using HostManagement.Communication.CommandLine.Commands.Deployment;
using HostManagement.Communication.CommandLine.Commands.Network;
using HostManagement.Communication.CommandLine.Commands.Service;
using HostManagement.Communication.CommandLine.Commands.System;
using HostManagement.Communication.CommandLine.Commands.ViciOneSuite;

namespace HostManagement.Tests.Communication.CommandLine;

public class CommandInterfaceTests
{
    private static readonly Func<EquivalencyOptions<Type>, EquivalencyOptions<Type>> s_config =
        options => options.WithStrictOrdering();

    [Fact]
    public void CreateRootCommand_ShouldReturnExpectedRootCommand()
    {
        // Arrange
        var expectedDescription = "ViciOne Host Management Tool";
        var expectedSubcommands = new[]
        {
            typeof(NetworkCommand),
            typeof(ServiceCommand),
            typeof(ViciOneSuiteCommand),
            typeof(DeploymentCommand),
            typeof(SystemCommand)
        };

        // Act
        var rootCommand = CommandInterface.CreateRootCommand();

        // Assert
        rootCommand.Should().NotBeNull();
        rootCommand.Description.Should().Be(expectedDescription);
        rootCommand.Subcommands.Select(c => c.GetType()).Should().BeEquivalentTo(expectedSubcommands, s_config);
    }
}

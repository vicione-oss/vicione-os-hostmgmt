using System.CommandLine;
using AwesomeAssertions;
using HostManagement.Communication.CommandLine;
using Xunit;

namespace HostManagement.Tests.Communication.CommandLine;

[Collection("Non Parallel Tests")]
public sealed class CliHelpExtractorTests
{
    [Fact]
    public void ShouldOutputMarkdownForAllCommands_WhenCommandTreeContainsNestedCommands()
    {
        // Arrange
        var setCommand = new Command("set", "Sets interface settings.");
        var interfaceCommand = new Command("interface", "Manages interfaces.") { setCommand };
        var networkCommand = new Command("network", "Manages network settings.") { interfaceCommand };
        var serviceCommand = new Command("service", "Manages services.");
        var rootCommand = new RootCommand("Root command description.") { networkCommand, serviceCommand };

        // Act
        var output = ExtractHelp(rootCommand);

        // Assert
        output.Should()
            .Contain("# Host Management CLI Documentation")
            .And.Contain("## Commands and Options")
            .And.Contain("### Root Command")
            .And.Contain("### Network Commands")
            .And.Contain("#### network interface")
            .And.Contain("#### network interface set")
            .And.Contain("### Service Commands");
    }

    [Fact]
    public void ShouldOutputHelpForEveryCommand_WhenCommandTreeContainsNestedCommands()
    {
        // Arrange
        var childCommand = new Command("child", "Child command description.");
        var parentCommand = new Command("parent", "Parent command description.") { childCommand };
        var rootCommand = new RootCommand("Root command description.") { parentCommand };

        // Act
        var output = ExtractHelp(rootCommand);

        // Assert
        output.Should()
            .Contain("Root command description.")
            .And.Contain("Parent command description.")
            .And.Contain("Child command description.");

        CountOccurrences(output, "```text").Should().Be(3);
        CountOccurrences(output, "```").Should().Be(6);
    }

    [Fact]
    public void ShouldHideDefaultHelpOptionFromAllGeneratedHelp()
    {
        // Arrange
        var childOption = new Option<bool>("--child-option") { Description = "A child option." };
        var childCommand = new Command("child", "Child command description.") { childOption };

        var parentOption = new Option<bool>("--parent-option") { Description = "A parent option." };
        var parentCommand = new Command("parent", "Parent command description.") { parentOption, childCommand };

        var rootOption = new Option<bool>("--root-option") { Description = "A root option." };
        var rootCommand = new RootCommand("Root command description.") { rootOption, parentCommand };

        // Act
        var output = ExtractHelp(rootCommand);

        // Assert
        output.Should()
            .Contain("--root-option")
            .And.Contain("--parent-option")
            .And.Contain("--child-option")
            .And.NotContainAny("--help", "-h", "/h", "-?", "/?")
            .And.NotContain("Show help and usage information");
    }

    [Fact]
    public void ShouldNotOutputDefaultHelpOption_WhenCalled()
    {
        // Arrange
        var rootCommand = new RootCommand("Root command description.")
        {
            new Command("network", "Network command description.")
        };

        // Act
        var output = ExtractHelp(rootCommand);

        // Assert
        output.Should().NotContainAny("--help", "-h", "/h", "-?", "/?")
            .And.NotContain("Show help and usage information");
    }

    private static string ExtractHelp(RootCommand rootCommand)
    {
        using var writer = new StringWriter();
        var originalConsoleOutput = Console.Out;

        try
        {
            Console.SetOut(writer);
            CliHelpExtractor.SetOutput(writer);
            CliHelpExtractor.ExtractHelp(rootCommand);
            return writer.ToString();
        }
        finally
        {
            CliHelpExtractor.SetOutput(originalConsoleOutput);
            Console.SetOut(originalConsoleOutput);
        }
    }

    private static int CountOccurrences(string value, string search)
        => value.Split(search, StringSplitOptions.None).Length - 1;
}

using FluentAssertions;
using Xunit;
using HostManagement.Communication.CommandLine.Output;

namespace HostManagement.Tests.Communication.CommandLine.Output;

public class KeyValueOutputTests
{
    private const string Title = "KeyValueOutput";
    private const string TitleEnabledString = " (enabled):";
    private const string TitleDisabledString = " (disabled):";
    private const string EmptyKey = "'no key'";
    private const string EmptyValue = "'no value'";

    [Fact]
    public void ShouldHaveCorrectOutput_WhenCreatedWithoutTitleAndWithSingleItem()
    {
        // Arrange
        var expectedOutput =
            """
            Key: Value

            """;

        // Act
        var output = new KeyValueOutput()
            .With(new("Key", "Value"))
            .GetOutput();

        // Assert
        output.Should().Be(expectedOutput);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(true)]
    [InlineData(false)]
    public void ShouldHaveCorrectOutput_WhenCreatedWithTitleAndSingleItem(bool? titleEnabled)
    {
        // Arrange
        var expectedTitle = Title;

        if (titleEnabled is null)
            expectedTitle += ":";
        else if (titleEnabled is true)
            expectedTitle += TitleEnabledString;
        else
            expectedTitle += TitleDisabledString;

        var expectedOutput =
            $"""
            {expectedTitle} Key: Value

            """;

        // Act
        var output = new KeyValueOutput()
            .WithTitle(Title, titleEnabled)
            .With(new("Key", "Value"))
            .GetOutput();

        // Assert
        output.Should().Be(expectedOutput);
    }

    [Fact]
    public void ShouldHaveCorrectOutput_WhenCreatedWithoutTitleAndWithMultipleItems()
    {
        // Arrange
        var expectedOutput =
            """
            Key 1........: Value 1
            Another Key 2: Value 2
            Key 3........: Value 3

            """;

        // Act
        var output = new KeyValueOutput()
            .With(new("Key 1", "Value 1"))
            .With(new("Another Key 2", "Value 2"))
            .With(new("Key 3", "Value 3"))
            .GetOutput();

        // Assert
        output.Should().Be(expectedOutput);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(true)]
    [InlineData(false)]
    public void ShouldHaveCorrectOutput_WhenCreatedWithTitleAndMultipleItems(bool? titleEnabled)
    {
        // Arrange
        var expectedTitle = Title;

        if (titleEnabled is null)
            expectedTitle += ":";
        else if (titleEnabled is true)
            expectedTitle += TitleEnabledString;
        else
            expectedTitle += TitleDisabledString;

        var expectedOutput =
            $"""
            {expectedTitle}
              Key 1........: Value 1
              Another Key 2: Value 2
              Key 3........: Value 3

            """;

        // Act
        var output = new KeyValueOutput()
            .WithTitle(Title, titleEnabled)
            .With(new("Key 1", "Value 1"))
            .With(new("Another Key 2", "Value 2"))
            .With(new("Key 3", "Value 3"))
            .GetOutput();

        // Assert
        output.Should().Be(expectedOutput);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void ShouldUseConfiguredEmptyKey_WhenKeyEmpty(string key)
    {
        // Arrange
        var expectedOutput =
            $"""
            {EmptyKey}: Value

            """;

        // Act
        var output = new KeyValueOutput()
            .With(new(key, "Value"))
            .GetOutput();

        // Assert
        output.Should().Be(expectedOutput);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void ShouldUseConfiguredEmptyValue_WhenValueEmpty(string value)
    {
        // Arrange
        var expectedOutput =
            $"""
            Key: {EmptyValue}

            """;

        // Act
        var output = new KeyValueOutput()
            .With(new("Key", value))
            .GetOutput();

        // Assert
        output.Should().Be(expectedOutput);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void ShouldUseConfiguredEmptyKeyAndValue_WhenKeyAndValueEmpty(string keyOrValue)
    {
        // Arrange
        var expectedOutput =
            $"""
            {EmptyKey}: {EmptyValue}

            """;

        // Act
        var output = new KeyValueOutput()
            .With(new(keyOrValue, keyOrValue))
            .GetOutput();

        // Assert
        output.Should().Be(expectedOutput);
    }
}

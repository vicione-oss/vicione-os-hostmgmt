using FluentAssertions;
using Xunit;
using HostManagement.Communication.CommandLine.Output;

namespace HostManagement.Tests.Communication.CommandLine.Output;

public class MultiKeyValueOutputTests
{
    private const string Title = "MultiKeyValueOutput";
    private const string TitleEnabledString = " (enabled):";
    private const string TitleDisabledString = " (disabled):";
    private const string EmptyKey = "'no key'";
    private const string EmptyValue = "'no value'";

    [Fact]
    public void ShouldThrow_WhenTooManyAdditionalKeyValuePairsSpecified()
    {
        // Arrange
        CliOutputKeyValuePair[] tooManyAdditionalKeyValuePairs =
        [
            new("Key C1", "Value C1"),
            new("Key D1", "Value D1"),
            new("Key E1", "Value E1"),
            new("Key F1", "Value F1"),
            new("Key G1", "Value G1"),
            new("Key H1", "Value H1"),
            new("too much", "too much")
        ];

        // Act
        var result = () => new MultiKeyValueOutput()
            .With(new("Key A1", "Value A1"), new("Key B1", "Value B1"), tooManyAdditionalKeyValuePairs)
            .GetOutput();

        // Assert
        result.Should()
            .Throw<ArgumentOutOfRangeException>()
            .WithParameterName("additionalKeyValuePairs")
            .WithMessage("Only up to 6 additional key-value pairs are allowed.*");
    }

    [Fact]
    public void ShouldHaveCorrectOutput_WhenCreatedWithoutTitleAndWithSingleItem()
    {
        // Arrange
        var expectedOutput =
            """
            Key A: Value A  Key B: Value B  Key C: Value C

            """
            .UseEnvironmentNewLine();

        // Act
        var output = new MultiKeyValueOutput()
            .With(new("Key A", "Value A"), new("Key B", "Value B"), [new("Key C", "Value C")])
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
            {expectedTitle} Key A: Value A  Key B: Value B  Key C: Value C

            """
            .UseEnvironmentNewLine();

        // Act
        var output = new MultiKeyValueOutput()
            .WithTitle(Title, titleEnabled)
            .With(new("Key A", "Value A"), new("Key B", "Value B"), [new("Key C", "Value C")])
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
            Key A1........: Value A1  Another Key B1: Value B1  Key C1........: Value C1
            Another Key A2: Value A2  Key B2........: Value B2  Key C2........: Value C2
            Key A3........: Value A3  Key B3........: Value B3  Another Key C3: Value C3

            """
            .UseEnvironmentNewLine();

        // Act
        var output = new MultiKeyValueOutput()
            .With(new("Key A1", "Value A1"), new("Another Key B1", "Value B1"), [new("Key C1", "Value C1")])
            .With(new("Another Key A2", "Value A2"), new("Key B2", "Value B2"), [new("Key C2", "Value C2")])
            .With(new("Key A3", "Value A3"), new("Key B3", "Value B3"), [new("Another Key C3", "Value C3")])
            .GetOutput();

        // Assert
        output.Should().Be(expectedOutput);
    }

    [Fact]
    public void ShouldHaveCorrectOutput_WhenCreatedWithoutTitleAndWithMultipleItemsWithDifferentLength()
    {
        // Arrange
        var expectedOutput =
            """
            Key A1........: Value A1  Another Key B1: Value B1
            Another Key A2: Value A2  Key B2........: Value B2  Key C2: Value C2
            Key A3........: Value A3

            """
            .UseEnvironmentNewLine();

        // Act
        var output = new MultiKeyValueOutput()
            .With(new("Key A1", "Value A1"), new("Another Key B1", "Value B1"))
            .With(new("Another Key A2", "Value A2"), new("Key B2", "Value B2"), [new("Key C2", "Value C2")])
            .With(new("Key A3", "Value A3"))
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
              Key A1........: Value A1  Another Key B1: Value B1  Key C1........: Value C1
              Another Key A2: Value A2  Key B2........: Value B2  Key C2........: Value C2
              Key A3........: Value A3  Key B3........: Value B3  Another Key C3: Value C3

            """
            .UseEnvironmentNewLine();

        // Act
        var output = new MultiKeyValueOutput()
            .WithTitle(Title, titleEnabled)
            .With(new("Key A1", "Value A1"), new("Another Key B1", "Value B1"), [new("Key C1", "Value C1")])
            .With(new("Another Key A2", "Value A2"), new("Key B2", "Value B2"), [new("Key C2", "Value C2")])
            .With(new("Key A3", "Value A3"), new("Key B3", "Value B3"), [new("Another Key C3", "Value C3")])
            .GetOutput();

        // Assert
        output.Should().Be(expectedOutput);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(true)]
    [InlineData(false)]
    public void ShouldHaveCorrectOutput_WhenCreatedWithTitleAndMultipleItemsWithDifferentLength(bool? titleEnabled)
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
              Key A1........: Value A1  Another Key B1: Value B1
              Another Key A2: Value A2  Key B2........: Value B2  Key C2: Value C2
              Key A3........: Value A3

            """
            .UseEnvironmentNewLine();

        // Act
        var output = new MultiKeyValueOutput()
            .WithTitle(Title, titleEnabled)
            .With(new("Key A1", "Value A1"), new("Another Key B1", "Value B1"))
            .With(new("Another Key A2", "Value A2"), new("Key B2", "Value B2"), [new("Key C2", "Value C2")])
            .With(new("Key A3", "Value A3"))
            .GetOutput();

        // Assert
        output.Should().Be(expectedOutput);
    }

    [Fact]
    public void ShouldHaveCorrectOutput_WhenCreatedWithTwoKeyValuePairs()
    {
        // Arrange
        var expectedOutput =
            """
            Key A1........: Value A1  Another Key B1: Value B1
            Another Key A2: Value A2  Key B2........: Value B2

            """
            .UseEnvironmentNewLine();

        // Act
        var output = new MultiKeyValueOutput()
            .With(new("Key A1", "Value A1"), new("Another Key B1", "Value B1"))
            .With(new("Another Key A2", "Value A2"), new("Key B2", "Value B2"))
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
        var expectedOutputWith2KeyValuePairs =
            $"""
            {EmptyKey}: Value A  Key B: Value B

            """
            .UseEnvironmentNewLine();

        var expectedOutputWith3KeyValuePairs =
            $"""
            {EmptyKey}: Value A  Key B: Value B  Key C: Value C

            """
            .UseEnvironmentNewLine();

        // Act
        var outputWith2KeyValuePairs = new MultiKeyValueOutput()
            .With(new(key, "Value A"), new("Key B", "Value B"))
            .GetOutput();

        var outputWith3KeyValuePairs = new MultiKeyValueOutput()
            .With(new(key, "Value A"), new("Key B", "Value B"), [new("Key C", "Value C")])
            .GetOutput();

        // Assert
        outputWith2KeyValuePairs.Should().Be(expectedOutputWith2KeyValuePairs);
        outputWith3KeyValuePairs.Should().Be(expectedOutputWith3KeyValuePairs);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void ShouldUseConfiguredEmptyValue_WhenValueEmpty(string value)
    {
        // Arrange
        var expectedOutputWith2KeyValuePairs =
            $"""
            Key A: {EmptyValue}  Key B: Value B

            """
            .UseEnvironmentNewLine();

        var expectedOutputWith3KeyValuePairs =
            $"""
            Key A: {EmptyValue}  Key B: Value B  Key C: Value C

            """
            .UseEnvironmentNewLine();

        // Act
        var outputWith2KeyValuePairs = new MultiKeyValueOutput()
            .With(new("Key A", value), new("Key B", "Value B"))
            .GetOutput();

        var outputWith3KeyValuePairs = new MultiKeyValueOutput()
            .With(new("Key A", value), new("Key B", "Value B"), [new("Key C", "Value C")])
            .GetOutput();

        // Assert
        outputWith2KeyValuePairs.Should().Be(expectedOutputWith2KeyValuePairs);
        outputWith3KeyValuePairs.Should().Be(expectedOutputWith3KeyValuePairs);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void ShouldUseConfiguredEmptyKeyAndValue_WhenKeyAndValueEmpty(string keyOrValue)
    {
        // Arrange
        var expectedOutputWith2KeyValuePairs =
            $"""
            {EmptyKey}: {EmptyValue}  Key B: Value B

            """
            .UseEnvironmentNewLine();

        var expectedOutputWith3KeyValuePairs =
            $"""
            {EmptyKey}: {EmptyValue}  Key B: Value B  Key C: Value C

            """
            .UseEnvironmentNewLine();

        // Act
        var outputWith2KeyValuePairs = new MultiKeyValueOutput()
            .With(new(keyOrValue, keyOrValue), new("Key B", "Value B"))
            .GetOutput();

        var outputWith3KeyValuePairs = new MultiKeyValueOutput()
            .With(new(keyOrValue, keyOrValue), new("Key B", "Value B"), [new("Key C", "Value C")])
            .GetOutput();

        // Assert
        outputWith2KeyValuePairs.Should().Be(expectedOutputWith2KeyValuePairs);
        outputWith3KeyValuePairs.Should().Be(expectedOutputWith3KeyValuePairs);
    }
}

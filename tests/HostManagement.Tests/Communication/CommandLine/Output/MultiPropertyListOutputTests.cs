using FluentAssertions;
using Xunit;
using HostManagement.Communication.CommandLine.Output;

namespace HostManagement.Tests.Communication.CommandLine.Output;

public class MultiPropertyListOutputTests
{
    private const string Title = "MultiPropertyListOutput";
    private const string TitleEnabledString = " (enabled):";
    private const string TitleDisabledString = " (disabled):";
    private const string EmptyKey = "'no key'";
    private const string EmptyValue = "'no value'";

    [Fact]
    public void ShouldThrow_WhenTooManyAdditionalPropertiesSpecified()
    {
        // Arrange
        CliOutputKeyValuePair[] tooManyAdditionalProperties =
        [
            new("Key C1", "Value C1"),
            new("Key D1", "Value D1"),
            new("Key E1", "Value E1"),
            new("Key F1", "Value F1"),
            new("Key G1", "Value G1"),
            new("too much", "too much")
        ];

        // Act
        var result = () => new MultiPropertyListOutput()
            .With("Item 1", new("Key A1", "Value A1"), new("Key B1", "Value B1"), tooManyAdditionalProperties)
            .GetOutput();

        // Assert
        result.Should()
            .Throw<ArgumentOutOfRangeException>()
            .WithParameterName("additionalProperties")
            .WithMessage("Only up to 5 additional key-value formatted properties are allowed.*");
    }

    [Fact]
    public void ShouldHaveCorrectOutput_WhenCreatedWithoutTitleAndWithSingleItem()
    {
        // Arrange
        var expectedOutput =
            """
            Item  -> Key A: Value A  Key B: Value B  Key C: Value C

            """
            .UseEnvironmentNewLine();

        // Act
        var output = new MultiPropertyListOutput()
            .With("Item", new("Key A", "Value A"), new("Key B", "Value B"), [new("Key C", "Value C")])
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
            {expectedTitle} Item  -> Key A: Value A  Key B: Value B  Key C: Value C

            """
            .UseEnvironmentNewLine();

        // Act
        var output = new MultiPropertyListOutput()
            .WithTitle(Title, titleEnabled)
            .With("Item", new("Key A", "Value A"), new("Key B", "Value B"), [new("Key C", "Value C")])
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
            Item 1          -> Key A1........: Value A1  Another Key B1: Value B1  Key C1........: Value C1
            Item 2          -> Another Key A2: Value A2  Key B2........: Value B2  Key C2........: Value C2
            Another Item 3  -> Key A3........: Value A3  Key B3........: Value B3  Another Key C3: Value C3

            """
            .UseEnvironmentNewLine();

        // Act
        var output = new MultiPropertyListOutput()
            .With("Item 1", new("Key A1", "Value A1"), new("Another Key B1", "Value B1"), [new("Key C1", "Value C1")])
            .With("Item 2", new("Another Key A2", "Value A2"), new("Key B2", "Value B2"), [new("Key C2", "Value C2")])
            .With("Another Item 3", new("Key A3", "Value A3"), new("Key B3", "Value B3"), [new("Another Key C3", "Value C3")])
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
              Item 1          -> Key A1........: Value A1  Another Key B1: Value B1  Key C1........: Value C1
              Item 2          -> Another Key A2: Value A2  Key B2........: Value B2  Key C2........: Value C2
              Another Item 3  -> Key A3........: Value A3  Key B3........: Value B3  Another Key C3: Value C3

            """
            .UseEnvironmentNewLine();

        // Act
        var output = new MultiPropertyListOutput()
            .WithTitle(Title, titleEnabled)
            .With("Item 1", new("Key A1", "Value A1"), new("Another Key B1", "Value B1"), [new("Key C1", "Value C1")])
            .With("Item 2", new("Another Key A2", "Value A2"), new("Key B2", "Value B2"), [new("Key C2", "Value C2")])
            .With("Another Item 3", new("Key A3", "Value A3"), new("Key B3", "Value B3"), [new("Another Key C3", "Value C3")])
            .GetOutput();

        // Assert
        output.Should().Be(expectedOutput);
    }

    [Fact]
    public void ShouldHaveCorrectOutput_WhenCreatedWithTwoProperties()
    {
        // Arrange
        var expectedOutput =
            """
            Item 1          -> Key A1........: Value A1  Another Key B1: Value B1
            Another Item 2  -> Another Key A2: Value A2  Key B2........: Value B2

            """
            .UseEnvironmentNewLine();

        // Act
        var output = new MultiPropertyListOutput()
            .With("Item 1", new("Key A1", "Value A1"), new("Another Key B1", "Value B1"))
            .With("Another Item 2", new("Another Key A2", "Value A2"), new("Key B2", "Value B2"))
            .GetOutput();

        // Assert
        output.Should().Be(expectedOutput);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void ShouldUseConfiguredEmptyValue_WhenItemValueEmpty(string itemValue)
    {
        // Arrange
        var expectedOutputWith2Properties =
            $"""
            {EmptyValue}  -> Key A: Value A  Key B: Value B

            """
            .UseEnvironmentNewLine();

        var expectedOutputWith3Properties =
            $"""
            {EmptyValue}  -> Key A: Value A  Key B: Value B  Key C: Value C

            """
            .UseEnvironmentNewLine();

        // Act
        var outputWith2Properties = new MultiPropertyListOutput()
            .With(itemValue, new("Key A", "Value A"), new("Key B", "Value B"))
            .GetOutput();

        var outputWith3Properties = new MultiPropertyListOutput()
            .With(itemValue, new("Key A", "Value A"), new("Key B", "Value B"), [new("Key C", "Value C")])
            .GetOutput();

        // Assert
        outputWith2Properties.Should().Be(expectedOutputWith2Properties);
        outputWith3Properties.Should().Be(expectedOutputWith3Properties);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void ShouldUseConfiguredEmptyValue_WhenKeyEmpty(string key)
    {
        // Arrange
        var expectedOutputWith2Properties =
            $"""
            Item A  -> {EmptyKey}: Value A  Key B: Value B

            """
            .UseEnvironmentNewLine();

        var expectedOutputWith3Properties =
            $"""
            Item A  -> {EmptyKey}: Value A  Key B: Value B  Key C: Value C

            """
            .UseEnvironmentNewLine();

        // Act
        var outputWith2Properties = new MultiPropertyListOutput()
            .With("Item A", new(key, "Value A"), new("Key B", "Value B"))
            .GetOutput();

        var outputWith3Properties = new MultiPropertyListOutput()
            .With("Item A", new(key, "Value A"), new("Key B", "Value B"), [new("Key C", "Value C")])
            .GetOutput();

        // Assert
        outputWith2Properties.Should().Be(expectedOutputWith2Properties);
        outputWith3Properties.Should().Be(expectedOutputWith3Properties);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void ShouldUseConfiguredEmptyValue_WhenValueEmpty(string value)
    {
        // Arrange
        var expectedOutputWith2Properties =
            $"""
            Item A  -> Key A: {EmptyValue}  Key B: Value B

            """
            .UseEnvironmentNewLine();

        var expectedOutputWith3Properties =
            $"""
            Item A  -> Key A: {EmptyValue}  Key B: Value B  Key C: Value C

            """
            .UseEnvironmentNewLine();

        // Act
        var outputWith2Properties = new MultiPropertyListOutput()
            .With("Item A", new("Key A", value), new("Key B", "Value B"))
            .GetOutput();

        var outputWith3Properties = new MultiPropertyListOutput()
            .With("Item A", new("Key A", value), new("Key B", "Value B"), [new("Key C", "Value C")])
            .GetOutput();

        // Assert
        outputWith2Properties.Should().Be(expectedOutputWith2Properties);
        outputWith3Properties.Should().Be(expectedOutputWith3Properties);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void ShouldUseConfiguredEmptyValue_WhenItemValueAndKeyAndValueEmpty(string itemOrKeyOrValue)
    {
        // Arrange
        var expectedOutputWith2Properties =
            $"""
            {EmptyValue}  -> {EmptyKey}: {EmptyValue}  Key B: Value B

            """
            .UseEnvironmentNewLine();

        var expectedOutputWith3Properties =
            $"""
            {EmptyValue}  -> {EmptyKey}: {EmptyValue}  Key B: Value B  Key C: Value C

            """
            .UseEnvironmentNewLine();

        // Act
        var outputWith2Properties = new MultiPropertyListOutput()
            .With(itemOrKeyOrValue, new(itemOrKeyOrValue, itemOrKeyOrValue), new("Key B", "Value B"))
            .GetOutput();

        var outputWith3Properties = new MultiPropertyListOutput()
            .With(itemOrKeyOrValue, new(itemOrKeyOrValue, itemOrKeyOrValue), new("Key B", "Value B"), [new("Key C", "Value C")])
            .GetOutput();

        // Assert
        outputWith2Properties.Should().Be(expectedOutputWith2Properties);
        outputWith3Properties.Should().Be(expectedOutputWith3Properties);
    }
}

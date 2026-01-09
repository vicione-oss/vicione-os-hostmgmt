using AwesomeAssertions;
using Xunit;
using HostManagement.Communication.CommandLine.Output;

namespace HostManagement.Tests.Communication.CommandLine.Output;

public class TableOutputTests
{
    private const string Title = "TableOutput";
    private const string TitleEnabledString = " (enabled):";
    private const string TitleDisabledString = " (disabled):";
    private const string TableCellEmptyValue = "-";

    [Fact]
    public void ShouldThrow_WhenTooManyAdditionalValuesSpecified()
    {
        // Arrange
        CliOutputTableValue[] tooManyAdditionalValues =
            ["C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "too much"];

        // Act
        var result = () => new TableOutput()
            .With("A", "B", tooManyAdditionalValues)
            .GetOutput();

        // Assert
        result.Should()
            .Throw<ArgumentOutOfRangeException>()
            .WithParameterName("additionalValues")
            .WithMessage("Only up to 14 additional values are allowed.*");
    }

    [Fact]
    public void ShouldHaveCorrectOutput_WhenCreatedWithoutTitleAndWithSingleItem()
    {
        // Arrange
        var expectedOutput =
            """
            Cell A  Cell B  Cell C

            """
            .UseEnvironmentNewLine();

        // Act
        var output = new TableOutput()
            .With("Cell A", "Cell B", "Cell C")
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

        var expectedItem = "Cell A  Cell B  Cell C";

        var expectedOutput =
            $"""
            {expectedTitle} {expectedItem}

            """
            .UseEnvironmentNewLine();

        // Act
        var output = new TableOutput()
            .WithTitle(Title, titleEnabled)
            .With("Cell A", "Cell B", "Cell C")
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
            Cell A1          Cell B1          Another Cell C1
            Cell A2          Another Cell B2  Cell C2
            Another Cell A3  Cell B3          Cell C3

            """
            .UseEnvironmentNewLine();

        // Act
        var output = new TableOutput()
            .With("Cell A1", "Cell B1", "Another Cell C1")
            .With("Cell A2", "Another Cell B2", "Cell C2")
            .With("Another Cell A3", "Cell B3", "Cell C3")
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
              Cell A1          Cell B1          Another Cell C1
              Cell A2          Another Cell B2  Cell C2
              Another Cell A3  Cell B3          Cell C3

            """
            .UseEnvironmentNewLine();

        // Act
        var output = new TableOutput()
            .WithTitle(Title, titleEnabled)
            .With("Cell A1", "Cell B1", "Another Cell C1")
            .With("Cell A2", "Another Cell B2", "Cell C2")
            .With("Another Cell A3", "Cell B3", "Cell C3")
            .GetOutput();

        // Assert
        output.Should().Be(expectedOutput);
    }

    [Fact]
    public void ShouldHaveCorrectOutput_WhenCreatedWithTwoValues()
    {
        // Arrange
        var expectedOutput =
            """
            Another Cell A1  Cell B1
            Cell A2          Another Cell B2

            """
            .UseEnvironmentNewLine();

        // Act
        var output = new TableOutput()
            .With("Another Cell A1", "Cell B1")
            .With("Cell A2", "Another Cell B2")
            .GetOutput();

        // Assert
        output.Should().Be(expectedOutput);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void ShouldUseConfiguredEmptyCellValue_WhenValueEmpty(string value)
    {
        var expectedOutput =
            $"""
            Cell A1  {TableCellEmptyValue}  Cell C1  {TableCellEmptyValue}

            """
            .UseEnvironmentNewLine();

        // Act
        var output = new TableOutput()
            .With("Cell A1", value, "Cell C1", value)
            .GetOutput();

        // Assert
        output.Should().Be(expectedOutput);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void ShouldUseConfiguredEmptyCellValue_WhenAllValuesEmpty(string value)
    {
        var expectedOutput =
            $"""
            {TableCellEmptyValue}  {TableCellEmptyValue}

            """
            .UseEnvironmentNewLine();

        // Act
        var output = new TableOutput()
            .With(value, value)
            .GetOutput();

        // Assert
        output.Should().Be(expectedOutput);
    }
}

using FluentAssertions;
using Xunit;
using HostManagement.Communication.CommandLine.Output;

namespace HostManagement.Tests.Communication.CommandLine.Output;

public class PropertyListOutputTests
{
    private const string Title = "PropertyListOutput";
    private const string TitleEnabledString = " (enabled):";
    private const string TitleDisabledString = " (disabled):";
    private const string ValueEnabledString = " (enabled)";
    private const string ValueDisabledString = " (disabled)";
    private const string EmptyKey = "'no key'";
    private const string EmptyValue = "'no value'";

    [Theory]
    [InlineData(null)]
    [InlineData(true)]
    [InlineData(false)]
    public void ShouldHaveCorrectOutput_WhenCreatedWithoutTitleAndWithSingleItem(bool? valueEnabled)
    {
        // Arrange
        var expectedItem = "Item  -> Key: Value";

        if (valueEnabled is true)
            expectedItem += ValueEnabledString;
        else if (valueEnabled is false)
            expectedItem += ValueDisabledString;

        var expectedOutput =
            $"""
            {expectedItem}

            """;

        // Act
        var output = new PropertyListOutput()
            .With("Item", new("Key", "Value"), valueEnabled)
            .GetOutput();

        // Assert
        output.Should().Be(expectedOutput);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData(true, null)]
    [InlineData(false, null)]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public void ShouldHaveCorrectOutput_WhenCreatedWithTitleAndSingleItem(
        bool? titleEnabled,
        bool? valueEnabled)
    {
        // Arrange
        var expectedTitle = Title;

        if (titleEnabled is null)
            expectedTitle += ":";
        else if (titleEnabled is true)
            expectedTitle += TitleEnabledString;
        else
            expectedTitle += TitleDisabledString;

        var expectedItem = "Item  -> Key: Value";

        if (valueEnabled is true)
            expectedItem += ValueEnabledString;
        else if (valueEnabled is false)
            expectedItem += ValueDisabledString;

        var expectedOutput =
            $"""
            {expectedTitle} {expectedItem}

            """;

        // Act
        var output = new PropertyListOutput()
            .WithTitle(Title, titleEnabled)
            .With("Item", new("Key", "Value"), valueEnabled)
            .GetOutput();

        // Assert
        output.Should().Be(expectedOutput);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(true)]
    [InlineData(false)]
    public void ShouldHaveCorrectOutput_WhenCreatedWithoutTitleAndWithMultipleItems(bool? valueEnabled)
    {
        // Arrange
        var expectedItem = "Another Item 2  -> Another Key 2: Another Value 2";

        if (valueEnabled is true)
            expectedItem += ValueEnabledString;
        else if (valueEnabled is false)
            expectedItem += ValueDisabledString;

        var expectedOutput =
            $"""
            Item 1          -> Key 1........: Value 1
            {expectedItem}
            Item 3          -> Key 3........: Value 3

            """;

        // Act
        var output = new PropertyListOutput()
            .With("Item 1", new("Key 1", "Value 1"))
            .With("Another Item 2", new("Another Key 2", "Another Value 2"), valueEnabled)
            .With("Item 3", new("Key 3", "Value 3"))
            .GetOutput();

        // Assert
        output.Should().Be(expectedOutput);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData(true, null)]
    [InlineData(false, null)]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public void ShouldHaveCorrectOutput_WhenCreatedWithTitleAndMultipleItems(
        bool? titleEnabled,
        bool? valueEnabled)
    {
        // Arrange
        var expectedTitle = Title;

        if (titleEnabled is null)
            expectedTitle += ":";
        else if (titleEnabled is true)
            expectedTitle += TitleEnabledString;
        else
            expectedTitle += TitleDisabledString;

        var expectedItem = "Another Item 2  -> Another Key 2: Another Value 2";

        if (valueEnabled is true)
            expectedItem += ValueEnabledString;
        else if (valueEnabled is false)
            expectedItem += ValueDisabledString;

        var expectedOutput =
            $"""
            {expectedTitle}
              Item 1          -> Key 1........: Value 1
              {expectedItem}
              Item 3          -> Key 3........: Value 3

            """;

        // Act
        var output = new PropertyListOutput()
            .WithTitle(Title, titleEnabled)
            .With("Item 1", new("Key 1", "Value 1"))
            .With("Another Item 2", new("Another Key 2", "Another Value 2"), valueEnabled)
            .With("Item 3", new("Key 3", "Value 3"))
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
        var expectedOutput =
            $"""
            {EmptyValue}  -> Key: Value

            """;

        // Act
        var output = new PropertyListOutput()
            .With(itemValue, new("Key", "Value"))
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
            Item  -> {EmptyKey}: Value

            """;

        // Act
        var output = new PropertyListOutput()
            .With("Item", new(key, "Value"))
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
            Item  -> Key: {EmptyValue}

            """;

        // Act
        var output = new PropertyListOutput()
            .With("Item", new("Key", value))
            .GetOutput();

        // Assert
        output.Should().Be(expectedOutput);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void ShouldUseConfiguredEmptyValue_WhenItemValueAndKeyAndValueEmpty(
        string itemOrKeyOrValue)
    {
        // Arrange
        var expectedOutput =
            $"""
            {EmptyValue}  -> {EmptyKey}: {EmptyValue}

            """;

        // Act
        var output = new PropertyListOutput()
            .With(itemOrKeyOrValue, new(itemOrKeyOrValue, itemOrKeyOrValue))
            .GetOutput();

        // Assert
        output.Should().Be(expectedOutput);
    }
}

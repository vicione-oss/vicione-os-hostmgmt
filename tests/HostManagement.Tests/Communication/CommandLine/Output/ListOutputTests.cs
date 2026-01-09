using FluentAssertions;
using Xunit;
using HostManagement.Communication.CommandLine.Output;

namespace HostManagement.Tests.Communication.CommandLine.Output;

public class ListOutputTests
{
    private const string Title = "ListOutput";
    private const string TitleEnabledString = " (enabled):";
    private const string TitleDisabledString = " (disabled):";
    private const string ItemEnabledString = " (enabled)";
    private const string ItemDisabledString = " (disabled)";
    private const string EmptyValue = "'no value'";

    public class WithSingleItem : ListOutputTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldHaveCorrectOutput_WhenCreatedWithoutTitle(bool? itemEnabled)
        {
            // Arrange
            var expectedItem = "Item";

            if (itemEnabled is true)
                expectedItem += ItemEnabledString;
            else if (itemEnabled is false)
                expectedItem += ItemDisabledString;

            var expectedOutput =
                $"""
            {expectedItem}

            """;

            // Act
            var output = new ListOutput()
                .With("Item", itemEnabled)
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
        public void ShouldHaveCorrectOutput_WhenCreatedWithTitle(
            bool? titleEnabled,
            bool? itemEnabled)
        {
            // Arrange
            var expectedTitle = Title;

            if (titleEnabled is null)
                expectedTitle += ":";
            else if (titleEnabled is true)
                expectedTitle += TitleEnabledString;
            else
                expectedTitle += TitleDisabledString;

            var expectedItem = "Item";

            if (itemEnabled is true)
                expectedItem += ItemEnabledString;
            else if (itemEnabled is false)
                expectedItem += ItemDisabledString;

            var expectedOutput =
                $"""
            {expectedTitle} {expectedItem}

            """;

            // Act
            var output = new ListOutput()
                .WithTitle(Title, titleEnabled)
                .With("Item", itemEnabled)
                .GetOutput();

            // Assert
            output.Should().Be(expectedOutput);
        }
    }

    public class WithMultipleItems : ListOutputTests
    {
        public class With
        {
            [Theory]
            [InlineData(null)]
            [InlineData(true)]
            [InlineData(false)]
            public void ShouldHaveCorrectOutput_WhenCreatedWithoutTitle(bool? itemEnabled)
            {
                // Arrange
                var expectedItem = "Item 2";

                if (itemEnabled is true)
                    expectedItem += ItemEnabledString;
                else if (itemEnabled is false)
                    expectedItem += ItemDisabledString;

                var expectedOutput =
                    $"""
                    Item 1
                    {expectedItem}
                    Item 3

                    """;

                // Act
                var output = new ListOutput()
                    .With("Item 1")
                    .With("Item 2", itemEnabled)
                    .With("Item 3")
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
            public void ShouldHaveCorrectOutput_WhenCreatedWithTitle(
                bool? titleEnabled,
                bool? itemEnabled)
            {
                // Arrange
                var expectedTitle = Title;

                if (titleEnabled is null)
                    expectedTitle += ":";
                else if (titleEnabled is true)
                    expectedTitle += TitleEnabledString;
                else
                    expectedTitle += TitleDisabledString;

                var expectedItem = "Item 2";

                if (itemEnabled is true)
                    expectedItem += ItemEnabledString;
                else if (itemEnabled is false)
                    expectedItem += ItemDisabledString;

                var expectedOutput =
                    $"""
                    {expectedTitle}
                      Item 1
                      {expectedItem}
                      Item 3

                    """;

                // Act
                var output = new ListOutput()
                    .WithTitle(Title, titleEnabled)
                    .With("Item 1")
                    .With("Item 2", itemEnabled)
                    .With("Item 3")
                    .GetOutput();

                // Assert
                output.Should().Be(expectedOutput);
            }
        }

        public class WithEach
        {
            [Fact]
            public void ShouldHaveCorrectOutput_WhenCreatedWithoutTitle()
            {
                // Arrange
                var items = new List<string>() { "Item 1", "Item 2", "Item 3" };

                var expectedOutput =
                    $"""
                    Item 1
                    Item 2
                    Item 3

                    """;

                // Act
                var output = new ListOutput()
                    .WithEach(items.Select(CliOutputValue.Create))
                    .GetOutput();

                // Assert
                output.Should().Be(expectedOutput);
            }

            [Theory]
            [InlineData(null)]
            [InlineData(true)]
            [InlineData(false)]
            public void ShouldHaveCorrectOutput_WhenCreatedWithTitle(bool? titleEnabled)
            {
                // Arrange
                var expectedTitle = Title;

                if (titleEnabled is null)
                    expectedTitle += ":";
                else if (titleEnabled is true)
                    expectedTitle += TitleEnabledString;
                else
                    expectedTitle += TitleDisabledString;

                var items = new List<string>() { "Item 1", "Item 2", "Item 3" };

                var expectedOutput =
                    $"""
                    {expectedTitle}
                      Item 1
                      Item 2
                      Item 3

                    """;

                // Act
                var output = new ListOutput()
                    .WithTitle(Title, titleEnabled)
                    .WithEach(items.Select(CliOutputValue.Create))
                    .GetOutput();

                // Assert
                output.Should().Be(expectedOutput);
            }
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void ShouldUseConfiguredEmptyValue_WhenValueEmpty(string value)
    {
        // Arrange
        var expectedOutput =
            $"""
            {EmptyValue}

            """;

        // Act
        var output = new ListOutput()
            .With(value)
            .GetOutput();

        // Assert
        output.Should().Contain(expectedOutput);
    }
}

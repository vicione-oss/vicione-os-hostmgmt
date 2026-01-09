using System.Text;
using FluentAssertions;
using Xunit;
using HostManagement.Communication.CommandLine.Helper;

namespace HostManagement.Tests.Communication.CommandLine.Helper;

public class CommandLineStringBuilderTests
{
    public class AppendLine
    {
        [Fact]
        public void ShouldThrow_WhenIndentationLevelNegative()
        {
            // Arrange
            var sut = new CommandLineStringBuilder();

            // Act
            var result = () => sut.AppendLine(-1);

            // Assert
            result.Should()
                .Throw<ArgumentOutOfRangeException>()
                .WithMessage("Indentation can not be negative.*");
        }

        [Fact]
        public void ShouldAppendEmptyLine_WhenNoIndentationAndValueGiven()
        {
            // Arrange
            var expectedOutput = new StringBuilder().AppendLine().ToString();

            var sut = new CommandLineStringBuilder();

            // Act
            sut.AppendLine();

            // Assert
            sut.Output.Should().BeEquivalentTo(expectedOutput);
            sut.ToString().Should().BeEquivalentTo(expectedOutput);
        }

        [Fact]
        public void ShouldAppendEmptyLine_WhenIndentationIsZeroAndNoValueGiven()
        {
            // Arrange
            var expectedOutput = new StringBuilder().AppendLine().ToString();

            var sut = new CommandLineStringBuilder();

            // Act
            sut.AppendLine(0);

            // Assert
            sut.Output.Should().BeEquivalentTo(expectedOutput);
            sut.ToString().Should().BeEquivalentTo(expectedOutput);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(5)]
        public void ShouldAppendLine_WhenIndentationIsPositiveAndNoValueGiven(int indentationLevel)
        {
            // Arrange
            var indentationString = "  ";
            var expectedValue = string.Concat(Enumerable.Repeat(indentationString, indentationLevel));
            var expectedOutput = new StringBuilder().AppendLine(expectedValue).ToString();

            var sut = new CommandLineStringBuilder(indentationString);

            // Act
            sut.AppendLine(indentationLevel);

            // Assert
            sut.Output.Should().BeEquivalentTo(expectedOutput);
            sut.ToString().Should().BeEquivalentTo(expectedOutput);
        }
    }

    public class Append
    {
        [Fact]
        public void ShouldThrow_WhenIndentationLevelNegative()
        {
            // Arrange
            var sut = new CommandLineStringBuilder();

            // Act
            var result = () => sut.Append(-1, string.Empty);

            // Assert
            result.Should()
                .Throw<ArgumentOutOfRangeException>()
                .WithMessage("Indentation can not be negative.*");
        }

        [Fact]
        public void ShouldAppend_WhenNoIndentationGiven()
        {
            // Arrange
            var value = "Value";
            var expectedOutput = new StringBuilder().Append(value).ToString();
            var sut = new CommandLineStringBuilder();

            // Act
            sut.Append(value);

            // Assert
            sut.Output.Should().BeEquivalentTo(expectedOutput);
            sut.ToString().Should().BeEquivalentTo(expectedOutput);
        }

        [Fact]
        public void ShouldAppend_WhenIndentationIsZero()
        {
            // Arrange
            var value = "Value";
            var expectedOutput = new StringBuilder().Append(value).ToString();
            var sut = new CommandLineStringBuilder();

            // Act
            sut.Append(0, value);

            // Assert
            sut.Output.Should().BeEquivalentTo(expectedOutput);
            sut.ToString().Should().BeEquivalentTo(expectedOutput);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(5)]
        public void ShouldAppend_WhenIndentationIsPositive(int indentationLevel)
        {
            // Arrange
            var value = "Value";
            var indentationString = "  ";
            var expectedOutput =
                string.Concat(Enumerable.Repeat(indentationString, indentationLevel)) + value;

            var sut = new CommandLineStringBuilder(indentationString);

            // Act
            sut.Append(indentationLevel, value);

            // Assert
            sut.Output.Should().BeEquivalentTo(expectedOutput);
            sut.ToString().Should().BeEquivalentTo(expectedOutput);
        }
    }
}

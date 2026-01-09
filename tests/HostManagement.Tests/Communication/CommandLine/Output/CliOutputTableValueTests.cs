using System.Net;
using AwesomeAssertions;
using Xunit;
using HostManagement.Communication.CommandLine.Output;

namespace HostManagement.Tests.Communication.CommandLine.Output;

public class CliOutputTableValueTests
{
    [Fact]
    public void ShouldReturnNotSetString_WhenValueNull()
    {
        // Arrange
        var expectedString = "'not set'";

        // Act
        var result = CliOutputTableValue.Create(null);

        // Assert
        result.ToString().Should().Be(expectedString);
        result.Length.Should().Be(expectedString.Length);
    }

    [Fact]
    public void ShouldReturnNotSetString_WhenNullPropertyCalled()
    {
        // Arrange
        var expectedString = "'not set'";

        // Act
        var result = CliOutputTableValue.Null;

        // Assert
        result.ToString().Should().Be(expectedString);
        result.Length.Should().Be(expectedString.Length);
    }

    [Fact]
    public void ShouldThrow_WhenTypeNotAllowed()
    {
        // Act
        var result = () => CliOutputTableValue.Create(new Result());

        // Assert
        result.Should()
            .Throw<ArgumentException>()
            .WithMessage($"Type '{nameof(Result)}' is not allowed.");
    }

    [Fact]
    public void ShouldReturnCorrectString_WhenImplicitConversionAndToStringCalled()
    {
        // Arrange
        var expectedString = "Host Management";

        // Act
        CliOutputTableValue value = expectedString;
        string resultFromImplicitConversion = value;

        // Assert
        resultFromImplicitConversion.Should().Be(expectedString);
        value.ToString().Should().Be(expectedString);
        value.Length.Should().Be(expectedString.Length);
    }

    [Theory]
    [InlineData(true, "True")]
    [InlineData(false, "False")]
    public void ShouldReturnCorrectStringRepresentation_WhenCalledWithBool(
            bool value,
            string expectedString)
    {
        // Act
        CliOutputTableValue result = value;

        // Assert
        result.ToString().Should().Be(expectedString);
    }

    [Theory]
    [InlineData(42, "42")]
    public void ShouldReturnCorrectStringRepresentation_WhenCalledWithInt(
        int value,
        string expectedString)
    {
        // Act
        CliOutputTableValue result = value;

        // Assert
        result.ToString().Should().Be(expectedString);
    }

    [Theory]
    [InlineData(3.14f, "3.14")]
    public void ShouldReturnCorrectStringRepresentation_WhenCalledWithFloat(
        float value,
        string expectedString)
    {
        // Act
        CliOutputTableValue result = value;

        // Assert
        result.ToString().Should().Be(expectedString);
    }

    [Theory]
    [InlineData("Host Management", "Host Management")]
    public void ShouldReturnCorrectStringRepresentation_WhenCalledWithString(
        string value,
        string expectedString)
    {
        // Act
        CliOutputTableValue result = value;

        // Assert
        result.ToString().Should().Be(expectedString);
    }

    [Fact]
    public void ShouldReturnCorrectStringRepresentation_WhenCalledWithIPAddress()
    {
        // Arrange
        var expectedString = "192.168.178.42";

        // Act
        CliOutputTableValue result = IPAddress.Parse(expectedString);

        // Assert
        result.ToString().Should().Be(expectedString);
    }

    [Fact]
    public void ShouldReturnCorrectStringRepresentation_WhenCalledWithDateTimeOffset()
    {
        // Arrange
        var expectedString = "03/26/2025 13:53:00 +01:00";

        // Act
        CliOutputTableValue result = new DateTimeOffset(2025, 3, 26, 13, 53, 0, TimeSpan.FromHours(1));

        // Assert
        result.ToString().Should().Be(expectedString);
    }

    [Fact]
    public void ShouldReturnCorrectStringRepresentation_WhenCalledWithDateTime()
    {
        // Arrange
        var expectedString = "03/26/2025 13:53:00";

        // Act
        CliOutputTableValue result = new DateTime(2025, 3, 26, 13, 53, 0);

        // Assert
        result.ToString().Should().Be(expectedString);
    }

    [Fact]
    public void ShouldReturnCorrectStringRepresentation_WhenCalledWithDateOnly()
    {
        // Arrange
        var expectedString = "03/26/2025";

        // Act
        CliOutputTableValue result = new DateOnly(2025, 3, 26);

        // Assert
        result.ToString().Should().Be(expectedString);
    }

    [Fact]
    public void ShouldReturnCorrectStringRepresentation_WhenCalledWithTimeOnly()
    {
        // Arrange
        var expectedString = "13:53";

        // Act
        CliOutputTableValue result = new TimeOnly(13, 53);

        // Assert
        result.ToString().Should().Be(expectedString);
    }
}

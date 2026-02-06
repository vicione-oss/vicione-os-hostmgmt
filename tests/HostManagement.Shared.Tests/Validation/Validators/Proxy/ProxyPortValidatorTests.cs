using AwesomeAssertions;
using HostManagement.Shared.Validation;
using HostManagement.Shared.Validation.Validators.Proxy;
using Xunit;

namespace HostManagement.Shared.Tests.Validation.Validators.Proxy;

public class ProxyPortValidatorTests
{
    private const int MinPort = 1;
    private const int MaxPort = 65_535;

    [Fact]
    public async Task ShouldReturnValidationError_WhenValidationCancelled()
    {
        // Arrange
        var expectedValidationEntry = new ValidationEntry(ValidationConstants.CancelledResult);

        // Act
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var result = await new ProxyPortValidator()
            .ValidateAsync((int)ValidationTestData.Proxy.HTTP.Port!, cts.Token);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData(-42)]
    [InlineData(0)]
    public async Task ShouldReturnValidationError_WhenPortSmallerThanMinimum(int invalidPort)
    {
        // Arrange
        var expectedValidationEntry = new ValidationEntry(
            $"Proxy port '{invalidPort}' can not be smaller than '{MinPort}'.");

        // Act
        var result = await new ProxyPortValidator().ValidateAsync(invalidPort, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldReturnValidationError_WhenPortGreaterThanMaximum()
    {
        // Arrange
        var invalidPort = 65_536;

        var expectedValidationEntry = new ValidationEntry(
            $"Proxy port '{invalidPort}' can not be greater than '{MaxPort}'.");

        // Act
        var result = await new ProxyPortValidator().ValidateAsync(invalidPort, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(42)]
    [InlineData(443)]
    [InlineData(8080)]
    [InlineData(65_535)]
    public async Task ShouldReturnValid_WhenPortValid(int port)
    {
        // Act
        var result = await new ProxyPortValidator().ValidateAsync(port, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Entries.Count.Should().Be(0);
    }
}

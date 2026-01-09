using AwesomeAssertions;
using Xunit;
using HostManagement.Shared.Validation;
using HostManagement.Shared.Validation.Validators.NetworkInterfaces;

namespace HostManagement.Shared.Tests.Validation.Validators.NetworkInterfaces;

public class VLANIDValidatorTests
{
    private static string GetCustomInterfaceNameText(string? interfaceName)
        => interfaceName is null ? "network interface" : $"network interface '{interfaceName}'";

    [Fact]
    public async Task ShouldReturnValidationError_WhenValidationCancelled()
    {
        // Arrange
        var expectedValidationEntry = new ValidationEntry(ValidationConstants.CancelledResult);

        // Act
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var result = await new VLANIDValidator().ValidateAsync(42, cts.Token);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData(null, -1)]
    [InlineData(null, 0)]
    [InlineData(null, 4095)]
    [InlineData("ens33", -1)]
    [InlineData("ens33", 0)]
    [InlineData("ens33", 4095)]
    public async Task ShouldReturnValidationError_WhenVLANIDInvalid(
        string? interfaceName,
        int invalidVLANID)
    {
        // Arrange
        var expectedValidationEntry = new ValidationEntry(
            $"VLAN ID '{invalidVLANID}' of {GetCustomInterfaceNameText(interfaceName)} " +
            $"is invalid (expected 1 to 4094).");

        // Act
        var result = await new VLANIDValidator(interfaceName).ValidateAsync(invalidVLANID);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(4094)]
    public async Task ShouldReturnValid_WhenVLANIDValid(int vlanID)
    {
        // Act
        var result = await new VLANIDValidator().ValidateAsync(vlanID);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Entries.Count.Should().Be(0);
    }
}

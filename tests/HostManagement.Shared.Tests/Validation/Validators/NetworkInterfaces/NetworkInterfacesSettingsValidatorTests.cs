using AwesomeAssertions;
using HostManagement.Shared.Contracts.Network;
using HostManagement.Shared.Validation;
using HostManagement.Shared.Validation.Validators.NetworkInterfaces;
using Xunit;

namespace HostManagement.Shared.Tests.Validation.Validators.NetworkInterfaces;

public class NetworkInterfacesSettingsValidatorTests
{
    [Fact]
    public async Task ShouldReturnValidationError_WhenValidationCancelled()
    {
        // Arrange
        var interfacesSettings = new NetworkInterfacesSettings();
        var expectedValidationEntry = new ValidationEntry(ValidationConstants.CancelledResult);

        // Act
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        var result = await new NetworkInterfacesSettingsValidator()
            .ValidateAsync(interfacesSettings, cts.Token);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldReturnValidationResult_WhenValidateAsyncCalled()
    {
        // Arrange
        var invalidInterfacesSettings = new NetworkInterfacesSettings() with
        {
            NetworkInterfaces =
            [
                ValidationTestData.Interface.Static with
                {
                    CommonInformation = ValidationTestData.Interface.Static.CommonInformation,
                    IPv4 = ValidationTestData.Interface.Static.IPv4 with
                    {
                        IPv4Details = []
                    }
                },
                ValidationTestData.Interface.DHCP
            ]
        };

        var validInterfacesSettings = new NetworkInterfacesSettings() with
        {
            NetworkInterfaces =
            [
                ValidationTestData.Interface.Static,
                ValidationTestData.Interface.DHCP
            ]
        };

        // Act
        var invalidResult = await new NetworkInterfacesSettingsValidator()
            .ValidateAsync(invalidInterfacesSettings, CancellationToken.None);

        var validResult = await new NetworkInterfacesSettingsValidator()
            .ValidateAsync(validInterfacesSettings, CancellationToken.None);

        // Assert
        invalidResult.IsValid.Should().BeFalse();
        invalidResult.Entries.Count.Should().Be(1);
        invalidResult.EntriesString.Should().NotBe(ValidationConstants.CancelledResult);

        validResult.IsValid.Should().BeTrue();
        validResult.Entries.Count.Should().Be(0);
    }
}

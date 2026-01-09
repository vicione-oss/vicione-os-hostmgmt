using FluentAssertions;
using Xunit;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Validation;

namespace HostManagement.Shared.Tests.Validation;

public class SystemConfigurationValidatorTests
{
    [Fact]
    public async Task ShouldAddValidationError_WhenExceptionOccurs()
    {
        // Arrange
        var currentConfig = new SystemConfiguration();
        SystemConfiguration? invalidProposedConfig = null;

        var expectedValidationEntry = new ValidationEntry(
            $"Validation failed." +
            Environment.NewLine +
            $"Object reference not set to an instance of an object.",
            ErrorCodes.ValidationFailed);

        // Act
        var validationResult = await SystemConfigurationValidator
            .Validate(currentConfig, invalidProposedConfig!);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Entries.Count.Should().Be(1);
        validationResult.Entries.Should().Contain(expectedValidationEntry);
    }
}

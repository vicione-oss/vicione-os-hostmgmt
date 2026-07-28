using AwesomeAssertions;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Enums;
using HostManagement.SystemConfig.InternalValidation;
using Xunit;

namespace HostManagement.Tests.SystemConfig.InternalValidation;

public class ServicesValidatorInternalTests
{
    public class Validate
    {
        [Fact]
        public void ShouldAddValidationError_WhenNotAllowedServicesFound()
        {
            // Arrange
            var notAllowedServiceName1 = "not-allowed-1.service";
            var notAllowedServiceName2 = "not-allowed-2.service";

            var proposed = new SystemConfiguration();
            proposed.Services.Add(new() { Name = notAllowedServiceName1, State = ServiceState.Enabled });
            proposed.Services.Add(new() { Name = notAllowedServiceName2, State = ServiceState.Disabled });

            List<string> configurableServices = ["ssh.service", "remoteconnect.service"];
            var validationResult = new ValidationResult();

            var expectedValidationMessage = string.Join(
                ' ',
                "Not allowed configurable services found. Affected services are:",
                $"'{notAllowedServiceName1}', '{notAllowedServiceName2}'.");

            // Act
            ServicesValidatorInternal.Validate(proposed, configurableServices, validationResult);

            // Assert
            validationResult.IsValid.Should().BeFalse();
            validationResult.Messages.Count.Should().Be(1);
            validationResult.Messages.Should().Contain(expectedValidationMessage);
        }

        [Fact]
        public void ShouldAddValidationError_WhenWildcardServicesFound()
        {
            // Arrange
            var wildcardServiceName1 = "wildcard@1.service";
            var wildcardServiceName2 = "wildcard@2.service";

            var proposed = new SystemConfiguration();
            proposed.Services.Add(new() { Name = wildcardServiceName1, State = ServiceState.Enabled });
            proposed.Services.Add(new() { Name = wildcardServiceName2, State = ServiceState.Disabled });

            List<string> configurableServices = ["ssh.service", "remoteconnect.service", "wildcard@*.service"];
            var validationResult = new ValidationResult();

            var expectedValidationMessage = string.Join(
                ' ',
                "Not allowed configurable services found. Affected services are:",
                $"'{wildcardServiceName1}', '{wildcardServiceName2}'.");

            // Act
            ServicesValidatorInternal.Validate(proposed, configurableServices, validationResult);

            // Assert
            validationResult.IsValid.Should().BeFalse();
            validationResult.Messages.Count.Should().Be(1);
            validationResult.Messages.Should().Contain(expectedValidationMessage);
        }

        [Fact]
        public void ShouldHaveNoValidationError_WhenOnlyAllowedServicesFound()
        {
            // Arrange
            var allowedServiceName1 = "allowed-1.service";
            var allowedServiceName2 = "allowed-2.service";

            var proposed = new SystemConfiguration();
            proposed.Services.Add(new() { Name = allowedServiceName1, State = ServiceState.Enabled });
            proposed.Services.Add(new() { Name = allowedServiceName2, State = ServiceState.Disabled });

            List<string> configurableServices = [allowedServiceName1, allowedServiceName2];
            var validationResult = new ValidationResult();

            // Act
            ServicesValidatorInternal.Validate(proposed, configurableServices, validationResult);

            // Assert
            validationResult.IsValid.Should().BeTrue();
            validationResult.Messages.Count.Should().Be(0);
        }
    }
}

using AwesomeAssertions;
using HostManagement.SystemConfig.Contracts;
using HostManagement.SystemConfig.Migrations;
using Xunit;

namespace HostManagement.Tests.SystemConfig.Migrations;

public class SystemConfigurationContractDeserializerRegistryTests
{
    public sealed class Constructor
    {
        [Fact]
        public void ShouldThrow_WhenVersionInvalid()
        {
            // Arrange
            var deserializers = new ISystemConfigurationContractDeserializer[]
            {
                new TestSystemConfigurationContractDeserializer(0)
            };

            // Act
            var func = () => new SystemConfigurationContractDeserializerRegistry(deserializers);

            // Assert
            func.Should()
                .Throw<InvalidOperationException>()
                .WithMessage(
                    "System configuration deserializer 'TestSystemConfigurationContractDeserializer' " +
                    "has invalid version '0'.");
        }

        [Fact]
        public void ShouldThrow_WhenVersionNewerThanCurrentVersion()
        {
            // Arrange
            var version = SystemConfiguration.CurrentVersion + 1;
            var deserializers = new ISystemConfigurationContractDeserializer[]
            {
                new TestSystemConfigurationContractDeserializer(version)
            };

            // Act
            var func = () => new SystemConfigurationContractDeserializerRegistry(deserializers);

            // Assert
            func.Should()
                .Throw<InvalidOperationException>()
                .WithMessage(
                    "System configuration deserializer 'TestSystemConfigurationContractDeserializer' targets version " +
                    $"'{version}', which is newer than the current supported version " +
                    $"'{SystemConfiguration.CurrentVersion}'.");
        }

        [Fact]
        public void ShouldThrow_WhenVersionRegisteredMultipleTimes()
        {
            // Arrange
            var deserializers = new ISystemConfigurationContractDeserializer[]
            {
                new TestSystemConfigurationContractDeserializer(1),
                new TestSystemConfigurationContractDeserializer(1)
            };

            // Act
            var func = () => new SystemConfigurationContractDeserializerRegistry(deserializers);

            // Assert
            func.Should()
                .Throw<InvalidOperationException>()
                .WithMessage("Multiple system configuration deserializers are registered for version '1'.");
        }
    }

    public sealed class TryGetDeserializer
    {
        [Fact]
        public void ShouldReturnFalse_WhenDeserializerMissing()
        {
            // Arrange
            var sut = new SystemConfigurationContractDeserializerRegistry([]);

            // Act
            var result = sut.TryGetDeserializer(1, out var deserializer);

            // Assert
            result.Should().BeFalse();
            deserializer.Should().BeNull();
        }

        [Fact]
        public void ShouldReturnTrue_WhenDeserializerRegistered()
        {
            // Arrange
            var expectedDeserializer = new TestSystemConfigurationContractDeserializer(1);
            var sut = new SystemConfigurationContractDeserializerRegistry([expectedDeserializer]);

            // Act
            var result = sut.TryGetDeserializer(1, out var deserializer);

            // Assert
            result.Should().BeTrue();
            deserializer.Should().BeSameAs(expectedDeserializer);
        }
    }

    private sealed class TestSystemConfigurationContractDeserializer(int version)
        : ISystemConfigurationContractDeserializer
    {
        public int Version { get; } = version;

        public ISystemConfigurationContract Deserialize(System.Text.Json.JsonElement configuration)
            => throw new NotSupportedException();
    }
}

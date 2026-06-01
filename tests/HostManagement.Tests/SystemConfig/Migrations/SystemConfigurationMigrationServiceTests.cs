using AwesomeAssertions;
using HostManagement.SystemConfig.Contracts;
using HostManagement.SystemConfig.Migrations;
using Xunit;

namespace HostManagement.Tests.SystemConfig.Migrations;

public class SystemConfigurationMigrationServiceTests
{
    public sealed class Migrate
    {
        [Fact]
        public void ShouldReturnCurrentConfiguration_WhenSourceVersionIsCurrent()
        {
            // Arrange
            var sourceConfiguration = new SystemConfiguration();
            var sut = new SystemConfigurationMigrationService([]);

            // Act
            var result = sut.Migrate(sourceConfiguration);

            // Assert
            result.Should().BeSameAs(sourceConfiguration);
        }

        [Fact]
        public void ShouldThrow_WhenCurrentVersionHasUnexpectedType()
        {
            // Arrange
            var sourceConfiguration = new TestSystemConfigurationContract(SystemConfiguration.CurrentVersion);
            var sut = new SystemConfigurationMigrationService([]);

            // Act
            var func = () => sut.Migrate(sourceConfiguration);

            // Assert
            func.Should()
                .Throw<InvalidOperationException>()
                .WithMessage(
                    $"System configuration contract version '{SystemConfiguration.CurrentVersion}' is not represented by " +
                    $"'{nameof(SystemConfiguration)}'.");
        }

        [Fact]
        public void ShouldThrow_WhenMigrationMissing()
        {
            // Arrange
            var sourceConfiguration = new TestSystemConfigurationContract(1);
            var sut = new SystemConfigurationMigrationService([]);

            // Act
            var func = () => sut.Migrate(sourceConfiguration);

            // Assert
            func.Should()
                .Throw<NotSupportedException>()
                .WithMessage("System configuration contract version '1' is not supported.");
        }

        [Fact]
        public void ShouldThrow_WhenMigrationProducesUnexpectedVersion()
        {
            // Arrange
            const int ProducedVersion = 1;
            const string ExpectedMessage = "Migration from version '1' to version '2' produced version '1'.";

            var sourceConfiguration = new TestSystemConfigurationContract(1);
            var migration = new TestSystemConfigurationMigration(producedVersion: ProducedVersion);
            var sut = new SystemConfigurationMigrationService([migration]);

            // Act
            var func = () => sut.Migrate(sourceConfiguration);

            // Assert
            func.Should().Throw<InvalidOperationException>().WithMessage(ExpectedMessage);
        }

        [Fact]
        public void ShouldReturnMigratedConfiguration_WhenMigrationRegistered()
        {
            // Arrange
            var sourceConfiguration = new TestSystemConfigurationContract(1);
            var migration = new TestSystemConfigurationMigration();
            var sut = new SystemConfigurationMigrationService([migration]);

            // Act
            var result = sut.Migrate(sourceConfiguration);

            // Assert
            result.Version.Should().Be(SystemConfiguration.CurrentVersion);
            migration.MigrateWasCalled.Should().BeTrue();
        }
    }

    private sealed class TestSystemConfigurationMigration(int producedVersion = SystemConfiguration.CurrentVersion)
        : ISystemConfigurationMigration
    {
        private readonly int _producedVersion = producedVersion;

        public int SourceVersion => 1;

        public int TargetVersion => SystemConfiguration.CurrentVersion;

        public bool MigrateWasCalled { get; private set; }

        public ISystemConfigurationContract Migrate(ISystemConfigurationContract sourceConfiguration)
        {
            MigrateWasCalled = true;
            return new SystemConfiguration()
            {
                Version = _producedVersion
            };
        }
    }

    private sealed class TestSystemConfigurationContract(int version) : ISystemConfigurationContract
    {
        public int Version { get; } = version;
    }
}

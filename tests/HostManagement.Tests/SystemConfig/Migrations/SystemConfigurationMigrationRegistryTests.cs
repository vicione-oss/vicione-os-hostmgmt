using AwesomeAssertions;
using HostManagement.SystemConfig.Contracts;
using HostManagement.SystemConfig.Migrations;
using Xunit;

namespace HostManagement.Tests.SystemConfig.Migrations;

public class SystemConfigurationMigrationRegistryTests
{
    public sealed class Constructor
    {
        [Fact]
        public void ShouldThrow_WhenSourceVersionInvalid()
        {
            // Arrange
            var migrations = new ISystemConfigurationMigration[]
            {
                new TestSystemConfigurationMigration(0, 1)
            };

            // Act
            var act = () => new SystemConfigurationMigrationRegistry(migrations);

            // Assert
            act.Should()
                .Throw<InvalidOperationException>()
                .WithMessage(
                    "System configuration migration 'TestSystemConfigurationMigration' has invalid source version '0'.");
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void ShouldThrow_WhenTargetVersionLessOrEqualToSourceVersion(int sourceVersion)
        {
            // Arrange
            var migrations = new ISystemConfigurationMigration[]
            {
                new TestSystemConfigurationMigration(sourceVersion, 1)
            };

            // Act
            var act = () => new SystemConfigurationMigrationRegistry(migrations);

            // Assert
            act.Should()
                .Throw<InvalidOperationException>()
                .WithMessage(
                    "System configuration migration 'TestSystemConfigurationMigration' must migrate forward, " +
                    $"but migrates from version '{sourceVersion}' to version '1'.");
        }

        [Fact]
        public void ShouldThrow_WhenTargetVersionNewerThanCurrentVersion()
        {
            // Arrange
            var targetVersion = SystemConfiguration.CurrentVersion + 1;
            var migrations = new ISystemConfigurationMigration[]
            {
                new TestSystemConfigurationMigration(1, targetVersion)
            };

            // Act
            var act = () => new SystemConfigurationMigrationRegistry(migrations);

            // Assert
            act.Should()
                .Throw<InvalidOperationException>()
                .WithMessage(
                    "System configuration migration 'TestSystemConfigurationMigration' targets version " +
                    $"'{targetVersion}', which is newer than the current supported version " +
                    $"'{SystemConfiguration.CurrentVersion}'.");
        }

        [Fact]
        public void ShouldThrow_WhenSourceVersionRegisteredMultipleTimes()
        {
            // Arrange
            var migrations = new ISystemConfigurationMigration[]
            {
                new TestSystemConfigurationMigration(1, 2),
                new TestSystemConfigurationMigration(1, 2)
            };

            // Act
            var act = () => new SystemConfigurationMigrationRegistry(migrations);

            // Assert
            act.Should()
                .Throw<InvalidOperationException>()
                .WithMessage("Multiple system configuration migrations are registered for source version '1'.");
        }

        // TODO: Extend registry tests with incomplete and multi-step migration chains when
        // SystemConfiguration.CurrentVersion is greater than 2.

        [Fact]
        public void ShouldCreateRegistry_WhenMigrationsValid()
        {
            // Arrange
            var migrations = new ISystemConfigurationMigration[]
            {
                new TestSystemConfigurationMigration(1, 2)
            };

            // Act
            var act = () => new SystemConfigurationMigrationRegistry(migrations);

            // Assert
            act.Should().NotThrow();
        }
    }

    public sealed class TryGetMigration
    {
        [Fact]
        public void ShouldReturnFalse_WhenMigrationMissing()
        {
            // Arrange
            var sut = new SystemConfigurationMigrationRegistry([]);

            // Act
            var result = sut.TryGetMigration(1, out var migration);

            // Assert
            result.Should().BeFalse();
            migration.Should().BeNull();
        }

        [Fact]
        public void ShouldReturnTrue_WhenMigrationRegistered()
        {
            // Arrange
            var expectedMigration = new TestSystemConfigurationMigration(1, 2);
            var sut = new SystemConfigurationMigrationRegistry([expectedMigration]);

            // Act
            var result = sut.TryGetMigration(1, out var migration);

            // Assert
            result.Should().BeTrue();
            migration.Should().BeSameAs(expectedMigration);
        }
    }

    private sealed class TestSystemConfigurationMigration(int sourceVersion, int targetVersion)
        : ISystemConfigurationMigration
    {
        public int SourceVersion { get; } = sourceVersion;
        public int TargetVersion { get; } = targetVersion;

        public ISystemConfigurationContract Migrate(ISystemConfigurationContract _)
            => throw new NotSupportedException();
    }
}

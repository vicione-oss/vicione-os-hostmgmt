using AwesomeAssertions;
using HostManagement.SystemConfig.Migrations;
using Xunit;

namespace HostManagement.Tests.SystemConfig.Migrations;

public class SystemConfigurationMigrationTests
{
    public sealed class Migrate
    {
        [Fact]
        public void ShouldThrow_WhenSourceVersionUnexpected()
        {
            // Arrange
            var sourceConfiguration = new TestSourceConfiguration()
            {
                Version = 0,
                Name = "source"
            };

            var sut = new ValidSystemConfigurationMigration();

            // Act
            var func = () => sut.Migrate(sourceConfiguration);

            // Assert
            func.Should()
                .Throw<InvalidOperationException>()
                .WithMessage(
                    "Migration 'ValidSystemConfigurationMigration' expected source version '1' " +
                    "but received version '0'.");
        }

        [Fact]
        public void ShouldThrow_WhenTargetVersionUnexpected()
        {
            // Arrange
            var sourceConfiguration = new TestSourceConfiguration()
            {
                Version = 1,
                Name = "source"
            };

            var sut = new InvalidTargetVersionSystemConfigurationMigration();

            // Act
            var func = () => sut.Migrate(sourceConfiguration);

            // Assert
            func.Should()
                .Throw<InvalidOperationException>()
                .WithMessage(
                    "Migration 'InvalidTargetVersionSystemConfigurationMigration' expected target version '2' " +
                    "but produced version '1'.");
        }

        [Fact]
        public void ShouldThrow_WhenSourceTypeUnexpected()
        {
            // Arrange
            var sourceConfiguration = new TestTargetConfiguration()
            {
                Version = 1,
                Name = "source"
            };

            var sut = new ValidSystemConfigurationMigration();

            // Act
            var func = () => sut.Migrate(sourceConfiguration);

            // Assert
            func.Should()
                .Throw<InvalidOperationException>()
                .WithMessage(
                    "Migration 'ValidSystemConfigurationMigration' expected source type 'TestSourceConfiguration' " +
                    "but received 'TestTargetConfiguration'.");
        }

        [Fact]
        public void ShouldReturnMigratedContract_WhenVersionsValid()
        {
            // Arrange
            var sourceConfiguration = new TestSourceConfiguration()
            {
                Version = 1,
                Name = "source"
            };

            var sut = new ValidSystemConfigurationMigration();

            // Act
            var result = sut.Migrate(sourceConfiguration);

            // Assert
            var typedResult = result.Should().BeOfType<TestTargetConfiguration>().Subject;

            typedResult.Version.Should().Be(2);
            typedResult.Name.Should().Be("source-migrated");
        }
    }

    private sealed class ValidSystemConfigurationMigration
        : SystemConfigurationMigration<TestSourceConfiguration, TestTargetConfiguration>
    {
        public override int SourceVersion => 1;
        public override int TargetVersion => 2;

        protected override TestTargetConfiguration Migrate(TestSourceConfiguration sourceConfiguration)
            => new()
            {
                Version = TargetVersion,
                Name = $"{sourceConfiguration.Name}-migrated"
            };
    }

    private sealed class InvalidTargetVersionSystemConfigurationMigration
        : SystemConfigurationMigration<TestSourceConfiguration, TestTargetConfiguration>
    {
        public override int SourceVersion => 1;
        public override int TargetVersion => 2;

        protected override TestTargetConfiguration Migrate(TestSourceConfiguration sourceConfiguration)
            => new()
            {
                Version = SourceVersion,
                Name = sourceConfiguration.Name
            };
    }
}

internal sealed class TestSourceConfiguration : ISystemConfigurationContract
{
    public int Version { get; init; }

    public string? Name { get; init; }
}

internal sealed class TestTargetConfiguration : ISystemConfigurationContract
{
    public int Version { get; init; }

    public string? Name { get; init; }
}

using AwesomeAssertions;
using Xunit;

namespace HostManagement.Scripts.Tests;

public sealed class ResourceHelperTests
{
    private const string ExistingResource =
        "HostManagement.Scripts.Resources.Scripts.shared.Common.get_system_architecture.sh";

#pragma warning disable CA1711
    public sealed class GetResourceStream
    {
        [Fact]
        public void ShouldThrow_WhenResourceDoesNotExist()
        {
            // Arrange
            var sut = new ResourceHelper();

            // Act
            var func = () => sut.GetResourceStream("Resource.That.Does.Not.Exist");

            // Assert
            func.Should().Throw<InvalidOperationException>().WithMessage("Resource stream was null.");
        }

        [Fact]
        public void ShouldReturnResourceStream_WhenResourceExists()
        {
            // Arrange
            var sut = new ResourceHelper();

            // Act
            using var result = sut.GetResourceStream(ExistingResource);

            // Assert
            result.CanRead.Should().BeTrue();
        }
    }
#pragma warning restore CA1711

    public sealed class GetManifestResourceNames
    {
        [Fact]
        public void ShouldReturnManifestResourceNames_WhenPredicateNotProvided()
        {
            // Arrange
            var sut = new ResourceHelper();

            // Act
            var result = sut.GetManifestResourceNames();

            // Assert
            result.Should().Contain(ExistingResource);
        }

        [Fact]
        public void ShouldReturnFilteredManifestResourceNames_WhenPredicateProvided()
        {
            // Arrange
            var sut = new ResourceHelper();

            // Act
            var result = sut.GetManifestResourceNames(
                resourceName => resourceName.EndsWith("shared.Common.get_system_architecture.sh"));

            // Assert
            result.Should().ContainSingle()
                .Which.Should().Be(ExistingResource);
        }
    }
}

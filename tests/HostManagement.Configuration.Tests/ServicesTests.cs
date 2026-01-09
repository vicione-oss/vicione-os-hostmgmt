using FluentAssertions;
using Xunit;

namespace HostManagement.Configuration.Tests;

public class ServicesTests
{
    public class KnownUnitTypes
    {
        [Fact]
        public void KnownUnitTypes_ShouldContainExpectedValues()
        {
            // Arrange
            var expectedUnitTypes = new HashSet<string>
            {
                ".service",
                ".socket",
                ".timer",
                ".mount",
                ".automount",
                ".target",
                ".path",
                ".slice",
                ".scope"
            };

            // Act
            var actualUnitTypes = Services.KnownUnitTypes;

            // Assert
            actualUnitTypes.Should().BeEquivalentTo(expectedUnitTypes);
        }
    }
}

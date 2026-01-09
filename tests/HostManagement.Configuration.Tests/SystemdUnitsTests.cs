using AwesomeAssertions;
using Xunit;

namespace HostManagement.Configuration.Tests;

public class SystemdUnitsTests
{
    public class KnownUnitTypes
    {
        [Fact]
        public void KnownUnitTypes_ShouldReturnExpectedValues()
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
            var actualUnitTypes = SystemdUnits.KnownUnitTypes;

            // Assert
            actualUnitTypes.Should().BeEquivalentTo(expectedUnitTypes);
        }
    }
}

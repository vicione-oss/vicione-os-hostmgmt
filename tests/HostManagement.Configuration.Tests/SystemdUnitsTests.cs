using AwesomeAssertions;
using Xunit;

namespace HostManagement.Configuration.Tests;

public class SystemdUnitsTests
{
    public class HasKnownUnitTypeSuffix
    {
        public static TheoryData<string> KnownUnitTypesData =>
        [
            ".service",
            ".socket",
            ".timer",
            ".mount",
            ".automount",
            ".target",
            ".path",
            ".slice",
            ".scope"
        ];

        [Theory]
        [MemberData(nameof(KnownUnitTypesData))]
        public void ShouldReturnTrue_WhenKnownUnitTypeFound(string knownUnitType)
            => SystemdUnits.HasKnownUnitTypeSuffix(knownUnitType).Should().BeTrue();

        [Theory]
        [InlineData(".unknown")]
        [InlineData(".dll")]
        [InlineData(".exe")]
        [InlineData(".services")]
        [InlineData("none")]
        public void ShouldReturnFalse_WhenUnknownUnitTypeFound(string unknownUnitType)
            => SystemdUnits.HasKnownUnitTypeSuffix(unknownUnitType).Should().BeFalse();
    }
}

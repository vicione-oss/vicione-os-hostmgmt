using AwesomeAssertions;
using Xunit;

namespace HostManagement.Configuration.Tests;

public class TargetEnvironmentExtensionsTests
{
    private const string Debian = "debian";
    private const string EdgeSViciOne = "edges-vicione";
    private const string Unknown = "unknown";

    public class ToEnvironmentString
    {
        [Fact]
        public void ShouldReturnTargetEnvironmentUnknownString_WhenTargetEnvironmentUnknown()
        {
            TargetEnvironment.Unknown.ToEnvironmentString().Should().Be(Unknown);
            ((TargetEnvironment)999).ToEnvironmentString().Should().Be(Unknown);
            ((TargetEnvironment)31415).ToEnvironmentString().Should().Be(Unknown);
        }

        [Fact]
        public void ShouldReturnTargetEnvironmentString_WhenTargetEnvironmentKnown()
        {
            TargetEnvironment.Debian.ToEnvironmentString().Should().Be(Debian);
            TargetEnvironment.EdgeSViciOne.ToEnvironmentString().Should().Be(EdgeSViciOne);
        }
    }

    public class FromEnvironmentString
    {
        [Fact]
        public void ShouldReturnTargetEnvironmentUnknown_WhenTargetEnvironmentEmpty()
        {
            TargetEnvironmentExtensions.FromEnvironmentString("").Should().Be(TargetEnvironment.Unknown);
            TargetEnvironmentExtensions.FromEnvironmentString("   ").Should().Be(TargetEnvironment.Unknown);
        }

        [Fact]
        public void ShouldReturnTargetEnvironmentUnknown_WhenTargetEnvironmentStringUnknown()
        {
            TargetEnvironmentExtensions.FromEnvironmentString("unknown").Should().Be(TargetEnvironment.Unknown);
            TargetEnvironmentExtensions.FromEnvironmentString("999").Should().Be(TargetEnvironment.Unknown);
            TargetEnvironmentExtensions.FromEnvironmentString("31415").Should().Be(TargetEnvironment.Unknown);
            TargetEnvironmentExtensions.FromEnvironmentString("lorem ipsum dolor").Should().Be(TargetEnvironment.Unknown);
        }

        [Fact]
        public void ShouldReturnTargetEnvironment_WhenTargetEnvironmentStringKnown()
        {
            TargetEnvironmentExtensions.FromEnvironmentString(Debian).Should().Be(TargetEnvironment.Debian);
            TargetEnvironmentExtensions.FromEnvironmentString(EdgeSViciOne).Should().Be(TargetEnvironment.EdgeSViciOne);
        }
    }
}

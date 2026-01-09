using FluentAssertions;
using Xunit;

namespace HostManagement.Tests;

public class DebianPackageVersionTests
{
    private const string DefaultEpoch = "0";
    private const string DefaultDebianRevision = "0";

    public static readonly TheoryData<string, string> SameVersionsSampleData = new()
    {
        // used for Equals, LessThanOrEqual and GreaterThanOrEqual tests

        // with default epoch and revision
        { "1.2.3",          "1.2.3" },
        { "1.2.3~8787",     "1.2.3~8787" },
        { "1.2.3~alpha1",   "1.2.3~alpha1" },
        { "1.2.3~beta2",    "1.2.3~beta2" },
        { "1.2.3~rc3~1234", "1.2.3~rc3~1234" },

        // with epoch specified and default revision
        { "2:1.2.3",          "2:1.2.3" },
        { "2:1.2.3~8787",     "2:1.2.3~8787" },
        { "2:1.2.3~alpha1",   "2:1.2.3~alpha1" },
        { "2:1.2.3~beta2",    "2:1.2.3~beta2" },
        { "2:1.2.3~rc3~1234", "2:1.2.3~rc3~1234" },

        // with epoch and revision specified
        { "2:1.2.3-2+deb12u2",          "2:1.2.3-2+deb12u2" },
        { "2:1.2.3~8787-2+deb12u2",     "2:1.2.3~8787-2+deb12u2" },
        { "2:1.2.3~alpha1-2+deb12u2",   "2:1.2.3~alpha1-2+deb12u2" },
        { "2:1.2.3~beta2-2+deb12u2",    "2:1.2.3~beta2-2+deb12u2" },
        { "2:1.2.3~rc3~1234-2+deb12u2", "2:1.2.3~rc3~1234-2+deb12u2" },
    };

    public static readonly TheoryData<string, string> DifferentVersionsSampleData = new()
    {
        // used for NotEquals, LessThan and GreaterThan tests

        // with default epoch and revision
        // only upstream different
        { "1.2.3",          "2.2.3" },
        { "1.2.3~8787",     "1.2.3~8989" },
        { "1.2.3~alpha1",   "1.2.3~alpha2" },
        { "1.2.3~beta2",    "1.2.3~rc2" },
        { "1.2.3~rc3~1234", "1.2.3~rc4~4321" },

        // with epoch specified and default revision
        // only epoch different
        { "2:1.2.3",          "3:1.2.3" },
        { "2:1.2.3~8787",     "3:1.2.3~8787" },
        { "2:1.2.3~alpha1",   "3:1.2.3~alpha1" },
        { "2:1.2.3~beta2",    "3:1.2.3~beta2" },
        { "2:1.2.3~rc3~1234", "3:1.2.3~rc3~1234" },

        // with epoch and revision specified
        // only revision different
        { "2:1.2.3-2+deb12u2",          "2:1.2.3-2+deb12u3" },
        { "2:1.2.3~8787-2+deb12u2",     "2:1.2.3~8787-2+deb13u2" },
        { "2:1.2.3~alpha1-2+deb12u2",   "2:1.2.3~alpha1-3+deb12u2" },
        { "2:1.2.3~beta2-2+deb12u2",    "2:1.2.3~beta2-2+deb12u3" },
        { "2:1.2.3~rc3~1234-2+deb12u2", "2:1.2.3~rc3~1234-3+deb12u2" },

        // with epoch specified and default revision
        // epoch and upstream different
        { "2:1.2.3",          "3:2.2.3" },
        { "2:1.2.3~8787",     "3:1.2.3~8989" },
        { "3:1.2.3~alpha1",   "42:1.2.3~alpha2" },
        { "3:1.2.3~beta2",    "42:1.2.3~rc2" },
        { "1:1.2.3~rc3~1234", "31415:1.2.3~rc4~4321" },

        // with epoch and revision specified
        // epoch, upstream and revision different
        { "1:1.2.3-2+deb12u2",          "2:1.2.3-2+deb12u3" },
        { "3:1.2.3~8787-2+deb12u2",     "4:1.2.3~8787-2+deb13u2" },
        { "5:1.2.3~alpha1-2+deb12u2",   "8:1.2.3~alpha1-3+deb12u2" },
        { "8:1.2.3~beta2-2+deb12u2",    "16:1.2.3~beta2-2+deb12u3" },
        { "13:1.2.3~rc3~1234-2+deb12u2", "32:1.2.3~rc3~1234-3+deb12u2" },
    };

    [Fact]
    public void ShouldSortVersionsWithDebianPackageVersionComparer_WhenCalled()
    {
        // Arrange
        var versions = new List<DebianPackageVersion>
        {
            new ("1.0"),
            new ("1.0-1"),
            new ("1.0~beta1"),
            new ("0.9.9"),
            new ("1.0.1"),
            new ("1.0.0~rc1~1234+10-1"),
            new ("1.0.0+git20190401"),
            new ("2:1.0"),
            new ("1:1.0"),
            new ("1.0.0~77878"),
            new ("1.0-2"),
            new ("1.0.0~rc1"),
            new ("1.0-deb9u1-2"),
            new ("1.0.0"),
            new ("1.0+deb9u1-1"),
            new ("1.0.0+dfsg1-1"),
            new ("1.0~alpha1"),
            new ("1.0.0~rc1~1234+1-2"),
            new ("1.0.0~beta1"),
            new ("1.0.0~alpha1"),
            new ("1.0~alpha1~1234"),
            new ("1.0.0~rc1-1"),
            new ("1.0+exp1"),
            new ("1.0+deb9u1"),
            new ("1.0+deb10u1"),
            new ("1.0.0-0.1"),
            new ("1.0.0-0"),
            new ("1.0.0-1"),
            new ("1.0.0-1ubuntu1"),
            new ("1.0.0-1ubuntu2"),
            new ("1.0.0-1~ubuntu1"),
            new ("1.0.0-1~ubuntu0.1"),
            new ("1.0.0-1~ubuntu0"),
            new ("1.0.0-1ubuntu0"),
            new ("1.0.0+repack-1"),
            new ("1.0.0+repack1-1"),
            new ("1.0.0+repack1-2"),
            new ("1.0.0+repack2-1"),
            new ("1.0.0+repack2-2")
        };

        var expectedSortedVersions = new List<DebianPackageVersion>
        {
            new ("0.9.9"),
            new ("1.0~alpha1~1234"),
            new ("1.0~alpha1"),
            new ("1.0~beta1"),
            new ("1.0"),
            new ("1.0-1"),
            new ("1.0-2"),
            new ("1.0+deb9u1"),
            new ("1.0+deb9u1-1"),
            new ("1.0+deb10u1"),
            new ("1.0+exp1"),
            new ("1.0-deb9u1-2"),
            new ("1.0.0~77878"),
            new ("1.0.0~alpha1"),
            new ("1.0.0~beta1"),
            new ("1.0.0~rc1~1234+1-2"),
            new ("1.0.0~rc1~1234+10-1"),
            new ("1.0.0~rc1"),
            new ("1.0.0~rc1-1"),
            new ("1.0.0"),
            new ("1.0.0"),
            new ("1.0.0-0.1"),
            new ("1.0.0-1~ubuntu0"),
            new ("1.0.0-1~ubuntu0.1"),
            new ("1.0.0-1~ubuntu1"),
            new ("1.0.0-1"),
            new ("1.0.0-1ubuntu0"),
            new ("1.0.0-1ubuntu1"),
            new ("1.0.0-1ubuntu2"),
            new ("1.0.0+dfsg1-1"),
            new ("1.0.0+git20190401"),
            new ("1.0.0+repack-1"),
            new ("1.0.0+repack1-1"),
            new ("1.0.0+repack1-2"),
            new ("1.0.0+repack2-1"),
            new ("1.0.0+repack2-2"),
            new ("1.0.1"),
            new ("1:1.0"),
            new ("2:1.0"),
        };

        // Act

        // Sort the versions using the CompareTo method of DebianPackageVersion class
        versions.Sort();

        // Assert
        versions.Should().BeEquivalentTo(expectedSortedVersions);
    }

    public class Constructor
    {
        [Fact]
        public void ShouldCreateDebianPackageVersion_WhenUpstreamVersionPresent()
        {
            // Arrange
            var upstreamVersion = "1.2.3~alpha1";

            // Act
            var result = new DebianPackageVersion(upstreamVersion);

            // Assert
            result.Should().BeOfType<DebianPackageVersion>();
            result.Epoch.Should().Be(DefaultEpoch);
            result.UpstreamVersion.Should().Be(upstreamVersion);
            result.DebianRevision.Should().Be(DefaultDebianRevision);
        }

        [Fact]
        public void ShouldCreateDebianPackageVersion_WhenUpstreamVersionAndRevisionPresent()
        {
            // Arrange
            var upstreamVersion = "1.2.3~alpha2";
            var debianRevision = "1";

            // Act
            var result = new DebianPackageVersion(upstreamVersion, debianRevision);

            // Assert
            result.Should().BeOfType<DebianPackageVersion>();
            result.Epoch.Should().Be(DefaultEpoch);
            result.UpstreamVersion.Should().Be(upstreamVersion);
            result.DebianRevision.Should().Be(debianRevision);
        }

        [Fact]
        public void ShouldCreateDebianPackageVersion_WhenEpochAndUpstreamVersionAndRevisionPresent()
        {
            // Arrange
            var epoch = "1";
            var upstreamVersion = "1.2.3~alpha3";
            var debianRevision = "2";

            // Act
            var result = new DebianPackageVersion(epoch, upstreamVersion, debianRevision);

            // Assert
            result.Should().BeOfType<DebianPackageVersion>();
            result.Epoch.Should().Be(epoch);
            result.UpstreamVersion.Should().Be(upstreamVersion);
            result.DebianRevision.Should().Be(debianRevision);
        }

        [Fact]
        public void ShouldCreateDebianPackageVersion_WhenCompleteVersionStringGiven()
        {
            // Arrange
            var expectedEpoch = "2";
            var expectedUpstreamVersion = "1.2.3~alpha4";
            var expectedDebianRevision = "3";

            var version = $"{expectedEpoch}:{expectedUpstreamVersion}-{expectedDebianRevision}";

            // Act
            var result = new DebianPackageVersion(version);

            // Assert
            result.Should().BeOfType<DebianPackageVersion>();
            result.Epoch.Should().Be(expectedEpoch);
            result.UpstreamVersion.Should().Be(expectedUpstreamVersion);
            result.DebianRevision.Should().Be(expectedDebianRevision);
        }
    }

    public class Parse
    {
        [Fact]
        public void ShouldThrow_WhenVersionNull()
        {
            // Arrange
            string? version = null;

            // Act
            var result = () => DebianPackageVersion.Parse(version!);

            // Assert
            result.Should()
                .Throw<ArgumentNullException>()
                .WithParameterName("version");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void ShouldThrow_WhenVersionEmptyOrWhiteSpace(string version)
        {
            // Arrange
            var expectedParameterName = "version";
            var expectedErrorMessage = $"Version '{version}' can not be empty.*";

            // Act
            var result = () => DebianPackageVersion.Parse(version);

            // Assert
            result.Should()
                .Throw<ArgumentException>()
                .WithMessage(expectedErrorMessage)
                .WithParameterName(expectedParameterName);
        }

        [Theory]
        [InlineData("\u0668")] // ARABIC-INDIC DIGIT EIGHT equivalent to "٨"
        [InlineData("-2")]
        public void ShouldThrow_WhenEpochContainsNonAsciiDigit(string nonAsciiEpoch)
        {
            // Arrange
            var invalidVersion = $"{nonAsciiEpoch}:1.2.3";

            var expectedErrorMessage =
                $"Epoch '{nonAsciiEpoch}' of version '{invalidVersion}' invalid. " +
                $"Epoch must consist of ASCII digits only (0-9).";

            // Act
            var result = () => DebianPackageVersion.Parse(invalidVersion);

            // Assert
            result.Should()
                .Throw<FormatException>()
                .WithMessage(expectedErrorMessage);
        }

        [Fact]
        public void ShouldThrow_WhenDebianRevisionEmpty()
        {
            // Arrange
            var invalidVersion = $"0:1.2.3-";

            var expectedErrorMessage =
                $"Debian revision '' of version '{invalidVersion}' invalid. " +
                $"Revision must contain only alphanumeric characters and '+', '.' or '~'.";

            // Act
            var result = () => DebianPackageVersion.Parse(invalidVersion);

            // Assert
            result.Should()
                .Throw<FormatException>()
                .WithMessage(expectedErrorMessage);
        }

        [Theory]
        [InlineData("_")]
        [InlineData(":")]
        [InlineData("ß")]
        [InlineData("Ö")]
        public void ShouldThrow_WhenDebianRevisionContainsInvalidCharacters(
            string invalidDebianRevision)
        {
            // Arrange
            var invalidVersion = $"0:1.2.3-{invalidDebianRevision}";

            var expectedErrorMessage =
                $"Debian revision '{invalidDebianRevision}' of version '{invalidVersion}' invalid. " +
                $"Revision must contain only alphanumeric characters and '+', '.' or '~'.";

            // Act
            var result = () => DebianPackageVersion.Parse(invalidVersion);

            // Assert
            result.Should()
                .Throw<FormatException>()
                .WithMessage(expectedErrorMessage);
        }

        [Fact]
        public void ShouldThrow_WhenUpstreamVersionEmpty()
        {
            // Arrange
            var invalidVersion = $"0:-42";

            var expectedErrorMessage =
                $"Upstream version '' of version '{invalidVersion}' invalid. " +
                $"Upstream version can not be empty.";

            // Act
            var result = () => DebianPackageVersion.Parse(invalidVersion);

            // Assert
            result.Should()
                .Throw<FormatException>()
                .WithMessage(expectedErrorMessage);
        }

        [Theory]
        [InlineData("M")]
        [InlineData("F")]
        [InlineData("+")]
        [InlineData(".")]
        [InlineData("~")]
        public void ShouldThrow_WhenUpstreamVersionStartsWithNoDigit(
            string notRecommendedUpstreamStart)
        {
            // Arrange
            var upstreamVersion = $"{notRecommendedUpstreamStart}1.2";
            var notRecommendedVersion = $"0:{upstreamVersion}-42";

            var expectedErrorMessage =
                $"Upstream version '{upstreamVersion}' of version '{notRecommendedVersion}' invalid. " +
                $"Upstream version should start with a digit.";

            // Act
            var result = () => DebianPackageVersion.Parse(notRecommendedVersion);

            // Assert
            result.Should()
                .Throw<FormatException>()
                .WithMessage(expectedErrorMessage);
        }

        [Theory]
        [InlineData("1,2")]
        [InlineData("1.2%")]
        [InlineData("1.2.*")]
        [InlineData("2024.11.29:2")]
        [InlineData("1.2.3~alpha?")]
        [InlineData("1.2.3+/nmu3")]
        public void ShouldThrow_WhenUpstreamVersionContainsInvalidCharactersWhenNoRevisionPresent(
            string invalidUpstreamVersion)
        {
            // Arrange
            var invalidVersion = $"0:{invalidUpstreamVersion}";
            var expectedErrorMessage =
                $"Upstream version '{invalidUpstreamVersion}' of version '{invalidVersion}' invalid. " +
                $"Upstream version must contain only alphanumeric characters and '+', '.' or '~'.";

            // Act
            var result = () => DebianPackageVersion.Parse(invalidVersion);

            // Assert
            result.Should()
                .Throw<FormatException>()
                .WithMessage(expectedErrorMessage);
        }

        [Theory]
        [InlineData("1,2")]
        [InlineData("1.2%")]
        [InlineData("1.2.*")]
        [InlineData("2024-11-29:2")]
        [InlineData("1.2.3~alpha?")]
        [InlineData("1.2.3+/nmu3")]
        public void ShouldThrow_WhenUpstreamVersionContainsInvalidCharactersWhenRevisionPresent(
            string invalidUpstreamVersion)
        {
            // Arrange
            var invalidVersion = $"0:{invalidUpstreamVersion}-42";
            var expectedErrorMessage =
                $"Upstream version '{invalidUpstreamVersion}' of version '{invalidVersion}' invalid. " +
                $"Upstream version must contain only alphanumeric characters and '-', '+', '.' or '~'.";

            // Act
            var result = () => DebianPackageVersion.Parse(invalidVersion);

            // Assert
            result.Should()
                .Throw<FormatException>()
                .WithMessage(expectedErrorMessage);
        }

        [Theory]
        [InlineData("", "1.2.3", "")]
        [InlineData("", "1.2.3", "2")]
        [InlineData("1", "1.2.3", "")]
        [InlineData("1", "1.2.3", "2")]
        [InlineData("", "1.2.3~77878", "")]
        [InlineData("", "1.2.3~alpha1", "")]
        [InlineData("", "1.2.3~beta2", "")]
        [InlineData("", "1.2.3~rc3", "")]
        [InlineData("", "1.2.3~rc3~1234+1", "")]
        [InlineData("", "1.2.3~rc3~1234+1", "1")]
        [InlineData("", "1.0+deb10u1", "")]
        [InlineData("", "1.2.3", "1ubuntu1")]
        public void ShouldReturnDebianPackageVersion_WhenVersionCanBeParsed(
            string epoch,
            string upstreamVersion,
            string debianRevision)
        {
            // Arrange
            var epochString = string.IsNullOrWhiteSpace(epoch)
                ? string.Empty
                : $"{epoch}:";

            var debianRevisionString = string.IsNullOrWhiteSpace(debianRevision)
                ? string.Empty
                : $"-{debianRevision}";

            var version = $"{epochString}{upstreamVersion}{debianRevisionString}";

            var expectedEpoch = string.IsNullOrWhiteSpace(epoch)
                ? "0"
                : epoch;

            var expectedDebianRevision = string.IsNullOrWhiteSpace(debianRevision)
                ? "0"
                : debianRevision;

            // Act
            var result = DebianPackageVersion.Parse(version);

            // Assert
            result.Epoch.Should().Be(expectedEpoch);
            result.UpstreamVersion.Should().Be(upstreamVersion);
            result.DebianRevision.Should().Be(expectedDebianRevision);
        }

        [Fact]
        public void ShouldReturnDebianPackageVersion_WhenVersionGivenAsSpan()
        {
            // Arrange
            var expectedEpoch = "3";
            var expectedUpstreamVersion = "1.2.3~beta1";
            var expectedDebianRevision = "4";

            var version = $"{expectedEpoch}:{expectedUpstreamVersion}-{expectedDebianRevision}";

            // Act
            var result = DebianPackageVersion.Parse(version.AsSpan());

            // Assert
            result.Epoch.Should().Be(expectedEpoch);
            result.UpstreamVersion.Should().Be(expectedUpstreamVersion);
            result.DebianRevision.Should().Be(expectedDebianRevision);
        }
    }

    public class TryParse
    {
        [Fact]
        public void ShouldReturnFalseWithParsedVersionNull_WhenVersionNull()
        {
            // Arrange
            string? version = null;

            // Act
            var result = DebianPackageVersion.TryParse(version!, out var parsedVersion);

            // Assert
            result.Should().BeFalse();
            parsedVersion.Should().BeNull();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void ShouldReturnFalseWithParsedVersionNull_WhenVersionEmptyOrWhiteSpace(
            string version)
        {
            // Act
            var result = DebianPackageVersion.TryParse(version, out var parsedVersion);

            // Assert
            result.Should().BeFalse();
            parsedVersion.Should().BeNull();
        }

        [Fact]
        public void ShouldReturnTrueWithParsedVersion_WhenVersionValid()
        {
            // Arrange
            var version = "1.2.3~rc1";
            var expectedVersion = new DebianPackageVersion(version);

            // Act
            var result = DebianPackageVersion.TryParse(version, out var parsedVersion);

            // Assert
            result.Should().BeTrue();
            parsedVersion.Should().BeEquivalentTo(expectedVersion);
        }
    }

    public class CompareTo_Object
    {
        [Fact]
        public void ShouldReturn1_WhenVersionNull()
        {
            // Arrange
            var leftVersion = new DebianPackageVersion("1.2.3~rc2");
            object? rightVersion = null;

            // Act
            var result = leftVersion.CompareTo(rightVersion);

            // Assert
            result.Should().Be(1); // left version greater than rightVersion ("null")
        }

        [Fact]
        public void ShouldUseCompareToWithDebianPackageVersion_WhenVersionIsDebianPackageVersion()
        {
            // Arrange
            var leftVersion = new DebianPackageVersion("1.2.3~rc2");
            object? rightVersion = new DebianPackageVersion("1.2.3~rc3");

            // Act
            // CompareTo(object?) should delegate to CompareTo(DebianPackageVersion?)
            var result = leftVersion.CompareTo(rightVersion);

            // Assert
            result.Should().Be(-1); // left version smaller than rightVersion
        }

        [Fact]
        public void ShouldThrow_WhenArgumentIsNotDebianPackageVersion()
        {
            // Arrange
            var leftVersion = new DebianPackageVersion("1.2.3~rc2");
            object? rightVersion = new Version("1.2.3");

            var expectedMessage = $"Argument must be a {nameof(DebianPackageVersion)}.*";

            // Act
            var result = () => leftVersion.CompareTo(rightVersion);

            // Assert
            result.Should()
                .Throw<ArgumentException>()
                .WithMessage(expectedMessage);
        }
    }

    public class CompareTo_DebianPackageVersion
    {
        [Fact]
        public void ShouldReturn0_WhenReferencesEqual()
        {
            // Arrange
            var leftVersion = new DebianPackageVersion("1.2.3~rc2");
            var rightVersion = leftVersion;

            // Act
            var result = leftVersion.CompareTo(rightVersion);

            // Assert
            result.Should().Be(0);
        }

        [Fact]
        public void ShouldReturn1_WhenVersionNull()
        {
            // Arrange
            var leftVersion = new DebianPackageVersion("1.2.3~rc2");
            DebianPackageVersion? rightVersion = null;

            // Act
            var result = leftVersion.CompareTo(rightVersion);

            // Assert
            result.Should().Be(1); // left version greater than rightVersion ("null")
        }

        [Theory]
        [InlineData("1", "2", -1)]
        [InlineData("2", "1", 1)]
        public void ShouldReturnComparerResult_WhenEpochsDifferent(
            string leftEpoch,
            string rightEpoch,
            int expectedComparison)
        {
            // Arrange
            var leftVersion = new DebianPackageVersion($"{leftEpoch}:1.2.3~rc2");
            var rightVersion = new DebianPackageVersion($"{rightEpoch}:1.2.3~rc2");

            // Act
            var result = leftVersion.CompareTo(rightVersion);

            // Assert
            result.Should().Be(expectedComparison);
        }

        [Fact]
        public void ShouldReturnNegativeComparerResult_WhenLeftUpstreamVersionSmaller()
        {
            // Arrange
            var leftVersion = new DebianPackageVersion($"1:1.2.3~alpha1");
            var rightVersion = new DebianPackageVersion($"1:1.2.3~rc2");

            // Act
            var result = leftVersion.CompareTo(rightVersion);

            // Assert
            result.Should().BeNegative();
        }

        [Fact]
        public void ShouldReturnPositiveComparerResult_WhenLeftUpstreamVersionGreater()
        {
            // Arrange
            var leftVersion = new DebianPackageVersion($"1:1.2.3~rc2");
            var rightVersion = new DebianPackageVersion($"1:1.2.3~alpha1");

            // Act
            var result = leftVersion.CompareTo(rightVersion);

            // Assert
            result.Should().BePositive();
        }

        [Fact]
        public void ShouldReturn0_WhenUpstreamVersionEqual()
        {
            // Arrange
            var leftVersion = new DebianPackageVersion($"1:1.2.3~rc2");
            var rightVersion = new DebianPackageVersion($"1:1.2.3~rc2");

            // Act
            var result = leftVersion.CompareTo(rightVersion);

            // Assert
            result.Should().Be(0);
        }

        [Theory]
        [InlineData("2", "3")]
        [InlineData("0ubuntu1", "1")]
        [InlineData("1~bpo70+1+b1", "1~bpo70+2+b1")]
        [InlineData("1~bpo70u1", "1~bpo70+1+b1")]
        public void ShouldReturnNegativeComparerResult_WhenLeftRevisionSmaller(
            string leftDebianRevision,
            string rightDebianRevision)
        {
            // Arrange
            var leftVersion = new DebianPackageVersion($"1:1.2.3~rc2-{leftDebianRevision}");
            var rightVersion = new DebianPackageVersion($"1:1.2.3~rc2-{rightDebianRevision}");

            // Act
            var result = leftVersion.CompareTo(rightVersion);

            // Assert
            result.Should().BeNegative();
        }

        [Theory]
        [InlineData("3", "2")]
        [InlineData("1", "0ubuntu1")]
        [InlineData("1~bpo70+2+b1", "1~bpo70+1+b1")]
        [InlineData("1~bpo70+2+b1", "1~bpo70u2")]
        public void ShouldReturnPositiveComparerResult_WhenLeftRevisionGreater(
            string leftDebianRevision,
            string rightDebianRevision)
        {
            // Arrange
            var leftVersion = new DebianPackageVersion($"1:1.2.3~rc2-{leftDebianRevision}");
            var rightVersion = new DebianPackageVersion($"1:1.2.3~rc2-{rightDebianRevision}");

            // Act
            var result = leftVersion.CompareTo(rightVersion);

            // Assert
            result.Should().BePositive();
        }

        [Theory]
        [InlineData("3", "3")]
        [InlineData("0ubuntu1", "0ubuntu1")]
        [InlineData("1~bpo70+2+b1", "1~bpo70+2+b1")]
        public void ShouldReturn0_WhenRevisionsEqual(
            string leftDebianRevision,
            string rightDebianRevision)
        {
            // Arrange
            var leftVersion = new DebianPackageVersion($"1:1.2.3~rc2-{leftDebianRevision}");
            var rightVersion = new DebianPackageVersion($"1:1.2.3~rc2-{rightDebianRevision}");

            // Act
            var result = leftVersion.CompareTo(rightVersion);

            // Assert
            result.Should().Be(0);
        }
    }

    public class Equals_Object
    {
        [Fact]
        public void ShouldReturnTrue_WhenReferencesEqual()
        {
            // Arrange
            var leftVersion = new DebianPackageVersion("1.2.3");
            object? rightVersion = leftVersion;

            // Act
            var result = leftVersion.Equals(rightVersion);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldReturnTrue_WhenVersionsEqual()
        {
            // Arrange
            var leftVersion = new DebianPackageVersion("1.2.3");
            object? rightVersion = new DebianPackageVersion("1.2.3");

            // Act
            var result = leftVersion.Equals(rightVersion);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldReturnFalse_WhenVersionsDifferent()
        {
            // Arrange
            var leftVersion = new DebianPackageVersion("1.2.3");
            object? rightVersion = new DebianPackageVersion("1.3.5");

            // Act
            var result = leftVersion.Equals(rightVersion);

            // Assert
            result.Should().BeFalse();
        }
    }

    public class Equals_DebianPackageVersion
    {
        [Fact]
        public void ShouldReturnTrue_WhenReferencesEqual()
        {
            // Arrange
            var leftVersion = new DebianPackageVersion("1.2.3");
            var rightVersion = leftVersion;

            // Act
            var result = leftVersion.Equals(rightVersion);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldReturnTrue_WhenVersionsEqual()
        {
            // Arrange
            var leftVersion = new DebianPackageVersion("1.2.3");
            var rightVersion = new DebianPackageVersion("1.2.3");

            // Act
            var result = leftVersion.Equals(rightVersion);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldReturnFalse_WhenVersionsDifferent()
        {
            // Arrange
            var leftVersion = new DebianPackageVersion("1.2.3");
            var rightVersion = new DebianPackageVersion("1.3.5");

            // Act
            var result = leftVersion.Equals(rightVersion);

            // Assert
            result.Should().BeFalse();
        }
    }

    public class GetHashCodeMethod
    {
        [Fact]
        public void ShouldReturnTrue_WhenHashCodesNotChanged()
        {
            // Arrange
            var debianVersion = DebianPackageVersion.Parse("2:1.2.3~alpha1-42");

            // Act
            var hash1 = debianVersion.GetHashCode();
            var hash2 = debianVersion.GetHashCode();

            // Assert
            hash1.Should().Be(hash2);
        }

        [Fact]
        public void ShouldReturnTrueWithEqualHashCode_WhenVersionsEqual()
        {
            // Arrange
            var debianVersion1 = DebianPackageVersion.Parse("2:1.2.3~alpha1-42");
            var debianVersion2 = DebianPackageVersion.Parse("2:1.2.3~alpha1-42");

            // Act
            var equalsResult = debianVersion1.Equals(debianVersion2);
            var debianVersion1Hash = debianVersion1.GetHashCode();
            var debianVersion2Hash = debianVersion2.GetHashCode();

            // Assert
            equalsResult.Should().BeTrue();
            debianVersion1Hash.Should().Be(debianVersion2Hash);
        }

        [Fact]
        public void ShouldReturnFalseWithDifferentHashCode_WhenVersionsNotEqual()
        {
            // Arrange
            var debianVersion1 = DebianPackageVersion.Parse("2:1.2.3~alpha1-42");
            var debianVersion2 = DebianPackageVersion.Parse("2:1.4.3~alpha1-42");

            // Act
            var equalsResult = debianVersion1.Equals(debianVersion2);
            var debianVersion1Hash = debianVersion1.GetHashCode();
            var debianVersion2Hash = debianVersion2.GetHashCode();

            // Assert
            equalsResult.Should().BeFalse();
            debianVersion1Hash.Should().NotBe(debianVersion2Hash);
        }

        [Fact]
        public void ShouldReturnTrue_WhenHashSetContainsDebianPackageVersion()
        {
            // Arrange
            var debianVersion1 = DebianPackageVersion.Parse("2:1.2.3~alpha1-42");
            var debianVersion2 = DebianPackageVersion.Parse("2:1.2.3~alpha1-42");

            var hashSet = new HashSet<DebianPackageVersion>
            {
                debianVersion1
            };

            // Act
            var result = hashSet.Contains(debianVersion2);

            // Assert
            result.Should().BeTrue();
        }
    }

    public class ToStringMethod
    {
        [Theory]
        [InlineData("", "1.2.3", "")]
        [InlineData("1", "1.2.3", "")]
        [InlineData("", "1.2.3", "2")]
        [InlineData("3", "1.2.3", "4")]
        public void ShouldReturnExpectedString_WhenCalled(
            string epoch,
            string upstream,
            string revision)
        {
            // Arrange
            var expectedEpoch = string.IsNullOrWhiteSpace(epoch)
                ? DefaultEpoch
                : epoch;

            var expectedEpochString = string.IsNullOrWhiteSpace(epoch)
                ? string.Empty
                : $"{epoch}:";

            var expectedUpstream = upstream;

            var expectedRevision = string.IsNullOrWhiteSpace(revision)
                ? DefaultDebianRevision
                : revision;

            var expectedRevisionString = string.IsNullOrWhiteSpace(revision)
                ? string.Empty
                : $"-{revision}";

            var expectedVersionString = $"{expectedEpochString}{upstream}{expectedRevisionString}";
            var debianPackageVersion = DebianPackageVersion.Parse(expectedVersionString);

            // Act
            var result = debianPackageVersion.ToString();

            // Assert
            debianPackageVersion.Epoch.Should().Be(expectedEpoch);
            debianPackageVersion.UpstreamVersion.Should().Be(expectedUpstream);
            debianPackageVersion.DebianRevision.Should().Be(expectedRevision);
            result.Should().Be(expectedVersionString);
        }
    }

    public class EqualsOperator : DebianPackageVersionTests
    {
        [Fact]
        public void ShouldReturnTrue_WhenVersionsNull()
        {
            // Arrange
            DebianPackageVersion? leftVersion = null;
            DebianPackageVersion? rightVersion = null;

            // Act
#pragma warning disable CA1508
            var result = leftVersion == rightVersion;
#pragma warning restore CA1508

            // Assert
            result.Should().BeTrue();
        }

        [Theory]
        [MemberData(nameof(SameVersionsSampleData))]
        public void ShouldReturnTrue_WhenVersionsEqual(
            string version1,
            string version2)
        {
            // Act
            var result = DebianPackageVersion.Parse(version1) == DebianPackageVersion.Parse(version2);

            // Assert
            result.Should().BeTrue();
        }

        [Theory]
        [MemberData(nameof(DifferentVersionsSampleData))]
        public void ShouldReturnFalse_WhenVersionsDifferent(
            string version1,
            string version2)
        {
            // Act
            var result = DebianPackageVersion.Parse(version1) == DebianPackageVersion.Parse(version2);

            // Assert
            result.Should().BeFalse();
        }
    }

    public class NotEqualsOperator : DebianPackageVersionTests
    {
        [Fact]
        public void ShouldReturnTrue_WhenOneVersionNull()
        {
            // Act
            var result1 = null != DebianPackageVersion.Parse("1.2.3");
            var result2 = DebianPackageVersion.Parse("1.2.3") != null;

            // Assert
            result1.Should().BeTrue();
            result2.Should().BeTrue();
        }

        [Theory]
        [MemberData(nameof(DifferentVersionsSampleData))]
        public void ShouldReturnTrue_WhenVersionsDifferent(
            string version1,
            string version2)
        {
            // Act
            var result = DebianPackageVersion.Parse(version1) != DebianPackageVersion.Parse(version2);

            // Assert
            result.Should().BeTrue();
        }

        [Theory]
        [MemberData(nameof(SameVersionsSampleData))]
        public void ShouldReturnFalse_WhenVersionsEqual(
            string version1,
            string version2)
        {
            // Act
            var result = DebianPackageVersion.Parse(version1) != DebianPackageVersion.Parse(version2);

            // Assert
            result.Should().BeFalse();
        }
    }

    public class LessThanOperator : DebianPackageVersionTests
    {
        [Fact]
        public void ShouldReturnTrue_WhenLeftVersionNull()
        {
            // Act
            var result = null < DebianPackageVersion.Parse("1.2.3");

            // Assert
            result.Should().BeTrue();
        }

        [Theory]
        [MemberData(nameof(DifferentVersionsSampleData))]
        public void ShouldReturnTrue_WhenLeftVersionSmaller(
            string version1,
            string version2)
        {
            // Act
            var result = DebianPackageVersion.Parse(version1) < DebianPackageVersion.Parse(version2);

            // Assert
            result.Should().BeTrue();
        }

        [Theory]
        [MemberData(nameof(SameVersionsSampleData))]
        public void ShouldReturnFalse_WhenVersionsEqual(
            string version1,
            string version2)
        {
            // Act
            var result = DebianPackageVersion.Parse(version1) < DebianPackageVersion.Parse(version2);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenRightVersionNull()
        {
            // Act
            var result = DebianPackageVersion.Parse("1.2.3") < null;

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [MemberData(nameof(DifferentVersionsSampleData))]
        public void ShouldReturnFalse_WhenRightVersionSmaller(
            string version1,
            string version2)
        {
            // Act
            var result = DebianPackageVersion.Parse(version2) < DebianPackageVersion.Parse(version1);

            // Assert
            result.Should().BeFalse();
        }
    }

    public class GreaterThanOperator : DebianPackageVersionTests
    {
        [Fact]
        public void ShouldReturnTrue_WhenRightVersionNull()
        {
            // Act
            var result = DebianPackageVersion.Parse("1.2.3") > null;

            // Assert
            result.Should().BeTrue();
        }

        [Theory]
        [MemberData(nameof(DifferentVersionsSampleData))]
        public void ShouldReturnTrue_WhenLeftVersionGreater(
            string version1,
            string version2)
        {
            // Act
            var result = DebianPackageVersion.Parse(version2) > DebianPackageVersion.Parse(version1);

            // Assert
            result.Should().BeTrue();
        }

        [Theory]
        [MemberData(nameof(SameVersionsSampleData))]
        public void ShouldReturnFalse_WhenVersionsEqual(
            string version1,
            string version2)
        {
            // Act
            var result = DebianPackageVersion.Parse(version1) > DebianPackageVersion.Parse(version2);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenLeftVersionNull()
        {
            // Act
            var result = null > DebianPackageVersion.Parse("1.2.3");

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [MemberData(nameof(DifferentVersionsSampleData))]
        public void ShouldReturnFalse_WhenLeftVersionSmaller(
            string version1,
            string version2)
        {
            // Act
            var result = DebianPackageVersion.Parse(version1) > DebianPackageVersion.Parse(version2);

            // Assert
            result.Should().BeFalse();
        }
    }

    public class LessThanOrEqualOperator : DebianPackageVersionTests
    {
        [Fact]
        public void ShouldReturnTrue_WhenLeftVersionNull()
        {
            // Act
            var result1 = null <= DebianPackageVersion.Parse("1.2.3");
            var result2 = null <= (DebianPackageVersion?)null;

            // Assert
            result1.Should().BeTrue();
            result2.Should().BeTrue();
        }

        [Theory]
        [MemberData(nameof(DifferentVersionsSampleData))]
        [MemberData(nameof(SameVersionsSampleData))]
        public void ShouldReturnTrue_WhenLeftVersionSmallerOrEqual(
            string version1,
            string version2)
        {
            // Act
            var result = DebianPackageVersion.Parse(version1) <= DebianPackageVersion.Parse(version2);

            // Assert
            result.Should().BeTrue();
        }

        [Theory]
        [MemberData(nameof(DifferentVersionsSampleData))]
        public void ShouldReturnFalse_WhenRightVersionSmaller(
            string version1,
            string version2)
        {
            // Act
            var result = DebianPackageVersion.Parse(version2) <= DebianPackageVersion.Parse(version1);

            // Assert
            result.Should().BeFalse();
        }
    }

    public class GreaterThanOrEqualOperator : DebianPackageVersionTests
    {
        [Fact]
        public void ShouldReturnTrue_WhenRightVersionNull()
        {
            // Act
            var result1 = DebianPackageVersion.Parse("1.2.3") >= null;
            var result2 = (DebianPackageVersion?)null >= null;

            // Assert
            result1.Should().BeTrue();
            result2.Should().BeTrue();
        }

        [Theory]
        [MemberData(nameof(DifferentVersionsSampleData))]
        [MemberData(nameof(SameVersionsSampleData))]
        public void ShouldReturnTrue_WhenLeftVersionGreaterOrEqual(
            string version1,
            string version2)
        {
            // Act
            var result = DebianPackageVersion.Parse(version2) >= DebianPackageVersion.Parse(version1);

            // Assert
            result.Should().BeTrue();
        }

        [Theory]
        [MemberData(nameof(DifferentVersionsSampleData))]
        public void ShouldReturnFalse_WhenLeftVersionSmaller(
            string version1,
            string version2)
        {
            // Act
            var result = DebianPackageVersion.Parse(version1) >= DebianPackageVersion.Parse(version2);

            // Assert
            result.Should().BeFalse();
        }
    }
}

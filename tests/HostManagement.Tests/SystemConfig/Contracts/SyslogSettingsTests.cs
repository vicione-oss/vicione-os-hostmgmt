using AwesomeAssertions;
using HostManagement.SystemConfig.Contracts;
using Xunit;

namespace HostManagement.Tests.SystemConfig.Contracts;

public sealed class SyslogSettingsTests
{
    public sealed class EqualsMethod
    {
        [Fact]
        public void ShouldReturnTrue_WhenIdenticalSettings()
        {
            // Arrange
            var settings1 = CreateSampleSettings();
            var settings2 = CreateSampleSettings();

            // Act
            var result = settings1.Equals(settings2);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldReturnTrue_WhenEmptySettings()
        {
            // Arrange
            var settings1 = new SyslogSettings();
            var settings2 = new SyslogSettings();

            // Act
            var result = settings1.Equals(settings2);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldReturnTrue_WhenDifferentExcludeSyslogFacilitiesOrder()
        {
            // Arrange
            var settings1 = new SyslogSettings
            {
                ExcludeSyslogFacilities = [SyslogFacility.Auth, SyslogFacility.Daemon]
            };
            var settings2 = new SyslogSettings
            {
                ExcludeSyslogFacilities = [SyslogFacility.Daemon, SyslogFacility.Auth]
            };

            // Act
            var result = settings1.Equals(settings2);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldReturnTrue_WhenDifferentExcludeSyslogLevelsOrder()
        {
            // Arrange
            var settings1 = new SyslogSettings
            {
                ExcludeSyslogLevels = [SyslogLevel.Alert, SyslogLevel.Debug]
            };
            var settings2 = new SyslogSettings
            {
                ExcludeSyslogLevels = [SyslogLevel.Debug, SyslogLevel.Alert]
            };

            // Act
            var result = settings1.Equals(settings2);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDifferentEnabled()
        {
            // Arrange
            var settings1 = CreateSampleSettings();
            var settings2 = CreateSampleSettings();
            settings2.Enabled = false;

            // Act
            var result = settings1.Equals(settings2);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenNullVsNonNullEnabled()
        {
            // Arrange
            var settings1 = new SyslogSettings { Enabled = null };
            var settings2 = new SyslogSettings { Enabled = true };

            // Act
            var result = settings1.Equals(settings2);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDifferentAddress()
        {
            // Arrange
            var settings1 = CreateSampleSettings();
            var settings2 = CreateSampleSettings();
            settings2.Address = "239.0.0.1:6000";

            // Act
            var result = settings1.Equals(settings2);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenNullVsNonNullAddress()
        {
            // Arrange
            var settings1 = new SyslogSettings { Address = null };
            var settings2 = new SyslogSettings { Address = "192.168.1.100:514" };

            // Act
            var result = settings1.Equals(settings2);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDifferentTransportProtocol()
        {
            // Arrange
            var settings1 = CreateSampleSettings();
            var settings2 = CreateSampleSettings();
            settings2.TransportProtocol = SyslogTransportProtocol.Udp;

            // Act
            var result = settings1.Equals(settings2);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenNullVsNonNullTransportProtocol()
        {
            // Arrange
            var settings1 = new SyslogSettings { TransportProtocol = null };
            var settings2 = new SyslogSettings { TransportProtocol = SyslogTransportProtocol.Tcp };

            // Act
            var result = settings1.Equals(settings2);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDifferentLogFormat()
        {
            // Arrange
            var settings1 = CreateSampleSettings();
            var settings2 = CreateSampleSettings();
            settings2.LogFormat = SyslogLogFormat.Rfc3164;

            // Act
            var result = settings1.Equals(settings2);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenNullVsNonNullLogFormat()
        {
            // Arrange
            var settings1 = new SyslogSettings { LogFormat = null };
            var settings2 = new SyslogSettings { LogFormat = SyslogLogFormat.Rfc5424 };

            // Act
            var result = settings1.Equals(settings2);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDifferentConnectionRetrySeconds()
        {
            // Arrange
            var settings1 = CreateSampleSettings();
            var settings2 = CreateSampleSettings();
            settings2.ConnectionRetrySeconds = 42;

            // Act
            var result = settings1.Equals(settings2);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenNullVsNonNullConnectionRetrySeconds()
        {
            // Arrange
            var settings1 = new SyslogSettings { ConnectionRetrySeconds = null };
            var settings2 = new SyslogSettings { ConnectionRetrySeconds = 21 };

            // Act
            var result = settings1.Equals(settings2);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDifferentExcludeSyslogFacilities()
        {
            // Arrange
            var settings1 = CreateSampleSettings();
            var settings2 = CreateSampleSettings();
            settings2.ExcludeSyslogFacilities!.Add(SyslogFacility.SolarisCron);

            // Act
            var result = settings1.Equals(settings2);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenNullVsNonNullExcludeSyslogFacilities()
        {
            // Arrange
            var settings1 = new SyslogSettings { ExcludeSyslogFacilities = null };
            var settings2 = new SyslogSettings { ExcludeSyslogFacilities = [SyslogFacility.Auth] };

            // Act
            var result = settings1.Equals(settings2);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDifferentExcludeSyslogLevels()
        {
            // Arrange
            var settings1 = CreateSampleSettings();
            var settings2 = CreateSampleSettings();
            settings2.ExcludeSyslogLevels!.Add(SyslogLevel.Info);

            // Act
            var result = settings1.Equals(settings2);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenNullVsNonNullExcludeSyslogLevels()
        {
            // Arrange
            var settings1 = new SyslogSettings { ExcludeSyslogLevels = null };
            var settings2 = new SyslogSettings { ExcludeSyslogLevels = [SyslogLevel.Alert] };

            // Act
            var result = settings1.Equals(settings2);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalse_WhenNull()
        {
            // Arrange
            var settings = CreateSampleSettings();

            // Act
            var result = settings.Equals(null);

            // Assert
            result.Should().BeFalse();
        }
    }

    public sealed class GetHashCodeMethod
    {
        [Fact]
        public void ShouldReturnSameHashCode_WhenIdenticalSettings()
        {
            // Arrange
            var settings1 = CreateSampleSettings();
            var settings2 = CreateSampleSettings();

            // Act
            var hash1 = settings1.GetHashCode();
            var hash2 = settings2.GetHashCode();

            // Assert
            hash1.Should().Be(hash2);
        }

        [Fact]
        public void ShouldReturnDifferentHashCode_WhenDifferentSettings()
        {
            // Arrange
            var settings1 = CreateSampleSettings();
            var settings2 = CreateSampleSettings();
            settings2.Enabled = false;

            // Act
            var hash1 = settings1.GetHashCode();
            var hash2 = settings2.GetHashCode();

            // Assert
            hash1.Should().NotBe(hash2);
        }
    }

    private static SyslogSettings CreateSampleSettings()
    {
        return new SyslogSettings
        {
            Enabled = true,
            Address = "192.168.1.100:514",
            TransportProtocol = SyslogTransportProtocol.Tcp,
            LogFormat = SyslogLogFormat.Rfc5424,
            ConnectionRetrySeconds = 21,
            ExcludeSyslogFacilities = [SyslogFacility.Auth, SyslogFacility.Daemon],
            ExcludeSyslogLevels = [SyslogLevel.Alert, SyslogLevel.Debug]
        };
    }
}

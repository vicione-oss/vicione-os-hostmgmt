using AwesomeAssertions;
using HostManagement.Shared.Contracts.Syslog;
using HostManagement.Shared.Validation;
using HostManagement.Shared.Validation.Validators.Syslog;
using Xunit;

namespace HostManagement.Shared.Tests.Validation.Validators.Syslog;

public sealed class SyslogAddressValidatorTests
{
    [Fact]
    public async Task ShouldReturnValidationError_WhenValidationCancelled()
    {
        // Arrange
        var expectedValidationEntry = new ValidationEntry(ValidationConstants.CancelledResult);

        // Act
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        var result = await new SyslogAddressValidator()
            .ValidateAsync(ValidationTestData.Syslog.IPv4Address, SyslogTransportProtocol.Udp, cts.Token);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public async Task ShouldReturnValidationError_WhenAddressEmptyOrWhitespace(string address)
    {
        // Act
        var result = await new SyslogAddressValidator()
            .ValidateAsync(address, SyslogTransportProtocol.Udp, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
    }

    [Fact]
    public async Task ShouldReturnValidationError_WhenAddressContainsControlCharacter()
    {
        // Arrange
        const string Address = "logs.example.com:514\nProtocol=tcp";

        // Act
        var result = await new SyslogAddressValidator()
            .ValidateAsync(Address, SyslogTransportProtocol.Udp, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
    }

    [Theory]
    [InlineData("logs.example.com")]
    [InlineData("logs.example.com:")]
    [InlineData(":514")]
    [InlineData("2001:db8::42:514")]
    [InlineData("[2001:db8::42:514")]
    [InlineData("2001:db8::42]:514")]
    [InlineData("[192.168.42.10]:514")]
    [InlineData("[logs.example.com]:514")]
    [InlineData(ValidationTestData.Syslog.InvalidIPv4Address)]
    public async Task ShouldReturnValidationError_WhenAddressFormatInvalid(string address)
    {
        // Act
        var result = await new SyslogAddressValidator()
            .ValidateAsync(address, SyslogTransportProtocol.Udp, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().BeGreaterThanOrEqualTo(1);
    }

    [Theory]
    [InlineData("logs.example.com:0")]
    [InlineData("logs.example.com:65536")]
    [InlineData("logs.example.com:not-a-port")]
    public async Task ShouldReturnValidationError_WhenPortInvalid(string address)
    {
        // Act
        var result = await new SyslogAddressValidator()
            .ValidateAsync(address, SyslogTransportProtocol.Udp, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
    }

    [Fact]
    public async Task ShouldReturnValidationError_WhenMulticastUsedWithTcp()
    {
        // Act
        var result = await new SyslogAddressValidator()
            .ValidateAsync(
                ValidationTestData.Syslog.MulticastIPv4Address,
                SyslogTransportProtocol.Tcp,
                CancellationToken.None);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
    }

    [Theory]
    [InlineData(ValidationTestData.Syslog.IPv4Address, SyslogTransportProtocol.Udp)]
    [InlineData(ValidationTestData.Syslog.IPv4Address, SyslogTransportProtocol.Tcp)]
    [InlineData(ValidationTestData.Syslog.IPv6Address, SyslogTransportProtocol.Tcp)]
    [InlineData(ValidationTestData.Syslog.FQDNAddress, SyslogTransportProtocol.Tcp)]
    [InlineData(ValidationTestData.Syslog.SimpleHostnameAddress, SyslogTransportProtocol.Udp)]
    [InlineData(ValidationTestData.Syslog.MulticastIPv4Address, SyslogTransportProtocol.Udp)]
    public async Task ShouldReturnValid_WhenAddressValid(string address, SyslogTransportProtocol transportProtocol)
    {
        // Act
        var result = await new SyslogAddressValidator()
            .ValidateAsync(address, transportProtocol, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Entries.Count.Should().Be(0);
    }
}

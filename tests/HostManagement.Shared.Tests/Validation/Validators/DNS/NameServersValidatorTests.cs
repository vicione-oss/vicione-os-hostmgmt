using System.Net;
using FluentAssertions;
using Xunit;
using HostManagement.Shared.Validation;
using HostManagement.Shared.Validation.Validators.DNS;

namespace HostManagement.Shared.Tests.Validation.Validators.DNS;

public class NameServersValidatorTests
{
    [Fact]
    public async Task ShouldReturnValidationError_WhenValidationCancelled()
    {
        // Arrange
        var expectedValidationEntry = new ValidationEntry(ValidationConstants.CancelledResult);

        // Act
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var result = await new NameServersValidator().ValidateAsync([], cts.Token);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldReturnValidationError_WhenNoNameServersSpecified()
    {
        // Arrange
        var expectedValidationEntry = new ValidationEntry($"No name servers specified.");

        // Act
        var result = await new NameServersValidator().ValidateAsync([]);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Fact]
    public async Task ShouldReturnValidationError_WhenDuplicateNameServersFound()
    {
        // Arrange
        var duplicateNameServer = ValidationTestData.NameServer.IPv4;
        var nameServers = new List<IPAddress>
        {
            IPAddress.Parse(ValidationTestData.NameServer.IPv4),
            IPAddress.Parse(duplicateNameServer)
        };

        var expectedValidationEntry = new ValidationEntry(
            $"Name servers have duplicate entries. Affected entries are: '{duplicateNameServer}'.");

        // Act
        var result = await new NameServersValidator().ValidateAsync(nameServers);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.Should().Contain(expectedValidationEntry);
    }

    [Theory]
    [InlineData(ValidationTestData.NameServer.InvalidLinkLocalIPv4, false)]
    [InlineData(ValidationTestData.NameServer.IPv6, true)]
    public async Task ShouldReturnValidationResult_WhenNameServerValidatorCalled(
        string nameServer,
        bool isValid)
    {
        // Arrange
        var nameServers = new List<IPAddress>()
        {
            IPAddress.Parse(ValidationTestData.NameServer.IPv4),
            IPAddress.Parse(nameServer)
        };

        // Act
        var result = await new NameServersValidator().ValidateAsync(nameServers);

        // Assert
        result.IsValid.Should().Be(isValid);
        result.EntriesString.Should().NotContain(ValidationConstants.CancelledResult);
    }
}

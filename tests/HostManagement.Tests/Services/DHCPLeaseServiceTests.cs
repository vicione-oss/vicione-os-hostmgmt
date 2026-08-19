using System.Net;
using AwesomeAssertions;
using HostManagement.Services;
using HostManagement.Shared.Communication.Enums;
using HostManagement.SystemConfig.Contracts;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace HostManagement.Tests.Services;

public sealed class DHCPLeaseServiceTests
{
    private const string InterfaceName = "lan1";
    private const string DHCPLeaseWasEmpty = "DHCP lease was empty.";

    private static readonly IPAddress s_sampleIPAddress = IPAddress.Parse("120.86.52.18");
    private static readonly DateTimeOffset s_sampleDateTimeOffset =
        new(2025, 06, 23, 12, 34, 56, offset: TimeSpan.FromHours(2));

    public sealed class GetDHCPLeaseInformation
    {
        private const string GetDHCPLeaseInformationFailed = "Getting DHCP lease failed.";
        private const string GetDHCPLeaseInformationCanceled = "Getting DHCP lease was canceled.";

        [Fact]
        public async Task ShouldLogAndReturnErrorResult_WhenExceptionOccurs()
        {
            // Arrange
            var logger = GetLoggerSubstitute();
            var expectedException = new InvalidOperationException("Unexpected Exception");

            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler
                .ExecuteGetInterfaceIPv4DHCPLeaseScript(Arg.Any<string>())
                .ThrowsAsync(expectedException);

            var sut = new DHCPLeaseService(logger, scriptHandler);

            // Act
            var result = await sut.GetDHCPLeaseInformation(InterfaceName, CancellationToken.None);

            // Assert
            logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(state => state.ToString()!.Contains(GetDHCPLeaseInformationFailed)),
                null,
                Arg.Any<Func<object, Exception?, string>>());

            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().Contain(GetDHCPLeaseInformationFailed);
        }

        [Fact]
        public async Task ShouldLogAndReturnWarningResult_WhenCancelled()
        {
            // Arrange
            var logger = GetLoggerSubstitute();
            var scriptHandler = Substitute.For<IScriptHandler>();
            var sut = new DHCPLeaseService(logger, scriptHandler);

            // Act
            using var cts = new CancellationTokenSource();
            await cts.CancelAsync();
            var result = await sut.GetDHCPLeaseInformation(InterfaceName, cts.Token);

            // Assert
            logger.Received(1).Log(
                LogLevel.Warning,
                Arg.Any<EventId>(),
                Arg.Is<object>(state => state.ToString()!.Contains(GetDHCPLeaseInformationCanceled)),
                null,
                Arg.Any<Func<object, Exception?, string>>());

            result.Status.Should().Be(OperationStatus.Warning);
            result.Message.Should().Contain(GetDHCPLeaseInformationCanceled);
        }

        [Fact]
        public async Task ShouldLogAndReturnErrorResult_WhenGetInterfaceIPv4DHCPLeaseReturnsError()
        {
            // Arrange
            var logger = GetLoggerSubstitute();

            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler
                .ExecuteGetInterfaceIPv4DHCPLeaseScript(Arg.Any<string>())
                .Returns(Task.FromResult((Result.ErrorResult, new DHCPLease())));

            var sut = new DHCPLeaseService(logger, scriptHandler);

            // Act
            var result = await sut.GetDHCPLeaseInformation(InterfaceName, CancellationToken.None);

            // Assert
            var expectedLogError = Result.ErrorMessage;
            logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(state =>
                    state.ToString()!.StartsWith(GetDHCPLeaseInformationFailed) &&
                    state.ToString()!.Contains(expectedLogError)),
                null,
                Arg.Any<Func<object, Exception?, string>>());

            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().Contain(GetDHCPLeaseInformationFailed);
            result.DHCPLease.Should().BeNull();
        }

        [Fact]
        public async Task ShouldLogAndReturnErrorResult_WhenLeaseObtainedIsNull()
        {
            // Arrange
            var logger = GetLoggerSubstitute();

            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler
                .ExecuteGetInterfaceIPv4DHCPLeaseScript(Arg.Any<string>())
                .Returns(Task.FromResult((Result.SuccessResult, new DHCPLease())));

            var sut = new DHCPLeaseService(logger, scriptHandler);

            // Act
            var result = await sut.GetDHCPLeaseInformation(InterfaceName, CancellationToken.None);

            // Assert
            logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(state =>
                    state.ToString()!.StartsWith(GetDHCPLeaseInformationFailed) &&
                    state.ToString()!.Contains(DHCPLeaseWasEmpty)),
                null,
                Arg.Any<Func<object, Exception?, string>>());

            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().Contain(GetDHCPLeaseInformationFailed);
            result.DHCPLease.Should().BeNull();
        }

        [Fact]
        public async Task ShouldReturnSuccessResult_WhenGetInterfaceIPv4DHCPLeaseReturnsSuccess()
        {
            // Arrange
            var logger = GetLoggerSubstitute();

            var dhcpLease = new DHCPLease
            {
                LeaseObtained = s_sampleDateTimeOffset,
                LeaseExpires = s_sampleDateTimeOffset,
                Gateway = s_sampleIPAddress,
                NetworkDNSSettings = new(),
                IPv4Detail = new() { IPAddress = s_sampleIPAddress, Netmask = s_sampleIPAddress }
            };
            var sharedDhcpLease = new Shared.Contracts.Network.DHCPLease
            {
                LeaseObtained = s_sampleDateTimeOffset,
                LeaseExpires = s_sampleDateTimeOffset,
                Gateway = s_sampleIPAddress,
                NetworkDNSSettings = new(),
                IPv4Detail = new() { IPAddress = s_sampleIPAddress, Netmask = s_sampleIPAddress }
            };

            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler
                .ExecuteGetInterfaceIPv4DHCPLeaseScript(Arg.Any<string>())
                .Returns(Task.FromResult((Result.SuccessResult, dhcpLease)));

            var sut = new DHCPLeaseService(logger, scriptHandler);

            // Act
            var result = await sut.GetDHCPLeaseInformation(InterfaceName, CancellationToken.None);

            // Assert
            logger.ReceivedCalls().Should().BeEmpty();

            result.Status.Should().Be(OperationStatus.Success);
            result.Message.Should().BeEmpty();
            result.DHCPLease.Should().Be(sharedDhcpLease);
        }
    }

    public sealed class RenewDHCPLease
    {
        private const string RenewDHCPLeaseFailed = "Renew DHCP lease failed.";
        private const string RenewDHCPLeaseCanceled = "Renew DHCP lease was canceled.";

        [Fact]
        public async Task ShouldLogAndReturnErrorResult_WhenExceptionOccurs()
        {
            // Arrange
            var logger = GetLoggerSubstitute();
            var expectedException = new InvalidOperationException("Unexpected Exception");

            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler
                .ExecuteIsNetworkInterfaceAvailableScript(Arg.Any<string>())
                .ThrowsAsync(expectedException);

            var sut = new DHCPLeaseService(logger, scriptHandler);

            // Act
            var result = await sut.RenewDHCPLease(InterfaceName, CancellationToken.None);

            // Assert
            logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(state =>
                    state.ToString()!.StartsWith(RenewDHCPLeaseFailed) &&
                    state.ToString()!.Contains(expectedException.Message)),
                null,
                Arg.Any<Func<object, Exception?, string>>());

            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().Be(RenewDHCPLeaseFailed);
            result.DHCPLease.Should().BeNull();
        }

        [Fact]
        public async Task ShouldLogAndReturnWarningResult_WhenCancelled()
        {
            // Arrange
            var logger = GetLoggerSubstitute();
            var scriptHandler = Substitute.For<IScriptHandler>();
            var sut = new DHCPLeaseService(logger, scriptHandler);

            // Act
            using var cts = new CancellationTokenSource();
            await cts.CancelAsync();
            var result = await sut.RenewDHCPLease(InterfaceName, cts.Token);

            // Assert
            logger.Received(1).Log(
                LogLevel.Warning,
                Arg.Any<EventId>(),
                Arg.Is<object>(state => state.ToString()!.Contains(RenewDHCPLeaseCanceled)),
                null,
                Arg.Any<Func<object, Exception?, string>>());

            result.Status.Should().Be(OperationStatus.Warning);
            result.Message.Should().Be(RenewDHCPLeaseCanceled);
            result.DHCPLease.Should().BeNull();
        }

        [Fact]
        public async Task ShouldLogAndReturnErrorResult_WhenNetworkInterfaceIsUnavailable()
        {
            // Arrange
            var logger = GetLoggerSubstitute();

            var scriptHandler = Substitute.For<IScriptHandler>();
            scriptHandler
                .ExecuteIsNetworkInterfaceAvailableScript(Arg.Any<string>())
                .Returns(Task.FromResult(Result.ErrorResult));

            var sut = new DHCPLeaseService(logger, scriptHandler);

            // Act
            var result = await sut.RenewDHCPLease(InterfaceName, CancellationToken.None);

            // Assert
            var expectedLogError = Result.ErrorMessage;
            logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(state =>
                    state.ToString()!.StartsWith(RenewDHCPLeaseFailed) &&
                    state.ToString()!.Contains(expectedLogError)),
                null,
                Arg.Any<Func<object, Exception?, string>>());

            await scriptHandler.DidNotReceive().ExecuteRenewDHCPLeaseScript(Arg.Any<string>());

            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().Be(RenewDHCPLeaseFailed);
            result.DHCPLease.Should().BeNull();
        }

        [Fact]
        public async Task ShouldLogAndReturnErrorResult_WhenRenewDHCPLeaseScriptReturnsError()
        {
            // Arrange
            var logger = GetLoggerSubstitute();
            var scriptHandler = Substitute.For<IScriptHandler>();

            scriptHandler
                .ExecuteIsNetworkInterfaceAvailableScript(Arg.Any<string>())
                .Returns(Task.FromResult(Result.SuccessResult));

            scriptHandler
                .ExecuteRenewDHCPLeaseScript(Arg.Any<string>())
                .Returns(Task.FromResult(Result.ErrorResult));

            var sut = new DHCPLeaseService(logger, scriptHandler);

            // Act
            var result = await sut.RenewDHCPLease(InterfaceName, CancellationToken.None);

            // Assert
            var expectedLogError = Result.ErrorMessage;
            logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(state =>
                    state.ToString()!.StartsWith(RenewDHCPLeaseFailed) &&
                    state.ToString()!.Contains(expectedLogError)),
                null,
                Arg.Any<Func<object, Exception?, string>>());

            await scriptHandler.DidNotReceive().ExecuteGetInterfaceIPv4DHCPLeaseScript(Arg.Any<string>());

            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().Be(RenewDHCPLeaseFailed);
            result.DHCPLease.Should().BeNull();
        }

        [Fact]
        public async Task ShouldLogAndReturnErrorResult_WhenDHCPLeaseRemainsEmptyAfterRetries()
        {
            // Arrange
            var logger = GetLoggerSubstitute();
            var scriptHandler = Substitute.For<IScriptHandler>();

            scriptHandler
                .ExecuteIsNetworkInterfaceAvailableScript(Arg.Any<string>())
                .Returns(Task.FromResult(Result.SuccessResult));

            scriptHandler
                .ExecuteRenewDHCPLeaseScript(Arg.Any<string>())
                .Returns(Task.FromResult(Result.SuccessResult));

            scriptHandler
                .ExecuteGetInterfaceIPv4DHCPLeaseScript(Arg.Any<string>())
                .Returns(Task.FromResult((Result.SuccessResult, new DHCPLease())));

            var sut = new DHCPLeaseService(logger, scriptHandler);

            // Act
            var result = await sut.RenewDHCPLease(InterfaceName, CancellationToken.None);

            // Assert
            logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(state =>
                    state.ToString()!.StartsWith(RenewDHCPLeaseFailed) &&
                    state.ToString()!.Contains(DHCPLeaseWasEmpty)),
                null,
                Arg.Any<Func<object, Exception?, string>>());

            await scriptHandler.Received(3).ExecuteGetInterfaceIPv4DHCPLeaseScript(InterfaceName);

            result.Status.Should().Be(OperationStatus.Error);
            result.Message.Should().Be(RenewDHCPLeaseFailed);
            result.DHCPLease.Should().BeNull();
        }

        [Fact]
        public async Task ShouldReturnSuccessResult_WhenDHCPLeaseIsRenewedSuccessfully()
        {
            // Arrange
            var logger = GetLoggerSubstitute();
            var scriptHandler = Substitute.For<IScriptHandler>();

            var dhcpLease = new DHCPLease
            {
                LeaseObtained = s_sampleDateTimeOffset,
                LeaseExpires = s_sampleDateTimeOffset,
                Gateway = s_sampleIPAddress,
                NetworkDNSSettings = new(),
                IPv4Detail = new() { IPAddress = s_sampleIPAddress, Netmask = s_sampleIPAddress }
            };
            var sharedDHCPLease = new Shared.Contracts.Network.DHCPLease
            {
                LeaseObtained = s_sampleDateTimeOffset,
                LeaseExpires = s_sampleDateTimeOffset,
                Gateway = s_sampleIPAddress,
                NetworkDNSSettings = new(),
                IPv4Detail = new() { IPAddress = s_sampleIPAddress, Netmask = s_sampleIPAddress }
            };

            scriptHandler
                .ExecuteIsNetworkInterfaceAvailableScript(Arg.Any<string>())
                .Returns(Task.FromResult(Result.SuccessResult));

            scriptHandler
                .ExecuteRenewDHCPLeaseScript(Arg.Any<string>())
                .Returns(Task.FromResult(Result.SuccessResult));

            scriptHandler
                .ExecuteGetInterfaceIPv4DHCPLeaseScript(Arg.Any<string>())
                .Returns(Task.FromResult((Result.SuccessResult, dhcpLease)));

            var sut = new DHCPLeaseService(logger, scriptHandler);

            // Act
            var result = await sut.RenewDHCPLease(InterfaceName, CancellationToken.None);

            // Assert
            logger.ReceivedCalls().Should().BeEmpty();
            await scriptHandler.Received(1).ExecuteIsNetworkInterfaceAvailableScript(InterfaceName);
            await scriptHandler.Received(1).ExecuteRenewDHCPLeaseScript(InterfaceName);
            await scriptHandler.Received(1).ExecuteGetInterfaceIPv4DHCPLeaseScript(InterfaceName);

            result.Status.Should().Be(OperationStatus.Success);
            result.Message.Should().BeEmpty();
            result.DHCPLease.Should().Be(sharedDHCPLease);
        }
    }

    private static ILogger GetLoggerSubstitute()
    {
        var logger = Substitute.For<ILogger>();
        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        return logger;
    }
}

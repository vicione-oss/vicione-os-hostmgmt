using AwesomeAssertions;
using HostManagement.Shared.Communication.Enums;
using HostManagement.SystemConfig.Contracts;
using HostManagement.SystemConfig.Network;
using NSubstitute;
using Xunit;

namespace HostManagement.Tests.SystemConfig.Network;

public class NetworkConfigurationAdapterTests
{
    private static NetworkConfigurationAdapter CreateSut(SystemConfiguration current, IScriptHandler handler, bool bypassHasChanges = false)
        => new(current, handler, bypassHasChanges, false);

    public sealed class HasChanges
    {
        [Fact]
        public void ShouldBeFalse_WhenEqual()
        {
            var handler = Substitute.For<IScriptHandler>();
            SystemConfiguration current = new();
            SystemConfiguration proposed = new();
            var sut = CreateSut(current, handler);

            sut.HasChanges(current, proposed).Should().BeFalse();
        }

        [Fact]
        public void ShouldBeTrue_WhenInterfacesDifferent()
        {
            var handler = Substitute.For<IScriptHandler>();
            SystemConfiguration current = new();
            SystemConfiguration proposed = new()
            {
                NetworkInterfacesSettings = new()
                {
                    NetworkInterfaces =
                    [
                        new()
                        {
                            CommonInformation = new() { Enabled = true, Name = "eth0" },
                            IPv4 = new(),
                        },
                    ],
                }
            };
            var sut = CreateSut(current, handler);

            sut.HasChanges(current, proposed).Should().BeTrue();
        }

        [Fact]
        public void ShouldBeTrue_WhenDNSDifferent()
        {
            var handler = Substitute.For<IScriptHandler>();
            SystemConfiguration current = new();
            SystemConfiguration proposed = new()
            {
                NetworkDNSSettings = new()
                {
                    Hostname = "other",
                },
            };
            var sut = CreateSut(current, handler);

            sut.HasChanges(current, proposed).Should().BeTrue();
        }

        [Fact]
        public void ShouldBeTrue_WhenBypassFlagEnabled()
        {
            var handler = Substitute.For<IScriptHandler>();
            SystemConfiguration current = new();
            SystemConfiguration proposed = new();
            var sut = CreateSut(current, handler, bypassHasChanges: true);

            sut.HasChanges(current, proposed).Should().BeTrue();
        }
    }

    public sealed class SetConfiguration
    {
        [Fact]
        public async Task ShouldApplyNetplan_WhenAnySubAdapterChanged()
        {
            var handler = Substitute.For<IScriptHandler>();
            SystemConfiguration current = new();
            SystemConfiguration proposed = new()
            {
                NetworkDNSSettings = new()
                {
                    Hostname = "new", // force change
                },
            };
            var sut = CreateSut(current, handler);

            var status = await sut.SetConfiguration(proposed);

            status.Should().Be(OperationStatus.Success);
            await handler.Received(1).ExecuteApplyNetplanConfigurationScript();
        }

        [Fact]
        public async Task ShouldNotApplyNetplan_WhenNoChanges()
        {
            var handler = Substitute.For<IScriptHandler>();
            handler.ExecuteApplyNetplanConfigurationScript().Returns(Task.FromResult(OperationStatus.Success));
            SystemConfiguration current = new();
            SystemConfiguration proposed = new();
            var sut = CreateSut(current, handler);

            var status = await sut.SetConfiguration(proposed);

            status.Should().Be(OperationStatus.Success);
            await handler.DidNotReceive().ExecuteApplyNetplanConfigurationScript();
        }
    }
}

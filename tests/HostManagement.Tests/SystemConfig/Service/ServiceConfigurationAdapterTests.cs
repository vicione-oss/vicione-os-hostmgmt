using AwesomeAssertions;
using HostManagement.Scripts;
using HostManagement.Shared.Communication.Enums;
using HostManagement.SystemConfig.Contracts;
using HostManagement.SystemConfig.Service;
using NSubstitute;
using Xunit;

namespace HostManagement.Tests.SystemConfig.Service;

public class ServiceConfigurationAdapterTests
{
    private static ServiceConfigurationAdapter CreateSut(IScriptHandler handler, IReadOnlyCollection<string>? configurableServices = default,
        bool bypassHasChanges = false, bool bypassApply = false)
        => new(handler, configurableServices ?? [], bypassHasChanges, bypassApply);

    private static ServiceDetail CreateDetail(string name, ServiceState state) => new() { Name = name, State = state };

    private const string Service1 = "svc1.service";
    private const string Service2 = "svc2.service";
    private const string Service3 = "svc3.service";

    public sealed class GetConfiguration
    {
        [Fact]
        public async Task ShouldReturnError_WhenAnyScriptFails()
        {
            var handler = Substitute.For<IScriptHandler>();
            handler.ExecuteGetSystemdUnitStateScript(Arg.Any<ScriptOptions>())
                .Returns(
                    (Result.ErrorResult, CreateDetail(Service1, ServiceState.Disabled)),
                    (Result.SuccessResult, CreateDetail(Service2, ServiceState.Enabled)));

            var sut = CreateSut(handler, [Service1, Service2,]);
            var config = new SystemConfiguration();

            var result = await sut.GetConfiguration(config, CancellationToken.None);

            result.OperationStatus.Should().Be(OperationStatus.Error);
            result.Messages.Should().Contain(Result.ErrorMessage);
            config.Services.Should().ContainSingle().Which.Name.Should().Be(Service2);
        }

        [Fact]
        public async Task ShouldReturnError_WhenErrorOccursAfterWarning()
        {
            var handler = Substitute.For<IScriptHandler>();
            handler.ExecuteGetSystemdUnitStateScript(Arg.Any<ScriptOptions>())
                .Returns(
                    (Result.WarningResult, CreateDetail(Service1, ServiceState.Enabled)),
                    (Result.ErrorResult, CreateDetail(Service2, ServiceState.Disabled)),
                    (Result.SuccessResult, CreateDetail(Service3, ServiceState.Enabled)));

            var sut = CreateSut(handler, [Service1, Service2, Service3,]);
            var config = new SystemConfiguration();

            var result = await sut.GetConfiguration(config, CancellationToken.None);

            result.OperationStatus.Should().Be(OperationStatus.Error);
            result.Messages.Should().ContainInOrder(Result.WarningMessage, Result.ErrorMessage);
            config.Services.Should().HaveCount(2)
                .And.Contain(d => d.Name == Service1)
                .And.Contain(d => d.Name == Service3);
        }

        [Fact]
        public async Task ShouldReturnWarning_WhenAnyScriptWarnsAndNoErrors()
        {
            var handler = Substitute.For<IScriptHandler>();
            handler.ExecuteGetSystemdUnitStateScript(Arg.Any<ScriptOptions>())
                .Returns(
                    (Result.SuccessResult, CreateDetail(Service1, ServiceState.Enabled)),
                    (Result.WarningResult, CreateDetail(Service2, ServiceState.Disabled)));

            var sut = CreateSut(handler, [Service1, Service2,]);
            var config = new SystemConfiguration();

            var result = await sut.GetConfiguration(config, CancellationToken.None);

            result.OperationStatus.Should().Be(OperationStatus.Warning);
            result.Messages.Should().Contain(Result.WarningMessage);
            config.Services.Should().HaveCount(2)
                .And.Contain(d => d.Name == Service1)
                .And.Contain(d => d.Name == Service2);
        }

        [Fact]
        public async Task ShouldReturnSuccess_WhenAllScriptsSucceed()
        {
            var handler = Substitute.For<IScriptHandler>();
            handler.ExecuteGetSystemdUnitStateScript(Arg.Any<ScriptOptions>())
                .Returns(
                    (Result.SuccessResult, CreateDetail(Service1, ServiceState.Enabled)),
                    (Result.SuccessResult, CreateDetail(Service2, ServiceState.Disabled)));

            var sut = CreateSut(handler, [Service1, Service2,]);
            var config = new SystemConfiguration();

            var result = await sut.GetConfiguration(config, CancellationToken.None);

            result.OperationStatus.Should().Be(OperationStatus.Success);
            result.Messages.Should().BeEmpty();
            config.Services.Should().HaveCount(2)
                .And.Contain(d => d.Name == Service1)
                .And.Contain(d => d.Name == Service2);
        }

        [Fact]
        public async Task ShouldPopulateOnlyNonWildcardServices()
        {
            var handler = Substitute.For<IScriptHandler>();
            handler.ExecuteGetSystemdUnitStateScript(Arg.Any<ScriptOptions>())
                .Returns((Result.SuccessResult, CreateDetail("plain.service", ServiceState.Enabled)));

            var sut = CreateSut(handler, ["plain.service", "wild@*.service",]);
            var config = new SystemConfiguration();

            var result = await sut.GetConfiguration(config, CancellationToken.None);

            result.OperationStatus.Should().Be(OperationStatus.Success);
            result.Messages.Should().BeEmpty();
            config.Services.Should().ContainSingle().Which.Name.Should().Be("plain.service");
            await handler.Received(1).ExecuteGetSystemdUnitStateScript(Arg.Is<ScriptOptions>(so => so.Options.Contains("plain.service")));
            await handler.DidNotReceive().ExecuteGetSystemdUnitStateScript(Arg.Is<ScriptOptions>(so => so.Options.Contains("wild@*.service")));
        }

        [Fact]
        public async Task ShouldReturnSuccess_WhenConfigurableServicesIsEmpty()
        {
            var handler = Substitute.For<IScriptHandler>();
            var sut = CreateSut(handler, []);
            var config = new SystemConfiguration();

            var result = await sut.GetConfiguration(config, CancellationToken.None);

            result.OperationStatus.Should().Be(OperationStatus.Success);
            result.Messages.Should().BeEmpty();
            config.Services.Should().BeNull();
            await handler.DidNotReceive().ExecuteGetSystemdUnitStateScript(Arg.Any<ScriptOptions>());
        }

        [Fact]
        public async Task ShouldReturnSuccess_WhenOnlyWildcardServicesGiven()
        {
            var handler = Substitute.For<IScriptHandler>();
            var sut = CreateSut(handler, ["wild@*.service", "another@*.service",]);
            var config = new SystemConfiguration();

            var result = await sut.GetConfiguration(config, CancellationToken.None);

            result.OperationStatus.Should().Be(OperationStatus.Success);
            result.Messages.Should().BeEmpty();
            config.Services.Should().BeNull();
            await handler.DidNotReceive().ExecuteGetSystemdUnitStateScript(Arg.Any<ScriptOptions>());
        }

        [Fact]
        public async Task ShouldFilterWildcards_AtDifferentPositions()
        {
            var handler = Substitute.For<IScriptHandler>();
            handler.ExecuteGetSystemdUnitStateScript(Arg.Any<ScriptOptions>())
                .Returns((Result.SuccessResult, CreateDetail("normal.service", ServiceState.Enabled)));

            var sut = CreateSut(handler, ["normal.service", "*@service.service", "service@*", "*wildcard*",]);
            var config = new SystemConfiguration();

            var result = await sut.GetConfiguration(config, CancellationToken.None);

            result.OperationStatus.Should().Be(OperationStatus.Success);
            result.Messages.Should().BeEmpty();
            config.Services.Should().ContainSingle().Which.Name.Should().Be("normal.service");
            await handler.Received(1).ExecuteGetSystemdUnitStateScript(Arg.Any<ScriptOptions>());
        }

        [Fact]
        public async Task ShouldInitializeServicesList_WhenNull()
        {
            var handler = Substitute.For<IScriptHandler>();
            handler.ExecuteGetSystemdUnitStateScript(Arg.Any<ScriptOptions>())
                .Returns((Result.SuccessResult, CreateDetail(Service1, ServiceState.Enabled)));

            var sut = CreateSut(handler, [Service1,]);
            var config = new SystemConfiguration() { Services = null, };

            var result = await sut.GetConfiguration(config, CancellationToken.None);

            result.OperationStatus.Should().Be(OperationStatus.Success);
            result.Messages.Should().BeEmpty();
            config.Services.Should().NotBeNull()
                .And.ContainSingle().Which.Name.Should().Be(Service1);
        }

        [Fact]
        public async Task ShouldPassCorrectScriptOptions()
        {
            var handler = Substitute.For<IScriptHandler>();
            ScriptOptions? capturedOptions = null;
            handler.ExecuteGetSystemdUnitStateScript(Arg.Do<ScriptOptions>(so => capturedOptions = so))
                .Returns((Result.SuccessResult, CreateDetail(Service1, ServiceState.Enabled)));

            var sut = CreateSut(handler, [Service1,]);
            var config = new SystemConfiguration();

            await sut.GetConfiguration(config, CancellationToken.None);

            capturedOptions.Should().NotBeNull();
            capturedOptions.Options.Should().Contain(ScriptOptionNames.GetSystemdUnitState.Name)
                .And.Contain(Service1);
        }
    }

    public sealed class HasChanges
    {
        [Fact]
        public void ShouldBeFalse_WhenServicesEqualIgnoringOrder()
        {
            var handler = Substitute.For<IScriptHandler>();
            var sut = CreateSut(handler);
            SystemConfiguration current = new()
            {
                Services =
                [
                    CreateDetail(Service1, ServiceState.Enabled),
                    CreateDetail(Service2, ServiceState.Disabled),
                ],
            };
            SystemConfiguration proposed = new()
            {
                Services =
                [
                    CreateDetail(Service2, ServiceState.Disabled),
                    CreateDetail(Service1, ServiceState.Enabled),
                ],
            };

            sut.HasChanges(current, proposed).Should().BeFalse();
        }

        [Fact]
        public void ShouldBeTrue_WhenServicesDifferent()
        {
            var handler = Substitute.For<IScriptHandler>();
            var sut = CreateSut(handler);
            SystemConfiguration current = new() { Services = [CreateDetail(Service1, ServiceState.Enabled),], };
            SystemConfiguration proposed = new() { Services = [CreateDetail(Service1, ServiceState.Disabled),], };

            sut.HasChanges(current, proposed).Should().BeTrue();
        }

        [Fact]
        public void ShouldBeTrue_WhenDifferentNumberOfServices()
        {
            var handler = Substitute.For<IScriptHandler>();
            var sut = CreateSut(handler);
            SystemConfiguration current = new()
            {
                Services =
                [
                    CreateDetail(Service1, ServiceState.Enabled),
                ],
            };
            SystemConfiguration proposed = new()
            {
                Services =
                [
                    CreateDetail(Service1, ServiceState.Enabled),
                    CreateDetail(Service2, ServiceState.Enabled),
                ],
            };

            sut.HasChanges(current, proposed).Should().BeTrue();
        }

        [Fact]
        public void ShouldBeFalse_WhenBothServiceListsAreEmpty()
        {
            var handler = Substitute.For<IScriptHandler>();
            var sut = CreateSut(handler);
            SystemConfiguration current = new();
            SystemConfiguration proposed = new();

            sut.HasChanges(current, proposed).Should().BeFalse();
        }

        [Fact]
        public void ShouldBeTrue_WhenOnlyCurrentIsEmpty()
        {
            var handler = Substitute.For<IScriptHandler>();
            var sut = CreateSut(handler);
            SystemConfiguration current = new();
            SystemConfiguration proposed = new()
            {
                Services = [CreateDetail(Service1, ServiceState.Enabled),],
            };

            sut.HasChanges(current, proposed).Should().BeTrue();
        }

        [Fact]
        public void ShouldBeTrue_WhenOnlyProposedIsEmpty()
        {
            var handler = Substitute.For<IScriptHandler>();
            var sut = CreateSut(handler);
            SystemConfiguration current = new()
            {
                Services = [CreateDetail(Service1, ServiceState.Enabled),],
            };
            SystemConfiguration proposed = new();

            sut.HasChanges(current, proposed).Should().BeTrue();
        }

        [Fact]
        public void ShouldBeTrue_WhenSameNameButDifferentState()
        {
            var handler = Substitute.For<IScriptHandler>();
            var sut = CreateSut(handler);
            SystemConfiguration current = new()
            {
                Services = [CreateDetail(Service1, ServiceState.Enabled),],
            };
            SystemConfiguration proposed = new()
            {
                Services = [CreateDetail(Service1, ServiceState.Disabled),],
            };

            sut.HasChanges(current, proposed).Should().BeTrue();
        }

        [Fact]
        public void ShouldBeTrue_WhenBypassFlagEnabled()
        {
            var handler = Substitute.For<IScriptHandler>();
            var sut = CreateSut(handler, bypassHasChanges: true);
            SystemConfiguration current = new();
            SystemConfiguration proposed = new();

            sut.HasChanges(current, proposed).Should().BeTrue();
        }
    }

    public sealed class SetConfiguration
    {
        [Fact]
        public async Task ShouldReturnError_WhenAnyScriptFails()
        {
            var handler = Substitute.For<IScriptHandler>();
            handler.ExecuteSetSystemdUnitStateScript(Arg.Any<ScriptOptions>())
                .Returns(Result.SuccessResult, Result.ErrorResult);

            var sut = CreateSut(handler, bypassHasChanges: true);
            var config = new SystemConfiguration()
            {
                Services =
                [
                    CreateDetail(Service1, ServiceState.Enabled),
                    CreateDetail(Service2, ServiceState.Disabled),
                    CreateDetail(Service3, ServiceState.Enabled),
                ],
            };

            var result = await sut.SetConfiguration(config, CancellationToken.None);

            result.OperationStatus.Should().Be(OperationStatus.Error);
            result.Messages.Should().Contain(Result.ErrorMessage);
            await handler.Received(2).ExecuteSetSystemdUnitStateScript(Arg.Any<ScriptOptions>()); // early exit
        }

        [Fact]
        public async Task ShouldReturnWarning_WhenAnyScriptWarnsAndNoErrors()
        {
            var handler = Substitute.For<IScriptHandler>();
            handler.ExecuteSetSystemdUnitStateScript(Arg.Any<ScriptOptions>())
                .Returns(Result.SuccessResult, Result.WarningResult);

            var sut = CreateSut(handler, bypassHasChanges: true);
            var config = new SystemConfiguration()
            {
                Services =
                [
                    CreateDetail(Service1, ServiceState.Enabled),
                    CreateDetail(Service2, ServiceState.Disabled),
                ],
            };

            var result = await sut.SetConfiguration(config, CancellationToken.None);

            result.OperationStatus.Should().Be(OperationStatus.Warning);
            result.Messages.Should().Contain(Result.WarningMessage);
            await handler.Received(2).ExecuteSetSystemdUnitStateScript(Arg.Any<ScriptOptions>());
        }

        [Fact]
        public async Task ShouldReturnSuccess_WhenAllScriptsSucceed()
        {
            var handler = Substitute.For<IScriptHandler>();
            handler.ExecuteSetSystemdUnitStateScript(Arg.Any<ScriptOptions>())
                .Returns(Result.SuccessResult);

            var sut = CreateSut(handler, bypassHasChanges: true);
            var config = new SystemConfiguration()
            {
                Services =
                [
                    CreateDetail(Service1, ServiceState.Enabled),
                    CreateDetail(Service2, ServiceState.Disabled),
                ],
            };

            var result = await sut.SetConfiguration(config, CancellationToken.None);

            result.OperationStatus.Should().Be(OperationStatus.Success);
            result.Messages.Should().BeEmpty();
            await handler.Received(2).ExecuteSetSystemdUnitStateScript(Arg.Any<ScriptOptions>());
        }

        [Fact]
        public async Task ShouldReturnSuccess_WhenServiceListIsEmpty()
        {
            var handler = Substitute.For<IScriptHandler>();
            var sut = CreateSut(handler, bypassHasChanges: true);
            var config = new SystemConfiguration();

            var result = await sut.SetConfiguration(config, CancellationToken.None);

            result.OperationStatus.Should().Be(OperationStatus.Success);
            result.Messages.Should().BeEmpty();
            await handler.DidNotReceive().ExecuteSetSystemdUnitStateScript(Arg.Any<ScriptOptions>());
        }

        [Fact]
        public async Task ShouldReturnSuccess_WhenServiceListIsNull()
        {
            var handler = Substitute.For<IScriptHandler>();
            var sut = CreateSut(handler, bypassHasChanges: true);
            var config = new SystemConfiguration() { Services = null };

            var result = await sut.SetConfiguration(config, CancellationToken.None);

            result.OperationStatus.Should().Be(OperationStatus.Success);
            result.Messages.Should().BeEmpty();
            await handler.DidNotReceive().ExecuteSetSystemdUnitStateScript(Arg.Any<ScriptOptions>());
        }

        [Fact]
        public async Task ShouldCallScript_WithCorrectOptionsForEachService()
        {
            var handler = Substitute.For<IScriptHandler>();
            List<ScriptOptions> captured = [];
            handler.ExecuteSetSystemdUnitStateScript(Arg.Do<ScriptOptions>(captured.Add))
                .Returns(Result.SuccessResult);

            var sut = CreateSut(handler, bypassHasChanges: true);
            var config = new SystemConfiguration()
            {
                Services =
                [
                    CreateDetail(Service1, ServiceState.Enabled),
                    CreateDetail(Service2, ServiceState.Disabled),
                    CreateDetail(Service3, ServiceState.Unknown),
                ],
            };

            var result = await sut.SetConfiguration(config, CancellationToken.None);

            result.OperationStatus.Should().Be(OperationStatus.Success);
            result.Messages.Should().BeEmpty();

            captured.Should().HaveCount(3).And.SatisfyRespectively(
                so => so.Options.Should().ContainInOrder(
                    ScriptOptionNames.SetSystemdUnitState.Name,
                    Service1,
                    ScriptOptionNames.SetSystemdUnitState.State,
                    "Enabled"),
                so => so.Options.Should().ContainInOrder(
                    ScriptOptionNames.SetSystemdUnitState.Name,
                    Service2,
                    ScriptOptionNames.SetSystemdUnitState.State,
                    "Disabled"),
                so => so.Options.Should().ContainInOrder(
                    ScriptOptionNames.SetSystemdUnitState.Name,
                    Service3,
                    ScriptOptionNames.SetSystemdUnitState.State,
                    "Unknown"));
        }

        [Fact]
        public async Task ShouldReturnWarning_WhenMultipleScriptsWarn()
        {
            var handler = Substitute.For<IScriptHandler>();
            handler.ExecuteSetSystemdUnitStateScript(Arg.Any<ScriptOptions>())
                .Returns(Result.WarningResult, Result.WarningResult, Result.SuccessResult);

            var sut = CreateSut(handler, bypassHasChanges: true);
            var config = new SystemConfiguration()
            {
                Services =
                [
                    CreateDetail(Service1, ServiceState.Enabled),
                    CreateDetail(Service2, ServiceState.Disabled),
                    CreateDetail(Service3, ServiceState.Enabled),
                ],
            };

            var result = await sut.SetConfiguration(config, CancellationToken.None);

            result.OperationStatus.Should().Be(OperationStatus.Warning);
            result.Messages.Should().BeEquivalentTo([Result.WarningMessage, Result.WarningMessage]);
            await handler.Received(3).ExecuteSetSystemdUnitStateScript(Arg.Any<ScriptOptions>());
        }
    }
}

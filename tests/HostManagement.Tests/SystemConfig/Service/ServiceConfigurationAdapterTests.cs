using AwesomeAssertions;
using HostManagement.Scripts;
using HostManagement.Shared.Communication.Enums;
using HostManagement.SystemConfig.Contracts;
using HostManagement.SystemConfig.Service;
using NSubstitute;
using Xunit;

namespace HostManagement.Tests.SystemConfig.Service;

public sealed class ServiceConfigurationAdapterTests
{
    private const string Service1 = "svc1.service";
    private const string Service2 = "svc2.service";
    private const string Service3 = "svc3.service";

    private static ServiceConfigurationAdapter CreateSut(
        IScriptHandler handler,
        IReadOnlyCollection<string>? configurableServices = default,
        bool bypassHasChanges = false,
        bool bypassApply = false)
           => new(handler, configurableServices ?? [], bypassHasChanges, bypassApply);

    private static ServiceDetail CreateDetail(string name, ServiceState state) => new() { Name = name, State = state };

    public sealed class GetConfiguration
    {
        [Fact]
        public async Task ShouldReturnError_WhenAnyScriptFails()
        {
            // Arrange
            var handler = Substitute.For<IScriptHandler>();
            handler.ExecuteGetSystemdUnitStateScript(Arg.Any<ScriptOptions>())
                .Returns(
                    (Result.TestErrorResult, CreateDetail(Service1, ServiceState.Disabled)),
                    (Result.SuccessResult, CreateDetail(Service2, ServiceState.Enabled)));

            var config = new SystemConfiguration();
            var sut = CreateSut(handler, [Service1, Service2]);

            // Act
            var result = await sut.GetConfiguration(config, CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Error);
            result.Messages.Should().Contain(Result.TestErrorMessage);
            config.Services.Should().ContainSingle().Which.Name.Should().Be(Service2);
        }

        [Fact]
        public async Task ShouldReturnError_WhenErrorOccursAfterWarning()
        {
            // Arrange
            var handler = Substitute.For<IScriptHandler>();
            handler.ExecuteGetSystemdUnitStateScript(Arg.Any<ScriptOptions>())
                .Returns(
                    (Result.TestWarningResult, CreateDetail(Service1, ServiceState.Enabled)),
                    (Result.TestErrorResult, CreateDetail(Service2, ServiceState.Disabled)),
                    (Result.SuccessResult, CreateDetail(Service3, ServiceState.Enabled)));

            var config = new SystemConfiguration();
            var sut = CreateSut(handler, [Service1, Service2, Service3]);

            // Act
            var result = await sut.GetConfiguration(config, CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Error);
            result.Messages.Should().ContainInOrder(Result.TestWarningMessage, Result.TestErrorMessage);
            config.Services.Should().HaveCount(2)
                .And.Contain(d => d.Name == Service1)
                .And.Contain(d => d.Name == Service3);
        }

        [Fact]
        public async Task ShouldReturnWarning_WhenAnyScriptWarnsAndNoErrors()
        {
            // Arrange
            var handler = Substitute.For<IScriptHandler>();
            handler.ExecuteGetSystemdUnitStateScript(Arg.Any<ScriptOptions>())
                .Returns(
                    (Result.SuccessResult, CreateDetail(Service1, ServiceState.Enabled)),
                    (Result.TestWarningResult, CreateDetail(Service2, ServiceState.Disabled)));

            var config = new SystemConfiguration();
            var sut = CreateSut(handler, [Service1, Service2]);

            // Act
            var result = await sut.GetConfiguration(config, CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Warning);
            result.Messages.Should().Contain(Result.TestWarningMessage);
            config.Services.Should().HaveCount(2)
                .And.Contain(d => d.Name == Service1)
                .And.Contain(d => d.Name == Service2);
        }

        [Fact]
        public async Task ShouldReturnSuccess_WhenAllScriptsSucceed()
        {
            // Arrange
            var handler = Substitute.For<IScriptHandler>();
            handler.ExecuteGetSystemdUnitStateScript(Arg.Any<ScriptOptions>())
                .Returns(
                    (Result.SuccessResult, CreateDetail(Service1, ServiceState.Enabled)),
                    (Result.SuccessResult, CreateDetail(Service2, ServiceState.Disabled)));

            var config = new SystemConfiguration();
            var sut = CreateSut(handler, [Service1, Service2]);

            // Act
            var result = await sut.GetConfiguration(config, CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Success);
            result.Messages.Should().BeEmpty();
            config.Services.Should().HaveCount(2)
                .And.Contain(d => d.Name == Service1)
                .And.Contain(d => d.Name == Service2);
        }

        [Fact]
        public async Task ShouldPopulateOnlyNonWildcardServices_WhenWildcardServicesConfigured()
        {
            // Arrange
            var handler = Substitute.For<IScriptHandler>();
            handler.ExecuteGetSystemdUnitStateScript(Arg.Any<ScriptOptions>())
                .Returns((Result.SuccessResult, CreateDetail("plain.service", ServiceState.Enabled)));

            var config = new SystemConfiguration();
            var sut = CreateSut(handler, ["plain.service", "wild@*.service"]);

            // Act
            var result = await sut.GetConfiguration(config, CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Success);
            result.Messages.Should().BeEmpty();
            config.Services.Should().ContainSingle().Which.Name.Should().Be("plain.service");

            await handler.Received(1)
                .ExecuteGetSystemdUnitStateScript(Arg.Is<ScriptOptions>(so => so.Options.Contains("plain.service")));
            await handler.DidNotReceive()
                .ExecuteGetSystemdUnitStateScript(Arg.Is<ScriptOptions>(so => so.Options.Contains("wild@*.service")));
        }

        [Fact]
        public async Task ShouldReturnSuccess_WhenConfigurableServicesIsEmpty()
        {
            // Arrange
            var handler = Substitute.For<IScriptHandler>();
            var config = new SystemConfiguration();
            var sut = CreateSut(handler, []);

            // Act
            var result = await sut.GetConfiguration(config, CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Success);
            result.Messages.Should().BeEmpty();
            config.Services.Should().BeNull();

            await handler.DidNotReceive().ExecuteGetSystemdUnitStateScript(Arg.Any<ScriptOptions>());
        }

        [Fact]
        public async Task ShouldReturnSuccess_WhenOnlyWildcardServicesGiven()
        {
            // Arrange
            var handler = Substitute.For<IScriptHandler>();
            var config = new SystemConfiguration();
            var sut = CreateSut(handler, ["wild@*.service", "another@*.service"]);

            // Act
            var result = await sut.GetConfiguration(config, CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Success);
            result.Messages.Should().BeEmpty();
            config.Services.Should().BeNull();

            await handler.DidNotReceive().ExecuteGetSystemdUnitStateScript(Arg.Any<ScriptOptions>());
        }

        [Fact]
        public async Task ShouldFilterWildcardServices_WhenWildcardsAtDifferentPositions()
        {
            // Arrange
            var handler = Substitute.For<IScriptHandler>();
            handler.ExecuteGetSystemdUnitStateScript(Arg.Any<ScriptOptions>())
                .Returns((Result.SuccessResult, CreateDetail("normal.service", ServiceState.Enabled)));

            var config = new SystemConfiguration();
            var sut = CreateSut(handler, ["normal.service", "*@service.service", "service@*", "*wildcard*"]);

            // Act
            var result = await sut.GetConfiguration(config, CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Success);
            result.Messages.Should().BeEmpty();
            config.Services.Should().ContainSingle().Which.Name.Should().Be("normal.service");

            await handler.Received(1).ExecuteGetSystemdUnitStateScript(Arg.Any<ScriptOptions>());
        }

        [Fact]
        public async Task ShouldInitializeServicesList_WhenServicesListNull()
        {
            // Arrange
            var handler = Substitute.For<IScriptHandler>();
            handler.ExecuteGetSystemdUnitStateScript(Arg.Any<ScriptOptions>())
                .Returns((Result.SuccessResult, CreateDetail(Service1, ServiceState.Enabled)));

            var config = new SystemConfiguration() { Services = null };
            var sut = CreateSut(handler, [Service1]);

            // Act
            var result = await sut.GetConfiguration(config, CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Success);
            result.Messages.Should().BeEmpty();
            config.Services.Should().NotBeNull()
                .And.ContainSingle().Which.Name.Should().Be(Service1);
        }

        [Fact]
        public async Task ShouldPassExpectedScriptOptions_WhenGettingServiceState()
        {
            // Arrange
            ScriptOptions? capturedOptions = null;

            var handler = Substitute.For<IScriptHandler>();
            handler.ExecuteGetSystemdUnitStateScript(Arg.Do<ScriptOptions>(so => capturedOptions = so))
                .Returns((Result.SuccessResult, CreateDetail(Service1, ServiceState.Enabled)));

            var config = new SystemConfiguration();
            var sut = CreateSut(handler, [Service1]);

            // Act
            _ = await sut.GetConfiguration(config, CancellationToken.None);

            // Assert
            capturedOptions.Should().NotBeNull();
            capturedOptions.Options.Should().ContainInOrder(ScriptOptionNames.GetSystemdUnitState.Name, Service1);
        }
    }

    public sealed class HasChanges
    {
        [Fact]
        public void ShouldReturnFalse_WhenServicesEqualIgnoringOrder()
        {
            // Arrange
            var handler = Substitute.For<IScriptHandler>();

            var current = new SystemConfiguration()
            {
                Services =
                [
                    CreateDetail(Service1, ServiceState.Enabled),
                    CreateDetail(Service2, ServiceState.Disabled)
                ]
            };
            var proposed = new SystemConfiguration()
            {
                Services =
                [
                    CreateDetail(Service2, ServiceState.Disabled),
                    CreateDetail(Service1, ServiceState.Enabled)
                ]
            };

            var sut = CreateSut(handler);

            // Act
            var result = sut.HasChanges(current, proposed);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnTrue_WhenServicesDiffer()
        {
            // Arrange
            var handler = Substitute.For<IScriptHandler>();
            var current = new SystemConfiguration() { Services = [CreateDetail(Service1, ServiceState.Enabled)] };
            var proposed = new SystemConfiguration() { Services = [CreateDetail(Service1, ServiceState.Disabled)] };
            var sut = CreateSut(handler);

            // Act
            var result = sut.HasChanges(current, proposed);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldReturnTrue_WhenServiceCountsDiffer()
        {
            // Arrange
            var handler = Substitute.For<IScriptHandler>();

            var current = new SystemConfiguration()
            {
                Services =
                [
                    CreateDetail(Service1, ServiceState.Enabled)
                ]
            };
            var proposed = new SystemConfiguration()
            {
                Services =
                [
                    CreateDetail(Service1, ServiceState.Enabled),
                    CreateDetail(Service2, ServiceState.Enabled)
                ]
            };

            var sut = CreateSut(handler);

            // Act
            var result = sut.HasChanges(current, proposed);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldReturnFalse_WhenBothServiceListsEmpty()
        {
            // Arrange
            var handler = Substitute.For<IScriptHandler>();
            var current = new SystemConfiguration();
            var proposed = new SystemConfiguration();
            var sut = CreateSut(handler);

            // Act
            var result = sut.HasChanges(current, proposed);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnTrue_WhenCurrentServiceListEmpty()
        {
            // Arrange
            var handler = Substitute.For<IScriptHandler>();
            var current = new SystemConfiguration();
            var proposed = new SystemConfiguration() { Services = [CreateDetail(Service1, ServiceState.Enabled)] };
            var sut = CreateSut(handler);

            // Act
            var result = sut.HasChanges(current, proposed);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldReturnTrue_WhenProposedServiceListEmpty()
        {
            // Arrange
            var handler = Substitute.For<IScriptHandler>();
            var current = new SystemConfiguration() { Services = [CreateDetail(Service1, ServiceState.Enabled)] };
            var proposed = new SystemConfiguration();
            var sut = CreateSut(handler);

            // Act
            var result = sut.HasChanges(current, proposed);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldReturnTrue_WhenServiceStatesDiffer()
        {
            // Arrange
            var handler = Substitute.For<IScriptHandler>();
            var current = new SystemConfiguration() { Services = [CreateDetail(Service1, ServiceState.Enabled)] };
            var proposed = new SystemConfiguration() { Services = [CreateDetail(Service1, ServiceState.Disabled)] };
            var sut = CreateSut(handler);

            // Act
            var result = sut.HasChanges(current, proposed);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldReturnTrue_WhenChangeCheckBypassed()
        {
            // Arrange
            var handler = Substitute.For<IScriptHandler>();
            var current = new SystemConfiguration();
            var proposed = new SystemConfiguration();
            var sut = CreateSut(handler, bypassHasChanges: true);

            // Act
            var result = sut.HasChanges(current, proposed);

            // Assert
            result.Should().BeTrue();
        }
    }

    public sealed class SetConfiguration
    {
        [Fact]
        public async Task ShouldReturnError_WhenAnyScriptFails()
        {
            // Arrange
            var handler = Substitute.For<IScriptHandler>();
            handler.ExecuteSetSystemdUnitStateScript(Arg.Any<ScriptOptions>())
                .Returns(Result.SuccessResult, Result.TestErrorResult);

            var config = new SystemConfiguration()
            {
                Services =
                [
                    CreateDetail(Service1, ServiceState.Enabled),
                    CreateDetail(Service2, ServiceState.Disabled),
                    CreateDetail(Service3, ServiceState.Enabled)
                ]
            };

            // Act
            var sut = CreateSut(handler, bypassHasChanges: true);

            // Assert
            var result = await sut.SetConfiguration(config, CancellationToken.None);

            result.OperationStatus.Should().Be(OperationStatus.Error);
            result.Messages.Should().Contain(Result.TestErrorMessage);

            await handler.Received(2).ExecuteSetSystemdUnitStateScript(Arg.Any<ScriptOptions>());
        }

        [Fact]
        public async Task ShouldReturnWarning_WhenAnyScriptWarnsAndNoErrors()
        {
            // Arrange
            var handler = Substitute.For<IScriptHandler>();
            handler.ExecuteSetSystemdUnitStateScript(Arg.Any<ScriptOptions>())
                .Returns(Result.SuccessResult, Result.TestWarningResult);

            var config = new SystemConfiguration()
            {
                Services =
                [
                    CreateDetail(Service1, ServiceState.Enabled),
                    CreateDetail(Service2, ServiceState.Disabled)
                ]
            };

            var sut = CreateSut(handler, bypassHasChanges: true);

            // Act
            var result = await sut.SetConfiguration(config, CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Warning);
            result.Messages.Should().Contain(Result.TestWarningMessage);

            await handler.Received(2).ExecuteSetSystemdUnitStateScript(Arg.Any<ScriptOptions>());
        }

        [Fact]
        public async Task ShouldReturnSuccess_WhenAllScriptsSucceed()
        {
            // Arrange
            var handler = Substitute.For<IScriptHandler>();
            handler.ExecuteSetSystemdUnitStateScript(Arg.Any<ScriptOptions>())
                .Returns(Result.SuccessResult);

            var config = new SystemConfiguration()
            {
                Services =
                [
                    CreateDetail(Service1, ServiceState.Enabled),
                    CreateDetail(Service2, ServiceState.Disabled)
                ]
            };

            var sut = CreateSut(handler, bypassHasChanges: true);

            // Act
            var result = await sut.SetConfiguration(config, CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Success);
            result.Messages.Should().BeEmpty();

            await handler.Received(2).ExecuteSetSystemdUnitStateScript(Arg.Any<ScriptOptions>());
        }

        [Fact]
        public async Task ShouldReturnSuccess_WhenServiceListIsEmpty()
        {
            // Arrange
            var handler = Substitute.For<IScriptHandler>();
            var config = new SystemConfiguration();
            var sut = CreateSut(handler, bypassHasChanges: true);

            // Act
            var result = await sut.SetConfiguration(config, CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Success);
            result.Messages.Should().BeEmpty();

            await handler.DidNotReceive().ExecuteSetSystemdUnitStateScript(Arg.Any<ScriptOptions>());
        }

        [Fact]
        public async Task ShouldReturnSuccess_WhenServiceListIsNull()
        {
            // Arrange
            var handler = Substitute.For<IScriptHandler>();
            var config = new SystemConfiguration() { Services = null };
            var sut = CreateSut(handler, bypassHasChanges: true);

            // Act
            var result = await sut.SetConfiguration(config, CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Success);
            result.Messages.Should().BeEmpty();

            await handler.DidNotReceive().ExecuteSetSystemdUnitStateScript(Arg.Any<ScriptOptions>());
        }

        [Fact]
        public async Task ShouldCallScriptWithExpectedOptions_WhenServicesConfigured()
        {
            // Arrange
            List<ScriptOptions> capturedOptions = [];
            var handler = Substitute.For<IScriptHandler>();
            handler.ExecuteSetSystemdUnitStateScript(Arg.Do<ScriptOptions>(capturedOptions.Add))
                .Returns(Result.SuccessResult);

            var config = new SystemConfiguration()
            {
                Services =
                [
                    CreateDetail(Service1, ServiceState.Enabled),
                    CreateDetail(Service2, ServiceState.Disabled),
                    CreateDetail(Service3, ServiceState.Unknown)
                ]
            };

            var sut = CreateSut(handler, bypassHasChanges: true);

            // Act
            var result = await sut.SetConfiguration(config, CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Success);
            result.Messages.Should().BeEmpty();

            capturedOptions.Should().HaveCount(3).And.SatisfyRespectively(
                so => so.Options.Should().ContainInOrder(
                    ScriptOptionNames.SetSystemdUnitState.Name, Service1,
                    ScriptOptionNames.SetSystemdUnitState.State, "Enabled"),
                so => so.Options.Should().ContainInOrder(
                    ScriptOptionNames.SetSystemdUnitState.Name, Service2,
                    ScriptOptionNames.SetSystemdUnitState.State, "Disabled"),
                so => so.Options.Should().ContainInOrder(
                    ScriptOptionNames.SetSystemdUnitState.Name, Service3,
                    ScriptOptionNames.SetSystemdUnitState.State, "Unknown"));
        }

        [Fact]
        public async Task ShouldReturnWarning_WhenMultipleScriptsWarn()
        {
            // Arrange
            var handler = Substitute.For<IScriptHandler>();
            handler.ExecuteSetSystemdUnitStateScript(Arg.Any<ScriptOptions>())
                .Returns(Result.TestWarningResult, Result.TestWarningResult, Result.SuccessResult);

            var config = new SystemConfiguration()
            {
                Services =
                [
                    CreateDetail(Service1, ServiceState.Enabled),
                    CreateDetail(Service2, ServiceState.Disabled),
                    CreateDetail(Service3, ServiceState.Enabled)
                ]
            };

            var sut = CreateSut(handler, bypassHasChanges: true);

            // Act
            var result = await sut.SetConfiguration(config, CancellationToken.None);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Warning);
            result.Messages.Should().BeEquivalentTo([Result.TestWarningMessage, Result.TestWarningMessage]);

            await handler.Received(3).ExecuteSetSystemdUnitStateScript(Arg.Any<ScriptOptions>());
        }
    }
}

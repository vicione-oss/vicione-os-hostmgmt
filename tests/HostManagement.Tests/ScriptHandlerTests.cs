using System.Net.NetworkInformation;
using System.Text.Json;
using AwesomeAssertions;
using HostManagement.Scripts;
using HostManagement.Scripts.Enums;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Contracts.NetworkStatus;
using HostManagement.SystemConfig.Contracts;
using Xunit;

namespace HostManagement.Tests;

public sealed class ScriptHandlerTests
{
    private const string ScriptError = "Script error";
    private const string ScriptWarning = "Script warning";
    private const string InterfaceName = "lan1";
    private const string RawInterfacesString =
        "lan1            UP             00:50:56:36:6d:33 <BROADCAST,MULTICAST,UP,LOWER_UP>";

    public sealed class Constructor
    {
        [Fact]
        public void ShouldThrowArgumentNullException_WhenRunScriptDelegateIsNull()
        {
            // Arrange
            Func<int?, ScriptIdentifier, IEnumerable<string>, Task<ScriptResponse>>? runner = null;

            // Act
            var action = () => _ = new ScriptHandler(runner!);

            // Assert
            action.Should().Throw<ArgumentNullException>().WithParameterName("runScript");
        }
    }

    public sealed class RunnerFailures
    {
        [Fact]
        public async Task ShouldPropagateException_WhenRunnerThrows()
        {
            // Arrange
            var expectedException = new InvalidOperationException("Runner failed.");

            var sut = new ScriptHandler((_, _, _) => Task.FromException<ScriptResponse>(expectedException));

            // Act
            var action = async () => await sut.ExecuteResetSystemScript();

            // Assert
            var exception = await action.Should().ThrowAsync<InvalidOperationException>();
            exception.Which.Should().BeSameAs(expectedException);
        }
    }

    public sealed class ResultHandling
    {
        [Fact]
        public async Task ShouldAddScriptError_WhenScriptReturnsError()
        {
            // Arrange
            var expectedResult = CreateErrorResult(ScriptIdentifier.ResetSystem);
            var sut = CreateSut(CreateErrorResponse());

            // Act
            var result = await sut.ExecuteResetSystemScript();

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

        [Fact]
        public async Task ShouldAddScriptWarning_WhenScriptReturnsWarning()
        {
            // Arrange
            var expectedResult = CreateWarningResult(ScriptIdentifier.ResetSystem);
            var sut = CreateSut(CreateWarningResponse());

            // Act
            var result = await sut.ExecuteResetSystemScript();

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

        [Fact]
        public async Task ShouldIgnoreWarning_WhenConfiguredAndScriptReturnsWarning()
        {
            // Arrange
            var sut = CreateSut(CreateWarningResponse());

            // Act
            var result = await sut.ExecuteApplyNetplanConfigurationScript();

            // Assert
            result.Should().BeEquivalentTo(new Result());
        }

        [Fact]
        public async Task ShouldReturnSuccessResult_WhenScriptSucceeds()
        {
            // Arrange
            var sut = CreateSut(CreateSuccessResponse());

            // Act
            var result = await sut.ExecuteResetSystemScript();

            // Assert
            result.Should().BeEquivalentTo(new Result());
        }
    }

    public sealed class RawOutputHandling
    {
        [Fact]
        public async Task ShouldReturnEmptyOutput_WhenScriptReturnsError()
        {
            // Arrange
            var expectedResult = CreateErrorResult(ScriptIdentifier.OriginalPhysicalAddressGet);
            var sut = CreateSut(CreateErrorResponse());

            // Act
            var (result, standardOutput) = await sut.ExecuteGetOriginalPhysicalAddressScript(InterfaceName);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
            standardOutput.Should().BeEmpty();
        }

        [Fact]
        public async Task ShouldReturnOutputAndWarning_WhenScriptReturnsWarning()
        {
            // Arrange
            const string StandardOutput = "00:11:22:33:44:55";

            var expectedResult = CreateWarningResult(ScriptIdentifier.OriginalPhysicalAddressGet);
            var sut = CreateSut(CreateWarningResponse(standardOutput: StandardOutput));

            // Act
            var (result, standardOutput) = await sut.ExecuteGetOriginalPhysicalAddressScript(InterfaceName);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
            standardOutput.Should().Be(StandardOutput);
        }

        [Fact]
        public async Task ShouldReturnRawOutput_WhenScriptSucceeds()
        {
            // Arrange
            const string StandardOutput = "00:11:22:33:44:55";

            var sut = CreateSut(CreateSuccessResponse(StandardOutput));

            // Act
            var (result, standardOutput) = await sut.ExecuteGetOriginalPhysicalAddressScript(InterfaceName);

            // Assert
            result.Should().BeEquivalentTo(new Result());
            standardOutput.Should().Be(StandardOutput);
        }
    }

    public sealed class SplitOutputHandling
    {
        [Fact]
        public async Task ShouldReturnEmptyCollection_WhenScriptReturnsError()
        {
            // Arrange
            var expectedResult = CreateErrorResult(ScriptIdentifier.NetworkNTPFallbackInformationGet);
            var sut = CreateSut(CreateErrorResponse());

            // Act
            var (result, values) = await sut.ExecuteGetNetworkNTPFallbackInformationScript();

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
            values.Should().BeEmpty();
        }

        [Fact]
        public async Task ShouldReturnEmptyCollection_WhenOutputIsEmpty()
        {
            // Arrange
            var sut = CreateSut(CreateSuccessResponse());

            // Act
            var (result, values) = await sut.ExecuteGetNetworkNTPFallbackInformationScript();

            // Assert
            result.Should().BeEquivalentTo(new Result());
            values.Should().BeEmpty();
        }

        [Fact]
        public async Task ShouldRemoveEmptyEntriesAndTrimValues_WhenOutputContainsWhitespace()
        {
            // Arrange
            var sut = CreateSut(CreateSuccessResponse(
                "  0.debian.pool.ntp.org " +
                "1.debian.pool.ntp.org  " +
                "2.debian.pool.ntp.org   "));

            // Act
            var (result, values) = await sut.ExecuteGetNetworkNTPFallbackInformationScript();

            // Assert
            result.Should().BeEquivalentTo(new Result());
            values.Should().Equal(
                "0.debian.pool.ntp.org",
                "1.debian.pool.ntp.org",
                "2.debian.pool.ntp.org");
        }
    }

    public sealed class SingleTypeDeserialization
    {
        [Fact]
        public async Task ShouldReturnDefaultValue_WhenScriptReturnsError()
        {
            // Arrange
            var expectedResult = CreateErrorResult(ScriptIdentifier.NetworkDNSSettingsGet);
            var sut = CreateSut(CreateErrorResponse());

            // Act
            var (result, settings) = await sut.ExecuteGetNetworkDNSSettingsScript();

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
            settings.Should().BeEquivalentTo(new NetworkDNSSettings());
        }

        [Fact]
        public async Task ShouldThrowInvalidOperationException_WhenOutputContainsInvalidJson()
        {
            // Arrange
            var sut = CreateSut(CreateSuccessResponse("{invalid-json"));

            // Act
            var action = async () => await sut.ExecuteGetNetworkDNSSettingsScript();

            // Assert
            await action
                .Should().ThrowAsync<InvalidOperationException>()
                .WithMessage(
                    $"Standard output could not be deserialized to '{nameof(NetworkDNSSettings)}' " +
                    $"for script with identifier '{ScriptIdentifier.NetworkDNSSettingsGet}'.*");
        }

        [Fact]
        public async Task ShouldThrowInvalidOperationException_WhenOutputDeserializesToNull()
        {
            // Arrange
            var sut = CreateSut(CreateSuccessResponse("null"));

            // Act
            var action = async () => await sut.ExecuteGetNetworkDNSSettingsScript();

            // Assert
            await action
                .Should().ThrowAsync<InvalidOperationException>()
                .WithMessage(
                    $"Standard output could not be deserialized to '{nameof(NetworkDNSSettings)}' " +
                    $"for script with identifier '{ScriptIdentifier.NetworkDNSSettingsGet}'." +
                    Environment.NewLine +
                    "null");
        }

        [Fact]
        public async Task ShouldDeserializePhysicalAddress_WhenCommonInformationOutputContainsMACAddress()
        {
            // Arrange
            const string Json =
                """
                [
                  {
                    "Enabled": true,
                    "Name": "lan1",
                    "UserDefinedMACAddress": {
                      "Enabled": true,
                      "Address": "01-23-45-67-89-ab"
                    }
                  }
                ]
                """;

            var sut = CreateSut(CreateSuccessResponse(Json));

            // Act
            var (result, commonInformation) =
                await sut.ExecuteGetNetworkInterfacesCommonInformationScript(new ScriptOptions());

            // Assert
            result.Should().BeEquivalentTo(new Result());
            commonInformation.Should().ContainSingle();

            var userDefinedMACAddress = commonInformation[0].UserDefinedMACAddress;
            userDefinedMACAddress.Should().NotBeNull();
            userDefinedMACAddress.Address.Should().Be(PhysicalAddress.Parse("01:23:45:67:89:AB"));
        }

        [Fact]
        public async Task ShouldReturnDeserializedValue_WhenOutputContainsValidJson()
        {
            // Arrange
            var expectedSettings = new NetworkDNSSettings() { Hostname = "debian" };

            var json = JsonSerializer.Serialize(
                expectedSettings,
                SystemConfigurationJsonSerializerContext.Default.NetworkDNSSettings);

            var sut = CreateSut(CreateSuccessResponse(json));

            // Act
            var (result, settings) = await sut.ExecuteGetNetworkDNSSettingsScript();

            // Assert
            result.Should().BeEquivalentTo(new Result());
            settings.Should().BeEquivalentTo(expectedSettings);
        }
    }

    public sealed class MultipleTypeDeserialization
    {
        [Fact]
        public async Task ShouldReturnDefaultValues_WhenScriptReturnsError()
        {
            // Arrange
            var expectedResult = CreateErrorResult(ScriptIdentifier.NetworkInterfaceSettingsGet);
            var sut = CreateSut(CreateErrorResponse());

            // Act
            var (result, ipv4Settings, vlanSettings) = await sut.ExecuteGetInterfaceSettingsScript(InterfaceName);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
            ipv4Settings.Should().BeEquivalentTo(new IPv4Settings());
            vlanSettings.Should().BeEquivalentTo(new VLANSettings());
        }

        [Fact]
        public async Task ShouldThrowInvalidOperationException_WhenOutputContainsInvalidJson()
        {
            // Arrange
            var sut = CreateSut(CreateSuccessResponse("{invalid-json"));

            // Act
            var action = async () => await sut.ExecuteGetInterfaceSettingsScript(InterfaceName);

            // Assert
            await action
                .Should().ThrowAsync<InvalidOperationException>()
                .WithMessage(
                    $"Standard output could not be deserialized to " +
                    $"'{nameof(IPv4Settings)} and {nameof(VLANSettings)}' " +
                    $"for script with identifier '{ScriptIdentifier.NetworkInterfaceSettingsGet}'.*")
                .WithInnerException(typeof(JsonException));
        }

        [Fact]
        public async Task ShouldThrowInvalidOperationException_WhenFirstSectionIsMissing()
        {
            // Arrange
            var sut = CreateSut(CreateSuccessResponse(
                """
                {
                    "VLAN": {}
                }
                """));

            // Act
            var action = async () => await sut.ExecuteGetInterfaceSettingsScript(InterfaceName);

            // Assert
            await action
                .Should().ThrowAsync<InvalidOperationException>()
                .WithMessage(
                    $"Standard output could not be deserialized to " +
                    $"'{nameof(IPv4Settings)} and {nameof(VLANSettings)}' " +
                    $"for script with identifier '{ScriptIdentifier.NetworkInterfaceSettingsGet}'." +
                    Environment.NewLine +
                    "Required JSON property 'IPv4' was not found.")
                .WithInnerException(typeof(JsonException));
        }

        [Fact]
        public async Task ShouldThrowInvalidOperationException_WhenFirstSectionContainsNull()
        {
            // Arrange
            var sut = CreateSut(CreateSuccessResponse(
                """
                {
                    "IPv4": null,
                    "VLAN": {}
                }
                """));

            // Act
            var action = async () => await sut.ExecuteGetInterfaceSettingsScript(InterfaceName);

            // Assert
            await action
                .Should().ThrowAsync<InvalidOperationException>()
                .WithMessage(
                    $"Standard output could not be deserialized to " +
                    $"'{nameof(IPv4Settings)} and {nameof(VLANSettings)}' " +
                    $"for script with identifier '{ScriptIdentifier.NetworkInterfaceSettingsGet}'." +
                    Environment.NewLine +
                    "JSON property 'IPv4' contains null.")
                .WithInnerException(typeof(JsonException));
        }

        [Fact]
        public async Task ShouldThrowInvalidOperationException_WhenSecondSectionIsMissing()
        {
            // Arrange
            var ipv4Json = JsonSerializer.Serialize(
                new IPv4Settings(),
                SystemConfigurationJsonSerializerContext.Default.IPv4Settings);

            var standardOutput =
                $$"""
                  {
                      "IPv4": {{ipv4Json}}
                  }
                  """;

            var sut = CreateSut(CreateSuccessResponse(standardOutput));

            // Act
            var action = async () => await sut.ExecuteGetInterfaceSettingsScript(InterfaceName);

            // Assert
            await action
                .Should().ThrowAsync<InvalidOperationException>()
                .WithMessage(
                    $"Standard output could not be deserialized to " +
                    $"'{nameof(IPv4Settings)} and {nameof(VLANSettings)}' " +
                    $"for script with identifier '{ScriptIdentifier.NetworkInterfaceSettingsGet}'." +
                    Environment.NewLine +
                    "Required JSON property 'VLAN' was not found.")
                .WithInnerException(typeof(JsonException));
        }

        [Fact]
        public async Task ShouldThrowInvalidOperationException_WhenSecondSectionContainsNull()
        {
            // Arrange
            var ipv4Json = JsonSerializer.Serialize(
                new IPv4Settings(),
                SystemConfigurationJsonSerializerContext.Default.IPv4Settings);

            var standardOutput =
                $$"""
                  {
                      "IPv4": {{ipv4Json}},
                      "VLAN": null
                  }
                  """;

            var sut = CreateSut(CreateSuccessResponse(standardOutput));

            // Act
            var action = async () => await sut.ExecuteGetInterfaceSettingsScript(InterfaceName);

            // Assert
            await action
                .Should().ThrowAsync<InvalidOperationException>()
                .WithMessage(
                    $"Standard output could not be deserialized to " +
                    $"'{nameof(IPv4Settings)} and {nameof(VLANSettings)}' " +
                    $"for script with identifier '{ScriptIdentifier.NetworkInterfaceSettingsGet}'." +
                    Environment.NewLine +
                    "JSON property 'VLAN' contains null.")
                .WithInnerException(typeof(JsonException));
        }

        [Fact]
        public async Task ShouldReturnDeserializedValues_WhenOutputContainsValidJson()
        {
            // Arrange
            var expectedIPv4Settings = new IPv4Settings() { DHCPEnabled = true };
            var expectedVLANSettings = new VLANSettings() { Enabled = true, ID = 42 };

            var ipv4Json = JsonSerializer.Serialize(
                expectedIPv4Settings,
                SystemConfigurationJsonSerializerContext.Default.IPv4Settings);

            var vlanJson = JsonSerializer.Serialize(
                expectedVLANSettings,
                SystemConfigurationJsonSerializerContext.Default.VLANSettings);

            var standardOutput =
                $$"""
                  {
                      "IPv4": {{ipv4Json}},
                      "VLAN": {{vlanJson}}
                  }
                  """;

            var sut = CreateSut(CreateSuccessResponse(standardOutput));

            // Act
            var (result, ipv4Settings, vlanSettings) = await sut.ExecuteGetInterfaceSettingsScript(InterfaceName);

            // Assert
            result.Should().BeEquivalentTo(new Result());
            ipv4Settings.Should().BeEquivalentTo(expectedIPv4Settings);
            vlanSettings.Should().BeEquivalentTo(expectedVLANSettings);
        }
    }

    public sealed class CustomValueFactoryHandling
    {
        [Fact]
        public async Task ShouldUseCustomValueFactory_WhenNetworkStatusScriptReturnsError()
        {
            // Arrange
            var scriptOptions = CreateScriptOptions(
                "--interface-name", InterfaceName,
                "--interfaces", RawInterfacesString);

            var sut = CreateSut(CreateErrorResponse());

            // Act
            var (result, information) = await sut.ExecuteGetNetworkStatusInformationScript(scriptOptions);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Error);

            information.Should().BeEquivalentTo(NetworkStatusInformation.Empty);
        }

        [Fact]
        public async Task ShouldUseCustomValueFactory_WhenSystemdUnitStateScriptReturnsError()
        {
            // Arrange
            var scriptOptions = CreateScriptOptions("--name", "ssh.service");

            var sut = CreateSut(CreateErrorResponse());

            // Act
            var (result, serviceDetail) = await sut.ExecuteGetSystemdUnitStateScript(scriptOptions);

            // Assert
            result.OperationStatus.Should().Be(OperationStatus.Error);

            serviceDetail.Name.Should().Be("Empty");
            serviceDetail.State.Should().Be(ServiceState.Unknown);
        }
    }

    public sealed class TimeoutHandling
    {
        [Fact]
        public async Task ShouldUseThreeMinuteTimeout_WhenInstallingDebianPackage()
        {
            // Arrange
            const string PackagePath = "package.deb";
            const int ExpectedTimeoutInMilliseconds = 180_000;

            var scriptOptions = CreateScriptOptions(PackagePath);

            Invocation? invocation = null;

            var sut = CreateSut(CreateSuccessResponse(), value => invocation = value);

            // Act
            await sut.ExecuteInstallDebianPackageScript(scriptOptions);

            // Assert
            AssertInvocation(
                invocation,
                ScriptIdentifier.InstallDebianPackage,
                ExpectedTimeoutInMilliseconds,
                PackagePath);
        }
    }

    public sealed class ArgumentRouting
    {
        private const string FirstOptionArgument = "--option";
        private const string SecondOptionArgument = "value";
        private const string Argument = "argument";

        [Fact]
        public async Task ExecuteGetNetworkInterfacesScript_ShouldPassNoArguments_WhenInterfaceTypeIsWhitespace()
        {
            // Arrange
            Invocation? invocation = null;

            var sut = CreateSut(CreateSuccessResponse(), value => invocation = value);

            // Act
            await sut.ExecuteGetNetworkInterfacesScript("   ");

            // Assert
            AssertInvocation(invocation, ScriptIdentifier.NetworkInterfacesGet);
        }

        [Fact]
        public async Task ExecuteGetNetworkInterfacesScript_ShouldPassArgument()
        {
            // Arrange
            Invocation? invocation = null;

            var sut = CreateSut(CreateSuccessResponse(), value => invocation = value);

            // Act
            await sut.ExecuteGetNetworkInterfacesScript(Argument);

            // Assert
            AssertInvocation(invocation, ScriptIdentifier.NetworkInterfacesGet, Argument);
        }

        [Fact]
        public async Task ExecuteIsNetworkInterfaceAvailableScript_ShouldPassArgument()
        {
            // Arrange
            Invocation? invocation = null;

            var sut = CreateSut(CreateSuccessResponse(), value => invocation = value);

            // Act
            await sut.ExecuteIsNetworkInterfaceAvailableScript(Argument);

            // Assert
            AssertInvocation(invocation, ScriptIdentifier.IsNetworkInterfaceAvailable, Argument);
        }

        [Fact]
        public async Task ExecuteGetNetworkInterfacesCommonInformationScript_ShouldPassOptions()
        {
            // Arrange
            var scriptOptions = CreateScriptOptions(FirstOptionArgument, SecondOptionArgument);

            Invocation? invocation = null;

            var sut = CreateSut(CreateErrorResponse(), value => invocation = value);

            // Act
            await sut.ExecuteGetNetworkInterfacesCommonInformationScript(scriptOptions);

            // Assert
            AssertInvocation(
                invocation,
                ScriptIdentifier.NetworkInterfacesCommonInformationGet,
                FirstOptionArgument,
                SecondOptionArgument);
        }

        [Fact]
        public async Task ExecuteSetInterfaceSettingsScript_ShouldPassOptions()
        {
            // Arrange
            var scriptOptions = CreateScriptOptions(FirstOptionArgument, SecondOptionArgument);

            Invocation? invocation = null;

            var sut = CreateSut(CreateSuccessResponse(), value => invocation = value);

            // Act
            await sut.ExecuteSetInterfaceSettingsScript(scriptOptions);

            // Assert
            AssertInvocation(
                invocation,
                ScriptIdentifier.NetworkInterfaceSettingsSet,
                FirstOptionArgument,
                SecondOptionArgument);
        }

        [Fact]
        public async Task ExecuteGetInterfaceIPv4DHCPLeaseScript_ShouldPassArgument()
        {
            // Arrange
            Invocation? invocation = null;

            var sut = CreateSut(CreateErrorResponse(), value => invocation = value);

            // Act
            await sut.ExecuteGetInterfaceIPv4DHCPLeaseScript(Argument);

            // Assert
            AssertInvocation(invocation, ScriptIdentifier.NetworkInterfaceIPv4DHCPLeaseGet, Argument);
        }

        [Fact]
        public async Task ExecuteRenewDHCPLeaseScript_ShouldPassArgument()
        {
            // Arrange
            Invocation? invocation = null;

            var sut = CreateSut(CreateSuccessResponse(), value => invocation = value);

            // Act
            await sut.ExecuteRenewDHCPLeaseScript(Argument);

            // Assert
            AssertInvocation(invocation, ScriptIdentifier.RenewDHCPLease, Argument);
        }

        [Fact]
        public async Task ExecuteSetStaticHostsScript_ShouldPassOptions()
        {
            // Arrange
            var scriptOptions = CreateScriptOptions(FirstOptionArgument, SecondOptionArgument);

            Invocation? invocation = null;

            var sut = CreateSut(CreateSuccessResponse(), value => invocation = value);

            // Act
            await sut.ExecuteSetStaticHostsScript(scriptOptions);

            // Assert
            AssertInvocation(
                invocation,
                ScriptIdentifier.NetworkStaticHostsSet,
                FirstOptionArgument,
                SecondOptionArgument);
        }

        [Fact]
        public async Task ExecuteSetNetworkDNSSettingsScript_ShouldPassOptions()
        {
            // Arrange
            var scriptOptions = CreateScriptOptions(FirstOptionArgument, SecondOptionArgument);

            Invocation? invocation = null;

            var sut = CreateSut(CreateSuccessResponse(), value => invocation = value);

            // Act
            await sut.ExecuteSetNetworkDNSSettingsScript(scriptOptions);

            // Assert
            AssertInvocation(
                invocation,
                ScriptIdentifier.NetworkDNSSettingsSet,
                FirstOptionArgument,
                SecondOptionArgument);
        }

        [Fact]
        public async Task ExecuteSetNetworkMulticastDNSSettingsScript_ShouldPassOptions()
        {
            // Arrange
            var scriptOptions = CreateScriptOptions(FirstOptionArgument, SecondOptionArgument);

            Invocation? invocation = null;

            var sut = CreateSut(CreateSuccessResponse(), value => invocation = value);

            // Act
            await sut.ExecuteSetNetworkMulticastDNSSettingsScript(scriptOptions);

            // Assert
            AssertInvocation(
                invocation,
                ScriptIdentifier.NetworkMulticastDNSSettingsSet,
                FirstOptionArgument,
                SecondOptionArgument);
        }

        [Fact]
        public async Task ExecuteSetHostnameScript_ShouldPassArgument()
        {
            // Arrange
            Invocation? invocation = null;

            var sut = CreateSut(CreateSuccessResponse(), value => invocation = value);

            // Act
            await sut.ExecuteSetHostnameScript(Argument);

            // Assert
            AssertInvocation(invocation, ScriptIdentifier.HostnameSet, Argument);
        }

        [Fact]
        public async Task ExecuteGetNetworkNTPSettingsScript_ShouldPassNoArguments()
        {
            // Arrange
            Invocation? invocation = null;

            var sut = CreateSut(CreateErrorResponse(), value => invocation = value);

            // Act
            await sut.ExecuteGetNetworkNTPSettingsScript();

            // Assert
            AssertInvocation(invocation, ScriptIdentifier.NetworkNTPSettingsGet);
        }

        [Fact]
        public async Task ExecuteSetNetworkNTPSettingsScript_ShouldPassOptions()
        {
            // Arrange
            var scriptOptions = CreateScriptOptions(FirstOptionArgument, SecondOptionArgument);

            Invocation? invocation = null;

            var sut = CreateSut(CreateSuccessResponse(), value => invocation = value);

            // Act
            await sut.ExecuteSetNetworkNTPSettingsScript(scriptOptions);

            // Assert
            AssertInvocation(
                invocation,
                ScriptIdentifier.NetworkNTPSettingsSet,
                FirstOptionArgument,
                SecondOptionArgument);
        }

        [Fact]
        public async Task ExecuteGetNetworkProxySettingsScript_ShouldPassNoArguments()
        {
            // Arrange
            Invocation? invocation = null;

            var sut = CreateSut(CreateErrorResponse(), value => invocation = value);

            // Act
            await sut.ExecuteGetNetworkProxySettingsScript();

            // Assert
            AssertInvocation(invocation, ScriptIdentifier.NetworkProxySettingsGet);
        }

        [Fact]
        public async Task ExecuteSetNetworkProxySettingsScript_ShouldPassOptions()
        {
            // Arrange
            var scriptOptions = CreateScriptOptions(FirstOptionArgument, SecondOptionArgument);

            Invocation? invocation = null;

            var sut = CreateSut(CreateSuccessResponse(), value => invocation = value);

            // Act
            await sut.ExecuteSetNetworkProxySettingsScript(scriptOptions);

            // Assert
            AssertInvocation(
                invocation,
                ScriptIdentifier.NetworkProxySettingsSet,
                FirstOptionArgument,
                SecondOptionArgument);
        }

        [Fact]
        public async Task ExecuteGetSystemdUnitActiveStateScript_ShouldPassOptions()
        {
            // Arrange
            var scriptOptions = CreateScriptOptions(FirstOptionArgument, SecondOptionArgument);

            Invocation? invocation = null;

            var sut = CreateSut(CreateSuccessResponse(), value => invocation = value);

            // Act
            await sut.ExecuteGetSystemdUnitActiveStateScript(scriptOptions);

            // Assert
            AssertInvocation(
                invocation,
                ScriptIdentifier.SystemdUnitActiveStateGet,
                FirstOptionArgument,
                SecondOptionArgument);
        }

        [Fact]
        public async Task ExecuteSetSystemdUnitStateScript_ShouldPassOptions()
        {
            // Arrange
            var scriptOptions = CreateScriptOptions(FirstOptionArgument, SecondOptionArgument);

            Invocation? invocation = null;

            var sut = CreateSut(CreateSuccessResponse(), value => invocation = value);

            // Act
            await sut.ExecuteSetSystemdUnitStateScript(scriptOptions);

            // Assert
            AssertInvocation(
                invocation,
                ScriptIdentifier.SystemdUnitStateSet,
                FirstOptionArgument,
                SecondOptionArgument);
        }

        [Fact]
        public async Task ExecuteUpdateSystemScript_ShouldPassOptions()
        {
            // Arrange
            var scriptOptions = CreateScriptOptions(FirstOptionArgument, SecondOptionArgument);

            Invocation? invocation = null;

            var sut = CreateSut(CreateSuccessResponse(), value => invocation = value);

            // Act
            await sut.ExecuteUpdateSystemScript(scriptOptions);

            // Assert
            AssertInvocation(
                invocation,
                ScriptIdentifier.UpdateSystem,
                FirstOptionArgument,
                SecondOptionArgument);
        }

        [Fact]
        public async Task ExecuteShutdownSystemScript_ShouldPassOptions()
        {
            // Arrange
            var scriptOptions = CreateScriptOptions(FirstOptionArgument, SecondOptionArgument);

            Invocation? invocation = null;

            var sut = CreateSut(CreateSuccessResponse(), value => invocation = value);

            // Act
            await sut.ExecuteShutdownSystemScript(scriptOptions);

            // Assert
            AssertInvocation(
                invocation,
                ScriptIdentifier.ShutdownSystem,
                FirstOptionArgument,
                SecondOptionArgument);
        }
    }

    private static ScriptHandler CreateSut(ScriptResponse response, Action<Invocation>? invocationCallback = null)
    {
        return new ScriptHandler(
            (timeoutInMilliseconds, scriptIdentifier, arguments) =>
            {
                invocationCallback?.Invoke(new Invocation(timeoutInMilliseconds, scriptIdentifier, [.. arguments]));

                return Task.FromResult(response);
            });
    }

    private static ScriptResponse CreateErrorResponse(string standardError = ScriptError)
        => new(1, string.Empty, standardError);

    private static ScriptResponse CreateSuccessResponse(string standardOutput = "")
        => new(0, standardOutput, string.Empty);

    private static ScriptResponse CreateWarningResponse(
        string standardError = ScriptWarning,
        string standardOutput = "")
        => new(0, standardOutput, standardError);

    private static Result CreateErrorResult(ScriptIdentifier scriptIdentifier, string error = ScriptError)
    {
        var result = new Result();
        result.AddScriptError(scriptIdentifier, error);

        return result;
    }

    private static Result CreateWarningResult(ScriptIdentifier scriptIdentifier, string warning = ScriptWarning)
    {
        var result = new Result();
        result.AddScriptWarning(scriptIdentifier, warning);

        return result;
    }

    private static ScriptOptions CreateScriptOptions(params string[] arguments)
    {
        var options = new ScriptOptions();

        foreach (var argument in arguments)
            options.Add(argument);

        return options;
    }

    private static void AssertInvocation(
        Invocation? invocation,
        ScriptIdentifier scriptIdentifier,
        params string[] arguments)
        => AssertInvocation(invocation, scriptIdentifier, timeoutInMilliseconds: null, arguments: arguments);

    private static void AssertInvocation(
        Invocation? invocation,
        ScriptIdentifier scriptIdentifier,
        int? timeoutInMilliseconds,
        params string[] arguments)
    {
        invocation.Should().NotBeNull();
        invocation.ScriptIdentifier.Should().Be(scriptIdentifier);
        invocation.Arguments.Should().Equal(arguments);
        invocation.TimeoutInMilliseconds.Should().Be(timeoutInMilliseconds);
    }

    private sealed record Invocation(
        int? TimeoutInMilliseconds,
        ScriptIdentifier ScriptIdentifier,
        IReadOnlyCollection<string> Arguments);
}

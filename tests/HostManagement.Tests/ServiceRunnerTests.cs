using System.IO.Abstractions;
using System.Reflection;
using AwesomeAssertions;
using HostManagement.Communication.NamedPipe.RequestHandlers;
using HostManagement.Services;
using HostManagement.Shared.Capabilities;
using HostManagement.Shared.Communication;
using HostManagement.Shared.Communication.Capabilities;
using HostManagement.SystemConfig;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace HostManagement.Tests;

[Collection("Non Parallel Tests")]
public sealed class ServiceRunnerTests
{
    public sealed class GetRequestHandlerRegistry
    {
        [Fact]
        public void ShouldRegisterExpectedHandlers_WhenCreatingRegistry()
        {
            // Arrange
            var expectedTopics = new[]
            {
#if DEBUG
                // Testing purposes
                Topics.Echo,
#endif
                Topics.GetSupportedCapabilities,
                Topics.InstallSignedDebianPackage,
                Topics.RedeployScripts,
                Topics.RenewDHCPLease,

                Topics.GetServiceState,
                Topics.StartService,
                Topics.StopService,
                Topics.RestartService,

                Topics.GetOriginalPhysicalAddress,
                Topics.GetNetworkStatusInformation,
                Topics.GetDHCPLeaseInformation,
                Topics.GetNTPFallbackInformation,

                Topics.GetSystemConfiguration,
                Topics.SetSystemConfiguration,
                Topics.ResetNetworkInterface,

                Topics.ResetSystem,
                Topics.RestartSystem,
                Topics.ShutdownSystem,
                Topics.UpdateSystem
            };

            using var capabilitiesService = CreateSupportedCapabilitiesService();
            var configurationService = CreateSystemConfigurationService();

            // Act
            var registry = CreateRequestHandlerRegistry(
                capabilitiesService,
                new SupportedCapabilities(),
                configurationService);

            // Assert
            registry.Handlers
                .Select(handler => handler.Topic)
                .Should().BeEquivalentTo(expectedTopics);
        }

        [Fact]
        public void ShouldUseSharedSystemConfigurationServiceInstance_WhenRegisteringSystemConfigurationHandlers()
        {
            // Arrange
            using var capabilitiesService = CreateSupportedCapabilitiesService();
            var expectedConfigurationService = CreateSystemConfigurationService();

            var registry = CreateRequestHandlerRegistry(
                capabilitiesService,
                new SupportedCapabilities(),
                expectedConfigurationService);

            // Act
            var systemConfigurationHandlers = registry.Handlers.Where(handler =>
                handler.Topic == Topics.GetSystemConfiguration ||
                handler.Topic == Topics.SetSystemConfiguration ||
                handler.Topic == Topics.ResetNetworkInterface);

            // Assert
            foreach (var handler in systemConfigurationHandlers)
                ShouldReferenceConfigurationService(handler, expectedConfigurationService);
        }

        private static RequestHandlerRegistry CreateRequestHandlerRegistry(
            SupportedCapabilitiesService capabilitiesService,
            SupportedCapabilities supportedCapabilities,
            SystemConfigurationService configurationService)
        {
            var initializationContextType = typeof(ServiceRunner)
                .GetNestedType("InitializationContext", BindingFlags.NonPublic);

            initializationContextType.Should().NotBeNull();

            var initializationContextConstructor = initializationContextType
                .GetConstructor(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    binder: null,
                    [typeof(SupportedCapabilitiesService), typeof(SupportedCapabilities), typeof(SystemConfigurationService)],
                    modifiers: null);

            initializationContextConstructor.Should().NotBeNull();

            var initializationContext = initializationContextConstructor
                .Invoke([capabilitiesService, supportedCapabilities, configurationService]);

            var getRequestHandlerRegistryMethod = typeof(ServiceRunner).GetMethod(
                "GetRequestHandlerRegistry",
                BindingFlags.NonPublic | BindingFlags.Static,
                [initializationContextType]);

            getRequestHandlerRegistryMethod.Should().NotBeNull();

            var result = getRequestHandlerRegistryMethod.Invoke(null, [initializationContext]);
            result.Should().BeOfType<RequestHandlerRegistry>();

            return (RequestHandlerRegistry)result;
        }

        private static SupportedCapabilitiesService CreateSupportedCapabilitiesService()
            => new(Substitute.For<IFileSystem>(), Substitute.For<ILogger>());

        private static SystemConfigurationService CreateSystemConfigurationService()
            => new(
                Substitute.For<IPersistence>(),
                Substitute.For<IScriptHandler>(),
                Substitute.For<ILogger>(),
                new SupportedSettings());

        private static void ShouldReferenceConfigurationService(
            IRequestHandler handler,
            SystemConfigurationService expectedConfigurationService)
        {
            var configurationServiceReferences = handler
                .GetType()
                .GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                .Where(field => field.FieldType == typeof(SystemConfigurationService))
                .Select(field => field.GetValue(handler))
                .ToList();

            configurationServiceReferences
                .Should()
                .ContainSingle(reference => ReferenceEquals(reference, expectedConfigurationService));
        }
    }
}

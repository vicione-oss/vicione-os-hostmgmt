using System.Reflection;
using AwesomeAssertions;
using HostManagement.Communication.NamedPipes.RequestHandlers;
using HostManagement.Shared.Communication;
using HostManagement.SystemConfig;
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

            // Act
            var registry = CreateRequestHandlerRegistry();
            var handlers = GetRegisteredHandlers(registry);

            // Assert
            handlers.Keys.Should().BeEquivalentTo(expectedTopics);
        }

        [Fact]
        public void ShouldUseSharedSystemConfigurationServiceInstance_WhenRegisteringSystemConfigurationHandlers()
        {
            // Arrange
            var expectedConfigurationService = GetSystemConfigurationService();
            var registry = CreateRequestHandlerRegistry();
            var handlers = GetRegisteredHandlers(registry);

            // Act
            var systemConfigurationHandlers = new[]
            {
                handlers[Topics.GetSystemConfiguration],
                handlers[Topics.SetSystemConfiguration],
                handlers[Topics.ResetNetworkInterface]
            };

            // Assert
            foreach (var handler in systemConfigurationHandlers)
                ShouldReferenceConfigurationService(handler, expectedConfigurationService);
        }

        private static RequestHandlerRegistry CreateRequestHandlerRegistry()
        {
            var getRequestHandlerRegistryMethod = typeof(ServiceRunner).GetMethod(
                "GetRequestHandlerRegistry",
                BindingFlags.NonPublic | BindingFlags.Static);

            getRequestHandlerRegistryMethod.Should().NotBeNull();

            var result = getRequestHandlerRegistryMethod.Invoke(null, null);
            result.Should().BeOfType<RequestHandlerRegistry>();

            return (RequestHandlerRegistry)result;
        }

        private static Dictionary<string, IRequestHandler> GetRegisteredHandlers(RequestHandlerRegistry registry)
        {
            var handlersField = typeof(RequestHandlerRegistry).GetField(
                "_handlers",
                BindingFlags.NonPublic | BindingFlags.Instance);

            handlersField.Should().NotBeNull();

            var value = handlersField.GetValue(registry);
            value.Should().BeOfType<Dictionary<string, IRequestHandler>>();

            return (Dictionary<string, IRequestHandler>)value;
        }

        private static SystemConfigurationService GetSystemConfigurationService()
        {
            var configurationServiceField = typeof(ServiceRunner).GetField(
                "s_configurationService",
                BindingFlags.NonPublic | BindingFlags.Static);

            configurationServiceField.Should().NotBeNull();

            var value = configurationServiceField.GetValue(null);
            value.Should().BeOfType<SystemConfigurationService>();

            return (SystemConfigurationService)value;
        }

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

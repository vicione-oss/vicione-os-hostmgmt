using System.Reflection;
using System.Text.Json;
using AwesomeAssertions;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Contracts.NetworkStatus;
using Xunit;

namespace HostManagement.Shared.Communication.Tests.Contracts;

public sealed class OperationResultFactoryTests
{
    private const string ErrorMessage = "Unit-Test-Error";

    [Fact]
    public void ShouldFindAllOperationResultFactories_WhenScanningAssembly()
        => GetOperationResultFactoryTypes().Length.Should().BePositive();

    [Fact]
    public void ShouldCreateErrorResult_WithCommonInvariants_ForAllTypes()
    {
        foreach (var type in GetOperationResultFactoryTypes())
        {
            var method = typeof(OperationResultFactoryTests)
                .GetMethod(nameof(TestFactoryForType), BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(type);

            try
            {
                method.Invoke(null, [ErrorMessage]);
            }
            catch (TargetInvocationException tie) when (tie.InnerException is not null)
            {
                // Re-throw for clear error from tested type
                throw tie.InnerException;
            }
        }
    }

    [Fact]
    public void ShouldRoundtripSerializeErrorResult_ForAllTypes()
    {
        foreach (var type in GetOperationResultFactoryTypes())
        {
            var method = typeof(OperationResultFactoryTests)
                .GetMethod(nameof(SerializeRoundtripForType), BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(type);

            try
            {
                method.Invoke(null, [ErrorMessage]);
            }
            catch (TargetInvocationException tie) when (tie.InnerException is not null)
            {
                // Re-throw for clear error from tested type
                throw tie.InnerException;
            }
        }
    }

    private static Type[] GetOperationResultFactoryTypes()
    {
        var assembly = typeof(OperationResult).Assembly;

        return [.. assembly
            .GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract)
            .Where(type => typeof(OperationResult).IsAssignableFrom(type))
            .Where(ImplementsOperationResultFactoryInterface)
            .OrderBy(type => type.FullName)];


        static bool ImplementsOperationResultFactoryInterface(Type type)
            => type.GetInterfaces().Any(@interface =>
                @interface.IsGenericType &&
                @interface.GetGenericTypeDefinition() == typeof(IOperationResultFactory<>));
    }

    private static void SerializeRoundtripForType<T>(string message)
        where T : OperationResult, IOperationResultFactory<T>
    {
        // Arrange
        var result = T.CreateErrorResult(message);

        var jsonTypeInfo = SourceGenerationContext.Default.GetTypeInfo(typeof(T));
        jsonTypeInfo.Should().NotBeNull();

        // Act
        var json = JsonSerializer.Serialize(result, jsonTypeInfo);
        var resultObject = JsonSerializer.Deserialize(json, jsonTypeInfo);

        // Assert
        json.Should().NotBeNullOrWhiteSpace();
        resultObject.Should().NotBeNull();

        var typedResultObject = (T)resultObject;
        typedResultObject.ResultType.Should().Be(typeof(T).Name);
        typedResultObject.Status.Should().Be(OperationStatus.Error);
        typedResultObject.Message.Should().Be(message);
    }

    private static void TestFactoryForType<T>(string message)
        where T : OperationResult, IOperationResultFactory<T>
    {
        // Act
        var result = T.CreateErrorResult(message);

        // Assert
        // Common invariants
        result.Should().NotBeNull();
        result.ResultType.Should().Be(typeof(T).Name);
        result.Status.Should().Be(OperationStatus.Error);
        result.Message.Should().Be(message);

        // Type specifics
        switch (result)
        {
            case GetDHCPLeaseInformationResult dhcp:
                dhcp.DHCPLease.Should().BeNull();
                break;

            case GetNetworkStatusInformationResult networkStatus:
                networkStatus.NetworkStatusInformation.Should().Be(NetworkStatusInformation.Empty);
                break;

            case GetNTPFallbackInformationResult ntp:
                ntp.FallbackNTPServers.Should().NotBeNull();
                ntp.FallbackNTPServers.Should().BeEmpty();
                break;

            case GetOriginalPhysicalAddressResult address:
                address.OriginalPhysicalAddress.Should().BeNull();
                break;

            case GetServiceStateResult service:
                service.ActiveState.Should().Be("unknown");
                break;

            case GetSystemConfigurationResult config:
                config.Configuration.Should().BeNull();
                break;
        }
    }
}

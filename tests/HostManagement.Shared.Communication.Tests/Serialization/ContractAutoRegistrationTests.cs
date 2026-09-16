using AwesomeAssertions;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Contracts.Service;
using HostManagement.Shared.Contracts.System;
using HostManagement.Shared.Serialization;
using Xunit;

namespace HostManagement.Shared.Communication.Tests.Serialization;

/// <summary>
/// Guards that every message contract shipped with the communication packages is resolvable without the consumer
/// calling <see cref="MessageTypeRegistry.AddContext"/>. Each owning assembly registers its context from a module
/// initializer, so no test here may register anything itself.
/// </summary>
public class ContractAutoRegistrationTests
{
    public static TheoryData<Type> ContractTypes =>
    [
        typeof(NamedPipeMessage),
        typeof(GetSupportedCapabilitiesResult),
        typeof(Response),
        typeof(GetSystemConfigurationResult),
        typeof(SetSystemConfigurationResult),
        typeof(GetOriginalPhysicalAddressResult),
        typeof(GetNetworkStatusInformationResult),
        typeof(GetDHCPLeaseInformationResult),
        typeof(GetNTPFallbackInformationResult),
        typeof(GetNTPSyncStatusResult),
        typeof(GetServiceStateResult),
        typeof(RedeployScriptsResult),
        typeof(ServiceControlResult),
        typeof(SystemControlResult),
        typeof(SystemConfiguration),
        typeof(SignedDebianPackage),
        typeof(ServiceControlRequest)
    ];

    [Theory]
    [MemberData(nameof(ContractTypes))]
    public void ShouldResolveTypeInfo_WhenNoContextRegisteredManually(Type contractType)
    {
        // Act
        var typeInfo = ResolveTypeInfo(contractType);

        // Assert
        typeInfo.Should().NotBeNull();
        typeInfo!.Type.Should().Be(contractType);
    }

    [Fact]
    public void ShouldRoundTripContract_WhenNoContextRegisteredManually()
    {
        // Arrange
        var result = new ServiceControlResult
        {
            Status = Enums.OperationStatus.Success,
            Message = "Service started."
        };

        // Act
        var content = MessageSerializer.Serialize(result);
        var deserialized = MessageSerializer.Deserialize<ServiceControlResult>(content);

        // Assert
        deserialized.Should().Be(result);
    }

    private static System.Text.Json.Serialization.Metadata.JsonTypeInfo? ResolveTypeInfo(Type contractType)
    {
        // 'TryGetTypeInfo' is generic, so the contract type is bound through reflection for this data-driven test.
        var method = typeof(MessageTypeRegistry)
            .GetMethod(nameof(MessageTypeRegistry.TryGetTypeInfo))!
            .MakeGenericMethod(contractType);

        var arguments = new object?[] { null };

        return (bool)method.Invoke(null, arguments)!
            ? (System.Text.Json.Serialization.Metadata.JsonTypeInfo?)arguments[0]
            : null;
    }
}

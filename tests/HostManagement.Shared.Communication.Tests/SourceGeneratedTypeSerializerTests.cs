using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using FluentAssertions;
using Xunit;
using HostManagement.Shared.Communication.Serialization;
using HostManagement.Shared.Contracts;
using SourceGenerationContext = HostManagement.Shared.Communication.Contracts.SourceGenerationContext;
using HostManagement.Shared.Contracts.NetworkStatus;
using HostManagement.Shared.Contracts.Network;

namespace HostManagement.Shared.Communication.Tests;

public class SourceGeneratedTypeSerializerTests
{
    public static IEnumerable<object[]> JsonNullData
    {
        get
        {
            var nullJson = "null";

            return new List<object[]>
            {
                new object[] { "RedeployScriptsResult", SourceGenerationContext.Default.RedeployScriptsResult, nullJson },
                new object[] { "GetSystemConfigurationResult", SourceGenerationContext.Default.GetSystemConfigurationResult, nullJson },
                new object[] { "SetSystemConfigurationResult", SourceGenerationContext.Default.SetSystemConfigurationResult, nullJson },
                new object[] { "GetViciOneSuiteVersionsResult", SourceGenerationContext.Default.GetViciOneSuiteVersionsResult, nullJson },
                new object[] { "InstallViciOneSuiteVersionResult", SourceGenerationContext.Default.InstallViciOneSuiteVersionResult, nullJson },
                new object[] { "GetNetworkStatusInformationResult", SourceGenerationContext.Default.GetNetworkStatusInformationResult, nullJson },
                new object[] { "RenewDHCPLeaseResult", SourceGenerationContext.Default.RenewDHCPLeaseResult, nullJson },
                new object[] { "ServiceControlResult", SourceGenerationContext.Default.ServiceControlResult, nullJson }
            };
        }
    }

    public static IEnumerable<object[]> MissingResultTypeData
    {
        get
        {
            var redeployContext = SourceGenerationContext.Default.RedeployScriptsResult;
            var loadContext = SourceGenerationContext.Default.GetSystemConfigurationResult;
            var applyContext = SourceGenerationContext.Default.SetSystemConfigurationResult;
            var getSuiteContext = SourceGenerationContext.Default.GetViciOneSuiteVersionsResult;
            var installSuiteContext = SourceGenerationContext.Default.InstallViciOneSuiteVersionResult;
            var getNetworkContext = SourceGenerationContext.Default.GetNetworkStatusInformationResult;
            var renewDHCPLeaseContext = SourceGenerationContext.Default.RenewDHCPLeaseResult;
            var serviceControlContext = SourceGenerationContext.Default.ServiceControlResult;

            var configurationJson = JsonSerializer.Serialize(
                new SystemConfiguration(),
                SourceGenerationContext.Default.SystemConfiguration);

            var networkStatusJson = JsonSerializer.Serialize(
                NetworkStatusInformation.Empty,
                SourceGenerationContext.Default.NetworkStatusInformation);

            return new List<object[]>
            {
                new object[] { "RedeployScriptsResult", redeployContext, """{"ResultType":"","Status":"Success","Message":null}""" },
                new object[] { "GetSystemConfigurationResult", loadContext, $$"""{"ResultType":"","Status":"Warning","Message":"Warning","Configuration":{{configurationJson}}}""" },
                new object[] { "SetSystemConfigurationResult", applyContext, """{"ResultType":"","Status":"Error","Message":"Error"}""" },
                new object[] { "GetViciOneSuiteVersionsResult", getSuiteContext, """{"ResultType":"","Status":"Warning","Message":"Warning","SuiteVersions":[]}""" },
                new object[] { "InstallViciOneSuiteVersionResult", installSuiteContext, """{"ResultType":"","Status":"Error","Message":"Error"}""" },
                new object[] { "GetNetworkStatusInformationResult", getNetworkContext, $$"""{"ResultType":"","Status":"Warning","Message":"Warning","NetworkStatusInformation":{{networkStatusJson}}}""" },
                new object[] { "RenewDHCPLeaseResult", renewDHCPLeaseContext, """{"ResultType":"","Status":"Warning","Message":"Warning","DHCPLease":null}""" },
                new object[] { "ServiceControlResult", serviceControlContext, """{"ResultType":"","Status":"Warning","Message":"Warning"}""" }
            };
        }
    }

    public static IEnumerable<object[]> ResultTypeMismatchData
    {
        get
        {
            var redeployContext = SourceGenerationContext.Default.RedeployScriptsResult;
            var loadContext = SourceGenerationContext.Default.GetSystemConfigurationResult;
            var applyContext = SourceGenerationContext.Default.SetSystemConfigurationResult;
            var getSuiteContext = SourceGenerationContext.Default.GetViciOneSuiteVersionsResult;
            var installSuiteContext = SourceGenerationContext.Default.InstallViciOneSuiteVersionResult;
            var getNetworkContext = SourceGenerationContext.Default.GetNetworkStatusInformationResult;
            var renewDHCPLeaseContext = SourceGenerationContext.Default.RenewDHCPLeaseResult;
            var serviceControlContext = SourceGenerationContext.Default.ServiceControlResult;

            var configurationJson = JsonSerializer.Serialize(
                new SystemConfiguration(),
                SourceGenerationContext.Default.SystemConfiguration);

            var networkStatusJson = JsonSerializer.Serialize(
                NetworkStatusInformation.Empty,
                SourceGenerationContext.Default.NetworkStatusInformation);

            return new List<object[]>
            {
                new object[] { "RedeployScriptsResult", redeployContext, """{"ResultType":"Mismatch","Status":"Success","Message":null}""" },
                new object[] { "GetSystemConfigurationResult", loadContext, $$"""{"ResultType":"Mismatch","Status":"Warning","Message":"Warning","Configuration":{{configurationJson}}}""" },
                new object[] { "SetSystemConfigurationResult", applyContext, """{"ResultType":"Mismatch","Status":"Error","Message":"Error"}""" },
                new object[] { "GetViciOneSuiteVersionsResult", getSuiteContext, """{"ResultType":"Mismatch","Status":"Warning","Message":"Warning","SuiteVersions":[]}""" },
                new object[] { "InstallViciOneSuiteVersionResult", installSuiteContext, """{"ResultType":"Mismatch","Status":"Error","Message":"Error"}""" },
                new object[] { "GetNetworkStatusInformationResult", getNetworkContext, $$"""{"ResultType":"Mismatch","Status":"Warning","Message":"Warning","NetworkStatusInformation":{{networkStatusJson}}}""" },
                new object[] { "RenewDHCPLeaseResult", renewDHCPLeaseContext, """{"ResultType":"Mismatch","Status":"Warning","Message":"Warning","DHCPLease":null}""" },
                new object[] { "ServiceControlResult", serviceControlContext, """{"ResultType":"Mismatch","Status":"Warning","Message":"Warning"}""" }
            };
        }
    }

    public static IEnumerable<object[]> DefaultResponseData
    {
        get
        {
            var defaultResponseJson = """{"Message":"Error processing request: Exception."}""";

            return new List<object[]>
            {
                new object[] { SourceGenerationContext.Default.RedeployScriptsResult, defaultResponseJson },
                new object[] { SourceGenerationContext.Default.GetSystemConfigurationResult, defaultResponseJson },
                new object[] { SourceGenerationContext.Default.SetSystemConfigurationResult, defaultResponseJson },
                new object[] { SourceGenerationContext.Default.GetViciOneSuiteVersionsResult, defaultResponseJson },
                new object[] { SourceGenerationContext.Default.InstallViciOneSuiteVersionResult, defaultResponseJson },
                new object[] { SourceGenerationContext.Default.GetNetworkStatusInformationResult, defaultResponseJson },
                new object[] { SourceGenerationContext.Default.RenewDHCPLeaseResult, defaultResponseJson },
                new object[] { SourceGenerationContext.Default.ServiceControlResult, defaultResponseJson }
            };
        }
    }

    public static IEnumerable<object[]> MissingMemberData
    {
        get
        {
            var redeployContext = SourceGenerationContext.Default.RedeployScriptsResult;
            var loadContext = SourceGenerationContext.Default.GetSystemConfigurationResult;
            var applyContext = SourceGenerationContext.Default.SetSystemConfigurationResult;
            var getSuiteContext = SourceGenerationContext.Default.GetViciOneSuiteVersionsResult;
            var installSuiteContext = SourceGenerationContext.Default.InstallViciOneSuiteVersionResult;
            var getNetworkContext = SourceGenerationContext.Default.GetNetworkStatusInformationResult;
            var renewDHCPLeaseContext = SourceGenerationContext.Default.RenewDHCPLeaseResult;
            var serviceControlContext = SourceGenerationContext.Default.ServiceControlResult;

            return new List<object[]>
            {
                new object[] { redeployContext, """{"ResultType":"RedeployScriptsResult","Status":"Success"}""" },
                new object[] { redeployContext, """{"ResultType":"RedeployScriptsResult","Message":null}""" },

                new object[] { loadContext, """{"ResultType":"GetSystemConfigurationResult","Status":"Warning"}""" },
                new object[] { loadContext, """{"ResultType":"GetSystemConfigurationResult","Message":"Warning"}""" },
                new object[] { loadContext, """{"ResultType":"GetSystemConfigurationResult","Configuration":{}}""" },
                new object[] { loadContext, """{"ResultType":"GetSystemConfigurationResult","Status":"Warning","Message":"Warning"}""" },
                new object[] { loadContext, """{"ResultType":"GetSystemConfigurationResult","Status":"Warning","Configuration":{}}""" },
                new object[] { loadContext, """{"ResultType":"GetSystemConfigurationResult","Message":"Warning","Configuration":{}}""" },

                new object[] { applyContext, """{"ResultType":"SetSystemConfigurationResult","Status":"Error"}""" },
                new object[] { applyContext, """{"ResultType":"SetSystemConfigurationResult","Message":"Error"}""" },

                new object[] { getSuiteContext, """{"ResultType":"GetViciOneSuiteVersionsResult","Status":"Warning"}""" },
                new object[] { getSuiteContext, """{"ResultType":"GetViciOneSuiteVersionsResult","Message":"Warning"}""" },
                new object[] { getSuiteContext, """{"ResultType":"GetViciOneSuiteVersionsResult","SuiteVersions":[]}""" },
                new object[] { getSuiteContext, """{"ResultType":"GetViciOneSuiteVersionsResult","Status":"Warning","Message":"Warning"}""" },
                new object[] { getSuiteContext, """{"ResultType":"GetViciOneSuiteVersionsResult","Status":"Warning","SuiteVersions":[]}""" },
                new object[] { getSuiteContext, """{"ResultType":"GetViciOneSuiteVersionsResult","Message":"Warning","SuiteVersions":[]}""" },

                new object[] { installSuiteContext, """{"ResultType":"InstallViciOneSuiteVersionResult","Status":"Error"}""" },
                new object[] { installSuiteContext, """{"ResultType":"InstallViciOneSuiteVersionResult","Message":"Error"}""" },

                new object[] { getNetworkContext, """{"ResultType":"GetNetworkStatusInformationResult","Status":"Warning"}""" },
                new object[] { getNetworkContext, """{"ResultType":"GetNetworkStatusInformationResult","Message":"Warning"}""" },
                new object[] { getNetworkContext, """{"ResultType":"GetNetworkStatusInformationResult","NetworkStatusInformation":{}}""" },
                new object[] { getNetworkContext, """{"ResultType":"GetNetworkStatusInformationResult","Status":"Warning","Message":"Warning"}""" },
                new object[] { getNetworkContext, """{"ResultType":"GetNetworkStatusInformationResult","Status":"Warning","NetworkStatusInformation":{}}""" },
                new object[] { getNetworkContext, """{"ResultType":"GetNetworkStatusInformationResult","Message":"Warning","NetworkStatusInformation":{}}""" },

                new object[] { renewDHCPLeaseContext, """{"ResultType":"RenewDHCPLeaseResult","Status":"Warning"}""" },
                new object[] { renewDHCPLeaseContext, """{"ResultType":"RenewDHCPLeaseResult","Message":"Warning"}""" },
                new object[] { renewDHCPLeaseContext, """{"ResultType":"RenewDHCPLeaseResult","DHCPLease":null}""" },
                new object[] { renewDHCPLeaseContext, """{"ResultType":"RenewDHCPLeaseResult","Status":"Warning","Message":"Warning"}""" },
                new object[] { renewDHCPLeaseContext, """{"ResultType":"RenewDHCPLeaseResult","Status":"Warning","DHCPLease":null}""" },
                new object[] { renewDHCPLeaseContext, """{"ResultType":"RenewDHCPLeaseResult","Message":"Warning","DHCPLease":null}""" },

                new object[] { serviceControlContext, """{"ResultType":"ServiceControlResult","Status":"Warning"}""" },
                new object[] { serviceControlContext, """{"ResultType":"ServiceControlResult","Message":"Warning"}""" }
            };
        }
    }

    public static IEnumerable<object[]> AdditionalMemberData
    {
        get
        {
            var redeployContext = SourceGenerationContext.Default.RedeployScriptsResult;
            var loadContext = SourceGenerationContext.Default.GetSystemConfigurationResult;
            var applyContext = SourceGenerationContext.Default.SetSystemConfigurationResult;
            var getSuiteContext = SourceGenerationContext.Default.GetViciOneSuiteVersionsResult;
            var installSuiteContext = SourceGenerationContext.Default.InstallViciOneSuiteVersionResult;
            var getNetworkContext = SourceGenerationContext.Default.GetNetworkStatusInformationResult;
            var renewDHCPLeaseContext = SourceGenerationContext.Default.RenewDHCPLeaseResult;
            var serviceControlContext = SourceGenerationContext.Default.ServiceControlResult;

            var configurationJson = JsonSerializer.Serialize(
                new SystemConfiguration(),
                SourceGenerationContext.Default.SystemConfiguration);

            var networkStatusJson = JsonSerializer.Serialize(
                NetworkStatusInformation.Empty,
                SourceGenerationContext.Default.NetworkStatusInformation);

            var dhcpLeaseJson = JsonSerializer.Serialize(
                DHCPLease.Empty,
                SourceGenerationContext.Default.DHCPLease);

            return new List<object[]>
            {
                new object[] { redeployContext, $$"""{"ResultType":"RedeployScriptsResult","Status":"Success","Message":null,"Configuration":{{configurationJson}}}""" },
                new object[] { redeployContext, """{"ResultType":"RedeployScriptsResult","AdditionalMember":"Additional","Status":"Success","Message":null}""" },

                new object[] { loadContext, $$"""{"ResultType":"GetSystemConfigurationResult","Status":"Warning","Message":"Warning","Configuration":{{configurationJson}},"AdditionalMember":"Additional"}""" },
                new object[] { loadContext, $$"""{"ResultType":"GetSystemConfigurationResult","Status":"Warning","Message":"Warning","AdditionalMember":"Additional","Configuration":{{configurationJson}}}""" },

                new object[] { applyContext, """{"ResultType":"SetSystemConfigurationResult","Status":"Error","Message":"Error","AdditionalMember":"Additional"}""" },
                new object[] { applyContext, """{"ResultType":"SetSystemConfigurationResult","AdditionalMember":"Additional","Status":"Error","Message":"Error"}""" },

                new object[] { getSuiteContext, """{"ResultType":"GetViciOneSuiteVersionsResult","Status":"Warning","Message":"Warning","SuiteVersions":[],"AdditionalMember":"Additional"}""" },
                new object[] { getSuiteContext, """{"ResultType":"GetViciOneSuiteVersionsResult","Status":"Warning","Message":"Warning","AdditionalMember":"Additional","SuiteVersions":[]}""" },

                new object[] { installSuiteContext, """{"ResultType":"InstallViciOneSuiteVersionResult","Status":"Error","Message":"Error","AdditionalMember":"Additional"}""" },
                new object[] { installSuiteContext, """{"ResultType":"InstallViciOneSuiteVersionResult","AdditionalMember":"Additional","Status":"Error","Message":"Error"}""" },

                new object[] { getNetworkContext, $$"""{"ResultType":"GetNetworkStatusInformationResult","Status":"Warning","Message":"Warning","NetworkStatusInformation":{{networkStatusJson}},"AdditionalMember":"Additional"}""" },
                new object[] { getNetworkContext, $$"""{"ResultType":"GetNetworkStatusInformationResult","Status":"Warning","Message":"Warning","AdditionalMember":"Additional","NetworkStatusInformation":{{networkStatusJson}}}""" },

                new object[] { renewDHCPLeaseContext, $$"""{"ResultType":"RenewDHCPLeaseResult","Status":"Warning","Message":"Warning","DHCPLease":{{dhcpLeaseJson}},"AdditionalMember":"Additional"}""" },
                new object[] { renewDHCPLeaseContext, $$"""{"ResultType":"RenewDHCPLeaseResult","Status":"Warning","Message":"Warning","AdditionalMember":"Additional","DHCPLease":{{dhcpLeaseJson}}}""" },

                new object[] { serviceControlContext, """{"ResultType":"ServiceControlResult","Status":"Warning","Message":"Warning","AdditionalMember":"Additional"}""" },
                new object[] { serviceControlContext, """{"ResultType":"ServiceControlResult","AdditionalMember":"Additional","Status":"Warning","Message":"Warning"}""" }
            };
        }
    }

    public static IEnumerable<object[]> InvalidNullMemberData
    {
        get
        {
            var redeployContext = SourceGenerationContext.Default.RedeployScriptsResult;
            var getSuiteContext = SourceGenerationContext.Default.GetViciOneSuiteVersionsResult;
            var getNetworkContext = SourceGenerationContext.Default.GetNetworkStatusInformationResult;

            var invalidNullData = "null";

            return new List<object[]>
            {
                new object[] { redeployContext, $$"""{"ResultType":{{invalidNullData}},"Status":"Success","Message":""}""" },
                new object[] { getSuiteContext, $$"""{"ResultType":"GetViciOneSuiteVersionsResult","Status":"Warning","Message":"Warning","SuiteVersions":{{invalidNullData}}}""" },
                new object[] { getNetworkContext, $$"""{"ResultType":"GetNetworkStatusInformationResult","Status":"Warning","Message":"Warning","NetworkStatusInformation":{{invalidNullData}}}""" }
            };
        }
    }

    public static IEnumerable<object[]> ValidMemberData
    {
        get
        {
            var redeployContext = SourceGenerationContext.Default.RedeployScriptsResult;
            var loadContext = SourceGenerationContext.Default.GetSystemConfigurationResult;
            var applyContext = SourceGenerationContext.Default.SetSystemConfigurationResult;
            var getSuiteContext = SourceGenerationContext.Default.GetViciOneSuiteVersionsResult;
            var installSuiteContext = SourceGenerationContext.Default.InstallViciOneSuiteVersionResult;
            var getNetworkContext = SourceGenerationContext.Default.GetNetworkStatusInformationResult;
            var renewDHCPLeaseContext = SourceGenerationContext.Default.RenewDHCPLeaseResult;
            var serviceControlContext = SourceGenerationContext.Default.ServiceControlResult;

            var configurationJson = JsonSerializer.Serialize(
                new SystemConfiguration(),
                SourceGenerationContext.Default.SystemConfiguration);

            var networkStatusJson = JsonSerializer.Serialize(
                NetworkStatusInformation.Empty,
                SourceGenerationContext.Default.NetworkStatusInformation);

            var dhcpLeaseJson = JsonSerializer.Serialize(
                DHCPLease.Empty,
                SourceGenerationContext.Default.DHCPLease);

            return new List<object[]>
            {
                new object[] { redeployContext, """{"ResultType":"RedeployScriptsResult","Status":"Success","Message":null}""" },
                new object[] { loadContext, $$"""{"ResultType":"GetSystemConfigurationResult","Status":"Warning","Message":"Warning","Configuration":{{configurationJson}}}""" },
                new object[] { applyContext, """{"ResultType":"SetSystemConfigurationResult","Status":"Error","Message":"Error"}""" },
                new object[] { getSuiteContext, """{"ResultType":"GetViciOneSuiteVersionsResult","Status":"Warning","Message":"Warning","SuiteVersions":[]}""" },
                new object[] { installSuiteContext, """{"ResultType":"InstallViciOneSuiteVersionResult","Status":"Error","Message":"Error"}""" },
                new object[] { getNetworkContext, $$"""{"ResultType":"GetNetworkStatusInformationResult","Status":"Warning","Message":"Warning","NetworkStatusInformation":{{networkStatusJson}}}""" },
                new object[] { renewDHCPLeaseContext, $$"""{"ResultType":"RenewDHCPLeaseResult","Status":"Warning","Message":"Warning","DHCPLease":{{dhcpLeaseJson}}}""" },
                new object[] { serviceControlContext, """{"ResultType":"ServiceControlResult","Status":"Success","Message":"Success"}""" }
            };
        }
    }

    public class Deserialize : SourceGeneratedTypeSerializerTests
    {
        [Theory]
        [MemberData(nameof(JsonNullData))]
        public void ShouldThrow_WhenJsonIsNull<TType>(
            string jsonTypeInfoTypeName,
            JsonTypeInfo<TType> jsonTypeInfo,
            string json)
        {
            // Act
            var result = () => SourceGeneratedTypeSerializer.Deserialize(json, jsonTypeInfo);

            // Assert
            result.Should()
                .Throw<InvalidOperationException>()
                .WithMessage(
                    $"JSON string could not be deserialized to '{jsonTypeInfoTypeName}'." +
                    Environment.NewLine +
                    $"JSON string was: 'null'");
        }

        [Theory]
        [MemberData(nameof(MissingResultTypeData))]
        public void ShouldThrow_WhenResultTypeIsMissing<TType>(
            string jsonTypeInfoTypeName,
            JsonTypeInfo<TType> jsonTypeInfo,
            string json)
        {
            // Act
            var result = () => SourceGeneratedTypeSerializer.Deserialize(json, jsonTypeInfo);

            // Assert
            result.Should()
                .Throw<JsonException>()
                .WithMessage(
                    $"Required property 'ResultType' is missing or empty " +
                    $"(expected type '{jsonTypeInfoTypeName}').");
        }

        [Theory]
        [MemberData(nameof(ResultTypeMismatchData))]
        public void ShouldThrow_WhenResultTypeIsMismatch<TType>(
            string jsonTypeInfoTypeName,
            JsonTypeInfo<TType> jsonTypeInfo,
            string json)
        {
            // Act
            var result = () => SourceGeneratedTypeSerializer.Deserialize(json, jsonTypeInfo);

            // Assert
            result.Should()
                .Throw<JsonException>()
                .WithMessage(
                    $"Type mismatch: Expected '{jsonTypeInfoTypeName}', but got 'Mismatch'.");
        }

        [Theory]
        [MemberData(nameof(DefaultResponseData))]
        [MemberData(nameof(MissingMemberData))]
        public void ShouldThrow_WhenMissingMemberFound<TType>(
            JsonTypeInfo<TType> jsonTypeInfo,
            string json)
        {
            // Act
            var result = () => SourceGeneratedTypeSerializer.Deserialize(json, jsonTypeInfo);

            // Assert
            result.Should()
                .Throw<JsonException>()
                .WithMessage("*missing*");
        }

        [Theory]
        [MemberData(nameof(AdditionalMemberData))]
        public void ShouldThrow_WhenUnmappedMemberFound<TType>(
            JsonTypeInfo<TType> jsonTypeInfo,
            string json)
        {
            // Act
            var result = () => SourceGeneratedTypeSerializer.Deserialize(json, jsonTypeInfo);

            // Assert
            result.Should()
                .Throw<JsonException>()
                .WithMessage("*could not be mapped*");
        }

        [Theory]
        [MemberData(nameof(InvalidNullMemberData))]
        public void ShouldThrow_WhenNullabilityIsNotRespected<TType>(
            JsonTypeInfo<TType> jsonTypeInfo,
            string json)
        {
            // Act
            var result = () => SourceGeneratedTypeSerializer.Deserialize(json, jsonTypeInfo);

            // Assert
            result.Should()
                .Throw<JsonException>()
                .WithMessage("*doesn't allow null values*");
        }

        [Theory]
        [MemberData(nameof(ValidMemberData))]
        public void ShouldReturnGetSystemConfigurationResult_WhenJsonValid<TType>(
            JsonTypeInfo<TType> jsonTypeInfo,
            string json)
        {
            // Act
            var result = SourceGeneratedTypeSerializer.Deserialize(json, jsonTypeInfo);

            // Assert
            result.Should().BeOfType<TType>();
        }
    }

    public class Serialize : SourceGeneratedTypeSerializerTests
    {
        [Theory]
        [MemberData(nameof(ValidMemberData))]
        public void ShouldSerializeResult_WhenCalled<TType>(
            JsonTypeInfo<TType> jsonTypeInfo,
            string json)
        {
            // Arrange
            var deserializedType = SourceGeneratedTypeSerializer.Deserialize(json, jsonTypeInfo);

            // Act
            var result = SourceGeneratedTypeSerializer.Serialize(deserializedType, jsonTypeInfo);

            // Assert
            result.Should().Be(json);
        }
    }
}

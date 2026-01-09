using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using FluentAssertions;
using Xunit;
using HostManagement.Shared.Communication.Serialization;
using HostManagement.Shared.Contracts;
using SourceGenerationContext = HostManagement.Shared.Communication.Contracts.SourceGenerationContext;

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
                new object[] { SourceGenerationContext.Default.RedeployScriptsResult, nullJson },
                new object[] { SourceGenerationContext.Default.ApplySystemConfigurationResult, nullJson },
                new object[] { SourceGenerationContext.Default.ApplySystemConfigurationResult, nullJson },
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
                new object[] { SourceGenerationContext.Default.LoadSystemConfigurationResult, defaultResponseJson },
                new object[] { SourceGenerationContext.Default.ApplySystemConfigurationResult, defaultResponseJson },
            };
        }
    }

    public static IEnumerable<object[]> MissingMemberData
    {
        get
        {
            var redeployContext = SourceGenerationContext.Default.RedeployScriptsResult;
            var loadContext = SourceGenerationContext.Default.LoadSystemConfigurationResult;
            var applyContext = SourceGenerationContext.Default.ApplySystemConfigurationResult;

            return new List<object[]>
            {
                new object[] { redeployContext, """{"Status":"Success","ResultType":"RedeployScriptsResult"}""" },
                new object[] { redeployContext, """{"Message":null,"ResultType":"RedeployScriptsResult"}""" },

                new object[] { loadContext, """{"Status":"Warning"}""" },
                new object[] { loadContext, """{"Message":"Warning"}""" },
                new object[] { loadContext, """{"Configuration":{}}""" },
                new object[] { loadContext, """{"Status":"Warning","Message":"Warning"}""" },
                new object[] { loadContext, """{"Status":"Warning","Configuration":{}}""" },
                new object[] { loadContext, """{"Message":"Warning","Configuration":{}}""" },

                new object[] { applyContext, """{"Status":"Error","ResultType":"ApplySystemConfigurationResult"}""" },
                new object[] { applyContext, """{"Message":"Error","ResultType":"ApplySystemConfigurationResult"}""" },
            };
        }
    }

    public static IEnumerable<object[]> AdditionalMemberData
    {
        get
        {
            var redeployContext = SourceGenerationContext.Default.RedeployScriptsResult;
            var loadContext = SourceGenerationContext.Default.LoadSystemConfigurationResult;
            var applyContext = SourceGenerationContext.Default.ApplySystemConfigurationResult;

            return new List<object[]>
            {
                new object[] { redeployContext, """{"Status":"Success","Message":null,"ResultType":"RedeployScriptsResult","Configuration":{}}""" },
                new object[] { redeployContext, """{"AdditionalMember":"Additional","Status":"Success","Message":null,"ResultType":"RedeployScriptsResult"}""" },

                new object[] { loadContext, """{"Status":"Warning","Message":"Warning","Configuration":{},"ResultType":"RedeployScriptsResult"}""" },
                new object[] { loadContext, """{"Status":"Warning","Message":"Warning","Configuration":{},"ResultType":"ApplySystemConfigurationResult"}""" },
                new object[] { loadContext, """{"AdditionalMember":"Additional","Status":"Warning","Message":"Warning","Configuration":{}}""" },

                new object[] { applyContext, """{"Status":"Error","Message":"Error","ResultType":"ApplySystemConfigurationResult","Configuration":{}}""" },
                new object[] { applyContext, """{"AdditionalMember":"Additional","Status":"Error","Message":"Error","ResultType":"ApplySystemConfigurationResult"}""" },
            };
        }
    }

    public static IEnumerable<object[]> UnknownMemberData
    {
        get
        {
            var redeployContext = SourceGenerationContext.Default.RedeployScriptsResult;
            var loadContext = SourceGenerationContext.Default.LoadSystemConfigurationResult;
            var applyContext = SourceGenerationContext.Default.ApplySystemConfigurationResult;

            return new List<object[]>
            {
                new object[] { redeployContext, """{"Status":"Success","UnknownMember":"Unknown","ResultType":"RedeployScriptsResult"}""" },
                new object[] { redeployContext, """{"UnknownMember":0,"Message":null,"ResultType":"RedeployScriptsResult"}""" },

                new object[] { loadContext, """{"Status":"Warning","UnknownMember":"Unknown","Configuration":{}}""" },
                new object[] { loadContext, """{"UnknownMember":1,"Message":"Warning","Configuration":{}}""" },

                new object[] { applyContext, """{"Status":"Error","UnknownMember":"Unknown","ResultType":"ApplySystemConfigurationResult"}""" },
                new object[] { applyContext, """{"UnknownMember":2,"Message":"Error","ResultType":"ApplySystemConfigurationResult"}""" },
            };
        }
    }

    public static IEnumerable<object[]> ValidMemberData
    {
        get
        {
            var redeployContext = SourceGenerationContext.Default.RedeployScriptsResult;
            var loadContext = SourceGenerationContext.Default.LoadSystemConfigurationResult;
            var applyContext = SourceGenerationContext.Default.ApplySystemConfigurationResult;

            var configurationJson = JsonSerializer.Serialize(
                new SystemConfiguration(),
                SourceGenerationContext.Default.SystemConfiguration);

            return new List<object[]>
            {
                new object[] { redeployContext, """{"Status":"Success","Message":null,"ResultType":"RedeployScriptsResult"}""" },
                new object[] { loadContext, $$"""{"Status":"Warning","Message":"Warning","Configuration":{{configurationJson}}}""" },
                new object[] { applyContext, """{"Status":"Error","Message":"Error","ResultType":"ApplySystemConfigurationResult"}""" },
            };
        }
    }

    public class Deserialize : SourceGeneratedTypeSerializerTests
    {
        [Theory]
        [MemberData(nameof(JsonNullData))]
        public void ShouldThrow_WhenDeserializationResultIsNull<TType>(
            JsonTypeInfo<TType> jsonTypeInfo,
            string json)
        {
            // Act
            var result = () => SourceGeneratedTypeSerializer.Deserialize(json, jsonTypeInfo);

            // Assert
            result.Should()
                .Throw<InvalidOperationException>()
                .WithMessage($"JSON string could not be deserialized to {nameof(jsonTypeInfo.Type)}*");
        }

        [Theory]
        [MemberData(nameof(DefaultResponseData))]
        [MemberData(nameof(MissingMemberData))]
        [MemberData(nameof(AdditionalMemberData))]
        [MemberData(nameof(UnknownMemberData))]
        public void ShouldThrow_WhenUnmappedMemberFound<TType>(
            JsonTypeInfo<TType> jsonTypeInfo,
            string json)
        {
            // Act
            var result = () => SourceGeneratedTypeSerializer.Deserialize(json, jsonTypeInfo);

            // Assert
            result.Should().Throw<JsonException>();
        }

        [Theory]
        [MemberData(nameof(ValidMemberData))]
        public void ShouldReturnLoadSystemConfigurationResult_WhenJsonValid<TType>(
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

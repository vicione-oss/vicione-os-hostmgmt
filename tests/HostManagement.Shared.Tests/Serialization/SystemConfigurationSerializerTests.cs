using System.Text.Json;
using FluentAssertions;
using Xunit;
using HostManagement.Shared.Contracts;
using HostManagement.Shared.Serialization;

namespace HostManagement.Shared.Tests.Serialization;

public class SystemConfigurationSerializerTests
{
    private static readonly SystemConfiguration s_newSystemConfiguration = new();
    private static readonly string s_newJsonSystemConfiguration =
        JsonSerializer.Serialize(s_newSystemConfiguration);

    public class Deserialize
    {
        [Fact]
        public void ShouldThrow_WhenJsonContainsLiteralNull()
        {
            // Act
            var literalNullJson = "null";
            var result = async () => await SystemConfigurationSerializer.Deserialize(literalNullJson);

            // Assert
            result.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage(
                    "JSON string could not be deserialized to 'SystemConfiguration'." +
                    Environment.NewLine +
                    $"JSON string was: '{literalNullJson}'");
        }

        [Fact]
        public async Task ShouldReturnSystemConfiguration_WhenJsonValid()
        {
            // Act
            var result = await SystemConfigurationSerializer.Deserialize(s_newJsonSystemConfiguration);

            // Assert
            result.Should().BeOfType<SystemConfiguration>();
        }
    }

    public class Serialize
    {
        [Fact]
        public async Task ShouldSerializeSystemConfiguration_WhenCalled()
        {
            // Act
            var result = await SystemConfigurationSerializer.Serialize(s_newSystemConfiguration);

            // Assert
            result.Should().Be(s_newJsonSystemConfiguration);
        }
    }
}

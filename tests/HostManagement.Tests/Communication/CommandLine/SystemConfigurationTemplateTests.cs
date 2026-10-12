using System.Text;
using System.Text.Json;
using AwesomeAssertions;
using HostManagement.Communication.CommandLine;
using Xunit;

namespace HostManagement.Tests.Communication.CommandLine;

public sealed class SystemConfigurationTemplateTests
{
    public sealed class CreateAndOutput
    {
        [Fact]
        public void ShouldOutputSystemConfigurationTemplate_WhenCalled()
        {
            // Arrange
            using var writer = new StringWriter();

            // Act
            SystemConfigurationTemplate.CreateAndOutput(writer);

            // Assert
            using var json = JsonDocument.Parse(writer.ToString());

            var root = json.RootElement;
            root.GetProperty("Version").GetInt32().Should().Be(2);

            var interfaces = root.GetProperty("NetworkInterfaces");
            interfaces.GetArrayLength().Should().Be(2);

            interfaces[0]
                .GetProperty("CommonInformation")
                .GetProperty("Name")
                .GetString()
                .Should().Be("interface1");

            interfaces[0]
                .GetProperty("IPv4")
                .GetProperty("DHCPEnabled")
                .GetBoolean()
                .Should().BeTrue();

            interfaces[1]
                .GetProperty("IPv4")
                .GetProperty("IPv4Details")
                .GetArrayLength()
                .Should().Be(2);

            root.GetProperty("NetworkDNSSettings")
                .GetProperty("Hostname")
                .GetString()
                .Should().Be("hostname");

            root.GetProperty("NetworkProxySettings")
                .GetProperty("HTTP")
                .GetProperty("Port")
                .GetInt32()
                .Should().Be(80);

            root.GetProperty("NetworkNTPSettings")
                .GetProperty("Servers")
                .GetArrayLength()
                .Should().Be(2);

            root.GetProperty("Services")
                .GetArrayLength()
                .Should().Be(2);
        }
    }

    public sealed class Validate
    {
        private const string ValidConfiguration =
            """
            {
              "Version": 2,
              "NetworkInterfaces": [
                {
                  "CommonInformation": {
                    "Enabled": true,
                    "Name": "lan1"
                  },
                  "IPv4": {
                    "DHCPEnabled": true
                  }
                }
              ],
              "NetworkDNSSettings": {
                "Hostname": "host"
              }
            }
            """;

        [Fact]
        public async Task ShouldThrow_WhenConfigurationIsLiteralNull()
        {
            // Arrange
            await using var stream = CreateStream("null");

            // Act
            var func = async () => await SystemConfigurationTemplate.Validate(stream);

            // Assert
            await func.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("UTF-8 JSON stream could not be deserialized to 'SystemConfiguration'.");
        }

        [Fact]
        public async Task ShouldThrow_WhenConfigurationContainsMalformedJson()
        {
            // Arrange
            await using var stream = CreateStream(
                """
                {
                  "Version": 2,
                """);

            // Act
            var func = async () => await SystemConfigurationTemplate.Validate(stream);

            // Assert
            await func.Should().ThrowAsync<JsonException>();
        }

        [Fact]
        public async Task ShouldOutputValidationErrors_WhenConfigurationInvalid()
        {
            // Arrange
            var configuration =
                """
                {
                  "Version": 2,
                  "NetworkInterfaces": [
                    {
                      "CommonInformation": {
                        "Enabled": true,
                        "Name": "lo"
                      },
                      "IPv4": {
                        "DHCPEnabled": true
                      }
                    }
                  ],
                  "NetworkDNSSettings": {
                    "Hostname": "host"
                  }
                }
                """;

            await using var stream = CreateStream(configuration);
            using var writer = new StringWriter();

            // Act
            await SystemConfigurationTemplate.Validate(stream, writer);

            // Assert
            writer.ToString().Should().Contain("ValidationFailed:").And.NotBe(Environment.NewLine);
        }

        [Fact]
        public async Task ShouldThrow_WhenConfigurationFileDoesNotExist()
        {
            // Arrange
            var filePath = Path.Combine(
                Path.GetTempPath(),
                $"{Guid.NewGuid():N}.json");

            // Act
            var func = () => SystemConfigurationTemplate.Validate(filePath);

            // Assert
            await func.Should().ThrowAsync<FileNotFoundException>();
        }

        [Fact]
        public async Task ShouldOutputNoValidationErrors_WhenConfigurationValid()
        {
            // Arrange
            await using var stream = CreateStream(ValidConfiguration);
            using var writer = new StringWriter();

            // Act
            await SystemConfigurationTemplate.Validate(stream, writer);

            // Assert
            writer.ToString().Should().Be(Environment.NewLine);
        }

        private static MemoryStream CreateStream(string content) => new(Encoding.UTF8.GetBytes(content));
    }
}

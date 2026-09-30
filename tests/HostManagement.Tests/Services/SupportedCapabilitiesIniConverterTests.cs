using System.Text.Json;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using HostManagement.Services;
using Xunit;

namespace HostManagement.Tests.Services;

public class SupportedCapabilitiesIniConverterTests
{
    private const string CapabilityPropertyName = "Capability";
    private const string DisabledCapabilityValue = "Disabled";
    private const string EnabledCapabilityValue = "Enabled";
    private const string SettingsSectionName = "Settings";
    private const string TopicsSectionName = "Topics";

    private const string DefaultCapabilityJsonSample =
        """
        {
            "Settings": {
                "DNS": {
                    "Name": "DNS",
                    "Capability": "Disabled",
                    "Hostname": {
                        "Name": "Hostname",
                        "Capability": "Disabled"
                    },
                    "StaticHosts": {
                        "Name": "StaticHosts",
                        "Capability": "Disabled"
                    }
                }
            },
            "Topics": {
                "GetServiceState": "Disabled",
                "GetSystemConfiguration": "Disabled"
            }
        }
        """;

    public class ToJsonDocument
    {
        public static TheoryData<string, string> InvalidIniDocuments =>
        [
            new (
                "SomeKey=Enabled",
                "INI entry at line 1 is not inside a section."),

            new (
                "[]",
                "Invalid empty INI section at line 1."),

            new (
                "[Unknown]\nSomeKey=Enabled",
                "Unknown supported capabilities INI section 'Unknown' at line 1."),

            new (
                "[Settings]\nCapability=Enabled",
                "Unsupported INI property 'Capability' at line 2. Configure child settings explicitly instead."),

            new (
                "[Settings]\nUnknownKey=Enabled",
                "Unknown supported setting 'UnknownKey'."),

            new (
                "[Settings]\nDNS=Enabled",
                "Setting 'DNS' has child settings and cannot be configured directly. Configure child settings in '[Settings.DNS]'."),

            new (
                "[Settings.DNS]\nUnknownKey=Enabled",
                "Unknown setting 'DNS.UnknownKey'."),

            new (
                "[Settings.DNS.Hostname]\nChild=Enabled",
                "The setting 'DNS.Hostname' does not support a child setting."),

            new (
                "[Settings.DNS]\nHostname=Enabled\nHostname=Disabled",
                "Duplicate INI property 'Capability' at line 3."),

            new (
                "[Topics]\nUnknownKey=Enabled",
                "Unknown supported topic 'UnknownKey'."),

            new (
                "[Topics.Invalid]\nSomeKey=Enabled",
                "Invalid INI section 'Topics.Invalid' at line 1."),

            new (
                "[Topics]\nSomeKey=Enabled\nSomeKey=Disabled",
                "Duplicate INI property 'SomeKey' at line 3."),

            new (
                "[Topics]\nSomeKey",
                "Invalid INI entry at line 2."),

            new (
                "[Topics]\nSomeKey=Unknown",
                "Invalid capability value 'Unknown' at line 2.")
        ];

        [Theory]
        [MemberData(nameof(InvalidIniDocuments))]
        public void ShouldThrow_WhenIniDocumentInvalid(string iniContent, string expectedMessage)
        {
            // Arrange
            var defaultCapabilitiesJsonObject = CreateTestDefaultSupportedCapabilitiesJsonObject();

            // Act
            var act = () => SupportedCapabilitiesIniConverter.ToJsonDocument(
                iniContent,
                defaultCapabilitiesJsonObject);

            // Assert
            act.Should().Throw<FormatException>().WithMessage(expectedMessage);
        }

        [Fact]
        public void ShouldReturnEmptyJsonObject_WhenIniDocumentEmpty()
        {
            // Act
            using var result = SupportedCapabilitiesIniConverter.ToJsonDocument(string.Empty);

            // Assert
            result.RootElement.ValueKind.Should().Be(JsonValueKind.Object);
            result.RootElement.EnumerateObject().Should().BeEmpty();
        }

        [Fact]
        public void ShouldReturnJsonDocument_WhenTopicsConfigured()
        {
            // Arrange
            var defaultCapabilitiesJsonObject = CreateTestDefaultSupportedCapabilitiesJsonObject();

            const string IniContent =
                """
                [Topics]
                GetServiceState=enabled
                GetSystemConfiguration=DISABLED
                """;

            // Act
            using var result = SupportedCapabilitiesIniConverter.ToJsonDocument(
                IniContent,
                defaultCapabilitiesJsonObject);

            // Assert
            var topics = result.RootElement.GetProperty(TopicsSectionName);
            topics.GetProperty("GetServiceState").GetString().Should().Be(EnabledCapabilityValue);
            topics.GetProperty("GetSystemConfiguration").GetString().Should().Be(DisabledCapabilityValue);
        }

        [Fact]
        public void ShouldReturnJsonDocument_WhenSettingsConfigured()
        {
            // Arrange
            var defaultCapabilitiesJsonObject = CreateTestDefaultSupportedCapabilitiesJsonObject();

            const string IniContent =
                """
                [Settings.DNS]
                Hostname=enabled
                """;

            // Act
            using var result = SupportedCapabilitiesIniConverter.ToJsonDocument(
                IniContent,
                defaultCapabilitiesJsonObject);

            // Assert
            var dns = result.RootElement.GetProperty(SettingsSectionName).GetProperty("DNS");
            dns.GetProperty(CapabilityPropertyName).GetString().Should().Be(EnabledCapabilityValue);
            dns.GetProperty("Hostname").GetProperty(CapabilityPropertyName).GetString().Should().Be(EnabledCapabilityValue);
        }

        [Fact]
        public void ShouldReturnJsonDocument_WhenSettingsAndTopicsConfigured()
        {
            // Arrange
            var defaultCapabilitiesJsonObject = CreateTestDefaultSupportedCapabilitiesJsonObject();

            const string IniContent =
                """
                # comment
                ; comment

                [Settings.DNS]
                Hostname=enabled

                [Topics]
                GetServiceState=enabled
                GetSystemConfiguration=DISABLED
                """;

            // Act
            using var result = SupportedCapabilitiesIniConverter.ToJsonDocument(
                IniContent,
                defaultCapabilitiesJsonObject);

            // Assert
            var dns = result.RootElement.GetProperty(SettingsSectionName).GetProperty("DNS");
            dns.GetProperty(CapabilityPropertyName).GetString().Should().Be(EnabledCapabilityValue);
            dns.GetProperty("Hostname").GetProperty(CapabilityPropertyName).GetString().Should().Be(EnabledCapabilityValue);

            var topics = result.RootElement.GetProperty(TopicsSectionName);
            topics.GetProperty("GetServiceState").GetString().Should().Be(EnabledCapabilityValue);
            topics.GetProperty("GetSystemConfiguration").GetString().Should().Be(DisabledCapabilityValue);
        }
    }

    public class ToTemplateIni
    {
        [Fact]
        public void ShouldThrow_WhenJsonRootElementIsNotObject()
        {
            // Arrange
            using var document = JsonDocument.Parse("\"Enabled\"");

            // Act
            var act = () => SupportedCapabilitiesIniConverter.ToTemplateIni(document);

            // Assert
            act.Should()
                .Throw<InvalidOperationException>()
                .WithMessage("The 'SupportedCapabilities' JSON root element must be an object.");
        }

        [Fact]
        public void ShouldThrow_WhenJsonContainsUnsupportedTopLevelSection()
        {
            // Arrange
            using var document = JsonDocument.Parse("""{"Invalid":{}}""");

            // Act
            var act = () => SupportedCapabilitiesIniConverter.ToTemplateIni(document);

            // Assert
            act.Should()
                .Throw<InvalidOperationException>()
                .WithMessage("Invalid 'SupportedCapabilities' JSON section 'Invalid'.");
        }

        [Theory]
        [InlineData("""{"Settings":"Enabled"}""", "Settings")]
        [InlineData("""{"Topics":"Enabled"}""", "Topics")]
        public void ShouldThrow_WhenKnownSectionIsNotObject(string json, string sectionName)
        {
            // Arrange
            using var document = JsonDocument.Parse(json);

            // Act
            var act = () => SupportedCapabilitiesIniConverter.ToTemplateIni(document);

            // Assert
            act.Should()
                .Throw<InvalidOperationException>()
                .WithMessage($"The '{sectionName}' section must be a JSON object.");
        }

        [Fact]
        public void ShouldThrow_WhenCapabilityValueInvalid()
        {
            // Arrange
            using var document = JsonDocument.Parse("""{"Topics":{"GetServiceState":"Unknown"}}""");

            // Act
            var act = () => SupportedCapabilitiesIniConverter.ToTemplateIni(document);

            // Assert
            act.Should()
                .Throw<InvalidOperationException>()
                .WithMessage("Capability values must be 'Disabled' or 'Enabled'.");
        }

        [Fact]
        public void ShouldReturnHeaderOnlyTemplateIni_WhenJsonDocumentEmpty()
        {
            // Arrange
            using var document = JsonDocument.Parse("{}");

            // Act
            var result = SupportedCapabilitiesIniConverter.ToTemplateIni(document);

            // Assert
            result.Should().StartWith("# This file is managed by ViciOne open automation HostManagement application.");
            result.Should().NotContain("[Settings]\n");
            result.Should().NotContain("[Topics]\n");
        }

        [Fact]
        public void ShouldReturnTemplateIni_WhenJsonDocumentValid()
        {
            // Arrange
            using var document = JsonDocument.Parse(DefaultCapabilityJsonSample);

            // Act
            var result = SupportedCapabilitiesIniConverter.ToTemplateIni(document);

            // Assert
            result.Should().StartWith("# This file is managed by ViciOne open automation HostManagement application.");
            result.Should().Contain("[Settings.DNS]");
            result.Should().Contain("#Hostname=Disabled");
            result.Should().Contain("#StaticHosts=Disabled");
            result.Should().Contain("[Topics]");
            result.Should().Contain("#GetServiceState=Disabled");
            result.Should().Contain("#GetSystemConfiguration=Disabled");
        }
    }

    private static JsonObject CreateTestDefaultSupportedCapabilitiesJsonObject()
        => JsonNode.Parse(DefaultCapabilityJsonSample)!.AsObject();
}

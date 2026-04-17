using AwesomeAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Xunit;

namespace HostManagement.Configuration.Tests;

public sealed class EnvironmentVariableTests
{
    private static IConfiguration CreateConfiguration(IEnumerable<KeyValuePair<string, string?>> values)
        => new ConfigurationBuilder().AddInMemoryCollection(values).Build();

    public sealed class GetStringValue
    {
        private const string EnvironmentVariableKey = "HOSTMANAGEMENT:PIPE:GROUP";
        private const string EnvironmentVariableValue = "vicione-suite";
        private const string ConfiguredDefaultValue = "default-group";

        private const string ExpectedInvalidVariableString = "HOSTMANAGEMENT__PIPE__GROUP";

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ShouldReturnEmptyString_WhenKeyNotExistsAndDefaultValueNull(bool isRequired)
        {
            var config = CreateConfiguration([]);
            var variable = new EnvironmentVariable<string>(
                config,
                EnvironmentVariableKey,
                null!,
                isRequired);

            variable.IsInvalid.Should().Be(isRequired);
            variable.Value.Should().Be(string.Empty);
            AssertInvalidVariableString(variable, isRequired, ExpectedInvalidVariableString);
        }

        [Theory]
        [InlineData(false, "")]
        [InlineData(false, "  ")]
        [InlineData(true, "")]
        [InlineData(true, "  ")]
        public void ShouldReturnDefaultValue_WhenKeyNotExistsAndDefaultValueEmpty(
            bool isRequired,
            string emptyDefaultValue)
        {
            var config = CreateConfiguration([]);
            var variable = new EnvironmentVariable<string>(
                config,
                EnvironmentVariableKey,
                emptyDefaultValue,
                isRequired);

            variable.IsInvalid.Should().Be(isRequired);
            variable.Value.Should().Be(emptyDefaultValue);
            AssertInvalidVariableString(variable, isRequired, ExpectedInvalidVariableString);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ShouldReturnDefaultValueAndAlwaysBeValid_WhenKeyNotExistsAndDefaultValueNonEmpty(
            bool isRequired)
        {
            var nonEmptyDefaultValue = "non-empty";
            var config = CreateConfiguration([]);
            var variable = new EnvironmentVariable<string>(
                config,
                EnvironmentVariableKey,
                nonEmptyDefaultValue,
                isRequired);

            variable.IsInvalid.Should().BeFalse();
            variable.Value.Should().Be(nonEmptyDefaultValue);
            variable.InvalidVariableString.Should().BeEmpty();
        }

        [Theory]
        [InlineData(false, "")]
        [InlineData(false, "  ")]
        [InlineData(true, "")]
        [InlineData(true, "  ")]
        public void ShouldReturnValue_WhenValueIsEmpty(bool isRequired, string emptyValue)
        {
            var config = CreateConfiguration([new(EnvironmentVariableKey, emptyValue)]);
            var variable = new EnvironmentVariable<string>(
                config,
                EnvironmentVariableKey,
                ConfiguredDefaultValue,
                isRequired);

            variable.IsInvalid.Should().Be(isRequired);
            variable.Value.Should().Be(emptyValue);
            AssertInvalidVariableString(variable, isRequired, ExpectedInvalidVariableString);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ShouldReturnValueAndAlwaysBeValid_WhenValueNonEmpty(bool isRequired)
        {
            var config = CreateConfiguration([new(EnvironmentVariableKey, EnvironmentVariableValue)]);
            var variable = new EnvironmentVariable<string>(
                config,
                EnvironmentVariableKey,
                ConfiguredDefaultValue,
                isRequired);

            variable.IsInvalid.Should().BeFalse();
            variable.Value.Should().Be(EnvironmentVariableValue);
            variable.InvalidVariableString.Should().BeEmpty();
        }
    }

    public sealed class GetIndexedStringListValue
    {
        private const string EnvironmentVariableBaseKey = "HOSTMANAGEMENT:PUBLIC_KEYS";
        private const string EnvironmentVariableIndexedKey0 = "HOSTMANAGEMENT:PUBLIC_KEYS:0";
        private const string EnvironmentVariableIndexedKey1 = "HOSTMANAGEMENT:PUBLIC_KEYS:1";
        private const string EnvironmentVariableIndexedKey2 = "HOSTMANAGEMENT:PUBLIC_KEYS:2";

        private const string EnvironmentVariableIndexedValue0 = "key-0";
        private const string EnvironmentVariableIndexedValue1 = "key-1";
        private const string EnvironmentVariableIndexedValue2 = "key-2";

        private static readonly IReadOnlyCollection<string> s_configuredDefaultValue = [];

        private const string ExpectedInvalidVariableString = "HOSTMANAGEMENT__PUBLIC_KEYS";

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ShouldReturnEmptyCollection_WhenKeyNotExistsAndDefaultValueNull(bool isRequired)
        {
            var config = CreateConfiguration([]);
            var variable = new EnvironmentVariable<IReadOnlyCollection<string>>(
                config,
                EnvironmentVariableBaseKey,
                null!,
                isRequired,
                isIndexed: true);

            variable.IsInvalid.Should().Be(isRequired);
            variable.Value.Should().Equal([]);
            AssertInvalidVariableString(variable, isRequired, ExpectedInvalidVariableString);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ShouldReturnEmptyCollection_WhenKeyNotExistsAndDefaultValueEmpty(bool isRequired)
        {
            var config = CreateConfiguration([]);
            var variable = new EnvironmentVariable<IReadOnlyCollection<string>>(
                config,
                EnvironmentVariableBaseKey,
                [],
                isRequired,
                isIndexed: true);

            variable.IsInvalid.Should().Be(isRequired);
            variable.Value.Should().Equal([]);
            AssertInvalidVariableString(variable, isRequired, ExpectedInvalidVariableString);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ShouldReturnDefaultValueAndAlwaysBeValid_WhenKeyNotExistsAndDefaultValueNonEmpty(bool isRequired)
        {
            IReadOnlyCollection<string> nonEmptyDefaultValue =
                [EnvironmentVariableIndexedValue0, EnvironmentVariableIndexedValue1];

            var config = CreateConfiguration([]);
            var variable = new EnvironmentVariable<IReadOnlyCollection<string>>(
                config,
                EnvironmentVariableBaseKey,
                nonEmptyDefaultValue,
                isRequired,
                isIndexed: true);

            variable.IsInvalid.Should().BeFalse();
            variable.Value.Should().Equal(nonEmptyDefaultValue);
            variable.InvalidVariableString.Should().BeEmpty();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ShouldReturnPartialValue_WhenKeyHasNonIntegerIndex(bool isRequired)
        {
            var invalidIndexKey = "HOSTMANAGEMENT:PUBLIC_KEYS:invalid";
            var invalidIndexKeyString = "HOSTMANAGEMENT__PUBLIC_KEYS__invalid";

            var config = CreateConfiguration(
            [
                new(EnvironmentVariableIndexedKey0, EnvironmentVariableIndexedValue0),
                new(EnvironmentVariableIndexedKey1, EnvironmentVariableIndexedValue1),
                new(invalidIndexKey, EnvironmentVariableIndexedValue2),
            ]);

            IReadOnlyCollection<string> expectedValue =
            [
                EnvironmentVariableIndexedValue0,
                EnvironmentVariableIndexedValue1
            ];

            var variable = new EnvironmentVariable<IReadOnlyCollection<string>>(
                config,
                EnvironmentVariableBaseKey,
                s_configuredDefaultValue,
                isRequired,
                isIndexed: true);

            variable.IsInvalid.Should().Be(isRequired);
            variable.Value.Should().Equal(expectedValue);
            AssertInvalidVariableString(variable, isRequired, invalidIndexKeyString);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ShouldReturnPartialValue_WhenKeyHasNegativeIntegerIndex(bool isRequired)
        {
            var invalidIndexedKey = "HOSTMANAGEMENT:PUBLIC_KEYS:-42";
            var invalidIndexedKeyString = "HOSTMANAGEMENT__PUBLIC_KEYS__-42";

            var config = CreateConfiguration(
            [
                new(EnvironmentVariableIndexedKey0, EnvironmentVariableIndexedValue0),
                new(EnvironmentVariableIndexedKey1, EnvironmentVariableIndexedValue1),
                new(invalidIndexedKey, EnvironmentVariableIndexedValue2),
            ]);

            IReadOnlyCollection<string> expectedValue =
            [
                EnvironmentVariableIndexedValue0,
                EnvironmentVariableIndexedValue1
            ];

            var variable = new EnvironmentVariable<IReadOnlyCollection<string>>(
                config,
                EnvironmentVariableBaseKey,
                s_configuredDefaultValue,
                isRequired,
                isIndexed: true);

            variable.IsInvalid.Should().Be(isRequired);
            variable.Value.Should().Equal(expectedValue);
            AssertInvalidVariableString(variable, isRequired, invalidIndexedKeyString);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ShouldReturnPartialValue_WhenMultipleKeysInvalid(bool isRequired)
        {
            var invalidIndexKey = "HOSTMANAGEMENT:PUBLIC_KEYS:invalid";
            var invalidIndexKeyString = "HOSTMANAGEMENT__PUBLIC_KEYS__invalid";
            var invalidValueKey = "HOSTMANAGEMENT:PUBLIC_KEYS:42";
            var invalidValueKeyString = "HOSTMANAGEMENT__PUBLIC_KEYS__42";

            var expectedInvalidVariableString = string.Join(
                Environment.NewLine,
                invalidIndexKeyString,
                invalidValueKeyString);

            var config = CreateConfiguration(
            [
                new(EnvironmentVariableIndexedKey0, EnvironmentVariableIndexedValue0),
                new(EnvironmentVariableIndexedKey1, EnvironmentVariableIndexedValue1),
                new(invalidIndexKey, EnvironmentVariableIndexedValue2),
                new(invalidValueKey, "  "),
            ]);

            IReadOnlyCollection<string> expectedValue =
            [
                EnvironmentVariableIndexedValue0,
                EnvironmentVariableIndexedValue1,
                "  "
            ];

            var variable = new EnvironmentVariable<IReadOnlyCollection<string>>(
                config,
                EnvironmentVariableBaseKey,
                s_configuredDefaultValue,
                isRequired,
                isIndexed: true);

            variable.IsInvalid.Should().Be(isRequired);
            variable.Value.Should().Equal(expectedValue);
            AssertInvalidVariableString(variable, isRequired, expectedInvalidVariableString);
        }

        [Theory]
        [InlineData(false, "")]
        [InlineData(false, "  ")]
        [InlineData(true, "")]
        [InlineData(true, "  ")]
        public void ShouldReturnValue_WhenValueIsEmpty(bool isRequired, string emptyValue)
        {
            var invalidIndexedKey = "HOSTMANAGEMENT:PUBLIC_KEYS:42";
            var invalidIndexedKeyString = "HOSTMANAGEMENT__PUBLIC_KEYS__42";

            var config = CreateConfiguration(
            [
                new(EnvironmentVariableIndexedKey0, EnvironmentVariableIndexedValue0),
                new(EnvironmentVariableIndexedKey1, EnvironmentVariableIndexedValue1),
                new(invalidIndexedKey, emptyValue),
            ]);

            IReadOnlyCollection<string> expectedValue =
            [
                EnvironmentVariableIndexedValue0,
                EnvironmentVariableIndexedValue1,
                emptyValue
            ];

            var variable = new EnvironmentVariable<IReadOnlyCollection<string>>(
                config,
                EnvironmentVariableBaseKey,
                s_configuredDefaultValue,
                isRequired,
                isIndexed: true);

            variable.IsInvalid.Should().Be(isRequired);
            variable.Value.Should().Equal(expectedValue);
            AssertInvalidVariableString(variable, isRequired, invalidIndexedKeyString);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ShouldReturnValueAndAlwaysBeValid_WhenValueNonEmpty(bool isRequired)
        {
            var config = CreateConfiguration(
            [
                new(EnvironmentVariableIndexedKey0, EnvironmentVariableIndexedValue0),
                new(EnvironmentVariableIndexedKey1, EnvironmentVariableIndexedValue1)
            ]);

            IReadOnlyCollection<string> expectedValue =
            [
                EnvironmentVariableIndexedValue0,
                EnvironmentVariableIndexedValue1
            ];

            var variable = new EnvironmentVariable<IReadOnlyCollection<string>>(
                config,
                EnvironmentVariableBaseKey,
                s_configuredDefaultValue,
                isRequired,
                isIndexed: true);

            variable.IsInvalid.Should().BeFalse();
            variable.Value.Should().Equal(expectedValue);
            variable.InvalidVariableString.Should().BeEmpty();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ShouldReturnValueOrderedByIndexAndAlwaysBeValid_WhenValueNonEmpty(bool isRequired)
        {
            var config = CreateConfiguration(
            [
                new(EnvironmentVariableIndexedKey2, EnvironmentVariableIndexedValue2),
                new(EnvironmentVariableIndexedKey0, EnvironmentVariableIndexedValue0),
                new(EnvironmentVariableIndexedKey1, EnvironmentVariableIndexedValue1),
            ]);

            IReadOnlyCollection<string> expectedValue =
            [
                EnvironmentVariableIndexedValue0,
                EnvironmentVariableIndexedValue1,
                EnvironmentVariableIndexedValue2
            ];

            var variable = new EnvironmentVariable<IReadOnlyCollection<string>>(
                config,
                EnvironmentVariableBaseKey,
                s_configuredDefaultValue,
                isRequired,
                isIndexed: true);

            variable.IsInvalid.Should().BeFalse();
            variable.Value.Should().Equal(expectedValue);
            variable.InvalidVariableString.Should().BeEmpty();
        }

    }

    public sealed class GetStringListValue
    {
        private const string EnvironmentVariableKey = "HOSTMANAGEMENT:CONFIGURABLE_SERVICES";
        private const string EnvironmentVariableValue = "ssh.service, remoteconnect.service";
        private const string EnvironmentVariableValueWithEmptyEntries = "ssh.service, , remoteconnect.service,,";

        private static readonly IReadOnlyCollection<string> s_configuredDefaultValue = [];
        private static readonly IReadOnlyCollection<string> s_expectedValue = ["ssh.service", "remoteconnect.service"];

        private const string ExpectedInvalidVariableString = "HOSTMANAGEMENT__CONFIGURABLE_SERVICES";

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ShouldReturnEmptyCollection_WhenKeyNotExistsAndDefaultValueNull(bool isRequired)
        {
            var config = CreateConfiguration([]);
            var variable = new EnvironmentVariable<IReadOnlyCollection<string>>(
                config,
                EnvironmentVariableKey,
                null!,
                isRequired);

            variable.IsInvalid.Should().Be(isRequired);
            variable.Value.Should().Equal([]);
            AssertInvalidVariableString(variable, isRequired, ExpectedInvalidVariableString);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ShouldReturnEmptyCollection_WhenKeyNotExistsAndDefaultValueEmpty(bool isRequired)
        {
            var config = CreateConfiguration([]);
            var variable = new EnvironmentVariable<IReadOnlyCollection<string>>(
                config,
                EnvironmentVariableKey,
                [],
                isRequired);

            variable.IsInvalid.Should().Be(isRequired);
            variable.Value.Should().Equal([]);
            AssertInvalidVariableString(variable, isRequired, ExpectedInvalidVariableString);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ShouldReturnDefaultValueAndAlwaysBeValid_WhenKeyNotExistsAndDefaultValueNonEmpty(bool isRequired)
        {
            IReadOnlyCollection<string> nonEmptyDefaultValue = ["ssh.service", "remoteconnect.service"];
            var config = CreateConfiguration([]);
            var variable = new EnvironmentVariable<IReadOnlyCollection<string>>(
                config,
                EnvironmentVariableKey,
                nonEmptyDefaultValue,
                isRequired);

            variable.IsInvalid.Should().BeFalse();
            variable.Value.Should().Equal(nonEmptyDefaultValue);
            variable.InvalidVariableString.Should().BeEmpty();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ShouldReturnValue_WhenValueIsEmpty(bool isRequired)
        {
            var config = CreateConfiguration([new(EnvironmentVariableKey, "")]);
            var variable = new EnvironmentVariable<IReadOnlyCollection<string>>(
                config,
                EnvironmentVariableKey,
                s_configuredDefaultValue,
                isRequired);

            variable.IsInvalid.Should().Be(isRequired);
            variable.Value.Should().BeEmpty();
            AssertInvalidVariableString(variable, isRequired, ExpectedInvalidVariableString);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ShouldReturnValueAndAlwaysBeValid_WhenValueNonEmpty(bool isRequired)
        {
            var config = CreateConfiguration([new(EnvironmentVariableKey, EnvironmentVariableValue)]);
            var variable = new EnvironmentVariable<IReadOnlyCollection<string>>(
                config,
                EnvironmentVariableKey,
                s_configuredDefaultValue,
                isRequired);

            variable.IsInvalid.Should().BeFalse();
            variable.Value.Should().Equal(s_expectedValue);
            variable.InvalidVariableString.Should().BeEmpty();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ShouldReturnValueAndAlwaysBeValid_WhenValueNonEmptyWithEmptyEntries(bool isRequired)
        {
            var config = CreateConfiguration([new(EnvironmentVariableKey, EnvironmentVariableValueWithEmptyEntries)]);
            var variable = new EnvironmentVariable<IReadOnlyCollection<string>>(
                config,
                EnvironmentVariableKey,
                s_configuredDefaultValue,
                isRequired);

            variable.IsInvalid.Should().BeFalse();
            variable.Value.Should().Equal(s_expectedValue);
            variable.InvalidVariableString.Should().BeEmpty();
        }
    }

    public sealed class GetEnumValue
    {
        private const string EnvironmentVariableKey = "LOGGING:LOGLEVEL:DEFAULT";
        private const string InvalidEnvironmentVariableValue = "Invalid";

        private const LogLevel ConfiguredDefaultValue = LogLevel.Warning;

        private const string ExpectedInvalidVariableString = "LOGGING__LOGLEVEL__DEFAULT";

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ShouldReturnDefaultValue_WhenEnumCouldNotBeParsed(bool isRequired)
        {
            var config = CreateConfiguration([new(EnvironmentVariableKey, InvalidEnvironmentVariableValue)]);
            var variable = new EnvironmentVariable<LogLevel>(
                config,
                EnvironmentVariableKey,
                ConfiguredDefaultValue,
                isRequired);

            variable.IsInvalid.Should().Be(isRequired);
            variable.Value.Should().Be(ConfiguredDefaultValue);
            AssertInvalidVariableString(variable, isRequired, ExpectedInvalidVariableString);
        }

        [Theory]
        [InlineData(false, "error")]
        [InlineData(false, "Error")]
        [InlineData(true, "error")]
        [InlineData(true, "Error")]
        public void ShouldReturnValueAndAlwaysBeValid_WhenEnumCouldBeParsedIgnoringCase(
            bool isRequired,
            string configValue)
        {
            var config = CreateConfiguration([new(EnvironmentVariableKey, configValue)]);
            var variable = new EnvironmentVariable<LogLevel>(
                config,
                EnvironmentVariableKey,
                ConfiguredDefaultValue,
                isRequired);

            variable.IsInvalid.Should().BeFalse();
            variable.Value.Should().Be(LogLevel.Error);
            variable.InvalidVariableString.Should().BeEmpty();
        }
    }

    public sealed class GetTargetEnvironmentValue
    {
        private const string EnvironmentVariableKey = "HOSTMANAGEMENT:TARGET_ENVIRONMENT";
        private const string EnvironmentVariableValue = "debian";
        private const string InvalidEnvironmentVariableValue = "invalid";

        private const TargetEnvironment ConfiguredDefaultValue = TargetEnvironment.Unknown;

        private const string ExpectedInvalidVariableString = "HOSTMANAGEMENT__TARGET_ENVIRONMENT";

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ShouldReturnUnknownEnvironmentValue_WhenEnvironmentStringInvalid(bool isRequired)
        {
            // Should discard and use value from 'TargetEnvironmentExtensions.FromEnvironmentString'
            var defaultConfiguredValue = TargetEnvironment.Debian;

            var config = CreateConfiguration([new(EnvironmentVariableKey, InvalidEnvironmentVariableValue)]);
            var variable = new EnvironmentVariable<TargetEnvironment>(
                config,
                EnvironmentVariableKey,
                defaultConfiguredValue,
                isRequired);

            variable.IsInvalid.Should().Be(isRequired);
            variable.Value.Should().Be(TargetEnvironment.Unknown);
            AssertInvalidVariableString(variable, isRequired, ExpectedInvalidVariableString);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ShouldReturnValue_WhenEnvironmentStringValid(bool isRequired)
        {
            var config = CreateConfiguration([new(EnvironmentVariableKey, EnvironmentVariableValue)]);
            var variable = new EnvironmentVariable<TargetEnvironment>(
                config,
                EnvironmentVariableKey,
                ConfiguredDefaultValue,
                isRequired);

            variable.IsInvalid.Should().BeFalse();
            variable.Value.Should().Be(TargetEnvironment.Debian);
            variable.InvalidVariableString.Should().BeEmpty();
        }
    }

    public sealed class UnsupportedType
    {
        private const string UnsupportedEnvironmentVariableKey = "HOSTMANAGEMENT:UNSUPPORTED";
        private const int UnsupportedEnvironmentVariableValue = 42;

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ShouldThrow_WhenTypeIsNotSupported(bool isRequired)
        {
            var config = CreateConfiguration([]);
            var action = () => new EnvironmentVariable<int>(
                config,
                UnsupportedEnvironmentVariableKey,
                UnsupportedEnvironmentVariableValue,
                isRequired);

            action.Should()
                .Throw<NotSupportedException>()
                .WithMessage("Unsupported environment variable type: System.Int32");
        }
    }

    private static void AssertInvalidVariableString(
        IEnvironmentVariable variable,
        bool isInvalid,
        string expectedInvalidVariableString)
    {
        variable.InvalidVariableString.Should().Be(isInvalid ? expectedInvalidVariableString : string.Empty);
    }
}

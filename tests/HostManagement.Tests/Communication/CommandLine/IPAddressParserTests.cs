using System.CommandLine;
using System.Net;
using AwesomeAssertions;
using Xunit;

namespace HostManagement.Communication.CommandLine.Tests;

public sealed class IPAddressParserTests
{
    public sealed class ParseSingle
    {
        [Theory]
        [InlineData("not-an-ip-address")]
        [InlineData("192.168.0.256")]
        [InlineData("192.168.0.1.5")]
        [InlineData("2001:zb8::1")]
        public void ShouldAddError_WhenInvalidIPAddress(string addressString)
        {
            // Arrange
            var argument = new Argument<IPAddress>("ip-address")
            {
                CustomParser = IPAddressParser.ParseSingle
            };

            var command = new RootCommand { argument };

            // Act
            var parseResult = command.Parse(addressString);

            // Assert
            parseResult.Errors.Should().ContainSingle()
                .Which.Message.Should().Be($"Invalid IP address: '{addressString}'.");
        }

        [Theory]
        [InlineData("0.0.0.0")]
        [InlineData("192.168.0")] // Gets parsed to 192.168.0.0
        [InlineData("192.168.0.1")]
        [InlineData("255.255.255.255")]
        [InlineData("::1")]
        [InlineData("2001:db8::1")]
        public void ShouldReturnParsedIPAddress_WhenValid(string addressString)
        {
            // Arrange
            var argument = new Argument<IPAddress>("ip-address")
            {
                CustomParser = IPAddressParser.ParseSingle
            };

            var command = new RootCommand { argument };
            var expectedValue = IPAddress.Parse(addressString);

            // Act
            var parseResult = command.Parse(addressString);
            var actualValue = parseResult.GetValue(argument);

            // Assert
            actualValue.Should().Be(expectedValue);
            parseResult.Errors.Should().BeEmpty();
        }
    }

    public sealed class ParseMultiple
    {
        [Fact]
        public void ShouldAddErrors_WhenSomeInvalidIPAddresses()
        {
            // Arrange
            var argument = new Argument<IPAddress[]>("ip-addresses")
            {
                Arity = ArgumentArity.OneOrMore,
                CustomParser = IPAddressParser.ParseMultiple
            };

            var command = new RootCommand { argument };

            var expectedValues = new[]
            {
                IPAddress.Parse("192.168.0.1"),
                IPAddress.Parse("::1")
            };

            // Act
            var parseResult = command.Parse(
                [
                    "192.168.0.1",
                    "invalid-address",
                    "::1",
                    "10.0.0.256"
                ]);

            // Assert
            parseResult.Errors.Select(error => error.Message).Should().Equal(
                "Invalid IP address: 'invalid-address'.",
                "Invalid IP address: '10.0.0.256'.");
        }

        [Fact]
        public void ShouldAddErrors_WhenAllInvalid()
        {
            // Arrange
            var argument = new Argument<IPAddress[]>("ip-addresses")
            {
                Arity = ArgumentArity.OneOrMore,
                CustomParser = IPAddressParser.ParseMultiple
            };

            var command = new RootCommand { argument };

            // Act
            var parseResult = command.Parse(
                [
                    "invalid-address",
                    "192.168.0.256"
                ]);

            // Assert
            parseResult.Errors.Select(error => error.Message).Should().Equal(
                "Invalid IP address: 'invalid-address'.",
                "Invalid IP address: '192.168.0.256'.");
        }

        [Fact]
        public void ShouldPreserveDuplicatesAndOrder_WhenDuplicateIPAddresses()
        {
            // Arrange
            var argument = new Argument<IPAddress[]>("ip-addresses")
            {
                Arity = ArgumentArity.OneOrMore,
                CustomParser = IPAddressParser.ParseMultiple
            };

            var command = new RootCommand { argument };

            var expectedValues = new[]
            {
                IPAddress.Parse("10.0.0.1"),
                IPAddress.Parse("192.168.0.1"),
                IPAddress.Parse("10.0.0.1")
            };

            // Act
            var parseResult = command.Parse(
                [
                    "10.0.0.1",
                    "192.168.0.1",
                    "10.0.0.1"
                ]);

            var actualValues = parseResult.GetValue(argument);

            // Assert
            actualValues.Should().Equal(expectedValues);
            parseResult.Errors.Should().BeEmpty();
        }

        [Fact]
        public void ShouldReturnAllParsedIPAddresses_WhenValid()
        {
            // Arrange
            var argument = new Argument<IPAddress[]>("ip-addresses")
            {
                Arity = ArgumentArity.OneOrMore,
                CustomParser = IPAddressParser.ParseMultiple
            };

            var command = new RootCommand { argument };

            var expectedValues = new[]
            {
                IPAddress.Parse("192.168.0.1"),
                IPAddress.Parse("10.0.0.1"),
                IPAddress.Parse("2001:db8::1")
            };

            // Act
            var parseResult = command.Parse(
                [
                    "192.168.0.1",
                    "10.0.0.1",
                    "2001:db8::1"
                ]);

            var actualValues = parseResult.GetValue(argument);

            // Assert
            actualValues.Should().Equal(expectedValues);
            parseResult.Errors.Should().BeEmpty();
        }
    }
}

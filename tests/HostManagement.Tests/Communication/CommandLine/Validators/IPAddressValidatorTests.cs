using System.CommandLine;
using System.Net;
using AwesomeAssertions;
using HostManagement.Communication.CommandLine.Validators;
using Xunit;

namespace HostManagement.Communication.CommandLine.Tests.Validators;

public sealed class IPAddressValidatorTests
{
    private const string IpAddressDescription = "IP address";

    public sealed class ValidateWithArgumentResult
    {
        [Theory]
        [InlineData("localhost", 1)]
        [InlineData("1.2", 2)]
        [InlineData("192.168.1", 3)]
        [InlineData("192.168.1.1.5", 5)]
        public void ShouldAddErrors_WhenArgumentWithoutExactlyFourOctets(string addressString, int octetCount)
        {
            // Arrange
            var argument = CreateArgument();
            var command = new RootCommand { argument };

            argument.Validators.Add(result => IPAddressValidator.Validate(result, IpAddressDescription));

            // Act
            var parseResult = command.Parse(addressString);

            // Assert
            parseResult.Errors.Should().ContainSingle()
                .Which.Message.Should().ContainAll(IpAddressDescription, octetCount.ToString());
        }

        [Fact]
        public void ShouldAddOneAggregatedError_WhenMultipleInvalidArguments()
        {
            // Arrange
            var argument = CreateArgument();
            var command = new RootCommand { argument };

            argument.Validators.Add(result => IPAddressValidator.Validate(result, IpAddressDescription));

            // Act
            var parseResult = command.Parse(["192.168.1", "10.0.0.1.25"]);

            // Assert
            parseResult.Errors.Should().ContainSingle().Which.Message.Should()
                .ContainAll("must have exactly 4 octets", "3", "5").And
                .NotEndWith("\r").And
                .NotEndWith("\n");
        }

        [Theory]
        [InlineData("0.0.0.0")]
        [InlineData("1.0.3.4")]
        [InlineData("192.168.0.1")]
        [InlineData("255.255.255.255")]
        public void ShouldNotAddError_WhenArgumentWithExactlyFourOctets(string addressString)
        {
            // Arrange
            var argument = CreateArgument();
            var command = new RootCommand { argument };

            argument.Validators.Add(result => IPAddressValidator.Validate(result, IpAddressDescription));

            // Act
            var parseResult = command.Parse(addressString);

            // Assert
            parseResult.Errors.Should().BeEmpty();
        }

        private static Argument<IPAddress[]> CreateArgument()
            => new("addresses") { Arity = ArgumentArity.OneOrMore };
    }

    public sealed class ValidateWithOptionResult
    {
        [Theory]
        [InlineData("localhost", 1)]
        [InlineData("1.2", 2)]
        [InlineData("192.168.1", 3)]
        [InlineData("192.168.1.1.5", 5)]
        public void ShouldAddErrors_WhenOptionWithoutExactlyFourOctets(string addressString, int octetCount)
        {
            // Arrange
            var option = CreateOption();
            var command = new RootCommand { option };

            option.Validators.Add(result => IPAddressValidator.Validate(result, IpAddressDescription));

            // Act
            var parseResult = command.Parse(["--ip-address", addressString]);

            // Assert
            parseResult.Errors.Should().ContainSingle().Which.Message.Should()
                .ContainAll(IpAddressDescription, octetCount.ToString());
        }

        [Theory]
        [InlineData("0.0.0.0")]
        [InlineData("1.0.3.4")]
        [InlineData("192.168.0.1")]
        [InlineData("255.255.255.255")]
        public void ShouldNotAddError_WhenOptionWithExactlyFourOctets(string addressString)
        {
            // Arrange
            var option = CreateOption();
            var command = new RootCommand { option };

            option.Validators.Add(result => IPAddressValidator.Validate(result, IpAddressDescription));

            // Act
            var parseResult = command.Parse(["--ip-address", addressString]);

            // Assert
            parseResult.Errors.Should().BeEmpty();
        }

        private static Option<string[]> CreateOption()
            => new Option<string[]>("--ip-address") { Arity = ArgumentArity.OneOrMore };
    }
}

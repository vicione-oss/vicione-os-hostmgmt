using FluentAssertions;
using Xunit;
using HostManagement.Shared.Validation.Utils;

namespace HostManagement.Shared.Tests.Validation.Utils;

public class FQDNValidatorUtilsTests
{
    public class HasDomainNameTooManyCharacters
    {
        [Fact]
        public void ShouldReturnTrue_WhenDomainNameHasMoreThan253Characters()
        {
            // Arrange
            var domainName = string.Concat("my-domain.", new string('x', 241), ".de");

            // Act
            var result = FQDNValidatorUtils.HasDomainNameTooManyCharacters(domainName);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDomainNameHas253Characters()
        {
            // Arrange
            var domainName = string.Concat("my-domain.", new string('x', 240), ".de");

            // Act
            var result = FQDNValidatorUtils.HasDomainNameTooManyCharacters(domainName);

            // Assert
            result.Should().BeFalse();
        }
    }

    public class HasDomainNameLeadingOrTrailingDots
    {
        [Theory]
        [InlineData(".ptbtime1.ptb.de")]
        [InlineData("ptbtime1.ptb.de.")]
        public void ShouldReturnTrue_WhenDomainNameHasLeadingOrTrailingDots(string domainName)
        {
            // Act
            var result = FQDNValidatorUtils.HasDomainNameLeadingOrTrailingDots(domainName);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDomainNameHasNoLeadingOrTrailingDots()
        {
            // Arrange
            var domainName = "ptbtime1.ptb.de";

            // Act
            var result = FQDNValidatorUtils.HasDomainNameLeadingOrTrailingDots(domainName);

            // Assert
            result.Should().BeFalse();
        }
    }

    public class HasDomainNameConsecutiveDots
    {
        [Fact]
        public void ShouldReturnTrue_WhenDomainNameHasConsecutiveDots()
        {
            // Arrange
            var domainName = "ptbtime1..de";

            // Act
            var result = FQDNValidatorUtils.HasDomainNameConsecutiveDots(domainName);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDomainNameHasNoConsecutiveDots()
        {
            // Arrange
            var domainName = "ptbtime1.ptb.de";

            // Act
            var result = FQDNValidatorUtils.HasDomainNameConsecutiveDots(domainName);

            // Assert
            result.Should().BeFalse();
        }
    }

    public class HasLessThanTwoDomainLabels
    {
        [Theory]
        [InlineData("")]
        [InlineData("ptbtime1")]
        public void ShouldReturnTrue_WhenDomainNameHasLessThanTwoDomainLabels(string domainName)
        {
            // Arrange
            var domainLabels = domainName.Split('.');

            // Act
            var result = FQDNValidatorUtils.HasLessThanTwoDomainLabels(domainLabels);

            // Assert
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData("my-time.de")]
        [InlineData("ptbtime1.time.de")]
        public void ShouldReturnFalse_WhenDomainNameHasAtLeastTwoDomainLabels(string domainName)
        {
            // Arrange
            var domainLabels = domainName.Split('.');

            // Act
            var result = FQDNValidatorUtils.HasLessThanTwoDomainLabels(domainLabels);

            // Assert
            result.Should().BeFalse();
        }
    }

    public class HasMoreThanOneNumericLabel
    {
        [Theory]
        [InlineData("123.abc.456.com")]
        [InlineData("ptbtime1.123.456.de")]
        public void ShouldReturnTrue_WhenDomainNameHasMoreThanOneNumericDomainLabel(string domainName)
        {
            // Arrange
            var domainLabels = domainName.Split('.');

            // Act
            var result = FQDNValidatorUtils.HasMoreThanOneNumericLabel(domainLabels);

            // Assert
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData("123.com")]
        [InlineData("123.abc.com")]
        [InlineData("ptbtime1.123.de")]
        public void ShouldReturnFalse_WhenDomainNameHasNoMoreThanOneNumericDomainLabel(string domainName)
        {
            // Arrange
            var domainLabels = domainName.Split('.');

            // Act
            var result = FQDNValidatorUtils.HasMoreThanOneNumericLabel(domainLabels);

            // Assert
            result.Should().BeFalse();
        }
    }

    public class HasTopLevelDomainInvalidCharacterCount
    {
        [Theory]
        [InlineData("d")]
        [InlineData("123456789a123456789b123456789c123456789d123456789e123456789f1234")]
        public void ShouldReturnTrue_WhenTopLevelDomainHasInvalidCharacterCount(string topLevelDomain)
        {
            // Act
            var result = FQDNValidatorUtils.HasTopLevelDomainInvalidCharacterCount(topLevelDomain);

            // Assert
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData("de")]
        [InlineData("123456789a123456789b123456789c123456789d123456789e123456789f123")]
        public void ShouldReturnFalse_WhenDomainLabelHasValidCharacterCount(string topLevelDomain)
        {
            // Act
            var result = FQDNValidatorUtils.HasTopLevelDomainInvalidCharacterCount(topLevelDomain);

            // Assert
            result.Should().BeFalse();
        }
    }

    public class HasDomainLabelInvalidCharacterCount
    {
        [Theory]
        [InlineData("")]
        [InlineData("123456789a123456789b123456789c123456789d123456789e123456789f1234")]
        public void ShouldReturnTrue_WhenDomainLabelHasInvalidCharacterCount(string domainLabel)
        {
            // Act
            var result = FQDNValidatorUtils.HasDomainLabelInvalidCharacterCount(domainLabel);

            // Assert
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData("1")]
        [InlineData("123456789a123456789b123456789c123456789d123456789e123456789f123")]
        public void ShouldReturnFalse_WhenDomainLabelHasValidCharacterCount(string domainLabel)
        {
            // Act
            var result = FQDNValidatorUtils.HasDomainLabelInvalidCharacterCount(domainLabel);

            // Assert
            result.Should().BeFalse();
        }
    }

    public class HasDomainLabelOnlyDigits
    {
        [Fact]
        public void ShouldReturnTrue_WhenDomainLabelHasOnlyDigits()
        {
            // Act
            var result = FQDNValidatorUtils.HasDomainLabelOnlyDigits("123");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldReturnFalse_WhenDomainLabelHasNotOnlyDigits()
        {
            // Act
            var result = FQDNValidatorUtils.HasDomainLabelOnlyDigits("1a2b3");

            // Assert
            result.Should().BeFalse();
        }
    }

    public class HasDomainLabelInvalidCharacter
    {
        [Theory]
        [InlineData("-ptb")]
        [InlineData("_ptb")]
        [InlineData("/ptb")]
        [InlineData("?ptb")]
        [InlineData("$ptb")]

        [InlineData("ptb-")]
        [InlineData("ptb_")]
        [InlineData("ptb/")]
        [InlineData("ptb?")]
        [InlineData("ptb$")]

        [InlineData("pt_b")]
        [InlineData("pt/b")]
        [InlineData("pt?b")]
        [InlineData("pt$b")]

        [InlineData("ápple")]
        [InlineData("gøogle")]
        public void ShouldReturnTrue_WhenDomainLabelHasInvalidCharacter(string domainLabel)
        {
            // Act
            var result = FQDNValidatorUtils.HasDomainLabelInvalidCharacter(domainLabel);

            // Assert
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData("ptbtime1")]
        [InlineData("pool")]
        [InlineData("my-time")]
        public void ShouldReturnFalse_WhenDomainLabelHasValidCharacters(string domainLabel)
        {
            // Act
            var result = FQDNValidatorUtils.HasDomainLabelInvalidCharacter(domainLabel);

            // Assert
            result.Should().BeFalse();
        }
    }
}

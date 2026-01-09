using FluentAssertions;
using Xunit;
using HostManagement.Shared.Validation;
using HostManagement.Shared.Validation.Validators.DNS;

namespace HostManagement.Shared.Tests.Validation.Validators.DNS;

public class SearchDomainsValidatorTests
{
    [Fact]
    public void ShouldHaveNoValidationError_WhenSearchDomainsDisabled()
    {
        // Arrange
        var searchDomainsEnabled = false;
        var searchDomains = new List<string>()
        {
            "local",
            "myCompany.com",
            "subDomain.anotherSearchDomain.com"
        };
        var validationResult = new ValidationResult();

        // Act
        SearchDomainsValidator.Validate(searchDomainsEnabled, searchDomains, validationResult);

        // Assert
        validationResult.IsValid.Should().BeTrue();
        validationResult.Entries.Count.Should().Be(0);
    }

    [Fact]
    public void ShouldHaveNoValidationError_WhenSearchDomainsValid()
    {
        // Arrange
        var searchDomainsEnabled = true;
        var searchDomains = new List<string>()
        {
            "local",
            "myCompany.com",
            "subDomain.anotherSearchDomain.com"
        };
        var validationResult = new ValidationResult();

        // Act
        SearchDomainsValidator.Validate(searchDomainsEnabled, searchDomains, validationResult);

        // Assert
        validationResult.IsValid.Should().BeTrue();
        validationResult.Entries.Count.Should().Be(0);
    }
}

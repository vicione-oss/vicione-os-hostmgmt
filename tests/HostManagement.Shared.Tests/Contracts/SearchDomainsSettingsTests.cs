using AwesomeAssertions;
using HostManagement.Shared.Contracts.Network;
using Xunit;

namespace HostManagement.Shared.Tests.Contracts;

public sealed class SearchDomainsSettingsTests
{
    public sealed class EqualsMethod
    {
        [Theory]
        [ClassData(typeof(DifferentSearchDomainsSettingsData))]
        public void ShouldReturnFalse_WhenDifferent(SearchDomainsSettings first, SearchDomainsSettings second)
            => first.Equals(second).Should().BeFalse();

        [Theory]
        [ClassData(typeof(EqualSearchDomainsSettingsData))]
        public void ShouldReturnTrue_WhenEqual(SearchDomainsSettings first, SearchDomainsSettings second)
            => first.Equals(second).Should().BeTrue();
    }

    public sealed class GetHashCodeMethod
    {
        [Theory]
        [ClassData(typeof(DifferentSearchDomainsSettingsData))]
        public void ShouldReturnDifferentHashCode_WhenDifferent(
            SearchDomainsSettings first,
            SearchDomainsSettings second)
        {
            // Act
            var hash1 = first.GetHashCode();
            var hash2 = second.GetHashCode();

            // Assert
            hash1.Should().NotBe(hash2);
        }

        [Theory]
        [ClassData(typeof(EqualSearchDomainsSettingsData))]
        public void ShouldReturnSameHashCode_WhenEqual(SearchDomainsSettings first, SearchDomainsSettings second)
        {
            // Act
            var hash1 = first.GetHashCode();
            var hash2 = second.GetHashCode();

            // Assert
            hash1.Should().Be(hash2);
        }
    }

#pragma warning disable CA1812
    internal sealed class DifferentSearchDomainsSettingsData : TheoryData<SearchDomainsSettings, SearchDomainsSettings>
    {
        public DifferentSearchDomainsSettingsData()
        {
            var searchDomainsSettings = new SearchDomainsSettings()
            {
                Enabled = true,
                Domains = ["example.com", "internal.example.com"]
            };

            // Enabled changed
            Add(
                searchDomainsSettings,
                searchDomainsSettings with
                {
                    Enabled = false
                });

            // Domains order changed
            Add(
                searchDomainsSettings,
                searchDomainsSettings with
                {
                    Domains =
                    [
                        "internal.example.com",
                        "example.com"
                    ]
                });

            // Second domain changed
            Add(
                searchDomainsSettings,
                searchDomainsSettings with
                {
                    Domains =
                    [
                        "example.com",
                        "other.example.com"
                    ]
                });

            // Settings completely changed
            Add(
                searchDomainsSettings,
                new SearchDomainsSettings()
                {
                    Enabled = false,
                    Domains = ["different.example.com"]
                });
        }
    }

    internal sealed class EqualSearchDomainsSettingsData : TheoryData<SearchDomainsSettings, SearchDomainsSettings>
    {
        public EqualSearchDomainsSettingsData()
        {
            Add(
                new SearchDomainsSettings()
                {
                    Enabled = true,
                    Domains =
                    [
                        "example.com",
                        "internal.example.com"
                    ]
                },
                new SearchDomainsSettings()
                {
                    Enabled = true,
                    Domains =
                    [
                        "example.com",
                        "internal.example.com"
                    ]
                });

            Add(new SearchDomainsSettings(), new SearchDomainsSettings());
        }
    }
#pragma warning restore CA1812
}

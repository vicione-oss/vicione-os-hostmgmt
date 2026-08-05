using AwesomeAssertions;
using HostManagement.Shared.Extensions;
using Xunit;

namespace HostManagement.Shared.Tests.Extensions;

public class HashCodeExtensionsTests
{
    [Fact]
    public void ShouldNotModifyHashCode_WhenValuesNull()
    {
        // Arrange
        IEnumerable<string>? values = null;
        var actual = new HashCode();
        var expected = new HashCode();
        actual.Add("prefix");
        expected.Add("prefix");

        // Act
        actual.AddRange(values);

        // Assert
        actual.ToHashCode().Should().Be(expected.ToHashCode());
    }

    [Fact]
    public void ShouldNotModifyHashCode_WhenValuesEmpty()
    {
        // Arrange
        IEnumerable<string> values = [];
        var actual = new HashCode();
        var expected = new HashCode();
        actual.Add("prefix");
        expected.Add("prefix");

        // Act
        actual.AddRange(values);

        // Assert
        actual.ToHashCode().Should().Be(expected.ToHashCode());
    }

    [Fact]
    public void ShouldAddValuesInEnumerationOrder_WhenComparerNotProvided()
    {
        // Arrange
        string[] values = ["alpha", "bravo", "charlie"];
        var actual = new HashCode();
        var expected = new HashCode();

        foreach (var value in values)
            expected.Add(value);

        // Act
        actual.AddRange(values);

        // Assert
        actual.ToHashCode().Should().Be(expected.ToHashCode());
    }

    [Fact]
    public void ShouldUseComparer_WhenComparerProvided()
    {
        // Arrange
        string[] values = ["Alpha", "BRAVO", "charlie"];
        var comparer = StringComparer.OrdinalIgnoreCase;
        var actual = new HashCode();
        var expected = new HashCode();

        foreach (var value in values)
            expected.Add(value, comparer);

        // Act
        actual.AddRange(values, comparer);

        // Assert
        actual.ToHashCode().Should().Be(expected.ToHashCode());
    }

    [Fact]
    public void ShouldAddValuesInSelectedOrder_WhenOrderProvided()
    {
        // Arrange
        string[] values = ["charlie", "alpha", "bravo"];
        var actual = new HashCode();
        var expected = new HashCode();

        expected.Add("alpha");
        expected.Add("bravo");
        expected.Add("charlie");

        // Act
        actual.AddRange(values, order: value => value);

        // Assert
        actual.ToHashCode().Should().Be(expected.ToHashCode());
    }
}

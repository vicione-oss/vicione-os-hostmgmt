using FluentAssertions;
using Xunit;
using HostManagement.Scripts.Extensions;

namespace HostManagement.Scripts.Tests.Extensions;

public class StringExtensionsTests
{
    [Theory]
    [InlineData("", "")]
    [InlineData("abc", "abc")]
    [InlineData("line1\r\nline2\r\n", "line1\nline2\n")]
    [InlineData("line1\rline2\r", "line1\nline2\n")]
    [InlineData("line1\nline2\n", "line1\nline2\n")]
    [InlineData("line1\r\nline2\rline3\n", "line1\nline2\nline3\n")]
    [InlineData("\r\n\r\r\n\n", "\n\n\n\n")]
    public void EnsureLFLineEndings_ShouldConvertAllLineEndingsToLF(string input, string expected)
        => input.EnsureLFLineEndings().Should().Be(expected);
}

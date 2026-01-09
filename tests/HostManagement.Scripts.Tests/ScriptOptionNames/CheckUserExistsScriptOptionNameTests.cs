using FluentAssertions;
using Xunit;
using HostManagement.Scripts.ScriptOptionNames;

namespace HostManagement.Scripts.Tests.ScriptOptionNames;

public class CheckUserExistsScriptOptionNameTests
{
    [Fact]
    public void User_ShouldReturnExpectedValue()
        => CheckUserExistsScriptOptionName.User.ToString().Should().Be("--user");
}

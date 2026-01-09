using AwesomeAssertions;
using Xunit;
using HostManagement.Shared.Communication.Enums;
using HostManagement.Shared.Communication.Extensions;

namespace HostManagement.Shared.Communication.Tests.Extensions;

public class OperationStatusExtensionsTests
{
    [Theory]
    [InlineData(OperationStatus.Success, OperationStatus.Success, OperationStatus.Success)]
    [InlineData(OperationStatus.Success, OperationStatus.Warning, OperationStatus.Warning)]
    [InlineData(OperationStatus.Success, OperationStatus.Error, OperationStatus.Error)]
    [InlineData(OperationStatus.Warning, OperationStatus.Success, OperationStatus.Warning)]
    [InlineData(OperationStatus.Warning, OperationStatus.Warning, OperationStatus.Warning)]
    [InlineData(OperationStatus.Warning, OperationStatus.Error, OperationStatus.Error)]
    [InlineData(OperationStatus.Error, OperationStatus.Success, OperationStatus.Error)]
    [InlineData(OperationStatus.Error, OperationStatus.Warning, OperationStatus.Error)]
    [InlineData(OperationStatus.Error, OperationStatus.Error, OperationStatus.Error)]
    public void GetCombinedStatus_ShouldReturnExpectedResult(OperationStatus first, OperationStatus second, OperationStatus combined)
        => first.GetCombinedStatus(second).Should().Be(combined);
}

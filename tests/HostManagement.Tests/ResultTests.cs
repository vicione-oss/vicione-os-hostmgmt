using AwesomeAssertions;
using HostManagement.Shared.Communication.Enums;
using Xunit;

namespace HostManagement.Tests;

public sealed class ResultTests
{
    private const string ErrorMessage = "An error occurred.";
    private const string WarningMessage = "A warning occurred.";

    public class MessagesProperty
    {
        [Fact]
        public void ShouldBeEmpty_WhenResultCreated() => new Result().Messages.Should().BeEmpty();

        [Fact]
        public void ShouldReturnMessagesInInsertionOrder_WhenMultipleMessagesAdded()
        {
            // Arrange
            var sut = new Result();
            sut.AddWarning(WarningMessage);
            sut.AddError(ErrorMessage);

            // Act
            var messages = sut.Messages;

            // Assert
            messages.Should().Be(string.Join(Environment.NewLine, WarningMessage, ErrorMessage));
        }
    }

    public class OperationStatusProperty
    {
        [Fact]
        public void ShouldBeSuccess_WhenResultCreated()
            => new Result().OperationStatus.Should().Be(OperationStatus.Success);
    }

    public class AddError
    {
        [Fact]
        public void ShouldAddMessageAndSetErrorStatus_WhenCalled()
        {
            // Arrange
            var sut = new Result();

            // Act
            sut.AddError(ErrorMessage);

            // Assert
            sut.Messages.Should().Be(ErrorMessage);
            sut.OperationStatus.Should().Be(OperationStatus.Error);
        }

        [Fact]
        public void ShouldFormatMessage_WhenArgumentsProvided()
        {
            // Arrange
            var sut = new Result();

            // Act
            sut.AddError("Error for interface '{0}'.", "lan1");

            // Assert
            sut.Messages.Should().Be("Error for interface 'lan1'.");
            sut.OperationStatus.Should().Be(OperationStatus.Error);
        }

        [Fact]
        public void ShouldUseMessageWithoutFormatting_WhenArgumentsEmpty()
        {
            // Arrange
            var sut = new Result();

            // Act
            sut.AddError(ErrorMessage, []);

            // Assert
            sut.Messages.Should().Be(ErrorMessage);
            sut.OperationStatus.Should().Be(OperationStatus.Error);
        }

        [Fact]
        public void ShouldUseMessageWithoutFormatting_WhenArgumentsNull()
        {
            // Arrange
            var sut = new Result();

            // Act
            sut.AddError(ErrorMessage, null);

            // Assert
            sut.Messages.Should().Be(ErrorMessage);
            sut.OperationStatus.Should().Be(OperationStatus.Error);
        }

        [Fact]
        public void ShouldSetErrorStatus_WhenWarningWasAddedPreviously()
        {
            // Arrange
            var sut = new Result();
            sut.AddWarning(WarningMessage);

            // Act
            sut.AddError(ErrorMessage);

            // Assert
            sut.OperationStatus.Should().Be(OperationStatus.Error);
        }
    }

    public class AddWarning
    {
        [Fact]
        public void ShouldAddMessageAndSetWarningStatus_WhenCalled()
        {
            // Arrange
            var sut = new Result();

            // Act
            sut.AddWarning(WarningMessage);

            // Assert
            sut.Messages.Should().Be(WarningMessage);
            sut.OperationStatus.Should().Be(OperationStatus.Warning);
        }

        [Fact]
        public void ShouldFormatMessage_WhenArgumentsProvided()
        {
            // Arrange
            var sut = new Result();

            // Act
            sut.AddWarning("Warning for interface '{0}'.", "lan1");

            // Assert
            sut.Messages.Should().Be("Warning for interface 'lan1'.");
            sut.OperationStatus.Should().Be(OperationStatus.Warning);
        }

        [Fact]
        public void ShouldUseMessageWithoutFormatting_WhenArgumentsEmpty()
        {
            // Arrange
            var sut = new Result();

            // Act
            sut.AddWarning(WarningMessage, []);

            // Assert
            sut.Messages.Should().Be(WarningMessage);
            sut.OperationStatus.Should().Be(OperationStatus.Warning);
        }

        [Fact]
        public void ShouldUseMessageWithoutFormatting_WhenArgumentsNull()
        {
            // Arrange
            var sut = new Result();

            // Act
            sut.AddWarning(WarningMessage, null);

            // Assert
            sut.Messages.Should().Be(WarningMessage);
            sut.OperationStatus.Should().Be(OperationStatus.Warning);
        }

        [Fact]
        public void ShouldKeepErrorStatus_WhenErrorWasAddedPreviously()
        {
            // Arrange
            var sut = new Result();
            sut.AddError(ErrorMessage);

            // Act
            sut.AddWarning(WarningMessage);

            // Assert
            sut.OperationStatus.Should().Be(OperationStatus.Error);
        }
    }
}

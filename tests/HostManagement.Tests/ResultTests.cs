using AwesomeAssertions;
using HostManagement.Shared.Communication.Enums;
using Xunit;

namespace HostManagement.Tests;

public sealed class ResultTests
{
    private const string ErrorMessage = "An error occurred.";
    private const string WarningMessage = "A warning occurred.";

    public class SuccessResultProperty
    {
        [Fact]
        public void ShouldReturnNewSuccessfulResult_WhenAccessed()
        {
            // Act
            var firstResult = Result.SuccessResult;
            var secondResult = Result.SuccessResult;

            // Assert
            firstResult.Should().NotBeSameAs(secondResult);
            firstResult.OperationStatus.Should().Be(OperationStatus.Success);
            firstResult.Messages.Should().BeEmpty();
        }
    }

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
            messages.Should().Equal(WarningMessage, ErrorMessage);
        }
    }

    public class FormattedMessagesProperty
    {
        [Fact]
        public void ShouldBeEmpty_WhenResultCreated() => new Result().FormattedMessages.Should().BeEmpty();

        [Fact]
        public void ShouldReturnMessagesInInsertionOrder_WhenMultipleMessagesAdded()
        {
            // Arrange
            var sut = new Result();
            sut.AddWarning(WarningMessage);
            sut.AddError(ErrorMessage);

            // Act
            var messages = sut.FormattedMessages;

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
            sut.FormattedMessages.Should().Be(ErrorMessage);
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
            sut.FormattedMessages.Should().Be("Error for interface 'lan1'.");
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
            sut.FormattedMessages.Should().Be(ErrorMessage);
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
            sut.FormattedMessages.Should().Be(ErrorMessage);
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
            sut.FormattedMessages.Should().Be(WarningMessage);
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
            sut.FormattedMessages.Should().Be("Warning for interface 'lan1'.");
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
            sut.FormattedMessages.Should().Be(WarningMessage);
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
            sut.FormattedMessages.Should().Be(WarningMessage);
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

    public class AddResult
    {
        [Fact]
        public void ShouldThrow_WhenResultNull()
        {
            // Arrange
            var sut = new Result();

            // Act
            var action = () => sut.AddResult(null!);

            // Assert
            action.Should().Throw<ArgumentNullException>().WithParameterName("result");
        }

        [Fact]
        public void ShouldAppendMessages_WhenResultContainsMessages()
        {
            // Arrange
            var sut = new Result();
            sut.AddWarning(WarningMessage);

            var result = new Result();
            result.AddError(ErrorMessage);

            // Act
            sut.AddResult(result);

            // Assert
            sut.Messages.Should().Equal(WarningMessage, ErrorMessage);
        }

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
        public void ShouldCombineOperationStatus_WhenResultAdded(
            OperationStatus currentStatus,
            OperationStatus addedStatus,
            OperationStatus expectedStatus)
        {
            // Arrange
            var sut = CreateResult(currentStatus);
            var result = CreateResult(addedStatus);

            // Act
            sut.AddResult(result);

            // Assert
            sut.OperationStatus.Should().Be(expectedStatus);
        }

        [Fact]
        public void ShouldReturnCurrentResult_WhenResultAdded()
        {
            // Arrange
            var sut = new Result();

            // Act
            var result = sut.AddResult(Result.SuccessResult);

            // Assert
            result.Should().BeSameAs(sut);
        }

        private static Result CreateResult(OperationStatus operationStatus)
        {
            var result = new Result();

            if (operationStatus == OperationStatus.Warning)
                result.AddWarning(WarningMessage);
            else if (operationStatus == OperationStatus.Error)
                result.AddError(ErrorMessage);

            return result;
        }
    }
}

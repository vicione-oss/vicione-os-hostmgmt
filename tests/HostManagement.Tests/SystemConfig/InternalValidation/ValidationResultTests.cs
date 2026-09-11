using AwesomeAssertions;
using HostManagement.SystemConfig.InternalValidation;
using Xunit;

namespace HostManagement.Tests.SystemConfig.InternalValidation;

public sealed class ValidationResultTests
{
    public sealed class Constructor
    {
        [Fact]
        public void ShouldCreateValidResultWithoutMessages()
        {
            // Arrange
            var sut = new ValidationResult();

            // Act
            var isValid = sut.IsValid;
            var messages = sut.Messages;
            var formattedMessages = sut.FormattedMessages;

            // Assert
            isValid.Should().BeTrue();
            messages.Should().BeEmpty();
            formattedMessages.Should().BeEmpty();
        }
    }

    public sealed class AddError
    {
        [Fact]
        public void ShouldAddMessageAndMarkResultAsInvalid()
        {
            // Arrange
            const string Message = "The configuration is invalid.";

            var sut = new ValidationResult();

            // Act
            sut.AddError(Message);

            // Assert
            sut.IsValid.Should().BeFalse();
            sut.Messages.Should().ContainSingle().Which.Should().Be(Message);
            sut.FormattedMessages.Should().Be(Message);
        }

        [Fact]
        public void ShouldPreserveMessageOrder_WhenCalledMultipleTimes()
        {
            // Arrange
            const string FirstMessage = "The first error.";
            const string SecondMessage = "The second error.";

            var expectedFormattedMessages = string.Join(Environment.NewLine, FirstMessage, SecondMessage);

            var sut = new ValidationResult();

            // Act
            sut.AddError(FirstMessage);
            sut.AddError(SecondMessage);

            // Assert
            sut.IsValid.Should().BeFalse();
            sut.Messages.Should().Equal(FirstMessage, SecondMessage);
            sut.FormattedMessages.Should().Be(expectedFormattedMessages);
        }
    }
}

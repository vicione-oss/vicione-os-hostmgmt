using AwesomeAssertions;
using HostManagement.Shared.Validation;
using Xunit;

namespace HostManagement.Shared.Tests.Validation;

public class ValidationResultTests
{
    [Fact]
    public void ShouldAddErrorMessage_WhenCalled()
    {
        // Arrange
        var errorMessage = "An error occurred.";
        var result = new ValidationResult();

        var expectedEntriesString = $"ValidationFailed: {errorMessage}";

        // Act
        result.AddError(errorMessage);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.First().Message.Should().Be(errorMessage);
        result.Entries.First().Code.Should().Be(ErrorCodes.ValidationFailed);
        result.EntriesString.Should().Be(expectedEntriesString);
    }

    [Fact]
    public void ShouldAddErrorMessages_WhenCalled()
    {
        // Arrange
        var errorMessage1 = "An error occurred.";
        var errorMessage2 = "Another error occurred.";
        var result = new ValidationResult();

        var expectedEntriesString =
            $"ValidationFailed: {errorMessage1}" +
            Environment.NewLine +
            $"ValidationFailed: {errorMessage2}";

        // Act
        result.AddError(errorMessage1);
        result.AddError(errorMessage2);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(2);
        result.ToList()[0].Message.Should().Be(errorMessage1);
        result.ToList()[0].Code.Should().Be(ErrorCodes.ValidationFailed);
        result.ToList()[1].Message.Should().Be(errorMessage2);
        result.ToList()[1].Code.Should().Be(ErrorCodes.ValidationFailed);
        result.EntriesString.Should().Be(expectedEntriesString);
    }

    [Fact]
    public void ShouldAddInformationMessage_WhenCalled()
    {
        // Arrange
        var informationMessage = "This is an information.";
        var result = new ValidationResult();

        var expectedEntriesString = $"Valid: {informationMessage}";

        // Act
        result.AddInformation(informationMessage);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Entries.Count.Should().Be(1);
        result.Entries.First().Message.Should().Be(informationMessage);
        result.Entries.First().Code.Should().Be(ErrorCodes.Valid);
        result.EntriesString.Should().Be(expectedEntriesString);
    }

    [Fact]
    public void ShouldAddInformationMessages_WhenCalled()
    {
        // Arrange
        var informationMessage1 = "This is an information.";
        var informationMessage2 = "This is another information.";
        var result = new ValidationResult();

        var expectedEntriesString =
            $"Valid: {informationMessage1}" +
            Environment.NewLine +
            $"Valid: {informationMessage2}";

        // Act
        result.AddInformation(informationMessage1);
        result.AddInformation(informationMessage2);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Entries.Count.Should().Be(2);
        result.ToList()[0].Message.Should().Be(informationMessage1);
        result.ToList()[0].Code.Should().Be(ErrorCodes.Valid);
        result.ToList()[1].Message.Should().Be(informationMessage2);
        result.ToList()[1].Code.Should().Be(ErrorCodes.Valid);
        result.EntriesString.Should().Be(expectedEntriesString);
    }

    [Fact]
    public void ShouldReturnValidResult_WhenValidCalled()
    {
        // Act
        var result = ValidationResult.Valid;

        // Assert
        result.IsValid.Should().BeTrue();
        result.Entries.Should().BeEmpty();
    }

    [Fact]
    public void ShouldReturnErrorResult_WhenCreateWithErrorCalled()
    {
        // Arrange
        var errorMessage = "An error occurred.";
        var expectedEntriesString = $"ValidationFailed: {errorMessage}";

        // Act
        var result = ValidationResult.CreateWithError(errorMessage);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Entries.Count.Should().Be(1);
        result.Entries.First().Message.Should().Be(errorMessage);
        result.Entries.First().Code.Should().Be(ErrorCodes.ValidationFailed);
        result.EntriesString.Should().Be(expectedEntriesString);
    }

    [Fact]
    public void ShouldReturnInformationResult_WhenCreateWithInformation()
    {
        // Arrange
        var informationMessage = "This is an information.";
        var expectedEntriesString = $"Valid: {informationMessage}";

        // Act
        var result = ValidationResult.CreateWithInformation(informationMessage);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Entries.Count.Should().Be(1);
        result.Entries.First().Message.Should().Be(informationMessage);
        result.Entries.First().Code.Should().Be(ErrorCodes.Valid);
        result.EntriesString.Should().Be(expectedEntriesString);
    }

    [Fact]
    public void ShouldRemainInvalid_WhenMultipleResultsAdded()
    {
        // Arrange
        var informationMessage = "An information.";
        var errorMessage = "Another error occurred.";

        var previousResult = ValidationResult.Valid;
        var currentResult = new ValidationResult();

        // Act + Assert
        currentResult.AddError(errorMessage); // start with invalid
        currentResult.IsValid.Should().BeFalse();
        currentResult.Entries.Count.Should().Be(1);
        currentResult.ToList()[0].Message.Should().Be(errorMessage);
        currentResult.ToList()[0].Code.Should().Be(ErrorCodes.ValidationFailed);
        currentResult.EntriesString.Should().Be($"ValidationFailed: {errorMessage}");

        currentResult.AddInformation(informationMessage); // remain invalid
        currentResult.IsValid.Should().BeFalse();
        currentResult.Entries.Count.Should().Be(2);
        currentResult.ToList()[0].Message.Should().Be(errorMessage);
        currentResult.ToList()[0].Code.Should().Be(ErrorCodes.ValidationFailed);
        currentResult.ToList()[1].Message.Should().Be(informationMessage);
        currentResult.ToList()[1].Code.Should().Be(ErrorCodes.Valid);
        currentResult.EntriesString.Should().Be(
            $"ValidationFailed: {errorMessage}" +
            Environment.NewLine +
            $"Valid: {informationMessage}");

        currentResult.AddResult(previousResult); // remain invalid
        currentResult.IsValid.Should().BeFalse();
        currentResult.Entries.Count.Should().Be(2);
        currentResult.ToList()[0].Message.Should().Be(errorMessage);
        currentResult.ToList()[0].Code.Should().Be(ErrorCodes.ValidationFailed);
        currentResult.ToList()[1].Message.Should().Be(informationMessage);
        currentResult.ToList()[1].Code.Should().Be(ErrorCodes.Valid);
        currentResult.EntriesString.Should().Be(
            $"ValidationFailed: {errorMessage}" +
            Environment.NewLine +
            $"Valid: {informationMessage}");
    }
}

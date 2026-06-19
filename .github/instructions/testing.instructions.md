---
description: "Guidelines for unit testing"
applyTo: "**/*Tests.cs"
---

# Unit Testing Instructions

These are the instructions for unit testing of the `HostManagement` project.

## Framework and Libraries

- Unit tests shall use:
  - `xUnit` as the test framework
  - `AwesomeAssertions` for assertions
  - `NSubstitute` for mocking (if needed)
  - `TestableIO.System.IO.Abstractions` for file system mocking (if needed)

## Unit Test Project Structure

- Unit test projects shall:
  - be located in the `tests/` folder
  - reflect the name of the project being tested → `<ProjectName>.Tests` (e.g. `HostManagement.Tests`)
  - reflect the folder structure of the project being tested
- Unit test files shall:
  - reflect the name of the file containing the class being tested → `<ClassName>Tests.cs` (e.g. `SystemdUnitServiceTests.cs`)

## Unit Test Class Structure

- Unit test classes shall:
  - reflect the name of the class being tested → `<ClassName>Tests` (e.g. `SystemdUnitServiceTests`)
  - use subclasses for methods/properties, if more than one method/property is defined
    - name subclasses for methods like the method being tested (if it is an overridable method, like `ToString`, it shall be appended with **Method**)
    - append subclasses for properties always with **Property** (e.g. `CountProperty`)

## Unit Test Method Structure

- Unit test methods shall:
  - be named in the format `Should<ExpectedBehaviour>_When<Condition>` (e.g. `ShouldReturnError_WhenPackageSignatureInvalid`, `ShouldThrow_WhenFormatExceptionOccurs`)
  - use the AAA (Arrange-Act-Assert) pattern
    - **Arrange**: Setup of data, mocking and dependencies
    - **Act**: Invocation of the method being tested
    - **Assert**: Check whether the expectations were met

    **Examples:**

    ```csharp
    [Theory]
    [InlineData(1, "", "StdErr")]
    [InlineData(42, "", "StdErr")]
    [InlineData(125, "", "StdErr")]
    public void ShouldReturnTrue_WhenExitCodeGreaterThanZero(int exitCode, string standardOutput, string standardError)
    {
        // Arrange
        var scriptResponse = new ScriptResponse(exitCode, standardOutput, standardError);

        // Act
        var result = scriptResponse.HasErrors;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ShouldThrow_WhenPathNull()
    {
        // Arrange
        var sut = new PathValidator();
        string? path = null;
    
        // Act
        var func = () => sut.ValidateExistingDirectory(path!);
    
        // Assert
        func.Should().Throw<ArgumentNullException>();
    }
    ```

  - omit the **Arrange** comment, if no data setup or mocking is needed and test values are given via the method parameters

    **Example:**

    ```csharp
    [Theory]
    [InlineData("192.168.42,21/24")]
    [InlineData("192.168.421.21/24")]
    public void ShouldThrow_WhenIPInvalid(string cidr)
    {
        // Act
        var func = () => IPUtilities.ConvertFromCIDR(cidr);
    
        // Assert
        func.Should()
            .Throw<FormatException>()
            .WithMessage("Could not parse IP address of provided CIDR string.");
    }
    ```

  - omit the **Arrange**/**Act**/**Assert** comments completely for short and simple unit tests

    **Example:**

    ```csharp
    [Fact]
    public void ShouldReturnTempPath_WhenCalled() => AppPaths.TempPath.Should().Be("/tmp");
    ```

  - first check all negative outcomes, e.g. exceptions, guard clauses etc.

## Asserting Log Messages

- When logging is used in the method being tested and the log message should be asserted, make sure that:
  - A substitute for `ILogger` is created in the `Arrange` step
  - `IsEnabled` returns `true`

  **Example:**

  ```csharp
  // Arrange
  var logger = Substitute.For<ILogger>();
  logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
  ```

- If the logger substitute is used in multiple test methods, it should be extracted to a method.

  **Example:**

  ```csharp
  [Fact]
  public async Task ShouldLogAndReturnResult_WhenExceptionOccurs()
  {
      // Arrange
      var logger = GetLoggerSubstitute();
  
      // Additional arrange logic here ...
      // Act & Assert logic here ...
  }
  
  private static ILogger GetLoggerSubstitute()
  {
      var logger = Substitute.For<ILogger>();
      logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
      return logger;
  }
  ```

- For logging assertions, it is generally sufficient not to check the final rendered text (which depends on the formatter/provider). It should rather be checked that:
  - `LogLevel` is correct
  - `Message` is passed as a State/Template argument
  - `Exception` (if used) is passed in the exception parameter

  **Example:**

  ```csharp
  logger.Received(1).Log(
      LogLevel.Error,
      Arg.Any<EventId>(),
      Arg.Is<object>(state => state.ToString()!.Contains(expectedMessage)),
      Arg.Is<Exception?>(ex => ReferenceEquals(ex, expectedException)),
      Arg.Any<Func<object, Exception?, string>>());
  ```

## Best Practices

### Unit Test Coverage

- Unit tests shall test all public methods and properties
- Unit tests shall cover:
  - the "happy path" and edge cases
  - null, empty or whitespace inputs
  - exception handling

### Unit Test Data

- Use meaningful constants instead of magic numbers/strings
- Use `TheoryData<T>` for common test data
- Use realistic data (e.g. real IPv6 addresses, valid paths etc.)

### Async/Await

- Use `async/await` for asynchronous tests

  **Example:**

  ```csharp
  [Theory]
  [InlineData(1)]
  [InlineData(4094)]
  public async Task ShouldReturnValid_WhenVLANIDValid(int vlanID)
  {
      // Act
      var result = await new VLANIDValidator().ValidateAsync(vlanID);
  
      // Assert
      result.IsValid.Should().BeTrue();
      result.Entries.Count.Should().Be(0);
  }
  ```

### Isolation

- Each test shall be independent (no dependency to other tests)
- Do cleanup after test execution, if needed
- Use the `[Collection("Non Parallel Tests")]` class attribute to make tests run sequentially, if needed (e.g. shared resources, mocked file system operations etc.)

  **Example:**

  ```csharp
  [Collection("Non Parallel Tests")]
  public class TestClass
  {
      // test methods here ...
  }
  ```

### Readability

- Use meaningful variable names
- Use comments, if needed (e.g. for complex test logic)

## Documentation

- XML comments are NOT REQUIRED for unit tests

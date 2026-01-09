using AwesomeAssertions;
using Xunit;
using HostManagement.Communication.CommandLine.Output;

namespace HostManagement.Tests.Communication.CommandLine.Output;

[Collection("Non Parallel Tests")]
public class SimpleOutputTests
{
    [Fact]
    public void ShouldWriteLine_WhenCalledWithValue()
    {
        // Arrange
        var input = "Test output";

        var expectedOutput =
            $"""
            {input}{Environment.NewLine}

            """
            .UseEnvironmentNewLine();

        using (var stringWriter = new StringWriter())
        {
            var originalConsoleOut = Console.Out;

            try
            {
                Console.SetOut(stringWriter);

                // Act
                SimpleOutput.WriteLine(input);
            }
            finally
            {
                Console.SetOut(originalConsoleOut);
            }

            // Assert
            stringWriter.ToString().Should().Be(expectedOutput);
        }
    }

    [Fact]
    public void ShouldWriteLine_WhenCalledWithFormatAndArgs()
    {
        // Arrange
        var format = "Hello, {0}{1}";
        var arg1 = "World";
        var arg2 = "!";
        var formattedMessage = string.Format(format, arg1, arg2);

        var expectedOutput =
            $"""
            {formattedMessage}{Environment.NewLine}

            """
            .UseEnvironmentNewLine();

        using (var stringWriter = new StringWriter())
        {
            var originalConsoleOut = Console.Out;

            try
            {
                Console.SetOut(stringWriter);

                // Act
                SimpleOutput.WriteLine(format, arg1, arg2);
            }
            finally
            {
                Console.SetOut(originalConsoleOut);
            }

            // Assert
            stringWriter.ToString().Should().Be(expectedOutput);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    [InlineData(null)]
    public void ShouldWriteLineError_WhenCalledWithValue(bool? useColoredOutput)
    {
        // Arrange
        CliOutputValue errorInput = "Test error";

        var expectedOutput =
            $"""
            ERROR: {errorInput}{Environment.NewLine}

            """
            .UseEnvironmentNewLine();

        using (var stringWriter = new StringWriter())
        {
            var originalConsoleOut = Console.Out;

            try
            {
                Console.SetOut(stringWriter);

                // Act
                if (useColoredOutput is bool useColor)
                    SimpleOutput.WriteLineError(useColor, errorInput);
                else
                    SimpleOutput.WriteLineError(errorInput);
            }
            finally
            {
                Console.SetOut(originalConsoleOut);
            }

            // Assert
            stringWriter.ToString().Should().Be(expectedOutput);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    [InlineData(null)]
    public void ShouldWriteLineError_WhenCalledWithFormatAndArgs(bool? useColoredOutput)
    {
        // Arrange
        var format = "Specified network interface '{0}' was not found.";
        var arg = "lan1";
        var formattedMessage = string.Format(format, arg);

        var expectedOutput =
            $"""
            ERROR: {formattedMessage}{Environment.NewLine}

            """
            .UseEnvironmentNewLine();

        using (var stringWriter = new StringWriter())
        {
            var originalConsoleOut = Console.Out;

            try
            {
                Console.SetOut(stringWriter);

                // Act
                if (useColoredOutput is bool useColor)
                    SimpleOutput.WriteLineError(useColor, format, arg);
                else
                    SimpleOutput.WriteLineError(format, arg);
            }
            finally
            {
                Console.SetOut(originalConsoleOut);
            }

            // Assert
            stringWriter.ToString().Should().Be(expectedOutput);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    [InlineData(null)]
    public void ShouldWriteLineWarning_WhenCalledWithValue(bool? useColoredOutput)
    {
        // Arrange
        CliOutputValue warningInput = "Test warning";

        var expectedOutput =
            $"""
            WARNING: {warningInput}{Environment.NewLine}

            """
            .UseEnvironmentNewLine();

        using (var stringWriter = new StringWriter())
        {
            var originalConsoleOut = Console.Out;

            try
            {
                Console.SetOut(stringWriter);

                // Act
                if (useColoredOutput is bool useColor)
                    SimpleOutput.WriteLineWarning(useColor, warningInput);
                else
                    SimpleOutput.WriteLineWarning(warningInput);
            }
            finally
            {
                Console.SetOut(originalConsoleOut);
            }

            // Assert
            stringWriter.ToString().Should().Be(expectedOutput);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    [InlineData(null)]
    public void ShouldWriteLineWarning_WhenCalledWithFormatAndArgs(bool? useColoredOutput)
    {
        // Arrange
        var format = "Specified gateway '{0}' is already defined.";
        var arg = "192.168.178.1";
        var formattedMessage = string.Format(format, arg);

        var expectedOutput =
            $"""
            WARNING: {formattedMessage}{Environment.NewLine}

            """
            .UseEnvironmentNewLine();

        using (var stringWriter = new StringWriter())
        {
            var originalConsoleOut = Console.Out;

            try
            {
                Console.SetOut(stringWriter);

                // Act
                if (useColoredOutput is bool useColor)
                    SimpleOutput.WriteLineWarning(useColor, format, arg);
                else
                    SimpleOutput.WriteLineWarning(format, arg);
            }
            finally
            {
                Console.SetOut(originalConsoleOut);
            }

            // Assert
            stringWriter.ToString().Should().Be(expectedOutput);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    [InlineData(null)]
    public void ShouldWriteLineInformation_WhenCalledWithValue(bool? useColoredOutput)
    {
        // Arrange
        CliOutputValue infoInput = "Test info";

        var expectedOutput =
            $"""
            INFORMATION: {infoInput}{Environment.NewLine}

            """
            .UseEnvironmentNewLine();

        using (var stringWriter = new StringWriter())
        {
            var originalConsoleOut = Console.Out;

            try
            {
                Console.SetOut(stringWriter);

                // Act
                if (useColoredOutput is bool useColor)
                    SimpleOutput.WriteLineInformation(useColor, infoInput);
                else
                    SimpleOutput.WriteLineInformation(infoInput);
            }
            finally
            {
                Console.SetOut(originalConsoleOut);
            }

            // Assert
            stringWriter.ToString().Should().Be(expectedOutput);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    [InlineData(null)]
    public void ShouldWriteLineInformation_WhenCalledWithFormatAndArgs(bool? useColoredOutput)
    {
        // Arrange
        var format = "This is a {0}.";
        var arg = "information";
        var formattedMessage = string.Format(format, arg);

        var expectedOutput =
            $"""
            INFORMATION: {formattedMessage}{Environment.NewLine}

            """
            .UseEnvironmentNewLine();

        using (var stringWriter = new StringWriter())
        {
            var originalConsoleOut = Console.Out;

            try
            {
                Console.SetOut(stringWriter);

                // Act
                if (useColoredOutput is bool useColor)
                    SimpleOutput.WriteLineInformation(useColor, format, arg);
                else
                    SimpleOutput.WriteLineInformation(format, arg);
            }
            finally
            {
                Console.SetOut(originalConsoleOut);
            }

            // Assert
            stringWriter.ToString().Should().Be(expectedOutput);
        }
    }
}

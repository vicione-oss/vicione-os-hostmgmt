using System.Text;
using AwesomeAssertions;
using Xunit;
using HostManagement.Communication.CommandLine.Output;

namespace HostManagement.Tests.Communication.CommandLine.Output;

public class CliComplexOutputBaseTests
{
    private sealed class SampleOutput : CliComplexOutputBase
    {
        private readonly List<CliOutputValue> _items = [];

        internal SampleOutput(
            int indentationLevel = 0,
            ICollection<CliComplexOutputBase>? cliOutputs = null,
            StringBuilder? stringBuilder = null)
            : base(
                  indentationLevel,
                  cliOutputs,
                  stringBuilder)
        { }

        internal SampleOutput With(CliOutputValue value)
        {
            _items.Add(value);
            return this;
        }

        internal static string GetEnabledTextTestMethod(bool? enabled) => GetEnabledText(enabled);

        protected internal override int ItemCount => 0;

        private protected override void Format()
        {
            foreach (var item in _items)
                AppendLine(item);
        }
    }

    public class Constructor : CliComplexOutputBaseTests
    {
        [Fact]
        public void ShouldCreateDerivedOutput_WhenNoArgumentsGiven()
        {
            // Act
            var result = new SampleOutput();

            // Assert
            result.Should().BeOfType<SampleOutput>();
        }

        [Fact]
        public void ShouldThrow_WhenIndentationLevelNegative()
        {
            // Act
            var result = () => new SampleOutput(-1);

            // Assert
            result.Should()
                .Throw<ArgumentOutOfRangeException>()
                .WithMessage("Indentation can not be negative.*")
                .WithParameterName("indentationLevel");
        }

        [Fact]
        public void ShouldCreateDerivedOutput_WhenIndentationGiven()
        {
            // Act
            var result = new SampleOutput(indentationLevel: 42);

            // Assert
            result.Should().BeOfType<SampleOutput>();
        }

        [Fact]
        public void ShouldCreateDerivedOutput_WhenOutputsGiven()
        {
            // Act
            var result = new SampleOutput(cliOutputs: [new ListOutput(), new KeyValueOutput()]);

            // Assert
            result.Should().BeOfType<SampleOutput>();
        }

        [Fact]
        public void ShouldCreateDerivedOutput_WhenStringBuilderGiven()
        {
            // Act
            var result = new SampleOutput(stringBuilder: new StringBuilder("test"));

            // Assert
            result.Should().BeOfType<SampleOutput>();
        }

        [Fact]
        public void ShouldCreateDerivedOutput_WhenIndentationAndOutputsGiven()
        {
            // Act
            var result = new SampleOutput(
                indentationLevel: 42,
                cliOutputs: [new ListOutput(), new KeyValueOutput()]);

            // Assert
            result.Should().BeOfType<SampleOutput>();
        }

        [Fact]
        public void ShouldCreateDerivedOutput_WhenIndentationAndStringBuilderGiven()
        {
            // Act
            var result = new SampleOutput(
                indentationLevel: 42,
                stringBuilder: new StringBuilder("test"));

            // Assert
            result.Should().BeOfType<SampleOutput>();
        }

        [Fact]
        public void ShouldCreateDerivedOutput_WhenOutputsAndStringBuilderGiven()
        {
            // Act
            var result = new SampleOutput(
                cliOutputs: [new ListOutput(), new KeyValueOutput()],
                stringBuilder: new StringBuilder("test"));

            // Assert
            result.Should().BeOfType<SampleOutput>();
        }

        [Fact]
        public void ShouldCreateDerivedOutput_WhenAllArgumentsGiven()
        {
            // Act
            var result = new SampleOutput(
                indentationLevel: 42,
                cliOutputs: [new ListOutput(), new KeyValueOutput()],
                stringBuilder: new StringBuilder("test"));

            // Assert
            result.Should().BeOfType<SampleOutput>();
        }
    }

    [Collection("Non Parallel Tests")]
    public class Write
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldWriteLine_WhenCalled(bool useColoredOutput)
        {
            // Arrange
            var output = new ListOutput(2)
                .WithInformation("This is an information.")
                .WithWarning("This is a warning.")
                .WithError("This is an error.")
                .WithTitle("Title")
                .With("Single Value");

            var expectedOutput =
                """
                ERROR: This is an error.
                WARNING: This is a warning.
                INFORMATION: This is an information.

                    Title: Single Value

                """
                .UseEnvironmentNewLine();

            using (var stringWriter = new StringWriter())
            {
                var originalConsoleOut = Console.Out;

                try
                {
                    Console.SetOut(stringWriter);

                    // Act
                    output.Write(useColoredOutput);
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

    [Collection("Non Parallel Tests")]
    public class WriteLine
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldWriteLine_WhenCalled(bool useColoredOutput)
        {
            // Arrange
            var output = new ListOutput(2)
                .WithInformation("This is an information.")
                .WithWarning("This is a warning.")
                .WithError("This is an error.")
                .WithTitle("Title")
                .With("Single Value");

            var expectedOutput =
                """
                ERROR: This is an error.
                WARNING: This is a warning.
                INFORMATION: This is an information.

                    Title: Single Value


                """
                .UseEnvironmentNewLine();

            using (var stringWriter = new StringWriter())
            {
                var originalConsoleOut = Console.Out;

                try
                {
                    Console.SetOut(stringWriter);

                    // Act
                    output.WriteLine(useColoredOutput);
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

    public class GetOutput
    {
        [Fact]
        public void ShouldGetOutput_WhenTitleAndSingleItemGiven()
        {
            // Arrange
            var output = new ListOutput(2);
            output.WithTitle("Title");
            output.With("Single Value");

            var expectedOutput =
                """
                    Title: Single Value

                """
                .UseEnvironmentNewLine();

            // Act
            var result = output.GetOutput();

            // Assert
            result.Should().Be(expectedOutput);
        }

        [Fact]
        public void ShouldGetOutput_WhenTitleAndSingleItemAndMessageGiven()
        {
            // Arrange
            var output = new ListOutput(2);
            output.WithTitle("Title");
            output.With("Single Value");
            output.WithWarning("Warning");

            var expectedOutput =
                """
                WARNING: Warning

                    Title: Single Value

                """
                .UseEnvironmentNewLine();

            // Act
            var result = output.GetOutput();

            // Assert
            result.Should().Be(expectedOutput);
        }

        [Fact]
        public void ShouldGetOutput_WhenTitleAndMultipleItemsGiven()
        {
            // Arrange
            var output = new ListOutput(2);
            output.WithTitle("Title");
            output.With("First Value");
            output.With("Second Value");

            var expectedOutput =
                """
                    Title:
                      First Value
                      Second Value

                """
                .UseEnvironmentNewLine();

            // Act
            var result = output.GetOutput();

            // Assert
            result.Should().Be(expectedOutput);
        }

        [Fact]
        public void ShouldGetOutput_WhenTitleAndMultipleItemsAndMessageGiven()
        {
            // Arrange
            var output = new ListOutput(2);
            output.WithTitle("Title");
            output.With("First Value");
            output.With("Second Value");
            output.WithWarning("Warning");

            var expectedOutput =
                """
                WARNING: Warning

                    Title:
                      First Value
                      Second Value

                """
                .UseEnvironmentNewLine();

            // Act
            var result = output.GetOutput();

            // Assert
            result.Should().Be(expectedOutput);
        }

        [Fact]
        public void ShouldGetOutput_WhenSingleItemWithoutTitleGiven()
        {
            // Arrange
            var output = new ListOutput(2);
            output.With("Single Value");

            var expectedOutput =
                """
                    Single Value

                """
                .UseEnvironmentNewLine();

            // Act
            var result = output.GetOutput();

            // Assert
            result.Should().Be(expectedOutput);
        }

        [Fact]
        public void ShouldGetOutput_WhenMultipleItemsWithoutTitleGiven()
        {
            // Arrange
            var output = new ListOutput(2);
            output.With("First Value");
            output.With("Second Value");

            var expectedOutput =
                """
                    First Value
                    Second Value

                """
                .UseEnvironmentNewLine();

            // Act
            var result = output.GetOutput();

            // Assert
            result.Should().Be(expectedOutput);
        }

        [Fact]
        public void ShouldGetOutput_WhenSingleItemOnNewLineConfigured()
        {
            // Arrange
            var output = new SampleOutput()
                .WithTitle("Title")
                .With("SingleItem");

            var expectedOutput =
                """
                Title:
                  SingleItem

                """
                .UseEnvironmentNewLine();

            // Act
            var result = output.WithSingleItemOnNewLine();

            // Assert
            result.GetOutput().Should().Be(expectedOutput);
        }
    }

    public class AddOutputMethods
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldAddKeyValueOutput_WhenCalled(bool addSingleItemToNewLine)
        {
            // Arrange
            var title = "KeyValueOutput";
            var output = new SampleOutput();

            if (addSingleItemToNewLine)
                output.WithSingleItemOnNewLine();

            // Act
            var resultBeforeAdd = output.GetOutput();
            output.AddKeyValueOutput().WithTitle(title);
            var resultAfterAdd = output.GetOutput();

            // Assert
            resultBeforeAdd.Should().BeEmpty();
            resultAfterAdd.Should().Contain(title);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldAddListOutput_WhenCalled(bool addSingleItemToNewLine)
        {
            // Arrange
            var title = "ListOutput";
            var output = new SampleOutput();

            if (addSingleItemToNewLine)
                output.WithSingleItemOnNewLine();

            // Act
            var resultBeforeAdd = output.GetOutput();
            output.AddListOutput().WithTitle(title);
            var resultAfterAdd = output.GetOutput();

            // Assert
            resultBeforeAdd.Should().BeEmpty();
            resultAfterAdd.Should().Contain(title);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldAddMultiKeyValueOutput_WhenCalled(bool addSingleItemToNewLine)
        {
            // Arrange
            var title = "MultiKeyValueOutput";
            var output = new SampleOutput();

            if (addSingleItemToNewLine)
                output.WithSingleItemOnNewLine();

            // Act
            var resultBeforeAdd = output.GetOutput();
            output.AddMultiKeyValueOutput().WithTitle(title);
            var resultAfterAdd = output.GetOutput();

            // Assert
            resultBeforeAdd.Should().BeEmpty();
            resultAfterAdd.Should().Contain(title);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldAddMultiPropertyListOutput_WhenCalled(bool addSingleItemToNewLine)
        {
            // Arrange
            var title = "MultiPropertyListOutput";
            var output = new SampleOutput();

            if (addSingleItemToNewLine)
                output.WithSingleItemOnNewLine();

            // Act
            var resultBeforeAdd = output.GetOutput();
            output.AddMultiPropertyListOutput().WithTitle(title);
            var resultAfterAdd = output.GetOutput();

            // Assert
            resultBeforeAdd.Should().BeEmpty();
            resultAfterAdd.Should().Contain(title);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldAddNewLineOutput_WhenCalled(bool addSingleItemToNewLine)
        {
            // Arrange
            var title = "NewLineOutput";
            var output = new SampleOutput();

            if (addSingleItemToNewLine)
                output.WithSingleItemOnNewLine();

            // Act
            var resultBeforeAdd = output.GetOutput();
            output.AddNewLine().WithTitle(title);
            var resultAfterAdd = output.GetOutput();

            // Assert
            resultBeforeAdd.Should().BeEmpty();
            resultAfterAdd.Should().Contain(title);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldAddPropertyListOutput_WhenCalled(bool addSingleItemToNewLine)
        {
            // Arrange
            var title = "PropertyListOutput";
            var output = new SampleOutput();

            if (addSingleItemToNewLine)
                output.WithSingleItemOnNewLine();

            // Act
            var resultBeforeAdd = output.GetOutput();
            output.AddPropertyListOutput().WithTitle(title);
            var resultAfterAdd = output.GetOutput();

            // Assert
            resultBeforeAdd.Should().BeEmpty();
            resultAfterAdd.Should().Contain(title);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldAddTableOutput_WhenCalled(bool addSingleItemToNewLine)
        {
            // Arrange
            var title = "TableOutput";
            var output = new SampleOutput();

            if (addSingleItemToNewLine)
                output.WithSingleItemOnNewLine();

            // Act
            var resultBeforeAdd = output.GetOutput();
            output.AddTableOutput().WithTitle(title);
            var resultAfterAdd = output.GetOutput();

            // Assert
            resultBeforeAdd.Should().BeEmpty();
            resultAfterAdd.Should().Contain(title);
        }
    }

    public class AddTitle
    {
        [Theory]
        [InlineData(true, true)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(false, false)]
        public void ShouldAddTitleWithEnabled_WhenCalled(bool enabled, bool useTitleDelimiter)
        {
            // Arrange
            var output = new SampleOutput();
            var title = "Title";
            var enabledString = enabled ? "enabled" : "disabled";
            var titleDelimiter = useTitleDelimiter ? ":" : string.Empty;

            var expectedTitle = $"{title} ({enabledString}){titleDelimiter}";

            // Act
            output.WithTitle(title, enabled, useTitleDelimiter);

            // Assert
            output.GetOutput().Should().Contain(expectedTitle);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldAddTitleWithoutEnabled_WhenCalled(bool useTitleDelimiter)
        {
            // Arrange
            var output = new SampleOutput();
            var title = "Title";
            var titleDelimiter = useTitleDelimiter ? ":" : string.Empty;

            var expectedTitle = $"{title}{titleDelimiter}";

            // Act
            output.WithTitle(title, useTitleDelimiter: useTitleDelimiter);

            // Assert
            output.GetOutput().Should().Contain(expectedTitle);
        }
    }

    public class AddErrorMessage
    {
        [Fact]
        public void ShouldAddErrorMessage_WhenCalled()
        {
            // Arrange
            var output = new SampleOutput();

            var expectedErrorMessage = "ERROR: This is an error message.";

            // Act
            output.WithError(expectedErrorMessage);

            // Assert
            output.GetOutput().Should().Contain(expectedErrorMessage);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void ShouldAddErrorMessageEmpty_WhenCalledWithEmptyStringFormat(string? format)
        {
            // Arrange
            var output = new SampleOutput();
            CliOutputValue[]? args = ["argument"];

            var expectedErrorMessage =
                "ERROR: An error was supposed to be displayed, " +
                "but the original error message was empty.";

            // Act
            output.WithError(format!, args);

            // Assert
            output.GetOutput().Should().Contain(expectedErrorMessage);
        }

        [Fact]
        public void ShouldAddErrorMessage_WhenCalledWithStringFormatAndArgumentsNull()
        {
            // Arrange
            var output = new SampleOutput();
            var format = "ERROR: This is an {0} message.";
            CliOutputValue[]? args = null;

            var expectedErrorMessage = "ERROR: This is an {0} message.";

            // Act
            output.WithError(format, args);

            // Assert
            output.GetOutput().Should().Contain(expectedErrorMessage);
        }

        [Fact]
        public void ShouldAddErrorMessageInvalidFormat_WhenCalledWithInvalidFormat()
        {
            // Arrange
            var output = new SampleOutput();
            var format = "ERROR: This is an {0} {1}.";
            var arg1 = "error";

            var expectedErrorMessage =
                "ERROR: An error was supposed to be displayed, " +
                "but the original error message had an invalid format.";

            // Act
            output.WithError(format, arg1);

            // Assert
            output.GetOutput().Should().Contain(expectedErrorMessage);
        }

        [Fact]
        public void ShouldAddErrorMessage_WhenCalledWithStringFormatAndArguments()
        {
            // Arrange
            var output = new SampleOutput();
            var format = "ERROR: This is an {0} {1}.";
            var arg1 = "error";
            var arg2 = "message";

            var expectedErrorMessage = "ERROR: This is an error message.";

            // Act
            output.WithError(format, arg1, arg2);

            // Assert
            output.GetOutput().Should().Contain(expectedErrorMessage);
        }
    }

    public class AddInformationMessage
    {
        [Fact]
        public void ShouldAddInformationMessage_WhenCalled()
        {
            // Arrange
            var output = new SampleOutput();

            var expectedInformationMessage = "INFORMATION: This is an information message.";

            // Act
            output.WithInformation(expectedInformationMessage);

            // Assert
            output.GetOutput().Should().Contain(expectedInformationMessage);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void ShouldAddInformationMessageEmpty_WhenCalledWithEmptyStringFormat(string? format)
        {
            // Arrange
            var output = new SampleOutput();
            CliOutputValue[]? args = ["argument"];

            var expectedInformationMessage =
                "INFORMATION: An information was supposed to be displayed, " +
                "but the original information message was empty.";

            // Act
            output.WithInformation(format!, args);

            // Assert
            output.GetOutput().Should().Contain(expectedInformationMessage);
        }

        [Fact]
        public void ShouldAddInformationMessage_WhenCalledWithStringFormatAndArgumentsNull()
        {
            // Arrange
            var output = new SampleOutput();
            var format = "INFORMATION: This is an {0} message.";
            CliOutputValue[]? args = null;

            var expectedInformationMessage = "INFORMATION: This is an {0} message.";

            // Act
            output.WithInformation(format, args);

            // Assert
            output.GetOutput().Should().Contain(expectedInformationMessage);
        }

        [Fact]
        public void ShouldAddInformationMessageInvalidFormat_WhenCalledWithInvalidFormat()
        {
            // Arrange
            var output = new SampleOutput();
            var format = "INFORMATION: This is an {0} {1}.";
            var arg1 = "information";

            var expectedInformationMessage =
                "INFORMATION: An information was supposed to be displayed, " +
                "but the original information message had an invalid format.";

            // Act
            output.WithInformation(format, arg1);

            // Assert
            output.GetOutput().Should().Contain(expectedInformationMessage);
        }

        [Fact]
        public void ShouldAddInformationMessage_WhenCalledWithStringFormatAndArguments()
        {
            // Arrange
            var output = new SampleOutput();
            var format = "INFORMATION: This is an {0} {1}.";
            var arg1 = "information";
            var arg2 = "message";

            var expectedInformationMessage = "INFORMATION: This is an information message.";

            // Act
            output.WithInformation(format, arg1, arg2);

            // Assert
            output.GetOutput().Should().Contain(expectedInformationMessage);
        }
    }

    public class AddWarningMessage
    {
        [Fact]
        public void ShouldAddWarningMessage_WhenCalled()
        {
            // Arrange
            var output = new SampleOutput();

            var expectedWarningMessage = "WARNING: This is a warning message.";

            // Act
            output.WithWarning(expectedWarningMessage);

            // Assert
            output.GetOutput().Should().Contain(expectedWarningMessage);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void ShouldAddWarningMessageEmpty_WhenCalledWithEmptyStringFormat(string? format)
        {
            // Arrange
            var output = new SampleOutput();
            CliOutputValue[]? args = ["argument"];

            var expectedErrorMessage =
                "WARNING: A warning was supposed to be displayed, " +
                "but the original warning message was empty.";

            // Act
            output.WithWarning(format!, args);

            // Assert
            output.GetOutput().Should().Contain(expectedErrorMessage);
        }

        [Fact]
        public void ShouldAddWarningMessage_WhenCalledWithStringFormatAndArgumentsNull()
        {
            // Arrange
            var output = new SampleOutput();
            var format = "WARNING: This is a {0} message.";
            CliOutputValue[]? args = null;

            var expectedWarningMessage = "WARNING: This is a {0} message.";

            // Act
            output.WithWarning(format, args);

            // Assert
            output.GetOutput().Should().Contain(expectedWarningMessage);
        }

        [Fact]
        public void ShouldAddWarningMessageInvalidFormat_WhenCalledWithInvalidFormat()
        {
            // Arrange
            var output = new SampleOutput();
            var format = "WARNING: This is a {0} {1}.";
            var arg1 = "information";

            var expectedWarningMessage =
                "WARNING: A warning was supposed to be displayed, " +
                "but the original warning message had an invalid format.";

            // Act
            output.WithWarning(format, arg1);

            // Assert
            output.GetOutput().Should().Contain(expectedWarningMessage);
        }

        [Fact]
        public void ShouldAddWarningMessage_WhenCalledWithStringFormatAndArguments()
        {
            // Arrange
            var output = new SampleOutput();
            var format = "WARNING: This is a {0} {1}.";
            var arg1 = "warning";
            var arg2 = "message";

            var expectedWarningMessage = "WARNING: This is a warning message.";

            // Act
            output.WithWarning(format, arg1, arg2);

            // Assert
            output.GetOutput().Should().Contain(expectedWarningMessage);
        }
    }

    public class GetEnabledText
    {
        [Theory]
        [InlineData(true, " (enabled)")]
        [InlineData(false, " (disabled)")]
        [InlineData(null, "")]
        public void ShouldGetEnabledText_WhenCalled(bool? enabled, string expectedText)
        {
            // Act
            var result = SampleOutput.GetEnabledTextTestMethod(enabled);

            // Assert
            result.Should().Be(expectedText);
        }
    }
}

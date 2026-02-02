using System.IO.Abstractions;
using System.Text;
using AwesomeAssertions;
using HostManagement.Common.Wrappers;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace HostManagement.Common.Tests.Wrappers;

public class FileSystemWrapperTests
{
    public class Constructor
    {
        [Fact]
        public void ShouldThrow_WhenIFileSystemIsNull()
        {
            // Arrange
            var testLogger = new TestLogger(LogLevel.Information);

            // Act
            var result = () => new FileSystemWrapper(null!, testLogger);

            // Assert
            result.Should()
                .Throw<ArgumentNullException>()
                .WithParameterName("fileSystem");
        }

        [Fact]
        public void ShouldThrow_WhenILoggerIsNull()
        {
            // Arrange
            var fileSystemSubstitute = Substitute.For<IFileSystem>();

            // Act
            var result = () => new FileSystemWrapper(fileSystemSubstitute, null!);

            // Assert
            result.Should()
                .Throw<ArgumentNullException>()
                .WithParameterName("logger");
        }
    }

    public class DeleteFile
    {
        private static readonly string s_testFile = Path.Combine("/tmp", "test.txt");
        private static readonly TestLogger s_testLogger = new(LogLevel.Information);

        [Fact]
        public void ShouldReturn_WhenFileNotExists()
        {
            // Arrange
            var fileSystemSubstitute = Substitute.For<IFileSystem>();
            fileSystemSubstitute.File.Exists(s_testFile).Returns(false);

            // Act
            new FileSystemWrapper(fileSystemSubstitute, s_testLogger).DeleteFile(s_testFile);

            // Assert
            fileSystemSubstitute.File.DidNotReceive().Delete(s_testFile);
        }

        [Fact]
        public void ShouldDeleteFile_WhenFileExists()
        {
            // Arrange
            var expectedMessage = $"Deleted file '{s_testFile}' successfully.";

            var fileSystemSubstitute = Substitute.For<IFileSystem>();
            fileSystemSubstitute.File.Exists(s_testFile).Returns(true);

            // Act
            new FileSystemWrapper(fileSystemSubstitute, s_testLogger).DeleteFile(s_testFile);

            // Assert
            fileSystemSubstitute.File.Received(1).Delete(s_testFile);
            s_testLogger.Logs.Should().Contain(expectedMessage);
        }

        [Fact]
        public void ShouldThrow_WhenDeleteFileThrows()
        {
            // Arrange
            var expectedMessage = $"Could not delete file '{s_testFile}'.*";

            var fileSystemSubstitute = Substitute.For<IFileSystem>();

            fileSystemSubstitute.File
                .Exists(s_testFile)
                .Returns(true);

            fileSystemSubstitute.File
                .When(file => file.Delete(s_testFile))
                .Throw(new IOException());

            // Act
            var result = () => new FileSystemWrapper(fileSystemSubstitute, s_testLogger)
                .DeleteFile(s_testFile);

            // Assert
            result.Should().Throw<IOException>();
            s_testLogger.Logs.Should().ContainMatch(expectedMessage);
        }
    }

    public class ExistsFile
    {
        private static readonly string s_testFile = Path.Combine("/tmp", "test.txt");
        private static readonly TestLogger s_testLogger = new(LogLevel.Information);

        [Fact]
        public void ShouldReturnFalse_WhenFileNotExists()
        {
            // Arrange
            var fileSystemSubstitute = Substitute.For<IFileSystem>();

            fileSystemSubstitute.File.Exists(s_testFile).Returns(false);

            // Act
            var result = new FileSystemWrapper(fileSystemSubstitute, s_testLogger).ExistsFile(s_testFile);

            // Assert
            fileSystemSubstitute.File.Received(1).Exists(s_testFile);
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnTrue_WhenFileExists()
        {
            // Arrange
            var fileSystemSubstitute = Substitute.For<IFileSystem>();

            fileSystemSubstitute.File.Exists(s_testFile).Returns(true);

            // Act
            var result = new FileSystemWrapper(fileSystemSubstitute, s_testLogger).ExistsFile(s_testFile);

            // Assert
            fileSystemSubstitute.File.Received(1).Exists(s_testFile);
            result.Should().BeTrue();
        }
    }

    public class WriteAllTextToFile
    {
        private static readonly string s_testFile = Path.Combine("/tmp", "test.txt");
        private const string TestFileContent = "test";

        [Fact]
        public void ShouldThrow_WhenWriteAllTextThrows()
        {
            // Arrange
            var testLogger = new TestLogger(LogLevel.Information);
            var expectedMessage = $"Could not write to file '{s_testFile}'.*";

            var fileSystemSubstitute = Substitute.For<IFileSystem>();

            fileSystemSubstitute.File
                .When(file => file.WriteAllTextAsync(s_testFile, TestFileContent))
                .Throw(new DirectoryNotFoundException());

            // Act
            var result = () => new FileSystemWrapper(fileSystemSubstitute, testLogger)
                .WriteAllTextToFile(s_testFile, TestFileContent);

            // Assert
            result.Should().ThrowAsync<DirectoryNotFoundException>();
            testLogger.Logs.Should().ContainMatch(expectedMessage);
        }

        [Fact]
        public async Task ShouldWriteAllText_WhenWriteAllTextToFileCalled()
        {
            // Arrange
            var testLogger = new TestLogger(LogLevel.Information);
            var fileSystemSubstitute = Substitute.For<IFileSystem>();

            var memoryStream = new MemoryStream();

            fileSystemSubstitute.File
                .CreateText(s_testFile)
                .Returns(_ => new StreamWriter(memoryStream, Encoding.UTF8, 512, true));

            // Act
            await new FileSystemWrapper(fileSystemSubstitute, testLogger).WriteAllTextToFile(s_testFile, TestFileContent);

            // Assert
            memoryStream.Position = 0;
            using var reader = new StreamReader(memoryStream);
            var writtenData = await reader.ReadToEndAsync();

            TestFileContent.Should().Be(writtenData);
        }
    }
}

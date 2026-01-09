using System.IO.Abstractions;
using System.Text;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using HostManagement.Common.Wrappers;

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

    public class CreateDirectory
    {
        private static readonly string s_testDirectory = Path.Combine("/tmp", "test");

        [Fact]
        public void ShouldThrow_WhenCreateDirectoryThrows()
        {
            // Arrange
            var testLogger = new TestLogger(LogLevel.Information);
            var expectedMessage = $"Could not create directory '{s_testDirectory}'.*";

            var fileSystemSubstitute = Substitute.For<IFileSystem>();

            fileSystemSubstitute.Directory
                .When(directory => directory.CreateDirectory(s_testDirectory))
                .Throw(new IOException());

            // Act
            var result = () => new FileSystemWrapper(fileSystemSubstitute, testLogger)
                .CreateDirectory(s_testDirectory);

            // Assert
            result.Should().Throw<IOException>();
            testLogger.Logs.Should().ContainMatch(expectedMessage);
        }

        [Fact]
        public void ShouldReturnDirectoryInfo_WhenDirectoryExists()
        {
            // Arrange
            var testLogger = new TestLogger(LogLevel.Information);

            var directoryInfoSubstitute = Substitute.For<IDirectoryInfo>();
            var fileSystemSubstitute = Substitute.For<IFileSystem>();

            fileSystemSubstitute.Directory.Exists(s_testDirectory).Returns(true);
            fileSystemSubstitute.DirectoryInfo.New(s_testDirectory).Returns(directoryInfoSubstitute);

            // Act
            var result = new FileSystemWrapper(fileSystemSubstitute, testLogger).CreateDirectory(s_testDirectory);

            // Assert
            result.Should().BeSameAs(directoryInfoSubstitute);
            fileSystemSubstitute.DirectoryInfo.Received(1).New(s_testDirectory);
        }

        [Fact]
        public void ShouldCreateDirectory_WhenDirectoryNotExists()
        {
            // Arrange
            var testLogger = new TestLogger(LogLevel.Information);
            var expectedMessage = $"Created directory '{s_testDirectory}' successfully.";

            var directoryInfoSubstitute = Substitute.For<IDirectoryInfo>();
            var fileSystemSubstitute = Substitute.For<IFileSystem>();

            fileSystemSubstitute.Directory
                .CreateDirectory(s_testDirectory)
                .Returns(directoryInfoSubstitute);

            // Act
            var result = new FileSystemWrapper(fileSystemSubstitute, testLogger).CreateDirectory(s_testDirectory);

            // Assert
            result.Should().BeSameAs(directoryInfoSubstitute);
            fileSystemSubstitute.Directory.Received(1).CreateDirectory(s_testDirectory);
            testLogger.Logs.Should().Contain(expectedMessage);
        }
    }

    public class DeleteDirectory
    {
        private static readonly string s_testDirectory = Path.Combine("/tmp", "test");
        private static readonly TestLogger s_testLogger = new(LogLevel.Information);

        [Fact]
        public void ShouldReturn_WhenDirectoryNotExists()
        {
            // Arrange
            var fileSystemSubstitute = Substitute.For<IFileSystem>();

            fileSystemSubstitute.Directory.Exists(s_testDirectory).Returns(false);

            // Act
            new FileSystemWrapper(fileSystemSubstitute, s_testLogger).DeleteDirectory(s_testDirectory);

            // Assert
            fileSystemSubstitute.Directory.DidNotReceive().Delete(s_testDirectory, true);
        }

        [Fact]
        public void ShouldDeleteDirectory_WhenDirectoryExists()
        {
            // Arrange
            var expectedMessage = $"Deleted directory '{s_testDirectory}' successfully.";

            var fileSystemSubstitute = Substitute.For<IFileSystem>();
            fileSystemSubstitute.Directory.Exists(s_testDirectory).Returns(true);

            // Act
            new FileSystemWrapper(fileSystemSubstitute, s_testLogger).DeleteDirectory(s_testDirectory);

            // Assert
            fileSystemSubstitute.Directory.Received(1).Delete(s_testDirectory, true);
            s_testLogger.Logs.Should().Contain(expectedMessage);
        }

        [Fact]
        public void ShouldThrow_WhenDeleteDirectoryThrows()
        {
            // Arrange
            var expectedMessage = $"Could not delete directory '{s_testDirectory}'.*";

            var fileSystemSubstitute = Substitute.For<IFileSystem>();

            fileSystemSubstitute.Directory
                .Exists(s_testDirectory)
                .Returns(true);

            fileSystemSubstitute.Directory
                .When(directory => directory.Delete(s_testDirectory, true))
                .Throw(new IOException());

            // Act
            var result = () => new FileSystemWrapper(fileSystemSubstitute, s_testLogger)
                .DeleteDirectory(s_testDirectory);

            // Assert
            result.Should().Throw<IOException>();
            s_testLogger.Logs.Should().ContainMatch(expectedMessage);
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

    public class ExistsDirectory
    {
        private static readonly string s_testDirectory = Path.Combine("/tmp", "test");
        private static readonly TestLogger s_testLogger = new(LogLevel.Information);

        [Fact]
        public void ShouldReturnFalse_WhenDirectoryNotExists()
        {
            // Arrange
            var fileSystemSubstitute = Substitute.For<IFileSystem>();

            fileSystemSubstitute.Directory.Exists(s_testDirectory).Returns(false);

            // Act
            var result = new FileSystemWrapper(fileSystemSubstitute, s_testLogger).ExistsDirectory(s_testDirectory);

            // Assert
            fileSystemSubstitute.Directory.Received(1).Exists(s_testDirectory);
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnTrue_WhenDirectoryExists()
        {
            // Arrange
            var fileSystemSubstitute = Substitute.For<IFileSystem>();

            fileSystemSubstitute.Directory.Exists(s_testDirectory).Returns(true);

            // Act
            var result = new FileSystemWrapper(fileSystemSubstitute, s_testLogger).ExistsDirectory(s_testDirectory);

            // Assert
            fileSystemSubstitute.Directory.Received(1).Exists(s_testDirectory);
            result.Should().BeTrue();
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

    public class GetDirectoryName
    {
        private const string TestFileDirectoryPath = "/tmp";
        private static readonly string s_testFile = Path.Combine("/tmp", "test.txt");

        [Fact]
        public void ShouldThrow_WhenGetDirectoryNameThrows()
        {
            // Arrange
            var testLogger = new TestLogger(LogLevel.Information);
            var expectedMessage = $"Could not get directory name for '{s_testFile}'.*";

            var fileSystemSubstitute = Substitute.For<IFileSystem>();

            fileSystemSubstitute.Path
                .When(path => path.GetDirectoryName(s_testFile))
                .Throw(new IOException());

            // Act
            var result = () => new FileSystemWrapper(fileSystemSubstitute, testLogger).GetDirectoryName(s_testFile);

            // Assert
            result.Should().Throw<IOException>();
            testLogger.Logs.Should().ContainMatch(expectedMessage);
        }

        [Fact]
        public void ShouldGetDirectoryName_WhenGetDirectoryNameCalled()
        {
            // Arrange
            var testLogger = new TestLogger(LogLevel.Information);
            var fileSystemSubstitute = Substitute.For<IFileSystem>();

            fileSystemSubstitute.Path.GetDirectoryName(s_testFile).Returns(TestFileDirectoryPath);

            // Act
            var result = new FileSystemWrapper(fileSystemSubstitute, testLogger).GetDirectoryName(s_testFile);

            // Assert
            result.Should().BeEquivalentTo(TestFileDirectoryPath);
        }
    }

    public class ReadAllBytesFromFile
    {
        private const string TestFileContent = "test";
        private static readonly string s_testFile = Path.Combine("/tmp", "test.txt");
        private static readonly byte[] s_testFileContentBytes = Encoding.UTF8.GetBytes(TestFileContent);

        [Fact]
        public void ShouldThrow_WhenReadAllBytesThrows()
        {
            // Arrange
            var testLogger = new TestLogger(LogLevel.Information);
            var expectedMessage = $"Could not read from file '{s_testFile}'.*";

            var fileSystemSubstitute = Substitute.For<IFileSystem>();

            fileSystemSubstitute.File
                .When(file => file.ReadAllBytes(s_testFile))
                .Throw(new DirectoryNotFoundException());

            // Act
            var result = () => new FileSystemWrapper(fileSystemSubstitute, testLogger).ReadAllBytesFromFile(s_testFile);

            // Assert
            result.Should().Throw<DirectoryNotFoundException>();
            testLogger.Logs.Should().ContainMatch(expectedMessage);
        }

        [Fact]
        public void ShouldReadAllBytes_WhenReadAllBytesFromFileCalled()
        {
            // Arrange
            var testLogger = new TestLogger(LogLevel.Information);
            var fileSystemSubstitute = Substitute.For<IFileSystem>();

            fileSystemSubstitute.File.ReadAllBytes(s_testFile).Returns(s_testFileContentBytes);

            // Act
            var result = new FileSystemWrapper(fileSystemSubstitute, testLogger).ReadAllBytesFromFile(s_testFile);

            // Assert
            result.Should().BeEquivalentTo(s_testFileContentBytes);
        }
    }

    public class ReadAllTextFromFile
    {
        private const string TestFileContent = "test";
        private static readonly string s_testFile = Path.Combine("/tmp", "test.txt");

        [Fact]
        public void ShouldThrow_WhenReadAllTextThrows()
        {
            // Arrange
            var testLogger = new TestLogger(LogLevel.Information);
            var expectedMessage = $"Could not read from file '{s_testFile}'.*";

            var fileSystemSubstitute = Substitute.For<IFileSystem>();

            fileSystemSubstitute.File
                .When(file => file.ReadAllTextAsync(s_testFile))
                .Throw(new DirectoryNotFoundException());

            // Act
            var result = () => new FileSystemWrapper(fileSystemSubstitute, testLogger).ReadAllTextFromFile(s_testFile);

            // Assert
            result.Should().ThrowAsync<DirectoryNotFoundException>();
            testLogger.Logs.Should().ContainMatch(expectedMessage);
        }

        [Fact]
        public async Task ShouldReadAllText_WhenReadAllTextFromFileCalled()
        {
            // Arrange
            var testLogger = new TestLogger(LogLevel.Information);
            var fileSystemSubstitute = Substitute.For<IFileSystem>();

            fileSystemSubstitute.File.ReadAllTextAsync(s_testFile).Returns(TestFileContent);

            // Act
            var result = await new FileSystemWrapper(fileSystemSubstitute, testLogger).ReadAllTextFromFile(s_testFile);

            // Assert
            result.Should().BeEquivalentTo(TestFileContent);
        }
    }

    public class WriteToFile
    {
        private static readonly string s_testFile = Path.Combine("/tmp", "test.txt");
        private const string TestFileContent = "test";

        [Fact]
        public void ShouldThrow_WhenFileCreateTextThrows()
        {
            // Arrange
            var testLogger = new TestLogger(LogLevel.Information);
            var expectedMessage = $"Could not write to file '{s_testFile}'.*";

            var fileSystemSubstitute = Substitute.For<IFileSystem>();

            fileSystemSubstitute.File
                .When(file => file.CreateText(s_testFile))
                .Throw(new DirectoryNotFoundException());

            // Act
            var result = () => new FileSystemWrapper(fileSystemSubstitute, testLogger)
                .WriteToFile(s_testFile, TestFileContent);

            // Assert
            result.Should().Throw<DirectoryNotFoundException>();
            testLogger.Logs.Should().ContainMatch(expectedMessage);
        }

        [Fact]
        public void ShouldWriteToFile_WhenFileCreateTextCalled()
        {
            // Arrange
            var testLogger = new TestLogger(LogLevel.Information);
            var expectedMessage = $"Created file '{s_testFile}' successfully.";

            var fileSystemSubstitute = Substitute.For<IFileSystem>();
            var memoryStream = new MemoryStream();

            fileSystemSubstitute.File
                .CreateText(s_testFile)
                .Returns(_ => new StreamWriter(memoryStream, Encoding.UTF8, 512, true));

            // Act
            new FileSystemWrapper(fileSystemSubstitute, testLogger).WriteToFile(s_testFile, TestFileContent);

            // Assert
            memoryStream.Position = 0;
            using var reader = new StreamReader(memoryStream);
            var writtenData = reader.ReadToEnd();

            TestFileContent.Should().Be(writtenData);
            testLogger.Logs.Should().Contain(expectedMessage);
        }

        [Fact]
        public void ShouldOverwriteFile_WhenFileCreateTextCalled()
        {
            // Arrange
            var testLogger = new TestLogger(LogLevel.Information);
            var expectedMessage = $"Overwritten file '{s_testFile}' successfully.";

            var fileSystemSubstitute = Substitute.For<IFileSystem>();
            var memoryStream = new MemoryStream();

            fileSystemSubstitute.File.Exists(s_testFile).Returns(true);
            fileSystemSubstitute.File
                .CreateText(s_testFile)
                .Returns(_ => new StreamWriter(memoryStream, Encoding.UTF8, 512, true));

            // Act
            new FileSystemWrapper(fileSystemSubstitute, testLogger).WriteToFile(s_testFile, TestFileContent);

            // Assert
            memoryStream.Position = 0;
            using var reader = new StreamReader(memoryStream);
            var writtenData = reader.ReadToEnd();

            TestFileContent.Should().Be(writtenData);
            testLogger.Logs.Should().Contain(expectedMessage);
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

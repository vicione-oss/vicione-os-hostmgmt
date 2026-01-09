using System.IO.Abstractions;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using HostManagement.Common.Wrappers;

namespace HostManagement.Scripts.Tests;

public class ResourceManagerTests
{
    public class GetResource
    {
        [Fact]
        public void ShouldGetResourceContent_WhenGetResourceCalled()
        {
            // Arrange
            var testResourceDeployPath = Path.Combine("/tmp", "HostManagement_Resources");

            var testDirectory = Path.Combine(testResourceDeployPath, "Scripts");
            var testResourceName = "HostManagement.Scripts.Resources.Scripts.test.sh";
            var testResourceContent = "test";
            var testResourceContentBytes = Encoding.UTF8.GetBytes(testResourceContent);

            var fileSystemSubstitute = Substitute.For<IFileSystem>();
            var directoryInfoSubstitute = Substitute.For<IDirectoryInfo>();
            var resourceHelperSubstitute = Substitute.For<IResourceHelper>();
            var processWrapperSubstitute = Substitute.For<IProcessWrapper>();

            // No resources are be deployed yet
            fileSystemSubstitute.Directory.Exists(testResourceDeployPath).Returns(false);

            // Get the embedded resource names (namespace syntax)
            resourceHelperSubstitute.GetManifestResourceNames().Returns([testResourceName]);

            // No directory structure shall exist...
            fileSystemSubstitute.Directory.Exists(testDirectory).Returns(false);

            // ...so CreateDirectory shall return a new IDirectoryInfo for each created directory
            directoryInfoSubstitute.Exists.Returns(true);
            directoryInfoSubstitute.FullName.Returns(testDirectory);
            directoryInfoSubstitute.Name.Returns("Scripts");

            fileSystemSubstitute.Directory.CreateDirectory(testDirectory).Returns(directoryInfoSubstitute);

            // Now get the resource stream of the resource...
            var resourceStream = new MemoryStream(testResourceContentBytes);
            resourceHelperSubstitute.GetResourceStream(testResourceName).Returns(resourceStream);

            // ...and deploy it
            var expandableMemoryStream = new MemoryStream();
            resourceStream.CopyTo(expandableMemoryStream);

            fileSystemSubstitute.File
                .CreateText(Path.Combine(testDirectory, "test.sh"))
                .Returns(_ => new StreamWriter(expandableMemoryStream, Encoding.UTF8, 16, true));

            // Return "OK" when setting execution permissions
            processWrapperSubstitute
                .Start(Arg.Any<string>(), Arg.Any<string>())
                .Returns(new ScriptResponse(0, "", ""));

            // ResourceManager can now be initialized
            var testLogger = new TestLogger(LogLevel.Information);
            var ioHelper = new FileSystemWrapper(fileSystemSubstitute, testLogger);
            ResourceManager.Initialize(testLogger, ioHelper, resourceHelperSubstitute, processWrapperSubstitute);

            // Getting the resources shall return the corresponding file content
            fileSystemSubstitute.File.Exists(Path.Combine(testDirectory, "test.sh")).Returns(true);
            fileSystemSubstitute.File.ReadAllBytes(Path.Combine(testDirectory, "test.sh")).Returns(testResourceContentBytes);

            // Act
            var result = ResourceManager.GetInstance().GetResource("test");

            // Assert
            result.Should().Be(testResourceContent);
        }
    }
}

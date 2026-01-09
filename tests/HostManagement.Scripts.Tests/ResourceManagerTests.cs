using System.IO.Abstractions;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace HostManagement.Scripts.Tests;

[Collection("Non Parallel Tests")]
public class ResourceManagerTests
{
    private readonly string _scriptsDirectoryName = "Scripts";
    private readonly string _scriptsDirectory = "HostManagement.Scripts.Resources.Scripts";
    private readonly string _testResourceDeployDirectory = Path.Combine("/tmp", "HostManagement_Resources");

    public class GetInstance : ResourceManagerTests
    {
        [Fact]
        public void ShouldThrow_WhenInstanceIsNull()
        {
            // Arrange
            ResourceManager.ResetInstance();

            // Act
            var result = ResourceManager.GetInstance;

            // Assert
            result.Should().Throw<InvalidOperationException>()
                .WithMessage("ResourceManager is not initialized, use Initialize first.");
        }
    }

    public class Initialize : ResourceManagerTests
    {
        [Fact]
        public void ShouldInitializeResourceManager_WhenCalledWithValidParameters()
        {
            // Arrange
            ResourceManager.ResetInstance();

            var testLogger = new TestLogger(LogLevel.Information);
            var fileSystemSubstitute = Substitute.For<IFileSystem>();
            var resourceHelperSubstitute = Substitute.For<IResourceHelper>();
            var processWrapperSubstitute = Substitute.For<IProcessWrapper>();

            // Act
            ResourceManager.Initialize(
                testLogger,
                fileSystemSubstitute,
                resourceHelperSubstitute,
                processWrapperSubstitute,
                _testResourceDeployDirectory);

            // Assert
            ResourceManager.GetInstance().Should().NotBeNull();
        }
    }

    public class GetResourceBytes : ResourceManagerTests
    {
        [Fact]
        public void ShouldThrow_WhenResourceNameIsUnknown()
        {
            // Arrange
            ResourceManager.ResetInstance();

            var testLogger = new TestLogger(LogLevel.Information);
            var fileSystemSubstitute = Substitute.For<IFileSystem>();
            var resourceHelperSubstitute = Substitute.For<IResourceHelper>();
            var processWrapperSubstitute = Substitute.For<IProcessWrapper>();

            ResourceManager.Initialize(
                testLogger,
                fileSystemSubstitute,
                resourceHelperSubstitute,
                processWrapperSubstitute,
                _testResourceDeployDirectory);

            var unknownResourceName = "unknownResource";
            var expectedMessage = $"Resource with name '{unknownResourceName}' is unknown.";

            // Act
            var result = () => ResourceManager.GetInstance().GetResourceBytes(unknownResourceName);

            // Assert
            result.Should()
                .Throw<InvalidOperationException>()
                .WithMessage(expectedMessage);
        }
    }

    public class GetResource : ResourceManagerTests
    {
        [Fact]
        public void ShouldGetResourceContent_WhenGetResourceCalled()
        {
            // Arrange
            ResourceManager.ResetInstance();

            var testEmbeddedResourceName = "test";
            var testEmbeddedResourceFile = "test.sh";
            var testEmbeddedResourceLocation = string.Concat(_scriptsDirectory, ".", testEmbeddedResourceFile);
            var testEmbeddedResourceContent = "test content";
            var testEmbeddedResourceContentBytes = Encoding.UTF8.GetBytes(testEmbeddedResourceContent);

            var testResourceDeployDirectoryStructure = Path.Combine(_testResourceDeployDirectory, _scriptsDirectoryName);

            var fileSystemSubstitute = Substitute.For<IFileSystem>();
            var directoryInfoSubstitute = Substitute.For<IDirectoryInfo>();
            var resourceHelperSubstitute = Substitute.For<IResourceHelper>();
            var processWrapperSubstitute = Substitute.For<IProcessWrapper>();

            // No resources shall be deployed yet
            fileSystemSubstitute.Directory.Exists(_testResourceDeployDirectory).Returns(false);

            // No directory structure shall exist...
            fileSystemSubstitute.Directory.Exists(testResourceDeployDirectoryStructure).Returns(false);

            // ...so CreateDirectory shall return a new IDirectoryInfo for each created directory
            directoryInfoSubstitute.Exists.Returns(true);
            directoryInfoSubstitute.FullName.Returns(testResourceDeployDirectoryStructure);
            directoryInfoSubstitute.Name.Returns(_scriptsDirectoryName);

            fileSystemSubstitute.Directory
                .CreateDirectory(testResourceDeployDirectoryStructure)
                .Returns(directoryInfoSubstitute);

            // GetManifestResourceNames shall return the embedded resource locations (namespace syntax)
            resourceHelperSubstitute
                .GetManifestResourceNames(Arg.Any<Func<string, bool>?>())
                .Returns([testEmbeddedResourceLocation]);

            // GetResourceStream shall return the resource stream of the resource...
            var resourceStream = new MemoryStream(testEmbeddedResourceContentBytes);
            resourceHelperSubstitute.GetResourceStream(testEmbeddedResourceLocation).Returns(resourceStream);

            // ...and deploy it
            var expandableMemoryStream = new MemoryStream();
            resourceStream.CopyTo(expandableMemoryStream);

            fileSystemSubstitute.File
                .CreateText(Path.Combine(testResourceDeployDirectoryStructure, testEmbeddedResourceFile))
                .Returns(_ => new StreamWriter(expandableMemoryStream, Encoding.UTF8, 16, true));

            fileSystemSubstitute.File
                .Exists(Path.Combine(testResourceDeployDirectoryStructure, testEmbeddedResourceFile))
                .Returns(true);

            // Getting the resources shall return the corresponding file content
            fileSystemSubstitute.File
                .ReadAllBytes(Path.Combine(testResourceDeployDirectoryStructure, testEmbeddedResourceFile))
                .Returns(testEmbeddedResourceContentBytes);

            // Return "OK" when setting execution permissions
            processWrapperSubstitute
                .Start(Arg.Any<string>(), Arg.Any<string>())
                .Returns(new ScriptResponse(0, "", ""));

            // ResourceManager can now be initialized
            var testLogger = new TestLogger(LogLevel.Information);
            ResourceManager.Initialize(
                testLogger,
                fileSystemSubstitute,
                resourceHelperSubstitute,
                processWrapperSubstitute,
                _testResourceDeployDirectory);

            // ResourceManager can now deploy the scripts
            var deployResult = ResourceManager.GetInstance().DeployScripts();

            // Act
            var result = ResourceManager.GetInstance().GetResource(testEmbeddedResourceName);

            // Assert
            deployResult.Should().BeTrue();
            result.Should().Be(testEmbeddedResourceContent);
        }
    }
}

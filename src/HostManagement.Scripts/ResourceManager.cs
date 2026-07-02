using System.Collections.Frozen;
using System.IO.Abstractions;
using System.Text;
using HostManagement.Scripts.Enums;
using HostManagement.Scripts.Extensions;
using Microsoft.Extensions.Logging;

namespace HostManagement.Scripts;

/// <summary>
/// Represents the <see cref="ResourceManager"/> responsible for deploying and handling resources.
/// </summary>
/// <remarks>
/// <see cref="ResourceManager"/> has to be initialized via
/// <see cref="Initialize(ILogger, IFileSystem, string, string)"/>.
/// </remarks>
public sealed partial class ResourceManager
{
    private const string TargetsDirectory = "targets";

    private static readonly Lock s_instanceLock = new();
    private static ResourceManager? s_instance;

    private static readonly string s_resourcesDirectory =
        string.Concat(typeof(ResourceManager).Namespace, ".", "Resources");

    private readonly ILogger _logger;
    private readonly IFileSystem _fileSystem;
    private readonly IResourceHelper _resourceHelper;
    private readonly IProcessWrapper _processWrapper;
    private readonly string _resourceDeployDirectory;
    private readonly string _targetEnvironmentString;
    private readonly FrozenDictionary<string, ResourceInformation> _resourceDictionary;

    private bool _isDeploymentInProgress;
    private readonly Lock _deploymentLock = new();

    private ResourceManager(
        ILogger logger,
        IFileSystem fileSystem,
        IResourceHelper resourceHelper,
        IProcessWrapper processWrapper,
        string resourceDeployDirectory,
        string targetEnvironmentString)
    {
        _logger = logger;
        _fileSystem = fileSystem;
        _resourceHelper = resourceHelper;
        _processWrapper = processWrapper;
        _resourceDeployDirectory = resourceDeployDirectory;

        // Embedded resources use '_' instead of '-' so we need to replace it
        _targetEnvironmentString = targetEnvironmentString.Replace('-', '_');

        var resources = _resourceHelper.GetManifestResourceNames(FilterResources);

        _resourceDictionary = AddResourcesToResourceDictionary(resources).ToFrozenDictionary(StringComparer.Ordinal);


        bool FilterResources(string resource) =>
            resource.StartsWith(s_resourcesDirectory) &&
            resource.Contains(_targetEnvironmentString);
    }

    /// <summary>
    /// Indicates if (re)deployment of resources is in progress.
    /// </summary>
    internal bool IsDeploymentInProgress
    {
        get => Volatile.Read(ref _isDeploymentInProgress);
        private set => Volatile.Write(ref _isDeploymentInProgress, value);
    }

    /// <summary>
    /// Gets a read-only dictionary that contains resource information indexed by resource name.
    /// </summary>
    /// <remarks>This dictionary provides access to various resources used within the application. It is
    /// populated at runtime and cannot be modified directly.</remarks>
    internal IReadOnlyDictionary<string, ResourceInformation> ResourceDictionary => _resourceDictionary;

    /// <summary>
    /// Gets the <see cref="ResourceManager"/> instance after it was initialized via
    /// <see cref="Initialize(ILogger, IFileSystem, string, string)"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException"/>
    public static ResourceManager GetInstance()
    {
        if (s_instance is null)
        {
            throw new InvalidOperationException(
                $"{nameof(ResourceManager)} is not initialized, use {nameof(Initialize)} first.");
        }

        return s_instance;
    }

    /// <summary>
    /// Initializes the <see cref="ResourceManager"/>.
    /// </summary>
    /// <param name="logger">An <see cref="ILogger"/> implementation.</param>
    /// <param name="fileSystem">An <see cref="IFileSystem"/> implementation.</param>
    /// <param name="resourceDeployDirectory">The resource deploy directory.</param>
    /// <param name="targetEnvironmentString">The target environment string.</param>
    /// <returns>The <see cref="ResourceManager"/> instance</returns>
    public static ResourceManager Initialize(
        ILogger logger,
        IFileSystem fileSystem,
        string resourceDeployDirectory,
        string targetEnvironmentString)
        => Initialize(
            logger,
            fileSystem,
            new ResourceHelper(),
            new ProcessWrapper(),
            resourceDeployDirectory,
            targetEnvironmentString);

    /// <summary>
    /// Initializes the <see cref="ResourceManager"/>.
    /// </summary>
    /// <param name="logger">An <see cref="ILogger"/> implementation.</param>
    /// <param name="fileSystem">An <see cref="IFileSystem"/> implementation.</param>
    /// <param name="resourceHelper">An <see cref="IResourceHelper"/> implementation.</param>
    /// <param name="processWrapper">An <see cref="IProcessWrapper"/> implementation.</param>
    /// <param name="resourceDeployDirectory">The resource deploy directory.</param>
    /// <param name="targetEnvironmentString">The target environment string.</param>
    /// <returns>The <see cref="ResourceManager"/> instance</returns>
    /// <remarks>Must only be used for unit tests.</remarks>
    internal static ResourceManager Initialize(
        ILogger logger,
        IFileSystem fileSystem,
        IResourceHelper resourceHelper,
        IProcessWrapper processWrapper,
        string resourceDeployDirectory,
        string targetEnvironmentString)
    {
        if (s_instance is null)
        {
            lock (s_instanceLock)
            {
#pragma warning disable CA1508
                s_instance ??= new ResourceManager(
                    logger,
                    fileSystem,
                    resourceHelper,
                    processWrapper,
                    resourceDeployDirectory,
                    targetEnvironmentString);
#pragma warning restore CA1508
            }
        }

        return s_instance;
    }

    /// <summary>
    /// Resets the <see cref="ResourceManager"/> instance.
    /// </summary>
    /// <remarks>Must only be used for unit tests.</remarks>
    internal static void ResetInstance()
    {
        lock (s_instanceLock)
        {
            s_instance = null;
        }
    }

    /// <summary>
    /// Deploys the application scripts to the resource deploy directory. If a script is already present, it is not
    /// overwritten. This is ok as long as HostManagement is only "updated" by flashing the device with a new image.
    /// </summary>
    /// <remarks>
    /// If the directory already exists and it is a redeployment, the directory will be cleaned up first.
    /// </remarks>
    /// <returns><see langword="true"/> if successful, otherwise <see langword="false"/></returns>
    public bool DeployScripts(bool isRedeployment = false)
    {
        lock (_deploymentLock)
        {
            try
            {
                IsDeploymentInProgress = true;

                if (isRedeployment)
                    CleanUpResourceDeployDirectory();

                var hasDeployedScripts = false;

                foreach (var (_, resourceInfo) in _resourceDictionary)
                {
                    if (!_fileSystem.File.Exists(resourceInfo.DeployPath))
                    {
                        DeployScript(resourceInfo);
                        hasDeployedScripts = true;
                    }
                }

                if (hasDeployedScripts)
                {
                    if (isRedeployment)
                        ScriptsRedeployed(_logger, _resourceDeployDirectory);
                    else
                        ScriptsDeployed(_logger, _resourceDeployDirectory);
                }

                return true;
            }
            catch (Exception ex)
            {
                if (isRedeployment)
                    ScriptRedeployFailed(_logger, ex);
                else
                    ScriptDeployFailed(_logger, ex);

                return false;
            }
            finally
            {
                IsDeploymentInProgress = false;
            }
        }
    }

    /// <summary>
    /// Gets the resource path for the resource with '<paramref name="resourceName"/>'.
    /// </summary>
    /// <param name="resourceName">The resource name</param>
    /// <returns>The resource path as a string.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the resource name is unknown or if the resource cannot be found at the expected path.
    /// </exception>
    internal string GetResourcePath(string resourceName)
    {
        if (!_resourceDictionary.TryGetValue(resourceName, out var resourceInfo))
            throw new InvalidOperationException($"Resource with name '{resourceName}' is unknown.");

        if (!_fileSystem.File.Exists(resourceInfo.DeployPath))
            throw new InvalidOperationException($"Resource with name '{resourceName}' could not be found.");

        return resourceInfo.DeployPath;
    }

    private Dictionary<string, ResourceInformation> AddResourcesToResourceDictionary(
        string[] embeddedResourceLocations)
    {
        var resourceDictionary = new Dictionary<string, ResourceInformation>(StringComparer.Ordinal);

        foreach (var embeddedResourceLocation in embeddedResourceLocations)
        {
            var resourceInfo = GetResourceInformation(embeddedResourceLocation);

            AddResourceToResourceDictionary(resourceDictionary, resourceInfo);
        }

        return resourceDictionary;
    }

    private ResourceInformation GetResourceInformation(string embeddedResourceLocation)
    {
        // Skip "HostManagement.Scripts.Resources." from embedded resource location,
        // so the relative embedded resource location starts with "Scripts."
        var relativeEmbeddedResourceLocation = embeddedResourceLocation[(s_resourcesDirectory.Length + 1)..];
        var relativeEmbeddedResourceLocationParts = relativeEmbeddedResourceLocation.Split('.');

        if (relativeEmbeddedResourceLocationParts.Length == 0 ||
            string.IsNullOrEmpty(embeddedResourceLocation))
        {
            throw new InvalidOperationException(
                $"Relative resource name for embedded resource '{embeddedResourceLocation}' was empty.");
        }

        string resourceDirectoryStructure;
        string resourceName;
        string resourceFile;

        if (relativeEmbeddedResourceLocationParts.Length >= 3 &&
            relativeEmbeddedResourceLocationParts.Last().Equals("sh"))
        {
            // Scripts.file.sh / Scripts.Dir.file.sh etc. -> Currently only '.sh' files are supported

            // relative embedded resource location without file name and extension
            resourceDirectoryStructure = string.Join(
                Path.DirectorySeparatorChar,
                relativeEmbeddedResourceLocationParts[..^2]);

            resourceName = relativeEmbeddedResourceLocationParts[^2];
            resourceFile = resourceName + "." + relativeEmbeddedResourceLocationParts[^1];
        }
        else
        {
            // Scripts.file / Scripts.Dir.file etc.

            // relative embedded resource location without file name
            resourceDirectoryStructure = string.Join(
                Path.DirectorySeparatorChar,
                relativeEmbeddedResourceLocationParts[..^1]);

            resourceName = relativeEmbeddedResourceLocationParts[^1];
            resourceFile = resourceName;
        }

        var deployDirectoryStructure = Path.Combine(_resourceDeployDirectory, resourceDirectoryStructure);
        deployDirectoryStructure = GetTargetIndependentDeployDirectoryStructure(deployDirectoryStructure);

        return new ResourceInformation(
            embeddedResourceLocation,
            relativeEmbeddedResourceLocation,
            deployDirectoryStructure,
            resourceFile);
    }

    private string GetTargetIndependentDeployDirectoryStructure(string deployDirectoryStructure)
    {
        // For the target independent structure we need to replace e.g. "Scripts/targets/debian" with "Scripts"
        var filteredParts = deployDirectoryStructure
            .Split(Path.DirectorySeparatorChar)
            .Where(part => part != TargetsDirectory && part != _targetEnvironmentString);

        return string.Join(Path.DirectorySeparatorChar, filteredParts);
    }

    private void AddResourceToResourceDictionary(
        Dictionary<string, ResourceInformation> resourceDictionary,
        ResourceInformation resourceInfo)
    {
        if (!resourceDictionary.TryAdd(resourceInfo.Name, resourceInfo))
        {
            throw new InvalidOperationException(
                $"Could not add resource '{resourceInfo.EmbeddedResourceLocation}' with key '{resourceInfo.Name}' " +
                $"to resource dictionary (resource name may already exist).");
        }
#if DEBUG
        Console.WriteLine(resourceInfo);
#endif
        AddedResourceToResourceDictionary(_logger, resourceInfo.RelativeEmbeddedResourceLocation);
    }

    private void CleanUpResourceDeployDirectory()
    {
        if (!_fileSystem.Directory.Exists(_resourceDeployDirectory))
            return;

        _fileSystem.Directory.Delete(_resourceDeployDirectory, true);

        DirectoryDeleted(_logger, _resourceDeployDirectory);
    }

    private void DeployScript(ResourceInformation resourceInfo)
    {
        if (resourceInfo.Name.Equals(ScriptIdentifier.UnitTesting.ToString()))
            return;

        if (!_fileSystem.Directory.Exists(resourceInfo.DeployDirectoryStructure))
        {
            _ = _fileSystem.Directory.CreateDirectory(resourceInfo.DeployDirectoryStructure);

            DirectoryCreated(_logger, resourceInfo.DeployDirectoryStructure);
        }

        CreateOrOverwriteScriptFile(resourceInfo.EmbeddedResourceLocation, resourceInfo.DeployPath);
        SetScriptExecutePermission(resourceInfo.DeployPath);
    }

    private void CreateOrOverwriteScriptFile(string embeddedResourceLocation, string resourceDeployPath)
    {
        try
        {
            using var resourceStream = _resourceHelper.GetResourceStream(embeddedResourceLocation)
                ?? throw new InvalidOperationException($"Resource stream for resource '{embeddedResourceLocation}' was null.");

            using var reader = new StreamReader(resourceStream);
            var resourceContent = reader.ReadToEnd();

            var isOverwrite = _fileSystem.File.Exists(resourceDeployPath);

            using var writer = _fileSystem.File.CreateText(resourceDeployPath);
            writer.Write(resourceContent.EnsureLFLineEndings());

            if (isOverwrite)
                FileOverwritten(_logger, resourceDeployPath);
            else
                FileCreated(_logger, resourceDeployPath);
        }
        catch (Exception ex)
        {
            throw new IOException($"Could not create or overwrite file '{resourceDeployPath}'.", ex);
        }
    }

    private void SetScriptExecutePermission(string scriptPath)
    {
        try
        {
            var response = _processWrapper.Start(
                @"/bin/bash",
                $"-c \"chmod +x {scriptPath}\"");

            if (response.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    $"Failed to set execute permission for '{scriptPath}'. Error: {response.StandardError}");

            }
        }
        catch (Exception ex)
        {
            throw new IOException($"An error occurred while setting execute permission for '{scriptPath}'.", ex);
        }
    }

    internal sealed class ResourceInformation(
        string embeddedResourceLocation,
        string relativeEmbeddedResourceLocation,
        string deployDirectoryStructure,
        string resourceFile)
    {
        /// <summary>
        /// The deploy directory structure of the resource (e.g.: "/var/lib/hostmanagement/Scripts").
        /// </summary>
        public string DeployDirectoryStructure { get; init; } = deployDirectoryStructure;

        /// <summary>
        /// The deploy directory structure including the resource file (e.g.: "/var/lib/hostmanagement/Scripts/script.sh").
        /// </summary>
        public string DeployPath { get; init; } = Path.Combine(deployDirectoryStructure, resourceFile);

        /// <summary>
        /// The location of the embedded resource (e.g.: "HostManagement.Scripts.Resources.Scripts.script.sh").
        /// </summary>
        public string EmbeddedResourceLocation { get; init; } = embeddedResourceLocation;

        /// <summary>
        /// The relative location of the embedded resource (e.g.: "Scripts.script.sh").
        /// </summary>
        public string RelativeEmbeddedResourceLocation { get; init; } = relativeEmbeddedResourceLocation;

        /// <summary>
        /// The name of the resource without file extension (e.g.: "script").
        /// </summary>
        // Can not handle multiple file extensions (e.g.: ".tar.gz")
        public string Name { get; init; } = Path.GetFileNameWithoutExtension(resourceFile) ?? string.Empty;

        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine($"Resource information for {resourceFile}");
            builder.AppendLine($"> EmbeddedResourceLocation: {EmbeddedResourceLocation}");
            builder.AppendLine($"> RelativeEmbeddedResourceLocation: {RelativeEmbeddedResourceLocation}");
            builder.AppendLine($"> DeployPath: {DeployPath}");
            builder.AppendLine($"> Name: {Name}");

            return builder.ToString();
        }
    }

    [LoggerMessage(LogLevel.Information, "Scripts deployed successfully to '{ResourceDeployDirectory}'.")]
    private static partial void ScriptsDeployed(ILogger logger, string resourceDeployDirectory);

    [LoggerMessage(LogLevel.Information, "Scripts redeployed successfully to '{ResourceDeployDirectory}'.")]
    private static partial void ScriptsRedeployed(ILogger logger, string resourceDeployDirectory);

    [LoggerMessage(LogLevel.Error, "Script deployment failed.")]
    private static partial void ScriptDeployFailed(ILogger logger, Exception? exception);

    [LoggerMessage(LogLevel.Error, "Script redeployment failed.")]
    private static partial void ScriptRedeployFailed(ILogger logger, Exception? exception);

    [LoggerMessage(LogLevel.Debug, "Deleted directory '{ResourceDeployDirectory}' successfully.")]
    private static partial void DirectoryDeleted(ILogger logger, string resourceDeployDirectory);

    [LoggerMessage(LogLevel.Debug, "Created directory '{ResourceDeployDirectory}' successfully.")]
    private static partial void DirectoryCreated(ILogger logger, string resourceDeployDirectory);

    [LoggerMessage(LogLevel.Debug, "Created file '{ResourceDeployPath}' successfully.")]
    private static partial void FileCreated(ILogger logger, string resourceDeployPath);

    [LoggerMessage(LogLevel.Debug, "Overwritten file '{ResourceDeployPath}' successfully.")]
    private static partial void FileOverwritten(ILogger logger, string resourceDeployPath);

    [LoggerMessage(LogLevel.Debug, "Added '{RelativeEmbeddedResourceLocation}' to resource dictionary.")]
    private static partial void AddedResourceToResourceDictionary(ILogger logger, string relativeEmbeddedResourceLocation);
}

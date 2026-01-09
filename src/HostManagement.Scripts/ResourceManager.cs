using System.IO.Abstractions;
using System.Text;
using Microsoft.Extensions.Logging;
using HostManagement.Scripts.Extensions;

namespace HostManagement.Scripts;

/// <summary>
/// Represents the <see cref="ResourceManager"/> responsible for deploying and handling resources.
/// </summary>
/// <remarks>
/// <see cref="ResourceManager"/> has to be initialized via <see cref="Initialize(ILogger, IFileSystem, string)"/>.
/// </remarks>
public sealed partial class ResourceManager
{
    private sealed class ResourceInformation(
        string embeddedResourceLocation,
        string deployDirectory,
        string deployFile)
    {
        /// <summary>
        /// The deploy directory of the resource (e.g.: "/tmp/Scripts").
        /// </summary>
        public string DeployDirectory { get; init; } = deployDirectory;

        /// <summary>
        /// The deploy path of the resource (e.g.: "/tmp/Scripts/script.sh").
        /// </summary>
        public string DeployPath { get; init; } = Path.Combine(deployDirectory, deployFile);

        /// <summary>
        /// The location of the embedded resource (e.g.: "HostManagement.Scripts.Resources.Scripts.script.sh").
        /// </summary>
        public string EmbeddedResourceLocation { get; init; } = embeddedResourceLocation;

        /// <summary>
        /// The name of the resource (e.g.: "script").
        /// </summary>
        // Can not handle multiple file extensions (e.g.: ".tar.gz")
        public string Name { get; init; } = Path.GetFileNameWithoutExtension(deployFile) ?? string.Empty;
    }

    private static readonly Lock s_instanceLock = new();
    private static ResourceManager? s_instance;

    private static readonly string s_resourcesDirectory =
        string.Concat(typeof(ResourceManager).Namespace!, ".", "Resources");

    private static readonly Func<string, bool> s_resourcesDirectoryFilter =
        embeddedResourceLocation => embeddedResourceLocation.StartsWith(s_resourcesDirectory);

    private readonly ILogger _logger;
    private readonly IFileSystem _fileSystem;
    private readonly IResourceHelper _resourceHelper;
    private readonly IProcessWrapper _processWrapper;
    private readonly string _resourceDeployDirectory;
    private readonly Dictionary<string, ResourceInformation> _resourceDictionary = [];

    private ResourceManager(
        ILogger logger,
        IFileSystem fileSystem,
        IResourceHelper resourceHelper,
        IProcessWrapper processWrapper,
        string resourceDeployDirectory)
    {
        _logger = logger;
        _fileSystem = fileSystem;
        _resourceHelper = resourceHelper;
        _processWrapper = processWrapper;
        _resourceDeployDirectory = resourceDeployDirectory;

        AddResourcesToResourceDictionary();
    }

    /// <summary>
    /// Indicates if (re)deployment of resources is in progress.
    /// </summary>
    public bool IsDeploymentInProgress { get; private set; }

    /// <summary>
    /// Gets the <see cref="ResourceManager"/> instance after it was initialized via
    /// <see cref="Initialize(ILogger, IFileSystem, string)"/>.
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
    /// <returns>The <see cref="ResourceManager"/> instance</returns>
    public static ResourceManager Initialize(ILogger logger, IFileSystem fileSystem, string resourceDeployDirectory)
        => Initialize(
            logger,
            fileSystem,
            new ResourceHelper(),
            new ProcessWrapper(),
            resourceDeployDirectory);

    /// <summary>
    /// Initializes the <see cref="ResourceManager"/>.
    /// </summary>
    /// <param name="logger">An <see cref="ILogger"/> implementation.</param>
    /// <param name="fileSystem">An <see cref="IFileSystem"/> implementation.</param>
    /// <param name="resourceHelper">An <see cref="IResourceHelper"/> implementation.</param>
    /// <param name="processWrapper">An <see cref="IProcessWrapper"/> implementation.</param>
    /// <param name="resourceDeployDirectory">The resource deploy directory.</param>
    /// <returns>The <see cref="ResourceManager"/> instance</returns>
    /// <remarks>Must only be used for unit tests.</remarks>
    internal static ResourceManager Initialize(
        ILogger logger,
        IFileSystem fileSystem,
        IResourceHelper resourceHelper,
        IProcessWrapper processWrapper,
        string resourceDeployDirectory)
    {
        if (s_instance is null)
        {
            lock (s_instanceLock)
            {
#pragma warning disable CA1508
                s_instance ??= new ResourceManager(logger, fileSystem, resourceHelper, processWrapper, resourceDeployDirectory);
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
    /// Gets the resource with '<paramref name="resourceName"/>' as an UTF8 string.
    /// </summary>
    /// <param name="resourceName">The resource name.</param>
    /// <returns>The resource as an UTF8 string</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public string GetResource(string resourceName)
    {
        var bytes = GetResourceBytes(resourceName);

        try
        {
            return Encoding.UTF8.GetString(bytes);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Could not load content of resource '{resourceName}'.",
                ex);
        }
    }

    /// <summary>
    /// Gets the resource with '<paramref name="resourceName"/>' as a byte array.
    /// </summary>
    /// <param name="resourceName">The resource name.</param>
    /// <returns>The resource as a byte array</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public byte[] GetResourceBytes(string resourceName)
    {
        if (!_resourceDictionary.TryGetValue(resourceName, out var resourceInfo))
        {
            throw new InvalidOperationException($"Resource with name '{resourceName}' is unknown.");
        }

        if (!_fileSystem.File.Exists(resourceInfo.DeployPath))
        {
            throw new InvalidOperationException($"Resource with name '{resourceName}' could not be found.");
        }

        try
        {
            return _fileSystem.File.ReadAllBytes(resourceInfo.DeployPath);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Could not load resource with name '{resourceName}'.", ex);
        }
    }

    /// <summary>
    /// Deploys the application scripts to the resource deploy directory.
    /// </summary>
    /// <returns><see langword="true"/> if successful, otherwise <see langword="false"/></returns>
    public bool DeployScripts(bool isRedeployment = false)
    {
        try
        {
            IsDeploymentInProgress = true;

            if (isRedeployment)
                CleanUpResourceDeployPath();

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
                    LogScriptsRedeployed(_logger, _resourceDeployDirectory);
                else
                    LogScriptsDeployed(_logger, _resourceDeployDirectory);
            }

            return true;
        }
        catch (Exception ex)
        {
            if (isRedeployment)
                LogScriptRedeployFailed(_logger, ex);
            else
                LogScriptDeployFailed(_logger, ex);

            return false;
        }
        finally
        {
            IsDeploymentInProgress = false;
        }
    }

    private void CleanUpResourceDeployPath()
    {
        if (!_fileSystem.Directory.Exists(_resourceDeployDirectory))
            return;

        _fileSystem.Directory.Delete(_resourceDeployDirectory, true);

        LogDirectoryDeleted(_logger, _resourceDeployDirectory);
    }

    private ResourceInformation GetResourceInformation(string embeddedResourceLocation)
    {
        // Skip "HostManagement.Scripts.Resources." from embedded resource location
        var relativeEmbeddedResourceNamespace = embeddedResourceLocation[(s_resourcesDirectory.Length + 1)..];
        var relativeEmbeddedResourceNamespaceParts = relativeEmbeddedResourceNamespace.Split('.');

        if (relativeEmbeddedResourceNamespaceParts.Length == 0 ||
            string.IsNullOrEmpty(embeddedResourceLocation))
        {
            throw new InvalidOperationException(
                $"Relative resource name for embedded resource '{embeddedResourceLocation}' was empty.");
        }

        string resourceDirectoryStructure;
        string resourceName;
        string resourceFile;

        if (relativeEmbeddedResourceNamespaceParts.Length > 1 &&
            relativeEmbeddedResourceNamespaceParts.Last().Equals("sh"))
        {
            // file.ext / dir.file.ext etc.

            // relative embedded resource namespace without file name and extension
            resourceDirectoryStructure = string.Join(
                Path.DirectorySeparatorChar,
                relativeEmbeddedResourceNamespaceParts[..^2]);

            resourceName = relativeEmbeddedResourceNamespaceParts[^2];
            resourceFile = resourceName + "." + relativeEmbeddedResourceNamespaceParts[^1];
        }
        else
        {
            // file / dir.file etc.

            // relative embedded resource namespace without file name
            resourceDirectoryStructure = string.Join(
                Path.DirectorySeparatorChar,
                relativeEmbeddedResourceNamespaceParts[..^1]);

            resourceName = relativeEmbeddedResourceNamespaceParts[^1];
            resourceFile = resourceName;
        }

        var resourceDeployDirectoryStructure = Path.Combine(
            _resourceDeployDirectory,
            resourceDirectoryStructure);

        return new ResourceInformation(embeddedResourceLocation, resourceDeployDirectoryStructure, resourceFile);
    }

    private void DeployScript(ResourceInformation resourceInfo)
    {
        if (!_fileSystem.Directory.Exists(resourceInfo.DeployDirectory))
        {
            _ = _fileSystem.Directory.CreateDirectory(resourceInfo.DeployDirectory);

            LogDirectoryCreated(_logger, resourceInfo.DeployDirectory);
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
                LogFileOverwritten(_logger, resourceDeployPath);
            else
                LogFileCreated(_logger, resourceDeployPath);
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

    private void AddResourcesToResourceDictionary()
    {
        var embeddedResourceLocations = _resourceHelper.GetManifestResourceNames(s_resourcesDirectoryFilter);

        foreach (var embeddedResourceLocation in embeddedResourceLocations)
        {
            var resourceInfo = GetResourceInformation(embeddedResourceLocation);

            AddResourceToResourceDictionary(resourceInfo);
        }
    }

    private void AddResourceToResourceDictionary(ResourceInformation resourceInfo)
    {
        if (!_resourceDictionary.TryAdd(resourceInfo.Name, resourceInfo))
        {
            throw new InvalidOperationException(
                $"Could not add resource '{resourceInfo.Name}' to resource dictionary " +
                $"(resource name may already exist).");
        }
    }

    [LoggerMessage(LogLevel.Information, "Scripts deployed successfully to '{ResourceDeployDirectory}'.")]
    private static partial void LogScriptsDeployed(ILogger logger, string resourceDeployDirectory);

    [LoggerMessage(LogLevel.Information, "Scripts redeployed successfully to '{ResourceDeployDirectory}'.")]
    private static partial void LogScriptsRedeployed(ILogger logger, string resourceDeployDirectory);

    [LoggerMessage(LogLevel.Error, "Script deployment failed.")]
    private static partial void LogScriptDeployFailed(ILogger logger, Exception? exception);

    [LoggerMessage(LogLevel.Error, "Script redeployment failed.")]
    private static partial void LogScriptRedeployFailed(ILogger logger, Exception? exception);

    [LoggerMessage(LogLevel.Debug, "Deleted directory '{ResourceDeployDirectory}' successfully.")]
    private static partial void LogDirectoryDeleted(ILogger logger, string? resourceDeployDirectory);

    [LoggerMessage(LogLevel.Debug, "Created directory '{ResourceDeployDirectory}' successfully.")]
    private static partial void LogDirectoryCreated(ILogger logger, string? resourceDeployDirectory);

    [LoggerMessage(LogLevel.Debug, "Created file '{ResourceDeployPath}' successfully.")]
    private static partial void LogFileCreated(ILogger logger, string? resourceDeployPath);

    [LoggerMessage(LogLevel.Debug, "Overwritten file '{ResourceDeployPath}' successfully.")]
    private static partial void LogFileOverwritten(ILogger logger, string? resourceDeployPath);
}

﻿using System.IO.Abstractions;
using System.Text;
using Microsoft.Extensions.Logging;
using HostManagement.Configuration;
using HostManagement.Scripts.Extensions;
using HostManagement.Common.Wrappers;

namespace HostManagement.Scripts;

/// <summary>
/// Represents the <see cref="ResourceManager"/> responsible for deploying and handling resources.
/// </summary>
/// <remarks>
/// <see cref="ResourceManager"/> has to be initialized via <see cref="Initialize(ILogger)"/>.
/// </remarks>
public sealed class ResourceManager
{
    private static readonly object s_instanceLock = new();
    private static ResourceManager? s_instance;

    private static readonly string s_namespace = typeof(ResourceManager).Namespace!;
    private static readonly string s_resourcesDirectory = string.Concat(s_namespace, ".", "Resources");

    private readonly ILogger _logger;
    private readonly FileSystemWrapper _fileSystemWrapper;
    private readonly IResourceHelper _resourceHelper;
    private readonly IProcessWrapper _processWrapper;
    private readonly Dictionary<string, string> _resourceDictionary = [];

    private ResourceManager(
        ILogger logger,
        FileSystemWrapper fileSystemWrapper,
        IResourceHelper resourceHelper,
        IProcessWrapper processWrapper)
    {
        _logger = logger;
        _fileSystemWrapper = fileSystemWrapper;
        _resourceHelper = resourceHelper;
        _processWrapper = processWrapper;

        DeployScripts();
    }

    /// <summary>
    /// Indicates if (re)deployment of resources is in progress.
    /// </summary>
    public bool IsDeploymentInProgress { get; private set; }

    /// <summary>
    /// Gets the <see cref="ResourceManager"/> instance after it was initialized via
    /// <see cref="Initialize(ILogger)"/>.
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
    /// Initializes the <see cref="ResourceManager"/> and deploys all resources to the
    /// <see cref="EnvironmentVariables.ResourceDeployDirectory"/>.
    /// </summary>
    /// <param name="logger">A logger.</param>
    /// <returns>The <see cref="ResourceManager"/> instance</returns>
    public static ResourceManager Initialize(ILogger logger)
        => Initialize(
            logger,
            new FileSystemWrapper(
                new FileSystem(),
                logger),
            new ResourceHelper(),
            new ProcessWrapper());

    /// <summary>
    /// Initializes the <see cref="ResourceManager"/> and deploys all resources to the
    /// <see cref="EnvironmentVariables.ResourceDeployDirectory"/>.
    /// </summary>
    /// <param name="logger">A logger.</param>
    /// <param name="fileSystemWrapper">The file system wrapper.</param>
    /// <param name="resourceHelper">An resource helper implementation.</param>
    /// <param name="processWrapper">An process wrapper implementation.</param>
    /// <returns>The <see cref="ResourceManager"/> instance</returns>
    /// <remarks>Must only be used for unit tests.</remarks>
    internal static ResourceManager Initialize(
        ILogger logger,
        FileSystemWrapper fileSystemWrapper,
        IResourceHelper resourceHelper,
        IProcessWrapper processWrapper)
    {
        if (s_instance is null)
        {
            lock (s_instanceLock)
            {
#pragma warning disable CA1508
                s_instance ??= new ResourceManager(logger, fileSystemWrapper, resourceHelper, processWrapper);
#pragma warning restore CA1508
            }
        }

        return s_instance;
    }

    /// <summary>
    /// Gets the resource with '<paramref name="resourceName"/>' as a byte array.
    /// </summary>
    /// <param name="resourceName">The resource name.</param>
    /// <returns>The resource as a byte array</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public byte[] GetResourceBytes(string resourceName)
    {
        if (!_resourceDictionary.TryGetValue(resourceName, out var resourcePath))
        {
            var errorMessage = $"Resource with name '{resourceName}' is unknown.";

            _logger.LogError(errorMessage);

            throw new InvalidOperationException(errorMessage);
        }

        if (!_fileSystemWrapper.ExistsFile(resourcePath))
        {
            var errorMessage = $"Resource with name '{resourceName}' could not be found.";

            _logger.LogError(errorMessage);

            throw new InvalidOperationException(errorMessage);
        }

        try
        {
            return _fileSystemWrapper.ReadAllBytesFromFile(resourcePath);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Could not load resource with name '{resourceName}'";

            _logger.LogError(ex, errorMessage);

            throw new InvalidOperationException(errorMessage, ex);
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
            var errorMessage = $"Could not load content of resource '{resourceName}'.";

            _logger.LogError(ex, errorMessage);

            throw new InvalidOperationException(errorMessage, ex);
        }
    }

    /// <summary>
    /// Redeploys the application scripts.
    /// </summary>
    public void RedeployScripts() => DeployScripts(true);

    private void DeployScripts(bool isRedeployment = false)
    {
        try
        {
            IsDeploymentInProgress = true;

            CleanUpResourceDeployPath();

            var resourceNames = _resourceHelper.GetManifestResourceNames();

            foreach (var resourceName in resourceNames)
            {
                if (!resourceName.StartsWith(s_resourcesDirectory))
                    continue;

                DeployScript(resourceName, out var resourceFileName, out var targetResourcePath);

                if (!isRedeployment)
                    AddResourceToResourceDictionary(resourceFileName, targetResourcePath);
            }

            if (isRedeployment)
                _logger.LogInformation("Scripts redeployed successfully.");
            else
                _logger.LogInformation("Scripts deployed successfully.");
        }
        finally
        {
            IsDeploymentInProgress = false;
        }
    }

    private void CleanUpResourceDeployPath()
        => _fileSystemWrapper.DeleteDirectory(EnvironmentVariables.ResourceDeployDirectory, true);

    private void DeployScript(string resourceName, out string resourceFileName, out string targetResourcePath)
    {
        var relativeResourceNamespace = resourceName[(s_resourcesDirectory.Length + 1)..];
        var relativeResourceNamespaceParts = relativeResourceNamespace.Split('.');

        if (relativeResourceNamespaceParts.Length == 0 ||
            string.IsNullOrEmpty(resourceName))
        {
            var errorMessage = $"Relative resource name for resource '{resourceName}' was empty.";

            _logger.LogError(errorMessage);

            throw new InvalidOperationException(errorMessage);
        }

        string resourceFile;
        string resourceDirectoryStructure;

        if (relativeResourceNamespaceParts.Length > 1 &&
            relativeResourceNamespaceParts.Last().Equals("sh"))
        {
            // file.ext / dir.file.ext etc.

            resourceFileName = relativeResourceNamespaceParts[^2];
            resourceFile = resourceFileName + "." + relativeResourceNamespaceParts[^1];

            resourceDirectoryStructure = string.Join(
                Path.DirectorySeparatorChar,
                relativeResourceNamespaceParts[..^2]);
        }
        else
        {
            // file / dir.file etc.

            resourceFileName = relativeResourceNamespaceParts[^1];
            resourceFile = resourceFileName;

            resourceDirectoryStructure = string.Join(
                Path.DirectorySeparatorChar,
                relativeResourceNamespaceParts[..^1]);
        }

        var targetDirectoryStructure = Path.Combine(
            EnvironmentVariables.ResourceDeployDirectory,
            resourceDirectoryStructure);

        targetResourcePath = Path.Combine(targetDirectoryStructure, resourceFile);

        CreateDirectoryIfNecessary(targetDirectoryStructure);
        CreateOrOverwriteScriptFile(resourceName, targetResourcePath);
        SetScriptExecutePermission(targetResourcePath);
    }

    private void AddResourceToResourceDictionary(string resourceFileName, string targetResourcePath)
    {
        if (!_resourceDictionary.TryAdd(resourceFileName, targetResourcePath))
        {
            var errorMessage =
                $"Could not add resource '{resourceFileName}' to resource dictionary " +
                $"(resource name may already exist).";

            _logger.LogError(errorMessage);

            throw new InvalidOperationException(errorMessage);
        }
    }

    private void CreateDirectoryIfNecessary(string directoryPath)
        => _ = _fileSystemWrapper.CreateDirectory(directoryPath);

    private void CreateOrOverwriteScriptFile(string resourceName, string targetFilePath)
    {
        try
        {
            using var resourceStream = _resourceHelper.GetResourceStream(resourceName)
                ?? throw new InvalidOperationException("stream was null.");

            using var reader = new StreamReader(resourceStream);
            var resourceContent = reader.ReadToEnd();

            _fileSystemWrapper.WriteToFile(targetFilePath, resourceContent.EnsureLFLineEndings());
        }
        catch (Exception ex)
        {
            var errorMessage = $"Could not create or overwrite file '{targetFilePath}'.";

            _logger.LogError(ex, errorMessage);

            throw new IOException(errorMessage, ex);
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
                var errorMessage = $"Failed to set execute permission for '{scriptPath}'. Error: {response.StandardError}";

                _logger.LogError(errorMessage);

                throw new InvalidOperationException(errorMessage);

            }
        }
        catch (Exception ex)
        {
            var errorMessage = $"An error occurred while setting execute permission for '{scriptPath}'.";

            _logger.LogError(ex, errorMessage);

            throw new IOException(errorMessage, ex);
        }
    }
}

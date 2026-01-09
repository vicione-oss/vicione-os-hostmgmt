using System.IO.Abstractions;
using Microsoft.Extensions.Logging;

namespace HostManagement.Common.Wrappers;

/// <summary>
/// Provides wrapper methods for file system operations using <see cref="System.IO.Abstractions"/>.
/// </summary>
public sealed partial class FileSystemWrapper
{
    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new <see cref="FileSystemWrapper"/>.
    /// </summary>
    /// <param name="fileSystem">An <see cref="IFileSystem"/> implementation.</param>
    /// <param name="logger">An <see cref="ILogger"/> implementation.</param>
    public FileSystemWrapper(IFileSystem fileSystem, ILogger logger)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc cref="IDirectory.CreateDirectory(string)"/>
    public IDirectoryInfo CreateDirectory(string path)
    {
        try
        {
            if (ExistsDirectory(path))
                return _fileSystem.DirectoryInfo.New(path);

            var iDirectoryInfo = _fileSystem.Directory.CreateDirectory(path);

            LogCreatedDirectorySuccessfully(path);
            return iDirectoryInfo;
        }
        catch (Exception ex)
        {
            LogCouldNotCreateDirectory(path, ex.Message);
            throw;
        }
    }

    /// <inheritdoc cref="IDirectory.Delete(string, bool)"/>
    public void DeleteDirectory(string path, bool recursive = true)
    {
        if (!_fileSystem.Directory.Exists(path))
            return;

        try
        {
            _fileSystem.Directory.Delete(path, recursive);
            LogDeletedDirectorySuccessfully(path);
        }
        catch (Exception ex)
        {
            LogCouldNotDeleteDirectory(path, ex.Message);
            throw;
        }
    }

    /// <inheritdoc cref="IFile.Delete(string)"/>
    public void DeleteFile(string path)
    {
        if (!_fileSystem.File.Exists(path))
            return;

        try
        {
            _fileSystem.File.Delete(path);
            LogDeletedFileSuccessfully(path);
        }
        catch (Exception ex)
        {
            LogCouldNotDeleteFile(path, ex.Message);
            throw;
        }
    }

    /// <inheritdoc cref="IDirectory.Exists(string?)"/>
    public bool ExistsDirectory(string path) => _fileSystem.Directory.Exists(path);

    /// <inheritdoc cref="IFile.Exists(string?)"/>
    public bool ExistsFile(string? path) => _fileSystem.File.Exists(path);

    /// <inheritdoc cref="IFile.ReadAllBytes(string)"/>
    public byte[] ReadAllBytesFromFile(string path)
    {
        try
        {
            return _fileSystem.File.ReadAllBytes(path);
        }
        catch (Exception ex)
        {
            LogCouldNotReadFromFile(path, ex.Message);
            throw;
        }
    }

    /// <inheritdoc cref="IFile.CreateText(string)"/>
    public void WriteToFile(string path, string? value)
    {
        try
        {
            var message = ExistsFile(path)
                ? $"Overwritten file '{path}' successfully."
                : $"Created file '{path}' successfully.";

            using var writer = _fileSystem.File.CreateText(path);
            writer.Write(value);

            LogFileCreatedOrOverwrittenSuccessfully(message);
        }
        catch (Exception ex)
        {
            LogCouldNotWriteToFile(path, ex.Message);
            throw;
        }
    }

    [LoggerMessage(LogLevel.Information, "Created directory '{Path}' successfully.")]
    private partial void LogCreatedDirectorySuccessfully(string? path);

    [LoggerMessage(LogLevel.Error, "Could not create directory '{Path}'. {ExceptionMessage}")]
    private partial void LogCouldNotCreateDirectory(string? path, string? exceptionMessage);

    [LoggerMessage(LogLevel.Information, "Deleted directory '{Path}' successfully.")]
    private partial void LogDeletedDirectorySuccessfully(string? path);

    [LoggerMessage(LogLevel.Error, "Could not delete directory '{Path}'. {ExceptionMessage}")]
    private partial void LogCouldNotDeleteDirectory(string? path, string? exceptionMessage);

    [LoggerMessage(LogLevel.Information, "Deleted file '{Path}' successfully.")]
    private partial void LogDeletedFileSuccessfully(string? path);

    [LoggerMessage(LogLevel.Error, "Could not delete file '{Path}'. {ExceptionMessage}")]
    private partial void LogCouldNotDeleteFile(string? path, string? exceptionMessage);

    [LoggerMessage(LogLevel.Error, "Could not read from file '{Path}'. {ExceptionMessage}")]
    private partial void LogCouldNotReadFromFile(string? path, string? exceptionMessage);

    [LoggerMessage(LogLevel.Information, "{InnerMessage}")]
    private partial void LogFileCreatedOrOverwrittenSuccessfully(string? innerMessage);

    [LoggerMessage(LogLevel.Error, "Could not write to file '{Path}'. {ExceptionMessage}")]
    private partial void LogCouldNotWriteToFile(string? path, string? exceptionMessage);
}

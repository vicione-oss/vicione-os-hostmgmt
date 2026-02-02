using System.IO.Abstractions;
using Microsoft.Extensions.Logging;

namespace HostManagement.Common.Wrappers;

/// <summary>
/// Provides wrapper methods for file system operations using <see cref="System.IO.Abstractions"/>.
/// </summary>
/// <param name="fileSystem">An <see cref="IFileSystem"/> implementation.</param>
/// <param name="logger">An <see cref="ILogger"/> implementation.</param>
public sealed partial class FileSystemWrapper(IFileSystem fileSystem, ILogger logger)
{
    private readonly IFileSystem _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <inheritdoc cref="IFile.WriteAllTextAsync(string, string?, CancellationToken)"/>
    public async Task WriteAllTextToFile(string path, string? value, CancellationToken cancellationToken = default)
    {
        try
        {
            using var writer = _fileSystem.File.CreateText(path);
            await writer.WriteAsync(value.AsMemory(), cancellationToken);
        }
        catch (Exception ex)
        {
            LogCouldNotWriteToFile(_logger, path, ex.Message);
            throw;
        }
    }

    [LoggerMessage(LogLevel.Error, "Could not write to file '{Path}'. {ExceptionMessage}")]
    private static partial void LogCouldNotWriteToFile(ILogger logger, string? path, string? exceptionMessage);
}

using System.IO.Abstractions;
using HostManagement.Configuration;
using HostManagement.Shared.Capabilities;
using HostManagement.Shared.Communication.Contracts;
using HostManagement.Shared.Communication.Enums;
using Microsoft.Extensions.Logging;

namespace HostManagement.Services;

/// <summary>
/// Represents the supported capabilities service.
/// </summary>
/// <param name="fileSystem">The file system.</param>
/// <param name="logger">An <see cref="ILogger"/> implementation.</param>
internal sealed partial class SupportedCapabilitiesService(IFileSystem fileSystem, ILogger logger) : IDisposable
{
    private readonly IFileSystem _fileSystem = fileSystem;
    private readonly ILogger _logger = logger;
    private readonly SemaphoreSlim _supportedCapabilitiesSemaphore = new(1, 1);

    private bool _disposed;
    private SupportedCapabilities? _supportedCapabilities;

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
            return;

        _supportedCapabilitiesSemaphore.Dispose();
        _disposed = true;
    }

    /// <summary>
    /// Gets the cached supported capabilities.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see cref="GetSupportedCapabilitiesResult"/></returns>
    internal async Task<GetSupportedCapabilitiesResult> GetSupportedCapabilities(CancellationToken cancellationToken)
    {
        try
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            cancellationToken.ThrowIfCancellationRequested();

            var capabilities = _supportedCapabilities
                ?? throw new InvalidOperationException("Supported capabilities were not loaded.");

            return new GetSupportedCapabilitiesResult()
            {
                Status = OperationStatus.Success,
                Message = string.Empty,
                SupportedCapabilities = capabilities
            };
        }
        catch (OperationCanceledException)
        {
            var message = ServicesResources.GetSupportedCapabilitiesCanceled;

            GetSupportedCapabilitiesCanceled(_logger, message);

            return new GetSupportedCapabilitiesResult()
            {
                Status = OperationStatus.Warning,
                Message = message,
                SupportedCapabilities = new SupportedCapabilities()
            };
        }
        catch (Exception ex)
        {
            var message = string.Join(' ', ServicesResources.GetSupportedCapabilitiesFailed, ex.Message);

            GetSupportedCapabilitiesFailed(_logger, message, ex);

            return new GetSupportedCapabilitiesResult()
            {
                Status = OperationStatus.Error,
                Message = message,
                SupportedCapabilities = new SupportedCapabilities()
            };
        }
    }

    /// <summary>
    /// Initializes the cached supported capabilities from file, if not cached yet.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see cref="SupportedCapabilities"/></returns>
    internal async Task<SupportedCapabilities> InitializeSupportedCapabilities(CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        await _supportedCapabilitiesSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            if (_supportedCapabilities is not null)
                return _supportedCapabilities;

            _supportedCapabilities = await LoadSupportedCapabilitiesFromFile(cancellationToken).ConfigureAwait(false);
            return _supportedCapabilities;
        }
        finally
        {
            _supportedCapabilitiesSemaphore.Release();
        }
    }

    private async Task<SupportedCapabilities> LoadSupportedCapabilitiesFromFile(CancellationToken cancellationToken)
    {
        var capabilities = new SupportedCapabilities();

        if (!_fileSystem.File.Exists(AppPaths.SupportedCapabilitiesFilePath))
        {
            NoSupportedCapabilitiesFileFound(_logger, AppPaths.SupportedCapabilitiesFilePath);
            return capabilities;
        }

        await using var stream = _fileSystem.FileStream.New(AppPaths.SupportedCapabilitiesFilePath, new FileStreamOptions()
        {
            Access = FileAccess.Read,
            Mode = FileMode.Open,
            Options = FileOptions.Asynchronous | FileOptions.SequentialScan,
            Share = FileShare.Read,
        });

        using var reader = new StreamReader(stream);

        var iniContent = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        using var jsonDocument = SupportedCapabilitiesIniConverter.ToJsonDocument(iniContent);

        capabilities.ApplyCapabilities(jsonDocument.RootElement);

        return capabilities;
    }

    [LoggerMessage(LogLevel.Warning, "Found no supported capabilities file '{Path}'.")]
    private static partial void NoSupportedCapabilitiesFileFound(ILogger logger, string? path);

    [LoggerMessage(LogLevel.Warning, "{InnerMessage}")]
    private static partial void GetSupportedCapabilitiesCanceled(ILogger logger, string? innerMessage);

    [LoggerMessage(LogLevel.Error, "{InnerMessage}")]
    private static partial void GetSupportedCapabilitiesFailed(ILogger logger, string? innerMessage, Exception? exception);
}

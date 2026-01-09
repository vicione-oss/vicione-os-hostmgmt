using HostManagement.Configuration;
using HostManagement.Shared.Contracts;

namespace HostManagement.SystemConfig;

/// <summary>
/// Represents a singleton cache for the <see cref="SystemConfiguration"/> with automatic invalidation.
/// <para>
/// To configure invalidation lifetime use environment variable.</para>
/// </summary>
internal sealed class SystemConfigurationCache : IDisposable
{
    private static readonly Lazy<SystemConfigurationCache> s_instance = new(() => new SystemConfigurationCache());
    private readonly Lock _instanceLock = new();

    private long _cacheLifetimeSeconds;
    private SystemConfiguration? _cachedConfiguration;
    private TimeProvider _timeProvider = TimeProvider.System;
    private ITimer? _cacheInvalidationTimer;

    private SystemConfigurationCache()
        => SetCacheLifetimeSeconds(EnvironmentVariables.SystemConfigurationCacheLifetimeInSeconds.Value);

    ~SystemConfigurationCache()
    {
        try
        {
            DisposeInvalidationTimer();
        }
        catch
        {
            // Finalizers should never throw
        }
    }

    /// <summary>
    /// Gets the <see cref="SystemConfigurationCache"/> instance.
    /// </summary>
    public static SystemConfigurationCache Instance => s_instance.Value;

    /// <summary>
    /// Gets the cached configuration if available or null.
    /// </summary>
    public SystemConfiguration? GetCachedConfig()
    {
        lock (_instanceLock)
        {
            return _cachedConfiguration;
        }
    }

    /// <summary>
    /// Sets the cached configuration and starts a timer to invalidate the cache.
    /// </summary>
    /// <param name="configuration">The <see cref="SystemConfiguration"/> to cache.</param>
    /// <param name="cacheLog">Optional delegate for logging that the system configuration has
    /// been cached.</param>
    /// <param name="invalidateCacheLog">Optional delegate for logging that the system
    /// configuration cache has been invalidated.</param>
    /// <param name="invalidateCacheFailedLog">Optional delegate for logging that the system
    /// configuration cache invalidation failed.</param>
    public void SetCachedConfig(
        SystemConfiguration configuration,
        Action<string>? cacheLog = null,
        Action<string>? invalidateCacheLog = null,
        Action<string>? invalidateCacheFailedLog = null)
    {
        lock (_instanceLock)
        {
            _cachedConfiguration = configuration;
            cacheLog?.Invoke(SystemConfigResources.ConfigCached);

            DisposeInvalidationTimer();

            _cacheInvalidationTimer = _timeProvider.CreateTimer(
                _ => Invalidate(invalidateCacheLog, invalidateCacheFailedLog),
                null,
                dueTime: TimeSpan.FromSeconds(_cacheLifetimeSeconds),
                period: Timeout.InfiniteTimeSpan);
        }
    }

    /// <summary>
    /// Invalidates the cache and stops the invalidation timer.
    /// </summary>
    /// <param name="invalidateCacheLog">Optional delegate for logging that the
    /// system configuration cache has been invalidated.</param>
    /// <param name="invalidateCacheFailedLog">Optional delegate for logging that the system
    /// configuration cache invalidation failed.</param>
    public void Invalidate(
        Action<string>? invalidateCacheLog = null,
        Action<string>? invalidateCacheFailedLog = null)
    {
        lock (_instanceLock)
        {
            try
            {
                _cachedConfiguration = null;

                DisposeInvalidationTimer();

                invalidateCacheLog?.Invoke(SystemConfigResources.ConfigCacheInvalidated);
            }
            catch (Exception ex)
            {
                invalidateCacheFailedLog?.Invoke(
                    SystemConfigResources.ConfigCacheInvalidationFailed +
                    ex.ToString());
            }
        }
    }

    /// <summary>
    /// Sets the cache lifetime duration in seconds.
    /// </summary>
    /// <remarks>If the cache invalidation timer is active, it will be reset with the new lifetime duration.</remarks>
    /// <param name="seconds">The duration, in seconds, for which the cache remains valid.</param>
    internal void SetCacheLifetimeSeconds(long seconds)
    {
        lock (_instanceLock)
        {
            if (seconds < 0)
                seconds = 0;

            _cacheLifetimeSeconds = seconds;

            _cacheInvalidationTimer?.Change(
                dueTime: TimeSpan.FromSeconds(_cacheLifetimeSeconds),
                period: Timeout.InfiniteTimeSpan);
        }
    }

    internal void SetTimeProviderForTests(TimeProvider timeProvider)
    {
        lock (_instanceLock)
        {
            _timeProvider = timeProvider;
        }
    }

    internal void ResetForTests()
    {
        lock (_instanceLock)
        {
            _cachedConfiguration = null;

            DisposeInvalidationTimer();

            SetCacheLifetimeSeconds(EnvironmentVariables.SystemConfigurationCacheLifetimeInSeconds.Value);
            _timeProvider = TimeProvider.System;
        }
    }

    /// <summary>
    /// Disposes resources associated with the instance.
    /// </summary>
    public void Dispose()
    {
        lock (_instanceLock)
        {
            DisposeInvalidationTimer();

            // Suppress finalization so the finalizer won't run
            GC.SuppressFinalize(this);
        }
    }

    private void DisposeInvalidationTimer()
    {
        var timer = _cacheInvalidationTimer;
        _cacheInvalidationTimer = null;

        timer?.Dispose();
    }
}

using AwesomeAssertions;
using HostManagement.Shared.Contracts;
using HostManagement.SystemConfig;
using Xunit;

namespace HostManagement.Tests.SystemConfig;

[Collection("Non Parallel Tests")]
public sealed class SystemConfigurationCacheTests : IDisposable
{
    public void Dispose() => SystemConfigurationCache.Instance.ResetForTests();

    [Fact]
    public void ShouldReturnNull_WhenNothingIsCached()
        => SystemConfigurationCache.Instance.GetCachedConfig().Should().BeNull();

    [Fact]
    public void ShouldReturnCachedConfiguration()
    {
        // Arrange
        var cache = SystemConfigurationCache.Instance;
        cache.SetTimeProviderForTests(new FakeTimeProvider());

        var config = new SystemConfiguration();

        cache.SetCachedConfig(config);

        // Act + Assert
        cache.GetCachedConfig().Should().BeSameAs(config);
    }

    [Fact]
    public void ShouldInvalidateCachedConfiguration()
    {
        // Arrange
        var cache = SystemConfigurationCache.Instance;
        cache.SetTimeProviderForTests(new FakeTimeProvider());

        cache.SetCachedConfig(new SystemConfiguration());

        // Act
        cache.Invalidate();

        // Assert
        cache.GetCachedConfig().Should().BeNull();
    }

    [Fact]
    public void ShouldInvalidate_WhenTimerFires()
    {
        // Arrange
        var cache = SystemConfigurationCache.Instance;
        var timeProvider = new FakeTimeProvider();
        cache.SetTimeProviderForTests(timeProvider);

        var invalidateLogged = false;

        cache.SetCachedConfig(
            new SystemConfiguration(),
            invalidateCacheLog: _ => invalidateLogged = true);

        cache.GetCachedConfig().Should().NotBeNull();
        invalidateLogged.Should().BeFalse();

        // Act
        timeProvider.LastTimer!.Fire();

        // Assert
        cache.GetCachedConfig().Should().BeNull();
        invalidateLogged.Should().BeTrue();
    }

    [Fact]
    public void ShouldDisposeTimer_WhenCacheIsInvalidated()
    {
        // Arrange
        var cache = SystemConfigurationCache.Instance;
        var timeProvider = new FakeTimeProvider();
        cache.SetTimeProviderForTests(timeProvider);

        cache.SetCachedConfig(new SystemConfiguration());

        var timer = timeProvider.LastTimer!;

        // Act
        cache.Invalidate();

        // Assert
        timer.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public void ShouldDisposeTimer_WhenDisposed()
    {
        // Arrange
        var cache = SystemConfigurationCache.Instance;
        var timeProvider = new FakeTimeProvider();

        cache.SetTimeProviderForTests(timeProvider);
        cache.SetCachedConfig(new SystemConfiguration());

        var timer = timeProvider.LastTimer!;
        timer.IsDisposed.Should().BeFalse();

        // Act
        cache.Dispose();

        // Assert
        timer.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public void ShouldDisposePreviousTimer_WhenConfigurationIsCachedAgain()
    {
        // Arrange
        var cache = SystemConfigurationCache.Instance;
        var timeProvider = new FakeTimeProvider();
        cache.SetTimeProviderForTests(timeProvider);

        cache.SetCachedConfig(new SystemConfiguration());
        var firstTimer = timeProvider.LastTimer!;

        // Act
        cache.SetCachedConfig(new SystemConfiguration());

        // Assert
        firstTimer.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public void ShouldUseConfiguredLifetime_WhenCreatingTimer()
    {
        // Arrange
        var cache = SystemConfigurationCache.Instance;
        var timeProvider = new FakeTimeProvider();
        cache.SetTimeProviderForTests(timeProvider);
        cache.SetCacheLifetimeSeconds(42);

        // Act
        cache.SetCachedConfig(new SystemConfiguration());

        // Assert
        timeProvider.LastTimer!.DueTime.Should().Be(TimeSpan.FromSeconds(42));
        timeProvider.LastTimer.Period.Should().Be(Timeout.InfiniteTimeSpan);
    }

    [Fact]
    public void ShouldResetTimer_WhenCacheLifetimeChanges()
    {
        // Arrange
        var cache = SystemConfigurationCache.Instance;
        var timeProvider = new FakeTimeProvider();
        cache.SetTimeProviderForTests(timeProvider);

        cache.SetCachedConfig(new SystemConfiguration());

        // Act
        cache.SetCacheLifetimeSeconds(21);

        // Assert
        timeProvider.LastTimer!.DueTime.Should().Be(TimeSpan.FromSeconds(21));
        timeProvider.LastTimer.Period.Should().Be(Timeout.InfiniteTimeSpan);
    }

    [Fact]
    public void ShouldUseZeroLifetime_WhenCacheLifetimeIsNegative()
    {
        // Arrange
        var cache = SystemConfigurationCache.Instance;
        var timeProvider = new FakeTimeProvider();
        cache.SetTimeProviderForTests(timeProvider);

        // Act
        cache.SetCacheLifetimeSeconds(-1);
        cache.SetCachedConfig(new SystemConfiguration());

        // Assert
        timeProvider.LastTimer!.DueTime.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void ShouldLog_WhenConfigurationIsCached()
    {
        // Arrange
        var cache = SystemConfigurationCache.Instance;
        cache.SetTimeProviderForTests(new FakeTimeProvider());

        string? message = null;

        // Act
        cache.SetCachedConfig(new SystemConfiguration(), cacheLog: value => message = value);

        // Assert
        message.Should().Be("System configuration has been cached.");
    }

    [Fact]
    public void ShouldLog_WhenCacheIsInvalidated()
    {
        // Arrange
        var cache = SystemConfigurationCache.Instance;
        cache.SetTimeProviderForTests(new FakeTimeProvider());

        string? message = null;

        cache.SetCachedConfig(new SystemConfiguration());

        // Act
        cache.Invalidate(value => message = value);

        // Assert
        message.Should().Be("System configuration cache has been invalidated.");
    }

    [Fact]
    public void ShouldLogFailure_WhenInvalidationFails()
    {
        // Arrange
        var cache = SystemConfigurationCache.Instance;
        cache.SetTimeProviderForTests(new FakeTimeProvider());
        cache.SetCachedConfig(new SystemConfiguration());

        string? failureMessage = null;

        // Act
        cache.Invalidate(
            invalidateCacheLog: _ => throw new InvalidOperationException("Test exception"),
            invalidateCacheFailedLog: message => failureMessage = message);

        // Assert
        failureMessage.Should().ContainAll(SystemConfigResources.ConfigCacheInvalidationFailed, "Test exception");
    }

    private sealed class FakeTimeProvider : TimeProvider
    {
        public FakeTimer? LastTimer { get; private set; }

        // Gets called in 'SystemConfigurationCache' > 'SetCachedConfig'
        public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
            => LastTimer = new FakeTimer(callback, state, dueTime, period);

        internal sealed class FakeTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
            : ITimer
        {
            public TimeSpan DueTime { get; private set; } = dueTime;
            public TimeSpan Period { get; private set; } = period;
            public bool IsDisposed { get; private set; }

            public bool Change(TimeSpan dueTime, TimeSpan period)
            {
                DueTime = dueTime;
                Period = period;
                return true;
            }

            public void Fire() => callback(state);

            public void Dispose() => IsDisposed = true;

            public ValueTask DisposeAsync()
            {
                Dispose();
                return ValueTask.CompletedTask;
            }
        }
    }
}

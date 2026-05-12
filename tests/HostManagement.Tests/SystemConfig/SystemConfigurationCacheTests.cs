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
    public void SetCachedConfig_ShouldCacheAndInvalidate()
    {
        // Arrange
        SystemConfigurationCache.Instance.ResetForTests();
        var cache = SystemConfigurationCache.Instance;

        var timeProvider = new FakeTimeProvider();
        cache.SetTimeProviderForTests(timeProvider);
        cache.SetCacheLifetimeSeconds(1);

        var cacheLogged = false;
        var invalidateLogged = false;

        var config = new SystemConfiguration();
        config.NetworkDNSSettings.Hostname = "debian";

        // Act
        cache.SetCachedConfig(
            config,
            cacheLog: _ => cacheLogged = true,
            invalidateCacheLog: _ => invalidateLogged = true);

        // Assert (cached)
        cache.GetCachedConfig().Should().BeEquivalentTo(config);
        cacheLogged.Should().BeTrue();
        invalidateLogged.Should().BeFalse();

        timeProvider.LastTimer.Should().NotBeNull();
        timeProvider.LastTimer!.Fire();

        // Assert (invalidated)
        cache.GetCachedConfig().Should().BeNull();
        invalidateLogged.Should().BeTrue();
    }

    private sealed class FakeTimeProvider : TimeProvider
    {
        public FakeTimer? LastTimer { get; private set; }

        // Gets called in 'SystemConfigurationCache' > 'SetCachedConfig'
        public override ITimer CreateTimer(
            TimerCallback callback,
            object? state,
            TimeSpan dueTime,
            TimeSpan period)
            => LastTimer = new FakeTimer(callback, state);

        internal sealed class FakeTimer(TimerCallback callback, object? state) : ITimer, IAsyncDisposable
        {
            public bool Change(TimeSpan dueTime, TimeSpan period) => true;

            public void Fire() => callback(state);

            public void Dispose() { }

            public ValueTask DisposeAsync()
            {
                Dispose();
                return ValueTask.CompletedTask;
            }
        }
    }
}

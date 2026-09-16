using System.Collections.Concurrent;

namespace PipeCommunicationSample;

internal sealed class KnownActionTracker
{
    private readonly ConcurrentDictionary<Guid, DateTimeOffset> _knownActions = [];
    private readonly TimeSpan _lifetime;

    public KnownActionTracker(TimeSpan lifetime)
    {
        if (lifetime <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(lifetime));

        _lifetime = lifetime;
    }

    public bool Add(Guid correlationId)
    {
        if (correlationId == Guid.Empty)
            return false;

        RemoveExpired();

        return _knownActions.TryAdd(
            correlationId,
            DateTimeOffset.UtcNow + _lifetime);
    }

    public bool Contains(Guid correlationId)
    {
        if (correlationId == Guid.Empty ||
            !_knownActions.TryGetValue(correlationId, out var expiresAt))
        {
            return false;
        }

        if (expiresAt > DateTimeOffset.UtcNow)
            return true;

        _knownActions.TryRemove(correlationId, out _);
        return false;
    }

    public bool Remove(Guid correlationId)
        => _knownActions.TryRemove(correlationId, out _);

    private void RemoveExpired()
    {
        var now = DateTimeOffset.UtcNow;

        foreach (var knownAction in _knownActions)
        {
            if (knownAction.Value <= now)
                _knownActions.TryRemove(knownAction.Key, out _);
        }
    }
}

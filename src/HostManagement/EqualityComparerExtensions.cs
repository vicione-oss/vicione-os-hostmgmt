using HostManagement;

namespace HostManagement;

internal static class EqualityComparerExtensions
{
    internal static bool SequenceEqual<T>(this IEqualityComparer<T> comparer, IEnumerable<T>? x, IEnumerable<T>? y)
        => comparer.SequenceEqual<T, T>(x, y, default);

    internal static bool SequenceEqual<T, TKey>(
        this IEqualityComparer<T> comparer,
        IEnumerable<T>? x,
        IEnumerable<T>? y,
        Func<T, TKey>? order = default)
    {
        if (x is null && y is null)
            return true;
        else if (x is null || y is null)
            return false;
        if (order is not null)
        {
            x = x.OrderBy(order);
            y = y.OrderBy(order);
        }
        return x.SequenceEqual(y, comparer);
    }
}

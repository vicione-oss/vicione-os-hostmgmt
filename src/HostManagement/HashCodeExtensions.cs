namespace HostManagement;

internal static class HashCodeExtensions
{
    internal static void AddRange<T>(this HashCode hashCode, IEnumerable<T>? values, IEqualityComparer<T> comparer)
        => hashCode.AddRange<T, T>(values, comparer, default);

    internal static void AddRange<T, TKey>(
        this HashCode hashCode,
        IEnumerable<T>? values,
        IEqualityComparer<T> comparer,
        Func<T, TKey>? order = default)
    {
        if (values is not null)
        {
            if (order is not null)
                values = values.OrderBy(order);

            foreach (var value in values)
                hashCode.Add(value, comparer);
        }
    }
}

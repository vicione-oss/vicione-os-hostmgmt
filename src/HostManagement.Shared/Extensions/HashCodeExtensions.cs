namespace HostManagement.Shared.Extensions;

/// <summary>
/// Provides extension members for <see cref="HashCode"/>.
/// </summary>
public static class HashCodeExtensions
{
    /// <summary>
    /// Adds a range of values to the hash code, optionally using a custom comparer.
    /// </summary>
    /// <typeparam name="T">The type of the values.</typeparam>
    /// <param name="hashCode">The hash code to add the values to.</param>
    /// <param name="values">The values to add to the hash code.</param>
    /// <param name="comparer">An optional comparer to use for the values.</param>
    public static void AddRange<T>(
        ref this HashCode hashCode,
        IEnumerable<T>? values,
        IEqualityComparer<T>? comparer = null)
            => hashCode.AddRange<T, T>(values, comparer, default);

    /// <summary>
    /// Adds a range of values to the hash code, optionally using a custom comparer and ordering function.
    /// </summary>
    /// <typeparam name="T">The type of the values.</typeparam>
    /// <typeparam name="TKey">The type of the key to order the values by.</typeparam>
    /// <param name="hashCode">The hash code to add the values to.</param>
    /// <param name="values">The values to add to the hash code.</param>
    /// <param name="comparer">An optional comparer to use for the values.</param>
    /// <param name="order">An optional function to order the values by.</param>
    public static void AddRange<T, TKey>(
        ref this HashCode hashCode,
        IEnumerable<T>? values,
        IEqualityComparer<T>? comparer = null,
        Func<T, TKey>? order = null)
    {
        if (values is null)
            return;

        if (order is not null)
            values = values.OrderBy(order);

        foreach (var value in values)
            hashCode.Add(value, comparer);
    }
}

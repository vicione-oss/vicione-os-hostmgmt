using HostManagement;

namespace HostManagement;

/// <summary>
/// Provides extension methods for <see cref="IEqualityComparer{T}"/>.
/// </summary>
internal static class EqualityComparerExtensions
{
    /// <summary>
    /// Determines whether two sequences are equal using the specified comparer.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="comparer">The comparer to use for equality.</param>
    /// <param name="x">The first sequence.</param>
    /// <param name="y">The second sequence.</param>
    /// <returns><see langword="true"/> when the sequences are equal; otherwise <see langword="false"/>.</returns>
    internal static bool SequenceEqual<T>(this IEqualityComparer<T> comparer, IEnumerable<T>? x, IEnumerable<T>? y)
        => comparer.SequenceEqual<T, T>(x, y, default);

    /// <summary>
    /// Determines whether two sequences are equal using the specified comparer and optional ordering.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <typeparam name="TKey">The sort key type.</typeparam>
    /// <param name="comparer">The comparer to use for equality.</param>
    /// <param name="x">The first sequence.</param>
    /// <param name="y">The second sequence.</param>
    /// <param name="order">Optional key selector used to order the sequences before comparison.</param>
    /// <returns><see langword="true"/> when the sequences are equal; otherwise <see langword="false"/>.</returns>
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

namespace HostManagement.SystemConfig;

/// <summary>
/// Provides side-effect-free operations for detecting configuration changes.
/// </summary>
internal static class SystemConfigurationChangeDetector
{
    /// <summary>
    /// Determines whether two values differ according to their default equality semantics.
    /// </summary>
    /// <typeparam name="T">The type of the values.</typeparam>
    /// <param name="current">The current value.</param>
    /// <param name="proposed">The proposed value.</param>
    /// <returns>
    /// <see langword="true"/> if the values differ, otherwise <see langword="false"/>.
    /// </returns>
    internal static bool HasChanges<T>(T? current, T? proposed)
        => !EqualityComparer<T>.Default.Equals(current, proposed);

    /// <summary>
    /// Determines whether two sequences differ in content or order.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="current">The current sequence.</param>
    /// <param name="proposed">The proposed sequence.</param>
    /// <returns>
    /// <see langword="true"/> if the sequences differ, otherwise <see langword="false"/>.
    /// </returns>
    internal static bool HasSequenceChanges<T>(IEnumerable<T>? current, IEnumerable<T>? proposed)
    {
        if (ReferenceEquals(current, proposed))
            return false;

        if (current is null || proposed is null)
            return true;

        return !current.SequenceEqual(proposed, EqualityComparer<T>.Default);
    }

    /// <summary>
    /// Determines whether two sequences differ in content using the specified order.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <typeparam name="TKey">The sort key type.</typeparam>
    /// <param name="current">The current sequence.</param>
    /// <param name="proposed">The proposed sequence.</param>
    /// <param name="order">Key selector used to order the sequences before comparison.</param>
    /// <returns>
    /// <see langword="true"/> if the sequences differ, otherwise <see langword="false"/>.
    /// </returns>
    internal static bool HasSequenceChanges<T, TKey>(
        IEnumerable<T>? current,
        IEnumerable<T>? proposed,
        Func<T, TKey>? order)
    {
        if (ReferenceEquals(current, proposed))
            return false;

        if (current is null || proposed is null)
            return true;

        if (order is not null)
        {
            current = current.OrderBy(order);
            proposed = proposed.OrderBy(order);
        }

        return !current.SequenceEqual(proposed, EqualityComparer<T>.Default);
    }
}

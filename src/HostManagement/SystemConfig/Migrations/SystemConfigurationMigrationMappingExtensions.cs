namespace HostManagement.SystemConfig.Migrations;

/// <summary>
/// Provides extension methods for mapping between system configuration contract versions.
/// </summary>
internal static class SystemConfigurationMigrationMappingExtensions
{
    /// <summary>
    /// Maps a nullable reference type when a source value is available.
    /// </summary>
    /// <typeparam name="TSource">The source value type.</typeparam>
    /// <typeparam name="TTarget">The target value type.</typeparam>
    /// <param name="source">The source value.</param>
    /// <param name="mapping">The mapping function.</param>
    /// <returns>The mapped target value or <see langword="null"/>.</returns>
    internal static TTarget? MapOrNull<TSource, TTarget>(this TSource? source, Func<TSource, TTarget> mapping)
        where TSource : class
        where TTarget : class
        => source is null ? null : mapping(source);

    /// <summary>
    /// Maps an enumerable source to a target list when a source value is available.
    /// </summary>
    /// <typeparam name="TSource">The source item type.</typeparam>
    /// <typeparam name="TTarget">The target item type.</typeparam>
    /// <param name="source">The source enumerable.</param>
    /// <param name="mapping">The item mapping function.</param>
    /// <returns>The mapped target list or <see langword="null"/>.</returns>
    internal static List<TTarget>? MapListOrNull<TSource, TTarget>(
        this IEnumerable<TSource>? source,
        Func<TSource, TTarget> mapping)
        => source?.Select(mapping).ToList();

    /// <summary>
    /// Copies an enumerable source to a list when a source value is available.
    /// </summary>
    /// <typeparam name="T">The item type.</typeparam>
    /// <param name="source">The source enumerable.</param>
    /// <returns>The copied list or <see langword="null"/>.</returns>
    internal static List<T>? CopyListOrNull<T>(this IEnumerable<T>? source) => source?.ToList();
}

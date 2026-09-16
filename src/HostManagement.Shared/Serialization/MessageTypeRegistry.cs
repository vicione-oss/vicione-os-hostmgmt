using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using HostManagement.Shared.Contracts;

namespace HostManagement.Shared.Serialization;

/// <summary>
/// Maps a message contract type to its source-generated <see cref="JsonTypeInfo{T}"/>.
/// </summary>
/// <remarks>
/// <para>
/// The contract contexts of the shared packages register themselves - this assembly's own from the static
/// constructor, a downstream one from a module initializer - so consumers never have to call <see cref="AddContext"/>.
/// It remains public so that applications with their own contracts can extend the map.
/// </para>
/// <para>
/// <see cref="string"/> is deliberately not resolvable. Raw text is sent as-is over the pipe and must not be routed
/// through JSON, because a <see cref="JsonTypeInfo{T}"/> for <see cref="string"/> would add surrounding quotes.
/// Use <see cref="MessageSerializer"/>, which handles raw text before consulting this map.
/// </para>
/// </remarks>
public static class MessageTypeRegistry
{
    private static readonly ConcurrentDictionary<Type, JsonTypeInfo?> s_resolvedTypeInfos = [];
    private static readonly Lock s_contextLock = new();

    private static JsonSerializerContext[] s_contexts = [];

    static MessageTypeRegistry() => AddContext(SourceGenerationContext.Default);

    /// <summary>
    /// Adds a <see cref="JsonSerializerContext"/> to the map.
    /// </summary>
    /// <param name="context">The source-generated context to add. Adding the same context twice is a no-op.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="context"/> is <see langword="null"/>.
    /// </exception>
    public static void AddContext(JsonSerializerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        lock (s_contextLock)
        {
            if (Array.IndexOf(s_contexts, context) >= 0)
                return;

            // Replace the array instead of mutating it so readers never observe a partially updated map.
            var contexts = new JsonSerializerContext[s_contexts.Length + 1];
            Array.Copy(s_contexts, contexts, s_contexts.Length);
            contexts[^1] = context;

            Volatile.Write(ref s_contexts, contexts);

            // A previously cached miss may be resolvable through the new context.
            s_resolvedTypeInfos.Clear();
        }
    }

    /// <summary>
    /// Tries to get the <see cref="JsonTypeInfo{T}"/> for <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The message contract type.</typeparam>
    /// <param name="typeInfo">
    /// The metadata for <typeparamref name="T"/>, or <see langword="null"/> if no registered context provides it.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the metadata was found, otherwise <see langword="false"/>.
    /// Always <see langword="false"/> for <see cref="string"/> (see <see cref="MessageTypeRegistry"/>).
    /// </returns>
    public static bool TryGetTypeInfo<T>([NotNullWhen(true)] out JsonTypeInfo<T>? typeInfo)
    {
        typeInfo = typeof(T) == typeof(string)
            ? null
            : s_resolvedTypeInfos.GetOrAdd(typeof(T), Resolve) as JsonTypeInfo<T>;

        return typeInfo is not null;
    }

    /// <summary>
    /// Gets the <see cref="JsonTypeInfo{T}"/> for <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The message contract type.</typeparam>
    /// <returns>The metadata for <typeparamref name="T"/>.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <typeparamref name="T"/> is <see cref="string"/>, or when no registered
    /// <see cref="JsonSerializerContext"/> provides metadata for <typeparamref name="T"/>.
    /// </exception>
    public static JsonTypeInfo<T> GetTypeInfo<T>()
    {
        if (typeof(T) == typeof(string))
        {
            throw new InvalidOperationException(
                "'String' is raw text and is not a JSON message contract. " +
                $"Use '{nameof(MessageSerializer)}', which passes raw text through unchanged.");
        }

        if (TryGetTypeInfo<T>(out var typeInfo))
            return typeInfo;

        throw new InvalidOperationException(
            $"No JSON type info is registered for '{typeof(T).FullName}'. " +
            $"Add the type to a '{nameof(JsonSerializerContext)}' with a 'JsonSerializableAttribute', " +
            $"and register that context via '{nameof(MessageTypeRegistry)}.{nameof(AddContext)}'.");
    }

    private static JsonTypeInfo? Resolve(Type type)
    {
        foreach (var context in Volatile.Read(ref s_contexts))
        {
            // Resolving through the owning context keeps the 'JsonSourceGenerationOptions' of that context intact.
            // Combining contexts into shared 'JsonSerializerOptions' would silently drop them.
            if (context.GetTypeInfo(type) is { } typeInfo)
                return typeInfo;
        }

        return null;
    }
}

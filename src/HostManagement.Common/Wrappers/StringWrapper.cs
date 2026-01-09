using HostManagement.Common.Abstractions;

namespace HostManagement.Common.Wrappers;

/// <inheritdoc cref="IStringWrapper"/>
public sealed class StringWrapper : IStringWrapper
{
    /// <inheritdoc/>
    public string ConcatenateStrings(params string?[] values) => string.Concat(values);
}

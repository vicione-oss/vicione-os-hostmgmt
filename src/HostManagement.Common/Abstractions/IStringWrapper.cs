namespace HostManagement.Common.Abstractions;

/// <summary>
/// Represents a string wrapper.
/// </summary>
public interface IStringWrapper
{
    /// <inheritdoc cref="string.Concat(string?[])"/>
    string ConcatenateStrings(params string?[] values);
}

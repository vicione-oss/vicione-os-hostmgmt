using System.Net;

namespace HostManagement.Communication.CommandLine.Output;

/// <summary>
/// Represents a <see cref="CliOutputValue"/>.
/// </summary>
/// <remarks>
/// Can convert a <see cref="CliOutputValue"/> to a <see cref="string"/> implicitly or
/// with <see cref="ToString"/>.<br/>
/// If the provided value is null it will be set to <see cref="CliOutputBase.NotSetValue"/>.
/// </remarks>
internal sealed class CliOutputValue
{
    private readonly string _value;

    private CliOutputValue(object? value)
        => _value = value?.ToString() ?? CliOutputBase.NotSetValue;

    /// <inheritdoc cref="string.Length"/>
    public int Length => _value.Length;

    /// <summary>
    /// The null value (see <see cref="CliOutputBase.NotSetValue"/>).
    /// </summary>
    public static CliOutputValue Null => new(null);

    /// <summary>
    /// Creates a new <see cref="CliOutputValue"/> from a supported type
    /// (e.g. <see cref="string"/>, <see cref="int"/>, <see cref="DateTimeOffset"/> etc.).
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns><see cref="CliOutputValue"/></returns>
    /// <exception cref="ArgumentException"></exception>
    public static CliOutputValue Create(object? value)
    {
        if (value is null ||
            value is bool ||
            value is int ||
            value is float ||
            value is string ||
            value is IPAddress ||
            value is DateTimeOffset ||
            value is DateTime ||
            value is DateOnly ||
            value is TimeOnly)
        {
            return new CliOutputValue(value);
        }

        throw new ArgumentException($"Type '{value.GetType().Name}' is not allowed.");
    }

    /// <summary>
    /// Implicitly converts a <see cref="CliOutputValue"/> to a <see cref="string"/>.
    /// </summary>
    /// <param name="value">The <see cref="CliOutputValue"/> to convert.</param>
    /// <returns><see cref="string"/></returns>
    public static implicit operator string(CliOutputValue value) => value._value;

    /// <summary>
    /// Implicitly converts <see cref="bool"/> to a <see cref="CliOutputValue"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns><see cref="CliOutputValue"/></returns>
    public static implicit operator CliOutputValue(bool? value) => Create(value);

    /// <summary>
    /// Implicitly converts <see cref="int"/> to a <see cref="CliOutputValue"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns><see cref="CliOutputValue"/></returns>
    public static implicit operator CliOutputValue(int? value) => Create(value);

    /// <summary>
    /// Implicitly converts <see cref="float"/> to a <see cref="CliOutputValue"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns><see cref="CliOutputValue"/></returns>
    public static implicit operator CliOutputValue(float? value) => Create(value);

    /// <summary>
    /// Implicitly converts <see cref="string"/> to a <see cref="CliOutputValue"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns><see cref="CliOutputValue"/></returns>
    public static implicit operator CliOutputValue(string? value) => Create(value);

    /// <summary>
    /// Implicitly converts <see cref="IPAddress"/> to a <see cref="CliOutputValue"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns><see cref="CliOutputValue"/></returns>
    public static implicit operator CliOutputValue(IPAddress? value) => Create(value);

    /// <summary>
    /// Implicitly converts <see cref="DateTimeOffset"/> to a <see cref="CliOutputValue"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns><see cref="CliOutputValue"/></returns>
    public static implicit operator CliOutputValue(DateTimeOffset? value) => Create(value);

    /// <summary>
    /// Implicitly converts <see cref="DateTime"/> to a <see cref="CliOutputValue"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns><see cref="CliOutputValue"/></returns>
    public static implicit operator CliOutputValue(DateTime? value) => Create(value);

    /// <summary>
    /// Implicitly converts <see cref="DateOnly"/> to a <see cref="CliOutputValue"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns><see cref="CliOutputValue"/></returns>
    public static implicit operator CliOutputValue(DateOnly? value) => Create(value);

    /// <summary>
    /// Implicitly converts <see cref="TimeOnly"/> to a <see cref="CliOutputValue"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns><see cref="CliOutputValue"/></returns>
    public static implicit operator CliOutputValue(TimeOnly? value) => Create(value);

    /// <inheritdoc/>
    public override string ToString() => _value;
}

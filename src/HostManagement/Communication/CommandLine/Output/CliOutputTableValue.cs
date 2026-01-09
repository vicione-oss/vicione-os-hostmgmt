using System.Net;

namespace HostManagement.Communication.CommandLine.Output;

/// <summary>
/// Represents a <see cref="CliOutputTableValue"/>.
/// </summary>
/// <remarks>
/// Can convert a <see cref="CliOutputTableValue"/> to a <see cref="string"/> implicitly or
/// with <see cref="ToString"/>.<br/>
/// If the provided value is empty it will be set to <see cref="CliOutputBase.TableCellEmptyValue"/>.
/// </remarks>
internal sealed class CliOutputTableValue
{
    private readonly CliOutputValue _value;

    private CliOutputTableValue(CliOutputValue value)
        => _value = string.IsNullOrWhiteSpace(value) ? CliOutputBase.TableCellEmptyValue : value;

    /// <inheritdoc cref="string.Length"/>
    public int Length => _value.Length;

    /// <summary>
    /// The null value (see <see cref="CliOutputBase.NotSetValue"/>).
    /// </summary>
    public static CliOutputTableValue Null => CliOutputValue.Create(null);

    /// <summary>
    /// Creates a new <see cref="CliOutputTableValue"/> from a supported type
    /// (e.g. <see cref="CliOutputValue"/>, <see cref="int"/>, <see cref="DateTimeOffset"/> etc.).
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns><see cref="CliOutputTableValue"/></returns>
    /// <exception cref="ArgumentException"></exception>
    public static CliOutputTableValue Create(object? value)
    {
        if (value is CliOutputValue cliOutputValue)
            return new CliOutputTableValue(cliOutputValue);

        if (value is null or
            bool or
            int or
            float or
            string or
            IPAddress or
            DateTimeOffset or
            DateTime or
            DateOnly or
            TimeOnly)
        {
            return new CliOutputTableValue(CliOutputValue.Create(value));
        }

        throw new ArgumentException($"Type '{value.GetType().Name}' is not allowed.");
    }

    /// <summary>
    /// Implicitly converts a <see cref="CliOutputTableValue"/> to a <see cref="string"/>.
    /// </summary>
    /// <param name="value">The <see cref="CliOutputTableValue"/> to convert.</param>
    /// <returns><see cref="string"/></returns>
    public static implicit operator string(CliOutputTableValue value) => value._value.ToString();

    /// <summary>
    /// Implicitly converts <see cref="CliOutputValue"/> to a <see cref="CliOutputTableValue"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns><see cref="CliOutputTableValue"/></returns>
    public static implicit operator CliOutputTableValue(CliOutputValue value) => Create(value);

    /// <summary>
    /// Implicitly converts <see cref="bool"/> to a <see cref="CliOutputTableValue"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns><see cref="CliOutputTableValue"/></returns>
    public static implicit operator CliOutputTableValue(bool? value) => Create(value);

    /// <summary>
    /// Implicitly converts <see cref="int"/> to a <see cref="CliOutputTableValue"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns><see cref="CliOutputTableValue"/></returns>
    public static implicit operator CliOutputTableValue(int? value) => Create(value);

    /// <summary>
    /// Implicitly converts <see cref="float"/> to a <see cref="CliOutputTableValue"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns><see cref="CliOutputTableValue"/></returns>
    public static implicit operator CliOutputTableValue(float? value) => Create(value);

    /// <summary>
    /// Implicitly converts <see cref="string"/> to a <see cref="CliOutputTableValue"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns><see cref="CliOutputTableValue"/></returns>
    public static implicit operator CliOutputTableValue(string? value) => Create(value);

    /// <summary>
    /// Implicitly converts <see cref="IPAddress"/> to a <see cref="CliOutputTableValue"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns><see cref="CliOutputTableValue"/></returns>
    public static implicit operator CliOutputTableValue(IPAddress? value) => Create(value);

    /// <summary>
    /// Implicitly converts <see cref="DateTimeOffset"/> to a <see cref="CliOutputTableValue"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns><see cref="CliOutputTableValue"/></returns>
    public static implicit operator CliOutputTableValue(DateTimeOffset? value) => Create(value);

    /// <summary>
    /// Implicitly converts <see cref="DateTime"/> to a <see cref="CliOutputTableValue"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns><see cref="CliOutputTableValue"/></returns>
    public static implicit operator CliOutputTableValue(DateTime? value) => Create(value);

    /// <summary>
    /// Implicitly converts <see cref="DateOnly"/> to a <see cref="CliOutputTableValue"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns><see cref="CliOutputTableValue"/></returns>
    public static implicit operator CliOutputTableValue(DateOnly? value) => Create(value);

    /// <summary>
    /// Implicitly converts <see cref="TimeOnly"/> to a <see cref="CliOutputTableValue"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns><see cref="CliOutputTableValue"/></returns>
    public static implicit operator CliOutputTableValue(TimeOnly? value) => Create(value);

    /// <inheritdoc/>
    public override string ToString() => _value.ToString();
}

using System.CommandLine;
using System.CommandLine.Parsing;

namespace HostManagement.Communication.CommandLine.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="Option{T}"/> type.
/// </summary>
internal static class OptionExtensions
{
    /// <summary>
    /// Adds a validator to the option.
    /// </summary>
    /// <typeparam name="T">The type that the option's arguments are expected to be parsed as.</typeparam>
    /// <param name="option">The option.</param>
    /// <param name="validate">The delegate used to validate the option's argument.</param>
    internal static Option<T> WithValidation<T>(
        this Option<T> option,
        Action<OptionResult> validate)
    {
        option.Validators.Add(validate);
        return option;
    }
}

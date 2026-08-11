using System.CommandLine;
using System.CommandLine.Parsing;

namespace HostManagement.Communication.CommandLine.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="Argument{T}"/> type.
/// </summary>
internal static class ArgumentExtensions
{
    /// <summary>
    /// Adds a validator to the argument.
    /// </summary>
    /// <typeparam name="T">The type that the argument is expected to be parsed as.</typeparam>
    /// <param name="argument">The argument.</param>
    /// <param name="validate">The delegate used to validate the argument.</param>
    internal static Argument<T> WithValidation<T>(this Argument<T> argument, Action<ArgumentResult> validate)
    {
        argument.Validators.Add(validate);
        return argument;
    }
}

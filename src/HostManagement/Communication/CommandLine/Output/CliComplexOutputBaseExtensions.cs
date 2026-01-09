namespace HostManagement.Communication.CommandLine.Output;

/// <summary>
/// Provides extension methods for types derived from <see cref="CliComplexOutputBase"/>.
/// </summary>
internal static class CliComplexOutputBaseExtensions
{
    /// <summary>
    /// Adds an error message to this output.
    /// </summary>
    /// <remarks>
    /// The error message is displayed before the output.
    /// </remarks>
    /// <typeparam name="T">The type of the output.</typeparam>
    /// <param name="cliOutput">This <see cref="CliComplexOutputBase"/> object.</param>
    /// <param name="error">The error message.</param>
    /// <returns>This <typeparamref name="T"/> with an error message added.</returns>
    internal static T WithError<T>(this T cliOutput, string error)
        where T : CliComplexOutputBase
    {
        cliOutput.AddErrorMessage(error);
        return cliOutput;
    }

    /// <summary>
    /// Adds an error message to this output using the specified arguments.
    /// </summary>
    /// <remarks>
    /// The error message is displayed before the output.
    /// </remarks>
    /// <typeparam name="T">The type of the output.</typeparam>
    /// <param name="cliOutput">This <see cref="CliComplexOutputBase"/> object.</param>
    /// <param name="error">The error message.</param>
    /// <param name="args">The arguments to replace the placeholders in the error message.</param>
    /// <returns>This <typeparamref name="T"/> with an error message added.</returns>
    internal static T WithError<T>(this T cliOutput, string error, params CliOutputValue[]? args)
        where T : CliComplexOutputBase
    {
        cliOutput.AddErrorMessage(error, args);
        return cliOutput;
    }

    /// <summary>
    /// Adds an information message to this output.
    /// </summary>
    /// <remarks>
    /// The information message is displayed before the output.
    /// </remarks>
    /// <typeparam name="T">The type of the output.</typeparam>
    /// <param name="cliOutput">This <see cref="CliComplexOutputBase"/> object.</param>
    /// <param name="information">The information message.</param>
    /// <returns>This <typeparamref name="T"/> with an information message added.</returns>
    internal static T WithInformation<T>(this T cliOutput, string information)
        where T : CliComplexOutputBase
    {
        cliOutput.AddInformationMessage(information);
        return cliOutput;
    }

    /// <summary>
    /// Adds an information message to this output using the specified arguments.
    /// </summary>
    /// <remarks>
    /// The information message is displayed before the output.
    /// </remarks>
    /// <typeparam name="T">The type of the output.</typeparam>
    /// <param name="cliOutput">This <see cref="CliComplexOutputBase"/> object.</param>
    /// <param name="information">The information message.</param>
    /// <param name="args">The arguments to replace the placeholders in the information message.</param>
    /// <returns>This <typeparamref name="T"/> with an information message added.</returns>
    internal static T WithInformation<T>(this T cliOutput, string information, params CliOutputValue[]? args)
        where T : CliComplexOutputBase
    {
        cliOutput.AddInformationMessage(information, args);
        return cliOutput;
    }

    /// <summary>
    /// Configures this output to append a single item to a new line.
    /// </summary>
    /// <typeparam name="T">The type of the output.</typeparam>
    /// <param name="cliOutput">This <see cref="CliComplexOutputBase"/> object.</param>
    /// <returns>This <typeparamref name="T"/> configured to append a single item to a new line.</returns>
    internal static T WithSingleItemOnNewLine<T>(this T cliOutput)
        where T : CliComplexOutputBase
    {
        cliOutput.AddSingleItemToNewLine();
        return cliOutput;
    }

    /// <summary>
    /// Adds a title to this output.
    /// </summary>
    /// <typeparam name="T">The type of the output.</typeparam>
    /// <param name="cliOutput">This <see cref="CliComplexOutputBase"/> object.</param>
    /// <param name="title">The title.</param>
    /// <param name="enabled">An optional enabled to show after the <paramref name="title"/>.</param>
    /// <param name="useTitleDelimiter">Should a title delimiter be used?</param>
    /// <returns>This <typeparamref name="T"/> with a title added.</returns>
    internal static T WithTitle<T>(this T cliOutput, string title, bool? enabled = null, bool useTitleDelimiter = true)
        where T : CliComplexOutputBase
    {
        cliOutput.AddTitle(title, enabled, useTitleDelimiter);
        return cliOutput;
    }

    /// <summary>
    /// Adds a warning message to this output.
    /// </summary>
    /// <remarks>
    /// The warning message is displayed before the output.
    /// </remarks>
    /// <typeparam name="T">The type of the output.</typeparam>
    /// <param name="cliOutput">This <see cref="CliComplexOutputBase"/> object.</param>
    /// <param name="warning">The warning message.</param>
    /// <returns>This <typeparamref name="T"/> with a warning message added.</returns>
    internal static T WithWarning<T>(this T cliOutput, string warning)
        where T : CliComplexOutputBase
    {
        cliOutput.AddWarningMessage(warning);
        return cliOutput;
    }

    /// <summary>
    /// Adds a warning message to this output using the specified arguments.
    /// </summary>
    /// <remarks>
    /// The warning message is displayed before the output.
    /// </remarks>
    /// <typeparam name="T">The type of the output.</typeparam>
    /// <param name="cliOutput">This <see cref="CliComplexOutputBase"/> object.</param>
    /// <param name="warning">The warning message.</param>
    /// <param name="args">The arguments to replace the placeholders in the warning message.</param>
    /// <returns>This <typeparamref name="T"/> with a warning message added.</returns>
    internal static T WithWarning<T>(this T cliOutput, string warning, params CliOutputValue[]? args)
        where T : CliComplexOutputBase
    {
        cliOutput.AddWarningMessage(warning, args);
        return cliOutput;
    }
}

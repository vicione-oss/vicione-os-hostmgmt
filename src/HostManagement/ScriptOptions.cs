using System.Net;
using HostManagement.Scripts.ScriptOptionNames;

namespace HostManagement;

/// <summary>
/// Represents the script options for a script.
/// </summary>
internal sealed class ScriptOptions
{
    private readonly List<string> _options = [];

    public IEnumerable<string> Options => _options.AsReadOnly();

    /// <summary>
    /// Adds an option <paramref name="name"/> to the collection of script options.
    /// </summary>
    /// <param name="name">The script option name.</param>
    public void Add(ScriptOptionName name) => _options.Add(name);

    /// <summary>
    /// Adds an option <paramref name="name"/> and its <paramref name="arguments"/> to the
    /// collection of script options.
    /// </summary>
    /// <param name="name">The script option name.</param>
    /// <param name="arguments">The script option arguments.</param>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    public void Add(ScriptOptionName name, params string[] arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments, nameof(arguments));

        _options.Add(name);

        foreach (var argument in arguments)
        {
            if (argument is null)
            {
                throw new ArgumentException(
                    $"Script option argument for '{name}' can not be null.",
                    nameof(arguments));
            }

            if (argument.StartsWith("--"))
            {
                throw new ArgumentException(
                    $"Script option argument '{argument}' for '{name}' can not start with '--'.",
                    nameof(arguments));
            }

            _options.Add(argument);
        }
    }

    /// <inheritdoc cref="Add(ScriptOptionName, string[])"/>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public void Add(ScriptOptionName name, params bool[] arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments, nameof(arguments));

        _options.Add(name);

        foreach (var argument in arguments)
            _options.Add(argument.ToString().ToLowerInvariant());
    }

    /// <inheritdoc cref="Add(ScriptOptionName, string[])"/>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public void Add(ScriptOptionName name, params int[] arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments, nameof(arguments));

        _options.Add(name);

        foreach (var argument in arguments)
            _options.Add(argument.ToString());
    }

    /// <inheritdoc cref="Add(ScriptOptionName, string[])"/>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public void Add(ScriptOptionName name, params IPAddress[] arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments, nameof(arguments));

        _options.Add(name);

        foreach (var argument in arguments)
            _options.Add(argument.ToString());
    }
}

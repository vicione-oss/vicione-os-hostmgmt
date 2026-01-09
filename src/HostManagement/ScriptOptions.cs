using System.Net;
using HostManagement.Scripts.ScriptOptionNames;

namespace HostManagement;

/// <summary>
/// Represents the script options for a script.
/// </summary>
internal sealed class ScriptOptions
{
    private readonly List<string> _options = [];

    /// <summary>
    /// Adds an option <paramref name="name"/> to the collection of script options.
    /// </summary>
    /// <param name="name">The script option name.</param>
    public void Add(ScriptOptionName name)
    {
        _options.Add(name);
    }

    /// <inheritdoc cref="Add(ScriptOptionName, string)"/>
    /// <exception cref="ArgumentException"></exception>
    public void Add(ScriptOptionName name, int argument)
    {
        Add(name, argument.ToString());
    }

    /// <inheritdoc cref="Add(ScriptOptionName, string[])"/>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public void Add(ScriptOptionName name, int[] arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments, nameof(arguments));

        _options.Add(name);

        foreach (var argument in arguments)
            _options.Add(argument.ToString());
    }

    /// <inheritdoc cref="Add(ScriptOptionName, string)"/>
    /// <exception cref="ArgumentException"></exception>
    public void Add(ScriptOptionName name, bool argument)
    {
        Add(name, argument.ToString().ToLowerInvariant());
    }

    /// <inheritdoc cref="Add(ScriptOptionName, string[])"/>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public void Add(ScriptOptionName name, bool[] arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments, nameof(arguments));

        _options.Add(name);

        foreach (var argument in arguments)
            _options.Add(argument.ToString().ToLowerInvariant());
    }

    /// <summary>
    /// Adds an option <paramref name="name"/> and its <paramref name="argument"/> to the
    /// collection of script options.
    /// </summary>
    /// <param name="name">The script option name.</param>
    /// <param name="argument">The script option argument.</param>
    /// <exception cref="ArgumentException"></exception>
    public void Add(ScriptOptionName name, string argument)
    {
        if (string.IsNullOrWhiteSpace(argument))
        {
            throw new ArgumentException(
                "Script option argument can not be null, empty or whitespace.",
                nameof(argument));
        }

        _options.Add(name);
        _options.Add(argument);
    }

    /// <summary>
    /// Adds an option <paramref name="name"/> and its <paramref name="arguments"/> to the
    /// collection of script options.
    /// </summary>
    /// <param name="name">The script option name.</param>
    /// <param name="arguments">The script option arguments.</param>
    /// <exception cref="ArgumentException"></exception>
    public void Add(ScriptOptionName name, string[] arguments)
    {
        if (arguments is null)
        {
            throw new ArgumentException(
                "Script option arguments can not be null.",
                nameof(arguments));
        }

        _options.Add(name);

        foreach (var argument in arguments)
        {
            if (string.IsNullOrWhiteSpace(argument))
            {
                throw new ArgumentException(
                    $"Script option argument in '{nameof(arguments)}' can not be null, empty or whitespace.",
                    nameof(arguments));
            }

            _options.Add(argument);
        }
    }

    /// <inheritdoc cref="Add(ScriptOptionName, string)"/>
    /// <exception cref="ArgumentException"></exception>
    public void Add(ScriptOptionName name, IPAddress argument)
    {
        Add(name, argument.ToString());
    }

    /// <inheritdoc cref="Add(ScriptOptionName, string[])"/>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public void Add(ScriptOptionName name, IPAddress[] arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments, nameof(arguments));

        _options.Add(name);

        foreach (var argument in arguments)
            _options.Add(argument.ToString());
    }

    /// <summary>
    /// Creates a new string array of all script options.
    /// </summary>
    public string[] ToArray() => [.. _options];

    /// <summary>
    /// Creates a new List of strings of all script options.
    /// </summary>
    public List<string> ToList() => new(_options);
}

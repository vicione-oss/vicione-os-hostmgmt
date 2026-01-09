namespace HostManagement.Scripts.ScriptOptionNames;

/// <summary>
/// Represents the abstract base class for a script option name.
/// </summary>
/// <remarks>Can convert a <see cref="ScriptOptionName"/> to a <see cref="string"/> implicitly or
/// with <see cref="ToString"/>.</remarks>
public abstract class ScriptOptionName
{
    private readonly string _name;

    private protected ScriptOptionName(string name) => _name = name;

    /// <summary>
    /// Implicitly converts a <see cref="ScriptOptionName"/> to a <see cref="string"/>.
    /// </summary>
    /// <param name="option">The <see cref="ScriptOptionName"/> to convert.</param>
    /// <returns><see cref="string"/></returns>
    public static implicit operator string(ScriptOptionName option) => option._name;

    /// <inheritdoc/>
    public override string ToString() => _name;
}

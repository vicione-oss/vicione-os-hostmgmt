namespace HostManagement.Scripts.ScriptOptionNames;

/// <summary>
/// Provides all option names for the set service state script.
/// </summary>
public sealed class SetServiceOptionName : ScriptOptionName
{
    private SetServiceOptionName(string value) : base(value) { }

    /// <summary>
    /// The script argument for the service name.
    /// </summary>
    public static SetServiceOptionName Service { get; } = new("--name");

    /// <summary>
    /// The script argument for the service state.
    /// </summary>
    public static SetServiceOptionName State { get; } = new("--state");
}

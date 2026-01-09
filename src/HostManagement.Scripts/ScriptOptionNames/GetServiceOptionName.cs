namespace HostManagement.Scripts.ScriptOptionNames;

/// <summary>
/// Provides all option names for the get service state script.
/// </summary>
public sealed class GetServiceOptionName : ScriptOptionName
{
    private GetServiceOptionName(string value) : base(value) { }

    /// <summary>
    /// The script argument for the service name whose status is to be determined.
    /// </summary>
    public static GetServiceOptionName Name { get; } = new("--name");
}

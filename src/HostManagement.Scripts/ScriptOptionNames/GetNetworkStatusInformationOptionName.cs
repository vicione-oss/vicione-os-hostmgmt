namespace HostManagement.Scripts.ScriptOptionNames;

/// <summary>
/// Provides all option names for the get network status information script.
/// </summary>
public sealed class GetNetworkStatusInformationOptionName : ScriptOptionName
{
    private GetNetworkStatusInformationOptionName(string value) : base(value) { }

    /// <summary>
    /// The script argument for the interface name whose status is to be determined.
    /// </summary>
    public static GetNetworkStatusInformationOptionName InterfaceName { get; } = new("--interface-name");
}

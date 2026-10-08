namespace HostManagement.Communication.CommandLine.Resources;

internal static partial class CliStrings
{
    /// <summary>
    /// Provides strings for syslog commands.
    /// </summary>
    internal static partial class Syslog
    {
        internal const string CommandDescription = "Manage syslog settings.";
        internal const string CommandName = "syslog";
        internal const string Label = "Syslog";

        internal const string ClearCommandDescription = "Clear all syslog settings.";
        internal const string DisableCommandDescription = "Disable syslog settings.";
        internal const string EnableCommandDescription = "Enable syslog settings.";
        internal const string StatusCommandDescription = "Show syslog settings.";
        internal const string SetCommandDescription = "Remove existing syslog settings and set new ones.";

        internal const string Address = "Address";
        internal const string AlreadyDisabled = "Syslog settings are already disabled.";
        internal const string AlreadyEnabled = "Syslog settings are already enabled.";
        internal const string Cleared = "Syslog settings have been cleared.";
        internal const string ConnectionRetrySeconds = "Connection retry seconds";
        internal const string Disabled = "Syslog settings have been disabled.";
        internal const string Enabled = "Syslog settings have been enabled.";
        internal const string ExcludeSyslogFacilities = "Exclude syslog facilities";
        internal const string ExcludeSyslogLevels = "Exclude syslog levels";
        internal const string LogFormat = "Log format";
        internal const string Set = "Syslog settings have been set";
        internal const string TransportProtocol = "Transport protocol";
    }
}

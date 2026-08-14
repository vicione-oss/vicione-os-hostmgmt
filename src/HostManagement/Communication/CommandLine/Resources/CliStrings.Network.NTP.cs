namespace HostManagement.Communication.CommandLine.Resources;

internal static partial class CliStrings
{
    internal static partial class Network
    {
        /// <summary>
        /// Provides strings for network NTP commands.
        /// </summary>
        internal static class NTP
        {
            internal const string CommandDescription = "Configure NTP settings.";
            internal const string CommandName = "ntp";

            internal const string AddCommandDescription = "Add NTP servers.";
            internal const string ClearCommandDescription = "Remove all NTP servers.";
            internal const string DisableCommandDescription = "Disable NTP servers.";
            internal const string EnableCommandDescription = "Enable NTP servers.";
            internal const string ListCommandDescription = "List all NTP servers.";
            internal const string ModifyCommandDescription = "Modify an existing NTP server.";
            internal const string RemoveCommandDescription = "Remove NTP servers.";
            internal const string SetCommandDescription = "Remove existing NTP servers and set new ones.";

            internal const string Added = "NTP server added";
            internal const string AddedMultiple = "NTP servers added";
            internal const string AlreadyDefined = "NTP server '{0}' is already defined.";
            internal const string AlreadyDisabled = "NTP servers are already disabled.";
            internal const string AlreadyEnabled = "NTP servers are already enabled.";
            internal const string Cleared = "NTP servers have been cleared.";
            internal const string CouldNotBeRemoved = "NTP server '{0}' could not be removed.";
            internal const string CurrentlyDisabled = "NTP servers are currently disabled.";
            internal const string Disabled = "NTP servers have been disabled.";
            internal const string Enabled = "NTP servers have been enabled.";
            internal const string FallbackServers = "Fallback NTP servers";
            internal const string Modified = "NTP server has been changed from '{0}' to '{1}'.";
            internal const string NotConfigured = "No NTP servers are configured.";
            internal const string NotFound = "NTP server '{0}' was not found.";
            internal const string Removed = "NTP server removed";
            internal const string RemovedMultiple = "NTP servers removed";
            internal const string Servers = "NTP servers";
            internal const string Set = "NTP server set";
            internal const string SetMultiple = "NTP servers set";
        }
    }
}

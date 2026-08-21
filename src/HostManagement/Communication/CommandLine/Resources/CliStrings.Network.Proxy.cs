namespace HostManagement.Communication.CommandLine.Resources;

internal static partial class CliStrings
{
    internal static partial class Network
    {
        /// <summary>
        /// Provides strings for network proxy commands.
        /// </summary>
        internal static class Proxy
        {
            internal const string CommandDescription = "Configure proxy settings.";
            internal const string CommandName = "proxy";

            internal const string DisableCommandDescription = "Disable the {0} proxy.";
            internal const string EnableCommandDescription = "Enable the {0} proxy.";
            internal const string SetCommandDescription = "Set {0} proxy settings.";
            internal const string StatusCommandDescription = "Show {0} proxy settings.";
            internal const string TypeCommandDescription = "Configure {0} proxy settings.";

            internal const string AlreadyDisabled = "{0} proxy is already disabled.";
            internal const string AlreadyEnabled = "{0} proxy is already enabled.";
            internal const string Configured = "{0} proxy has been configured.";
            internal const string Disabled = "{0} proxy has been disabled.";
            internal const string Enabled = "{0} proxy has been enabled.";
            internal const string NotConfigured = "{0} proxy is not configured.";

            /// <summary>
            /// Provides strings for network proxy no-proxy commands.
            /// </summary>
            internal static class NoProxy
            {
                internal const string CommandDescription = "Configure the list of ignored URLs by the proxy.";
                internal const string CommandName = "no-proxy";
                internal const string Label = "No-proxy list";

                internal const string AddCommandDescription = "Add the specified items to the no-proxy list.";
                internal const string ClearCommandDescription = "Remove all entries from the no-proxy list.";
                internal const string DisableCommandDescription = "Disable the no-proxy list.";
                internal const string EnableCommandDescription = "Enable the no-proxy list.";
                internal const string ListCommandDescription = "List all entries of the no-proxy list.";
                internal const string RemoveCommandDescription = "Remove the specified entry from the no-proxy list.";
                internal const string SetCommandDescription = "Remove existing items from the no-proxy list and set new ones.";

                internal const string Added = "Following item was added to the list of ignored URLs by the proxy";
                internal const string AddedMultiple = "Following items were added to the list of ignored URLs by the proxy";
                internal const string AlreadyDefined = "No-proxy item '{0}' is already defined.";
                internal const string AlreadyDisabled = "No-proxy list is already disabled.";
                internal const string AlreadyEnabled = "No-proxy list is already enabled.";
                internal const string Cleared = "List of ignored URLs by the proxy has been cleared.";
                internal const string CouldNotBeRemoved = "No-proxy item '{0}' could not be removed.";
                internal const string CurrentlyDisabled = "No-proxy list is currently disabled.";
                internal const string Disabled = "No-proxy list has been disabled.";
                internal const string Enabled = "No-proxy list has been enabled.";
                internal const string NotConfigured = "No-proxy list is not configured.";
                internal const string NotFound = "No-proxy item '{0}' was not found.";
                internal const string Removed = "Following item was removed from the list of ignored URLs by the proxy";
                internal const string RemovedMultiple = "Following items were removed from the list of ignored URLs by the proxy";
                internal const string Set = "Following item was set for the list of ignored URLs by the proxy";
                internal const string SetMultiple = "Following items were set for the list of ignored URLs by the proxy";
            }
        }
    }
}

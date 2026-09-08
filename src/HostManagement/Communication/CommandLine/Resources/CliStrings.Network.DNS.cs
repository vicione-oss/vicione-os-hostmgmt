namespace HostManagement.Communication.CommandLine.Resources;

internal static partial class CliStrings
{
    internal static partial class Network
    {
        /// <summary>
        /// Provides strings for network DNS commands.
        /// </summary>
        internal static class DNS
        {
            internal const string CommandDescription = "Configure DNS settings.";
            internal const string CommandName = "dns";
            internal const string Label = "DNS";

            /// <summary>
            /// Provides strings for DNS hostname commands.
            /// </summary>
            internal static class Hostname
            {
                internal const string CommandDescription =
                    "Show the current hostname. An optional new name can be specified to change the hostname.";

                internal const string CommandName = "hostname";
                internal const string Label = "Hostname";

                internal const string Set = "Hostname set to '{0}'.";
            }

            /// <summary>
            /// Provides strings for multicast DNS commands.
            /// </summary>
            internal static class Multicast
            {
                internal const string CommandDescription = "Configure multicast DNS (mDNS).";
                internal const string CommandName = "mdns";

                internal const string DisableCommandDescription = "Disable mDNS.";
                internal const string EnableCommandDescription = "Enable mDNS.";
                internal const string StatusCommandDescription = "Show mDNS enabled status.";

                internal const string AlreadyDisabled = "Multicast DNS is already disabled.";
                internal const string AlreadyEnabled = "Multicast DNS is already enabled.";
                internal const string Disabled = "Multicast DNS has been disabled.";
                internal const string Enabled = "Multicast DNS has been enabled.";
                internal const string IsDisabled = "Multicast DNS is disabled.";
                internal const string IsEnabled = "Multicast DNS is enabled.";
            }

            /// <summary>
            /// Provides strings for DNS static host commands.
            /// </summary>
            internal static class StaticHosts
            {
                internal const string CommandDescription = "Configure static hosts. Specified hostnames are mapped to IP addresses.";
                internal const string CommandName = "static-hosts";
                internal const string Label = "Static Hosts";

                internal const string AddCommandDescription = "Add a new static host entry.";
                internal const string ClearCommandDescription = "Clear all static host entries.";
                internal const string DisableCommandDescription = "Disable static hosts.";
                internal const string EnableCommandDescription = "Enable static hosts.";
                internal const string ListCommandDescription = "List all configured static hosts.";
                internal const string RemoveCommandDescription = "Remove a static host entry.";

                internal const string Added = "Static host mapping added";
                internal const string AlreadyDefined = "Static host mapping '{0}  {1}' is already defined.";
                internal const string AlreadyDisabled = "Static hosts are already disabled.";
                internal const string AlreadyEnabled = "Static hosts are already enabled.";
                internal const string Cleared = "Static hosts have been cleared.";
                internal const string CouldNotBeRemoved = "Static host mapping '{0}  {1}' could not be removed.";
                internal const string CurrentlyDisabled = "Static hosts are currently disabled.";
                internal const string Disabled = "Static hosts have been disabled.";
                internal const string Enabled = "Static hosts have been enabled.";
                internal const string NotConfigured = "No static hosts are configured.";
                internal const string NotFound = "Static host mapping '{0}  {1}' was not found.";
                internal const string Removed = "Static host mapping removed";
            }

            /// <summary>
            /// Provides strings for DNS name server commands.
            /// </summary>
            internal static class NameServers
            {
                internal const string CommandDescription = "Configure DNS name servers.";
                internal const string CommandName = "name-servers";
                internal const string Label = "Name Servers";

                internal const string AddCommandDescription = "Add new DNS name servers.";
                internal const string ClearCommandDescription = "Clear all DNS name servers.";
                internal const string DisableCommandDescription = "Disable DNS name servers.";
                internal const string EnableCommandDescription = "Enable DNS name servers.";
                internal const string ListCommandDescription = "List all DNS name servers.";
                internal const string ModifyCommandDescription = "Modify an existing DNS name server.";
                internal const string RemoveCommandDescription = "Remove existing DNS name servers.";
                internal const string SetCommandDescription = "Remove existing DNS name servers and set new ones.";

                internal const string Added = "Name server added";
                internal const string AddedMultiple = "Name servers added";
                internal const string AlreadyDefined = "Name server '{0}' is already defined.";
                internal const string AlreadyDisabled = "Name servers are already disabled.";
                internal const string AlreadyEnabled = "Name servers are already enabled.";
                internal const string Cleared = "Name servers have been cleared.";
                internal const string CouldNotBeRemoved = "Name server '{0}' could not be removed.";
                internal const string CurrentlyDisabled = "Name servers are currently disabled.";
                internal const string Disabled = "Name servers have been disabled.";
                internal const string Enabled = "Name servers have been enabled.";
                internal const string Modified = "Name server has been changed from '{0}' to '{1}'.";
                internal const string NotConfigured = "No name servers are configured.";
                internal const string NotFound = "Name server '{0}' was not found.";
                internal const string Removed = "Name server removed";
                internal const string RemovedMultiple = "Name servers removed";
                internal const string Set = "Name server set";
                internal const string SetMultiple = "Name servers set";
            }

            /// <summary>
            /// Provides strings for primary DNS suffix commands.
            /// </summary>
            internal static class Suffix
            {
                internal const string CommandDescription = "Configure the primary DNS suffix.";
                internal const string CommandName = "dns-suffix";
                internal const string Label = "Primary DNS Suffix";

                internal const string DisableCommandDescription = "Disable the primary DNS Suffix.";
                internal const string EnableCommandDescription = "Enable the primary DNS Suffix.";
                internal const string RemoveCommandDescription = "Remove the primary DNS suffix.";
                internal const string SetCommandDescription = "Remove the existing primary DNS suffix and set a new one.";
                internal const string StatusCommandDescription = "Show primary DNS suffix status.";

                internal const string AlreadyDisabled = "Primary DNS suffix is already disabled.";
                internal const string AlreadyEnabled = "Primary DNS suffix is already enabled.";
                internal const string CurrentlyDisabled = "Primary DNS suffix is currently disabled.";
                internal const string Disabled = "Primary DNS suffix has been disabled.";
                internal const string Enabled = "Primary DNS suffix has been enabled.";
                internal const string NotConfigured = "No primary DNS suffix is configured.";
                internal const string Removed = "Primary DNS suffix removed";
                internal const string Set = "Primary DNS suffix set";
            }

            /// <summary>
            /// Provides strings for DNS search domain commands.
            /// </summary>
            internal static class SearchDomains
            {
                internal const string CommandDescription = "Configure DNS search domains.";
                internal const string CommandName = "search-domains";
                internal const string Label = "Search Domains";

                internal const string AddCommandDescription = "Add new DNS search domains.";
                internal const string ClearCommandDescription = "Clear all DNS search domains.";
                internal const string DisableCommandDescription = "Disable DNS search domains.";
                internal const string EnableCommandDescription = "Enable DNS search domains.";
                internal const string ListCommandDescription = "List all DNS search domains.";
                internal const string ModifyCommandDescription = "Modify an existing DNS search domain.";
                internal const string RemoveCommandDescription = "Remove existing DNS search domains.";
                internal const string SetCommandDescription = "Remove existing DNS search domains and set new ones.";

                internal const string Added = "Search domain added";
                internal const string AddedMultiple = "Search domains added";
                internal const string AlreadyDefined = "Search domain '{0}' is already defined.";
                internal const string AlreadyDisabled = "Search domains are already disabled.";
                internal const string AlreadyEnabled = "Search domains are already enabled.";
                internal const string Cleared = "Search domains have been cleared.";
                internal const string CouldNotBeRemoved = "Search domain '{0}' could not be removed.";
                internal const string CurrentlyDisabled = "Search domains are currently disabled.";
                internal const string Disabled = "Search domains have been disabled.";
                internal const string Enabled = "Search domains have been enabled.";
                internal const string Modified = "Search domain has been changed from '{0}' to '{1}'.";
                internal const string NotConfigured = "No search domains are configured.";
                internal const string NotFound = "Search domain '{0}' was not found.";
                internal const string Removed = "Search domain removed";
                internal const string RemovedMultiple = "Search domains removed";
                internal const string Set = "Search domain set";
                internal const string SetMultiple = "Search domains set";
            }
        }
    }
}

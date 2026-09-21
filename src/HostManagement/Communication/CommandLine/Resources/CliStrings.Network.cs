namespace HostManagement.Communication.CommandLine.Resources;

internal static partial class CliStrings
{
    /// <summary>
    /// Provides strings for network commands.
    /// </summary>
    internal static partial class Network
    {
        internal const string CommandDescription = "Manage network settings.";
        internal const string CommandName = "network";

        internal const string StatusCommandDescription = "Get network status information of the specified interface.";

        internal const string DefaultGatewayAvailable = "Default gateway available";
        internal const string DefaultRouteConfigured = "Default route configured";
        internal const string DNSFunctional = "DNS functional";
        internal const string InterfaceState = "Interface state";
        internal const string InternetAvailable = "Internet available";

        /// <summary>
        /// Provides strings for network interface commands.
        /// </summary>
        internal static class Interface
        {
            internal const string CommandDescription = "Configure a specified network interface.";
            internal const string CommandName = "interface";

            internal const string DisableCommandDescription = "Disable the specified network interface.";
            internal const string EnableCommandDescription = "Enable the specified network interface.";
            internal const string ListInterfacesCommandDescription = "List all network interfaces with their configuration.";
            internal const string ListInterfacesCommandName = "list-interfaces";
            internal const string ResetCommandDescription = "Reset the specified network interface to its default configuration.";
            internal const string StatusCommandDescription = "Show enabled status of the specified network interface.";

            internal const string AlreadyDisabled = "Network interface '{0}' is already disabled.";
            internal const string AlreadyEnabled = "Network interface '{0}' is already enabled.";
            internal const string CommonInfo = "Common Information";
            internal const string Disabled = "Network interface '{0}' has been disabled.";
            internal const string Enabled = "Network interface '{0}' has been enabled.";
            internal const string Interfaces = "Interfaces";
            internal const string InterfacesNotFound = "Network interfaces were not found.";
            internal const string IsDisabled = "Network interface '{0}' is disabled.";
            internal const string IsEnabled = "Network interface '{0}' is enabled.";
            internal const string NotFound = "Network interface '{0}' was not found.";
            internal const string ResetSucceeded = "Network interface '{0}' has been reset to default configuration.";
            internal const string Unknown = "Unknown";

            /// <summary>
            /// Provides strings for interface MAC commands.
            /// </summary>
            internal static class MAC
            {
                internal const string CommandDescription = "Configure MAC address settings of a specified network interface.";
                internal const string CommandName = "mac";

                internal const string DisableCommandDescription = "Disable the user-defined MAC address for the specified network interface. The original MAC address will be used again.";
                internal const string EnableCommandDescription = "Enable the user-defined MAC address for the specified network interface.";
                internal const string ResetCommandDescription = "Reset the MAC address of the specified network interface to the original MAC address.";
                internal const string SetCommandDescription = "Remove the existing user-defined MAC address and set a new one.";
                internal const string StatusCommandDescription = "Show MAC address settings of the specified network interface.";

                internal const string AlreadyDefined = "User-defined MAC address '{0}' is already defined.";
                internal const string AlreadyDisabled = "User-defined MAC address '{0}' is already disabled for network interface '{1}'.";
                internal const string AlreadyEnabled = "User-defined MAC address '{0}' is already enabled for network interface '{1}'.";
                internal const string Disabled = "User-defined MAC address '{0}' has been disabled for network interface '{1}'. The original MAC address will be used again.";
                internal const string Enabled = "User-defined MAC address '{0}' has been enabled for network interface '{1}'.";

                internal const string IdenticalToOriginalAddress = "User-defined MAC address '{0}' is identical to the original MAC address.";
                internal const string OriginalAddress = "Original MAC address";
                internal const string UserDefinedAddress = "User-defined MAC address";


                internal const string InvalidLength = "User-defined MAC address '{0}' has invalid length.";
                internal const string InvalidFormat = "User-defined MAC address '{0}' has invalid format.";
                internal const string ResetSucceeded = "MAC address of network interface '{0}' has been reset to its default '{1}'.";
                internal const string Set = "User-defined MAC address '{0}' has been set to '{1}' for network interface '{2}'.";
            }

            /// <summary>
            /// Provides strings for interface VLAN commands.
            /// </summary>
            internal static class VLAN
            {
                internal const string CommandDescription = "Configure VLAN settings of a specified network interface.";
                internal const string CommandName = "vlan";
                internal const string Label = "VLAN";

                internal const string DisableCommandDescription = "Disable VLAN for the specified network interface.";
                internal const string EnableCommandDescription = "Enable VLAN for the specified network interface (assuming a valid VLAN-ID is configured).";
                internal const string ListCommandDescription = "List the existing VLAN configuration of all available network interfaces.";
                internal const string RemoveCommandDescription = "Remove the existing VLAN ID and disable VLAN of the specified network interface.";
                internal const string SetCommandDescription = "Remove the existing VLAN ID and set a new one to the specified network interface.";

                internal const string AlreadyDisabled = "VLAN is already disabled for network interface '{0}'.";
                internal const string AlreadyEnabled = "VLAN is already enabled for network interface '{0}'.";
                internal const string Disabled = "VLAN has been disabled for network interface '{0}'.";
                internal const string Enabled = "VLAN has been enabled for network interface '{0}'.";
                internal const string ID = "VLAN ID";
                internal const string IDAdded = "VLAN ID '{0}' has been added to network interface '{1}'.";
                internal const string IDRemoved = "VLAN ID '{0}' has been removed from network interface '{1}'.";
                internal const string IDSet = "VLAN ID '{0}' has been set to '{1}' for network interface '{2}'.";
                internal const string NotConfigured = "VLAN is not configured for network interface '{0}'.";
            }

            /// <summary>
            /// Provides strings for interface IP commands.
            /// </summary>
            internal static class IP
            {
                internal const string CommandDescription = "Configure IP address and netmask of a specified network interface.";
                internal const string CommandName = "ip";

                internal const string Added = "IP address '{0} ({1})' has been added to network interface '{2}'.";
                internal const string AlreadyDefined = "IP address '{0} ({1})' is already defined.";
                internal const string IPAddress = "IP address";
                internal const string IPAddresses = "IP addresses";
                internal const string SubnetMask = "Subnet mask";
                internal const string Modified = "IP address and/or netmask of network interface '{0}' have been changed from '{1} ({2})' to '{3} ({4})'.";
                internal const string NoneSpecified = "No IP addresses specified for network interface '{0}'.";
                internal const string NotFound = "IP address '{0}' of network interface '{1}' was not found.";
                internal const string Removed = "IP address '{0}' has been removed from network interface '{1}'.";
            }

            /// <summary>
            /// Provides strings for interface IPv4 commands.
            /// </summary>
            internal static class IPv4
            {
                internal const string CommandDescription = "Configure IPv4 settings of a specified network interface.";
                internal const string CommandName = "ipv4";
                internal const string Label = "IPv4";

                internal const string AddCommandDescription = "Add a new IPv4 address and netmask to the specified network interface.";
                internal const string ModifyCommandDescription = "Modify the IPv4 address and/or netmask of the specified network interface.";
                internal const string RemoveCommandDescription = "Remove the IPv4 address of the specified network interface.";

                internal const string AddressMustHave4Octets = "{0} must have exactly 4 octets, but found {1}.";
            }

            /// <summary>
            /// Provides strings for interface DHCP commands.
            /// </summary>
            internal static class DHCP
            {
                internal const string CommandDescription = "Configure DHCP of a specified network interface.";
                internal const string CommandName = "dhcp";
                internal const string Label = "DHCP";

                internal const string DisableCommandDescription = "Disable DHCP of the specified network interface.";
                internal const string EnableCommandDescription = "Enable DHCP of the specified network interface.";
                internal const string ListCommandDescription = "List the DHCP enabled state of all available network interfaces.";
                internal const string RenewLeaseCommandDescription = "Renew DHCP lease of the specified network interface.";
                internal const string RenewLeaseCommandName = "renew-lease";

                internal const string AlreadyDisabled = "DHCP is already disabled for '{0}'.";
                internal const string AlreadyEnabled = "DHCP is already enabled for '{0}'.";
                internal const string Disabled = "DHCP has been disabled for '{0}'.";
                internal const string Enabled = "DHCP has been enabled for '{0}'.";
                internal const string IsDisabled = "DHCP is currently disabled for '{0}'.";
                internal const string IsEnabled = "DHCP is currently enabled for '{0}'.";
                internal const string Lease = "DHCP Lease";
                internal const string LeaseExpires = "Lease expires";
                internal const string LeaseNotAvailable = "No DHCP lease information is available.";
                internal const string LeaseObtained = "Lease obtained";
                internal const string LeaseRenewed = "DHCP Lease renewed";
            }

            /// <summary>
            /// Provides strings for interface gateway commands.
            /// </summary>
            internal static class Gateway
            {
                internal const string CommandDescription = "Configure the default gateway.";
                internal const string CommandName = "gateway";
                internal const string Label = "Gateway";

                internal const string RemoveCommandDescription = "Remove the default gateway.";
                internal const string SetCommandDescription = "Remove the existing default gateway and set a new one. Only one default gateway can be specified.";

                internal const string Added = "Gateway '{0}' has been added to network interface '{1}'.";
                internal const string AlreadyDefined = "Gateway '{0}' is already defined.";
                internal const string IsNotConfigured = "A default gateway is not configured.";
                internal const string Removed = "Gateway '{0}' has been removed.";
                internal const string Set = "Gateway '{0}' has been changed to '{1}' for network interface '{2}'.";
            }
        }
    }
}

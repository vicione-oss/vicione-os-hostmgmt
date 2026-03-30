# Changelog

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Added functionality to reset network interfaces to factory default settings
- Added possibility to create a template of `SystemConfiguration.json` and validate it

### Changed

- Moved call to `get_network_interfaces.sh` from scripts to C#
  - from `get_network_interfaces_common_information.sh`
  - from `get_network_status_information.sh`
  - from `set_network_dns_settings.sh`

### Removed

- Validation: Removed obsolete method `Validate` in `SystemConfigurationValidator`
- Shared: Removed installation of ViciOne Suite via update server (was replaced with generic functionality in [1.2.0](#120---2025-12-02))
- Removed obsolete `FallbackNTPServers` from `SystemConfiguration` (separate `GetNTPFallbackInformation` request was added in [1.0.0](#100---2025-07-17))
- Removed obsolete `DHCPLease` from `SystemConfiguration` (separate `GetDHCPLeaseInformation` request was added in [1.0.0](#100---2025-07-17))
- Removed obsolete `RenewDHCPLeaseResult` (new `GetDHCPLeaseInformationResult` was added in [1.0.0](#100---2025-07-17))

### Fixed

- Fixed a bug where proxy settings were not cleared during a system reset.

## [1.3.0] - 2026-03-23

### Added

- Added functionality to get the current state of a systemd unit (via named pipe topic `GetServiceState`)
- Added optional delay in seconds to shutdown or restart of the system
- Added NuGet packages
  - `Microsoft.Testing.Extensions.CodeCoverage` version `18.5.2`
  - `xunit.v3.mtp-v2` version `3.2.2`

### Changed

- Replaced '\u0027' string in log output with actual apostrophe
- Aligned cli proxy commands with other commands, e.g. separate enable/disable commands etc.
- Moved call to `restart_system.sh` from scripts to C#
- Moved target environment handling from `PipeServer` to `RequestHandlerRegistry`
  - Improved response when request handler not supported on the platform
  - Improved response when request is missing required message content
- Made environment variables related to ViciOne Suite update optional (deprecated; new generic method via topic `InstallSignedDebianPackage` was implemented)
  - `HOSTMANAGEMENT__SUITE_REPOSITORY__ENDPOINT`
  - `HOSTMANAGEMENT__SUITE_REPOSITORY__USERNAME`
  - `HOSTMANAGEMENT__SUITE_REPOSITORY__PASSWORD`
- Moved call to `is_network_interface_available.sh` to C#
  - from `get_network_status_information.sh`
  - from `renew_dhcp_lease.sh`
  - from `set_network_interface_ipv4_settings.sh`
- Moved call to `get_network_interfaces.sh` from scripts to C# for MDNS
- Improved console logger: Only log if a message and/or an exception is provided.
- Updated `.NET Runtime` from `9.0.11` to `10.0.5`
- Updated `.NET SDK` from `9.0.307` to `10.0.201`
- Updated NuGet packages
  - `AwesomeAssertions` from `9.2.0` to `9.4.0`
  - `Microsoft.CodeAnalysis.CSharp` from `4.14.0` to `5.3.0`
  - `Microsoft.Extensions.Configuration.EnvironmentVariables` from `9.0.10` to `10.0.5`
  - `Microsoft.Extensions.Logging` from `9.0.10` to `10.0.5`
  - `Microsoft.Extensions.Logging.Abstractions` from `9.0.10` to `10.0.5`
  - `System.CommandLine` from `2.0.0` to `2.0.5`
  - `TestableIO.System.IO.Abstractions` from `22.0.16` to `22.1.0`
  - `TestableIO.System.IO.Abstractions.TestingHelpers` from `22.0.16` to `22.1.0`
  - `TestableIO.System.IO.Abstractions.Wrappers` from `22.0.16` to `22.1.0`
- Increased timeout for the Debian package install script (ExecuteInstallDebianPackageScript) to 180 seconds to prevent errors when installing larger packages.

### Removed

- Removed NuGet packages
  - `coverlet.collector`
  - `Microsoft.NET.Test.Sdk`
  - `xunit`;
  - `xunit.runner.visualstudio`

### Fixed

- Fixed cli command `NoProxyListClearCommand` not disabling the feature on clear
- Fixed `RenewDHCPLease` request not able to support new result type `GetDHCPLeaseInformationResult` (old but obsolete `RenewDHCPLeaseResult` still works)
- Fixed format of warning message when getting network status information returns warning

## 1.2.0 - 2025-12-02

### Added

- Added API parameter and system variable (`HOSTMANAGEMENT__SUITE_REPOSITORY__PACKAGE_API`) for downloading suite versions
- Added ability to add further supported target platforms
  - Added support to read and use target environment from environment variable `HOSTMANAGEMENT__TARGET_ENVIRONMENT`
  - Created the following resource folders
    - `Scripts/shared` for scripts that can be used in multiple platforms (are linked as symlinks into targets)
    - `Scripts/targets/edges-vicione` for scripts used for edgeGatewayS target
    - `Scripts/targets/debian` for scripts used on standard debian12 server installations
  - Added multi-platform support to `ResourceManager` (scripts deployment now depends on target environment)
  - Updated prepare phase of service start to use target environment
  - Updated request handler registration to use target environment
- Added functionality to install signed Debian packages

### Changed

- Updated .NET Runtime to `9.0.11`
- Updated .NET SDK to version `9.0.307`
- Updated NuGet packages
  - `Microsoft.Extensions.Configuration.EnvironmentVariables` from `9.0.7` to `9.0.10`
  - `Microsoft.Extensions.Logging` from `9.0.7` to `9.0.10`
  - `Microsoft.Extensions.Logging.Abstractions` from `9.0.7` to `9.0.10`
  - `Microsoft.NET.Test.Sdk` from  `17.14.1` to `18.0.0`
  - `TestableIO.System.IO.Abstractions` from `22.0.15` to `22.0.16`
  - `TestableIO.System.IO.Abstractions.TestingHelpers` from `22.0.15` to `22.0.16`
  - `TestableIO.System.IO.Abstractions.Wrappers` from `22.0.15` to `22.0.16`
  - `xunit.runner.visualstudio` from `3.1.3` to `3.1.5`
- Replaced NuGet package `FluentAssertions` with `AwesomeAssertions`
- Shared: Marked installation of ViciOne Suite via update server as **obsolete** (will be replaced with generic functionality to install signed Debian packages)

### Removed

- Removed default update directory for swu-update (env var `HOSTMANAGEMENT__UPDATE_DIRECTORY` must be set via environment e.g. on EdgeS to `/mnt/update`)

### Fixed

- Fixed that consumers were not able to use `Shared.Communication` NuGet package due to reference to source generator (introduced in [1.1.0](#110---2025-10-15))
- Prevent deadlocks when reading output of scripts

## 1.1.0 - 2025-10-15

### Added

- Added validators for `netmask` and `IPv4Detail`
- Added check whether hostmanagement service (started via `--daemon` flag) is already running
- Added CRC check of swu-file to update system function
- Added project `HostManagement.Shared.Communication.SourceGeneration`
  - Added source generator for list of valid topics and topics that require content
  - Added possibility to send any topic with `PipeCommunicationSample`
  - Added possibility to send JSON config with `PipeCommunicationSample`
- Added original physical address lookup feature
- Added request handling for original MAC address
- Added `shutdown system` functionality
- Added handling of special characters in proxy authentication (username and password)
- Added additional proxy authentication validation for control characters and length
- Added link-local functionality für DHCP
- CLI: Added VLAN output to `list-interfaces` command
- CLI: Added possibility to also change netmask with `ipv4 ip` command
- CLI: Added an additional Yes/No prompt to the `system shutdown` command

### Changed

- Updated .NET SDK to version `9.0.305`
- Updated NuGet packages
  - `Microsoft.Extensions.Configuration.EnvironmentVariables` from `9.0.6` to `9.0.7`
  - `Microsoft.Extensions.Logging` from `9.0.6` to `9.0.7`
  - `Microsoft.Extensions.Logging.Abstractions` from `9.0.6` to `9.0.7`
  - `TestableIO.System.IO.Abstractions` from `22.0.14` to `22.0.15`
  - `TestableIO.System.IO.Abstractions.TestingHelpers` from `22.0.14` to `22.0.15`
  - `TestableIO.System.IO.Abstractions.Wrappers` from `22.0.14` to `22.0.15`
  - `xunit.runner.visualstudio` from `3.1.1` to `3.1.3`
- CLI: Corrected sorting of commands to be consistent
- Changed `NamedPipeRequestHelper` to use source generated list of topics instead of manually maintained list
- Satisfies CA1848: Use the LoggerMessage delegates
- CLI: Updated commands `list-interfaces` and `network` > `mac` to include original MAC address
- CLI: Changed display of proxy password to a partially masked string when using `list` command (e.g.: P******d)
- Improved error message for an invalid proxy server
- Shared: Marked `DHCPLease` as **obsolete** (will be replaced with `GetDHCPLeaseInformationResult` request and removed from config)
- Increased timeout to download ViciOne Suite from 5 minutes to 60 minutes

### Removed

- Removed `IStringWrapper`

### Fixed

- Fixed wrong log message when `NetworkStatusInformation` requested via CLI was null
- Improved `restart_system.sh` script
  - Use "systemctl" instead of compatibility wrapper "shutdown" (see shutdown --help for further details)
  - Return the error code if restart could not be scheduled.
- Fixed possibility to have multiple colons in the proxy server string by adding additional validation
- CLI: Fixed `list-interfaces` names-only output (unnecessary colon was shown)
- CLI: Fixed indentation level of global DNS output in `list-interfaces` command

## 1.0.0 - 2025-07-17

### Added

- Added percent encoding for IPv6 link-local no-proxy items with zone id
  - '%' is replaced with '%25' for `/etc/environment` when setting configuration
  - '%25' is replaced back to '%' when getting configuration (consumer must handle percent encoding for URIs by itself)
- Added validation for empty list of no-proxy items, name servers, search domains or static hosts
- Added information text in front of message for do-not-proxy list command
- Added unit tests for `EnsureLFLineEndings` of `HostManagement.Scripts.Extensions`
- Added missing unit tests for script option names
- Added unit test for `OperationStatusExtensions` of `HostManagement.Shared.Communication`
- Added missing result type data for `SourceGeneratedTypeSerializerTests`
- Added unit tests for `Utilities` of `HostManagement.Shared.Communication`
- Added unit tests for `SystemConfiguration` Services property GetHashCode and Equals method
- Added meta data for DNS and IP configuration in netplan yaml file
- Added separate `GetNTPFallbackInformation` request
- Added separate `GetDHCPLeaseInformation` request
- Added gateway validation

### Changed

- Updated NuGet packages
  - `Microsoft.Extensions.Configuration.EnvironmentVariables` from `9.0.1` to `9.0.6`
  - `Microsoft.Extensions.Logging` from `9.0.1` to `9.0.6`
  - `Microsoft.Extensions.Logging.Abstractions` from `9.0.1` to `9.0.6`
  - `Microsoft.NET.Test.Sdk` from `17.12.0` to `17.14.1`
  - `TestableIO.System.IO.Abstractions` from `21.3.1` to `22.0.14`
  - `TestableIO.System.IO.Abstractions.TestingHelpers` from `21.3.1` to `22.0.14`
  - `TestableIO.System.IO.Abstractions.Wrappers` from `21.3.3` to `22.0.14`
  - `xunit.runner.visualstudio` from `3.0.1` to `3.1.1`
- Changed `ScriptRunner` to a singleton
  - Removed need for `HostManagement.Configuration` (resource deploy directory is set during `Initialize`)
  - Added some unit tests
- Updates product name and descriptions of Shared Assemblies (NuGet packages)
- Simplified unit tests for script option names
- Improved `SourceGeneratedTypeSerializerTests`
  - Reduced code duplication
  - Uses better sample data
- Altered overloads of `SystemConfigurationSerializer` methods
- Renamed `Services` to `SystemdUnits` in `HostManagement.Configuration`
- Improved descriptions of "service" in `HostManagement.Shared`
- Replaced "service" with "systemd unit" in handler and scripts
- Moved all "service (systemd unit)" logic into a single service
- Adapted CLI to use "systemd unit" instead of "service" where applicable
- Adapted IPv4, DNS and NTP handling to remember deactivated settings again
- Changed setting hostname directly (not via 'set_network_dns_settings.sh' anymore)
- Changed getting fallback NTP servers
- Marked `FallbackNTPServers` of contract as **obsolete** (will be replaced with `GetNTPFallbackInformation` request)
- Changed CLI to use new `GetNTPFallbackInformation` request
- Changed `RenewDHCPLeaseManager` to more general service (now `DHCPLeaseService`)
- Marked `RenewDHCPLeaseResult` of communication contract as **obsolete** (will be replaced with `GetDHCPLeaseInformationResult` request)
- Added own namespace for network interfaces and NTP validators
- Moved validation results into per-feature resource files
- Changed relevant validators from internal to public
  - Moved validation logic to own validators where applicable

### Fixed

- Fixed zone identifier handling for no-proxy items
- Fixed getting string of no-proxy list
  - Fixed check for zone identifier returning collection of bool instead of string
  - Fixed no-proxy item having ':' specified twice if port detected
  - Fixed throwing `ArgumentOutOfRangeException` when no-proxy item has zone identifier ('%')
- Fixed enabling empty list via CLI (no-proxy list, name servers, search domains, static hosts)
- Fixed the path to the DHCP lease information
- Fixed a bug in `SystemConfiguration` GetHashCode method
- Fixed file modes (file permission) for `.markdownlint.json` and `set_service_state.sh`
- Fixed sorting of `CommandLineStrings`
- Fixed unhandled exception "Broken pipe" when the other end of the pipe has gone away

## 0.11.0 - 2025-06-16

### Added

- Added `update system` functionality to update the system via swu file (accessible through CLI and named pipe topic)

### Changed

- `get_service_state.sh` and `set_service_state.sh`
  - Improved robustness
  - Enabled a restart even if the service has not already been started
- Updated .NET SDK to version `9.0.301`
- Improved `ResourceManager`
  - No dependency for `FileSystemWrapper` anymore
  - No dependency for `HostManagement.Configuration` anymore
  - Deploys only missing scripts on service start

### Fixed

- Fixed reading DHCP lease information
- Fixed missing part of error message on revert config, if revert failed

## 0.10.0 - 2025-05-27

### Added

- Added a 'set-config' flag (file) to determine after first boot (HM started via `--prepare-system-configuration` flag) if the service (HM started via `--daemon` flag) needs to set the configuration
- Added version information to system configuration
- Added `restart system` functionality
- Added `reset system` functionality
- Added internal validation for service configuration (relevant for persistence of config in `SystemConfiguration.json`)
  - Only **non-wildcard** services from `HOSTMANAGEMENT__CONFIGURABLE_SERVICES` are valid
- Added `ServiceControlResult` (replaced `RestartServiceResult`)
- Added start/stop service functionality for all services in `HOSTMANAGEMENT__CONFIGURABLE_SERVICES`
- Added `update system` functionality to update the system via swu file (for internal use only)

### Changed

- Updated .NET SDK to version `9.0.300`
- Bypassed setting service states at boot time (HM started via `--prepare-system-configuration`)
- Changed MAC address validation to allow "unconfigured" but disallows setting it
- Disallowed setting proxy detail that is "unconfigured"
- Removed read-only information from system configuration before saving it in `SystemConfiguration.json`
- Changed persisting of system configuration only on user triggered requests
- Restricts restart of service to defined services in `HOSTMANAGEMENT__CONFIGURABLE_SERVICES`
- Changed simple status information of network interface to real status information

### Removed

- Removed loading `SystemConfiguration.json` when get config called
- Removed persisting config after get config called
- Removed functionality to create `SystemConfiguration.json` on startup (with configurable delay in seconds)
- Removed now obsolete `--apply-now`-option to `set_service.sh`
- Removed need to implement `PrepareConfiguration` if not needed
- Removed `RestartServiceResult` (uses now `ServiceControlResult`)

## 0.9.0 - 2025-04-30

### Added

- Added functionality to set permissions and ownership of the named pipe via environment variables
  - `HOSTMANAGEMENT__PIPE__USER`
  - `HOSTMANAGEMENT__PIPE__GROUP`
  - `HOSTMANAGEMENT__PIPE__CHMOD`
- Added validation for search domains and combined hostname and DNS suffix length
- Added validation for no-proxy list
- Added unit test for `ScriptOptions`
- Added extension method for `SystemConfiguration` to get interface by name
- Added `CliMessageHelper` which provides console messages that are used multiple times (e.g. network interface not found)
- Added functionality to set network interface ignore list (blacklist) via environment variable `HOSTMANAGEMENT__IGNORED_NETWORK_INTERFACES`
- Added new CLI output handling
  - Added various output types like list, key-value or table output
  - Added consistent formatting (configurable in `CliOutputBase.Configuration`)
- Added some CLI messages where they were missing (e.g. feature already enabled/disabled or feature not configured)
- Added `--apply-now`-option to `set_service.sh`, allowing the desired service state to be applied immediately or after restart of the system
- Added flag and functionality to skip apply (`--prepare-system-configuration`)
- Added handling of a 'DefaultSystemConfiguration.json' (in the context of prepare configuration)
- Added functionality to bypass script validation functions via environment variable `HOSTMANAGEMENT__BYPASS_VALIDATION_CHECKS`

### Changed

- Changed default permissions of named pipe (from `rwxr-xr-x` to `rw-rw-r--`)
- Changed default ownership of named pipe (from `root:root` to `vicione-suite:vicione-suite`)
- Updated .NET SDK to version `9.0.203`
- Improved configuration and handling of services enable, disable and restart functionality
- Improved validation for hostname, static hosts, name servers and DNS suffix
- Refactored `ScriptOptions`
- Improved CLI messages for various commands
- Improved `SystemConfigurationManager` and renamed it to `SystemConfigurationService`
- Improved service handling
  - Service state is `Enabled` if `is-active` and `is-enabled` to prevent misinterpretation if service is active but not enabled
  - Changing services states will check and apply `is-active` and `is-enabled` separately
- Changed CLI output to use new CLI output handling
  - Changed `ConsoleLogger` to be able to use long log level strings for console output (e.g. "WARNING" instead of "WRN"; short log levels are still the default)
  - Changed coloring of warnings/errors etc. to only color the word and not the whole message
  - Moved CLI strings from command handler methods to resources
- Switched from load/apply configuration to get/set configuration
- Improved Get/Set result creation with new static methods
- Improved the validation of VLAN settings so that duplicate IDs across network interfaces are detected as errors
- Improved script code
- Separated set configuration into prepare/apply steps
- Refactored and improved code of system config adapters
- Refactored `Program.cs` (CLI logic now in `CommandLineRunner` and service logic in `ServiceRunner`)
- Refactored `ApplicationPaths` and `EnvironmentVariables`
- Bypassed setting MAC address at boot time

### Removed

- Removed obsolete `CommandLineStringBuilder` (functionality now in `CliComplexOutputBase` and `CliOutputBase`)
- Removed obsolete `CliMessageHelper`
- Removed obsolete method `ApplyLastSavedConfiguration`

### Fixed

- Fixed dns information getter script
- Fixed logging in `FileSystemWrapper`
- Fixed not saving json on warning in `SystemConfigurationService`
- Fixed outdated xml comments in `SystemConfigurationService`
- Fixed network base information
- Fixed empty NTP result list
- Fixed validation error for VLAN ID 0 when VLAN disabled

## 0.8.0 - 2025-03-03

### Added

- Added validation for DNS suffix
- Added IP address validation logic to cli commands (ip's must have 4 octets)
- Added MAC address interface settings
- Added functionality to create `SystemConfiguration.json` on startup (with configurable delay in seconds)
- Added MAC address CLI command, handling and validation
- Added functionality to renew DHCP lease of a specified network interface
- Added revert config functionality if apply config failed
- Added `restart Linux services` functionality
- Added `RespectNullableAnnotations` attribute to `Shared.Communication.SourceGenerationContext` to prevent setting null to non-nullable properties

### Changed

- Improved `ScriptResponse`
- Updated NuGet packages
  - `FluentAssertions` from `6.12.2` to `7.1.0` (fixed due to license changes)
  - `xunit` from `2.9.2` to `2.9.3`
  - `xunit.runner.visualstudio` from `2.8.2` to `3.0.1`
  - `coverlet.collector` from `6.0.2` to `6.0.4`
  - `Microsoft.Extensions.Configuration.EnvironmentVariables` from `9.0.0` to `9.0.1`
  - `Microsoft.Extensions.Configuration.EnvironmentVariables.Abstractions` from `9.0.0` to `9.0.1`
  - `TestableIO.System.IO.Abstractions` from `21.1.3` to `21.3.1`
  - `TestableIO.System.IO.Abstractions.Wrappers` from `21.1.3` to `21.3.1`
- Improved error messages in scripts showing exit code of called 3rd-party tools.
- Improved DNS suffix commands and messages
- Improved DHCP CLI commands
- Uses `OperationResult` base type and implemented ordering
- Improved CLI commands
- Switched getting network status information to individual interfaces
- Updated .NET SDK to version `9.0.200`
- Improved `NetworkInterfaceCommandGroupProvider`
  - Added enable/disable commands instead of "-e" option
  - Added status command
- Improved performance for configuration getter scripts
- Improved format of `InvalidOperationException` in `SourceGeneratedTypeSerializer`
- Improved JSON compatibility using `JsonConstructor` instead of `JsonRequired` attribute (lists can now be left out in JSON and are still initialized in code)

### Removed

- Removed redundant `JsonRequired` attribute for name and state in `ServiceDetail`

### Fixed

- Fixed DHCP enabled behaviour
  - Removed validation for cases where DHCP gets enabled and manually configured IPv4 details were previously configured (IPv4 details are still only applied when DHCP is disabled)
  - Removed clearing of IPv4 details when DHCP gets enabled (preserving manually configured IPv4 details in `SystemConfiguration.json`)
- Fixed typo in CLI VLAN disabled message
- Fixed validation not preventing conflicts between IP addresses specified for network interfaces and the default gateway
- Fixed missing interface name in CLI interface status command
- Fixed not using correct type name for `InvalidOperationException` in `SourceGeneratedTypeSerializer`

## 0.7.0 - 2025-01-31

### Added

- Added check for invalid environment variables at startup
- Added `NameServersEnabled` functionality
- Added separate script to restart `systemd-timesyncd` (NTP service)
- Added new overloads to `ScriptOptions` to work with `IPAddress`
- Added `static hosts` functionality
- Added `get network status information` functionality
- Added `VLAN` interface settings functionality
- Added `enable and disable Linux services` functionality
- Added `MulticastDNSEnabled` functionality

### Changed

- Changed ViciOne Suite package version comparison to use new type `DebianPackageVersion` instead of .NET `Version` type
- Suppresses warning on install ViciOne Suite via apt
- Changed DHCP lease information to use `DateTimeOffset` instead of `DateTime`
- Updated .NET SDK to version `9.0.102`
- Aligned CLI "items" command order with other commands
  - resulting order: 'add', 'remove', 'set', 'clear', 'modify', 'list'
  - 'enable' and 'disable' are sorted before those commands
- Moved `ConfigurationHelper` to namespace `Helper` and renamed it to `NamedPipeRequestHelper`
- Renamed `CommonStrings` to `CommandLineStrings`
- Improved error message if getting ViciOne versions failed
- Improved persistence of network interfaces state
- Improved script 'wait_for_service.sh'
- Improved install scripts for .deb-files
- Changed applying network settings from restarting `systemd-networkd` service to calling `netplan apply` instead
- Improved result message format when a script has warnings or errors
- Moved `GetSequenceHashCode` method to new `IEnumerableExtensions` class
- Changed `GetHashCode` method to support more than 8 properties
- Changed scripts for getting DNS settings and DHCP lease information
- Refactored FQDN validation
- Improved hostname validation
- Improved gateway CLI commands to allow setting default gateway to a specific interface
- Hides `install-pre-release` CLI command

### Fixed

- Fixed errors occurring if virtual network interfaces configured on target
  - Network interface state detection failed for states different than `UP`, `LOWERLAYERUP`, `DOWN` and `LOWERLAYERDOWN`
  - Physical network address detection failed
  - DHCP enabled state detection failed
- Fixed non-optional constructor parameters set to null during deserialization, when not present or set to null in json
- Fixed equality comparison of `IPv4Settings` contract
- Fixed gateway validation logic to allow a single default gateway even if some interfaces have DHCP enabled

## 0.6.0 - 2024-12-13

### Added

- Added compatibility warning to netplan yaml file
- Regard console lifetime when running as service

### Changed

- Simplified `ScriptRunner` by removing the ultimate timeout
- Improved exception handling and logging for named pipe requests
- Improved exception logging for file system operations
- Updated target framework to .NET 9
- Changed assembly name from `HostManagement` to `hostmanagement`
- Improved script response exception handling

### Fixed

- Disabled auto apply of system configuration on start up (temporary hotfix)
- Disabled call to persist network interface state via a service-file (temporary hotfix)

## 0.5.0 - 2024-11-18

### Added

- Added DHCP lease obtained and expires output to CLI
- Added `CommandLineStringBuilder` to help format CLI output
- Added extended DHCP lease information
- Added `DateTimeConverter` for `DateTime` JSON deserialization
- Added possibility to extract CLI help
- Added possibility to apply a `SystemConfiguration.json` on service startup

### Changed

- Refactored CLI `ListNetworkInterfacesCommandGroupProvider`
- Added required option `--daemon` to start as service (if not given, CLI will print help)
- Updated .NET SDK to version `8.0.404`
- Adjusted pre-release version handling to new format (`0.21.0~1428429` instead of `0.21.0.1428429`)
- Changed `DateTimeConverter` to use UTC time
- Adjusted pre-release version handling of installed suite version

### Removed

- Removed option (`--bypass`) to bypass named pipe communication by direct API call between CLI and daemon

### Fixed

- Fixed a problem with virtual network names
- Fixed a problem with multiple DNS addresses in a DHCP lease
- Fixed a problem with multiple domain names in a DHCP lease
- Fixed a problem with time stamps in a DHCP lease

## 0.4.0 - 2024-10-24

### Added

- Added NTP scripts
- Added colored output to `ConsoleLogger`
- Added ability to read environment variables such as log level
- Added deployment of scripts to a directory when service starts
- Added redeployment of scripts when named pipe request is received
- Added missing NoProxyList functionality
- Added `CommandHelper` to construct new commands enabling usage of simpler Handlers (e.g. no redundant null checks necessary anymore)
- Added manually configured DNS settings to `list-interfaces` CLI output
- Added application paths to `HostManagement.Configuration` project
- Added version information class `VersionInfo`
- Added script for loading version file from the ViciOne Suite repository
- Added script to get system architecture
- Added script to get current ViciOne Suite version
- Added environment variables for ViciOne Suite repository
- Added functionality to get list of compatible ViciOne Suite versions to install
- Added new project `HostManagement.Common` for common logic (helper, utils, abstractions etc.)
- Added script for loading .deb-file from the ViciOne Suite repository
- Added script for installing .deb-file
- Added functionality to install suite version
  - Added environment variable for suite download directory (`HOSTMANAGEMENT__SUITE_DOWNLOAD_DIRECTORY`)
  - Added topic and result type to install suite versions
  - Added CLI command to install a suite version
- Added search domains enabled handling
- Added scripts for hostname handling
- Added functionality to get/set hostname
- Added possibility to accept multiple CLI arguments for one option without repeating the option name

### Changed

- Changed `ConsoleEx` to use `ConsoleLogger`
- Use log level defined in environment variable `LOGGING__LOGLEVEL__DEFAULT` or default log level `Warning` if not set
- Improved log messages of pipe send and receive
- Adjusts script usage to be called from a directory
- Replaced `ConsoleEx` call with direct usage of `ConsoleLogger` in `ConfigurationHelper`
- Improved CLI description of set commands to indicate that all items are cleared before setting new ones
- Improved formatting of log messages
- Enabled setting configuration of SystemConfiguration.json path defined in environment variable `HOSTMANAGEMENT_SYSTEMCONFIGURATION_PATH`
- Moved application configuration (environment variables) to own project (`HostManagement.Configuration`)
- Moved resource deploy directory (see `ResourceManager`) to environment variables
- Updated .NET SDK to version `8.0.403`
- Changed `ConfigurationResult` to more general result type
- Moved `ScriptOptions` to more general namespace
- Moved `IPUtils` to `HostManagement.Common` (now `IPUtilities`)
- Moved `IOHelper` to `HostManagement.Common` (now `FileSystemWrapper`)
- Enhanced DNS scripts for using search domains
- Enabled search domains functionality
- Enhanced dns scripts to get/set DNS-Suffix
- Enhanced dns CLI and handler functionality

### Fixed

- Adds workaround for errors "Already in a connected state", "Broken pipe" and halt of the client
- Fixes black log messages text if terminal has black background
- Fixed wrong type for exception message in `LoadSystemConfigurationRequestHandler`
- Fixed argument handling in ConfigurationResult (added check for no arguments)
- Fixed `JsonSerializer` not throwing when type mismatch or member missing or unknown
- Fixes an error that leads to missing exception messages of messages logged via the `ConsoleLogger`
- Fixed a problem with an uninstalled ViciOne Suite version which was uninstalled with apt remove (version was still available)

## 0.3.0 - 2024-08-29

### Added

- Added `ScriptOptions` overloads to use non string arguments
- Added unit tests for `ScriptResponse`
- Added named pipe communication between CLI and service

### Changed

- Moved script call to separate method and improved exception handling in `NetworkInterfacesConfigurationAdapter`
- Improved XML comments of `ScriptRunner`
- Simplified `GetValueForOption` call in `NoProxyListCommandGroupProvider`
- Updated .NET Runtime to 8.0.8 (SDK 8.0.400)

### Fixed

- Fixed unhandled exception in `LoadSystemConfigurationRequestHandler`
- Fixed IPv4 console output spacing
- Fixed IPv4 IP modify command showing incorrect message when IP already defined

## 0.2.0 - 2024-08-09

### Added

- Added functionality to get proxy settings
- Added additional proxy validation
- Added utility class for CIDR conversion
- Added DHCP lease object and handling

### Changed

- Improved proxy validation to allow unit tests
- Improved network interfaces validation to allow unit tests
- Switched to `systemd-networkd` instead of `ifupdown` (`networking`) as network manager using `netplan`
- Moved DNS settings contract parallel to proxy settings

### Fixed

- Fixed `ScriptRunner` not using overwritten timeout
- Fixed returning on warning early in `SystemConfigurationManager`

## 0.1.0 - 2024-07-25

- Initial Release
  - Implemented getting and setting of network interfaces' common information
  - Implemented getting and setting of IPv4 settings

[1.3.0]: https://gitlab.com/vicione-oss/vicione/os/hostmgmt/-/tags/v1.3.0

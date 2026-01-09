# Changelog

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.8.0] - [Unreleased]

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

## [0.7.0] - 2025-01-31

### Added

- Added check for invalid environment variables at startup
- Added `NameServersEnabled` functionality
- Added separate script to restart `systemd-timesyncd` (NTP service)
- Added new overloads to `ScriptOptions` to work with `IPAddress`
- Added `static hosts` functionality
- Added `get network status information` functionality
- Added `VLAN` interface settings functionality
- Added `enable and disable linux services` functionality
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

## [0.6.0] - 2024-12-13

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

## [0.5.0] - 2024-11-18

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

## [0.4.0] - 2024-10-24

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

## [0.3.0] - 2024-08-29

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

## [0.2.0] - 2024-08-09

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

## [0.1.0] - 2024-07-25

- Initial Release
  - Implemented getting and setting of network interfaces' common information
  - Implemented getting and setting of IPv4 settings

[0.1.0]: https://gitlab.i40.ifm-datalink.net/acx/vo-suite/hostmgmt/-/tags/v0.1.0
[0.2.0]: https://gitlab.i40.ifm-datalink.net/acx/vo-suite/hostmgmt/-/tags/v0.2.0
[0.3.0]: https://gitlab.i40.ifm-datalink.net/acx/vo-suite/hostmgmt/-/tags/v0.3.0
[0.4.0]: https://gitlab.i40.ifm-datalink.net/acx/vo-suite/hostmgmt/-/tags/v0.4.0
[0.5.0]: https://gitlab.i40.ifm-datalink.net/acx/vo-suite/hostmgmt/-/tags/v0.5.0
[0.6.0]: https://gitlab.i40.ifm-datalink.net/acx/vo-suite/hostmgmt/-/tags/v0.6.0
[0.7.0]: https://gitlab.i40.ifm-datalink.net/acx/vo-suite/hostmgmt/-/tags/v0.7.0

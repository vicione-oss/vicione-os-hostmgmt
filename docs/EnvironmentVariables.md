# Environment Variables

Below is a list of all environment variables defined for HostManagement.

> [!IMPORTANT]
> When adding, removing, or renaming environment variables, the config files in [`conf.d folder`](https://gitlab.com/vicione-oss/vicione/os/deb-packaging/-/blob/master/src/conf.d) in the `Debian packaging` repository may also need to be updated accordingly.

| Variable                                                 | Description                                                                                                                    | Default value                                        | Current value in the Linux image[^1]                            | Required                                     |
| -------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------ | ---------------------------------------------------- | --------------------------------------------------------------- | -------------------------------------------- |
| `HOSTMANAGEMENT__BYPASS_VALIDATION_CHECKS`[^2]           | If set, certain validations are skipped (e.g. during the OS startup phase when not all required information is available yet). | `""`                                                 | `true`                                                          | Yes during early OS startup; No in operation |
| `HOSTMANAGEMENT__CONFIGURABLE_SERVICES`                  | List of Linux services that can be configured by HostManagement                                                                | `""`                                                 | see [Configurable Services](#configurable-services)             | No                                           |
| `HOSTMANAGEMENT__DEFAULT_SYSTEM_CONFIGURATION_DIRECTORY` | Directory in which `DefaultSystemConfiguration.json` is located                                                                | `/etc/hostmanagement`                                | `/etc/hostmanagement`                                           | No (Yes for persistence)                     |
| `HOSTMANAGEMENT__IGNORED_NETWORK_INTERFACES`[^2]         | List of network interfaces ignored by HostManagement                                                                           | fixed to `lo,docker` in `get_network_interfaces.sh`  | `lo,ifm-mrc`                                                    | No                                           |
| `HOSTMANAGEMENT__PIPE__CHMOD`                            | Permissions of the named pipe (numeric notation)                                                                               | `664`                                                | `664`                                                           | No                                           |
| `HOSTMANAGEMENT__PIPE__GROUP`                            | Group of the named pipe                                                                                                        | `vicione-suite` (`root` if the group does not exist) | `vicione-suite`                                                 | No                                           |
| `HOSTMANAGEMENT__PIPE__USER`                             | User/owner of the named pipe                                                                                                   | `vicione-suite` (`root` if the user does not exist)  | `vicione-suite`                                                 | No                                           |
| `HOSTMANAGEMENT__PUBLIC_KEYS__[0...x]`                   | List of known public keys used to verify installation files etc.                                                               | `""`                                                 | `%%KEY%%`                                                       | No (Yes for updating the ViciOne Suite)      |
| `HOSTMANAGEMENT__RESOURCE_DEPLOY_DIRECTORY`              | Directory to which all resources (scripts) are deployed when the service starts                                                | `/var/lib/hostmanagement`                            | `/var/lib/hostmanagement`                                       | No                                           |
| `HOSTMANAGEMENT__SYSTEM_CONFIGURATION_DIRECTORY`         | Directory in which `SystemConfiguration.json` is located                                                                       | `/mnt/persistent/hostmanagement`                     | `/mnt/persistent/hostmanagement`                                | No (Yes for persistence)                     |
| `HOSTMANAGEMENT__TARGET_ENVIRONMENT`                     | Target environment/platform, e.g. `EdgeS` or `"Standard" Debian`                                                               | `unknown`                                            | depends on target environment, e.g. `edges-vicione` or `debian` | Yes                                          |
| `HOSTMANAGEMENT__UPDATE_DIRECTORY`[^2]                   | Directory in which `.swu` files must be placed for updates                                                                     | not set                                              | `/mnt/update`                                                   | No (Yes for updating the system)             |
| `LOGGING__LOGLEVEL__DEFAULT`                             | Application `LogLevel`                                                                                                         | `Warning`                                            | `Warning`                                                       | No                                           |

## Configurable Services

### Normal Services

* `ssh.service`
* `remoteconnect.service`
* `vicione-suite.service`

### Wildcard Services

* `enginehost@*.service`
* `enginehost-privileged@*.service`
* `enginehost-reset@*.service`

[^1]: The values in the Linux image differ between releases and products and may change at any time. The values shown are examples for better understanding only.
[^2]: `script only`: On startup, HostManagement checks whether all required environment variables are set. If this is not the case, an error message is logged. Variables marked as `script only` are exceptions, as they are currently used directly in shell scripts and are not known to HostManagement itself.

# DNS Validation

## Domain Label vs. FQDN (Fully Qualified Domain Name)

| &nbsp;                    | `Domain Label`                                                                 | `FQDN` = multiple `domain labels` separated by dots                                                          |
| ------------------------- | ------------------------------------------------------------------------------ | ------------------------------------------------------------------------------------------------------------ |
| **Length**                | **1 to 63 characters**                                                         | **1 to 253 characters** including separators (`.`)                                                           |
| **Case Sensitivity**      | **Not case-sensitive**, but usually lowercase                                  | **Not case-sensitive**, but usually lowercase                                                                |
| **Allowed Characters**    | `a-z`, `A-Z`, `0-9`, `-`                                                       | `a-z`, `A-Z`, `0-9`, `-` and `.` (dots separate `domain labels`)                                             |
| **Characters (start)**    | Must not start with `-`                                                        | Each `domain label` must not start with `-`                                                                  |
| **Characters (end)**      | Must not end with `-`                                                          | Each `domain label` must not end with `-`                                                                    |
| **Numbers**               | Must not consist of digits only                                                | Must not consist of digits only<br><br>More than one `domain label` consisting only of digits is not allowed |
| **Reserved Names**        | `Domain labels` such as `local`, `test` or `example` (e.g. in `printer.local`) | `FQDNs` such as `localhost.localdomain` and certain top-level domains (e.g.: `.example`)                     |

## DNS Suffix

**One or more** `domain labels` separated by **dots**, e.g..: `local`, `google.com`, `mail.company.com`.

### Primary DNS Suffix vs. Search Domains

| Feature               | Primary DNS Suffix                                                                                  | Search Domains                                                                                                                                                    |
| --------------------- | --------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Purpose**           | Defines the `FQDN` of the local computer                                                            | Resolves unqualified hostnames                                                                                                                                    |
| **Scope**             | One suffix per computer                                                                             | Multiple suffixes in search order                                                                                                                                 |
| **Example**           | **Hostname**: `device42`<br/>**Primary suffix**: `company.com`<br/>**FQDN**: `device42.company.com` | **Resolving:** `server1`<br/>**Search domains**: `backup.company.com`, `mail.company.com`<br/>**FQDNs**: `server1.backup.company.com`, `server1.mail.company.com` |
| **Configuration**     | System (hostname-based)                                                                             | Network adapter settings                                                                                                                                          |
| **Applies to**        | Local identity of the computer                                                                      | All DNS queries on the system                                                                                                                                     |

## Hostname

A `hostname` can be a `domain label` or an `FQDN`.

### Debian Hostname

Uses **strict validation** for **1 to 63 characters** (see `hostnamectl`).

### Hostnames in `/etc/hosts`

- **1 to 253 characters**
- **max. 63 characters** for a **non-FQDN** hostname
- **max. 253 characters** for an **FQDN**
- `_` are allowed but usually **not recommended** for compatibility reasons
- Technically can consist of digits only, but can be ambiguous and cause issues on certain systems
- Reserved names like `localhost` and `broadcasthost` are typically present in `/etc/hosts` and used for specific purposes

## Static Hosts

`Static hosts` are pairs of `IP address` and [Hostname](#hostname).

## NTP-Server

`NTP servers` can be defined as IPv4, IPv6, or as an [FQDN](#domain-label-vs-fqdn-fully-qualified-domain-name).

## RFC Documents

The most important **RFC documents** that define `domain labels`, their structure, and DNS naming conventions are:

| RFC                                                                                   | Description                                                                                                                         |
| ------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------- |
| [RFC 1035]<br/>**Domain Names - Implementation and Specification**<br/>1987           | This is the core DNS specification. It defines the structure of domain names, label length restrictions, and general DNS behaviour. |
| [RFC 1123]<br/>**Requirements for Internet Hosts - Application and Support**<br/>1989 | Extends **RFC 1035** by clarifying rules for hostnames, including the prohibition of underscores `_` in DNS labels.                 |
| [RFC 2181]<br/>**Clarifications to the DNS Specification**<br/>1997                   | Clarifies restrictions regarding DNS label length and the FQDN.                                                                     |
| [RFC 2606]<br/>**Reserved Top-Level DNS Names**<br/>1999                              | Reserves TLDs such as `.example`, `.test`, `.invalid`, and `.localhost`.                                                            |
| [RFC 6761]<br/>**Special-Use Domain Names**<br/>2013                                  | Defines "special-use" domain names such as `.local`, `.onion`, and `.home`.                                                         |
| [RFC 5890]<br/>**Internationalized Domain Names for Applications (IDNA)**<br/>2010    | Introduces support for internationalized domain names (IDNs).                                                                       |

[RFC 1035]: https://www.rfc-editor.org/rfc/rfc1035.html
[RFC 1123]: https://www.rfc-editor.org/rfc/rfc1123.html
[RFC 2181]: https://www.rfc-editor.org/rfc/rfc2181
[RFC 2606]: https://www.rfc-editor.org/rfc/rfc2606.html
[RFC 6761]: https://www.rfc-editor.org/rfc/rfc6761.html
[RFC 5890]: https://www.rfc-editor.org/rfc/rfc5890.html

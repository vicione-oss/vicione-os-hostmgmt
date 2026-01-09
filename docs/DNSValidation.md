# DNS Validation

## Domain Label vs. FQDN (Fully Qualified Domain Name)

| &nbsp;                    | `Domain-Label`                                                                | `FQDN` = mehrere `Domain-Labels` separiert durch Punkte                                                |
| ------------------------- | ----------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------ |
| **Länge**                 | **1 bis 63 Zeichen**                                                          | **1 bis 253 Zeichen** inklusive Trennzeichen (`.`)                                                     |
| **Groß-/Kleinschreibung** | **Wird nicht beachtet** aber üblicherweise klein                              | **Wird nicht beachtet** aber üblicherweise klein                                                       |
| **Erlaubte Zeichen**      | `a-z`, `A-Z`, `0-9`, `-`                                                      | `a-z`, `A-Z`, `0-9`, `-` und `.` (Punkte separieren `Domain-Labels`)                                   |
| **Zeichen (Beginn)**      | Darf nicht beginnen mit `-`                                                   | Jedes `Domain-Label` darf nicht beginnen mit `-`                                                       |
| **Zeichen (Ende)**        | Darf nicht enden mit `-`                                                      | Jedes `Domain-Label` darf nicht enden mit `-`                                                          |
| **Zahlen**                | Darf nicht nur aus Zahlen bestehen                                            | Darf nicht nur aus Zahlen bestehen<br><br>Mehr als ein `Domain-Label` nur aus Zahlen ist nicht erlaubt |
| **Reservierte Namen**     | `Domain-Labels` wie `local`, `test` oder `example` (z. B. in `printer.local`) | `FQDNs` wie `localhost.localdomain` und bestimmte top-level Domains (z. B.: `.example`)                |

## DNS Suffix

**Ein oder mehrere** `Domain-Labels` separiert durch **Punkte**, z. B.: `local`, `google.com`, `mail.company.com`.

### Primärer DNS Suffix vs. Suchdomains

| Feature               | Primärer DNS Suffix                                                                                  | Suchdomains                                                                                                                                                       |
| --------------------- | ---------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Zweck**             | Definiert den `FQDN` des lokalen Computers                                                           | Löst unqualifizierte Hostnamen auf                                                                                                                                |
| **Anwendungsbereich** | Ein Suffix pro Computer                                                                              | Mehrere Suffixe in Suchreihenfolge                                                                                                                                |
| **Beispiel**          | **Hostname**: `device42`<br/>**Primärer Suffix**: `company.com`<br/>**FQDN**: `device42.company.com` | **Auflösen von:** `server1`<br/>**Suchdomains**: `backup.company.com`, `mail.company.com`<br/>**FQDNs**: `server1.backup.company.com`, `server1.mail.company.com` |
| **Konfigurationsort** | System (Hostname-basiert)                                                                            | Netzwerkadapter-Einstellungen                                                                                                                                     |
| **Gilt für**          | Lokale Identität des Computers                                                                       | Alle DNS-Anfragen auf dem System                                                                                                                                  |

## Hostname

Ein `Hostname` kann ein `Domain-Label` oder ein `FQDN` sein.

### Debian Hostname

Nutzt **strikte Validierung** auf **1 bis 63 Zeichen** (Siehe `hostnamectl`).

### Hostnamen in `/etc/hosts`

- **1 bis 253 Zeichen**
- **max. 63 Zeichen** wenn **nicht-FQDN** Hostname
- **max. 253 Zeichen** wenn **FQDN**
- `_` sind erlaubt aber üblicherweise aus Kompatibilitätsgründen **nicht empfohlen**
- Kann technisch gesehen nur aus Ziffern bestehen, kann aber mehrdeutig sein und in bestimmten Systemen zu Problemen führen
- Reservierte Namen wie `localhost` und `broadcasthost` sind typischerweise in `/etc/hosts` vorhanden und werden für bestimmte Zwecke genutzt

## Statische Hosts

`Statische Hosts` sind Paare aus `IP-Adresse` and [Hostname](#hostname).

## NTP-Server

`NTP-Server` können als IPv4, IPv6 oder als [FQDN](#domain-label-vs-fqdn-fully-qualified-domain-name) definiert werden.

## RFC-Dokumente

Die wichtigsten **RFC-Dokumente**, welche `Domain-Labels`, ihre Struktur und DNS Namenskonventionen definieren, sind:

| RFC                                                                                   | Beschreibung                                                                                                                                           |
| ------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------ |
| [RFC 1035]<br/>**Domain Names - Implementation and Specification**<br/>1987           | Dies ist die zentrale DNS-Spezifikation. Sie definiert die Struktur von Domänennamen, Längenbeschränkungen der Label und das allgemeine DNS-Verhalten. |
| [RFC 1123]<br/>**Requirements for Internet Hosts - Application and Support**<br/>1989 | Erweitert **RFC 1035** durch Klarstellung der Regeln für Hostnamen, einschließlich des Verbots von Unterstrichen `_` in DNS-Bezeichnungen.             |
| [RFC 2181]<br/>**Clarifications to the DNS Specification**<br/>1997                   | Einschränkungen hinsichtlich der DNS-Labellänge und des FQDN werden erläutert.                                                                         |
| [RFC 2606]<br/>**Reserved Top-Level DNS Names**<br/>1999                              | Reservierung von TLDs wie `.example`, `.test`, `.invalid` und `.localhost`.                                                                            |
| [RFC 6761]<br/>**Special-Use Domain Names**<br/>2013                                  | "Special-use" Domainnamen wie `.local`, `.onion` und `.home` werden definiert.                                                                         |
| [RFC 5890]<br/>**Internationalized Domain Names for Applications (IDNA)**<br/>2010    | Führt Unterstützung für internationalisierte Domänennamen (IDNs) ein.                                                                                  |

[RFC 1035]: https://www.rfc-editor.org/rfc/rfc1035.html
[RFC 1123]: https://www.rfc-editor.org/rfc/rfc1123.html
[RFC 2181]: https://www.rfc-editor.org/rfc/rfc2181
[RFC 2606]: https://www.rfc-editor.org/rfc/rfc2606.html
[RFC 6761]: https://www.rfc-editor.org/rfc/rfc6761.html
[RFC 5890]: https://www.rfc-editor.org/rfc/rfc5890.html

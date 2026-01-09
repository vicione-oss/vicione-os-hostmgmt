# Konzept

Diese Datei enthält die konzeptionellen Ideen und grundlegende Designentscheidungen des HostManagement.

## Unterstützte Szenarien

### Szenario 1/1: Neues Gerät, keine persistierten Daten (Produktion)

* Beschreibung
  * Image wird auf neues Gerät geflasht
  * Gerät enthält **keine** persistierten Daten
* Ausgangszustand
  * **Keine** `SystemConfiguration.json` vorhanden
  * `DefaultSystemConfiguration.json` vorhanden
* Ablauf bei Systemstart
  * siehe `Ablauf Systemstart Linux und HostManagement`

### Szenario 1/2: Neues Gerät, keine persistierten Daten (Produktion)

* Beschreibung
  * Image wird auf neues Gerät geflasht
  * Gerät enthält **keine** persistierten Daten
* Ausgangszustand
  * **Keine** `SystemConfiguration.json` vorhanden
  * **Keine** `DefaultSystemConfiguration.json` vorhanden
* Ablauf bei Systemstart
  * siehe `Ablauf Systemstart Linux und HostManagement`

### Szenario 2: Bestandsgerät, persistierte Daten (Update des Images im Feld, oder Restore)

* Beschreibung
  * Image wird auf benutztes Gerät geflasht
  * Gerät enthält persistierte Daten
* Ausgangszustand
  * `SystemConfiguration.json` vorhanden
* Ablauf bei Systemstart
  * siehe `Ablauf Systemstart Linux und HostManagement`

### Szenario 3: Neustart, persistierte Daten (Neustart im Feld)

* Beschreibung
  * Gerät wurde neu gestartet
  * Gerät enthält persistierte Daten
* Ausgangszustand
  * `SystemConfiguration.json` vorhanden
* Ablauf bei Systemstart
  * siehe `Ablauf Systemstart Linux und HostManagement`

### Szenario 4: Einstellungen werden während der Laufzeit geändert (Betrieb)

* Beschreibung
  * Einstellungen werden durch Benutzer geändert
  * Gerät enthält persistierte Daten
* Ausgangszustand
  * `SystemConfiguration.json` vorhanden
* Ablauf
  * siehe `Ablauf Systemstart Linux und HostManagement`

## Ablauf Systemstart und Betrieb von Linux und HostManagement

* System startet Bootvorgang
* Vor Starten des Netzwerkdienstes (`network-pre.target`) wird HM mit `--prepare-system-configuration`-Parameter ausgeführt
* HM sucht nach `DefaultSystemConfiguration.json` und `SystemConfiguration.json` und erstellt Gesamtkonfiguration (`SystemConfiguration.json` überschreibt Werte `DefaultSystemConfiguration.json`)
* HM schreibt zu ändernde Einstellungen in das System
  * Angewendet werden nur Werte die im Code entsprechend markiert sind (Bspw. `early-boot`)
  * HM setzt einen flag, der HM im Daemon-Mode mitteilt, dass Konfiguration angewendet werden soll
  * (Änderungen werden nicht angewendet -> kein Apply)
* System führt Bootvorgang fort (Hinweis: Hier werden die Einstellungen (setzen sich aus `SystemConfiguration.json` und `DefaultSystemConfiguration.json` zusammen) tatsächlich angewendet, da Linux alle Dienste startet.)
* HM mit `--daemon`-Parameter wird ausgeführt
* HM prüft, ob flag gesetzt ist: Wenn ja werden alle Einstellungen angewendet.
* HM wartet auf Anfragen
* HM wendet angefragte Änderungen während der Laufzeit direkt an (ggf. Neustart des Systems) und schreibt die Änderungen in die `SystemConfiguration.json` (neuanlegen, wenn nicht vorhanden und wenn sich Werte von `DefaultSystemConfiguration.json` unterscheiden)

### Hinweise

* `--prepare-system-configuration`-Parameter
  * Schreiben ohne Anwenden der `SystemConfiguration.json` (zum Beispiel **kein** `netplan apply` ausführen)
  * In dieser Phase ist kein Ausführen der get-Skripte möglich, da das System noch nicht vollständig hochgefahren ist. -> Alle Werte aus den `DefaultSystemConfiguration.json` und `SystemConfiguration.json` müssen als gültig angesehen werden.
* `--daemon`-Parameter
  * Es ist kein Anwenden der `SystemConfiguration.json` notwendig -> Warten auf Anfragen
* Ist `SystemConfiguration.json` fehlerhaft oder nicht vorhanden, darf das HM nicht abstürzen. Wenn `SystemConfiguration.json` nicht lesbar wird `DefaultSystemConfiguration.json` geladen. `SystemConfiguration.json` bleibt unverändert im System. Ändert ein Nutzer die Einstellungen wird die fehlerhafte `SystemConfiguration.json` überschrieben.

## Aufbau und Anwendung `DefaultSystemConfiguration.json` und `SystemConfiguration.json`

* `DefaultSystemConfiguration.json` enthält den Default-Zustand des Systems.
* `SystemConfiguration.json` enthält den Soll-Zustand des Systems.
* Werte die nicht gesetzt sind, sollen durch das HM nicht geändert werden.
* Wird ein Rücksetzen des Systems auf Default-Einstellungen durchgeführt, bleiben Einstellungen, die nicht in der `DefaultSystemConfiguration.json` vorkommen, unverändert erhalten.
* Die Datenobjekte zum Senden/Empfangen über die NamedPipe enthalten einstellbare Konfigurationseinstellungen (hostname, IP-Adresse, usw.) und unveränderliche Ist-Werte (lease-Informationen, usw.). Die Konfigurationsdateien enthalten aber nur einstellbare Konfigurationseinstellungen (hostname, IP-Adresse, usw.)

## Ablauf Setzen und Auslesen von Konfigurationseinstellungen

### Setzen von Konfigurationseinstellungen

* Konfigurationseinstellungen von Suite, CLI, `SystemConfiguration.json` und `DefaultSystemConfiguration.json` enthalten nur die zu ändernden Einstellungen
* Alle empfangenen Einstellungen werden in die `SystemConfiguration.json` geschrieben, außer, diese sind bereits mit gleichen Werten in der `DefaultSystemConfiguration.json` vorhanden

### Lesen von Konfigurationseinstellungen

* HM bietet als topic die gesamte Systemkonfiguration (Ist-Zustand des Systems) als Objekt an -> vor Senden müssen get-Skripte aufgeführt werden
* HM bietet als topic einzelne Strukturen separat an

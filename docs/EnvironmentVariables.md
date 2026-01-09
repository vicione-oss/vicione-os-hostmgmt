# Umgebungsvariablen

Im Folgenden werden alle Umgebungsvariablen, welche für das HostManagement definiert sind, aufgelistet.

| Variable                                         | Beschreibung                                                                                  | Standardwert<br>(wenn nicht gesetzt) | Standardwert<br>(im Linux-Image)                            | Erforderlich |
| ------------------------------------------------ | --------------------------------------------------------------------------------------------- | ------------------------------------ | ----------------------------------------------------------- | ------------ |
| `LOGGING__LOGLEVEL__DEFAULT`                     | Der `LogLevel` der Anwendung                                                                  | `Warning`                            | `Warning`                                                   | Nein         |
| `HOSTMANAGEMENT__RESOURCE_DEPLOY_DIRECTORY`      | Der Ordner in den alle Ressourcen (Scripte) beim Start des Dienstes deployed werden           | `/tmp/HostManagement_Resources`      | tbd                                                         | Nein         |
| `HOSTMANAGEMENT__SYSTEM_CONFIGURATION_DIRECTORY` | Der Ordner in dem die `SystemConfiguration` liegt                                             | `/tmp`                               | tbd                                                         | Nein         |
| `HOSTMANAGEMENT__SUITE_DOWNLOAD_DIRECTORY`       | Der Ordner in den das ViciOne-Suite Debian-Paket bei Installation/Update heruntergeladen wird | `/tmp`                               | tbd                                                         | Nein         |
| `HOSTMANAGEMENT__SUITE_REPOSITORY__ENDPOINT`     | Der ViciOne-Suite-Repository Endpunkt (URL)                                                   | `""`                                 | `https://nexus.nsc-gmbh.de/repository/raw-fb-server/suites` | Ja           |
| `HOSTMANAGEMENT__SUITE_REPOSITORY__USERNAME`     | Der ViciOne-Suite-Repository Username                                                         | `""`                                 | `*****`                                                     | Ja           |
| `HOSTMANAGEMENT__SUITE_REPOSITORY__PASSWORD`     | Das ViciOne-Suite-Repository Passwort                                                         | `""`                                 | `*****`                                                     | Ja           |
| `HOSTMANAGEMENT__CONFIGURABLE_SERVICES`          | Die Liste der durch das HostManagement konfigurierbaren Linux-Dienste                         | `""`                                 | `ssh,systemd-networkd,vicione-suite`                        | Nein         |

# Installieren der Toolchain

Die Installation der Toolchain zur Erstellung und Ausführung des HostManagements wird hier beispielhaft anhand der Nutzung in einer VM mit VMware beschrieben. Hierfür sind zusätzliche Schritte im Vergleich zur nativen Ausführung notwendig (z. B.: Installation der VMware Tools). Die zusätzlichen Schritte werden mit (**_VM_**) gekennzeichnet.

1. **(_VM_)** **VMware Workstation** 17.6.4 oder neuer installieren
1. **Debian** 12 installieren
1. **Debian** einrichten
    1. `Automatic Suspend` in den Einstellungen **ausschalten**
    1. Benutzerkonto der `sudoers.d`-Datei hinzufügen, um sudo-Befehle ausführen zu können
        1. In den Ordner `etc` navigieren
        1. Über `Terminal` `su` für den Administrator/Root-Account ausführen
        1. Befehl `sudo visudo` ausführen (Datei `sudoers.d` wird geöffnet)
        1. Name des Benutzerkontos wie folgt hinzufügen: `Benutzerkonto ALL=(ALL:ALL) ALL`
        1. Datei speichern
    1. **(_VM_)** **VMware Tools** über Terminal installieren, falls nicht automatisch geschehen
        1. `sudo apt-get install open-vm-tools`
        1. `sudo apt-get install open-vm-tools-desktop`
    1. **(_VM_)** **SharedFolder** in VMware einrichten (z. B.: "Shared")
    1. **(_VM_)** Ordner in Debian **permanent mounten** (z. B.: "Shared" von Host nach "/home/USER/Shared" auf Gast)
        1. In den Ordner `etc` navigieren
        1. Über Terminal `sudo nano fstab` ausführen
        1. Datei ergänzen um `.host:/Shared  /home/USER/Shared  fuse.vmhgfs-fuse  defaults,allow_other,nofail  0  0`
        1. Neueinlesen der fstab über `sudo mount -a`
    1. **.NET SDK installieren** (siehe [InstallNetSdk])
    1. **Abhängigkeiten** für `dotnet publish` installieren
        1. `sudo apt-get install clang`
        1. `sudo apt-get install zlib1g-dev`
    1. **Netplan** als **Netzwerk-Manager** einrichten und mit **systemd-networkd** sowie **systemd-resolved** nutzen (Anleitung nach [DebianToNetplan])
        1. **Netplan** und **systemd-resolved** installieren: `sudo apt install netplan.io systemd-resolved -y`
        1. **Netzwerk-Dienste** aktivieren und **networking**-Dienst über unmask komplett deaktivieren

            ```bash
            sudo systemctl unmask systemd-networkd.service;
            sudo systemctl unmask systemd-resolved.service;
            sudo systemctl enable systemd-networkd.service;
            sudo systemctl mask networking;
            sudo systemctl enable systemd-resolved.service;
            ```

        1. Zu **Netplan** migrieren:
            1. `sudo ENABLE_TEST_COMMANDS=1 netplan migrate && sudo netplan try`
            1. Berechtigungen korrigieren: `sudo chmod 600 /etc/netplan/*`
                > **Hinweis:** Falls keine Einstellungen migriert wurden (da nicht vorhanden etc.), sollte eine Default-Config unter `etc/netplan/10-config.yaml` für alle Netzwerkschnittstellen angelegt werden.
                **Beispiel für eine Default-Config**

                ```yaml
                network:
                  version: 2
                  renderer: networkd
                  ethernets:
                    ens33:
                      dhcp4: true
                    ens35:
                      dhcp4: true
                ```

        1. System **neustarten**
        1. **Network Manager**, **ifupdown** und **resolvconf** deinstallieren:
            * `sudo apt purge network-manager ifupdown resolvconf -y`
            * `sudo rm -rf /etc/NetworkManager && sudo rm -rf /etc/network`
        1. Neuen Link auf `resolv.conf` erstellen: `sudo ln -sf /run/systemd/resolve/stub-resolv.conf /etc/resolv.conf`
    1. **Paketquellen** für **JFrogNuGetProxy** und **JFrogViciOne** einrichten (Name und Passwort entsprechend einsetzen)
        1. `dotnet nuget add source https://jfrog.i40.ifm-datalink.net/artifactory/api/nuget/v3/vicione-nuget.org-proxy/index.json -n JFrogNuGetProxy -u MAX.MUSTERMANN@ifm.com> -p TOKEN --store-password-in-clear-text --protocol-version 3`
        1. `dotnet nuget add source https://jfrog.i40.ifm-datalink.net/artifactory/api/nuget/v3/vicione-nuget/index.json -n JFrogViciOne -u MAX.MUSTERMANN -p PASSWORD --store-password-in-clear-text`

[DebianToNetplan]: https://pedroagrodrigues.com/posts/Debian_To_Netplan/
[InstallNetSdk]: https://learn.microsoft.com/en-us/dotnet/core/install/linux-debian#install-the-sdk

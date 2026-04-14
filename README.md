# Bugtracker V2

Der Bugtracker dient in erster Linie der raschen Fehlerdokumentation und erleichtert die Analyse und Identifikation bei Softwareproblematiken. 

Ausgeführt als Applikation mit grafischem Interface oder via CMD/Terminal sammelt der Bugtracker Logdateien der jeweiligen Software, erstellt optional Screenshots und Bildschirmaufnahmen und überträgt alle gesammelten Daten an ein konfiguriertes Ziel (Netzwerkordner, E-Mail, Webserver oder PowerShell-Skript).

Die in Visual Basic geschriebene Erstversion des Bugtrackers ist bereits erschienen und seit geraumer Zeit im Einsatz, jedoch ist hiervon nur noch die ausführbare Datei vorhanden — der Quellcode ging verloren.
Deshalb wurde beschlossen, eine neue Version des Bugtrackers zu entwickeln und neue Features umzusetzen.

## Funktionen
- Konfiguration vollständig über XML-Dateien (Applikationen, Ziele, Problemkategorien)
- Logdateien anhand konfigurierter Pfade finden und kopieren
  - Filterung nach neuester Datei (NEW), allen Dateien (ALL) oder Alter (AGE)
  - Konfigurierbare Zeilenbegrenzung (letzte N Zeilen)
  - Unterstützung für lokale, Netzwerk- und RDP-Client-Pfade
- Aufnahme aller Monitore (Screenshots, Snipping-Tool, Schrittweise-Aufnahme, Videoaufnahme)
- Mehrere Sendetypen konfigurierbar:
  - Netzwerkordner (SMB)
  - E-Mail (SMTP mit HTML-Vorlage)
  - Web-Upload (REST-API, Django-Backend)
  - PowerShell-Skript
- Konfigurationsbezug mit Priorisierung: Webserver → SMB-Share → Lokale Kopie
- Variablensubstitution in Konfigurationswerten (`%hostname%`, `%date%`, `%abbrev%`, …)
- RDP-Sitzungserkennung (`%clientname%` vs. `%hostname%`)
- Plugin-Architektur ([GUI als eigenständiges Plugin]())
- Bedienung via [GUI]() und vollständiger [CLI]()
- Windows Toast-Benachrichtigungen (siehe [Bugtracker Diagnostics UI]())
- MSI-Installer auf Basis von WiX Toolset v5 (siehe [BugtrackerSetup]())

## Versionierung

### Version 2.0 ✔
Usprüngliche Version von BugtrackerV2. Deckt Grundfunktionalität, grafische Oberfläche, sowie die ersten zusätzlichen Features:
- Grundgerüst und Programmstruktur
- XML-Konfigurationsdatei (f&uuml;r aktuelle Optionen siehe [CONFIGURATION.md](CONFIGURATION.md))
- Log-Sammlung (`fetchLogfile`) und Screenshot-Erfassung (`captureMonitors`)
- CLI-Steuerung (siehe [CONSOLE_USAGE.md](CONSOLE_USAGE.md))
- GUI-Plugin (Diagnostics UI) mit dynamischer Programmsektion (siehe [Bugtracker Diagnostics UI]())
- RDP-Sitzungsunterstützung
- Variablensubstitutionssystem

### Version 2.1.x ✔ *(aktuelle Version)*
Erweiterungen, die über den ursprünglichen Plan hinausgehen:
- Neue Sendetypen: E-Mail/SMTP, Web-Upload, PowerShell-Skript
- Konfigurationsbezug vom Webserver (HTTP)
- Logrotation und konfigurierbare Zeilenbegrenzung
- AGE/NEW/ALL Log-Suchspezifizierer
- WiX-basierter MSI-Installer (ersetzt alten Click-Once-Installer) (siehe [BugtrackerSetup]())
- Zusätzliche Aufnahmetypen (siehe [Bugtracker Diagnostics UI]()): Snipping-Tool (Multi-Monitor), Schrittweise-Aufnahme, Videoaufnahme (H.264) 
- Toast-Benachrichtigungen (siehe [Bugtracker Diagnostics UI]())

### Version 2.2
Remote-Ausführung: Bugtracks sollen ohne direkten Zugriff auf den Ziel-PCs durchgeführt werden können. 
Ein Agent soll automatisch auf den PCs gestartet werden und auf einem konfigurierten Port auf Anfragen warten.

## Kompatibilit&auml;t
Die Applikation sowie die Plugins sind als C# Apps unter Verwendung von dotnet vollst&auml;ndig mit modernen Windows Systemen kompatibel.
Linux-Kompatibilität ist zumindest für das Grundprojekt geplant, aber noch nicht umgesetzt. Eine systemunabhängige Unterstützung der Plugins ist derzeit nicht vorgesehen.

## Copyright
- [MouseKeyHook](https://github.com/gmamaladze/globalmousekeyhook) von George Mamaladze — globaler Maus-Hook für die Schrittweise-Aufnahme
- [ScreenRecorderLib](https://github.com/sskodje/ScreenRecorderLib) von Rune Holm — Bildschirmaufnahme für die Videoaufnahme
using System;
using System.Collections.Generic;
using System.Globalization;

namespace MP1.ConfigurationStudio
{
    internal enum UiLanguage
    {
        German,
        English,
        French,
        Dutch
    }

    internal static class Localization
    {
        private static readonly Dictionary<string, string> English = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            {"Allgemein", "General"},
            {"Oberfläche", "Interface"},
            {"Wiedergabe", "Playback"},
            {"Medienordner", "Media folders"},
            {"Bedienung", "Input"},
            {"Plugins", "Plugins"},
            {"System & Diagnose", "System & diagnostics"},
            {"KONFIGURATION", "CONFIGURATION"},
            {"WERKZEUGE", "TOOLS"},
            {"Konfigurationsdateien", "Configuration files"},
            {"Protokolle", "Logs"},
            {"Standard", "Standard"},
            {"Experte", "Expert"},
            {"Standard- oder Expertenansicht", "Standard or expert view"},
            {"Schrift und Oberfläche vergrößern", "Increase text and interface size"},
            {"Größe 100%", "Size 100%"},
            {"Größe 115%", "Size 115%"},
            {"Größe 125%", "Size 125%"},
            {"Größe 150%", "Size 150%"},
            {"Größe 175%", "Size 175%"},
            {"Dunkel", "Dark"},
            {"Hell", "Light"},
            {"Hell oder Dunkel", "Light or dark"},
            {"Werkzeuge", "Tools"},
            {"Hilfe ein- oder ausblenden", "Show or hide help"},
            {"Rechte Hilfespalte ein- oder ausblenden", "Show or hide the help column"},
            {"Einstellungen suchen …  Strg+F", "Search settings …  Ctrl+F"},
            {"Suche zurücksetzen", "Clear search"},
            {"Einstellung oder Bereich suchen", "Search for a setting or section"},
            {"Suchergebnisse", "Search results"},
            {"Unterbereiche", "Subsections"},
            {"HILFE & KONTEXT", "HELP & CONTEXT"},
            {"Online-Hilfe öffnen", "Open online help"},
            {"ÄNDERUNGEN", "CHANGES"},
            {"OK speichert mit der originalen MediaPortal-Prüfung. Abbrechen verwirft die Einstellungen.", "OK saves with the original MediaPortal validation. Cancel discards the settings."},
            {"&OK – Speichern", "&OK – Save"},
            {"&Abbrechen", "&Cancel"},
            {"Bereit · Keine offenen Änderungen", "Ready · No pending changes"},
            {"{0} ungespeicherte Änderungen", "{0} unsaved changes"},
            {"Keine Feldänderungen erkannt.\r\n\r\nPlugin-Dialoge können eigene Speicheraktionen haben.", "No field changes detected.\r\n\r\nPlugin dialogs may have their own save actions."},
            {"Änderungsstatus", "Change status"},
            {"Keine Treffer in dieser Ansicht", "No matches in this view"},
            {"Doppelklick oder Enter öffnet den Treffer. Für zusätzliche Bereiche oben in den Expertenmodus wechseln.", "Double-click or Enter opens a result. Switch to expert mode for additional sections."},
            {"Konfiguration  /  ", "Configuration  /  "},
            {"Seite auf Originalgröße gesetzt.", "Page reset to original size."},
            {"Gefundene Option: ", "Found option: "},
            {"Hilfe ausblenden", "Hide help"},
            {"Hilfe einblenden", "Show help"},
            {"Hoher Kontrast aktiv", "High contrast active"},
            {"Dieser Ordnerzugriff wird von der Installation nicht angeboten.", "This folder action is not provided by the installation."},
            {"Darstellung konnte nicht gespeichert werden: ", "Appearance could not be saved: "},
            {"Speichern über OK.", "Save with OK."},
            {"Tier A · voll unterstützt", "Tier A · fully supported"},
            {"Tier B · nativer Look", "Tier B · native look"},
            {"Tier C · nicht unterstützt", "Tier C · not supported"},
            {"Protokoll öffnen", "Open log"},
            {"Support-Bundle…", "Support bundle…"},
            {"Klassisch starten", "Start classic"},
            {"Protokoll: ", "Log: "},
            {"Support-Bundle erstellt:\r\n", "Support bundle created:\r\n"},
            {"Das Support-Bundle konnte nicht erstellt werden. Details stehen im Protokoll.", "The support bundle could not be created. See the log for details."},
            {"Konfigurations-Backups", "Configuration backups"},
            {"Sicherungen unter:", "Backups in:"},
            {"Keine Backups gefunden.", "No backups found."},
            {"Ordner öffnen", "Open folder"},
            {"Löschen", "Delete"},
            {"Wiederherstellen", "Restore"},
            {"Backup löschen", "Delete backup"},
            {"Backup wirklich löschen?\r\n\r\n", "Delete this backup?\r\n\r\n"},
            {"Das Backup konnte nicht gelöscht werden.\r\n\r\n", "The backup could not be deleted.\r\n\r\n"},
            {"Konfiguration gesichert:\r\n", "Configuration backed up:\r\n"},
            {"Die Konfiguration konnte nicht gesichert werden.\r\n\r\n", "The configuration could not be backed up.\r\n\r\n"},
            {"Konfiguration wiederherstellen", "Restore configuration"},
            {"Die Konfiguration konnte nicht wiederhergestellt werden.\r\n\r\n", "The configuration could not be restored.\r\n\r\n"},
            {"Klassische Oberfläche", "Classic interface"},
            {"Zurück", "Back"},
            {"Vorwärts", "Forward"},
            {"Zurück · Alt+Links", "Back · Alt+Left"},
            {"Vorwärts · Alt+Rechts", "Forward · Alt+Right"},
            {"Konfiguration sichern…", "Back up configuration…"},
            {"Konfiguration wiederherstellen…", "Restore configuration…"},
            {"Backups verwalten…", "Manage backups…"},
            {"Vor dem Speichern automatisch sichern", "Back up automatically before saving"},
            {"Aufräumen", "Clean up"},
            {"{0} Sicherungen · {1} KB", "{0} backups · {1} KB"},
            {"{0} Sicherungen gelöscht.", "{0} backups deleted."},
            {"Keine alten Vor-Sicherungen gefunden.", "No old pre-save backups found."},
            {"Seite in Originalgröße", "Reset page to original size"},
            {"Tastenkürzel…", "Keyboard shortcuts…"},
            {"Klassische Oberfläche starten", "Start classic interface"},
            {"Support-Bundle erzeugen…", "Create support bundle…"},
            {"Über MP1 Configuration Studio", "About MP1 Configuration Studio"},
            {"Konfiguration speichern", "Save configuration"},
            {"Konfiguration sichern", "Back up configuration"},
            {"ZIP-Archiv (*.zip)|*.zip", "ZIP archive (*.zip)|*.zip"},
            {"Support-Bundle speichern", "Save support bundle"},
            {"Tastenkürzel", "Keyboard shortcuts"},
            {"Ungespeicherte Änderungen", "Unsaved changes"},
            {"Studio neu starten", "Restart Studio"},
            {"Native Seiten im Original-Look", "Native pages in original look"},
            {"Neue Oberfläche (Beta)", "New interface (beta)"},
            {"Schließen", "Close"},
            {"Verstanden", "Understood"},
            {"Warnung", "Warning"},
            {"Native Konfigurationsoberfläche für MediaPortal 1.39.\r\n\r\nLiest und schreibt MediaPortal.xml direkt, erhält unbekannte Bereiche und Einträge und erstellt vor dem Speichern eine ZIP-Sicherung.\r\n\r\nLizenz: GPL-3.0-or-later\r\nMediaPortal ist ein separates Projekt und nicht Teil dieser Anwendung.", "Native configuration interface for MediaPortal 1.39.\r\n\r\nReads and writes MediaPortal.xml directly, preserves unknown sections and entries, and creates a ZIP backup before saving.\r\n\r\nLicense: GPL-3.0-or-later\r\nMediaPortal is a separate project and is not bundled with this application."},
            {"Die aktuelle MediaPortal.xml wird vorher gesichert. Danach wird Studio geschlossen und die originale Oberfläche mit der wiederhergestellten Konfiguration gestartet.\r\n\r\nNicht gespeicherte Änderungen gehen verloren. Fortfahren?", "The current MediaPortal.xml is backed up first. Studio then closes and the original interface starts with the restored configuration.\r\n\r\nUnsaved changes will be lost. Continue?"},
            {"Alte automatische Vor-Sicherungen löschen und die neuesten 10 behalten?\r\n\r\nManuelle und First-Run-Sicherungen bleiben erhalten.", "Delete old automatic pre-save backups and keep the newest 10?\r\n\r\nManual and first-run backups are kept."},
            {"Standard- und Expertenmodus", "Standard and Expert mode"},
            {"Der Standard-Modus zeigt die alltäglichen Seiten", "Standard mode keeps the everyday pages"},
            {"Die Seitenleiste startet im Standard-Modus mit Projekt, Allgemein, Wiedergabe, Medien, TV, Untertiteln und Discs.\r\n\r\nStelle den Modus in der Werkzeugleiste auf Experte, um Video-/Musikdatenbanken, Ansichten, DSP, Tastatur, Codecs, Plugins, Fernbedienung und XML-Rohdaten zu sehen.\r\n\r\nDer Modus lässt sich jederzeit wechseln; dieser Hinweis erscheint nur einmal.", "The sidebar starts in Standard mode with Project, general, playback, media, TV, subtitle and disc settings.\r\n\r\nSwitch the mode selector in the toolbar to Expert to show video/music databases, views, DSP, keyboard, codecs, plugins, remote and raw XML editors.\r\n\r\nYou can change the mode at any time; this hint appears only once."},
            {"Expertenmodus zeigen", "Show Expert mode"},
            {"Willkommen bei MP1 Configuration Studio", "Welcome to MP1 Configuration Studio"},
            {"Musikordner", "Music folder"},
            {"Ordner …", "Browse …"},
            {"Speichern", "Save"},
            {"Neu laden", "Reload"},
            {"BASS-Effekt hinzufügen", "Add BASS effect"},
            {"VST-Plugin hinzufügen", "Add VST plugin"},
            {"VST suchen", "Scan VST"},
            {"Winamp suchen", "Scan Winamp"},
            {"Winamp-Plugin hinzufügen", "Add Winamp plugin"},
            {"Testwiedergabe", "Test playback"},
            {"Ungespeicherte Musik-DSP-Änderungen verwerfen?", "Discard unsaved music DSP changes?"},
            {"DSP-Plugin-Ordner nicht gefunden:\r\n", "DSP plugin folder not found:\r\n"},
            {"Diesen Eintrag entfernen?", "Remove this entry?"},
            {"Gespeichert · Sicherung: ", "Saved · backup: "},
            {"Die Musik-DSP-Einstellungen konnten nicht gespeichert werden.\r\n\r\n", "The music DSP settings could not be saved.\r\n\r\n"},
            {"Auswählen: ", "Select "},
            {"Abbrechen", "Cancel"},
            {"DSP-Testwiedergabe", "DSP test playback"},
            {"Datei …", "Browse …"},
            {"Testdatei wählen", "Select test file"},
            {"Stopp", "Stop"},
            {"Gain: ", "Gain: "},
            {"in dieser Vorschau aktiv", "applied in this test"},
            {"Datei nicht gefunden.", "File not found."},
            {"BASS konnte nicht initialisieren (Fehler ", "BASS could not initialise (error "},
            {"Audiodatei kann nicht geöffnet werden (BASS-Fehler ", "Cannot open audio file (BASS error "},
            {"Wiedergabe mit ", "Playing with "},
            {"Dieser Filter ist auf diesem System nicht registriert.", "This filter is not registered on this system."},
            {"Für diesen Filter ist keine Eigenschaftsseite verfügbar.", "No property page is available for this filter."},
            {"Die Filterkonfiguration konnte nicht geöffnet werden.\r\n\r\n", "The filter configuration could not be opened.\r\n\r\n"},
            {"Einträge filtern …", "Filter entries …"},
            {" Einträge", " entries"},
            {"Bereich", "Context"},
            {"Aktionen filtern", "Filter actions"},
            {"Aktionen filtern …", "Filter actions …"},
            {"Aktion hinzufügen", "Add action"},
            {"Fenster hinzufügen", "Add window"},
            {"Fenster löschen", "Delete window"},
            {"Ungespeicherte Tastenbelegung verwerfen?", "Discard unsaved key mapping changes?"},
            {"Datei nicht gefunden: ", "File not found: "},
            {"Global", "Global"},
            {"Fenster", "Window"},
            {"Fenster hinzugefügt · ungespeichert", "Window added · unsaved"},
            {"Nur Fenster-Bereiche können gelöscht werden; Global ist erforderlich.", "Only window contexts can be deleted; Global is required."},
            {"Fenster „", "Delete the window “"},
            {"“ und alle Zuordnungen löschen?", "” and all its mappings?"},
            {"Fenster gelöscht · ungespeichert", "Window deleted · unsaved"},
            {"Keine Aktionen in diesem Bereich.", "No actions in this context."},
            {" Aktionen", " actions"},
            {"  ·  ungespeichert", "  ·  unsaved"},
            {"(ohne Beschreibung)", "(no description)"},
            {"(leer)", "(empty)"},
            {"Leer", "Clear"},
            {"Aktion hinzugefügt · Konflikt aufgelöst", "Action added · key conflict resolved"},
            {"Aktion hinzugefügt · ungespeichert", "Action added · unsaved"},
            {"Diese Aktion löschen?", "Delete this action?"},
            {"Aktion gelöscht · ungespeichert", "Action deleted · unsaved"},
            {"Tastenkonflikt unverändert gelassen.", "Key conflict left unchanged."},
            {"Taste ", "Key "},
            {" zugewiesen · Konflikt aufgelöst", " assigned · conflict resolved"},
            {" zugewiesen · ungespeichert", " assigned · unsaved"},
            {"Die Taste ", "The key "},
            {" ist bereits belegt durch:\r\n\r\n", " is already used by:\r\n\r\n"},
            {"\r\n\r\nTaste bei diesen Aktionen entfernen und hier zuweisen?", "\r\n\r\nRemove the key from those actions and assign it here?"},
            {"ungespeichert", "unsaved"},
            {"Die Tastenbelegung konnte nicht gespeichert werden.\r\n\r\n", "The key mapping could not be saved.\r\n\r\n"},
            {"Fenster-ID", "Window ID"},
            {"Beschreibung", "Description"},
            {"Die Fenster-ID muss eine positive Zahl sein.", "The window ID must be a positive number."},
            {"Bitte eine Beschreibung eingeben.", "Please enter a description."},
            {"Aktions-ID", "Action ID"},
            {"Taste", "Key"},
            {"Aufnehmen …", "Capture …"},
            {"Die Aktions-ID muss eine nicht negative Zahl sein.", "The action ID must be a non-negative number."},
            {"Taste drücken", "Press a key"},
            {"Neue Taste für diese Aktion drücken.", "Press the new key for this action."},
            {"Aktuell: ", "Current: "},
            {"keymap.xml speichert nur einzelne Tasten. Strg-/Alt-Kombinationen sind nicht Teil der nativen MediaPortal-Tastenbelegung; bitte die Taste ohne Strg oder Alt drücken.", "keymap.xml stores single keys only. Ctrl/Alt combinations are not part of MediaPortal's native keymap; press the key without Ctrl or Alt."},
            {"Plugin", "Plugin"},
            {"Konfigurieren", "Configure"},
            {"Aktualisieren", "Refresh"},
            {"Klassische Konfiguration", "Classic configuration"},
            {"Plugin-Fenster behalten ihre eigene Oberfläche. Das Laden kann einen Moment dauern; Fehler bleiben isoliert und können erneut versucht oder über die klassische Konfiguration geöffnet werden.", "Plugin windows use the plugin's own UI. Loading a plugin can take a moment; failures are isolated and can be retried or opened through the classic configuration."},
            {"Plugin-Liste wird gelesen …", "Reading plugin list …"},
            {"Kein Plugin mit Konfigurationsfenster gefunden.", "No plugin with a configuration window found."},
            {" konfigurierbare Plugins", " configurable plugins"},
            {"Geöffnet: ", "Opened: "},
            {"Plugins werden gelesen …", "Reading plugins …"},
            {"Plugins filtern", "Filter plugins"},
            {"Plugins filtern…", "Filter plugins…"},
            {"Aktivieren / deaktivieren", "Enable / disable"},
            {"Im Home-Menü", "In Home"},
            {"In My Plugins", "In Plugins"},
            {"Plugins suchen und installieren…", "Browse and install plugins…"},
            {"Die Plugin-Erfassung hat zu lange gedauert. Klicke auf Aktualisieren.", "The plugin scan timed out. Click Refresh to try again."},
            {"Plugin-Erfassung fehlgeschlagen: ", "Plugin scan failed: "},
            {"Keine Plugins gefunden in ", "No plugins found in "},
            {" Plugins · ", " plugins · "},
            {" aktiviert · Rechtsklick auf ein Plugin für Schnellaktionen", " enabled · right-click a tile for quick actions"},
            {"Rechtsklick für Schnellaktionen", "Right-click for quick actions"},
            {"Autor: ", "Author: "},
            {"Fenster-Plugins", "Window plugins"},
            {"Externe Player", "External players"},
            {"Prozess-Plugins", "Process plugins"},
            {"Weitere Plugins", "Other plugins"},
            {"Deaktivieren", "Disable"},
            {"Aktivieren", "Enable"},
            {"Aus Home entfernen", "Remove from Home"},
            {"Aus My Plugins entfernen", "Remove from Plugins"},
            {"konfigurierbar", "configurable"},
            {"nicht deaktivierbar", "cannot be disabled"},
            {"MediaPortal ist ein freies Open-Source-Projekt und verwandelt deinen PC in ein vollwertiges Media Center (HTPC). Sieh und nimm Fernsehen auf, spiele Videos, DVDs und Blu-rays ab, höre Musik und Radio, betrachte Bilder und erweitere alles mit Plugins und Skins.", "MediaPortal is a free open source project that turns your computer into a full media center (HTPC). Watch and record TV, play videos, DVDs and Blu-rays, listen to music and radio, view pictures, and extend everything with plugins and skins."},
            {"Projekt-Links", "Project links"},
            {"Homepage", "Homepage"},
            {"Forum", "Forum"},
            {"Online-Dokumentation", "Online documentation"},
            {"MediaPortal auf GitHub", "MediaPortal on GitHub"},
            {"Studio auf GitHub", "Studio on GitHub"},
            {"Über: ", "About: "},
            {"MediaPortal unterstützen\r\nSpenden helfen dem MediaPortal-Projekt bei Hosting, Entwicklung und Infrastruktur. Die offizielle Spendenseite liegt im MediaPortal-Forum.", "Support MediaPortal\r\nDonations help the MediaPortal project with hosting, development and infrastructure. The official donation page is hosted in the MediaPortal forum."},
            {"Über PayPal spenden", "Donate via PayPal"},
            {"XML-Bereich", "XML section"},
            {"Bereiche filtern", "Filter sections"},
            {"Bereiche filtern …", "Filter sections …"},
            {"Eintrag hinzufügen", "Add entry"},
            {"  × entfernen (Bestätigung) · ↺ auf Standard · farbig = weicht vom Standard ab", "  × remove (confirmation) · ↺ reset to default · highlighted = differs from default"},
            {"Für diesen Eintrag ist kein MediaPortal-Standardwert bekannt.", "No MediaPortal default is known for this entry."},
            {"Auf MediaPortal-Standard zurücksetzen: ", "Reset to MediaPortal default: "},
            {"Zuletzt gespeicherten Wert wiederherstellen: ", "Restore last saved value: "},
            {"Diesen Eintrag aus MediaPortal.xml entfernen", "Remove this entry from MediaPortal.xml"},
            {"Dieser Eintrag existiert bereits. Überschreiben?", "This entry already exists. Overwrite it?"},
            {"Name", "Name"},
            {"Wert", "Value"},
            {"Bitte einen Namen eingeben.", "Please enter a name."},
            {"Ordner", "Folder"},
            {"Scannen", "Scan"},
            {"Ordner hinzufügen…", "Add folder…"},
            {"Bearbeiten", "Edit"},
            {"Entfernen", "Remove"},
            {"Ja", "Yes"},
            {"Nein", "No"},
            {"Diesen Ordner entfernen?", "Remove this folder?"},
            {"Diesen Ordner scannen", "Scan this folder"},
            {"Medienordner hinzufügen", "Add media folder"},
            {"Medienordner bearbeiten", "Edit media folder"},
            {"Bitte einen Ordner auswählen.", "Please select a folder."},
            {"MP1 Configuration Studio · Neue Oberfläche", "MP1 Configuration Studio · New interface"},
            {"Neue Oberfläche · MediaPortal 1.39", "New interface · MediaPortal 1.39"},
            {"Schriftgröße", "Text size"},
            {"Einstellungen durchsuchen", "Search settings"},
            {"Ansichtsmodus", "View mode"},
            {"Farbschema", "Theme"},
            {"Klassisch / Plugins", "Classic / plugins"},
            {"Studio 1.9-Oberfläche", "Studio 1.9 interface"},
            {"Bereit", "Ready"},
            {"Sicherung", "Backup"},
            {"Weitere Einstellungen", "More settings"},
            {"Keine Einträge in diesem Bereich gefunden.", "No entries found in this section."},
            {"Keine Einträge in der Konfiguration gefunden.", "No entries found in the configuration."},
            {"Ordner auswählen", "Select a folder"},
            {"Standard: ", "Default: "},
            {"Eine oder mehrere Dateien konnten nicht gespeichert werden.", "One or more files could not be saved."},
            {"Dateien gespeichert", "Files saved"},
            {"Keine Änderungen zu speichern", "No changes to save"},
            {"Die Konfiguration konnte nicht gespeichert werden.\r\n\r\n", "The configuration could not be saved.\r\n\r\n"},
            {"Update-Prüfung läuft …", "Checking for updates …"},
            {"Update-Prüfung fehlgeschlagen:\r\n", "Update check failed:\r\n"},
            {"Sie verwenden die neueste Version (", "You are using the latest version ("},
            {"Update verfügbar: ", "Update available: "},
            {"Version ", "Version "},
            {" ist verfügbar. Release-Seite öffnen?", " is available. Open the release page?"},
            {"Sicherung erstellt · ", "Backup created · "},
            {"Die Sicherung konnte nicht erstellt werden.\r\n\r\n", "The backup could not be created.\r\n\r\n"},
            {"Keine ausstehenden Änderungen", "No pending changes"},
            {"Jetzt sichern", "Back up now"},
            {"Sicherungen verwalten…", "Manage backups…"},
            {"Über", "About"},
            {"Support-Bundle erstellt: ", "Support bundle created: "},
            {"Das Support-Bundle konnte nicht erstellt werden.\r\n\r\n", "The support bundle could not be created.\r\n\r\n"},
            {"Keine Treffer", "No matches"},
            {"Suche: ", "Search: "},
            {"Videos", "Videos"},
            {"Musik", "Music"},
            {"Weitere Bereiche", "More areas"},
            {"Einstellungen durchsuchen…", "Search settings…"},
            {"MPTray mit Windows starten", "Start MPTray with Windows"},
            {"MPTray-Autostart aktualisiert.", "MPTray autostart updated."},
            {"Klassische Prüfungen öffnen", "Open classic checks"},
            {"Der MPTray-Autostart ist eine Benutzer-Registry-Einstellung. Änderungen an der Media-Center-Dienstrichtlinie benötigen Administratorrechte und bleiben in der klassischen Prüfseite.", "MPTray autostart is a per-user registry setting. Media Center service policy changes need administrator rights and remain in the classic checks page."},
            {"Media-Center-Dienste: nicht installiert", "Media Center services: not installed"},
            {"Media-Center-Dienste: per Richtlinie deaktiviert", "Media Center services: disabled by policy"},
            {"Media-Center-Dienste: laufen", "Media Center services: running"},
            {"Media-Center-Dienste: gestoppt", "Media Center services: stopped"},
            {"TV-Server-Konfiguration öffnen", "Open TV Server configuration"},
            {"TV-Server-Ordner öffnen", "Open TV Server folder"},
            {"Details anzeigen", "Show details"},
            {"Die Zähler oben sind schreibgeschützt und kommen aus der TV-Server-Business-Layer. Kanäle, EPG, Karten, Aufnahmen und die TV-Datenbank werden vom TV-Server-Dienst und der eigenen SetupTv-Konfiguration verwaltet; Kanalsuche und TV-Server-Verwaltung werden nicht ersetzt.", "The counts above are read-only and come from the TV Server business layer. Channels, EPG, cards, schedules, and the TV database are managed by the TV Server service and its own SetupTv configuration; this page does not replace channel scanning or TV Server administration."},
            {"TV-Server gefunden", "TV Server found"},
            {"TV-Server nicht gefunden", "TV Server not found"},
            {"Keine TV-Server-Gentle.config gefunden.", "No TV Server Gentle.config found."},
            {"Kein Datenbank-Provider konfiguriert.", "No database provider configured."},
            {"Datenbank: ", "Database: "},
            {"Keine TV-Server-Business-Layer gefunden.", "No TV Server business layer found."},
            {"TV-Server-Übersicht wird gelesen …", "Reading TV Server overview …"},
            {"Zeitüberschreitung bei der TV-Server-Übersicht.", "TV Server overview timed out."},
            {"TV-Server-Übersicht nicht verfügbar: ", "TV Server overview unavailable: "},
            {"SetupTv.exe wurde nicht gefunden.", "SetupTv.exe was not found."},
            {"Der TV-Server-Ordner wurde nicht gefunden.", "The TV Server folder was not found."},
            {"keine EPG-Daten", "no EPG data"},
            {"jetzt: ", "now: "},
            {"  ·  danach: ", "  ·  next: "},
            {"TV-Server-Details", "TV Server details"},
            {"TV-Kanäle", "TV channels"},
            {"TV-EPG 12 h", "TV EPG 12 h"},
            {"Radio-Kanäle", "Radio channels"},
            {"Radio-EPG 12 h", "Radio EPG 12 h"},
            {"TV-Gruppen", "TV groups"},
            {"Radio-Gruppen", "Radio groups"},
            {"Aufnahmen", "Recordings"},
            {"Timer", "Schedules"},
            {"Kategorie", "Category"},
            {"＋ Ansicht", "＋ View"},
            {"Ansicht suchen", "Find view"},
            {"Ansicht suchen …", "Find view …"},
            {"Ungespeicherte Ansichtsänderungen verwerfen?", "Discard unsaved view changes?"},
            {" Ansichten", " views"},
            {" Filter", " filters"},
            {"Diese Ansicht hat keine Filter.", "This view has no filters."},
            {"Ungespeicherte Ansichtsänderungen", "Unsaved view changes"},
            {"Reihenfolge geändert · ungespeichert", "Order changed · unsaved"},
            {"Die Ansichten konnten nicht gespeichert werden.\r\n\r\n", "The views could not be saved.\r\n\r\n"},
            {"Filter hinzufügen", "Add filter"},
            {"Feld", "Field"},
            {"Sortierung", "Sort"},
            {"Ansicht", "View"},
            {"Virtuelles Laufwerk aktivieren", "Enable virtual drive"},
            {"Programmpfad", "Program path"},
            {"Durchsuchen …", "Browse …"},
            {"Programm für virtuelles Laufwerk wählen", "Select virtual drive program"},
            {"Programme (*.exe)|*.exe|Alle Dateien (*.*)|*.*", "Programs (*.exe)|*.exe|All files (*.*)|*.*"},
            {"Unterstützte Endungen", "Supported extensions"},
            {"Laufwerksnummer", "Drive number"},
            {"Vor Wiedergabe fragen", "Ask before playing"},
            {"Pfad prüfen", "Check path"},
            {"Erkannten Pfad übernehmen", "Apply detected path"},
            {"Erkannter Pfad übernommen: ", "Detected path applied: "},
            {"Daemon Tools wurde auf diesem System nicht erkannt. Programm manuell auswählen.", "Daemon Tools was not detected on this system. Select the program manually."},
            {"Erkannt: ", "Detected: "},
            {"Kein Pfad konfiguriert.", "No path configured."},
            {"Pfad ist gültig · ", "Path is valid · "},
            {" Bytes", " bytes"},
            {"XML-Datei", "XML file"},
            {"Datei sichern", "Save file"},
            {"Andere Datei …", "Other file …"},
            {"Ungespeicherte XML-Änderungen verwerfen?", "Discard unsaved XML changes?"},
            {"XML- oder Konfigurationsdatei öffnen", "Open XML or configuration file"},
            {"  ·  neue Datei", "  ·  new file"},
            {"Die XML-Datei konnte nicht gespeichert werden.\r\n\r\n", "The XML file could not be saved.\r\n\r\n"},
            {"Änderungen vor dem Verlassen speichern?", "Save your changes before leaving?"},
            {"Ausstehende Konfigurationsänderungen", "Pending configuration changes"},
            {"Keine ausstehenden Konfigurationseinträge. XML-Editoren können weiterhin ungespeicherten Text enthalten.", "No pending configuration entries. XML file editors may still contain unsaved text."},
            {"Nicht speichern", "Don't save"},
            {"entfernt", "removed"},
            {"neu", "new"},
            {"MP1 Configuration Studio läuft bereits in dieser Sitzung. Bitte zuerst das vorhandene Fenster schließen.", "MP1 Configuration Studio is already running in this session. Please close the existing window first."},
            {"MP1 Configuration Studio läuft bereits in dieser Sitzung. Bitte verwenden Sie das vorhandene Fenster.", "MP1 Configuration Studio is already running in this session. Please use the existing window."},
            {"Die originale Configuration.exe konnte nicht gestartet werden.\r\n\r\n", "The original Configuration.exe could not be started.\r\n\r\n"},
            {"Die neue Oberfläche konnte nicht initialisiert werden. Bitte dieses Fenster schließen und die klassische Oberfläche verwenden.\r\n\r\n", "The new interface could not be initialized. Please close this window and use the classic interface.\r\n\r\n"},
            {"Die Bildschirmskalierung hat sich geändert. Studio jetzt neu starten, damit das Layout neu berechnet wird?\r\n\r\nNicht gespeicherte Änderungen gehen verloren.", "Display scaling changed. Restart Studio now so the layout is recalculated?\r\n\r\nUnsaved changes will be lost."},
            {"Die neue Oberfläche konnte nicht gestartet werden.\r\n\r\n", "The new interface could not be started.\r\n\r\n"},
            {"Darstellung der nativen Seiten geändert. Bitte Studio neu starten.", "Native page mode changed. Please restart Studio to apply it."},
            {"Sprache", "Language"},
            {"Deutsch", "German"},
            {"Das Studio-Fenster wird geschlossen. Nicht gespeicherte Änderungen gehen verloren.\r\n\r\nDie originale Oberfläche wird anschließend gestartet. Fortfahren?", "The Studio window will close. Unsaved changes will be lost.\r\n\r\nThe original interface will then start. Continue?"},
            {"Für MediaPortal 1.39 x64.\r\n\r\n", "For MediaPortal 1.39 x64.\r\n\r\n"},
            {"Inoffizielles Projekt, nicht mit Team MediaPortal verbunden.\r\n", "Unofficial project, not affiliated with Team MediaPortal.\r\n"},
            {"Start, Verhalten und Grundeinstellungen deines Media Centers.", "Startup, behavior, and basic settings for your media center."},
            {"Skin, Sprache und Darstellung von MediaPortal.", "Skin, language, and presentation of MediaPortal."},
            {"Player, Ton und Bild – mit den originalen MediaPortal-Einstellungen.", "Player, audio, and video with the original MediaPortal settings."},
            {"Quellen und Bibliotheken für Videos, Musik und Bilder.", "Sources and libraries for videos, music, and pictures."},
            {"Tastatur, Fernbedienung und Eingabegeräte einrichten.", "Set up keyboard, remote, and input devices."},
            {"Erweiterungen aktivieren und ihre Konfiguration öffnen.", "Enable extensions and open their configuration."},
            {"Informationen, Kompatibilität und Diagnose.", "Information, compatibility, and diagnostics."},
            {"Manuell", "Manual"},
        };

        public static UiLanguage Current { get; private set; }

        static Localization()
        {
            Current = UiLanguage.German;
        }

        public static bool IsEnglish
        {
            get { return Current == UiLanguage.English; }
        }

        public static void Set(UiLanguage language)
        {
            Current = language;
        }

        public static void Set(string code)
        {
            if (String.Equals(code, "en", StringComparison.OrdinalIgnoreCase)) Current = UiLanguage.English;
            else if (String.Equals(code, "fr", StringComparison.OrdinalIgnoreCase)) Current = UiLanguage.French;
            else if (String.Equals(code, "nl", StringComparison.OrdinalIgnoreCase)) Current = UiLanguage.Dutch;
            else Current = UiLanguage.German;
        }

        public static string Code
        {
            get
            {
                switch (Current)
                {
                    case UiLanguage.English: return "en";
                    case UiLanguage.French: return "fr";
                    case UiLanguage.Dutch: return "nl";
                    default: return "de";
                }
            }
        }

        public static string DefaultCode
        {
            get { return "en"; }
        }

        public static string T(string german)
        {
            if (String.IsNullOrEmpty(german))
            {
                return german;
            }
            if (Current == UiLanguage.French || Current == UiLanguage.Dutch)
            {
                string translated = UiTranslations.Lookup(Code, german);
                if (translated != null) return translated;
            }
            if (Current == UiLanguage.German)
            {
                return german;
            }
            string english;
            return English.TryGetValue(german, out english) ? english : german;
        }

        /// <summary>Resolves an inline pair directly; for French and Dutch the German key is looked up in the UI tables and falls back to the English text.</summary>
        public static string T2(string german, string english)
        {
            if (Current == UiLanguage.French || Current == UiLanguage.Dutch)
            {
                string translated = UiTranslations.Lookup(Code, german);
                if (translated != null) return translated;
            }
            return Current == UiLanguage.English ? english : german;
        }

        /// <summary>Resolves an inline English/German pair in source order; the German text acts as the French/Dutch lookup key.</summary>
        public static string Pair(string english, string german)
        {
            return T2(german, english);
        }

        /// <summary>Generic pair for arrays and other values; French and Dutch fall back to the English value.</summary>
        public static TValue Pair<TValue>(TValue english, TValue german)
        {
            return Current == UiLanguage.English ? english : (Current == UiLanguage.German ? german : english);
        }

        public static string T(string german, params object[] args)
        {
            return String.Format(CultureInfo.CurrentCulture, T(german), args);
        }
    }
}

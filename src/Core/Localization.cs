using System;
using System.Collections.Generic;
using System.Globalization;

namespace MP1.ConfigurationStudio
{
    internal enum UiLanguage
    {
        German,
        English
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
            {"Warnung", "Warning"}
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
            Current = String.Equals(code, "en", StringComparison.OrdinalIgnoreCase) ? UiLanguage.English : UiLanguage.German;
        }

        public static string Code
        {
            get { return Current == UiLanguage.English ? "en" : "de"; }
        }

        public static string DefaultCode
        {
            get { return String.Equals(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName, "de", StringComparison.OrdinalIgnoreCase) ? "de" : "en"; }
        }

        public static string T(string german)
        {
            if (!IsEnglish || String.IsNullOrEmpty(german))
            {
                return german;
            }

            string english;
            return English.TryGetValue(german, out english) ? english : german;
        }

        public static string T(string german, params object[] args)
        {
            return String.Format(CultureInfo.CurrentCulture, T(german), args);
        }
    }
}

# Architektur – ALPHA3

## Native Konfiguration

Program.cs startet die unveränderte installierte Configuration.exe von MediaPortal 1.39 in einer eigenen AppDomain. Deren Konfigurationsdatei, ApplicationBase und Plugin-Probing-Pfade bleiben maßgeblich. Der zusätzliche Studio-Starter läuft im nativen Verzeichnis, damit Application.StartupPath auch für BASS und Plugin-Helfer stimmt. Die Architektur wird anhand der installierten Core.dll gewählt.

NativeBridge registriert den eigenen UI-Thread-Beobachter vor dem originalen EntryPoint. Der Shell-Umbau erfolgt vor der ersten Anzeige. Native Load-Ereignisse bleiben erhalten; erst nach dem Laden werden Eingabebaselines übernommen. Application.Idle dient als Rückfall. Der Thread-Hook wird beim Beenden entfernt.

## Navigation und originale Controls

Der originale TreeView bleibt als verborgenes Steuerelement vorhanden. StudioNavigation gruppiert seine vorhandenen Knoten, ohne sie zu verändern. Die Icon-Schaltflächen, Tabs, Suche und der Verlauf wählen dieselben TreeNodes. Damit bleiben CanActivate sowie OnSectionActivated/OnSectionDeActivated unter nativer Kontrolle. Änderungen des Standard-/Expertenmodus führen weiterhin die ursprüngliche Toolbar-Aktion aus.

Die Originalseiten, holderPanel und OK/Abbrechen bleiben dieselben Objekte. Ordner-, Log- und Werkzeugaktionen werden an ihre ursprünglichen ToolStripItems weitergereicht. Studio baut keine Einstellungswerte oder Speicherlogik nach. Die Gruppenmitgliedschaft verwendet die vorhandenen Abschnittstypen; unbekannte Typen bleiben unter System & Diagnose erreichbar.

## Gestaltung und Lesbarkeit

StudioShell.Layout.cs definiert das neue Layout mit Logo, Suche, Icon-Titeln, gruppierter Navigation und ausblendbarer Kontextspalte. StudioVisuals.cs zeichnet die eigenen Vektoricons, flache Aktionen und abgerundete Auswahlflächen ohne Bilddateien oder Icon-Font-Abhängigkeit.

PageZoom skaliert die vorhandene Geometrie zusammen mit den ursprünglichen Fonts und Mindestmaßen. Standardfonts werden an Segoe UI angeglichen; spezielle Symbol- und Festbreitenschriften bleiben als Familie erhalten. Werte, Radio-Gruppen und Control-Identität werden nicht ersetzt. Der native Seitencontainer erhält explizite Inhaltsmaße statt eines Dock-Fill-Kindes, damit auch große Seiten scrollbar bleiben.

StudioScrollHost erhält das originale AutoScroll-Verhalten. Ein geclippter Viewport blendet native Scrollleisten optisch aus; die schlanken eigenen Scroll-Controls bewegen dieselbe AutoScrollPosition. Tastaturfokus und die Suche können weiterhin das ursprüngliche ScrollControlIntoView verwenden. Auch die Hilfespalte läuft über einen solchen Host und erhält ihre Höhe aus dem tatsächlichen Textumbruch.

## Zeichnen

FrameRendering aktiviert die gemeinsame native Fensterkomposition. Sichtbare Layout- und Themewechsel verwenden verschachtelte Redraw-Transaktionen. Verborgene Fenster werden dabei nicht eingeschaltet. Der Inhaltsbereich bleibt während der nativen Seitenaktivierung zurückgehalten; unveränderte Theme-Aufrufe führen keinen erneuten Durchlauf aus.

ControlChrome zeichnet Standard-Auswahlfelder anhand der tatsächlichen HWND-Clientmaße. Bei individueller Owner-Draw-Darstellung bleibt der ursprüngliche Zeichner verantwortlich. Native Tabs, Listenüberschriften und Regler werden gepuffert gezeichnet. Reglerpositionen stammen aus den nativen Kanal- und Griffrechtecken. Listen und der verborgene Baum besitzen native Double-Buffering-Styles. Alle eigenen Graphics-/DC-Objekte werden pro Zeichenaufruf wieder freigegeben.

## Persistenz und Prüfwerkzeuge

Appearance.cs speichert nur Darstellungspräferenzen in einer separaten XML-Datei. Änderungen der MediaPortal-Einstellungen bleiben ausschließlich bei den nativen Handlern. Eine vollständige Transaktionskontrolle für beliebige Plugins oder Datenbankaktionen ist nicht implementiert.

Stage-TestRuntime.ps1 erzeugt eine isolierte Laufzeit für Config, Log, Database, Thumbs und Cache. `--verify` ist ausschließlich mit Testmarker und nach bestätigter Pfadisolation erlaubt. Die Prüfung ruft keine produktive Speicherfunktion auf und beendet sich über den ursprünglichen Abbrechen-Button. `--audit` inventarisiert die Originalseiten ohne Werteausgabe.

Update-Local.ps1 schließt nur Studio-Instanzen der ausgewählten Installation regulär, prüft den Build isoliert, sichert den vorherigen Stand und ersetzt nur die zusätzlichen Studio-Dateien. Eine Hashprüfung schützt das Originalprogramm; während der isolierten Prüfung wird auch die produktive Konfiguration überwacht. Anschließend wird der installierte Starter geöffnet.

## Referenzen und Lizenz

Die Integrationspunkte wurden anhand der installierten 1.39-Binärdateien und des offiziellen MediaPortal-Quellcodes geprüft:

- https://github.com/MediaPortal/MediaPortal-1/blob/master/mediaportal/Configuration/SettingsForm.cs
- https://github.com/MediaPortal/MediaPortal-1/blob/master/mediaportal/Configuration/SectionSettings.cs
- https://learn.microsoft.com/en-us/windows/win32/gdi/wm-setredraw
- https://learn.microsoft.com/en-us/windows/win32/winmsg/wm-showwindow

Der eigene Quellcode wird unter GPL-3.0-or-later bereitgestellt. Native MediaPortal-Binärdateien werden nicht mitverteilt.

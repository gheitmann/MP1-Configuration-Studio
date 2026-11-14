using System.Collections.Generic;

namespace MP1.ConfigurationStudio
{
    internal enum NativeFieldKind
    {
        Bool,
        Text,
        Integer,
        Combo,
        Info,
        Shares,
        DynamicBools,
        Folder,
        Filter,
        DynamicEntries,
        XmlFiles,
        RawSections,
        Classic,
        Keymap,
        Views,
        Dsp,
        TvServer,
        PluginConfig,
        GroupedSection,
        SystemChecks,
        VirtualDrive
    }

    internal enum NativeFilterKind
    {
        Video,
        Audio,
        AudioRenderer,
        Legacy
    }

    internal sealed class NativeOption
    {
        public string Value;
        public string LabelDe;
        public string LabelEn;

        public NativeOption(string value, string labelDe, string labelEn)
        {
            Value = value;
            LabelDe = labelDe;
            LabelEn = labelEn;
        }
    }

    internal sealed class NativeField
    {
        public string Section;
        public string Entry;
        public string LabelDe;
        public string LabelEn;
        public NativeFieldKind Kind;
        public string HelpDe;
        public string HelpEn;
        public string Suffix;
        public int Minimum;
        public int Maximum;
        public List<NativeOption> Options = new List<NativeOption>();
        public NativeFilterKind FilterKind;
        public string[] EntryPrefixes;

        public NativeField(string section, string entry, string labelDe, string labelEn, NativeFieldKind kind, string helpDe = null, string helpEn = null)
        {
            Section = section;
            Entry = entry;
            LabelDe = labelDe;
            LabelEn = labelEn;
            Kind = kind;
            HelpDe = helpDe;
            HelpEn = helpEn;
        }

        public NativeField Option(string value, string labelDe, string labelEn)
        {
            Options.Add(new NativeOption(value, labelDe, labelEn));
            return this;
        }

        public NativeField Range(int minimum, int maximum, string suffix = null)
        {
            Minimum = minimum;
            Maximum = maximum;
            Suffix = suffix;
            return this;
        }

        public NativeField Filter(NativeFilterKind kind)
        {
            FilterKind = kind;
            return this;
        }

        public NativeField Entries(params string[] prefixes)
        {
            EntryPrefixes = prefixes ?? new string[0];
            return this;
        }
    }

    internal sealed class NativeGroup
    {
        public string TitleDe;
        public string TitleEn;
        public List<NativeField> Fields = new List<NativeField>();

        public NativeGroup(string titleDe, string titleEn)
        {
            TitleDe = titleDe;
            TitleEn = titleEn;
        }

        public NativeGroup Add(NativeField field)
        {
            Fields.Add(field);
            return this;
        }
    }

    internal sealed class NativeSection
    {
        public string Id;
        public string TitleDe;
        public string TitleEn;
        public string SubtitleDe;
        public string SubtitleEn;
        public string Icon;
        public List<NativeGroup> Groups = new List<NativeGroup>();

        public NativeSection(string id, string titleDe, string titleEn, string subtitleDe, string subtitleEn, string icon)
        {
            Id = id;
            TitleDe = titleDe;
            TitleEn = titleEn;
            SubtitleDe = subtitleDe;
            SubtitleEn = subtitleEn;
            Icon = icon;
        }

        public NativeSection Group(NativeGroup group)
        {
            Groups.Add(group);
            return this;
        }
    }

    internal static class NativeSettingsSchema
    {
        public static List<NativeSection> Sections()
        {
            return new List<NativeSection>
            {
                new NativeSection("general", "Allgemein", "General", "Start, Stabilität und Grundverhalten von MediaPortal.", "Startup, stability, and basic MediaPortal behavior.", "settings")
                    .Group(new NativeGroup("Protokollierung & Stabilität", "Logging & stability")
                        .Add(new NativeField("general", "loglevel", "Protokollstufe", "Log level", NativeFieldKind.Combo, "Höhere Stufen schreiben mehr Details ins MediaPortal-Log.", "Higher levels write more detail to the MediaPortal log.")
                            .Option("0", "0 · Fehler", "0 · Error").Option("1", "1 · Warnung", "1 · Warning").Option("2", "2 · Information", "2 · Information").Option("3", "3 · Debug", "3 · Debug"))
                        .Add(new NativeField("general", "ThreadPriority", "Thread-Priorität", "Thread priority", NativeFieldKind.Combo, "Priorität der MediaPortal-Hintergrundthreads.", "Priority of MediaPortal background threads.")
                            .Option("BelowNormal", "Niedriger als normal", "Below normal").Option("Normal", "Normal", "Normal").Option("AboveNormal", "Höher als normal", "Above normal").Option("High", "Hoch", "High"))
                        .Add(new NativeField("general", "watchdogEnabled", "Watchdog aktivieren", "Enable watchdog", NativeFieldKind.Bool, "Startet MediaPortal nach einem Absturz neu.", "Restarts MediaPortal after a crash."))
                        .Add(new NativeField("general", "restartOnError", "Nach Fehler neu starten", "Restart after error", NativeFieldKind.Bool, "MediaPortal nach einem Fehler automatisch neu starten.", "Automatically restart MediaPortal after an error.")))
                    .Group(new NativeGroup("Start & Fenster", "Startup & window")
                        .Add(new NativeField("general", "startfullscreen", "Im Vollbild starten", "Start fullscreen", NativeFieldKind.Bool))
                        .Add(new NativeField("general", "alwaysontop", "Immer im Vordergrund", "Always on top", NativeFieldKind.Bool))
                        .Add(new NativeField("general", "hidetaskbar", "Taskleiste ausblenden", "Hide taskbar", NativeFieldKind.Bool))
                        .Add(new NativeField("general", "minimizeonstartup", "Beim Start minimieren", "Minimize on startup", NativeFieldKind.Bool))
                        .Add(new NativeField("general", "minimizeonexit", "Beim Beenden minimieren", "Minimize on exit", NativeFieldKind.Bool))
                        .Add(new NativeField("general", "minimizeonfocusloss", "Bei Fokusverlust minimieren", "Minimize on focus loss", NativeFieldKind.Bool))
                        .Add(new NativeField("general", "autostart", "Mit Windows starten", "Start with Windows", NativeFieldKind.Bool))
                        .Add(new NativeField("general", "delay startup", "Start verzögern", "Delay startup", NativeFieldKind.Bool))
                        .Add(new NativeField("general", "delay", "Verzögerung in Sekunden", "Delay in seconds", NativeFieldKind.Integer).Range(0, 300, "s")))
                    .Group(new NativeGroup("Vollbild & Start", "Fullscreen & startup")
                        .Add(new NativeField("general", "usefullscreensplash", "Vollbild-Splash anzeigen", "Show fullscreen splash", NativeFieldKind.Bool))
                        .Add(new NativeField("general", "ignorefullscreenresolutionchanges", "Auflösungswechsel ignorieren", "Ignore resolution changes", NativeFieldKind.Bool))
                        .Add(new NativeField("general", "delay resume", "Fortsetzen verzögern", "Delay resume", NativeFieldKind.Bool))
                        .Add(new NativeField("general", "turnoffmonitor", "Monitor ausschalten", "Turn off monitor", NativeFieldKind.Bool))
                        .Add(new NativeField("general", "showlastactivemodule", "Letztes Modul anzeigen", "Show last active module", NativeFieldKind.Bool))
                        .Add(new NativeField("general", "stoponaudioremoval", "Bei Audioentfernung stoppen", "Stop on audio removal", NativeFieldKind.Bool))
                        .Add(new NativeField("general", "noautostartonrdp", "Kein Autostart per RDP", "No autostart over RDP", NativeFieldKind.Bool))
                        .Add(new NativeField("general", "useprimaryscreen", "Primären Bildschirm verwenden", "Use primary screen", NativeFieldKind.Bool))
                        .Add(new NativeField("general", "usefcublackscreenfix", "FCU-Black-Screen-Fix", "FCU black-screen fix", NativeFieldKind.Bool)))
                    .Group(new NativeGroup("Renderer & Bild", "Renderer & picture")
                        .Add(new NativeField("general", "useEVRenderer", "EVR verwenden", "Use EVR", NativeFieldKind.Bool))
                        .Add(new NativeField("general", "useMadVideoRenderer", "madVR verwenden", "Use madVR", NativeFieldKind.Bool))
                        .Add(new NativeField("general", "useEVRMadVRForTV", "madVR für TV", "madVR for TV", NativeFieldKind.Bool))
                        .Add(new NativeField("general", "disableLowLatencyMode", "Low-Latency-Modus deaktivieren", "Disable low-latency mode", NativeFieldKind.Bool))
                        .Add(new NativeField("general", "useMadVideoRenderer3D", "madVR 3D", "madVR 3D", NativeFieldKind.Bool))
                        .Add(new NativeField("general", "useInternalDRC", "Interne DRC verwenden", "Use internal DRC", NativeFieldKind.Bool))
                        .Add(new NativeField("general", "useRestoreMadvr1080p", "madVR 1080p wiederherstellen", "Restore madVR 1080p", NativeFieldKind.Bool))
                        .Add(new NativeField("general", "usevrm9forwebstreams", "VRM9 für Webstreams", "VRM9 for web streams", NativeFieldKind.Bool))
                        .Add(new NativeField("general", "dx9decimatemask", "DX9-Dezimierungsmaske", "DX9 decimate mask", NativeFieldKind.Bool))
                        .Add(new NativeField("general", "nonsquare", "Nichtquadratische Pixel", "Non-square pixels", NativeFieldKind.Bool))
                        .Add(new NativeField("general", "exclusivemode", "Exklusiver Modus", "Exclusive mode", NativeFieldKind.Bool))
                        .Add(new NativeField("general", "dx9filteringmode", "DX9-Filtermodus", "DX9 filtering mode", NativeFieldKind.Text)))
                    .Group(new NativeGroup("Dynamic Refresh Rate", "Dynamic refresh rate")
                        .Add(new NativeField("general", "autochangerefreshrate", "Bildwiederholrate automatisch ändern", "Change refresh rate automatically", NativeFieldKind.Bool))
                        .Add(new NativeField("general", "notify_on_refreshrate", "Bei Änderung benachrichtigen", "Notify on change", NativeFieldKind.Bool))
                        .Add(new NativeField("general", "use_default_hz", "Standard-Hz verwenden", "Use default Hz", NativeFieldKind.Bool))
                        .Add(new NativeField("general", "devicereset", "Gerät zurücksetzen", "Reset device", NativeFieldKind.Bool))
                        .Add(new NativeField("general", "force_refresh_rate", "Bildwiederholrate erzwingen", "Force refresh rate", NativeFieldKind.Bool))
                        .Add(new NativeField("general", "default_hz", "Standard-Hz", "Default Hz", NativeFieldKind.Integer).Range(0, 240, "Hz"))
                        .Add(new NativeField("general", "default_hz_name", "Standard-Profilname", "Default profile name", NativeFieldKind.Text)))
                    .Group(new NativeGroup("Leerlauf & Autoplay", "Idle & autoplay")
                        .Add(new NativeField("general", "IdleTimer", "Leerlauf-Timer aktivieren", "Enable idle timer", NativeFieldKind.Bool))
                        .Add(new NativeField("general", "IdleTimeValue", "Leerlaufzeit in Sekunden", "Idle time in seconds", NativeFieldKind.Integer).Range(0, 3600, "s"))
                        .Add(new NativeField("general", "IdleBlanking", "Bildschirm abdunkeln", "Blank screen", NativeFieldKind.Bool))
                        .Add(new NativeField("general", "IdlePlugin", "Leerlauf-Plugin aktivieren", "Enable idle plugin", NativeFieldKind.Bool))
                        .Add(new NativeField("general", "IdlePluginWindow", "Leerlauf-Plugin-Fenster", "Idle plugin window", NativeFieldKind.Integer).Range(0, 99999))
                        .Add(new NativeField("general", "autoplay_audio", "Audio automatisch wiedergeben", "Autoplay audio", NativeFieldKind.Combo)
                            .Option("Ask", "Fragen", "Ask").Option("Yes", "Wiedergeben", "Play").Option("No", "Nicht wiedergeben", "Do not play"))
                        .Add(new NativeField("general", "autoplay_video", "Video automatisch wiedergeben", "Autoplay video", NativeFieldKind.Combo)
                            .Option("Ask", "Fragen", "Ask").Option("Yes", "Wiedergeben", "Play").Option("No", "Nicht wiedergeben", "Do not play"))
                        .Add(new NativeField("general", "autoplay_photo", "Bilder automatisch wiedergeben", "Autoplay photos", NativeFieldKind.Combo)
                            .Option("Ask", "Fragen", "Ask").Option("Yes", "Wiedergeben", "Play").Option("No", "Nicht wiedergeben", "Do not play")))
                    .Group(new NativeGroup("TV-Server", "TV server")
                        .Add(new NativeField("general", "HostDetectMethod", "Host-Erkennung", "Host detection", NativeFieldKind.Text, "Zum Beispiel Ping.", "For example Ping."))
                        .Add(new NativeField("general", "wait for tvserver", "Auf TV-Server warten", "Wait for TV server", NativeFieldKind.Bool, "Beim Start auf den TV-Server warten.", "Wait for the TV server during startup."))),
                new NativeSection("system", "System und Laufwerke", "System and drives", "Autoplay, CD-/DVD-Geschwindigkeit und Systemprüfungen.", "Autoplay, CD/DVD speed, and system checks.", "settings")
                    .Group(new NativeGroup("CD/DVD-Geschwindigkeit", "CD/DVD speed")
                        .Add(new NativeField("cdspeed", "drivespeedCD", "CD-Geschwindigkeit", "CD speed", NativeFieldKind.Text, "Kommagetrennte Geschwindigkeiten pro Laufwerk, z. B. 4,8,16.", "Comma-separated speeds per drive, e.g. 4,8,16."))
                        .Add(new NativeField("cdspeed", "drivespeedDVD", "DVD-Geschwindigkeit", "DVD speed", NativeFieldKind.Text, "Kommagetrennte Geschwindigkeiten pro Laufwerk.", "Comma-separated speeds per drive."))
                        .Add(new NativeField("cdspeed", "disableCD", "CD-Geschwindigkeit nicht ändern", "Do not change CD speed", NativeFieldKind.Text, "Kommagetrennte Flags pro Laufwerk: 1 deaktiviert die Regelung.", "Comma-separated flags per drive: 1 disables speed control."))
                        .Add(new NativeField("cdspeed", "disableDVD", "DVD-Geschwindigkeit nicht ändern", "Do not change DVD speed", NativeFieldKind.Text, "Kommagetrennte Flags pro Laufwerk: 1 deaktiviert die Regelung.", "Comma-separated flags per drive: 1 disables speed control.")))
                    .Group(new NativeGroup("Systemprüfungen", "System checks")
                        .Add(new NativeField("system", "checks", "Systemprüfungen", "System checks", NativeFieldKind.SystemChecks, "Media-Center-Dienste, MPTray-Autostart und Zugriff auf die klassischen Prüfungen.", "Media Center services, MPTray autostart, and access to the classic checks."))),
                new NativeSection("volume", "Lautstärke", "Volume", "Lautstärkeverhalten und Bildschirmanzeige.", "Volume behavior and on-screen display.", "play")
                    .Group(new NativeGroup("Wiedergabe", "Playback")
                        .Add(new NativeField("volume", "digital", "Digitale Lautstärke", "Digital volume", NativeFieldKind.Bool, "Digitale Lautstärkeregelung verwenden, sofern verfügbar.", "Use digital volume control when available."))
                        .Add(new NativeField("volume", "defaultVolumeOSD", "Lautstärke-OSD anzeigen", "Show volume OSD", NativeFieldKind.Bool))
                        .Add(new NativeField("volume", "hideWindowsOSD", "Windows-OSD ausblenden", "Hide Windows OSD", NativeFieldKind.Bool))
                        .Add(new NativeField("volume", "startuplevel", "Startlautstärke", "Startup volume", NativeFieldKind.Integer, "Wert zwischen 0 und 65535.", "Value between 0 and 65535.").Range(0, 65535)))
                    .Group(new NativeGroup("Hinweis", "Note")
                        .Add(new NativeField("volume", "handler", "Lautstärke-Handler", "Volume handler", NativeFieldKind.Info, "Die erweiterten Lautstärke-Handler und die Skalierungstabelle bleiben der klassischen Seite vorbehalten.", "Advanced volume handlers and the response table remain on the classic page."))),
                new NativeSection("pictures", "Bilder", "Pictures", "Diashow, Wiedergabe und Bildquellen.", "Slideshow, playback, and picture sources.", "surface")
                    .Group(new NativeGroup("Diashow", "Slideshow")
                        .Add(new NativeField("pictures", "speed", "Geschwindigkeit", "Speed", NativeFieldKind.Integer).Range(1, 10))
                        .Add(new NativeField("pictures", "transition", "Übergang", "Transition", NativeFieldKind.Integer).Range(0, 30))
                        .Add(new NativeField("pictures", "kenburns", "Ken Burns aktivieren", "Enable Ken Burns", NativeFieldKind.Bool))
                        .Add(new NativeField("pictures", "kenburnsspeed", "Ken-Burns-Geschwindigkeit", "Ken Burns speed", NativeFieldKind.Integer).Range(1, 30))
                        .Add(new NativeField("pictures", "random", "Zufällige Reihenfolge", "Random order", NativeFieldKind.Bool))
                        .Add(new NativeField("pictures", "autoShuffle", "Automatisch mischen", "Auto shuffle", NativeFieldKind.Bool))
                        .Add(new NativeField("pictures", "autoRepeat", "Automatisch wiederholen", "Auto repeat", NativeFieldKind.Bool)))
                    .Group(new NativeGroup("Anzeige & Wiedergabe", "Display & playback")
                        .Add(new NativeField("pictures", "useExif", "EXIF-Daten verwenden", "Use EXIF data", NativeFieldKind.Bool))
                        .Add(new NativeField("pictures", "useDayGrouping", "Nach Tagen gruppieren", "Group by day", NativeFieldKind.Bool))
                        .Add(new NativeField("pictures", "enableVideoPlayback", "Videowiedergabe aktivieren", "Enable video playback", NativeFieldKind.Bool))
                        .Add(new NativeField("pictures", "playVideosInSlideshows", "Videos in Diashows abspielen", "Play videos in slideshows", NativeFieldKind.Bool))
                        .Add(new NativeField("pictures", "showOSD", "OSD anzeigen", "Show OSD", NativeFieldKind.Bool))
                        .Add(new NativeField("pictures", "autoHideOSD", "OSD automatisch ausblenden", "Auto-hide OSD", NativeFieldKind.Bool)))
                    .Group(new NativeGroup("Dateien", "Files")
                        .Add(new NativeField("pictures", "extensions", "Bilddatei-Endungen", "Picture extensions", NativeFieldKind.Text, "Kommagetrennte Liste, z. B. .jpg,.png", "Comma-separated list, e.g. .jpg,.png"))
                        .Add(new NativeField("pictures", "rememberlastfolder", "Letzten Ordner merken", "Remember last folder", NativeFieldKind.Bool))
                        .Add(new NativeField("pictures", "AddOpticalDiskDrives", "Optische Laufwerke einbinden", "Include optical drives", NativeFieldKind.Bool))
                        .Add(new NativeField("pictures", "SwitchRemovableDrives", "Wechseldatenträger einbinden", "Include removable drives", NativeFieldKind.Bool))),
                new NativeSection("gui", "Oberfläche", "Interface", "Skin, Bedienung und Verhalten der MediaPortal-Oberfläche.", "Skin, input, and MediaPortal interface behavior.", "surface")
                    .Group(new NativeGroup("Allgemein", "General")
                        .Add(new NativeField("gui", "hideextensions", "Dateiendungen ausblenden", "Hide file extensions", NativeFieldKind.Bool))
                        .Add(new NativeField("gui", "enableguisounds", "Oberflächenklänge aktivieren", "Enable interface sounds", NativeFieldKind.Bool))
                        .Add(new NativeField("gui", "mousesupport", "Mausunterstützung", "Mouse support", NativeFieldKind.Bool))
                        .Add(new NativeField("gui", "reduceframerate", "Bildrate reduzieren", "Reduce frame rate", NativeFieldKind.Bool))
                        .Add(new NativeField("gui", "startbasichome", "Mit Basic Home starten", "Start with Basic Home", NativeFieldKind.Bool))
                        .Add(new NativeField("gui", "useonlyonehome", "Nur ein Home-Screen", "Use only one home screen", NativeFieldKind.Bool))
                        .Add(new NativeField("gui", "allowRememberLastFocusedItem", "Letztes fokussiertes Element merken", "Remember last focused item", NativeFieldKind.Bool))
                        .Add(new NativeField("gui", "fileexistscache", "Datei-Cache verwenden", "Use file-exists cache", NativeFieldKind.Bool))
                        .Add(new NativeField("gui", "addVideoFilesToDb", "Videodateien zur Datenbank hinzufügen", "Add video files to database", NativeFieldKind.Bool))
                        .Add(new NativeField("gui", "myprefix", "My-Präfix anzeigen", "Show My prefix", NativeFieldKind.Bool))
                        .Add(new NativeField("skin", "name", "Skin", "Skin", NativeFieldKind.Text, "Name des installierten Skins, z. B. Backdrop. Änderungen werden nach einem Neustart wirksam.", "Name of the installed skin, e.g. Backdrop. Changes take effect after a restart."))
                        .Add(new NativeField("gui", "language", "Sprache", "Language", NativeFieldKind.Text, "MediaPortal-Sprache, z. B. German oder English.", "MediaPortal language, e.g. German or English.")))
                    .Group(new NativeGroup("Listen & Scrollen", "Lists & scrolling")
                        .Add(new NativeField("gui", "ScrollSpeedRight", "Horizontale Scroll-Geschwindigkeit", "Horizontal scroll speed", NativeFieldKind.Integer).Range(1, 20))
                        .Add(new NativeField("gui", "ScrollSpeedDown", "Vertikale Scroll-Geschwindigkeit", "Vertical scroll speed", NativeFieldKind.Integer).Range(1, 20))
                        .Add(new NativeField("gui", "listLoopDelay", "Loop-Verzögerung", "Loop delay", NativeFieldKind.Integer, "Verzögerung beim Scrollen durch Listen.", "Delay when scrolling through lists.").Range(0, 5000, "ms"))),
                new NativeSection("home", "Startmenü", "Start menu", "Einträge im MediaPortal-Startmenü ein- oder ausblenden.", "Show or hide entries in the MediaPortal home menu.", "surface")
                    .Group(new NativeGroup("Startmenü-Einträge", "Home menu entries")
                        .Add(new NativeField("home", "menu", "Einträge", "Entries", NativeFieldKind.DynamicBools, "Die Liste wird aus der Konfiguration gelesen. Plugin-Einträge und unbekannte Menüpunkte bleiben erhalten.", "The list is read from the configuration. Plugin entries and unknown menu items are preserved."))),
                new NativeSection("movies", "Video", "Video", "Wiedergabe, Wiedergabelisten und Videoordner.", "Playback, playlists, and video folders.", "play")
                    .Group(new NativeGroup("Wiedergabe", "Playback")
                        .Add(new NativeField("movies", "repeat", "Wiedergabe wiederholen", "Repeat playback", NativeFieldKind.Bool))
                        .Add(new NativeField("movies", "playallinfolder", "Ganzen Ordner abspielen", "Play all in folder", NativeFieldKind.Combo)
                            .Option("0", "Nie", "Never").Option("1", "Fragen", "Ask").Option("2", "Immer", "Always").Option("3", "Nur bei Ordnern", "Folders only"))
                        .Add(new NativeField("movies", "keepfolderstogether", "Ordner zusammenhalten", "Keep folders together", NativeFieldKind.Bool))
                        .Add(new NativeField("movies", "markwatched", "Als gesehen markieren", "Mark as watched", NativeFieldKind.Bool))
                        .Add(new NativeField("movies", "playedpercentagewatched", "Gesehen ab Prozent", "Watched percentage", NativeFieldKind.Integer).Range(1, 100, "%")))
                    .Group(new NativeGroup("Dateien & Ordner", "Files & folders")
                        .Add(new NativeField("movies", "rememberlastfolder", "Letzten Ordner merken", "Remember last folder", NativeFieldKind.Bool))
                        .Add(new NativeField("movies", "AddOpticalDiskDrives", "Optische Laufwerke einbinden", "Include optical drives", NativeFieldKind.Bool))
                        .Add(new NativeField("movies", "SwitchRemovableDrives", "Wechseldatenträger einbinden", "Include removable drives", NativeFieldKind.Bool))
                        .Add(new NativeField("movies", "extensions", "Video-Endungen", "Video extensions", NativeFieldKind.Text, "Kommagetrennte Liste der Videoformate.", "Comma-separated video extensions."))
                        .Add(new NativeField("movies", "sharepath0", "Videoordner (klassisch bearbeiten)", "Video folders (edit in classic)", NativeFieldKind.Info, "Videoordner werden auf der Seite Medienordner verwaltet.", "Video folders are managed on the Media folders page.")))
                    .Group(new NativeGroup("Player", "Player")
                        .Add(new NativeField("movieplayer", "osdtimeout", "OSD-Timeout", "OSD timeout", NativeFieldKind.Integer, "0 = OSD dauerhaft anzeigen.", "0 = keep the OSD visible.").Range(0, 120, "s"))
                        .Add(new NativeField("movieplayer", "zapdelay", "Zap-Verzögerung", "Zap delay", NativeFieldKind.Integer).Range(0, 60, "s"))
                        .Add(new NativeField("movieplayer", "zaptimeout", "Zap-Timeout", "Zap timeout", NativeFieldKind.Integer).Range(0, 60, "s"))
                        .Add(new NativeField("movieplayer", "zapKeyTimeout", "Zap-Tasten-Timeout", "Zap key timeout", NativeFieldKind.Integer).Range(0, 60, "s"))
                        .Add(new NativeField("movieplayer", "skipsteps", "Skip-Schritte", "Skip steps", NativeFieldKind.Text, "Kommagetrennte Sekundenwerte.", "Comma-separated values in seconds."))
                        .Add(new NativeField("movieplayer", "skipsteptimeout", "Skip-Timeout", "Skip timeout", NativeFieldKind.Integer).Range(0, 10000, "ms"))
                        .Add(new NativeField("movieplayer", "immediateskipstepsisrelative", "Relative Skip-Schritte", "Relative skip steps", NativeFieldKind.Bool))
                        .Add(new NativeField("movieplayer", "immediateskipstepsize", "Direkte Skip-Größe", "Immediate skip size", NativeFieldKind.Integer).Range(0, 600, "s"))
                        .Add(new NativeField("movieplayer", "audiolanguage", "Bevorzugte Audiosprache", "Preferred audio language", NativeFieldKind.Text, "Sprachcode, z. B. de.", "Language code, e.g. de."))
                        .Add(new NativeField("movieplayer", "audiodefaultlanguage", "Audiosprache erzwingen", "Force audio language", NativeFieldKind.Bool))
                        .Add(new NativeField("movieplayer", "streamlavselection", "Stream-LAV-Auswahl", "Stream LAV selection", NativeFieldKind.Bool))
                        .Add(new NativeField("movieplayer", "wmvaudio", "WMV-Audio", "WMV audio", NativeFieldKind.Bool))
                        .Add(new NativeField("movieplayer", "defaultar", "Standard-Seitenverhältnis", "Default aspect ratio", NativeFieldKind.Combo)
                            .Option("Normal", "Normal", "Normal").Option("Original", "Original", "Original").Option("Zoom", "Zoom", "Zoom")
                            .Option("Zoom 14:9", "Zoom 14:9", "Zoom 14:9").Option("Stretch", "Strecken", "Stretch")
                            .Option("Non-linear Stretch", "Nichtlinear strecken", "Non-linear stretch").Option("Letterbox 4:3", "Letterbox 4:3", "Letterbox 4:3")
                            .Option("CinemaScope 2:35", "CinemaScope 2:35", "CinemaScope 2:35"))
                        .Add(new NativeField("movieplayer", "internal", "Internen Player verwenden", "Use internal player", NativeFieldKind.Bool))
                        .Add(new NativeField("movieplayer", "path", "Externer Player-Pfad", "External player path", NativeFieldKind.Text))
                        .Add(new NativeField("movieplayer", "arguments", "Externe Player-Parameter", "External player arguments", NativeFieldKind.Text))
                        .Add(new NativeField("comskip", "automaticskip", "Werbung automatisch überspringen", "Skip commercials automatically", NativeFieldKind.Bool))
                        .Add(new NativeField("audiodelay", "noaudioresetzero", "Audio-Delay nicht zurücksetzen", "Do not reset audio delay", NativeFieldKind.Bool)))
                    .Group(new NativeGroup("Bild & Zoom", "Picture & zoom")
                        .Add(new NativeField("movies", "defaultar", "Standard-Seitenverhältnis", "Default aspect ratio", NativeFieldKind.Combo)
                            .Option("Normal", "Normal", "Normal").Option("Original", "Original", "Original").Option("Zoom", "Zoom", "Zoom")
                            .Option("Zoom 14:9", "Zoom 14:9", "Zoom 14:9").Option("Stretch", "Strecken", "Stretch")
                            .Option("Non-linear Stretch", "Nichtlinear strecken", "Non-linear stretch").Option("Letterbox 4:3", "Letterbox 4:3", "Letterbox 4:3")
                            .Option("CinemaScope 2:35", "CinemaScope 2:35", "CinemaScope 2:35"))
                        .Add(new NativeField("movies", "allowarnormal", "Normal erlauben", "Allow normal", NativeFieldKind.Bool))
                        .Add(new NativeField("movies", "allowaroriginal", "Original erlauben", "Allow original", NativeFieldKind.Bool))
                        .Add(new NativeField("movies", "allowarzoom", "Zoom erlauben", "Allow zoom", NativeFieldKind.Bool))
                        .Add(new NativeField("movies", "allowarzoom149", "Zoom 14:9 erlauben", "Allow zoom 14:9", NativeFieldKind.Bool))
                        .Add(new NativeField("movies", "allowarstretch", "Strecken erlauben", "Allow stretch", NativeFieldKind.Bool))
                        .Add(new NativeField("movies", "allowarnonlinear", "Nichtlinear erlauben", "Allow non-linear", NativeFieldKind.Bool))
                        .Add(new NativeField("movies", "allowarletterbox", "Letterbox erlauben", "Allow letterbox", NativeFieldKind.Bool))
                        .Add(new NativeField("movies", "allowarcinemascope235", "CinemaScope 2.35 erlauben", "Allow CinemaScope 2.35", NativeFieldKind.Bool)))
                    .Group(new NativeGroup("Ansicht", "View")
                        .Add(new NativeField("myvideo", "layout", "Ansicht", "Layout", NativeFieldKind.Combo)
                            .Option("0", "Liste", "List").Option("1", "Kleine Symbole", "Small icons").Option("2", "Große Symbole", "Large icons")
                            .Option("3", "Filmstreifen", "Filmstrip").Option("4", "Albumansicht", "Album view").Option("5", "Wiedergabeliste", "Playlist").Option("6", "CoverFlow", "CoverFlow"))
                        .Add(new NativeField("myvideo", "sortmethod", "Sortierung", "Sort order", NativeFieldKind.Combo)
                            .Option("0", "Name", "Name").Option("1", "Name (alle)", "Name (all)").Option("2", "Name + Dauer", "Name + duration")
                            .Option("3", "Datum", "Date").Option("4", "Größe", "Size").Option("5", "Gesehen", "Watched")
                            .Option("6", "Jahr", "Year").Option("7", "Bewertung", "Rating").Option("8", "Label", "Label")
                            .Option("9", "Geändert", "Modified").Option("10", "Erstellt", "Created"))
                        .Add(new NativeField("myvideo", "sortmethodroot", "Sortierung (Stammordner)", "Sort order (root folders)", NativeFieldKind.Combo)
                            .Option("0", "Name", "Name").Option("1", "Name (alle)", "Name (all)").Option("2", "Name + Dauer", "Name + duration")
                            .Option("3", "Datum", "Date").Option("4", "Größe", "Size").Option("5", "Gesehen", "Watched")
                            .Option("6", "Jahr", "Year").Option("7", "Bewertung", "Rating").Option("8", "Label", "Label")
                            .Option("9", "Geändert", "Modified").Option("10", "Erstellt", "Created"))
                        .Add(new NativeField("myvideo", "sortasc", "Aufsteigend sortieren", "Sort ascending", NativeFieldKind.Bool))),
                new NativeSection("moviedatabase", "Video-Datenbank", "Video database", "Titel, Fanart und Scan-Verhalten der Videodatenbank.", "Title, fanart, and scan behavior of the video database.", "file")
                    .Group(new NativeGroup("Titel & Suche", "Title & search")
                        .Add(new NativeField("moviedatabase", "usesorttitle", "Sortiertitel verwenden", "Use sort title", NativeFieldKind.Bool))
                        .Add(new NativeField("moviedatabase", "usefolderastitle", "Ordnernamen als Titel", "Use folder name as title", NativeFieldKind.Bool))
                        .Add(new NativeField("moviedatabase", "preferfilenameforsearch", "Dateiname für Suche bevorzugen", "Prefer filename for search", NativeFieldKind.Bool))
                        .Add(new NativeField("moviedatabase", "striptitleprefixes", "Titelpräfixe entfernen", "Strip title prefixes", NativeFieldKind.Bool))
                        .Add(new NativeField("moviedatabase", "titleprefixes", "Titelpräfixe", "Title prefixes", NativeFieldKind.Text, "Kommagetrennte Präfixe, z. B. The, Les, Die.", "Comma-separated prefixes, e.g. The, Les, Die.")))
                    .Group(new NativeGroup("Fanart & Darstellung", "Fanart & display")
                        .Add(new NativeField("moviedatabase", "usefanart", "Fanart verwenden", "Use fanart", NativeFieldKind.Bool))
                        .Add(new NativeField("moviedatabase", "fanartnumber", "Anzahl Fanart-Bilder", "Fanart image count", NativeFieldKind.Integer).Range(0, 50))
                        .Add(new NativeField("moviedatabase", "actorslistsize", "Schauspielerliste", "Actors list size", NativeFieldKind.Text)))
                    .Group(new NativeGroup("Scan", "Scan")
                        .Add(new NativeField("moviedatabase", "scanskipexisting", "Vorhandene überspringen", "Skip existing", NativeFieldKind.Bool))
                        .Add(new NativeField("moviedatabase", "useonlynfoscraper", "Nur NFO-Scraper", "NFO scraper only", NativeFieldKind.Bool))
                        .Add(new NativeField("moviedatabase", "fetchactors", "Schauspieler laden", "Fetch actors", NativeFieldKind.Bool))),
                new NativeSection("videoviews", "Video-Ansichten", "Video views", "Ansichten, Filter und Sortierung für Videos komfortabel bearbeiten.", "Edit video views, filters, and sorting comfortably.", "video")
                    .Group(new NativeGroup("Ansichten", "Views")
                        .Add(new NativeField("videoviews", "VideoViews.xml", "Video-Ansichten", "Video views", NativeFieldKind.Views, "Ansichten und Filter aus VideoViews.xml. Reihenfolge mit ▲/▼ ändern; jede Änderung wird mit Backup gespeichert.", "Views and filters from VideoViews.xml. Change order with ▲/▼; every save creates a backup."))),
                new NativeSection("music", "Musik", "Music", "Audiowiedergabe, Playlists und Musikordner.", "Audio playback, playlists, and music folders.", "play")
                    .Group(new NativeGroup("Wiedergabe", "Playback")
                        .Add(new NativeField("audioplayer", "crossfade", "Crossfade", "Crossfade", NativeFieldKind.Integer, "Überblendzeit in Millisekunden.", "Crossfade time in milliseconds.").Range(0, 20000, "ms"))
                        .Add(new NativeField("audioplayer", "buffering", "Pufferung", "Buffering", NativeFieldKind.Integer).Range(0, 10000, "ms"))
                        .Add(new NativeField("audioplayer", "fadeOnStartStop", "Ein-/Ausblenden", "Fade in/out", NativeFieldKind.Bool))
                        .Add(new NativeField("audioplayer", "gaplessPlayback", "Gapless Playback", "Gapless playback", NativeFieldKind.Bool))
                        .Add(new NativeField("audioplayer", "wasapiExclusive", "WASAPI exklusiv", "WASAPI exclusive", NativeFieldKind.Bool))
                        .Add(new NativeField("audioplayer", "enableResume", "Wiedergabe fortsetzen", "Resume playback", NativeFieldKind.Bool))
                        .Add(new NativeField("musicmisc", "lookupSimilarTracks", "Ähnliche Titel suchen", "Look up similar tracks", NativeFieldKind.Bool))
                        .Add(new NativeField("musicmisc", "vumeter", "VU-Meter", "VU meter", NativeFieldKind.Text, "Zum Beispiel none.", "For example none.")))
                    .Group(new NativeGroup("Ausgabe & ReplayGain", "Output & ReplayGain")
                        .Add(new NativeField("audioplayer", "enableReplayGain", "ReplayGain aktivieren", "Enable ReplayGain", NativeFieldKind.Bool))
                        .Add(new NativeField("audioplayer", "enableAlbumReplayGain", "Album-ReplayGain", "Album ReplayGain", NativeFieldKind.Bool))
                        .Add(new NativeField("audioplayer", "streamOutputLevel", "Stream-Ausgangspegel", "Stream output level", NativeFieldKind.Integer).Range(0, 100, "%"))
                        .Add(new NativeField("audioplayer", "useSkipSteps", "Skip-Steps verwenden", "Use skip steps", NativeFieldKind.Bool))
                        .Add(new NativeField("audioplayer", "sounddevice", "Soundgerät", "Sound device", NativeFieldKind.Text))
                        .Add(new NativeField("audioplayer", "wasApiSpeakers", "WASAPI-Lautsprecher", "WASAPI speakers", NativeFieldKind.Integer).Range(1, 8)))
                    .Group(new NativeGroup("Ansicht", "View")
                        .Add(new NativeField("mymusic", "layout", "Ansicht", "Layout", NativeFieldKind.Combo)
                            .Option("0", "Liste", "List").Option("1", "Kleine Symbole", "Small icons").Option("2", "Große Symbole", "Large icons")
                            .Option("3", "Filmstreifen", "Filmstrip").Option("4", "Albumansicht", "Album view").Option("5", "Wiedergabeliste", "Playlist").Option("6", "CoverFlow", "CoverFlow"))
                        .Add(new NativeField("mymusic", "sortmethod", "Sortierung", "Sort order", NativeFieldKind.Combo)
                            .Option("0", "Name", "Name").Option("1", "Datum", "Date").Option("2", "Größe", "Size").Option("3", "Titelnummer", "Track")
                            .Option("4", "Dauer", "Duration").Option("5", "Titel", "Title").Option("6", "Interpret", "Artist").Option("7", "Album", "Album")
                            .Option("8", "Dateiname", "Filename").Option("9", "Bewertung", "Rating").Option("10", "Album-Interpret", "Album artist")
                            .Option("11", "Jahr", "Year").Option("12", "Disc-ID", "Disc ID").Option("13", "Komponist", "Composer")
                            .Option("14", "Wiedergaben", "Times played").Option("15", "Dateityp", "File type"))
                        .Add(new NativeField("mymusic", "sortasc", "Aufsteigend sortieren", "Sort ascending", NativeFieldKind.Bool)))
                    .Group(new NativeGroup("Dateien & Ordner", "Files & folders")
                        .Add(new NativeField("music", "rememberlastfolder", "Letzten Ordner merken", "Remember last folder", NativeFieldKind.Bool))
                        .Add(new NativeField("music", "AddOpticalDiskDrives", "Optische Laufwerke einbinden", "Include optical drives", NativeFieldKind.Bool))
                        .Add(new NativeField("music", "SwitchRemovableDrives", "Wechseldatenträger einbinden", "Include removable drives", NativeFieldKind.Bool))
                        .Add(new NativeField("music", "extensions", "Audio-Endungen", "Audio extensions", NativeFieldKind.Text, "Kommagetrennte Liste der Audioformate.", "Comma-separated audio extensions."))
                        .Add(new NativeField("music", "playlists", "Playlist-Ordner", "Playlist folder", NativeFieldKind.Folder, "Ordner für gespeicherte Wiedergabelisten.", "Folder for saved playlists."))
                        .Add(new NativeField("music", "sharepath0", "Musikordner (klassisch bearbeiten)", "Music folders (edit in classic)", NativeFieldKind.Info, "Musikordner werden auf der Seite Medienordner verwaltet.", "Music folders are managed on the Media folders page."))),
                new NativeSection("musicfiles", "Musik-Dateien", "Music files", "Wiedergabe, Wiedergabelisten und Import der Musikdatenbank.", "Playback, playlists, and music database import.", "file")
                    .Group(new NativeGroup("Wiedergabe", "Playback")
                        .Add(new NativeField("musicfiles", "repeat", "Wiederholen", "Repeat", NativeFieldKind.Bool))
                        .Add(new NativeField("musicfiles", "autoshuffle", "Automatisch mischen", "Auto shuffle", NativeFieldKind.Bool))
                        .Add(new NativeField("musicfiles", "numberOfSongsToLoad", "Geladene Titel", "Songs to load", NativeFieldKind.Integer).Range(0, 100000))
                        .Add(new NativeField("musicfiles", "selectOption", "Auswahlaktion", "Selection action", NativeFieldKind.Text, "Zum Beispiel play.", "For example play.")))
                    .Group(new NativeGroup("Wiedergabelisten", "Playlists")
                        .Add(new NativeField("musicfiles", "savePlaylistOnExit", "Playlist beim Beenden speichern", "Save playlist on exit", NativeFieldKind.Bool))
                        .Add(new NativeField("musicfiles", "resumePlaylistOnMusicEnter", "Playlist beim Öffnen fortsetzen", "Resume playlist on open", NativeFieldKind.Bool))
                        .Add(new NativeField("musicfiles", "playlistIsCurrent", "Playlist ist aktuell", "Playlist is current", NativeFieldKind.Bool))
                        .Add(new NativeField("musicfiles", "savePlaylistUTF8", "Playlist als UTF-8 speichern", "Save playlist as UTF-8", NativeFieldKind.Bool))
                        .Add(new NativeField("mymusicplaylist", "layout", "Playlist-Ansicht", "Playlist layout", NativeFieldKind.Combo)
                            .Option("0", "Liste", "List").Option("1", "Kleine Symbole", "Small icons").Option("2", "Große Symbole", "Large icons")
                            .Option("3", "Filmstreifen", "Filmstrip").Option("4", "Albumansicht", "Album view").Option("5", "Wiedergabeliste", "Playlist").Option("6", "CoverFlow", "CoverFlow"))
                        .Add(new NativeField("mymusicplaylist", "sortmethod", "Playlist-Sortierung", "Playlist sort order", NativeFieldKind.Combo)
                            .Option("0", "Name", "Name").Option("1", "Datum", "Date").Option("2", "Größe", "Size").Option("3", "Titelnummer", "Track")
                            .Option("4", "Dauer", "Duration").Option("5", "Titel", "Title").Option("6", "Interpret", "Artist").Option("7", "Album", "Album")
                            .Option("8", "Dateiname", "Filename").Option("9", "Bewertung", "Rating").Option("10", "Album-Interpret", "Album artist")
                            .Option("11", "Jahr", "Year").Option("12", "Disc-ID", "Disc ID").Option("13", "Komponist", "Composer")
                            .Option("14", "Wiedergaben", "Times played").Option("15", "Dateityp", "File type"))
                        .Add(new NativeField("mymusicplaylist", "sortasc", "Playlist aufsteigend sortieren", "Sort playlist ascending", NativeFieldKind.Bool)))
                    .Group(new NativeGroup("Import & Vorschaubilder", "Import & thumbnails")
                        .Add(new NativeField("musicfiles", "addall", "Alle Titel hinzufügen", "Add all tracks", NativeFieldKind.Bool))
                        .Add(new NativeField("musicfiles", "extractthumbs", "Vorschaubilder extrahieren", "Extract thumbnails", NativeFieldKind.Bool))
                        .Add(new NativeField("musicfiles", "createartistthumbs", "Interpreten-Vorschaubilder", "Artist thumbnails", NativeFieldKind.Bool))
                        .Add(new NativeField("musicfiles", "creategenrethumbs", "Genre-Vorschaubilder", "Genre thumbnails", NativeFieldKind.Bool))
                        .Add(new NativeField("musicfiles", "useFolderThumbs", "Ordner-Vorschaubilder", "Folder thumbnails", NativeFieldKind.Bool))
                        .Add(new NativeField("musicfiles", "useAllImages", "Alle Bilder verwenden", "Use all images", NativeFieldKind.Bool))
                        .Add(new NativeField("musicfiles", "createMissingFolderThumbs", "Fehlende Ordnerbilder erzeugen", "Create missing folder thumbnails", NativeFieldKind.Bool))
                        .Add(new NativeField("musicfiles", "treatFolderAsAlbum", "Ordner als Album behandeln", "Treat folder as album", NativeFieldKind.Bool))
                        .Add(new NativeField("musicfiles", "monitorShares", "Freigaben überwachen", "Monitor shares", NativeFieldKind.Bool))
                        .Add(new NativeField("musicfiles", "updateSinceLastImport", "Nur seit letztem Import aktualisieren", "Update since last import", NativeFieldKind.Bool))
                        .Add(new NativeField("musicfiles", "stripartistprefixes", "Interpreten-Präfixe entfernen", "Strip artist prefixes", NativeFieldKind.Bool))
                        .Add(new NativeField("musicfiles", "artistprefixes", "Interpreten-Präfixe", "Artist prefixes", NativeFieldKind.Text, "Kommagetrennte Präfixe, z. B. The, Les, Die.", "Comma-separated prefixes, e.g. The, Les, Die.")))
                    .Group(new NativeGroup("MP3-Import", "MP3 import")
                        .Add(new NativeField("musicimport", "mp3importdir", "Import-Ordner", "Import folder", NativeFieldKind.Folder))
                        .Add(new NativeField("musicimport", "format", "Zielformat", "Target format", NativeFieldKind.Text, "Muster für Dateinamen und Ordner.", "Pattern for file and folder names."))
                        .Add(new NativeField("musicimport", "mp3priority", "Priorität", "Priority", NativeFieldKind.Integer).Range(0, 10))
                        .Add(new NativeField("musicimport", "mp3bitrate", "Bitrate", "Bitrate", NativeFieldKind.Integer).Range(0, 10))
                        .Add(new NativeField("musicimport", "mp3quality", "Qualität", "Quality", NativeFieldKind.Integer).Range(0, 10))
                        .Add(new NativeField("musicimport", "mp3vbr", "VBR", "VBR", NativeFieldKind.Bool))
                        .Add(new NativeField("musicimport", "mp3cbr", "CBR", "CBR", NativeFieldKind.Bool))
                        .Add(new NativeField("musicimport", "mp3mono", "Mono", "Mono", NativeFieldKind.Bool))
                        .Add(new NativeField("musicimport", "mp3fastmode", "Schnellmodus", "Fast mode", NativeFieldKind.Bool))
                        .Add(new NativeField("musicimport", "mp3database", "Datenbank aktualisieren", "Update database", NativeFieldKind.Bool))
                        .Add(new NativeField("musicimport", "mp3background", "Im Hintergrund", "In background", NativeFieldKind.Bool))
                        .Add(new NativeField("musicimport", "mp3replaceexisting", "Vorhandene ersetzen", "Replace existing", NativeFieldKind.Bool))
                        .Add(new NativeField("musicimport", "importunknown", "Unbekannte importieren", "Import unknown", NativeFieldKind.Bool))),
                new NativeSection("musicviews", "Musik-Ansichten", "Music views", "Ansichten, Filter und Sortierung für Musik komfortabel bearbeiten.", "Edit music views, filters, and sorting comfortably.", "music")
                    .Group(new NativeGroup("Ansichten", "Views")
                        .Add(new NativeField("musicviews", "MusicViews.xml", "Musik-Ansichten", "Music views", NativeFieldKind.Views, "Ansichten und Filter aus MusicViews.xml. Reihenfolge mit ▲/▼ ändern; jede Änderung wird mit Backup gespeichert.", "Views and filters from MusicViews.xml. Change order with ▲/▼; every save creates a backup."))),
                new NativeSection("musicdsp", "Musik-DSP", "Music DSP", "Musikordner sowie BASS-, VST- und Winamp-DSP-Effekte bearbeiten.", "Edit the music folder and BASS, VST, and Winamp DSP effects.", "music")
                    .Group(new NativeGroup("Musik-DSP", "Music DSP")
                        .Add(new NativeField("musicdsp", "musicdsp.xml", "Musik-DSP", "Music DSP", NativeFieldKind.Dsp, "musicdsp.xml mit Musikordner, BASS-Effekten und VST-/Winamp-Plugins. Speichern erstellt ein Backup.", "musicdsp.xml with music folder, BASS effects, and VST/Winamp plugins. Save creates a backup."))),
                new NativeSection("folders", "Medienordner", "Media folders", "Video-, Musik- und Bilderordner verwalten.", "Manage video, music, and picture folders.", "folder")
                    .Group(new NativeGroup("Videoordner", "Video folders")
                        .Add(new NativeField("movies", "sharename", "Videoordner", "Video folders", NativeFieldKind.Shares, "Lokale oder Netzwerkordner für Videos. Scannen aktualisiert die Datenbank.", "Local or network folders for videos. Scan updates the database.")))
                    .Group(new NativeGroup("Musikordner", "Music folders")
                        .Add(new NativeField("music", "sharename", "Musikordner", "Music folders", NativeFieldKind.Shares, "Lokale oder Netzwerkordner für Musik. Scannen aktualisiert die Datenbank.", "Local or network folders for music. Scan updates the database.")))
                    .Group(new NativeGroup("Bilderordner", "Picture folders")
                        .Add(new NativeField("pictures", "sharename", "Bilderordner", "Picture folders", NativeFieldKind.Shares, "Lokale oder Netzwerkordner für Bilder.", "Local or network folders for pictures."))),
                new NativeSection("tv", "TV", "TV", "TV-Server, Aufnahmen und Live-TV-Verhalten.", "TV server, recordings, and live TV behavior.", "play")
                    .Group(new NativeGroup("TV-Server-Status", "TV server status")
                        .Add(new NativeField("tvserver", "status", "TV-Server", "TV server", NativeFieldKind.TvServer, "Status, Datenbank und Schnellzugriff auf SetupTv. Kanäle, EPG und Aufnahmen bleiben in der TV-Server-Datenbank.", "Status, database, and quick access to SetupTv. Channels, EPG, and recordings remain in the TV Server database.")))
                    .Group(new NativeGroup("TV-Server", "TV server")
                        .Add(new NativeField("tvservice", "hostname", "Server-Hostname", "Server hostname", NativeFieldKind.Text))
                        .Add(new NativeField("tvservice", "isWakeOnLanEnabled", "Wake-on-LAN", "Wake on LAN", NativeFieldKind.Bool))
                        .Add(new NativeField("tvservice", "WOLTimeOut", "WOL-Timeout", "WOL timeout", NativeFieldKind.Integer).Range(0, 120, "s"))
                        .Add(new NativeField("tvservice", "isAutoMacAddressEnabled", "MAC-Adresse automatisch", "Automatic MAC address", NativeFieldKind.Bool))
                        .Add(new NativeField("tvservice", "macAddress", "MAC-Adresse", "MAC address", NativeFieldKind.Text))
                        .Add(new NativeField("tvservice", "recordingpath", "Aufnahmeordner", "Recording folder", NativeFieldKind.Folder, "Leer lassen für den MediaPortal-Standard.", "Leave empty for the MediaPortal default."))
                        .Add(new NativeField("tvservice", "timeshiftingpath", "Timeshift-Ordner", "Timeshift folder", NativeFieldKind.Folder))
                        .Add(new NativeField("tvservice", "preferac3", "AC3 bevorzugen", "Prefer AC3", NativeFieldKind.Bool))
                        .Add(new NativeField("tvservice", "preferAudioTypeOverLang", "Audiospur vor Sprache", "Prefer audio type over language", NativeFieldKind.Bool))
                        .Add(new NativeField("tvservice", "preferredaudiolanguages", "Bevorzugte Audiosprachen", "Preferred audio languages", NativeFieldKind.Text, "Kommagetrennte Sprachcodes, z. B. de,en.", "Comma-separated language codes, e.g. de,en."))
                        .Add(new NativeField("tvservice", "preferredsublanguages", "Bevorzugte Untertitelsprachen", "Preferred subtitle languages", NativeFieldKind.Text, "Kommagetrennte Sprachcodes, z. B. de,en.", "Comma-separated language codes, e.g. de,en."))
                        .Add(new NativeField("tvservice", "dvbbitmapsubtitles", "DVB-Bitmap-Untertitel", "DVB bitmap subtitles", NativeFieldKind.Bool))
                        .Add(new NativeField("tvservice", "dvbttxtsubtitles", "DVB-Teletext-Untertitel", "DVB teletext subtitles", NativeFieldKind.Bool))
                        .Add(new NativeField("tvservice", "ccsubtitles", "Closed Captions", "Closed captions", NativeFieldKind.Bool))
                        .Add(new NativeField("tvservice", "autoshowsubwhentvstarts", "Untertitel beim TV-Start anzeigen", "Show subtitles when TV starts", NativeFieldKind.Bool))
                        .Add(new NativeField("tvservice", "audiodualmono", "Dual-Mono-Audio", "Dual-mono audio", NativeFieldKind.Bool))
                        .Add(new NativeField("tvservice", "usertsp", "RTSP verwenden", "Use RTSP", NativeFieldKind.Bool)))
                    .Group(new NativeGroup("Live-TV", "Live TV")
                        .Add(new NativeField("mytv", "autoturnontv", "TV automatisch starten", "Auto-start TV", NativeFieldKind.Bool))
                        .Add(new NativeField("mytv", "autofullscreen", "Automatisch Vollbild", "Auto fullscreen", NativeFieldKind.Bool))
                        .Add(new NativeField("mytv", "showchannelnumber", "Kanalnummer anzeigen", "Show channel number", NativeFieldKind.Bool))
                        .Add(new NativeField("mytv", "channelnumbermaxlength", "Maximale Kanalnummernlänge", "Maximum channel number length", NativeFieldKind.Integer).Range(1, 6))
                        .Add(new NativeField("mytv", "hideAllChannelsGroup", "Gruppe „Alle Kanäle“ ausblenden", "Hide all-channels group", NativeFieldKind.Bool))
                        .Add(new NativeField("mytv", "showChannelStateIcons", "Kanalstatus-Symbole", "Channel state icons", NativeFieldKind.Bool))
                        .Add(new NativeField("mytv", "continuousScrollGuide", "Endlos-Scroll im Guide", "Continuous guide scroll", NativeFieldKind.Bool))
                        .Add(new NativeField("mytv", "enableRecNotifier", "Aufnahme-Hinweis", "Recording notifier", NativeFieldKind.Bool))
                        .Add(new NativeField("mytv", "notifyTVBefore", "Hinweis vor Aufnahme", "Notify before recording", NativeFieldKind.Integer).Range(0, 3600, "s"))
                        .Add(new NativeField("mytv", "notifyTVTimeout", "Hinweisdauer", "Notification duration", NativeFieldKind.Integer).Range(1, 120, "s"))
                        .Add(new NativeField("mytv", "notifybeep", "Signalton bei Hinweis", "Beep on notification", NativeFieldKind.Bool))
                        .Add(new NativeField("mytv", "confirmTimeshiftStop", "Timeshift-Stopp bestätigen", "Confirm timeshift stop", NativeFieldKind.Bool))
                        .Add(new NativeField("mytv", "enableChannelChange", "Kanalwechsel per Nummer", "Channel change by number", NativeFieldKind.Bool))
                        .Add(new NativeField("mytv", "byindex", "Nach Index sortieren", "Sort by index", NativeFieldKind.Bool))
                        .Add(new NativeField("mytv", "relaxTsReader", "TsReader entspannen", "Relax TsReader", NativeFieldKind.Bool, "Reduziert die Leselast bei langsamen Datenträgern.", "Reduces read pressure on slow media.")))
                    .Group(new NativeGroup("Bild & Ton", "Picture & sound")
                        .Add(new NativeField("mytv", "deinterlace", "Deinterlacing", "Deinterlacing", NativeFieldKind.Integer, "0 = aus, höhere Werte aktivieren die Deinterlacing-Stufe.", "0 = off, higher values enable the deinterlacing stage.").Range(0, 10))
                        .Add(new NativeField("mytv", "defaultar", "Standard-Seitenverhältnis", "Default aspect ratio", NativeFieldKind.Combo)
                            .Option("Normal", "Normal", "Normal").Option("Original", "Original", "Original").Option("Zoom", "Zoom", "Zoom")
                            .Option("Zoom 14:9", "Zoom 14:9", "Zoom 14:9").Option("Stretch", "Strecken", "Stretch")
                            .Option("Non-linear Stretch", "Nichtlinear strecken", "Non-linear stretch").Option("Letterbox 4:3", "Letterbox 4:3", "Letterbox 4:3")
                            .Option("CinemaScope 2:35", "CinemaScope 2:35", "CinemaScope 2:35"))
                        .Add(new NativeField("mytv", "allowarnormal", "Normal erlauben", "Allow normal", NativeFieldKind.Bool))
                        .Add(new NativeField("mytv", "allowaroriginal", "Original erlauben", "Allow original", NativeFieldKind.Bool))
                        .Add(new NativeField("mytv", "allowarzoom", "Zoom erlauben", "Allow zoom", NativeFieldKind.Bool))
                        .Add(new NativeField("mytv", "allowarzoom149", "Zoom 14:9 erlauben", "Allow zoom 14:9", NativeFieldKind.Bool))
                        .Add(new NativeField("mytv", "allowarstretch", "Strecken erlauben", "Allow stretch", NativeFieldKind.Bool))
                        .Add(new NativeField("mytv", "allowarnonlinear", "Nichtlinear erlauben", "Allow non-linear", NativeFieldKind.Bool))
                        .Add(new NativeField("mytv", "allowarletterbox", "Letterbox erlauben", "Allow letterbox", NativeFieldKind.Bool))
                        .Add(new NativeField("mytv", "allowarcinemascope235", "CinemaScope 2.35 erlauben", "Allow CinemaScope 2.35", NativeFieldKind.Bool)))
                    .Group(new NativeGroup("Teletext", "Teletext")
                        .Add(new NativeField("mytv", "teletextHidden", "Teletext verbergen", "Hide teletext", NativeFieldKind.Bool))
                        .Add(new NativeField("mytv", "teletextTransparent", "Teletext transparent", "Transparent teletext", NativeFieldKind.Bool))
                        .Add(new NativeField("mytv", "teletextRemember", "Teletext-Einstellungen merken", "Remember teletext settings", NativeFieldKind.Bool))
                        .Add(new NativeField("mytv", "teletextMaxFontSize", "Maximale Teletext-Schriftgröße", "Maximum teletext font size", NativeFieldKind.Integer).Range(50, 300, "%")))
                    .Group(new NativeGroup("Suche", "Search")
                        .Add(new NativeField("tvsearch", "searchmode", "Suchmodus", "Search mode", NativeFieldKind.Text, "Zum Beispiel Title.", "For example Title."))
                        .Add(new NativeField("tvsearch", "cursortmethod", "Aktuelle Sortierung", "Current sort", NativeFieldKind.Text))
                        .Add(new NativeField("tvsearch", "chosortmethod", "Sortierung nach Suche", "Sort after search", NativeFieldKind.Text))),
                new NativeSection("radio", "Radio", "Radio", "Radio-Sender, Gruppen und Verhalten.", "Radio channels, groups, and behavior.", "play")
                    .Group(new NativeGroup("Radio", "Radio")
                        .Add(new NativeField("myradio", "hideAllChannelsGroup", "Gruppe „Alle Kanäle“ ausblenden", "Hide all-channels group", NativeFieldKind.Bool))
                        .Add(new NativeField("myradio", "rememberlastgroup", "Letzte Gruppe merken", "Remember last group", NativeFieldKind.Bool))
                        .Add(new NativeField("myradio", "rootgroup", "Startgruppe", "Start group", NativeFieldKind.Text, "Leer oder (none) für alle Gruppen.", "Empty or (none) for all groups."))
                        .Add(new NativeField("myradio", "autoturnonradio", "Radio automatisch starten", "Auto-start radio", NativeFieldKind.Bool))
                        .Add(new NativeField("myradio", "byindex", "Nach Index sortieren", "Sort by index", NativeFieldKind.Bool))
                        .Add(new NativeField("myradio", "showchannelnumber", "Kanalnummer anzeigen", "Show channel number", NativeFieldKind.Bool))
                        .Add(new NativeField("myradio", "channelnumbermaxlength", "Maximale Kanalnummernlänge", "Maximum channel number length", NativeFieldKind.Integer).Range(1, 6))
                        .Add(new NativeField("myradio", "layout", "Ansicht", "Layout", NativeFieldKind.Integer).Range(0, 10))
                        .Add(new NativeField("myradio", "sort", "Sortierung", "Sort order", NativeFieldKind.Text, "Zum Beispiel number oder name.", "For example number or name."))
                        .Add(new NativeField("myradio", "sortasc", "Aufsteigend sortieren", "Sort ascending", NativeFieldKind.Bool)))
                    .Group(new NativeGroup("Suche", "Search")
                        .Add(new NativeField("radiosearch", "searchmode", "Suchmodus", "Search mode", NativeFieldKind.Text, "Zum Beispiel Title.", "For example Title."))
                        .Add(new NativeField("radiosearch", "cursortmethod", "Aktuelle Sortierung", "Current sort", NativeFieldKind.Text))
                        .Add(new NativeField("radiosearch", "chosortmethod", "Sortierung nach Suche", "Sort after search", NativeFieldKind.Text))),
                new NativeSection("recordings", "TV-Aufnahmen", "TV recordings", "Aufnahmeliste und TV-Guide.", "Recording list and TV guide.", "play")
                    .Group(new NativeGroup("Aufnahmeliste", "Recording list")
                        .Add(new NativeField("tvrecorded", "layout", "Ansicht", "Layout", NativeFieldKind.Integer).Range(0, 10))
                        .Add(new NativeField("tvrecorded", "sort", "Sortierung", "Sort order", NativeFieldKind.Text, "Zum Beispiel channel, name oder date.", "For example channel, name, or date."))
                        .Add(new NativeField("tvrecorded", "sortasc", "Aufsteigend sortieren", "Sort ascending", NativeFieldKind.Bool)))
                    .Group(new NativeGroup("TV-Guide", "TV guide")
                        .Add(new NativeField("tvguide", "timeperblock", "Minuten pro Block", "Minutes per block", NativeFieldKind.Integer).Range(5, 120, "min"))),
                new NativeSection("subtitles", "Untertitel", "Subtitles", "Untertitel-Engine, Darstellung und Timing.", "Subtitle engine, appearance, and timing.", "keyboard")
                    .Group(new NativeGroup("Allgemein", "General")
                        .Add(new NativeField("subtitles", "enabled", "Untertitel aktivieren", "Enable subtitles", NativeFieldKind.Bool))
                        .Add(new NativeField("subtitles", "autoloadSubtitle", "Untertitel automatisch laden", "Auto-load subtitles", NativeFieldKind.Bool))
                        .Add(new NativeField("subtitles", "engine", "Engine", "Engine", NativeFieldKind.Text))
                        .Add(new NativeField("subtitles", "language", "Sprache", "Language", NativeFieldKind.Text))
                        .Add(new NativeField("subtitles", "paths", "Suchpfade", "Search paths", NativeFieldKind.Text, "Mehrere Pfade mit Semikolon trennen.", "Separate multiple paths with semicolons."))
                        .Add(new NativeField("subtitles", "saveNever", "Nie speichern", "Never save", NativeFieldKind.Bool))
                        .Add(new NativeField("subtitles", "saveAsk", "Beim Speichern fragen", "Ask before saving", NativeFieldKind.Bool))
                        .Add(new NativeField("subtitles", "selectionoff", "Auswahl deaktiviert", "Selection disabled", NativeFieldKind.Bool)))
                    .Group(new NativeGroup("Darstellung", "Appearance")
                        .Add(new NativeField("subtitles", "fontface", "Schriftart", "Font face", NativeFieldKind.Text))
                        .Add(new NativeField("subtitles", "fontsize", "Schriftgröße", "Font size", NativeFieldKind.Integer).Range(8, 72))
                        .Add(new NativeField("subtitles", "bold", "Fett", "Bold", NativeFieldKind.Bool))
                        .Add(new NativeField("subtitles", "color", "Farbe (RRGGBB)", "Color (RRGGBB)", NativeFieldKind.Text))
                        .Add(new NativeField("subtitles", "borderOutline", "Randkontur", "Border outline", NativeFieldKind.Bool))
                        .Add(new NativeField("subtitles", "borderWidth", "Randbreite", "Border width", NativeFieldKind.Integer).Range(0, 10))
                        .Add(new NativeField("subtitles", "shadow", "Schatten", "Shadow", NativeFieldKind.Integer).Range(0, 10))
                        .Add(new NativeField("subtitles", "charset", "Zeichensatz", "Character set", NativeFieldKind.Integer).Range(0, 255))
                        .Add(new NativeField("subtitles", "subPosRelative", "Position relativ", "Relative position", NativeFieldKind.Bool))
                        .Add(new NativeField("subtitles", "subStyleOverride", "Stil überschreiben", "Override style", NativeFieldKind.Bool)))
                    .Group(new NativeGroup("Timing", "Timing")
                        .Add(new NativeField("subtitles", "delayInterval", "Verzögerungsschritt", "Delay step", NativeFieldKind.Integer).Range(50, 5000, "ms"))),
                new NativeSection("remote", "Fernbedienung", "Remote", "Empfänger und Profile für Fernbedienungen.", "Receivers and profiles for remote controls.", "keyboard")
                    .Group(new NativeGroup("Hauppauge HCW", "Hauppauge HCW")
                        .Add(new NativeField("remote", "HCW", "HCW aktivieren", "Enable HCW", NativeFieldKind.Bool))
                        .Add(new NativeField("remote", "HCWAllowExternal", "Externe Steuerung erlauben", "Allow external control", NativeFieldKind.Bool))
                        .Add(new NativeField("remote", "HCWKeepControl", "Steuerung behalten", "Keep control", NativeFieldKind.Bool))
                        .Add(new NativeField("remote", "HCWVerboseLog", "Ausführliches Log", "Verbose log", NativeFieldKind.Bool))
                        .Add(new NativeField("remote", "HCWButtonRelease", "Tastenfreigabe", "Button release", NativeFieldKind.Integer).Range(0, 2000, "ms"))
                        .Add(new NativeField("remote", "HCWRepeatFilter", "Wiederholungsfilter", "Repeat filter", NativeFieldKind.Integer).Range(0, 20))
                        .Add(new NativeField("remote", "HCWRepeatSpeed", "Wiederholungsgeschwindigkeit", "Repeat speed", NativeFieldKind.Integer).Range(0, 100))
                        .Add(new NativeField("remote", "HCWFilterDoubleKlicks", "Doppelklicks filtern", "Filter double clicks", NativeFieldKind.Bool)))
                    .Group(new NativeGroup("X10", "X10")
                        .Add(new NativeField("remote", "X10", "X10 aktivieren", "Enable X10", NativeFieldKind.Bool))
                        .Add(new NativeField("remote", "X10Medion", "Medion", "Medion", NativeFieldKind.Bool))
                        .Add(new NativeField("remote", "X10ATI", "ATI Remote Wonder", "ATI Remote Wonder", NativeFieldKind.Bool))
                        .Add(new NativeField("remote", "X10Firefly", "Firefly", "Firefly", NativeFieldKind.Bool))
                        .Add(new NativeField("remote", "X10VerboseLog", "Ausführliches Log", "Verbose log", NativeFieldKind.Bool))
                        .Add(new NativeField("remote", "X10UseChannelControl", "Kanalsteuerung verwenden", "Use channel control", NativeFieldKind.Bool))
                        .Add(new NativeField("remote", "X10Channel", "X10-Kanal", "X10 channel", NativeFieldKind.Integer).Range(0, 999)))
                    .Group(new NativeGroup("HID", "HID")
                        .Add(new NativeField("remote", "HidEnabled", "HID aktivieren", "Enable HID", NativeFieldKind.Bool))
                        .Add(new NativeField("remote", "HidVerbose", "Ausführliches Log", "Verbose log", NativeFieldKind.Bool))
                        .Add(new NativeField("remote", "HidKeyboard", "HID-Tastatur", "HID keyboard", NativeFieldKind.Bool))
                        .Add(new NativeField("remote", "HidRepeatDelayInMs", "Wiederholungsverzögerung", "Repeat delay", NativeFieldKind.Integer).Range(-1, 5000, "ms"))
                        .Add(new NativeField("remote", "HidRepeatSpeedInMs", "Wiederholungsgeschwindigkeit", "Repeat speed", NativeFieldKind.Integer).Range(-1, 5000, "ms"))
                        .Add(new NativeField("remote", "HidProfile", "HID-Profil", "HID profile", NativeFieldKind.Text)))
                    .Group(new NativeGroup("AppCommand", "AppCommand")
                        .Add(new NativeField("remote", "AppCommand", "AppCommand aktivieren", "Enable AppCommand", NativeFieldKind.Bool))
                        .Add(new NativeField("remote", "AppCommandVerbose", "Ausführliches Log", "Verbose log", NativeFieldKind.Bool))
                        .Add(new NativeField("remote", "AppCommandBackground", "Im Hintergrund", "In background", NativeFieldKind.Bool)))
                    .Group(new NativeGroup("IRTrans", "IRTrans")
                        .Add(new NativeField("remote", "IRTrans", "IRTrans aktivieren", "Enable IRTrans", NativeFieldKind.Bool))
                        .Add(new NativeField("remote", "IRTransRemoteModel", "Modell", "Model", NativeFieldKind.Text))
                        .Add(new NativeField("remote", "IRTransServerPort", "Server-Port", "Server port", NativeFieldKind.Integer).Range(1, 65535))
                        .Add(new NativeField("remote", "IRTransVerboseLog", "Ausführliches Log", "Verbose log", NativeFieldKind.Bool)))
                    .Group(new NativeGroup("FireDTV", "FireDTV")
                        .Add(new NativeField("remote", "FireDTV", "FireDTV aktivieren", "Enable FireDTV", NativeFieldKind.Bool))
                        .Add(new NativeField("remote", "FireDTVVerboseLog", "Ausführliches Log", "Verbose log", NativeFieldKind.Bool)))
                    .Group(new NativeGroup("Centarea", "Centarea")
                        .Add(new NativeField("remote", "Centarea", "Centarea aktivieren", "Enable Centarea", NativeFieldKind.Bool))
                        .Add(new NativeField("remote", "CentareaVerbose", "Ausführliches Log", "Verbose log", NativeFieldKind.Bool))
                        .Add(new NativeField("remote", "CentareaMouseOkMap", "Maus-OK-Zuordnung", "Mouse OK mapping", NativeFieldKind.Bool))
                        .Add(new NativeField("remote", "CentareaJoystickMap", "Joystick-Zuordnung", "Joystick mapping", NativeFieldKind.Bool))),
                new NativeSection("keymap", "Tastatur", "Keyboard", "Tastenbelegung für Global und alle MediaPortal-Fenster bearbeiten.", "Edit key mappings for Global and all MediaPortal windows.", "keyboard")
                    .Group(new NativeGroup("Tastenbelegung", "Key mapping")
                        .Add(new NativeField("keymap", "keymap.xml", "Tastenbelegung", "Key mapping", NativeFieldKind.Keymap, "Aktion auswählen und eine neue Taste aufnehmen. Speichern erstellt ein Backup von keymap.xml.", "Select an action and capture a new key. Save creates a backup of keymap.xml."))),
                new NativeSection("codecs", "Codecs", "Codecs", "Decoder und Renderer für Video, TV und Audio.", "Decoders and renderers for video, TV, and audio.", "settings")
                    .Group(new NativeGroup("Video", "Video")
                        .Add(new NativeField("movieplayer", "autodecodersettings", "Automatische Decoder-Einstellungen", "Automatic decoder settings", NativeFieldKind.Bool))
                        .Add(new NativeField("movieplayer", "forcesourcesplitter", "Source-Splitter erzwingen", "Force source splitter", NativeFieldKind.Bool))
                        .Add(new NativeField("movieplayer", "usemoviecodects", "Movie-Codecs verwenden", "Use movie codecs", NativeFieldKind.Bool))
                        .Add(new NativeField("movieplayer", "mpeg2videocodec", "MPEG-2 Video", "MPEG-2 video", NativeFieldKind.Filter).Filter(NativeFilterKind.Video))
                        .Add(new NativeField("movieplayer", "h264videocodec", "H.264 Video", "H.264 video", NativeFieldKind.Filter).Filter(NativeFilterKind.Video))
                        .Add(new NativeField("movieplayer", "hevcvideocodec", "HEVC Video", "HEVC video", NativeFieldKind.Filter).Filter(NativeFilterKind.Video))
                        .Add(new NativeField("movieplayer", "vc1videocodec", "VC-1 Video", "VC-1 video", NativeFieldKind.Filter).Filter(NativeFilterKind.Video))
                        .Add(new NativeField("movieplayer", "vc1ivideocodec", "VC-1i Video", "VC-1i video", NativeFieldKind.Filter).Filter(NativeFilterKind.Video))
                        .Add(new NativeField("movieplayer", "xvidvideocodec", "DivX / Xvid Video", "DivX / Xvid video", NativeFieldKind.Filter).Filter(NativeFilterKind.Video)))
                    .Group(new NativeGroup("Audio", "Audio")
                        .Add(new NativeField("movieplayer", "mpeg2audiocodec", "MPEG / AC3 Audio", "MPEG / AC3 audio", NativeFieldKind.Filter).Filter(NativeFilterKind.Audio))
                        .Add(new NativeField("movieplayer", "aacaudiocodec", "AAC Audio", "AAC audio", NativeFieldKind.Filter).Filter(NativeFilterKind.Audio))
                        .Add(new NativeField("movieplayer", "audiorenderer", "Audio-Renderer", "Audio renderer", NativeFieldKind.Filter).Filter(NativeFilterKind.AudioRenderer))
                        .Add(new NativeField("movieplayer", "splitterfilter", "Source-Filter", "Source filter", NativeFieldKind.Filter).Filter(NativeFilterKind.Legacy))
                        .Add(new NativeField("movieplayer", "splitterfilefilter", "Splitter-Filter", "Splitter filter", NativeFieldKind.Filter).Filter(NativeFilterKind.Legacy)))
                    .Group(new NativeGroup("TV", "TV")
                        .Add(new NativeField("mytv", "videocodec", "TV Video", "TV video", NativeFieldKind.Filter).Filter(NativeFilterKind.Video))
                        .Add(new NativeField("mytv", "h264videocodec", "TV H.264", "TV H.264", NativeFieldKind.Filter).Filter(NativeFilterKind.Video))
                        .Add(new NativeField("mytv", "hevcvideocodec", "TV HEVC", "TV HEVC", NativeFieldKind.Filter).Filter(NativeFilterKind.Video))
                        .Add(new NativeField("mytv", "audiocodec", "TV Audio", "TV audio", NativeFieldKind.Filter).Filter(NativeFilterKind.Audio))
                        .Add(new NativeField("mytv", "aacaudiocodec", "TV AAC", "TV AAC", NativeFieldKind.Filter).Filter(NativeFilterKind.Audio))
                        .Add(new NativeField("mytv", "ddplusaudiocodec", "TV DD+", "TV DD+", NativeFieldKind.Filter).Filter(NativeFilterKind.Audio))
                        .Add(new NativeField("mytv", "audiorenderer", "TV Audio-Renderer", "TV audio renderer", NativeFieldKind.Filter).Filter(NativeFilterKind.AudioRenderer)))
                    .Group(new NativeGroup("Blu-ray", "Blu-ray")
                        .Add(new NativeField("bdplayer", "mpeg2videocodec", "MPEG-2 Video", "MPEG-2 video", NativeFieldKind.Filter).Filter(NativeFilterKind.Video))
                        .Add(new NativeField("bdplayer", "h264videocodec", "H.264 Video", "H.264 video", NativeFieldKind.Filter).Filter(NativeFilterKind.Video))
                        .Add(new NativeField("bdplayer", "hevcvideocodec", "HEVC Video", "HEVC video", NativeFieldKind.Filter).Filter(NativeFilterKind.Video))
                        .Add(new NativeField("bdplayer", "vc1videocodec", "VC-1 Video", "VC-1 video", NativeFieldKind.Filter).Filter(NativeFilterKind.Video))
                        .Add(new NativeField("bdplayer", "mpeg2audiocodec", "MPEG / AC3 Audio", "MPEG / AC3 audio", NativeFieldKind.Filter).Filter(NativeFilterKind.Audio))
                        .Add(new NativeField("bdplayer", "audiorenderer", "Audio-Renderer", "Audio renderer", NativeFieldKind.Filter).Filter(NativeFilterKind.AudioRenderer)))
                    .Group(new NativeGroup("DVD", "DVD")
                        .Add(new NativeField("dvdplayer", "videocodec", "DVD Video", "DVD video", NativeFieldKind.Filter).Filter(NativeFilterKind.Video))
                        .Add(new NativeField("dvdplayer", "audiocodec", "DVD Audio", "DVD audio", NativeFieldKind.Filter).Filter(NativeFilterKind.Audio))
                        .Add(new NativeField("dvdplayer", "ac3", "AC3", "AC3", NativeFieldKind.Bool))
                        .Add(new NativeField("dvdplayer", "navigator", "DVD Navigator", "DVD navigator", NativeFieldKind.Filter).Filter(NativeFilterKind.Legacy))
                        .Add(new NativeField("dvdplayer", "audiorenderer", "Audio-Renderer", "Audio renderer", NativeFieldKind.Filter).Filter(NativeFilterKind.AudioRenderer))
                        .Add(new NativeField("dvdplayer", "turnoffdxva", "DXVA deaktivieren", "Disable DXVA", NativeFieldKind.Bool)))
                    .Group(new NativeGroup("Video Renderer", "Video renderer")
                        .Add(new NativeField("general", "useEVRenderer", "EVR verwenden", "Use EVR", NativeFieldKind.Bool))
                        .Add(new NativeField("general", "useMadVideoRenderer", "madVR verwenden", "Use madVR", NativeFieldKind.Bool))
                        .Add(new NativeField("general", "useEVRMadVRForTV", "madVR für TV", "madVR for TV", NativeFieldKind.Bool))
                        .Add(new NativeField("general", "disableLowLatencyMode", "Low-Latency-Modus deaktivieren", "Disable low-latency mode", NativeFieldKind.Bool))
                        .Add(new NativeField("general", "useMadVideoRenderer3D", "madVR 3D", "madVR 3D", NativeFieldKind.Bool))
                        .Add(new NativeField("general", "useInternalDRC", "Interne DRC verwenden", "Use internal DRC", NativeFieldKind.Bool))
                        .Add(new NativeField("general", "useRestoreMadvr1080p", "madVR 1080p wiederherstellen", "Restore madVR 1080p", NativeFieldKind.Bool))),
                new NativeSection("discs", "BD / DVD", "BD / DVD", "Blu-ray- und DVD-Wiedergabe.", "Blu-ray and DVD playback.", "play")
                    .Group(new NativeGroup("Blu-ray", "Blu-ray")
                        .Add(new NativeField("bdplayer", "useInternalBDPlayer", "Internen BD-Player verwenden", "Use internal BD player", NativeFieldKind.Bool))
                        .Add(new NativeField("bdplayer", "useforbluray", "Externen Player für Blu-rays", "External player for Blu-rays", NativeFieldKind.Bool))
                        .Add(new NativeField("bdplayer", "path", "Player-Pfad", "Player path", NativeFieldKind.Text))
                        .Add(new NativeField("bdplayer", "arguments", "Player-Parameter", "Player arguments", NativeFieldKind.Text))
                        .Add(new NativeField("bdplayer", "audiolanguage", "Audiosprache", "Audio language", NativeFieldKind.Text))
                        .Add(new NativeField("bdplayer", "subtitlelanguage", "Untertitelsprache", "Subtitle language", NativeFieldKind.Text))
                        .Add(new NativeField("bdplayer", "subtitlesenabled", "Untertitel aktiviert", "Subtitles enabled", NativeFieldKind.Bool))
                        .Add(new NativeField("bdplayer", "regioncode", "Regionscode", "Region code", NativeFieldKind.Text)))
                    .Group(new NativeGroup("Blu-ray Bild & Zoom", "Blu-ray picture & zoom")
                        .Add(new NativeField("bdplayer", "defaultar", "Standard-Seitenverhältnis", "Default aspect ratio", NativeFieldKind.Combo)
                            .Option("Normal", "Normal", "Normal").Option("Original", "Original", "Original").Option("Zoom", "Zoom", "Zoom")
                            .Option("Zoom 14:9", "Zoom 14:9", "Zoom 14:9").Option("Stretch", "Strecken", "Stretch")
                            .Option("Non-linear Stretch", "Nichtlinear strecken", "Non-linear stretch").Option("Letterbox 4:3", "Letterbox 4:3", "Letterbox 4:3")
                            .Option("CinemaScope 2:35", "CinemaScope 2:35", "CinemaScope 2:35"))
                        .Add(new NativeField("bdplayer", "allowarnormal", "Normal erlauben", "Allow normal", NativeFieldKind.Bool))
                        .Add(new NativeField("bdplayer", "allowaroriginal", "Original erlauben", "Allow original", NativeFieldKind.Bool))
                        .Add(new NativeField("bdplayer", "allowarzoom", "Zoom erlauben", "Allow zoom", NativeFieldKind.Bool))
                        .Add(new NativeField("bdplayer", "allowarzoom149", "Zoom 14:9 erlauben", "Allow zoom 14:9", NativeFieldKind.Bool))
                        .Add(new NativeField("bdplayer", "allowarstretch", "Strecken erlauben", "Allow stretch", NativeFieldKind.Bool))
                        .Add(new NativeField("bdplayer", "allowarnonlinear", "Nichtlinear erlauben", "Allow non-linear", NativeFieldKind.Bool))
                        .Add(new NativeField("bdplayer", "allowarletterbox", "Letterbox erlauben", "Allow letterbox", NativeFieldKind.Bool))
                        .Add(new NativeField("bdplayer", "allowarcinemascope235", "CinemaScope 2.35 erlauben", "Allow CinemaScope 2.35", NativeFieldKind.Bool)))
                    .Group(new NativeGroup("DVD", "DVD")
                        .Add(new NativeField("dvdplayer", "internal", "Internen DVD-Player verwenden", "Use internal DVD player", NativeFieldKind.Bool))
                        .Add(new NativeField("dvdplayer", "usefordvd", "Externen Player für DVDs", "External player for DVDs", NativeFieldKind.Bool))
                        .Add(new NativeField("dvdplayer", "path", "Player-Pfad", "Player path", NativeFieldKind.Text))
                        .Add(new NativeField("dvdplayer", "arguments", "Player-Parameter", "Player arguments", NativeFieldKind.Text))
                        .Add(new NativeField("dvdplayer", "audiolanguage", "Audiosprache", "Audio language", NativeFieldKind.Text))
                        .Add(new NativeField("dvdplayer", "subtitlelanguage", "Untertitelsprache", "Subtitle language", NativeFieldKind.Text))
                        .Add(new NativeField("dvdplayer", "showsubtitles", "Untertitel anzeigen", "Show subtitles", NativeFieldKind.Bool))
                        .Add(new NativeField("dvdplayer", "showclosedcaptions", "Closed Captions anzeigen", "Show closed captions", NativeFieldKind.Bool))
                        .Add(new NativeField("dvdplayer", "pixelratiocorrection", "Pixel-Ratio-Korrektur", "Pixel ratio correction", NativeFieldKind.Bool)))
                    .Group(new NativeGroup("DVD Bild & Zoom", "DVD picture & zoom")
                        .Add(new NativeField("dvdplayer", "defaultar", "Standard-Seitenverhältnis", "Default aspect ratio", NativeFieldKind.Combo)
                            .Option("Normal", "Normal", "Normal").Option("Original", "Original", "Original").Option("Zoom", "Zoom", "Zoom")
                            .Option("Zoom 14:9", "Zoom 14:9", "Zoom 14:9").Option("Stretch", "Strecken", "Stretch")
                            .Option("Non-linear Stretch", "Nichtlinear strecken", "Non-linear stretch").Option("Letterbox 4:3", "Letterbox 4:3", "Letterbox 4:3")
                            .Option("CinemaScope 2:35", "CinemaScope 2:35", "CinemaScope 2:35"))
                        .Add(new NativeField("dvdplayer", "allowarnormal", "Normal erlauben", "Allow normal", NativeFieldKind.Bool))
                        .Add(new NativeField("dvdplayer", "allowaroriginal", "Original erlauben", "Allow original", NativeFieldKind.Bool))
                        .Add(new NativeField("dvdplayer", "allowarzoom", "Zoom erlauben", "Allow zoom", NativeFieldKind.Bool))
                        .Add(new NativeField("dvdplayer", "allowarzoom149", "Zoom 14:9 erlauben", "Allow zoom 14:9", NativeFieldKind.Bool))
                        .Add(new NativeField("dvdplayer", "allowarstretch", "Strecken erlauben", "Allow stretch", NativeFieldKind.Bool))
                        .Add(new NativeField("dvdplayer", "allowarnonlinear", "Nichtlinear erlauben", "Allow non-linear", NativeFieldKind.Bool))
                        .Add(new NativeField("dvdplayer", "allowarletterbox", "Letterbox erlauben", "Allow letterbox", NativeFieldKind.Bool))
                        .Add(new NativeField("dvdplayer", "allowarcinemascope235", "CinemaScope 2.35 erlauben", "Allow CinemaScope 2.35", NativeFieldKind.Bool)))
                    .Group(new NativeGroup("Virtuelles Laufwerk", "Virtual drive")
                        .Add(new NativeField("daemon", "path", "Virtuelles Laufwerk", "Virtual drive", NativeFieldKind.VirtualDrive, "Daemon Tools erkennen, Programm wählen, Endungen festlegen und den Pfad prüfen.", "Detect Daemon Tools, choose the program, set extensions, and check the path."))),
                new NativeSection("plugins", "Plugins", "Plugins", "Plugins aktivieren, deaktivieren und direkt konfigurieren.", "Enable, disable, and configure plugins directly.", "plugin")
                    .Group(new NativeGroup("Plugin-Konfiguration", "Plugin configuration")
                        .Add(new NativeField("plugins", "native", "Plugin-Konfiguration", "Plugin configuration", NativeFieldKind.PluginConfig, "Erkennt Plugins mit eigenem Konfigurationsfenster und öffnet es direkt. Die Fenster selbst bleiben im Plugin-Stil.", "Detects plugins with their own configuration window and opens it directly. The windows keep the plugin's own style.")))
                    .Group(new NativeGroup("Klassische Konfiguration", "Classic configuration")
                        .Add(new NativeField("plugins", "plugin", "Plugins", "Plugins", NativeFieldKind.Info, "Fallback: Die klassische MediaPortal-Konfiguration öffnet weiterhin alle Plugin-Fenster.", "Fallback: the classic MediaPortal configuration still opens every plugin window.")))
                    .Group(new NativeGroup("Aktiviert", "Enabled")
                        .Add(new NativeField("plugins", "windows", "Aktivierte Plugins", "Enabled plugins", NativeFieldKind.DynamicBools, "Entspricht „Enabled“ im klassischen Plugin-Menü. Änderungen werden nach einem Neustart wirksam.", "Matches “Enabled” in the classic plugin menu. Changes take effect after a restart.")))
                    .Group(new NativeGroup("Im Home-Menü", "Listed in Home")
                        .Add(new NativeField("home", "home", "Im Home-Menü gelistet", "Listed in Home", NativeFieldKind.DynamicBools, "Entspricht „Listed in Home“ im klassischen Plugin-Menü.", "Matches “Listed in Home” in the classic plugin menu.")))
                    .Group(new NativeGroup("In My Plugins", "Listed in My Plugins")
                        .Add(new NativeField("myplugins", "plugins", "In My Plugins gelistet", "Listed in My Plugins", NativeFieldKind.DynamicBools, "Entspricht „Listed in My Plugins“ im klassischen Plugin-Menü.", "Matches “Listed in My Plugins” in the classic plugin menu.")))
                    .Group(new NativeGroup("Plugin-DLLs", "Plugin DLLs")
                        .Add(new NativeField("pluginsdlls", "dlls", "Aktivierte Plugin-DLLs", "Enabled plugin DLLs", NativeFieldKind.DynamicBools, "Lädt oder entlädt Plugin-DLLs. Nur ändern, wenn die Auswirkung bekannt ist.", "Loads or unloads plugin DLLs. Change only when the effect is known."))),
                new NativeSection("plugindata", "Plugin-Daten", "Plugin data", "Plugin- und interne Bereiche als Formular bearbeiten.", "Edit plugin and internal sections as forms.", "settings")
                    .Group(new NativeGroup("OnlineVideos", "OnlineVideos")
                        .Add(new NativeField("onlinevideos", "entries", "OnlineVideos", "OnlineVideos", NativeFieldKind.GroupedSection, "Ansichten, Verlauf und Startzeit als Formular.", "Views, history, and first-run time as a form.")))
                    .Group(new NativeGroup("Trakt", "Trakt")
                        .Add(new NativeField("Trakt", "entries", "Trakt", "Trakt", NativeFieldKind.GroupedSection, "Konto, Synchronisierung, Layouts, Sortierung, Empfehlungen und weitere Trakt-Einträge als Formular.", "Account, sync, layouts, sorting, recommendations, and other Trakt entries as a form.")))
                    .Group(new NativeGroup("Plugin-Sortierung", "Plugin sorting")
                        .Add(new NativeField("pluginSorting", "entries", "Einträge", "Entries", NativeFieldKind.DynamicEntries)))
                    .Group(new NativeGroup("Plugin-Fenster", "Plugin windows")
                        .Add(new NativeField("pluginswindows", "entries", "Sichtbarkeit", "Visibility", NativeFieldKind.DynamicBools)))
                    .Group(new NativeGroup("Menü 0", "Menu 0")
                        .Add(new NativeField("Menu0", "entries", "Einträge", "Entries", NativeFieldKind.DynamicEntries)))
                    .Group(new NativeGroup("Menü 34", "Menu 34")
                        .Add(new NativeField("Menu34", "entries", "Einträge", "Entries", NativeFieldKind.DynamicEntries)))
                    .Group(new NativeGroup("IMDb+", "IMDb+")
                        .Add(new NativeField("IMDbPlus", "entries", "Einträge", "Entries", NativeFieldKind.DynamicEntries)))
                    .Group(new NativeGroup("Last.fm", "Last.fm")
                        .Add(new NativeField("lastfm:test", "entries", "Einträge", "Entries", NativeFieldKind.DynamicEntries)))
                    .Group(new NativeGroup("FFDShow", "FFDShow")
                        .Add(new NativeField("FFDShow", "entries", "Einträge", "Entries", NativeFieldKind.DynamicEntries)))
                    .Group(new NativeGroup("mpSettings", "mpSettings")
                        .Add(new NativeField("mpsettings", "entries", "Einträge", "Entries", NativeFieldKind.DynamicEntries)))
                    .Group(new NativeGroup("Konfigurator", "Configurator")
                        .Add(new NativeField("Configuration", "entries", "Einträge", "Entries", NativeFieldKind.DynamicEntries))),
                new NativeSection("more", "Spezialbereiche", "Special areas", "Vorschaubilder, Fernbedienungsgeräte, Medieninfo und weitere Spezialbereiche.", "Thumbnails, remote devices, media info, and other special areas.", "settings")
                    .Group(new NativeGroup("Vorschaubilder", "Thumbnails")
                        .Add(new NativeField("thumbnails", "entries", "Einträge", "Entries", NativeFieldKind.DynamicEntries)))
                    .Group(new NativeGroup("Dateimenü", "File menu")
                        .Add(new NativeField("filemenu", "entries", "Einträge", "Entries", NativeFieldKind.DynamicEntries)))
                    .Group(new NativeGroup("Benachrichtigungen", "Notifications")
                        .Add(new NativeField("guiNotifier", "entries", "Einträge", "Entries", NativeFieldKind.DynamicEntries)))
                    .Group(new NativeGroup("USBUIRT", "USBUIRT")
                        .Add(new NativeField("USBUIRT", "entries", "Einträge", "Entries", NativeFieldKind.DynamicEntries)))
                    .Group(new NativeGroup("SerialUIR", "SerialUIR")
                        .Add(new NativeField("SerialUIR", "entries", "Einträge", "Entries", NativeFieldKind.DynamicEntries)))
                    .Group(new NativeGroup("WinLIRC", "WinLIRC")
                        .Add(new NativeField("WINLIRC", "entries", "Einträge", "Entries", NativeFieldKind.DynamicEntries)))
                    .Group(new NativeGroup("RedEye", "RedEye")
                        .Add(new NativeField("RedEye", "entries", "Einträge", "Entries", NativeFieldKind.DynamicEntries)))
                    .Group(new NativeGroup("Bildschirmauswahl", "Screen selector")
                        .Add(new NativeField("screenselector", "entries", "Einträge", "Entries", NativeFieldKind.DynamicEntries)))
                    .Group(new NativeGroup("Medieninformationen", "Media info")
                        .Add(new NativeField("MediaInfo", "entries", "Einträge", "Entries", NativeFieldKind.DynamicEntries)))
                    .Group(new NativeGroup("Erweiterungen", "Extensions")
                        .Add(new NativeField("myextensions2", "entries", "Einträge", "Entries", NativeFieldKind.DynamicEntries)))
                    .Group(new NativeGroup("Video Post-Processing", "Video post-processing")
                        .Add(new NativeField("movieplayer", "entries", "Filter", "Filters", NativeFieldKind.DynamicEntries).Entries("filter")))
                    .Group(new NativeGroup("DVD Post-Processing", "DVD post-processing")
                        .Add(new NativeField("dvdplayer", "entries", "Filter", "Filters", NativeFieldKind.DynamicEntries).Entries("filter")))
                    .Group(new NativeGroup("Blu-ray Post-Processing", "Blu-ray post-processing")
                        .Add(new NativeField("bdplayer", "entries", "Filter", "Filters", NativeFieldKind.DynamicEntries).Entries("filter")))
                    .Group(new NativeGroup("TV Post-Processing", "TV post-processing")
                        .Add(new NativeField("mytv", "entries", "Filter", "Filters", NativeFieldKind.DynamicEntries).Entries("filter")))
                    .Group(new NativeGroup("Musik-Ansichtsvorlagen", "Music view templates")
                        .Add(new NativeField("mymusic", "entries", "Vorlagen", "Templates", NativeFieldKind.DynamicEntries).Entries("Name", "Date", "Size", "Track", "Duration", "Title", "Artist", "Album", "Filename", "Rating", "Year", "DiscID", "Composer", "Times Played", "File Type")))
                    .Group(new NativeGroup("Video-Ansicht", "Video view")
                        .Add(new NativeField("movies", "entries", "Ansicht", "View", NativeFieldKind.DynamicEntries).Entries("start", "last"))),
                new NativeSection("files", "Spezialdateien", "Special files", "Tastaturbelegung, Ansichten, Musik-DSP und weitere XML-Dateien bearbeiten.", "Edit key mapping, views, music DSP, and other XML files.", "file")
                    .Group(new NativeGroup("XML-Dateien", "XML files")
                        .Add(new NativeField("files", "xml", "Datei", "File", NativeFieldKind.XmlFiles, "Keymap, Ansichten, DSP und weitere XML-Dateien werden mit Backup und atomarem Schreiben gespeichert. Über Andere Datei lässt sich jede XML- oder Konfigurationsdatei öffnen.", "Keymap, views, DSP, and other XML files are saved with backup and atomic replacement. Other file opens any XML or configuration file."))),
                new NativeSection("rawxml", "XML-Rohdaten", "Raw XML", "Jeden Bereich und Eintrag der MediaPortal.xml direkt bearbeiten, auch Plugins und unbekannte Werte.", "Edit every section and entry of MediaPortal.xml, including plugin settings and unknown values.", "file")
                    .Group(new NativeGroup("Bereich", "Section")
                        .Add(new NativeField("raw", "entries", "Bereich", "Section", NativeFieldKind.RawSections, "Alle Einträge des gewählten XML-Bereichs. Unbekannte Werte bleiben erhalten.", "All entries of the selected XML section. Unknown values are preserved."))),
            };
        }
    }
}

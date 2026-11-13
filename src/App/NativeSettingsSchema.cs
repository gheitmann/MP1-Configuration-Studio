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
        Shares
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
                    .Group(new NativeGroup("Leerlauf & Autoplay", "Idle & autoplay")
                        .Add(new NativeField("general", "IdleTimer", "Leerlauf-Timer aktivieren", "Enable idle timer", NativeFieldKind.Bool))
                        .Add(new NativeField("general", "IdleTimeValue", "Leerlaufzeit in Sekunden", "Idle time in seconds", NativeFieldKind.Integer).Range(0, 3600, "s"))
                        .Add(new NativeField("general", "IdleBlanking", "Bildschirm abdunkeln", "Blank screen", NativeFieldKind.Bool))
                        .Add(new NativeField("general", "autoplay_audio", "Audio automatisch wiedergeben", "Autoplay audio", NativeFieldKind.Combo)
                            .Option("Ask", "Fragen", "Ask").Option("yes", "Ja", "Yes").Option("no", "Nein", "No"))
                        .Add(new NativeField("general", "autoplay_video", "Video automatisch wiedergeben", "Autoplay video", NativeFieldKind.Combo)
                            .Option("Ask", "Fragen", "Ask").Option("yes", "Ja", "Yes").Option("no", "Nein", "No"))
                        .Add(new NativeField("general", "autoplay_photo", "Bilder automatisch wiedergeben", "Autoplay photos", NativeFieldKind.Combo)
                            .Option("Ask", "Fragen", "Ask").Option("yes", "Ja", "Yes").Option("no", "Nein", "No")))
                    .Group(new NativeGroup("TV-Server", "TV server")
                        .Add(new NativeField("general", "wait for tvserver", "Auf TV-Server warten", "Wait for TV server", NativeFieldKind.Bool, "Beim Start auf den TV-Server warten.", "Wait for the TV server during startup."))),
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
                        .Add(new NativeField("gui", "useonlyonehome", "Nur ein Home-Screen", "Use only one home screen", NativeFieldKind.Bool)))
                    .Group(new NativeGroup("Listen & Scrollen", "Lists & scrolling")
                        .Add(new NativeField("gui", "ScrollSpeedRight", "Horizontale Scroll-Geschwindigkeit", "Horizontal scroll speed", NativeFieldKind.Integer).Range(1, 20))
                        .Add(new NativeField("gui", "ScrollSpeedDown", "Vertikale Scroll-Geschwindigkeit", "Vertical scroll speed", NativeFieldKind.Integer).Range(1, 20))
                        .Add(new NativeField("gui", "listLoopDelay", "Loop-Verzögerung", "Loop delay", NativeFieldKind.Integer, "Verzögerung beim Scrollen durch Listen.", "Delay when scrolling through lists.").Range(0, 5000, "ms"))),
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
                        .Add(new NativeField("movies", "sharepath0", "Videoordner (klassisch bearbeiten)", "Video folders (edit in classic)", NativeFieldKind.Info, "Videoordner werden auf der Seite Medienordner verwaltet.", "Video folders are managed on the Media folders page."))),
                new NativeSection("music", "Musik", "Music", "Audiowiedergabe, Playlists und Musikordner.", "Audio playback, playlists, and music folders.", "play")
                    .Group(new NativeGroup("Wiedergabe", "Playback")
                        .Add(new NativeField("audioplayer", "crossfade", "Crossfade", "Crossfade", NativeFieldKind.Integer, "Überblendzeit in Millisekunden.", "Crossfade time in milliseconds.").Range(0, 20000, "ms"))
                        .Add(new NativeField("audioplayer", "buffering", "Pufferung", "Buffering", NativeFieldKind.Integer).Range(0, 10000, "ms"))
                        .Add(new NativeField("audioplayer", "fadeOnStartStop", "Ein-/Ausblenden", "Fade in/out", NativeFieldKind.Bool))
                        .Add(new NativeField("audioplayer", "gaplessPlayback", "Gapless Playback", "Gapless playback", NativeFieldKind.Bool))
                        .Add(new NativeField("audioplayer", "wasapiExclusive", "WASAPI exklusiv", "WASAPI exclusive", NativeFieldKind.Bool))
                        .Add(new NativeField("audioplayer", "enableResume", "Wiedergabe fortsetzen", "Resume playback", NativeFieldKind.Bool)))
                    .Group(new NativeGroup("Dateien & Ordner", "Files & folders")
                        .Add(new NativeField("music", "rememberlastfolder", "Letzten Ordner merken", "Remember last folder", NativeFieldKind.Bool))
                        .Add(new NativeField("music", "AddOpticalDiskDrives", "Optische Laufwerke einbinden", "Include optical drives", NativeFieldKind.Bool))
                        .Add(new NativeField("music", "SwitchRemovableDrives", "Wechseldatenträger einbinden", "Include removable drives", NativeFieldKind.Bool))
                        .Add(new NativeField("music", "extensions", "Audio-Endungen", "Audio extensions", NativeFieldKind.Text, "Kommagetrennte Liste der Audioformate.", "Comma-separated audio extensions."))
                        .Add(new NativeField("music", "sharepath0", "Musikordner (klassisch bearbeiten)", "Music folders (edit in classic)", NativeFieldKind.Info, "Musikordner werden auf der Seite Medienordner verwaltet.", "Music folders are managed on the Media folders page."))),
                new NativeSection("folders", "Medienordner", "Media folders", "Video-, Musik- und Bilderordner verwalten.", "Manage video, music, and picture folders.", "folder")
                    .Group(new NativeGroup("Videoordner", "Video folders")
                        .Add(new NativeField("movies", "sharename", "Videoordner", "Video folders", NativeFieldKind.Shares, "Lokale oder Netzwerkordner für Videos. Scannen aktualisiert die Datenbank.", "Local or network folders for videos. Scan updates the database.")))
                    .Group(new NativeGroup("Musikordner", "Music folders")
                        .Add(new NativeField("music", "sharename", "Musikordner", "Music folders", NativeFieldKind.Shares, "Lokale oder Netzwerkordner für Musik. Scannen aktualisiert die Datenbank.", "Local or network folders for music. Scan updates the database.")))
                    .Group(new NativeGroup("Bilderordner", "Picture folders")
                        .Add(new NativeField("pictures", "sharename", "Bilderordner", "Picture folders", NativeFieldKind.Shares, "Lokale oder Netzwerkordner für Bilder.", "Local or network folders for pictures."))),
                new NativeSection("tv", "TV", "TV", "TV-Server, Aufnahmen und Live-TV-Verhalten.", "TV server, recordings, and live TV behavior.", "play")
                    .Group(new NativeGroup("TV-Server", "TV server")
                        .Add(new NativeField("tvservice", "hostname", "Server-Hostname", "Server hostname", NativeFieldKind.Text))
                        .Add(new NativeField("tvservice", "isWakeOnLanEnabled", "Wake-on-LAN", "Wake on LAN", NativeFieldKind.Bool))
                        .Add(new NativeField("tvservice", "WOLTimeOut", "WOL-Timeout", "WOL timeout", NativeFieldKind.Integer).Range(0, 120, "s"))
                        .Add(new NativeField("tvservice", "recordingpath", "Aufnahmeordner", "Recording folder", NativeFieldKind.Text, "Leer lassen für den MediaPortal-Standard.", "Leave empty for the MediaPortal default."))
                        .Add(new NativeField("tvservice", "timeshiftingpath", "Timeshift-Ordner", "Timeshift folder", NativeFieldKind.Text))
                        .Add(new NativeField("tvservice", "preferac3", "AC3 bevorzugen", "Prefer AC3", NativeFieldKind.Bool))
                        .Add(new NativeField("tvservice", "preferAudioTypeOverLang", "Audiospur vor Sprache", "Prefer audio type over language", NativeFieldKind.Bool)))
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
                        .Add(new NativeField("mytv", "confirmTimeshiftStop", "Timeshift-Stopp bestätigen", "Confirm timeshift stop", NativeFieldKind.Bool))),
                new NativeSection("radio", "Radio", "Radio", "Radio-Sender, Gruppen und Verhalten.", "Radio channels, groups, and behavior.", "play")
                    .Group(new NativeGroup("Radio", "Radio")
                        .Add(new NativeField("myradio", "hideAllChannelsGroup", "Gruppe „Alle Kanäle“ ausblenden", "Hide all-channels group", NativeFieldKind.Bool))
                        .Add(new NativeField("myradio", "rememberlastgroup", "Letzte Gruppe merken", "Remember last group", NativeFieldKind.Bool))
                        .Add(new NativeField("myradio", "autoturnonradio", "Radio automatisch starten", "Auto-start radio", NativeFieldKind.Bool))
                        .Add(new NativeField("myradio", "byindex", "Nach Index sortieren", "Sort by index", NativeFieldKind.Bool))
                        .Add(new NativeField("myradio", "showchannelnumber", "Kanalnummer anzeigen", "Show channel number", NativeFieldKind.Bool))
                        .Add(new NativeField("myradio", "channelnumbermaxlength", "Maximale Kanalnummernlänge", "Maximum channel number length", NativeFieldKind.Integer).Range(1, 6))
                        .Add(new NativeField("myradio", "layout", "Ansicht", "Layout", NativeFieldKind.Integer).Range(0, 10))
                        .Add(new NativeField("myradio", "sort", "Sortierung", "Sort order", NativeFieldKind.Text))),
                new NativeSection("codecs", "Codecs", "Codecs", "Decoder und Renderer für Video, TV und Audio.", "Decoders and renderers for video, TV, and audio.", "settings")
                    .Group(new NativeGroup("Video", "Video")
                        .Add(new NativeField("movieplayer", "autodecodersettings", "Automatische Decoder-Einstellungen", "Automatic decoder settings", NativeFieldKind.Bool))
                        .Add(new NativeField("movieplayer", "forcesourcesplitter", "Source-Splitter erzwingen", "Force source splitter", NativeFieldKind.Bool))
                        .Add(new NativeField("movieplayer", "usemoviecodects", "Movie-Codecs verwenden", "Use movie codecs", NativeFieldKind.Bool))
                        .Add(new NativeField("movieplayer", "mpeg2videocodec", "MPEG-2 Video", "MPEG-2 video", NativeFieldKind.Text))
                        .Add(new NativeField("movieplayer", "h264videocodec", "H.264 Video", "H.264 video", NativeFieldKind.Text))
                        .Add(new NativeField("movieplayer", "hevcvideocodec", "HEVC Video", "HEVC video", NativeFieldKind.Text))
                        .Add(new NativeField("movieplayer", "vc1videocodec", "VC-1 Video", "VC-1 video", NativeFieldKind.Text)))
                    .Group(new NativeGroup("Audio", "Audio")
                        .Add(new NativeField("movieplayer", "mpeg2audiocodec", "MPEG-2 Audio", "MPEG-2 audio", NativeFieldKind.Text))
                        .Add(new NativeField("movieplayer", "aacaudiocodec", "AAC Audio", "AAC audio", NativeFieldKind.Text))
                        .Add(new NativeField("movieplayer", "audiorenderer", "Audio-Renderer", "Audio renderer", NativeFieldKind.Text))
                        .Add(new NativeField("movieplayer", "splitterfilter", "Splitter", "Splitter", NativeFieldKind.Text))
                        .Add(new NativeField("movieplayer", "splitterfilefilter", "Datei-Splitter", "File splitter", NativeFieldKind.Text)))
                    .Group(new NativeGroup("TV", "TV")
                        .Add(new NativeField("mytv", "videocodec", "TV Video", "TV video", NativeFieldKind.Text))
                        .Add(new NativeField("mytv", "h264videocodec", "TV H.264", "TV H.264", NativeFieldKind.Text))
                        .Add(new NativeField("mytv", "hevcvideocodec", "TV HEVC", "TV HEVC", NativeFieldKind.Text))
                        .Add(new NativeField("mytv", "audiocodec", "TV Audio", "TV audio", NativeFieldKind.Text))
                        .Add(new NativeField("mytv", "audiorenderer", "TV Audio-Renderer", "TV audio renderer", NativeFieldKind.Text))),
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
                    .Group(new NativeGroup("DVD", "DVD")
                        .Add(new NativeField("dvdplayer", "internal", "Internen DVD-Player verwenden", "Use internal DVD player", NativeFieldKind.Bool))
                        .Add(new NativeField("dvdplayer", "usefordvd", "Externen Player für DVDs", "External player for DVDs", NativeFieldKind.Bool))
                        .Add(new NativeField("dvdplayer", "path", "Player-Pfad", "Player path", NativeFieldKind.Text))
                        .Add(new NativeField("dvdplayer", "arguments", "Player-Parameter", "Player arguments", NativeFieldKind.Text))
                        .Add(new NativeField("dvdplayer", "audiolanguage", "Audiosprache", "Audio language", NativeFieldKind.Text))
                        .Add(new NativeField("dvdplayer", "subtitlelanguage", "Untertitelsprache", "Subtitle language", NativeFieldKind.Text))
                        .Add(new NativeField("dvdplayer", "showsubtitles", "Untertitel anzeigen", "Show subtitles", NativeFieldKind.Bool))
                        .Add(new NativeField("dvdplayer", "showclosedcaptions", "Closed Captions anzeigen", "Show closed captions", NativeFieldKind.Bool))
                        .Add(new NativeField("dvdplayer", "pixelratiocorrection", "Pixel-Ratio-Korrektur", "Pixel ratio correction", NativeFieldKind.Bool))),
                new NativeSection("plugins", "Plugins", "Plugins", "Plugin-Konfiguration öffnet weiterhin die Originalfenster.", "Plugin configuration still opens the original windows.", "plugin")
                    .Group(new NativeGroup("Plugin-Konfiguration", "Plugin configuration")
                        .Add(new NativeField("plugins", "plugin", "Plugins", "Plugins", NativeFieldKind.Info, "Plugins verwalten ihre eigenen Fenster und Einstellungen. Nutze Klassisch / Plugins, um die originale Plugin-Konfiguration zu oeffnen.", "Plugins manage their own windows and settings. Use Classic / plugins to open the original plugin configuration.")))
            };
        }
    }
}

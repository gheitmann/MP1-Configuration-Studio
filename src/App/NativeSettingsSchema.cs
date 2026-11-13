using System.Collections.Generic;

namespace MP1.ConfigurationStudio
{
    internal enum NativeFieldKind
    {
        Bool,
        Text,
        Integer,
        Combo,
        Info
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
                        .Add(new NativeField("movies", "sharepath0", "Videoordner (klassisch bearbeiten)", "Video folders (edit in classic)", NativeFieldKind.Info, "Die Liste der Videoordner bleibt vorerst der klassischen Oberfläche vorbehalten.", "The video folder list remains on the classic interface for now."))),
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
                        .Add(new NativeField("music", "sharepath0", "Musikordner (klassisch bearbeiten)", "Music folders (edit in classic)", NativeFieldKind.Info, "Die Liste der Musikordner bleibt vorerst der klassischen Oberfläche vorbehalten.", "The music folder list remains on the classic interface for now.")))
            };
        }
    }
}

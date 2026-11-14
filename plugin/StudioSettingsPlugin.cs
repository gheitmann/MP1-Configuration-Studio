using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MP1.ConfigurationStudio;
using MediaPortalAction = MediaPortal.GUI.Library.Action;

namespace MP1.ConfigurationStudio.Plugin
{
    public class StudioSettingsPlugin : GUIWindow, ISetupForm, IShowPlugin
    {
        public const int WindowId = 8917;
        private const int VisibleRows = 12;
        private const int RowBase = 10;
        private const int PageUpId = 2;
        private const int PageDownId = 3;
        private const int SaveId = 4;
        private const int BackId = 5;
        private const int TitleId = 6;
        private const int StatusId = 7;

        private readonly List<Row> rows = new List<Row>();
        private readonly Dictionary<string, List<Choice>> choiceCache = new Dictionary<string, List<Choice>>(StringComparer.OrdinalIgnoreCase);
        private PluginSettingsStore settings;
        private string loadError;

        private sealed class Choice
        {
            public string Value;
            public string Label;
        }
        private NativeSection section;
        private int page;
        private int focus;
        private bool dirty;
        private string status;

        private sealed class Row
        {
            public NativeSection Section;
            public NativeField Field;
            public string Group;
            public string Text;
        }

        public StudioSettingsPlugin()
        {
            GetID = WindowId;
            try
            {
                settings = new PluginSettingsStore();
                ChoiceCatalog.Configure(Path.GetDirectoryName(MPSettings.ConfigPathName), Application.StartupPath);
            }
            catch (Exception ex) { loadError = ex.Message; }
            ShowSections();
        }

        public override bool Init()
        {
            return Load(GUIGraphicsContext.Skin + @"\MP1StudioSettings.xml");
        }

        public override bool OnMessage(GUIMessage message)
        {
            switch (message.Message)
            {
                case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
                    base.OnMessage(message);
                    ShowSections();
                    focus = 0;
                    RefreshControls();
                    return true;
                case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
                    if (!ConfirmDiscard()) return false;
                    break;
                case GUIMessage.MessageType.GUI_MSG_CLICKED:
                    OnClicked(message.SenderControlId);
                    break;
            }
            return base.OnMessage(message);
        }

        public override void OnAction(MediaPortalAction action)
        {
            switch (action.wID)
            {
                case MediaPortalAction.ActionType.ACTION_PREVIOUS_MENU:
                    GoBack();
                    return;
                case MediaPortalAction.ActionType.ACTION_MOVE_UP:
                    MoveFocus(-1);
                    return;
                case MediaPortalAction.ActionType.ACTION_MOVE_DOWN:
                    MoveFocus(1);
                    return;
                case MediaPortalAction.ActionType.ACTION_MOVE_LEFT:
                    AdjustFocused(-1);
                    return;
                case MediaPortalAction.ActionType.ACTION_MOVE_RIGHT:
                    AdjustFocused(1);
                    return;
                case MediaPortalAction.ActionType.ACTION_SELECT_ITEM:
                    ActivateFocused();
                    return;
                case MediaPortalAction.ActionType.ACTION_PAGE_UP:
                    ChangePage(-1);
                    return;
                case MediaPortalAction.ActionType.ACTION_PAGE_DOWN:
                    ChangePage(1);
                    return;
            }
            base.OnAction(action);
        }

        private void OnClicked(int controlId)
        {
            if (controlId == PageUpId) { ChangePage(-1); return; }
            if (controlId == PageDownId) { ChangePage(1); return; }
            if (controlId == SaveId) { SaveChanges(); return; }
            if (controlId == BackId) { GoBack(); return; }
            if (controlId >= RowBase && controlId < RowBase + VisibleRows)
            {
                int index = page * VisibleRows + (controlId - RowBase);
                if (index < 0 || index >= rows.Count) return;
                focus = controlId - RowBase;
                ActivateRow(rows[index]);
            }
        }

        private int VisibleCount()
        {
            return Math.Max(0, Math.Min(VisibleRows, rows.Count - page * VisibleRows));
        }

        private bool HasPreviousPage { get { return page > 0; } }
        private bool HasNextPage { get { return (page + 1) * VisibleRows < rows.Count; } }

        private int FocusCount()
        {
            return VisibleCount() + (HasPreviousPage ? 1 : 0) + (HasNextPage ? 1 : 0) + 2;
        }

        private int FocusControlId(int index)
        {
            int count = VisibleCount();
            if (index < count) return RowBase + index;
            index -= count;
            if (HasPreviousPage)
            {
                if (index == 0) return PageUpId;
                index--;
            }
            if (HasNextPage)
            {
                if (index == 0) return PageDownId;
                index--;
            }
            return index <= 0 ? SaveId : BackId;
        }

        private void SelectFocused()
        {
            int count = FocusCount();
            if (count <= 0) return;
            if (focus < 0) focus = 0;
            if (focus >= count) focus = count - 1;
            int id = FocusControlId(focus);
            GUIControl control = GetControl(id);
            if (control == null || !control.Visible) return;
            var message = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS, GetID, 0, id, 0, 0, null);
            OnMessage(message);
        }

        private void MoveFocus(int direction)
        {
            int count = FocusCount();
            if (count <= 0) return;
            focus = (focus + direction + count) % count;
            SelectFocused();
        }

        private void ChangePage(int direction)
        {
            int pages = Math.Max(1, (rows.Count + VisibleRows - 1) / VisibleRows);
            int target = Math.Max(0, Math.Min(pages - 1, page + direction));
            if (target == page) return;
            page = target;
            focus = 0;
            RefreshControls();
        }

        private int FocusedRowIndex()
        {
            int count = VisibleCount();
            return focus >= 0 && focus < count ? page * VisibleRows + focus : -1;
        }

        private void AdjustFocused(int direction)
        {
            if (settings == null)
            {
                ShowInfo(LocalizationText("Konfiguration nicht verfügbar", "Configuration unavailable"), loadError ?? String.Empty);
                return;
            }
            int index = FocusedRowIndex();
            if (index < 0) return;
            Row row = rows[index];
            if (row.Field == null) return;
            if (row.Field.Kind == NativeFieldKind.Bool)
            {
                settings.SetBool(row.Field.Section, row.Field.Entry, !settings.GetBool(row.Field.Section, row.Field.Entry, false));
                dirty = true;
                RefreshControls();
                return;
            }
            List<Choice> choices = ChoicesFor(row.Field);
            if (choices.Count > 0)
            {
                CycleChoice(row.Field, choices, direction);
                return;
            }
            if (row.Field.Kind == NativeFieldKind.Integer)
            {
                int value = settings.GetInt(row.Field.Section, row.Field.Entry, row.Field.Minimum) + direction;
                value = Math.Max(row.Field.Minimum, Math.Min(row.Field.Maximum, value));
                settings.Set(row.Field.Section, row.Field.Entry, value.ToString(CultureInfo.InvariantCulture));
                dirty = true;
                RefreshControls();
            }
        }

        private void ActivateFocused()
        {
            int count = VisibleCount();
            if (focus >= 0 && focus < count)
            {
                ActivateRow(rows[page * VisibleRows + focus]);
                return;
            }
            int id = FocusControlId(focus);
            if (id == PageUpId) ChangePage(-1);
            else if (id == PageDownId) ChangePage(1);
            else if (id == SaveId) SaveChanges();
            else if (id == BackId) GoBack();
        }

        private void ActivateRow(Row row)
        {
            if (row.Section != null)
            {
                section = row.Section;
                page = 0;
                ShowFields(section);
                focus = 0;
                RefreshControls();
                return;
            }
            NativeField field = row.Field;
            if (field == null) return;
            if (settings == null)
            {
                ShowInfo(LocalizationText("Konfiguration nicht verfügbar", "Configuration unavailable"), loadError ?? String.Empty);
                return;
            }
            if (field.Kind == NativeFieldKind.Bool)
            {
                settings.SetBool(field.Section, field.Entry, !settings.GetBool(field.Section, field.Entry, false));
                dirty = true;
            }
            else
            {
                List<Choice> choices = ChoicesFor(field);
                if (choices.Count > 0)
                {
                    ShowChoice(field);
                }
                else if (field.Kind == NativeFieldKind.Integer)
                {
                    string value;
                    if (EditValue(GetValue(field), true, out value))
                    {
                        int number;
                        if (!Int32.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out number)) number = field.Minimum;
                        number = Math.Max(field.Minimum, Math.Min(field.Maximum, number));
                        settings.Set(field.Section, field.Entry, number.ToString(CultureInfo.InvariantCulture));
                        dirty = true;
                    }
                }
                else if (field.Kind == NativeFieldKind.Text || field.Kind == NativeFieldKind.Folder)
                {
                    string value;
                    if (EditValue(GetValue(field), false, out value))
                    {
                        settings.Set(field.Section, field.Entry, value);
                        dirty = true;
                    }
                }
                else
                {
                    ShowInfo(LocalizationText("Im Studio bearbeiten", "Edit in Studio"),
                        LocalizationText("Dieser Bereich wird im MP1 Configuration Studio bearbeitet.", "This area is edited in MP1 Configuration Studio."));
                }
            }
            RefreshControls();
        }

        private bool ShowChoice(NativeField field)
        {
            List<Choice> source = ChoicesFor(field);
            if (source.Count == 0) return false;
            var dialog = (GUIDialogSelect)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_SELECT);
            if (dialog == null) return false;
            string current = GetValue(field);
            var choices = new List<Choice>(source);
            int selected = choices.FindIndex(choice => String.Equals(choice.Value, current, StringComparison.OrdinalIgnoreCase));
            if (selected > 0)
            {
                Choice active = choices[selected];
                choices.RemoveAt(selected);
                choices.Insert(0, active);
            }
            dialog.SetHeading(LocalizationEnglish ? field.LabelEn : field.LabelDe);
            foreach (Choice choice in choices) dialog.Add(choice.Label);
            dialog.SelectedLabel = 0;
            dialog.DoModal(GetID);
            int index = dialog.SelectedLabel;
            if (index < 0 || index >= choices.Count) return true;
            settings.Set(field.Section, field.Entry, choices[index].Value);
            dirty = true;
            status = LocalizationText("Ausgewählt: ", "Selected: ") + choices[index].Label;
            return true;
        }

        private void CycleChoice(NativeField field, List<Choice> choices, int direction)
        {
            string current = GetValue(field);
            int index = choices.FindIndex(choice => String.Equals(choice.Value, current, StringComparison.OrdinalIgnoreCase));
            if (index < 0) index = 0;
            index = (index + direction + choices.Count) % choices.Count;
            settings.Set(field.Section, field.Entry, choices[index].Value);
            dirty = true;
            status = LocalizationText("Ausgewählt: ", "Selected: ") + choices[index].Label;
        }

        private List<Choice> ChoicesFor(NativeField field)
        {
            if (field == null) return new List<Choice>();
            if (field.Kind == NativeFieldKind.Combo)
            {
                var options = new List<Choice>();
                foreach (NativeOption option in field.Options)
                    options.Add(new Choice { Value = option.Value, Label = LocalizationEnglish ? option.LabelEn : option.LabelDe });
                return options;
            }
            string key = (field.Section ?? string.Empty) + "/" + (field.Entry ?? string.Empty);
            List<Choice> cached;
            if (choiceCache.TryGetValue(key, out cached)) return cached;
            List<ChoiceOption> catalog = ChoiceCatalog.Resolve(field.Section, field.Entry);
            if (catalog.Count > 0)
            {
                var catalogChoices = new List<Choice>();
                foreach (ChoiceOption option in catalog) catalogChoices.Add(new Choice { Value = option.Value, Label = option.Label });
                choiceCache[key] = catalogChoices;
                return catalogChoices;
            }
            List<Choice> choices = null;
            if (String.Equals(key, "skin/name", StringComparison.OrdinalIgnoreCase)) choices = SkinChoices();
            else if (String.Equals(key, "gui/language", StringComparison.OrdinalIgnoreCase)) choices = LanguageChoices();
            else if (String.Equals(key, "bdplayer/regioncode", StringComparison.OrdinalIgnoreCase)) choices = RegionChoices();
            else if (String.Equals(key, "subtitles/language", StringComparison.OrdinalIgnoreCase)
                || String.Equals(key, "movieplayer/audiolanguage", StringComparison.OrdinalIgnoreCase)
                || String.Equals(key, "bdplayer/audiolanguage", StringComparison.OrdinalIgnoreCase)
                || String.Equals(key, "bdplayer/subtitlelanguage", StringComparison.OrdinalIgnoreCase)) choices = LanguageCodeChoices();
            else if (String.Equals(key, "subtitles/fontface", StringComparison.OrdinalIgnoreCase)) choices = FontChoices();
            if (choices == null) choices = new List<Choice>();
            choiceCache[key] = choices;
            return choices;
        }

        private bool HasChoices(NativeField field)
        {
            return field != null && ChoicesFor(field).Count > 0;
        }

        private List<Choice> SkinChoices()
        {
            var list = new List<Choice>();
            try
            {
                string root = Path.Combine(Path.GetDirectoryName(MPSettings.ConfigPathName), "skin");
                if (Directory.Exists(root))
                {
                    foreach (string directory in Directory.GetDirectories(root))
                    {
                        string name = Path.GetFileName(directory);
                        if (String.IsNullOrEmpty(name) || name.StartsWith(".", StringComparison.Ordinal)) continue;
                        if (!File.Exists(Path.Combine(directory, "references.xml")) && !Directory.Exists(Path.Combine(directory, "Media"))) continue;
                        list.Add(new Choice { Value = name, Label = name });
                    }
                }
            }
            catch { }
            list.Sort((left, right) => String.Compare(left.Label, right.Label, StringComparison.CurrentCultureIgnoreCase));
            return list;
        }

        private List<Choice> LanguageChoices()
        {
            var list = new List<Choice>();
            try
            {
                string root = Path.Combine(Path.GetDirectoryName(MPSettings.ConfigPathName), "language");
                if (Directory.Exists(root))
                {
                    foreach (string file in Directory.GetFiles(root, "strings_*.xml"))
                    {
                        try
                        {
                            var document = new XmlDocument { XmlResolver = null };
                            document.Load(file);
                            string name = document.DocumentElement == null ? null : document.DocumentElement.GetAttribute("name");
                            string code = Path.GetFileNameWithoutExtension(file);
                            if (code.StartsWith("strings_", StringComparison.OrdinalIgnoreCase)) code = code.Substring("strings_".Length);
                            if (String.IsNullOrEmpty(name)) name = code;
                            list.Add(new Choice { Value = name, Label = name + " (" + code + ")" });
                        }
                        catch { }
                    }
                }
            }
            catch { }
            list.Sort((left, right) => String.Compare(left.Label, right.Label, StringComparison.CurrentCultureIgnoreCase));
            return list;
        }

        private static List<Choice> RegionChoices()
        {
            return new List<Choice>
            {
                new Choice { Value = "A", Label = "A (Americas)" },
                new Choice { Value = "B", Label = "B (Europe, Africa, Australia)" },
                new Choice { Value = "C", Label = "C (Asia, Russia)" }
            };
        }

        private static List<Choice> FontChoices()
        {
            var list = new List<Choice>();
            try
            {
                using (var fonts = new InstalledFontCollection())
                {
                    foreach (FontFamily family in fonts.Families)
                        list.Add(new Choice { Value = family.Name, Label = family.Name });
                }
            }
            catch { }
            list.Sort((left, right) => String.Compare(left.Label, right.Label, StringComparison.CurrentCultureIgnoreCase));
            return list;
        }

        private static List<Choice> LanguageCodeChoices()
        {
            var codes = new[]
            {
                new[] { "de", "German" }, new[] { "en", "English" }, new[] { "fr", "French" },
                new[] { "it", "Italian" }, new[] { "es", "Spanish" }, new[] { "nl", "Dutch" },
                new[] { "pl", "Polish" }, new[] { "pt", "Portuguese" }, new[] { "ru", "Russian" },
                new[] { "sv", "Swedish" }, new[] { "da", "Danish" }, new[] { "fi", "Finnish" },
                new[] { "no", "Norwegian" }, new[] { "cs", "Czech" }, new[] { "hu", "Hungarian" },
                new[] { "tr", "Turkish" }, new[] { "ja", "Japanese" }, new[] { "zh", "Chinese" }
            };
            var list = new List<Choice>();
            foreach (string[] code in codes) list.Add(new Choice { Value = code[0], Label = code[0] + " – " + code[1] });
            return list;
        }

        private string GetValue(NativeField field)
        {
            if (settings == null) return string.Empty;
            if (field.Kind == NativeFieldKind.Bool) return settings.GetBool(field.Section, field.Entry, false) ? "yes" : "no";
            if (field.Kind == NativeFieldKind.Integer) return settings.GetInt(field.Section, field.Entry, field.Minimum).ToString(CultureInfo.InvariantCulture);
            return settings.Get(field.Section, field.Entry, string.Empty);
        }

        private string DisplayValue(NativeField field)
        {
            if (field.Kind == NativeFieldKind.Shares || field.Kind == NativeFieldKind.DynamicBools
                || field.Kind == NativeFieldKind.DynamicEntries || field.Kind == NativeFieldKind.XmlFiles
                || field.Kind == NativeFieldKind.RawSections || field.Kind == NativeFieldKind.Classic
                || field.Kind == NativeFieldKind.Keymap || field.Kind == NativeFieldKind.Views
                || field.Kind == NativeFieldKind.Dsp || field.Kind == NativeFieldKind.TvServer
                || field.Kind == NativeFieldKind.PluginConfig)
                return LocalizationText("im Studio bearbeiten", "edit in Studio");
            if (field.Kind == NativeFieldKind.Info) return LocalizationText("Hinweis", "note");
            string value = GetValue(field);
            if (field.Kind == NativeFieldKind.Combo)
            {
                NativeOption option = field.Options.FirstOrDefault(candidate => String.Equals(candidate.Value, value, StringComparison.OrdinalIgnoreCase));
                if (option != null) value = LocalizationEnglish ? option.LabelEn : option.LabelDe;
            }
            if (field.Kind == NativeFieldKind.Bool) value = String.Equals(value, "yes", StringComparison.OrdinalIgnoreCase) ? LocalizationText("Ja", "Yes") : LocalizationText("Nein", "No");
            if (String.IsNullOrEmpty(value)) value = LocalizationText("(leer)", "(empty)");
            if (value.Length > 80) value = value.Substring(0, 77) + "…";
            if (HasChoices(field)) value += "  ▸";
            return value;
        }

        private static bool LocalizationEnglish
        {
            get { return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "en"; }
        }

        private static string LocalizationText(string german, string english)
        {
            return LocalizationEnglish ? english : german;
        }

        private void ShowSections()
        {
            section = null;
            rows.Clear();
            foreach (NativeSection candidate in NativeSettingsSchema.Sections())
            {
                if (candidate.Id == "files" || candidate.Id == "rawxml" || candidate.Id == "keymap"
                    || candidate.Id == "musicviews" || candidate.Id == "videoviews" || candidate.Id == "musicdsp") continue;
                rows.Add(new Row { Section = candidate, Text = LocalizationEnglish ? candidate.TitleEn : candidate.TitleDe });
            }
            page = 0;
        }

        private void ShowFields(NativeSection value)
        {
            rows.Clear();
            foreach (NativeGroup group in value.Groups)
            {
                string groupTitle = LocalizationEnglish ? group.TitleEn : group.TitleDe;
                foreach (NativeField field in group.Fields)
                {
                    rows.Add(new Row { Field = field, Group = groupTitle, Text = RowText(field, groupTitle) });
                }
            }
        }

        private string RowText(NativeField field, string groupTitle)
        {
            string label = LocalizationEnglish ? field.LabelEn : field.LabelDe;
            return groupTitle + " · " + label + ": " + DisplayValue(field);
        }

        private void RefreshControls()
        {
            GUILabelControl title = GetControl(TitleId) as GUILabelControl;
            if (title != null) title.Label = section == null ? "MP1 Configuration Studio" : (LocalizationEnglish ? section.TitleEn : section.TitleDe);
            GUILabelControl statusLabel = GetControl(StatusId) as GUILabelControl;
            int pages = Math.Max(1, (rows.Count + VisibleRows - 1) / VisibleRows);
            bool hasPreviousPage = page > 0;
            bool hasNextPage = (page + 1) * VisibleRows < rows.Count;
            int visibleCount = Math.Max(0, Math.Min(VisibleRows, rows.Count - page * VisibleRows));
            string pageText = String.Format(CultureInfo.InvariantCulture, "{0} {1}/{2}", LocalizationText("Seite", "Page"), page + 1, pages);
            string statusText = String.IsNullOrEmpty(loadError) ? (status ?? pageText) : (LocalizationText("Konfiguration nicht verfügbar", "Configuration unavailable") + ": " + loadError);
            if (statusLabel != null) statusLabel.Label = statusText + (dirty ? "  ·  " + LocalizationText("ungespeichert", "unsaved") : string.Empty);

            for (int i = 0; i < VisibleRows; i++)
            {
                GUIButtonControl button = GetControl(RowBase + i) as GUIButtonControl;
                if (button == null) continue;
                int index = page * VisibleRows + i;
                if (index < rows.Count)
                {
                    Row row = rows[index];
                    if (row.Field != null) row.Text = RowText(row.Field, row.Group);
                    button.Visible = true;
                    button.Label = row.Text;
                    button.NavigateUp = i == 0 ? (hasPreviousPage ? PageUpId : RowBase) : RowBase + i - 1;
                    button.NavigateDown = i == visibleCount - 1 ? (hasNextPage ? PageDownId : RowBase + i) : RowBase + i + 1;
                    button.NavigateLeft = RowBase + i;
                    button.NavigateRight = RowBase + i;
                }
                else
                {
                    button.Visible = false;
                    button.Label = string.Empty;
                }
            }

            GUIButtonControl pageUp = GetControl(PageUpId) as GUIButtonControl;
            GUIButtonControl pageDown = GetControl(PageDownId) as GUIButtonControl;
            if (pageUp != null) pageUp.Visible = hasPreviousPage;
            if (pageDown != null) pageDown.Visible = hasNextPage;
            GUIButtonControl save = GetControl(SaveId) as GUIButtonControl;
            if (save != null) save.Label = LocalizationText("Sichern", "Save");
            GUIButtonControl back = GetControl(BackId) as GUIButtonControl;
            if (back != null) back.Label = section == null ? LocalizationText("Schließen", "Close") : LocalizationText("Zurück", "Back");
            SelectFocused();
        }

        private void GoBack()
        {
            if (section != null)
            {
                section = null;
                ShowSections();
                focus = 0;
                RefreshControls();
                return;
            }
            if (!ConfirmDiscard()) return;
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_HOME);
        }

        private bool ConfirmDiscard()
        {
            if (!dirty) return true;
            GUIDialogYesNo dialog = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
            if (dialog == null) return true;
            dialog.SetHeading(LocalizationText("Ungespeicherte Änderungen", "Unsaved changes"));
            dialog.SetLine(1, LocalizationText("Änderungen jetzt speichern?", "Save changes now?"));
            dialog.SetLine(2, string.Empty);
            dialog.SetLine(3, string.Empty);
            dialog.DoModal(GetID);
            if (!dialog.IsConfirmed) return true;
            if (SaveChanges()) return true;
            return false;
        }

        private bool SaveChanges()
        {
            if (!dirty)
            {
                status = LocalizationText("Keine Änderungen", "No changes");
                RefreshControls();
                return true;
            }
            try
            {
                if (settings == null) throw new InvalidOperationException(loadError ?? "MediaPortal.xml is unavailable.");
                string backup = settings.Save();
                dirty = false;
                status = LocalizationText("Gespeichert · Neustart erforderlich · Sicherung: ", "Saved · restart required · backup: ") + Path.GetFileName(backup);
                RefreshControls();
                return true;
            }
            catch (Exception ex)
            {
                status = LocalizationText("Speichern fehlgeschlagen", "Save failed");
                ShowInfo(status, ex.Message);
                RefreshControls();
                return false;
            }
        }

        private bool EditValue(string current, bool numeric, out string result)
        {
            result = current;
            VirtualKeyboard keyboard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_VIRTUAL_KEYBOARD);
            if (keyboard == null) return false;
            keyboard.Reset();
            keyboard.Text = current;
            keyboard.IsNumeric = numeric;
            keyboard.DoModal(GetID);
            if (!keyboard.IsConfirmed) return false;
            result = keyboard.Text;
            return true;
        }

        private void ShowInfo(string heading, string line)
        {
            GUIDialogOK dialog = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
            if (dialog == null) return;
            dialog.SetHeading(heading);
            dialog.SetLine(1, line ?? string.Empty);
            dialog.SetLine(2, string.Empty);
            dialog.SetLine(3, string.Empty);
            dialog.DoModal(GetID);
        }

        public string PluginName() { return "MP1 Configuration Studio"; }
        public string Description() { return LocalizationText("MediaPortal-Konfiguration direkt in der Oberfläche bearbeiten.", "Edit MediaPortal configuration inside the interface."); }
        public string Author() { return "MP1 Configuration Studio"; }
        public void ShowPlugin() { }
        public bool CanEnable() { return true; }
        public int GetWindowId() { return WindowId; }
        public bool DefaultEnabled() { return false; }
        public bool HasSetup() { return false; }
        public bool ShowDefaultHome() { return false; }

        public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
        {
            strButtonText = PluginName();
            strButtonImage = string.Empty;
            strButtonImageFocus = string.Empty;
            strPictureImage = string.Empty;
            return true;
        }
    }
}

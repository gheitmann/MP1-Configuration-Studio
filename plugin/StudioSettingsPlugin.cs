using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Forms;
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
        private PluginSettingsStore settings;
        private string loadError;
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
            try { settings = new PluginSettingsStore(); }
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
            }
            else if (row.Field.Kind == NativeFieldKind.Combo)
            {
                CycleCombo(row.Field, direction);
            }
            else if (row.Field.Kind == NativeFieldKind.Integer)
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
            else if (field.Kind == NativeFieldKind.Combo)
            {
                CycleCombo(field, 1);
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
            RefreshControls();
        }

        private void CycleCombo(NativeField field, int direction)
        {
            if (field.Options.Count == 0) return;
            string current = settings.Get(field.Section, field.Entry, string.Empty);
            int index = field.Options.FindIndex(option => String.Equals(option.Value, current, StringComparison.OrdinalIgnoreCase));
            if (index < 0) index = 0;
            index = (index + direction + field.Options.Count) % field.Options.Count;
            settings.Set(field.Section, field.Entry, field.Options[index].Value);
            dirty = true;
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

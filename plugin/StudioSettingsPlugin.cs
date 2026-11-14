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
        private MPSettings settings;
        private NativeSection section;
        private int page;
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
            settings = new MPSettings();
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
                case MediaPortalAction.ActionType.ACTION_MOVE_LEFT:
                case MediaPortalAction.ActionType.ACTION_MOVE_RIGHT:
                    AdjustFocused(action.wID == MediaPortalAction.ActionType.ACTION_MOVE_LEFT ? -1 : 1);
                    return;
            }
            base.OnAction(action);
        }

        private void OnClicked(int controlId)
        {
            if (controlId == PageUpId) { if (page > 0) { page--; selectedRow(0); RefreshControls(); } return; }
            if (controlId == PageDownId) { if ((page + 1) * VisibleRows < rows.Count) { page++; selectedRow(0); RefreshControls(); } return; }
            if (controlId == SaveId) { SaveChanges(); return; }
            if (controlId == BackId) { GoBack(); return; }
            if (controlId >= RowBase && controlId < RowBase + VisibleRows)
            {
                int index = page * VisibleRows + (controlId - RowBase);
                if (index < 0 || index >= rows.Count) return;
                ActivateRow(rows[index]);
            }
        }

        private void selectedRow(int value)
        {
            int id = RowBase + Math.Max(0, Math.Min(VisibleRows - 1, value));
            GUIControl.SelectControl(GetID, id);
        }

        private int FocusedIndex()
        {
            int focus = GetFocusControlId();
            if (focus < RowBase || focus >= RowBase + VisibleRows) return -1;
            int index = page * VisibleRows + (focus - RowBase);
            return index < rows.Count ? index : -1;
        }

        private void AdjustFocused(int direction)
        {
            int index = FocusedIndex();
            if (index < 0) return;
            Row row = rows[index];
            if (row.Field == null) return;
            if (row.Field.Kind == NativeFieldKind.Bool)
            {
                settings.SetValueAsBool(row.Field.Section, row.Field.Entry, !settings.GetValueAsBool(row.Field.Section, row.Field.Entry, false));
                dirty = true;
                RefreshControls();
            }
            else if (row.Field.Kind == NativeFieldKind.Combo)
            {
                CycleCombo(row.Field, direction);
            }
            else if (row.Field.Kind == NativeFieldKind.Integer)
            {
                int value = settings.GetValueAsInt(row.Field.Section, row.Field.Entry, row.Field.Minimum) + direction;
                value = Math.Max(row.Field.Minimum, Math.Min(row.Field.Maximum, value));
                settings.SetValue(row.Field.Section, row.Field.Entry, value.ToString(CultureInfo.InvariantCulture));
                dirty = true;
                RefreshControls();
            }
        }

        private void ActivateRow(Row row)
        {
            if (row.Section != null)
            {
                section = row.Section;
                page = 0;
                ShowFields(section);
                RefreshControls();
                selectedRow(0);
                return;
            }
            NativeField field = row.Field;
            if (field == null) return;
            if (field.Kind == NativeFieldKind.Bool)
            {
                settings.SetValueAsBool(field.Section, field.Entry, !settings.GetValueAsBool(field.Section, field.Entry, false));
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
                    settings.SetValue(field.Section, field.Entry, number.ToString(CultureInfo.InvariantCulture));
                    dirty = true;
                }
            }
            else if (field.Kind == NativeFieldKind.Text || field.Kind == NativeFieldKind.Folder)
            {
                string value;
                if (EditValue(GetValue(field), false, out value))
                {
                    settings.SetValue(field.Section, field.Entry, value);
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
            string current = settings.GetValueAsString(field.Section, field.Entry, string.Empty);
            int index = field.Options.FindIndex(option => String.Equals(option.Value, current, StringComparison.OrdinalIgnoreCase));
            if (index < 0) index = 0;
            index = (index + direction + field.Options.Count) % field.Options.Count;
            settings.SetValue(field.Section, field.Entry, field.Options[index].Value);
            dirty = true;
        }

        private string GetValue(NativeField field)
        {
            if (field.Kind == NativeFieldKind.Bool) return settings.GetValueAsBool(field.Section, field.Entry, false) ? "yes" : "no";
            if (field.Kind == NativeFieldKind.Integer) return settings.GetValueAsInt(field.Section, field.Entry, field.Minimum).ToString(CultureInfo.InvariantCulture);
            return settings.GetValueAsString(field.Section, field.Entry, string.Empty);
        }

        private string DisplayValue(NativeField field)
        {
            if (field.Kind == NativeFieldKind.Shares || field.Kind == NativeFieldKind.DynamicBools) return LocalizationText("im Studio bearbeiten", "edit in Studio");
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
                    string label = LocalizationEnglish ? field.LabelEn : field.LabelDe;
                    rows.Add(new Row { Field = field, Group = groupTitle, Text = groupTitle + " · " + label + ": " + DisplayValue(field) });
                }
            }
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
            if (statusLabel != null) statusLabel.Label = (status ?? pageText) + (dirty ? "  ·  " + LocalizationText("ungespeichert", "unsaved") : string.Empty);

            for (int i = 0; i < VisibleRows; i++)
            {
                GUIButtonControl button = GetControl(RowBase + i) as GUIButtonControl;
                if (button == null) continue;
                int index = page * VisibleRows + i;
                if (index < rows.Count)
                {
                    button.Visible = true;
                    button.Label = rows[index].Text;
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
        }

        private void GoBack()
        {
            if (section != null)
            {
                section = null;
                ShowSections();
                RefreshControls();
                selectedRow(0);
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
                string backup = CreateBackup();
                MediaPortal.Profile.Settings.SaveCache();
                dirty = false;
                status = LocalizationText("Gespeichert · Sicherung: ", "Saved · backup: ") + Path.GetFileName(backup);
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

        private static string CreateBackup()
        {
            string root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MP1 Configuration Studio", "backups");
            Directory.CreateDirectory(root);
            string path = Path.Combine(root, "mp-plugin-" + DateTime.Now.ToString("yyyyMMdd-HHmmss-fff", CultureInfo.InvariantCulture) + ".zip");
            using (ZipArchive archive = ZipFile.Open(path, ZipArchiveMode.Create))
            {
                ZipArchiveEntry entry = archive.CreateEntry("MediaPortal.xml", CompressionLevel.Optimal);
                using (Stream output = entry.Open())
                using (Stream input = File.OpenRead(MPSettings.ConfigPathName))
                    input.CopyTo(output);
            }
            return path;
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

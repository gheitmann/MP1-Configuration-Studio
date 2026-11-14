using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace MP1.ConfigurationStudio
{
    internal sealed class NativeViewsEditor : UserControl, IStudioFileEditor
    {
        private sealed class ViewModel
        {
            public XmlElement Definition;
            public XmlElement Item;
            public string Id;
            public string Label;
            public List<XmlElement> Filters = new List<XmlElement>();
        }

        private readonly string path;
        private readonly bool video;
        private readonly float scale;
        private readonly Palette palette = Palette.For(true);
        private readonly ListBox viewList = new ListBox();
        private readonly Panel detail = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
        private readonly FlowLayoutPanel detailFlow = new FlowLayoutPanel { Dock = DockStyle.Top, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true, BackColor = Palette.For(true).Background, Padding = new Padding(0) };
        private readonly Label status = new Label();
        private readonly List<ViewModel> views = new List<ViewModel>();
        private readonly Dictionary<string, string> strings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, XmlElement> byId = new Dictionary<string, XmlElement>(StringComparer.OrdinalIgnoreCase);
        private XmlDocument document;
        private Encoding encoding = new UTF8Encoding(false);
        private bool dirty;

        public NativeViewsEditor(string configDirectory, string fileName, string language, float scale)
        {
            this.path = Path.Combine(configDirectory, fileName);
            this.video = String.Equals(fileName, "VideoViews.xml", StringComparison.OrdinalIgnoreCase);
            this.scale = scale;
            BackColor = palette.Background;
            ForeColor = palette.Text;
            var body = Typography.Create(10f * scale, FontStyle.Regular);
            LoadStrings(configDirectory, language);

            var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = Px(38), FlowDirection = FlowDirection.LeftToRight, WrapContents = false, BackColor = palette.Background, Padding = new Padding(0, Px(4), 0, 0) };
            var reload = ButtonFor(Localization.IsEnglish ? "Reload" : "Neu laden", delegate { Reload(); }, body);
            var save = ButtonFor(Localization.IsEnglish ? "Save" : "Speichern", delegate { SaveFile(); }, body);
            var up = ButtonFor("▲", delegate { MoveView(-1); }, body);
            var down = ButtonFor("▼", delegate { MoveView(1); }, body);
            up.Width = Px(44);
            down.Width = Px(44);
            top.Controls.Add(reload);
            top.Controls.Add(save);
            top.Controls.Add(up);
            top.Controls.Add(down);
            status.AutoSize = false;
            status.Width = Px(420);
            status.Height = Px(28);
            status.AutoEllipsis = true;
            status.TextAlign = ContentAlignment.MiddleLeft;
            status.Font = Typography.Create(9f * scale, FontStyle.Regular);
            status.ForeColor = palette.Muted;
            top.Controls.Add(status);

            var split = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = palette.Background };
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Px(300)));
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            var left = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 1, BackColor = palette.Background, Margin = new Padding(0, 0, Px(10), 0) };
            left.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            viewList.Dock = DockStyle.Fill;
            viewList.BackColor = palette.Input;
            viewList.ForeColor = palette.Text;
            viewList.BorderStyle = BorderStyle.FixedSingle;
            viewList.Font = body;
            viewList.DrawMode = DrawMode.OwnerDrawFixed;
            viewList.ItemHeight = Px(34);
            viewList.IntegralHeight = false;
            viewList.DrawItem += DrawViewItem;
            viewList.SelectedIndexChanged += delegate { BuildDetail(); };
            left.Controls.Add(viewList, 0, 0);
            split.Controls.Add(left, 0, 0);
            split.Controls.Add(detail, 1, 0);
            detail.Controls.Add(detailFlow);
            detail.Resize += delegate
            {
                int width = Math.Max(Px(480), detail.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - Px(12));
                detailFlow.MinimumSize = new Size(width, 0);
                detailFlow.MaximumSize = new Size(width, 0);
                detailFlow.Width = width;
                ResizeCards();
            };

            Controls.Add(split);
            Controls.Add(top);
            LoadFile();
        }

        public bool HasUnsavedChanges { get { return dirty; } }

        public bool ConfirmDiscardChanges()
        {
            if (!dirty) return true;
            return MessageBox.Show(FindForm(), Localization.IsEnglish ? "Discard unsaved view changes?" : "Ungespeicherte Ansichtsänderungen verwerfen?", "MP1 Configuration Studio", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes;
        }

        private int Px(float value) { return (int)Math.Round(value * scale); }

        private static string L(string de, string en) { return Localization.IsEnglish ? en : de; }

        private Button ButtonFor(string text, EventHandler click, Font font)
        {
            var button = new Button { Text = text, Font = font, Height = Px(28), MinimumSize = new Size(Px(60), Px(28)), AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(Px(8), 0, Px(8), 0), FlatStyle = FlatStyle.Flat, BackColor = palette.Panel, ForeColor = palette.Text, Margin = new Padding(0, 0, Px(6), 0) };
            button.FlatAppearance.BorderColor = palette.Border;
            button.Click += click;
            return button;
        }

        private void DrawViewItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= views.Count) return;
            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            using (var background = new SolidBrush(selected ? palette.Accent : palette.Input))
                e.Graphics.FillRectangle(background, e.Bounds);
            if (selected)
                using (var accent = new SolidBrush(palette.Accent))
                    e.Graphics.FillRectangle(accent, e.Bounds.Left, e.Bounds.Top, Px(3), e.Bounds.Height);
            using (var text = new SolidBrush(palette.Text))
                e.Graphics.DrawString(views[e.Index].Label, e.Font, text, e.Bounds.Left + Px(10), e.Bounds.Top + Px(7));
        }

        private void LoadStrings(string configDirectory, string language)
        {
            string code = LanguageCode(language);
            string folder = Path.Combine(configDirectory, "language");
            string file = Path.Combine(folder, "strings_" + code + ".xml");
            if (!File.Exists(file)) file = Path.Combine(folder, "strings_en.xml");
            if (!File.Exists(file)) return;
            try
            {
                var xml = new XmlDocument { XmlResolver = null };
                xml.Load(file);
                foreach (XmlNode node in xml.SelectNodes("//*[local-name()='String']"))
                {
                    var element = node as XmlElement;
                    if (element == null) continue;
                    string id = element.GetAttribute("id");
                    if (String.IsNullOrEmpty(id) || strings.ContainsKey(id)) continue;
                    strings[id] = (element.GetAttribute("prefix") ?? string.Empty) + element.InnerText;
                }
            }
            catch { }
        }

        private static string LanguageCode(string language)
        {
            if (String.IsNullOrWhiteSpace(language)) return "en";
            var known = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "German", "de" }, { "Deutsch", "de" }, { "English", "en" }, { "French", "fr" }, { "Français", "fr" },
                { "Spanish", "es" }, { "Español", "es" }, { "Italian", "it" }, { "Italiano", "it" }, { "Dutch", "nl" },
                { "Nederlands", "nl" }, { "Swedish", "sv" }, { "Svenska", "sv" }, { "Danish", "da" }, { "Dansk", "da" },
                { "Norwegian", "no" }, { "Norsk", "no" }, { "Finnish", "fi" }, { "Suomi", "fi" }, { "Polish", "pl" },
                { "Polski", "pl" }, { "Russian", "ru" }, { "Portuguese", "pt" }, { "Czech", "cs" }, { "Hungarian", "hu" }
            };
            string result;
            if (known.TryGetValue(language.Trim(), out result)) return result;
            string trimmed = language.Trim();
            return trimmed.Length >= 2 ? trimmed.Substring(0, 2).ToLowerInvariant() : "en";
        }

        private string Localized(string id)
        {
            string value;
            if (id != null && strings.TryGetValue(id, out value) && !String.IsNullOrWhiteSpace(value)) return value + "  ·  #" + id;
            return "#" + id;
        }

        private void LoadFile()
        {
            try
            {
                if (!File.Exists(path))
                {
                    status.Text = (Localization.IsEnglish ? "File not found: " : "Datei nicht gefunden: ") + path;
                    return;
                }
                string text = NativeXmlFileStore.ReadText(path, out encoding);
                document = new XmlDocument { XmlResolver = null, PreserveWhitespace = true };
                document.LoadXml(text);
                byId.Clear();
                foreach (XmlNode node in document.SelectNodes("//*[@id]"))
                {
                    var element = node as XmlElement;
                    if (element == null) continue;
                    byId[element.GetAttribute("id")] = element;
                }
                views.Clear();
                XmlElement array = FindRootArray();
                if (array != null)
                {
                    foreach (XmlNode node in array.ChildNodes)
                    {
                        var item = node as XmlElement;
                        if (item == null || !String.Equals(item.LocalName, "item", StringComparison.OrdinalIgnoreCase)) continue;
                        XmlElement definition = Resolve(item.GetAttribute("href"));
                        if (definition == null || !String.Equals(definition.LocalName, "ViewDefinition", StringComparison.OrdinalIgnoreCase)) continue;
                        string id = ChildText(definition, "name");
                        views.Add(new ViewModel { Definition = definition, Item = item, Id = id, Label = Localized(id) });
                    }
                }
                RefreshList();
                dirty = false;
                status.Text = views.Count + (Localization.IsEnglish ? " views" : " Ansichten");
            }
            catch (Exception ex)
            {
                status.Text = ex.Message;
            }
        }

        private XmlElement FindRootArray()
        {
            foreach (XmlNode node in document.SelectNodes("//*[local-name()='ArrayList']"))
            {
                var list = node as XmlElement;
                if (list == null) continue;
                var items = Child(list, "_items");
                if (items == null) continue;
                XmlElement array = Resolve(items.GetAttribute("href"));
                if (array == null) continue;
                foreach (XmlNode child in array.ChildNodes)
                {
                    var item = child as XmlElement;
                    if (item == null || !String.Equals(item.LocalName, "item", StringComparison.OrdinalIgnoreCase)) continue;
                    XmlElement definition = Resolve(item.GetAttribute("href"));
                    if (definition != null && String.Equals(definition.LocalName, "ViewDefinition", StringComparison.OrdinalIgnoreCase)) return array;
                }
            }
            return null;
        }

        private XmlElement Resolve(string href)
        {
            if (String.IsNullOrEmpty(href)) return null;
            XmlElement element;
            return byId.TryGetValue(href.TrimStart('#'), out element) ? element : null;
        }

        private XmlElement ResolveArray(XmlElement listOrArray)
        {
            if (listOrArray == null) return null;
            if (!String.Equals(listOrArray.LocalName, "ArrayList", StringComparison.OrdinalIgnoreCase)) return listOrArray;
            XmlElement items = Child(listOrArray, "_items");
            return items == null ? null : Resolve(items.GetAttribute("href"));
        }

        private static XmlElement Child(XmlElement parent, string localName)
        {
            if (parent == null) return null;
            foreach (XmlNode node in parent.ChildNodes)
            {
                var element = node as XmlElement;
                if (element != null && String.Equals(element.LocalName, localName, StringComparison.OrdinalIgnoreCase)) return element;
            }
            return null;
        }

        private static string ChildText(XmlElement parent, string localName)
        {
            XmlElement child = Child(parent, localName);
            return child == null ? string.Empty : child.InnerText;
        }

        private void RefreshList()
        {
            int selected = viewList.SelectedIndex;
            viewList.BeginUpdate();
            try
            {
                viewList.Items.Clear();
                foreach (ViewModel view in views) viewList.Items.Add(view.Label);
            }
            finally { viewList.EndUpdate(); }
            if (viewList.Items.Count > 0) viewList.SelectedIndex = Math.Max(0, Math.Min(selected, viewList.Items.Count - 1));
        }

        private void BuildDetail()
        {
            detailFlow.SuspendLayout();
            try
            {
                while (detailFlow.Controls.Count > 0)
                {
                    Control existing = detailFlow.Controls[0];
                    detailFlow.Controls.RemoveAt(0);
                    existing.Dispose();
                }
                if (viewList.SelectedIndex < 0 || viewList.SelectedIndex >= views.Count) return;
                ViewModel view = views[viewList.SelectedIndex];
                view.Filters.Clear();
                XmlElement filterList = Child(view.Definition, "listFilters");
                XmlElement filterArray = filterList == null ? null : ResolveArray(Resolve(filterList.GetAttribute("href")));
                if (filterArray != null)
                {
                    foreach (XmlNode node in filterArray.ChildNodes)
                    {
                        var item = node as XmlElement;
                        if (item == null || !String.Equals(item.LocalName, "item", StringComparison.OrdinalIgnoreCase)) continue;
                        XmlElement filter = Resolve(item.GetAttribute("href"));
                        if (filter != null && String.Equals(filter.LocalName, "FilterDefinition", StringComparison.OrdinalIgnoreCase)) view.Filters.Add(filter);
                    }
                }

                var title = new Label { Text = view.Label + "  ·  " + view.Filters.Count + (Localization.IsEnglish ? " filters" : " Filter"), Font = Typography.Create(13f * scale, FontStyle.Bold), ForeColor = palette.Text, AutoSize = true, Margin = new Padding(0, 0, 0, Px(8)) };
                detailFlow.Controls.Add(title);
                if (view.Filters.Count == 0)
                    detailFlow.Controls.Add(new Label { Text = Localization.IsEnglish ? "This view has no filters." : "Diese Ansicht hat keine Filter.", Font = Typography.Create(10f * scale, FontStyle.Regular), ForeColor = palette.Muted, AutoSize = true, Margin = new Padding(0, 0, 0, Px(8)) });
                foreach (XmlElement filter in view.Filters) detailFlow.Controls.Add(BuildFilterCard(filter));
                int width = Math.Max(Px(480), detail.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - Px(12));
                detailFlow.MinimumSize = new Size(width, 0);
                detailFlow.MaximumSize = new Size(width, 0);
                detailFlow.Width = width;
                ResizeCards();
            }
            finally { detailFlow.ResumeLayout(true); }
            detail.AutoScrollPosition = Point.Empty;
        }

        private void ResizeCards()
        {
            int width = Math.Max(Px(440), detailFlow.ClientSize.Width - Px(8));
            foreach (Control control in detailFlow.Controls)
            {
                if (!(control is TableLayoutPanel)) continue;
                control.MinimumSize = new Size(width, 0);
                control.MaximumSize = new Size(width, 0);
                control.Width = width;
            }
        }

        private Control BuildFilterCard(XmlElement filter)
        {
            var card = new TableLayoutPanel { ColumnCount = 1, RowCount = 4, AutoSize = true, BackColor = palette.Panel, Margin = new Padding(0, 0, 0, Px(10)), Padding = new Padding(Px(12), Px(8), Px(12), Px(10)) };
            var main = new TableLayoutPanel { ColumnCount = 2, RowCount = 2, AutoSize = true, Dock = DockStyle.Fill, BackColor = palette.Panel, Margin = new Padding(0) };
            main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            main.Controls.Add(Field(L("Feld", "Field"), Combo(FilterFieldOptions(), Read(filter, "whereClause"), delegate(string value) { Write(filter, "whereClause", value); })), 0, 0);
            main.Controls.Add(Field(L("Operator", "Operator"), Combo(new[] { "=", ">", "<", "group", "like", "<>" }, Read(filter, "sqloperator"), delegate(string value) { Write(filter, "sqloperator", value); })), 1, 0);
            main.Controls.Add(Field(L("Wert", "Value"), Text(Read(filter, "whereValue"), delegate(string value) { Write(filter, "whereValue", value); })), 0, 1);
            main.Controls.Add(Field(L("Limit", "Limit"), Text(Read(filter, "limit"), delegate(string value) { Write(filter, "limit", value); })), 1, 1);
            card.Controls.Add(main, 0, 0);

            var display = new TableLayoutPanel { ColumnCount = 2, RowCount = 1, AutoSize = true, Dock = DockStyle.Fill, BackColor = palette.Panel, Margin = new Padding(0, Px(6), 0, 0) };
            display.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
            display.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
            display.Controls.Add(Field(L("Sortierung", "Sort"), Combo(SortFieldOptions(), Read(filter, "defaultSort"), delegate(string value) { Write(filter, "defaultSort", value); })), 0, 0);
            display.Controls.Add(Field(L("Ansicht", "View"), Combo(new[] { "List", "Icons", "Big Icons", "Cover Flow" }, Read(filter, "defaultView"), delegate(string value) { Write(filter, "defaultView", value); })), 1, 0);
            card.Controls.Add(display, 0, 1);

            var checks = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, WrapContents = true, AutoSize = true, Dock = DockStyle.Fill, BackColor = palette.Panel, Margin = new Padding(0, Px(6), 0, 0) };
            checks.Controls.Add(Check(L("Aufsteigend", "Ascending"), Read(filter, "sortAscending"), delegate(bool value) { Write(filter, "sortAscending", value ? "true" : "false"); }));
            checks.Controls.Add(Check("Distinct", Read(filter, "distinct"), delegate(bool value) { Write(filter, "distinct", value ? "true" : "false"); }));
            checks.Controls.Add(Check(L("Ebene überspringen", "Skip level"), Read(filter, "skipLevel"), delegate(bool value) { Write(filter, "skipLevel", value ? "true" : "false"); }));
            card.Controls.Add(checks, 0, 2);
            return card;
        }

        private Control Field(string label, Control input)
        {
            var panel = new TableLayoutPanel { ColumnCount = 1, RowCount = 2, AutoSize = true, Dock = DockStyle.Fill, BackColor = palette.Panel, Margin = new Padding(0, 0, Px(8), 0) };
            panel.Controls.Add(new Label { Text = label, Font = Typography.Create(8.5f * scale, FontStyle.Regular), ForeColor = palette.Muted, AutoSize = true, Margin = new Padding(0) }, 0, 0);
            input.Dock = DockStyle.Fill;
            input.Margin = new Padding(0, Px(2), 0, 0);
            panel.Controls.Add(input, 0, 1);
            return panel;
        }

        private ComboBox Combo(IEnumerable<string> values, string current, Action<string> changed)
        {
            var combo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDown, FlatStyle = FlatStyle.Flat, Font = Typography.Create(9.5f * scale, FontStyle.Regular), BackColor = palette.Input, ForeColor = palette.Text };
            foreach (string value in values)
                if (!combo.Items.Contains(value)) combo.Items.Add(value);
            if (!String.IsNullOrEmpty(current) && !combo.Items.Contains(current)) combo.Items.Add(current);
            combo.Text = current ?? string.Empty;
            combo.SelectedIndexChanged += delegate { if (combo.SelectedItem != null) { changed(combo.SelectedItem.ToString()); } };
            combo.Leave += delegate { changed(combo.Text); };
            return combo;
        }

        private TextBox Text(string value, Action<string> changed)
        {
            var box = new TextBox { Text = value ?? string.Empty, Font = Typography.Create(9.5f * scale, FontStyle.Regular), BackColor = palette.Input, ForeColor = palette.Text, BorderStyle = BorderStyle.FixedSingle };
            box.TextChanged += delegate { changed(box.Text); };
            return box;
        }

        private CheckBox Check(string label, string value, Action<bool> changed)
        {
            var check = new CheckBox { Text = label, Checked = String.Equals(value, "true", StringComparison.OrdinalIgnoreCase), AutoSize = true, Font = Typography.Create(9f * scale, FontStyle.Regular), ForeColor = palette.Text, BackColor = palette.Panel, FlatStyle = FlatStyle.Flat, Margin = new Padding(0, Px(16), 0, 0) };
            check.CheckedChanged += delegate { changed(check.Checked); };
            return check;
        }

        private IEnumerable<string> FilterFieldOptions()
        {
            return video
                ? new[] { "title", "titleindex", "actor", "actorindex", "director", "directorindex", "genre", "year", "watched", "recently added", "recently watched", "movie collections", "user groups", "user groups only" }
                : new[] { "title", "artist", "album", "albumartist", "genre", "year", "track", "disc#", "rating", "filetype", "timesplayed", "recently added", "favorites" };
        }

        private IEnumerable<string> SortFieldOptions()
        {
            return video
                ? new[] { "Name", "Date", "Year" }
                : new[] { "Name", "Title", "Artist", "Album", "Date", "Disc#", "Track", "Year", "Rating", "File Type" };
        }

        private string Read(XmlElement filter, string name)
        {
            XmlElement element = Child(filter, name);
            if (element == null) return string.Empty;
            if (element.HasAttribute("href"))
            {
                XmlElement target = Resolve(element.GetAttribute("href"));
                return target == null ? string.Empty : target.InnerText;
            }
            return element.InnerText;
        }

        private void Write(XmlElement filter, string name, string value)
        {
            string previous = Read(filter, name);
            if (String.Equals(previous, value ?? string.Empty, StringComparison.Ordinal)) return;
            XmlElement element = Child(filter, name);
            if (element == null)
            {
                element = filter.OwnerDocument.CreateElement(name);
                filter.AppendChild(element);
            }
            if (element.HasAttribute("href"))
            {
                element.RemoveAttribute("href");
                element.RemoveAttribute("id");
            }
            else
            {
                string id = element.GetAttribute("id");
                if (!String.IsNullOrEmpty(id)) DetachReferences(id, previous);
            }
            element.InnerText = value ?? string.Empty;
            dirty = true;
            status.Text = (Localization.IsEnglish ? "Unsaved view changes" : "Ungespeicherte Ansichtsänderungen");
        }

        private void DetachReferences(string id, string previous)
        {
            string reference = "#" + id;
            var targets = new List<XmlElement>();
            foreach (XmlNode node in document.SelectNodes("//*"))
            {
                var element = node as XmlElement;
                if (element != null && String.Equals(element.GetAttribute("href"), reference, StringComparison.Ordinal)) targets.Add(element);
            }
            foreach (XmlElement element in targets)
            {
                XmlElement clone = document.CreateElement(element.Prefix, element.LocalName, element.NamespaceURI);
                clone.InnerText = previous ?? string.Empty;
                element.ParentNode.ReplaceChild(clone, element);
            }
        }

        private void MoveView(int direction)
        {
            int index = viewList.SelectedIndex;
            int target = index + direction;
            if (index < 0 || target < 0 || index >= views.Count || target >= views.Count) return;
            ViewModel current = views[index];
            ViewModel other = views[target];
            string href = current.Item.GetAttribute("href");
            current.Item.SetAttribute("href", other.Item.GetAttribute("href"));
            other.Item.SetAttribute("href", href);
            views[index] = other;
            views[target] = current;
            dirty = true;
            RefreshList();
            viewList.SelectedIndex = target;
            status.Text = (Localization.IsEnglish ? "Order changed · unsaved" : "Reihenfolge geändert · ungespeichert");
        }

        private void Reload()
        {
            if (!ConfirmDiscardChanges()) return;
            LoadFile();
        }

        private void SaveFile()
        {
            if (document == null) return;
            try
            {
                string backup = NativeXmlFileStore.SaveDocument(path, document, encoding);
                dirty = false;
                status.Text = (Localization.IsEnglish ? "Saved · backup: " : "Gespeichert · Sicherung: ") + Path.GetFileName(backup);
            }
            catch (Exception ex)
            {
                status.Text = ex.Message;
                MessageBox.Show(FindForm(), (Localization.IsEnglish ? "The views could not be saved.\r\n\r\n" : "Die Ansichten konnten nicht gespeichert werden.\r\n\r\n") + ex.Message, "MP1 Configuration Studio", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}

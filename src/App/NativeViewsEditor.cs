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
            public List<XmlElement> FilterItems = new List<XmlElement>();
        }

        private readonly string path;
        private readonly bool video;
        private readonly float scale;
        private readonly Palette palette = Palette.For(true);
        private readonly ListBox viewList = new ListBox();
        private readonly StudioTextInput viewSearch;
        private readonly Panel detail = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
        private readonly FlowLayoutPanel detailFlow = new FlowLayoutPanel { Dock = DockStyle.Top, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true, BackColor = Palette.For(true).Background, Padding = new Padding(0) };
        private readonly Label status = new Label();
        private readonly List<ViewModel> views = new List<ViewModel>();
        private readonly Dictionary<string, string> strings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, XmlElement> byId = new Dictionary<string, XmlElement>(StringComparer.OrdinalIgnoreCase);
        private XmlDocument document;
        private Encoding encoding = new UTF8Encoding(false);
        private bool dirty;
        private int nextId = 1;
        private const string ListNamespace = "http://schemas.microsoft.com/clr/ns/System.Collections";
        private const string ArrayNamespace = "http://schemas.xmlsoap.org/soap/encoding/";
        private const string ViewNamespace = "http://schemas.microsoft.com/clr/nsassem/MediaPortal.GUI.View/Core%2C%20Version%3D1.39.0.0%2C%20Culture%3Dneutral%2C%20PublicKeyToken%3Dnull";

        public NativeViewsEditor(string configDirectory, string fileName, string language, float scale)
        {
            this.path = Path.Combine(configDirectory, fileName);
            this.video = String.Equals(fileName, "VideoViews.xml", StringComparison.OrdinalIgnoreCase);
            this.scale = scale;
            BackColor = palette.Background;
            ForeColor = palette.Text;
            var body = Typography.Create(10f * scale, FontStyle.Regular);
            LoadStrings(configDirectory, language);

            var top = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = true, FlowDirection = FlowDirection.LeftToRight, BackColor = palette.Background, Padding = new Padding(0, Px(4), 0, 0) };
            var reload = ButtonFor(Localization.T("Neu laden"), delegate { Reload(); }, body);
            var save = ButtonFor(Localization.T("Speichern"), delegate { SaveFile(); }, body);
            var up = ButtonFor("▲", delegate { MoveView(-1); }, body);
            var down = ButtonFor("▼", delegate { MoveView(1); }, body);
            var addView = ButtonFor(Localization.T("＋ Ansicht"), delegate { AddView(); }, body);
            up.Width = Px(44);
            down.Width = Px(44);
            top.Controls.Add(reload);
            top.Controls.Add(save);
            top.Controls.Add(up);
            top.Controls.Add(down);
            top.Controls.Add(addView);
            status.AutoSize = false;
            status.Width = Px(220);
            status.Height = Px(28);
            status.AutoEllipsis = true;
            status.TextAlign = ContentAlignment.MiddleLeft;
            status.Font = Typography.Create(9f * scale, FontStyle.Regular);
            status.ForeColor = palette.Muted;
            top.Controls.Add(status);

            var split = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = palette.Background };
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32));
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68));

            var left = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = palette.Background, Margin = new Padding(0, 0, Px(10), 0) };
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, Px(32)));
            left.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            viewSearch = new StudioTextInput(scale, body, palette) { Dock = DockStyle.Fill, Colors = () => palette, Margin = new Padding(0, 0, 0, Px(4)), AccessibleName = Localization.T("Ansicht suchen") };
            viewSearch.Box.ForeColor = palette.Muted;
            viewSearch.Box.Text = Localization.T("Ansicht suchen …");
            viewSearch.Box.GotFocus += delegate { if (viewSearch.Box.ForeColor == palette.Muted) { viewSearch.Box.Text = string.Empty; viewSearch.Box.ForeColor = palette.Text; } };
            viewSearch.Box.LostFocus += delegate { if (viewSearch.Box.Text.Length == 0) { viewSearch.Box.ForeColor = palette.Muted; viewSearch.Box.Text = Localization.T("Ansicht suchen …"); } };
            viewSearch.Box.TextChanged += delegate { if (viewSearch.Box.ForeColor == palette.Text) SelectMatchingView(viewSearch.Text); };
            left.Controls.Add(viewSearch, 0, 0);
            viewList.Dock = DockStyle.Fill;
            viewList.BackColor = palette.Input;
            viewList.ForeColor = palette.Text;
            viewList.BorderStyle = BorderStyle.None;
            viewList.Font = body;
            viewList.DrawMode = DrawMode.OwnerDrawFixed;
            viewList.ItemHeight = Px(34);
            viewList.IntegralHeight = false;
            viewList.DrawItem += DrawViewItem;
            viewList.SelectedIndexChanged += delegate { BuildDetail(); };
            left.Controls.Add(viewList, 0, 1);
            split.Controls.Add(left, 0, 0);
            detail.Controls.Add(detailFlow);
            detail.Resize += delegate
            {
                int width = Math.Max(Px(480), detail.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - Px(12));
                detailFlow.MinimumSize = new Size(width, 0);
                detailFlow.MaximumSize = new Size(width, 0);
                detailFlow.Width = width;
                ResizeCards();
            };

            split.Controls.Add(new StudioScrollHost(detail, delegate { return new Size(Math.Max(1, detailFlow.Width), Math.Max(1, detailFlow.Height)); }, () => palette, false, (Color?)palette.Background), 1, 0);
            Controls.Add(split);
            Controls.Add(top);
            LoadFile();
        }

        public bool HasUnsavedChanges { get { return dirty; } }

        public bool ConfirmDiscardChanges()
        {
            if (!dirty) return true;
            return MessageBox.Show(FindForm(), Localization.T("Ungespeicherte Ansichtsänderungen verwerfen?"), "MP1 Configuration Studio", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes;
        }

        public bool SaveChanges()
        {
            SaveFile();
            return !dirty;
        }

        private int Px(float value) { return (int)Math.Round(value * scale); }

        private static string L(string de, string en) { return Localization.T2(de, en); }

        private Button ButtonFor(string text, EventHandler click, Font font)
        {
            var button = new StudioButton { Text = text, Font = font, Height = Px(28), MinimumSize = new Size(Px(60), Px(28)), AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(Px(8), 0, Px(8), 0), FlatStyle = FlatStyle.Flat, BackColor = palette.Panel, ForeColor = palette.Text, Margin = new Padding(0, 0, Px(6), 0) };
            button.FlatAppearance.BorderColor = palette.Border;
            button.Click += click;
            return button;
        }

        private void DrawViewItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= views.Count) return;
            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            using (var background = new SolidBrush(selected ? palette.Selection : palette.Input))
                e.Graphics.FillRectangle(background, e.Bounds);
            if (selected)
                using (var accent = new SolidBrush(palette.Accent))
                    e.Graphics.FillRectangle(accent, e.Bounds.Left, e.Bounds.Top, Px(3), e.Bounds.Height);
            TextRenderer.DrawText(e.Graphics, views[e.Index].Label, e.Font,
                new Rectangle(e.Bounds.Left + Px(10), e.Bounds.Top, Math.Max(0, e.Bounds.Width - Px(14)), e.Bounds.Height),
                selected ? palette.SelectionText : palette.Text,
                TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
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
            if (String.IsNullOrWhiteSpace(id)) return L("(unbenannt)", "(unnamed)");
            string value;
            if (strings.TryGetValue(id, out value) && !String.IsNullOrWhiteSpace(value)) return value + "  ·  #" + id;
            if (id.All(char.IsDigit)) return "#" + id;
            return id;
        }

        private void LoadFile()
        {
            try
            {
                if (!File.Exists(path))
                {
                    status.Text = (Localization.T("Datei nicht gefunden: ")) + path;
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
                nextId = 1;
                foreach (string id in byId.Keys)
                {
                    if (id.StartsWith("ref-", StringComparison.OrdinalIgnoreCase))
                    {
                        int value;
                        if (Int32.TryParse(id.Substring(4), out value) && value >= nextId) nextId = value + 1;
                    }
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
                status.Text = views.Count + (Localization.T(" Ansichten"));
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
                view.FilterItems.Clear();
                XmlElement filterList = Child(view.Definition, "listFilters");
                XmlElement filterArray = filterList == null ? null : ResolveArray(Resolve(filterList.GetAttribute("href")));
                if (filterArray != null)
                {
                    foreach (XmlNode node in filterArray.ChildNodes)
                    {
                        var item = node as XmlElement;
                        if (item == null || !String.Equals(item.LocalName, "item", StringComparison.OrdinalIgnoreCase)) continue;
                        XmlElement filter = Resolve(item.GetAttribute("href"));
                        if (filter != null && String.Equals(filter.LocalName, "FilterDefinition", StringComparison.OrdinalIgnoreCase))
                        {
                            view.Filters.Add(filter);
                            view.FilterItems.Add(item);
                        }
                    }
                }

                int titleWidth = Math.Max(Px(240), detail.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - Px(20));
                var title = new Label { Text = view.Label, Font = Typography.Create(13f * scale, FontStyle.Bold), ForeColor = palette.Text, AutoSize = true, MaximumSize = new Size(titleWidth, 0), Margin = new Padding(0, 0, 0, Px(2)) };
                detailFlow.Controls.Add(title);
                detailFlow.Controls.Add(new Label { Text = view.Filters.Count + (Localization.T(" Filter")), Font = Typography.Create(9.5f * scale, FontStyle.Regular), ForeColor = palette.Muted, AutoSize = true, Margin = new Padding(0, 0, 0, Px(6)) });
                var viewActions = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, WrapContents = true, AutoSize = true, BackColor = palette.Background, Margin = new Padding(0, 0, 0, Px(8)) };
                viewActions.Controls.Add(ButtonFor(L("Ansicht löschen", "Delete view"), delegate { DeleteView(); }, Typography.Create(9f * scale, FontStyle.Regular)));
                viewActions.Controls.Add(ButtonFor(L("Filter hinzufügen", "Add filter"), delegate { AddFilter(); }, Typography.Create(9f * scale, FontStyle.Regular)));
                detailFlow.Controls.Add(viewActions);
                if (view.Filters.Count == 0)
                    detailFlow.Controls.Add(new Label { Text = Localization.T("Diese Ansicht hat keine Filter."), Font = Typography.Create(10f * scale, FontStyle.Regular), ForeColor = palette.Muted, AutoSize = true, Margin = new Padding(0, 0, 0, Px(8)) });
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
            main.Controls.Add(Field(L("Wert", "Value"), TextInput(Read(filter, "whereValue"), delegate(string value) { Write(filter, "whereValue", value); })), 0, 1);
            main.Controls.Add(Field(L("Limit", "Limit"), TextInput(Read(filter, "limit"), delegate(string value) { Write(filter, "limit", value); })), 1, 1);
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
            var remove = ButtonFor(L("Filter löschen", "Delete filter"), delegate { DeleteFilter(filter); }, Typography.Create(9f * scale, FontStyle.Regular));
            remove.Margin = new Padding(Px(16), Px(4), 0, 0);
            checks.Controls.Add(remove);
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

        private StudioComboInput Combo(IEnumerable<string> values, string current, Action<string> changed)
        {
            var combo = new StudioComboInput(scale, Typography.Create(9.5f * scale, FontStyle.Regular), palette) { Colors = () => palette };
            foreach (string value in values)
                if (!combo.Items.Contains(value)) combo.Items.Add(value);
            if (!String.IsNullOrEmpty(current) && !combo.Items.Contains(current)) combo.Items.Add(current);
            combo.Text = current ?? string.Empty;
            combo.SelectedIndexChanged += delegate { changed(combo.Text); };
            combo.Leave += delegate { changed(combo.Text); };
            return combo;
        }

        private StudioTextInput TextInput(string value, Action<string> changed)
        {
            var box = new StudioTextInput(scale, Typography.Create(9.5f * scale, FontStyle.Regular), palette) { Colors = () => palette };
            box.Text = value ?? string.Empty;
            box.Box.TextChanged += delegate { changed(box.Text); };
            return box;
        }

        private StudioCheck Check(string label, string value, Action<bool> changed)
        {
            var font = Typography.Create(9f * scale, FontStyle.Regular);
            var check = new StudioCheck(scale) { Text = label, Checked = String.Equals(value, "true", StringComparison.OrdinalIgnoreCase), Font = font, Colors = () => palette, Height = Px(30), Width = TextRenderer.MeasureText(label, font).Width + Px(50), Margin = new Padding(0, 0, Px(12), 0) };
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
            status.Text = (Localization.T("Ungespeicherte Ansichtsänderungen"));
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
            status.Text = (Localization.T("Reihenfolge geändert · ungespeichert"));
        }

        private ViewModel SelectedView()
        {
            int index = viewList.SelectedIndex;
            return index >= 0 && index < views.Count ? views[index] : null;
        }

        private void SelectMatchingView(string query)
        {
            if (String.IsNullOrWhiteSpace(query)) return;
            for (int i = 0; i < views.Count; i++)
            {
                if (views[i].Label.IndexOf(query.Trim(), StringComparison.CurrentCultureIgnoreCase) >= 0)
                {
                    viewList.SelectedIndex = i;
                    return;
                }
            }
        }

        private void AddView()
        {
            using (var dialog = new TextInputDialog(scale, L("Neue Ansicht", "New view"), L("Name der Ansicht", "View name"), string.Empty))
            {
                if (dialog.ShowDialog(FindForm()) != DialogResult.OK) return;
                string name = dialog.Value.Trim();
                if (name.Length == 0) return;
                XmlElement list = CreateEmptyList();
                XmlElement definition = CreateViewDefinition(name, list.GetAttribute("id"));
                XmlElement rootArray = FindRootArray();
                XmlElement rootList = FindRootList(rootArray);
                if (rootArray == null || rootList == null) return;
                XmlElement item = document.CreateElement("item");
                item.SetAttribute("href", "#" + definition.GetAttribute("id"));
                rootArray.AppendChild(item);
                SetListSize(rootList, views.Count + 1);
                views.Add(new ViewModel { Definition = definition, Item = item, Id = name, Label = Localized(name) });
                dirty = true;
                RefreshList();
                viewList.SelectedIndex = views.Count - 1;
                status.Text = L("Ansicht hinzugefügt · ungespeichert", "View added · unsaved");
            }
        }

        private void DeleteView()
        {
            ViewModel view = SelectedView();
            if (view == null) return;
            if (MessageBox.Show(FindForm(), L("Diese Ansicht löschen?", "Delete this view?"), "MP1 Configuration Studio", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
            XmlElement rootArray = FindRootArray();
            XmlElement rootList = FindRootList(rootArray);
            if (view.Item != null && view.Item.ParentNode != null) view.Item.ParentNode.RemoveChild(view.Item);
            views.Remove(view);
            if (rootList != null) SetListSize(rootList, views.Count);
            dirty = true;
            RefreshList();
            if (viewList.Items.Count > 0) viewList.SelectedIndex = Math.Min(viewList.SelectedIndex, viewList.Items.Count - 1);
            status.Text = L("Ansicht gelöscht · ungespeichert", "View deleted · unsaved");
        }

        private void AddFilter()
        {
            ViewModel view = SelectedView();
            if (view == null) return;
            using (var dialog = new FilterDialog(scale, FilterFieldOptions(), SortFieldOptions()))
            {
                if (dialog.ShowDialog(FindForm()) != DialogResult.OK) return;
                XmlElement definition = CreateFilter(dialog.Field, dialog.Operator, dialog.Value, dialog.Sort, dialog.ViewMode, dialog.Limit);
                XmlElement list = Child(view.Definition, "listFilters");
                if (list == null)
                {
                    list = CreateEmptyList();
                    XmlElement link = document.CreateElement("listFilters");
                    link.SetAttribute("href", "#" + list.GetAttribute("id"));
                    view.Definition.AppendChild(link);
                }
                XmlElement array = ResolveArray(Resolve(list.GetAttribute("href")));
                if (array == null) return;
                XmlElement item = document.CreateElement("item");
                item.SetAttribute("href", "#" + definition.GetAttribute("id"));
                array.AppendChild(item);
                SetListSize(Resolve(list.GetAttribute("href")), view.Filters.Count + 1);
                dirty = true;
                BuildDetail();
                status.Text = L("Filter hinzugefügt · ungespeichert", "Filter added · unsaved");
            }
        }

        private void DeleteFilter(XmlElement filter)
        {
            ViewModel view = SelectedView();
            if (view == null) return;
            int index = view.Filters.IndexOf(filter);
            if (index < 0) return;
            if (MessageBox.Show(FindForm(), L("Diesen Filter löschen?", "Delete this filter?"), "MP1 Configuration Studio", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
            XmlElement item = index < view.FilterItems.Count ? view.FilterItems[index] : null;
            if (item != null && item.ParentNode != null) item.ParentNode.RemoveChild(item);
            XmlElement list = Child(view.Definition, "listFilters");
            XmlElement listObject = list == null ? null : Resolve(list.GetAttribute("href"));
            if (listObject != null) SetListSize(listObject, Math.Max(0, view.Filters.Count - 1));
            dirty = true;
            BuildDetail();
            status.Text = L("Filter gelöscht · ungespeichert", "Filter deleted · unsaved");
        }

        private string TakeId()
        {
            string id = "ref-" + nextId;
            nextId++;
            return id;
        }

        private XmlElement Body()
        {
            foreach (XmlNode node in document.DocumentElement.ChildNodes)
            {
                var element = node as XmlElement;
                if (element != null && String.Equals(element.LocalName, "Body", StringComparison.OrdinalIgnoreCase)) return element;
            }
            return document.DocumentElement;
        }

        private XmlElement CreateEmptyList()
        {
            string listId = TakeId();
            string arrayId = TakeId();
            XmlElement list = document.CreateElement("a1", "ArrayList", ListNamespace);
            list.SetAttribute("id", listId);
            XmlElement items = document.CreateElement("_items");
            items.SetAttribute("href", "#" + arrayId);
            list.AppendChild(items);
            AppendChildText(list, "_size", "0");
            AppendChildText(list, "_version", "0");
            XmlElement array = document.CreateElement("SOAP-ENC", "Array", ArrayNamespace);
            array.SetAttribute("id", arrayId);
            array.SetAttribute("arrayType", "xsd:anyType[0]");
            Body().AppendChild(list);
            Body().AppendChild(array);
            byId[listId] = list;
            byId[arrayId] = array;
            return list;
        }

        private XmlElement CreateViewDefinition(string name, string listId)
        {
            string id = TakeId();
            XmlElement definition = document.CreateElement("a3", "ViewDefinition", ViewNamespace);
            definition.SetAttribute("id", id);
            XmlElement list = document.CreateElement("listFilters");
            list.SetAttribute("href", "#" + listId);
            definition.AppendChild(list);
            XmlElement nameNode = document.CreateElement("name");
            nameNode.SetAttribute("id", TakeId());
            nameNode.InnerText = name;
            definition.AppendChild(nameNode);
            Body().AppendChild(definition);
            byId[id] = definition;
            return definition;
        }

        private XmlElement CreateFilter(string field, string sqlOperator, string value, string sort, string view, string limit)
        {
            string id = TakeId();
            XmlElement filter = document.CreateElement("a3", "FilterDefinition", ViewNamespace);
            filter.SetAttribute("id", id);
            AppendChildText(filter, "distinct", "false");
            AppendChildText(filter, "sortAscending", "true");
            AppendChildText(filter, "skipLevel", "false");
            AppendChildText(filter, "restriction", string.Empty);
            AppendChildText(filter, "whereClause", field);
            AppendChildText(filter, "fromStatement", string.Empty);
            AppendChildText(filter, "sqloperator", sqlOperator);
            AppendChildText(filter, "whereValue", value);
            AppendChildText(filter, "selectedValue", string.Empty);
            AppendChildText(filter, "defaultView", view);
            AppendChildText(filter, "defaultSort", sort);
            AppendChildText(filter, "limit", limit);
            Body().AppendChild(filter);
            byId[id] = filter;
            return filter;
        }

        private static void AppendChildText(XmlElement parent, string name, string value)
        {
            XmlElement child = parent.OwnerDocument.CreateElement(name);
            child.InnerText = value ?? string.Empty;
            parent.AppendChild(child);
        }

        private XmlElement FindRootList(XmlElement rootArray)
        {
            if (rootArray == null) return null;
            foreach (XmlNode node in document.SelectNodes("//*[local-name()='ArrayList']"))
            {
                var list = node as XmlElement;
                if (list == null) continue;
                XmlElement items = Child(list, "_items");
                if (items == null) continue;
                if (Object.ReferenceEquals(Resolve(items.GetAttribute("href")), rootArray)) return list;
            }
            return null;
        }

        private void SetListSize(XmlElement list, int size)
        {
            if (list == null) return;
            XmlElement sizeNode = Child(list, "_size");
            if (sizeNode == null)
            {
                sizeNode = document.CreateElement("_size");
                list.AppendChild(sizeNode);
            }
            sizeNode.InnerText = Math.Max(0, size).ToString();
            XmlElement items = Child(list, "_items");
            XmlElement array = items == null ? null : Resolve(items.GetAttribute("href"));
            if (array == null) return;
            string type = array.GetAttribute("arrayType");
            int start = type.IndexOf('[');
            int end = type.IndexOf(']');
            int capacity = 0;
            if (start >= 0 && end > start) Int32.TryParse(type.Substring(start + 1, end - start - 1), out capacity);
            if (size > capacity)
                array.SetAttribute("arrayType", "xsd:anyType[" + size + "]");
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
                status.Text = (Localization.T("Gespeichert · Sicherung: ")) + Path.GetFileName(backup);
            }
            catch (Exception ex)
            {
                status.Text = ex.Message;
                MessageBox.Show(FindForm(), (Localization.T("Die Ansichten konnten nicht gespeichert werden.\r\n\r\n")) + ex.Message, "MP1 Configuration Studio", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private sealed class TextInputDialog : Form
        {
            private readonly TextBox input = new TextBox();
            private readonly float scale;
            public string Value { get { return input.Text.Trim(); } }

            public TextInputDialog(float scale, string title, string label, string initial)
            {
                this.scale = scale;
                Text = title;
                FormBorderStyle = FormBorderStyle.FixedDialog;
                StartPosition = FormStartPosition.CenterParent;
                MaximizeBox = false;
                MinimizeBox = false;
                ShowInTaskbar = false;
                BackColor = Palette.For(true).Panel;
                ForeColor = Palette.For(true).Text;
                ClientSize = new Size(Px(460), Px(170));
                var caption = new Label { Text = label, ForeColor = Palette.For(true).Muted, AutoSize = true, Left = Px(18), Top = Px(18) };
                input.Left = Px(18);
                input.Top = Px(44);
                input.Width = Px(424);
                input.Font = Typography.Create(10f * scale, FontStyle.Regular);
                input.BackColor = Palette.For(true).Input;
                input.ForeColor = Palette.For(true).Text;
                input.Text = initial ?? string.Empty;
                var cancel = new StudioButton { Text = Localization.T("Abbrechen"), DialogResult = DialogResult.Cancel, FlatStyle = FlatStyle.Flat, BackColor = Palette.For(true).Panel, ForeColor = Palette.For(true).Text, Left = Px(342), Top = Px(112), Width = Px(100), Height = Px(30) };
                cancel.FlatAppearance.BorderColor = Palette.For(true).Border;
                var ok = new StudioButton { Text = "OK", FlatStyle = FlatStyle.Flat, BackColor = Palette.For(true).Panel, ForeColor = Palette.For(true).Text, Left = Px(232), Top = Px(112), Width = Px(100), Height = Px(30) };
                ok.FlatAppearance.BorderColor = Palette.For(true).Border;
                ok.Click += delegate { if (input.Text.Trim().Length == 0) return; DialogResult = DialogResult.OK; Close(); };
                Controls.Add(caption);
                Controls.Add(input);
                Controls.Add(ok);
                Controls.Add(cancel);
                AcceptButton = ok;
                CancelButton = cancel;
            }

            private int Px(float value) { return (int)Math.Round(value * scale); }
        }

        private sealed class FilterDialog : Form
        {
            private readonly float scale;
            private readonly StudioComboInput fieldChoice;
            private readonly StudioComboInput sqlOperator;
            private readonly StudioTextInput value;
            private readonly StudioComboInput sort;
            private readonly StudioComboInput view;
            private readonly StudioTextInput limit;
            public string Field { get { return fieldChoice.Text.Trim(); } }
            public string Operator { get { return sqlOperator.Text.Trim(); } }
            public string Value { get { return value.Text; } }
            public string Sort { get { return sort.Text.Trim(); } }
            public string ViewMode { get { return view.Text.Trim(); } }
            public string Limit { get { return limit.Text.Trim(); } }

            public FilterDialog(float scale, IEnumerable<string> fields, IEnumerable<string> sorts)
            {
                this.scale = scale;
                Text = Localization.T("Filter hinzufügen");
                FormBorderStyle = FormBorderStyle.FixedDialog;
                StartPosition = FormStartPosition.CenterParent;
                MaximizeBox = false;
                MinimizeBox = false;
                ShowInTaskbar = false;
                BackColor = Palette.For(true).Panel;
                ForeColor = Palette.For(true).Text;
                ClientSize = new Size(Px(560), Px(390));
                var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 8, Padding = new Padding(Px(16)), BackColor = Palette.For(true).Panel };
                table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Px(120)));
                table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                var dialogFont = Typography.Create(10f * scale, FontStyle.Regular);
                fieldChoice = new StudioComboInput(scale, dialogFont, Palette.For(true)) { Colors = () => Palette.For(true) };
                sqlOperator = new StudioComboInput(scale, dialogFont, Palette.For(true)) { Colors = () => Palette.For(true) };
                value = new StudioTextInput(scale, dialogFont, Palette.For(true)) { Colors = () => Palette.For(true) };
                sort = new StudioComboInput(scale, dialogFont, Palette.For(true)) { Colors = () => Palette.For(true) };
                view = new StudioComboInput(scale, dialogFont, Palette.For(true)) { Colors = () => Palette.For(true) };
                limit = new StudioTextInput(scale, dialogFont, Palette.For(true)) { Colors = () => Palette.For(true) };
                Fill(fieldChoice, fields, null);
                Fill(sqlOperator, new[] { "=", ">", "<", "group", "like", "<>" }, "=");
                value.Text = "*";
                Fill(sort, sorts, null);
                Fill(view, new[] { "List", "Icons", "Big Icons", "Cover Flow" }, "List");
                limit.Text = "-1";
                AddRow(table, 0, Localization.T("Feld"), fieldChoice);
                AddRow(table, 1, "Operator", sqlOperator);
                AddRow(table, 2, Localization.T("Wert"), value);
                AddRow(table, 3, Localization.T("Sortierung"), sort);
                AddRow(table, 4, Localization.T("Ansicht"), view);
                AddRow(table, 5, "Limit", limit);
                var buttons = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill, BackColor = Palette.For(true).Panel, Margin = new Padding(0, Px(8), 0, 0) };
                var cancel = new StudioButton { Text = Localization.T("Abbrechen"), DialogResult = DialogResult.Cancel, FlatStyle = FlatStyle.Flat, BackColor = Palette.For(true).Panel, ForeColor = Palette.For(true).Text, Width = Px(100), Height = Px(30) };
                cancel.FlatAppearance.BorderColor = Palette.For(true).Border;
                var ok = new StudioButton { Text = "OK", FlatStyle = FlatStyle.Flat, BackColor = Palette.For(true).Panel, ForeColor = Palette.For(true).Text, Width = Px(100), Height = Px(30) };
                ok.FlatAppearance.BorderColor = Palette.For(true).Border;
                ok.Click += delegate
                {
                    if (String.IsNullOrWhiteSpace(fieldChoice.Text)) return;
                    DialogResult = DialogResult.OK;
                    Close();
                };
                buttons.Controls.Add(cancel);
                buttons.Controls.Add(ok);
                table.Controls.Add(buttons, 0, 6);
                table.SetColumnSpan(buttons, 2);
                Controls.Add(table);
                AcceptButton = ok;
                CancelButton = cancel;
            }

            private int Px(float value) { return (int)Math.Round(value * scale); }

            private void Fill(StudioComboInput combo, IEnumerable<string> values, string selected)
            {
                combo.Font = Typography.Create(10f * scale, FontStyle.Regular);
                foreach (string item in values)
                    if (!combo.Items.Contains(item)) combo.Items.Add(item);
                combo.Text = selected ?? (combo.Items.Count > 0 ? combo.Items[0] : String.Empty);
            }

            private void AddRow(TableLayoutPanel table, int row, string label, Control input)
            {
                table.Controls.Add(new Label { Text = label, ForeColor = Palette.For(true).Text, TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, row);
                input.Dock = DockStyle.Fill;
                input.Font = Typography.Create(10f * scale, FontStyle.Regular);
                if (input is TextBox)
                {
                    input.BackColor = Palette.For(true).Input;
                    input.ForeColor = Palette.For(true).Text;
                }
                table.Controls.Add(input, 1, row);
            }
        }
    }
}

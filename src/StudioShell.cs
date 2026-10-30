using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;

namespace MP1.ConfigurationStudio
{
    public sealed partial class StudioShell
    {
        private readonly Form form;
        private readonly TreeView tree;
        private readonly Panel holder;
        private readonly Button nativeOk, nativeCancel, nativeApply;
        private readonly ToolStrip nativeTools;
        private readonly Control donate;
        private readonly Appearance appearance;
        private readonly Theme theme;
        private readonly List<Control> sections;
        private readonly Dictionary<Control, string> baselines = new Dictionary<Control,string>();
        private readonly HashSet<Control> edits = new HashSet<Control>();
        private readonly HashSet<Control> observed = new HashSet<Control>();
        private readonly HashSet<Control> userTouched = new HashSet<Control>();
        private readonly List<Form> dialogs = new List<Form>();
        private readonly ToolTip tips = new ToolTip();
        private TableLayoutPanel root, body;
        private Panel sidebar, center, context, footer, header, toolbar;
        private TextBox search;
        private ListBox results;
        private Label status, title, breadcrumb, helpTitle, helpText, changeText, hint, count, contextCaption;
        private FlowLayoutPanel tabs;
        private ComboBox themeChoice;
        private Button contextButton, clearSearch;
        private bool switching, building;
        private TreeNode lastSelection;
        private TreeNode[] displayedTabs=new TreeNode[0];
        private IDisposable selectionFrame;
        private bool changingTheme;
        private bool initializing;
        private Font font,headingFont,labelFont,treeRootFont;
        private string fieldHelp = "";
        public int OriginalSectionCount { get { return sections.Count; } }
        public int ChangedCount { get { return edits.Count; } }
        public bool ContextVisible { get { return appearance.ContextVisible; } }
        public bool IsDark { get { return appearance.Dark; } }
        public int NativeTreeCount { get { return Flatten(tree.Nodes).Count(); } }
        public bool PreparedBeforeFirstDisplay {get;private set;}

        public static StudioShell Attach(Form form,string preferences)
        {
            return new StudioShell(form,Require<TreeView>(form,"sectionTree"),Require<Panel>(form,"holderPanel"),
                Require<Button>(form,"okButton"),Require<Button>(form,"cancelButton"),FindField(form,"applyButton") as Button,
                Require<ToolStrip>(form,"toolStrip1"),FindField(form,"linkLabel1") as Control,preferences,ReadSections(form));
        }
        internal static StudioShell AttachFixture(Form form, TreeView tree, Panel holder, Button ok, Button cancel, ToolStrip tools,string preferences,List<Control> sections)
        {
            return new StudioShell(form,tree,holder,ok,cancel,null,tools,null,preferences,sections);
        }
        private static object FindField(object obj,string name)
        {
            Type t=obj.GetType();
            while(t!=null){var f=t.GetField(name,BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public|BindingFlags.DeclaredOnly);if(f!=null)return f.GetValue(obj);t=t.BaseType;}
            return null;
        }
        private static T Require<T>(object obj,string name) where T:class
        {
            T value=FindField(obj,name) as T;
            if(value==null)throw new NotSupportedException("Die native Oberfläche enthält nicht das erwartete Feld: "+name);
            return value;
        }
        private static List<Control> ReadSections(Form f)
        {
            var property=f.GetType().GetProperty("SettingSections",BindingFlags.Static|BindingFlags.Public);
            var values=property==null?null:property.GetValue(null,null) as IDictionary;
            if(values==null)throw new NotSupportedException("Die nativen Konfigurationsseiten konnten nicht ermittelt werden.");
            var pages=new List<Control>();
            foreach(DictionaryEntry entry in values)
            {
                var config=entry.Value.GetType().GetProperty("ConfigSection").GetValue(entry.Value,null) as Control;
                if(config!=null) pages.Add(config);
            }
            if(pages.Count==0)throw new NotSupportedException("Es wurden keine Originalseiten gefunden.");
            return pages;
        }
        private StudioShell(Form f,TreeView originalTree,Panel originalHolder,Button ok,Button cancel,Button apply,ToolStrip tools,Control donateLink,string preferencePath,List<Control> originalSections)
        {
            form=f;tree=originalTree;holder=originalHolder;nativeOk=ok;nativeCancel=cancel;nativeApply=apply;nativeTools=tools;donate=donateLink;sections=originalSections;
            PreparedBeforeFirstDisplay=!FrameRendering.IsVisible(form);
            initializing=PreparedBeforeFirstDisplay;
            appearance=new Appearance(preferencePath);theme=new Theme(appearance.Dark);
            InitializeTypography();
            FrameRendering.EnableComposition(form);
            using(FrameRendering.Suspend(form))
            {
            // Freeze existing page fonts and minimum geometry before changing their parent.
            // We keep every original page, control and native event handler.
            foreach(Control section in sections)
            {
                section.Font=section.Font;
                int contentWidth=section.Controls.Cast<Control>().Select(c=>c.Right).DefaultIfEmpty(section.Width).Max();
                int contentHeight=section.Controls.Cast<Control>().Select(c=>c.Bottom).DefaultIfEmpty(section.Height).Max();
                section.MinimumSize=new Size(Math.Max(section.MinimumSize.Width,contentWidth+4),Math.Max(section.MinimumSize.Height,contentHeight+8));
                Watch(section);
                theme.Prepare(section);
                pageZoom.Capture(section);pageZoom.Apply(section,PageScale);
            }
            building=true;form.SuspendLayout();
            try { Build(); }
            finally { building=false;form.ResumeLayout(true); }
            tree.BeforeSelect+=delegate(object sender,TreeViewCancelEventArgs e){switching=!e.Cancel;if(!e.Cancel){CompleteSelectionFrame(null,EventArgs.Empty);selectionFrame=FrameRendering.Suspend(center);}};
            lastSelection=tree.SelectedNode;
            tree.AfterSelect+=delegate{try{switching=false;RefreshPage();InvalidateSelection(lastSelection);InvalidateSelection(tree.SelectedNode);lastSelection=tree.SelectedNode;}finally{CompleteSelectionFrame(null,EventArgs.Empty);}};
            Application.Idle+=CompleteSelectionFrame;
            form.Shown+=delegate{
                if(!initializing)return;
                foreach(Control input in baselines.Keys.ToArray())if(!input.IsDisposed)baselines[input]=Value(input);
                edits.Clear();userTouched.Clear();initializing=false;UpdateStatus();
            };
            nativeTools.ItemClicked+=delegate { if(form.IsHandleCreated)form.BeginInvoke((Action)delegate{RefreshPage();UpdateSearch();}); };
            holder.ControlAdded+=delegate(object sender,ControlEventArgs e){Watch(e.Control);theme.Prepare(e.Control);LayoutNativePage();};
            form.FormClosed+=delegate{
                Application.Idle-=CompleteSelectionFrame;CompleteSelectionFrame(null,EventArgs.Empty);
                if(form.WindowState==FormWindowState.Normal){appearance.Width=form.Width;appearance.Height=form.Height;}
                appearance.Save();tips.Dispose();pageZoom.Dispose();foreach(Font owned in shellFonts)owned.Dispose();
            };
            form.KeyPreview=true;
            form.KeyDown+=OnKeyDown;
            if(tree.SelectedNode!=null&&NodeSection(tree.SelectedNode)!=null&&NodeSection(tree.SelectedNode).GetType().Name=="Project")
            {
                TreeNode start=Flatten(tree.Nodes).FirstOrDefault(n=>NodeSection(n)!=null&&NodeSection(n).GetType().Name=="General");
                if(start!=null)Navigate(start);
            }
            RefreshPage();ApplyTheme();
            LayoutNativePage();
            LayoutHelp();
            }
        }
        private void CompleteSelectionFrame(object sender,EventArgs e)
        {if(selectionFrame!=null){var pending=selectionFrame;selectionFrame=null;pending.Dispose();}}

        private Button ActionButton(string text,Action action,int width)
        {
            var b=new StudioAction { Text=text,Width=width,Height=Px(36),Colors=()=>theme.Colors,Font=font,Margin=new Padding(0,0,Px(8),0),UseVisualStyleBackColor=false };
            b.Click+=delegate{action();};return b;
        }
        private void OnKeyDown(object sender,KeyEventArgs e)
        {
            if(e.Control&&e.KeyCode==Keys.F){search.Focus();search.SelectAll();e.SuppressKeyPress=true;}
            else if(e.Alt&&e.KeyCode==Keys.Left){TraverseHistory(-1);e.SuppressKeyPress=true;}
            else if(e.Alt&&e.KeyCode==Keys.Right){TraverseHistory(1);e.SuppressKeyPress=true;}
            else if(e.Control&&(e.KeyCode==Keys.Add||e.KeyCode==Keys.Oemplus)){StepTextScale(1);e.SuppressKeyPress=true;}
            else if(e.Control&&(e.KeyCode==Keys.Subtract||e.KeyCode==Keys.OemMinus)){StepTextScale(-1);e.SuppressKeyPress=true;}
            else if(e.KeyCode==Keys.Escape&&(search.Focused||results.Focused)&&search.TextLength>0){search.Clear();search.Focus();e.SuppressKeyPress=true;}
            else if(e.KeyCode==Keys.F1){OpenHelp();e.SuppressKeyPress=true;}
        }
        public void ToggleContext(){SetContext(!appearance.ContextVisible,true);}
        private void SetContext(bool visible,bool save)
        {
            using(FrameRendering.Suspend(form))
            {
            appearance.ContextVisible=visible;context.Visible=visible;
            body.ColumnStyles[2].Width=visible?Px(225):0;
            contextButton.Text="";contextButton.AccessibleName=visible?"Hilfe ausblenden":"Hilfe einblenden";tips.SetToolTip(contextButton,contextButton.AccessibleName);
            if(save){appearance.Save();UpdateStatus();}
            body.PerformLayout();
            }
        }
        public void SetDark(bool dark)
        {
            if(changingTheme)return;
            if(theme.Dark==dark&&appearance.Dark==dark&&themeChoice.SelectedIndex==(dark?0:1))return;
            changingTheme=true;
            try
            {
            using(FrameRendering.Suspend(form))
            {
            appearance.Dark=dark;appearance.ThemeMode=dark?"Dunkel":"Hell";
            if(themeChoice.SelectedIndex!=(dark?0:1))themeChoice.SelectedIndex=dark?0:1;
            theme.Set(dark);ApplyTheme();appearance.Save();UpdateStatus();
            }
            }
            finally{changingTheme=false;}
        }
        private void ApplyTheme()
        {
            using(FrameRendering.Suspend(form))
            {
            form.SuspendLayout();
            try
            {
                theme.Apply(form);
                foreach(Control section in sections)if(!form.Contains(section))theme.Apply(section);
                foreach(Form dialog in dialogs.ToArray())if(!dialog.IsDisposed)theme.Apply(dialog);
                var p=theme.Colors;
                center.BackColor=p.Background;holder.BackColor=p.Background;tree.BackColor=p.Panel;tabs.BackColor=p.Background;
                foreach(Control c in Descendants(form))if(c.Name=="studioMuted")c.ForeColor=p.Muted;
                title.ForeColor=p.Text;breadcrumb.ForeColor=p.Muted;hint.ForeColor=p.Muted;helpText.ForeColor=p.Muted;changeText.ForeColor=p.Muted;status.ForeColor=p.Muted;
                foreach(Control b in tabs.Controls)SetTabColor(b as Button);
                navigation.Sync(tree.SelectedNode,false);
                searchBar.BackColor=search.BackColor=searchPlaceholder.BackColor=p.Input;searchPlaceholder.ForeColor=p.Muted;
            }
            finally{form.ResumeLayout(true);}
            form.Invalidate(true);
            }
        }
        public void ThemeDialog(Form dialog)
        {
            if(dialog==form||dialog.IsDisposed)return;
            if(!dialogs.Contains(dialog)){dialogs.Add(dialog);dialog.FormClosed+=delegate{dialogs.Remove(dialog);};}
            using(FrameRendering.Suspend(dialog))theme.Apply(dialog);
        }
        private void DrawNode(object sender,DrawTreeNodeEventArgs e)
        {
            bool selected=tree.SelectedNode==e.Node;var p=theme.Colors;
            // Paint the entire row, including indentation. Native partial painting otherwise
            // leaves a system-colored selection block beside the themed label.
            var row=new Rectangle(0,e.Bounds.Top,tree.ClientSize.Width,e.Bounds.Height);
            var bounds=new Rectangle(e.Node.Bounds.Left,row.Top,Math.Max(0,row.Width-e.Node.Bounds.Left-8),row.Height);
            using(var brush=new SolidBrush(selected?p.Selection:p.Panel))e.Graphics.FillRectangle(brush,row);
            if(selected)using(var brush=new SolidBrush(p.Accent))e.Graphics.FillRectangle(brush,0,row.Top,3,row.Height);
            if(tree.ShowPlusMinus&&e.Node.Nodes.Count>0)
            {
                int x=e.Node.Bounds.Left-tree.Indent/2,y=row.Top+row.Height/2;
                using(var pen=new Pen(selected?p.Accent:p.Muted,1.4f))
                {
                    if(e.Node.IsExpanded){e.Graphics.DrawLine(pen,x-4,y-2,x,y+2);e.Graphics.DrawLine(pen,x,y+2,x+4,y-2);}
                    else {e.Graphics.DrawLine(pen,x-2,y-4,x+2,y);e.Graphics.DrawLine(pen,x+2,y,x-2,y+4);}
                }
            }
            TextRenderer.DrawText(e.Graphics,e.Node.Text,e.Node.Parent==null?treeRootFont:font,bounds,selected?p.Accent:p.Text,TextFormatFlags.VerticalCenter|TextFormatFlags.Left|TextFormatFlags.EndEllipsis);
        }
        private void InvalidateSelection(TreeNode node)
        {
            if(node!=null&&node.TreeView==tree&&!node.Bounds.IsEmpty)
                tree.Invalidate(new Rectangle(0,node.Bounds.Top,tree.ClientSize.Width,tree.ItemHeight));
        }
        private void RefreshPage()
        {
            TreeNode node=tree.SelectedNode;if(node==null)return;
            CorrectStoredSettingDisplay(NodeSection(node));
            navigation.Sync(node,false);RecordHistory(node);
            breadcrumb.Text="Konfiguration  /  "+navigation.Group(node);title.Text=navigation.Label(node);pageLead.Text=Lead(node);pageIcon.Invalidate();
            TreeNode group=node.Parent??node;
            TreeNode[] requestedTabs=(new[]{group}).Concat(group.Nodes.Cast<TreeNode>()).ToArray();
            if(!requestedTabs.SequenceEqual(displayedTabs))
            {
                tabs.SuspendLayout();
                try
                {
                    foreach(Control c in tabs.Controls.Cast<Control>().ToArray())c.Dispose();
                    foreach(TreeNode tabNode in requestedTabs)AddTab(tabNode,node);
                    tabs.Visible=group.Nodes.Count>0;displayedTabs=requestedTabs;
                }
                finally{tabs.ResumeLayout(true);}
            }
            else foreach(Button button in tabs.Controls.OfType<Button>())SetTabColor(button);
            helpTitle.Text=navigation.Label(node);fieldHelp="";helpText.Text=SectionHelp(node);
            foreach(Control c in holder.Controls){Watch(c);theme.Prepare(c);}
            if(holder.Controls.Count>0)pageZoom.EnsureContentFits(holder.Controls[0]);
            LayoutNativePage();
            UpdateStatus();
        }
        private void AddTab(TreeNode node,TreeNode selected)
        {
            string caption=navigation.Label(node);var button=new StudioTabButton {Text=caption,Colors=()=>theme.Colors,Font=font,Width=Math.Min(Px(240),Math.Max(Px(80),TextRenderer.MeasureText(caption,font).Width+Px(30))),UseVisualStyleBackColor=false};
            button.Click+=delegate{Navigate(node);};
            button.Tag=node;button.Height=Px(40);button.Margin=new Padding(0,0,Px(8),Px(9));button.AccessibleName="Bereich "+caption;tips.SetToolTip(button,node.FullPath);
            tabs.Controls.Add(button);theme.Apply(button);SetTabColor(button);
        }
        private void SetTabColor(Button b)
        {
            if(b==null)return;bool active=Object.ReferenceEquals(b.Tag,tree.SelectedNode);b.BackColor=theme.Colors.Background;b.ForeColor=active?theme.Colors.Accent:theme.Colors.Muted;b.FlatAppearance.BorderSize=0;var tab=b as StudioTabButton;if(tab!=null){tab.Selected=active;tab.Invalidate();}
        }
        private bool Navigate(TreeNode node)
        {
            if(node==null||node.TreeView!=tree)return false;
            switching=true;
            try{tree.SelectedNode=node;node.EnsureVisible();}
            finally{switching=false;}
            if(tree.SelectedNode!=node)return false;
            return true;
        }
        private static string SectionHelp(TreeNode node)
        {
            string root=node.FullPath.ToLowerInvariant();
            if(root.Contains("codec")||root.Contains("renderer"))return "Wähle die installierten Decoder und die Bild- und Tonausgabe. F1 öffnet die Dokumentation für den gewählten Bereich.";
            if(root.Contains("plugin"))return "Wähle eine Erweiterung aus, um sie zu aktivieren oder ihre Konfiguration zu öffnen. Einzelne Plugins speichern direkt in ihrem eigenen Dialog.";
            if(root.Contains("gui")||root.Contains("skin"))return "Wähle die Oberfläche und das Verhalten von MediaPortal. Hell/Dunkel oben rechts ändert nur dieses Konfigurationsfenster. Die Medienoberfläche wird davon nicht umgestellt.";
            if(root.Contains("tv")||root.Contains("radio"))return "Hier bleiben die Einstellungen des TV-/Radio-Clients erreichbar. Die separate TV-Server-Konfiguration gehört weiterhin zum TV-Server.";
            if(root.Contains("keyboard")||root.Contains("remote")||root.Contains("keys"))return "Richte Tastaturkürzel und Fernbedienung ein. Über die Suche findest du Tastenbelegungen und einzelne Optionen schneller.";
            if(root.Contains("folder")||root.Contains("picture"))return "Verwalte deine Medienquellen und Bilder. Änderungen an Ordnern und Bibliotheken nimmst du hier für den jeweiligen Medientyp vor.";
            if(root.Contains("general"))return "Die Registerkarten führen zu Lautstärke, Tastaturkürzeln, Bildwiederholrate und Startverhalten. Mit Strg+F kannst du auch einzelne Einstellungen suchen.";
            return "Wähle einen Unterbereich oder suche mit Strg+F nach einer Einstellung. F1 öffnet die passende Dokumentation.";
        }
        private void OpenHelp()
        {
            var help=nativeTools.Items.Find("helpToolStripSplitButton",true).FirstOrDefault();
            if(help!=null)ClickNative(help);
        }
        private static IEnumerable<TreeNode> Flatten(TreeNodeCollection nodes)
        {
            foreach(TreeNode node in nodes){yield return node;foreach(TreeNode child in Flatten(node.Nodes))yield return child;}
        }
        private static IEnumerable<Control> Descendants(Control control)
        {
            foreach(Control child in control.Controls){yield return child;foreach(Control c in Descendants(child))yield return c;}
        }
        private static Control NodeSection(TreeNode node)
        {
            var p=node.GetType().GetProperty("Section");return p==null?node.Tag as Control:p.GetValue(node,null) as Control;
        }
        private void UpdateSearch()
        {
            string query=search.Text.Trim();bool has=query.Length>0;
            tree.Visible=false;navigation.Visible=!has;results.Visible=has;results.Items.Clear();
            if(!has)return;
            foreach(TreeNode node in Flatten(tree.Nodes))
            {
                if(node.FullPath.IndexOf(query,StringComparison.CurrentCultureIgnoreCase)>=0||navigation.Label(node).IndexOf(query,StringComparison.CurrentCultureIgnoreCase)>=0||navigation.Group(node).IndexOf(query,StringComparison.CurrentCultureIgnoreCase)>=0)
                {results.Items.Add(new SearchResult(node,null,navigation.Group(node)+" / "+navigation.Label(node)));continue;}
                Control section=NodeSection(node);if(section==null)continue;
                foreach(Control c in Descendants(section))
                {
                    var checks=c as CheckedListBox;
                    if(checks!=null)
                    {
                        for(int item=0;item<checks.Items.Count;item++)
                        {
                            string option=checks.GetItemText(checks.Items[item]);
                            if(option.IndexOf(query,StringComparison.CurrentCultureIgnoreCase)>=0)results.Items.Add(new SearchResult(node,c,node.Text+" · "+option,item));
                            if(results.Items.Count>=150)break;
                        }
                        continue;
                    }
                    if(!(c is Label)&&!(c is CheckBox)&&!(c is RadioButton)&&!(c is GroupBox)&&!(c is Button))continue;
                    string text=c.Text.Replace("&","").Trim();
                    if(text.Length>0&&text.IndexOf(query,StringComparison.CurrentCultureIgnoreCase)>=0)
                    {results.Items.Add(new SearchResult(node,c,node.Text+" · "+text));if(results.Items.Count>=150)break;}
                }
                if(results.Items.Count>=150)break;
            }
            if(results.Items.Count==0)results.Items.Add(new SearchResult(null,null,"Keine Treffer in dieser Ansicht"));
            tips.SetToolTip(results,"Doppelklick oder Enter öffnet den Treffer. Für zusätzliche Bereiche oben in den Expertenmodus wechseln.");
        }
        private void OpenResult()
        {
            var result=results.SelectedItem as SearchResult;if(result==null||result.Node==null)return;
            if(Navigate(result.Node))
            {
                search.Clear();
                if(result.Control!=null&&!result.Control.IsDisposed){holder.ScrollControlIntoView(result.Control);var checks=result.Control as CheckedListBox;if(checks!=null&&result.ItemIndex>=0&&result.ItemIndex<checks.Items.Count)checks.SelectedIndex=result.ItemIndex;Control target=result.Control.CanFocus?result.Control:result.Control.Parent;target.Focus();helpText.Text="Gefundene Option: "+result.Label+"\r\n\r\n"+SectionHelp(result.Node);}
            }
        }
        private static string Value(Control c)
        {
            if(c is CheckedListBox)return String.Join(",",((CheckedListBox)c).CheckedIndices.Cast<int>().Select(i=>i.ToString(System.Globalization.CultureInfo.InvariantCulture)));
            if(c is CheckBox)return ((CheckBox)c).CheckState.ToString();
            if(c is RadioButton)return ((RadioButton)c).Checked.ToString();
            if(c is NumericUpDown)return ((NumericUpDown)c).Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
            if(c is TrackBar)return ((TrackBar)c).Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
            if(c is TextBoxBase||c is ComboBox)return c.Text;
            return null;
        }
        private void Watch(Control c)
        {
            if(observed.Add(c))
            {
                string value=Value(c);if(value!=null)baselines[c]=value;
                c.Enter+=delegate{ShowFieldHelp(c);};
                var nativeTabs=c as TabControl;if(nativeTabs!=null)nativeTabs.SelectedIndexChanged+=delegate{if(!building&&!settingSize&&holder.Controls.Count>0){pageZoom.EnsureContentFits(holder.Controls[0]);LayoutNativePage();}};
                EventHandler changed=delegate{OnValueChanged(c);};
                if(c is CheckedListBox)((CheckedListBox)c).ItemCheck+=delegate{if(building||switching)return;if(c.IsHandleCreated)c.BeginInvoke((Action)delegate{OnValueChanged(c);});};
                else if(c is CheckBox)((CheckBox)c).CheckStateChanged+=changed;
                else if(c is RadioButton)((RadioButton)c).CheckedChanged+=changed;
                else if(c is NumericUpDown)((NumericUpDown)c).ValueChanged+=changed;
                else if(c is TrackBar)((TrackBar)c).ValueChanged+=changed;
                else if(c is TextBoxBase||c is ComboBox)c.TextChanged+=changed;
                c.ControlAdded+=delegate(object sender,ControlEventArgs e){Watch(e.Control);};
                c.Disposed+=delegate{edits.Remove(c);observed.Remove(c);baselines.Remove(c);userTouched.Remove(c);};
            }
            foreach(Control child in c.Controls)Watch(child);
        }
        private void OnValueChanged(Control c)
        {
            if(building||switching||initializing||c.IsDisposed)return;
            string baseline;if(!baselines.TryGetValue(c,out baseline))return;
            if(Value(c)==baseline)edits.Remove(c);else edits.Add(c);
            UpdateStatus();
        }
        private string FieldLabel(Control c)
        {
            if(c is CheckBox||c is RadioButton)return c.Text.Replace("&","");
            if(!String.IsNullOrEmpty(c.AccessibleName))return c.AccessibleName;
            if(c.Parent!=null)
            {
                var label=c.Parent.Controls.OfType<Label>().Where(l=>!String.IsNullOrWhiteSpace(l.Text)&&l.Top<=c.Bottom&&Math.Abs(l.Top-c.Top)<45).OrderBy(l=>Math.Abs(l.Top-c.Top)+Math.Abs(l.Right-c.Left)).FirstOrDefault();
                if(label!=null)return label.Text.Replace("&","");
            }
            return c.Name;
        }
        private void ShowFieldHelp(Control c)
        {
            if(building||helpTitle==null||helpText==null||c.IsDisposed)return;
            string label=FieldLabel(c);if(String.IsNullOrWhiteSpace(label))return;
            helpTitle.Text=tree.SelectedNode==null?"Einstellung":navigation.Label(tree.SelectedNode);
            fieldHelp=label;helpText.Text="Ausgewählte Option\r\n"+label+"\r\n\r\nDie fachliche Erklärung findest du über die Online-Hilfe für diesen Bereich.";
        }
        private void UpdateStatus()
        {
            if(status==null)return;
            string info=edits.Count==0?"Bereit · Keine offenen Änderungen":edits.Count+" ungespeicherte Änderungen";
            status.Text=info;
            if(!String.IsNullOrEmpty(appearance.LastError))status.Text="Darstellung konnte nicht gespeichert werden: "+appearance.LastError;
            var names=edits.Where(c=>!c.IsDisposed).Select(FieldLabel).Where(s=>!String.IsNullOrWhiteSpace(s)).Distinct().Take(9).ToArray();
            changeText.Text=names.Length==0?"Keine Feldänderungen erkannt.\r\n\r\nPlugin-Dialoge können eigene Speicheraktionen haben.":String.Join("\r\n",names)+"\r\n\r\nSpeichern über OK.";
        }
        public void StartAudit(string path)
        {
            // Read-only inventory: no page activation, save action, setting value or plugin execution.
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));
            using(var w=XmlWriter.Create(path,new XmlWriterSettings{Indent=true}))
            {
                w.WriteStartElement("configurationStudioAudit");w.WriteAttributeString("runtime",AppDomain.CurrentDomain.BaseDirectory);
                w.WriteAttributeString("entryAssembly",Assembly.GetEntryAssembly()==null?"":Assembly.GetEntryAssembly().Location);
                w.WriteAttributeString("nativeSectionCount",sections.Count.ToString());w.WriteAttributeString("visibleTreeNodes",NativeTreeCount.ToString());
                w.WriteAttributeString("originalOk",nativeOk.GetType().FullName);w.WriteAttributeString("originalCancel",nativeCancel.GetType().FullName);
                foreach(Control section in sections){w.WriteStartElement("section");w.WriteAttributeString("type",section.GetType().FullName);w.WriteAttributeString("title",section.Text);w.WriteAttributeString("controls",Descendants(section).Count().ToString());w.WriteAttributeString("nativeAssembly",section.GetType().Assembly.Location);w.WriteEndElement();}
                w.WriteEndElement();
            }
        }
        public void StartNativeChecks(string path)
        {
            string runtime=AppDomain.CurrentDomain.BaseDirectory;
            if(!File.Exists(Path.Combine(runtime,"studio-test-runtime.marker")))throw new InvalidOperationException("Native verification is allowed only in a marked, isolated test runtime.");
            // These are in-process application tests, not a substitute for external UI inspection.
            var timer=new System.Windows.Forms.Timer { Interval=600 };
            timer.Tick+=delegate {
                timer.Stop();timer.Dispose();
                var checks=new List<KeyValuePair<string,bool>>();
                string error=null;
                try
                {
                    checks.Add(new KeyValuePair<string,bool>("Original native entry assembly",Path.GetFileName(Assembly.GetEntryAssembly().Location).Equals("Configuration.exe",StringComparison.OrdinalIgnoreCase)));
                    checks.Add(new KeyValuePair<string,bool>("Process startup directory matches native runtime",Path.GetFullPath(Application.StartupPath).TrimEnd('\\').Equals(runtime.TrimEnd('\\'),StringComparison.OrdinalIgnoreCase)));
                    var config=Assembly.Load("Utils").GetType("MediaPortal.Configuration.Config");var dir=config.GetNestedType("Dir");
                    string isolatedRoot=Path.GetFullPath(Path.Combine(runtime,"..")).TrimEnd('\\')+"\\";
                    foreach(string kind in new[]{"Config","Log","Database","Thumbs","Cache"})
                    {
                        string location=Convert.ToString(config.GetMethod("GetFolder").Invoke(null,new[]{Enum.Parse(dir,kind)}));
                        if(!Path.GetFullPath(location).StartsWith(isolatedRoot,StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("Refusing verification with a non-isolated "+kind+" path.");
                    }
                    checks.Add(new KeyValuePair<string,bool>("Mutable native paths isolated",true));
                    checks.Add(new KeyValuePair<string,bool>("Native debug marker path isolated",Path.GetFullPath(IsolatedRuntime.DebugMarkerPath()).StartsWith(isolatedRoot,StringComparison.OrdinalIgnoreCase)));
                    checks.Add(new KeyValuePair<string,bool>("Studio shell prepared before the first native display",PreparedBeforeFirstDisplay));
                    checks.Add(new KeyValuePair<string,bool>("Top-level native child composition is enabled",FrameRendering.IsComposited(form)));
                    checks.Add(new KeyValuePair<string,bool>("Native startup completed with clean loaded input baselines",!initializing&&ChangedCount==0));
                    checks.Add(new KeyValuePair<string,bool>("Icon navigation retains the native tree and groups all visible pages",navigation.Visible&&!tree.Visible&&Flatten(tree.Nodes).All(n=>!String.IsNullOrEmpty(navigation.Group(n)))));
                    var inputs=baselines.Keys.Where(c=>!c.IsDisposed).ToDictionary(c=>c,Value);
                    SetDark(false);checks.Add(new KeyValuePair<string,bool>("Light mode preserves all native input values",inputs.All(x=>Value(x.Key)==x.Value)));
                    SetDark(true);checks.Add(new KeyValuePair<string,bool>("Dark mode preserves all native input values",inputs.All(x=>Value(x.Key)==x.Value)));
                    int textScale=appearance.TextScale;SetTextScale(150);checks.Add(new KeyValuePair<string,bool>("Enlarging native pages preserves all captured configuration values",inputs.All(x=>Value(x.Key)==x.Value)));SetTextScale(100);SetTextScale(textScale);checks.Add(new KeyValuePair<string,bool>("Changing and restoring readability scale preserves native values",inputs.All(x=>Value(x.Key)==x.Value)));
                    int before=NativeTreeCount;
                    int width=center.Width;bool wasVisible=ContextVisible;if(!wasVisible)ToggleContext();
                    width=center.Width;ToggleContext();checks.Add(new KeyValuePair<string,bool>("Collapsing context expands content",center.Width>width));ToggleContext();
                    var references=sections.ToArray();
                    foreach(string name in new[]{"General","Gui","GuiLanguage","MoviePlayer","Music","Pictures","Remote","FiltersSection","PluginsNew","GeneralDaemonTools"})
                    {
                        TreeNode node=Flatten(tree.Nodes).FirstOrDefault(n=>NodeSection(n)!=null&&NodeSection(n).GetType().Name==name);
                        bool navigated=node!=null&&Navigate(node);
                        checks.Add(new KeyValuePair<string,bool>("Native page activation: "+name,navigated&&holder.Controls.Count>0&&Object.ReferenceEquals(holder.Controls[0],NodeSection(node))));
                    }
                    search.Text="Enable skin sound";checks.Add(new KeyValuePair<string,bool>("Search includes native checked-list options",results.Items.Cast<SearchResult>().Any(r=>r.Node!=null)));search.Clear();
                    TreeNode bdZoom=Flatten(tree.Nodes).FirstOrDefault(n=>NodeSection(n)!=null&&NodeSection(n).GetType().Name=="BDZoom");
                    bool bdChoicePreserved=false;
                    if(bdZoom!=null&&Navigate(bdZoom))
                    {
                        var choice=Descendants(NodeSection(bdZoom)).OfType<ComboBox>().First(c=>c.Name=="defaultZoomModeComboBox");int originalIndex=choice.SelectedIndex;
                        if(choice.Items.Count>1){int selected=(originalIndex+1)%choice.Items.Count;choice.SelectedIndex=selected;Navigate(Flatten(tree.Nodes).First(n=>NodeSection(n)!=null&&NodeSection(n).GetType().Name=="General"));Navigate(bdZoom);bdChoicePreserved=choice.SelectedIndex==selected;choice.SelectedIndex=originalIndex;}
                    }
                    checks.Add(new KeyValuePair<string,bool>("Unsaved Blu-ray aspect-ratio selection survives native page navigation",bdChoicePreserved));
                    checks.Add(new KeyValuePair<string,bool>("All native pages and navigation retained",before==NativeTreeCount&&references.SequenceEqual(sections)));
                    checks.Add(new KeyValuePair<string,bool>("Native OK and Cancel are still wired as dialog buttons",Object.ReferenceEquals(form.AcceptButton,nativeOk)&&Object.ReferenceEquals(form.CancelButton,nativeCancel)));
                }
                catch(Exception ex){error=ex.ToString();checks.Add(new KeyValuePair<string,bool>("Unexpected verification exception",false));}
                Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));
                using(var w=XmlWriter.Create(path,new XmlWriterSettings{Indent=true}))
                {
                    w.WriteStartElement("nativeChecks");w.WriteAttributeString("passed",checks.Count(c=>c.Value).ToString());w.WriteAttributeString("total",checks.Count.ToString());
                    foreach(var check in checks){w.WriteStartElement("check");w.WriteAttributeString("name",check.Key);w.WriteAttributeString("passed",check.Value.ToString());w.WriteEndElement();}
                    if(error!=null)w.WriteElementString("error",error);w.WriteEndElement();
                }
                nativeCancel.PerformClick();
            };
            timer.Start();
        }
        private sealed class SearchResult
        {
            public readonly TreeNode Node;public readonly Control Control;public readonly int ItemIndex;public string Label{get;private set;}
            public SearchResult(TreeNode node,Control control,string label,int itemIndex=-1){Node=node;Control=control;Label=label;ItemIndex=itemIndex;}
            public override string ToString(){return Label;}
        }
    }
}


using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MP1.ConfigurationStudio
{
    public sealed partial class StudioShell
    {
        private StudioNavigation navigation;
        private ComboBox sizeChoice,modeChoice;
        private StudioAction backButton,forwardButton;
        private Panel pageIcon;
        private Panel searchBar;
        private FlowLayoutPanel options;
        private StudioScrollHost contentScroll;
        private Label pageLead;
        private Label searchPlaceholder;
        private readonly PageZoom pageZoom=new PageZoom();
        private readonly List<Font> shellFonts=new List<Font>();
        private readonly List<TreeNode> history=new List<TreeNode>();
        private int historyIndex=-1;private bool traversingHistory,settingSize;
        private bool layingOutPage;
        private bool layingOutToolbar;
        private bool layingOutContext;
        private FlowLayoutPanel helpFlow;
        private StudioScrollHost helpScroll;
        private float UiScale {get{return appearance.TextScale/100f;}}
        private float PageScale {get{return UiScale*9.5f/8.25f;}}
        private int Px(float value){return (int)Math.Round(value*UiScale);}
        private Font MakeFont(float size,FontStyle style){var result=new Font("Segoe UI",size,style);shellFonts.Add(result);return result;}
        private void InitializeTypography()
        {font=MakeFont(10*UiScale,FontStyle.Regular);labelFont=MakeFont(9*UiScale,FontStyle.Regular);headingFont=MakeFont(22*UiScale,FontStyle.Regular);treeRootFont=font;}
        [DllImport("user32.dll",EntryPoint="SendMessageW",CharSet=CharSet.Unicode)] private static extern IntPtr SendCue(IntPtr h,int m,IntPtr w,string text);
        private void Build()
        {
            form.Text="MP1 Configuration Studio · MediaPortal 1.39";form.AutoScroll=false;form.MinimumSize=new Size(1150,740);
            Rectangle screen=Screen.FromControl(form).WorkingArea;form.Size=new Size(Math.Min(screen.Width,appearance.Width),Math.Min(screen.Height,appearance.Height));form.StartPosition=FormStartPosition.CenterScreen;
            form.Controls.Clear();
            root=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=4,Margin=Padding.Empty,Padding=Padding.Empty,Font=font,Name="studioRoot"};
            root.RowStyles.Add(new RowStyle(SizeType.Absolute,Px(66)));root.RowStyles.Add(new RowStyle(SizeType.Absolute,Px(56)));root.RowStyles.Add(new RowStyle(SizeType.Percent,100));root.RowStyles.Add(new RowStyle(SizeType.Absolute,Px(58)));root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));form.Controls.Add(root);
            var backend=new Panel {Visible=false,Name="studioNativeBackend",Size=new Size(1,1)};form.Controls.Add(backend);tree.Parent=backend;tree.Visible=false;nativeTools.Parent=backend;nativeTools.Visible=false;if(donate!=null)donate.Parent=backend;
            header=new Panel {Dock=DockStyle.Fill,Margin=Padding.Empty};
            header.Controls.Add(new StudioLogo {Colors=()=>theme.Colors,Bounds=new Rectangle(Px(23),Px(14),Px(36),Px(36)),Font=MakeFont(13*UiScale,FontStyle.Bold)});
            header.Controls.Add(new Label {Text="MP1 / CONFIGURATION",AutoSize=true,Location=new Point(Px(72),Px(10)),Font=MakeFont(15*UiScale,FontStyle.Bold)});
            header.Controls.Add(new Label {Text="MEDIAPORTAL · STUDIO",AutoSize=true,Location=new Point(Px(74),Px(39)),Font=MakeFont(10*UiScale,FontStyle.Regular),UseCompatibleTextRendering=true,Name="studioBrandSubtitle"});root.Controls.Add(header,0,0);
            toolbar=new Panel {Dock=DockStyle.Fill,Margin=Padding.Empty};root.Controls.Add(toolbar,0,1);
            backButton=new StudioAction {Name="studioHistoryBack",Text="",IconName="back",Colors=()=>theme.Colors,Size=new Size(Px(35),Px(35)),Location=new Point(Px(13),Px(8)),AccessibleName="Zurück",Enabled=false};backButton.Click+=delegate{TraverseHistory(-1);};
            forwardButton=new StudioAction {Name="studioHistoryForward",Text="",IconName="forward",Colors=()=>theme.Colors,Size=backButton.Size,Location=new Point(Px(48),Px(8)),AccessibleName="Vorwärts",Enabled=false};forwardButton.Click+=delegate{TraverseHistory(1);};toolbar.Controls.Add(backButton);toolbar.Controls.Add(forwardButton);
            tips.SetToolTip(backButton,"Zurück · Alt+Links");tips.SetToolTip(forwardButton,"Vorwärts · Alt+Rechts");
            options=new FlowLayoutPanel {Dock=DockStyle.None,Width=Px(550),WrapContents=false,Padding=new Padding(0,Px(8),Px(12),0),Margin=Padding.Empty};toolbar.Controls.Add(options);
            modeChoice=Choice("studioMode","Standard- oder Expertenansicht",100,new object[]{"Standard","Experte"});modeChoice.SelectedIndex=IsExpert()?1:0;modeChoice.SelectedIndexChanged+=delegate{SetMode(modeChoice.SelectedIndex==1);};
            sizeChoice=Choice("studioTextSize","Schrift und Oberfläche vergrößern",110,new object[]{"Größe 100%","Größe 115%","Größe 125%","Größe 150%","Größe 175%"});sizeChoice.SelectedIndex=Array.IndexOf(new[]{100,115,125,150,175},appearance.TextScale);if(sizeChoice.SelectedIndex<0)sizeChoice.SelectedIndex=2;sizeChoice.SelectedIndexChanged+=delegate{if(!settingSize&&sizeChoice.SelectedIndex>=0)SetTextScale(new[]{100,115,125,150,175}[sizeChoice.SelectedIndex]);};
            themeChoice=Choice("studioTheme","Hell oder Dunkel",85,new object[]{"Dunkel","Hell"});themeChoice.SelectedIndex=appearance.Dark?0:1;themeChoice.SelectedIndexChanged+=delegate{SetDark(themeChoice.SelectedIndex==0);};
            contextButton=ActionButton("",ToggleContext,Px(43));contextButton.Name="studioContextToggle";contextButton.AccessibleName="Hilfe ein- oder ausblenden";((StudioAction)contextButton).IconName="help";tips.SetToolTip(contextButton,"Rechte Hilfespalte ein- oder ausblenden");
            var more=(StudioAction)ActionButton("Werkzeuge",delegate{},Px(140));more.IconName="settings";more.Click+=delegate{ShowNativeTools(more);};
            options.Controls.Add(modeChoice);options.Controls.Add(sizeChoice);options.Controls.Add(themeChoice);options.Controls.Add(contextButton);options.Controls.Add(more);
            searchBar=new Panel {Name="studioSearchBar",Height=Px(36),Padding=new Padding(Px(34),Px(7),Px(7),Px(5))};
            search=new TextBox {Dock=DockStyle.Fill,BorderStyle=BorderStyle.None,AutoSize=false,Font=font,Name="studioSearch",AccessibleName="Einstellung oder Bereich suchen"};
            search.HandleCreated+=delegate{SendCue(search.Handle,0x1501,IntPtr.Zero,"");};
            clearSearch=ActionButton("×",delegate{search.Clear();search.Focus();},Px(25));clearSearch.Dock=DockStyle.Right;clearSearch.Visible=false;clearSearch.AccessibleName="Suche zurücksetzen";searchBar.Controls.Add(search);searchBar.Controls.Add(clearSearch);
            searchPlaceholder=new Label {Text="Einstellungen suchen …  Strg+F",Font=font,UseMnemonic=false,AutoEllipsis=true,TextAlign=ContentAlignment.MiddleLeft,Name="studioSearchPlaceholder"};searchBar.Controls.Add(searchPlaceholder);searchPlaceholder.BringToFront();searchPlaceholder.Click+=delegate{search.Focus();};searchBar.Layout+=delegate{searchPlaceholder.Bounds=search.Bounds;};search.Enter+=delegate{searchPlaceholder.Visible=false;};search.Leave+=delegate{searchPlaceholder.Visible=search.TextLength==0;};
            searchBar.Paint+=delegate(object sender,PaintEventArgs e){e.Graphics.Clear(theme.Colors.Input);using(var path=StudioIcons.Rounded(new RectangleF(0,0,searchBar.Width-1,searchBar.Height-1),5))using(var pen=new Pen(theme.Colors.Border))e.Graphics.DrawPath(pen,path);StudioIcons.Draw(e.Graphics,"search",new Rectangle(Px(10),Px(10),Px(16),Px(16)),theme.Colors.Muted);};
            toolbar.Controls.Add(searchBar);toolbar.Resize+=delegate{LayoutToolbar();};
            toolbar.Paint+=delegate(object sender,PaintEventArgs e){using(var pen=new Pen(theme.Colors.Border))e.Graphics.DrawLine(pen,0,toolbar.Height-1,toolbar.Width,toolbar.Height-1);};
            body=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=3,RowCount=1,Margin=Padding.Empty};body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,Px(250)));body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,Px(225)));body.RowStyles.Add(new RowStyle(SizeType.Percent,100));root.Controls.Add(body,0,2);
            sidebar=new Panel {Dock=DockStyle.Fill,Padding=new Padding(Px(10),Px(10),Px(6),Px(10)),Margin=Padding.Empty};navigation=new StudioNavigation(tree,NodeSection,Navigate,()=>theme.Colors,InvokeNativeTool){Font=font};sidebar.Controls.Add(navigation);
            results=new ListBox {Dock=DockStyle.Fill,Visible=false,IntegralHeight=false,BorderStyle=BorderStyle.None,Font=font,Name="studioSearchResults",DisplayMember="Label",AccessibleName="Suchergebnisse"};sidebar.Controls.Add(results);
            results.DoubleClick+=delegate{OpenResult();};results.KeyDown+=delegate(object sender,KeyEventArgs e){if(e.KeyCode==Keys.Enter){e.SuppressKeyPress=true;OpenResult();}};
            search.TextChanged+=delegate{clearSearch.Visible=search.TextLength>0;searchPlaceholder.Visible=search.TextLength==0&&!search.Focused;UpdateSearch();};search.KeyDown+=delegate(object sender,KeyEventArgs e){if((e.KeyCode==Keys.Down||e.KeyCode==Keys.Enter)&&results.Visible&&results.Items.Count>0){results.SelectedIndex=0;results.Focus();if(e.KeyCode==Keys.Enter)OpenResult();e.SuppressKeyPress=true;}};
            center=new Panel {Dock=DockStyle.Fill,Name="studioContent",Margin=Padding.Empty,Padding=new Padding(Px(24),Px(21),Px(24),Px(14))};
            var headings=new Panel {Dock=DockStyle.Top,Height=Px(99)};breadcrumb=new Label {AutoEllipsis=true,Dock=DockStyle.Top,Height=Px(23),Font=labelFont,Name="studioMuted"};
            pageLead=new Label {Dock=DockStyle.Bottom,Height=Px(27),Font=labelFont,Name="studioMuted",AutoEllipsis=true};
            var titleRow=new Panel {Dock=DockStyle.Fill};pageIcon=new Panel {Dock=DockStyle.Left,Width=Px(42)};pageIcon.Paint+=delegate(object sender,PaintEventArgs e){if(tree.SelectedNode!=null)StudioIcons.Draw(e.Graphics,navigation.Icon(tree.SelectedNode),new Rectangle(0,Px(9),Px(29),Px(29)),theme.Colors.Accent);};title=new Label {Dock=DockStyle.Fill,Font=headingFont,AutoEllipsis=true,UseMnemonic=false,Name="studioPageTitle"};titleRow.Controls.Add(title);titleRow.Controls.Add(pageIcon);headings.Controls.Add(titleRow);headings.Controls.Add(pageLead);headings.Controls.Add(breadcrumb);
            tabs=new FlowLayoutPanel {Dock=DockStyle.Top,AutoSize=true,AutoSizeMode=AutoSizeMode.GrowAndShrink,WrapContents=true,AutoScroll=false,AccessibleName="Unterbereiche",Margin=Padding.Empty};
            holder.Padding=Padding.Empty;holder.Margin=Padding.Empty;holder.AutoScroll=true;contentScroll=new StudioScrollHost(holder,()=>holder.AutoScrollMinSize,()=>theme.Colors,true);center.Controls.Add(contentScroll);holder.SizeChanged+=delegate{LayoutNativePage();};center.Controls.Add(tabs);center.Controls.Add(headings);
            context=new Panel {Dock=DockStyle.Fill,Margin=Padding.Empty,Padding=new Padding(Px(20),Px(22),Px(18),Px(12)),Name="studioContext",AccessibleName="Hilfe und Kontext"};
            contextCaption=new Label {Text="HILFE & KONTEXT",Dock=DockStyle.Top,Height=Px(31),Font=labelFont,UseMnemonic=false,Name="studioMuted"};helpTitle=new Label {Dock=DockStyle.Top,Height=Px(46),Font=MakeFont(11*UiScale,FontStyle.Bold),UseMnemonic=false,AutoEllipsis=true};helpText=new Label {Dock=DockStyle.Top,Height=Px(147),Font=font};
            var help=ActionButton("Online-Hilfe öffnen",OpenHelp,Px(180));help.Dock=DockStyle.Top;var changesHeading=new Label {Text="ÄNDERUNGEN",Dock=DockStyle.Top,Height=Px(44),Padding=new Padding(0,Px(16),0,0),Font=labelFont,Name="studioMuted"};changeText=new Label {Dock=DockStyle.Fill,Font=font,AutoEllipsis=true};hint=new Label {Text="OK speichert mit der originalen MediaPortal-Prüfung. Abbrechen verwirft die Einstellungen.",Dock=DockStyle.Bottom,Height=Px(97),Font=labelFont,Name="studioMuted"};
            context.Controls.Add(changeText);context.Controls.Add(hint);context.Controls.Add(changesHeading);context.Controls.Add(help);context.Controls.Add(helpText);context.Controls.Add(helpTitle);context.Controls.Add(contextCaption);body.Controls.Add(sidebar,0,0);body.Controls.Add(center,1,0);body.Controls.Add(context,2,0);
            helpFlow=new FlowLayoutPanel {FlowDirection=FlowDirection.TopDown,WrapContents=false,AutoScroll=true,Padding=new Padding(0,0,0,SystemInformation.HorizontalScrollBarHeight)};
            foreach(Control item in new Control[]{contextCaption,helpTitle,helpText,help,changesHeading,changeText,hint})
            {
                item.Parent=helpFlow;item.Dock=DockStyle.None;item.Margin=new Padding(0,0,0,Px(18));
                var label=item as Label;if(label!=null){label.Padding=Padding.Empty;label.AutoSize=true;label.UseMnemonic=false;}
            }
            helpScroll=new StudioScrollHost(helpFlow,()=>new Size(0,Math.Max(0,helpFlow.GetPreferredSize(new Size(Math.Max(1,helpFlow.ClientSize.Width-10),0)).Height-SystemInformation.HorizontalScrollBarHeight)),()=>theme.Colors,false);context.Controls.Add(helpScroll);helpFlow.SizeChanged+=delegate{LayoutHelp();};
            footer=new Panel {Dock=DockStyle.Fill,Padding=new Padding(Px(23),Px(11),Px(18),Px(10)),Margin=Padding.Empty};var actions=new FlowLayoutPanel {Dock=DockStyle.Right,FlowDirection=FlowDirection.RightToLeft,WrapContents=false,Width=Px(290)};
            nativeOk.Parent=actions;nativeOk.Text="&OK – Speichern";nativeOk.Size=new Size(Px(151),Px(36));nativeOk.Font=font;nativeOk.Margin=new Padding(Px(10),0,0,0);nativeOk.Tag="studio-primary";
            nativeCancel.Parent=actions;nativeCancel.Text="&Abbrechen";nativeCancel.Size=new Size(Px(115),Px(36));nativeCancel.Font=font;nativeCancel.Margin=new Padding(Px(8),0,0,0);
            if(nativeApply!=null){nativeApply.Parent=actions;if(nativeApply.Visible){nativeApply.Size=new Size(Px(110),Px(36));actions.Width+=Px(120);}}
            status=new Label {Dock=DockStyle.Fill,AutoEllipsis=true,TextAlign=ContentAlignment.MiddleLeft,Font=labelFont,Name="studioStatus",AccessibleName="Änderungsstatus"};footer.Controls.Add(status);footer.Controls.Add(actions);root.Controls.Add(footer,0,3);
            foreach(Button button in new[]{nativeOk,nativeCancel})button.Paint+=delegate(object sender,PaintEventArgs e){var b=(Button)sender;bool primary=Object.ReferenceEquals(b,nativeOk);e.Graphics.Clear(theme.Colors.Panel);if(primary)using(var path=StudioIcons.Rounded(new RectangleF(0,0,b.Width-1,b.Height-1),6))using(var brush=new SolidBrush(theme.Colors.Primary))e.Graphics.FillPath(brush,path);TextRenderer.DrawText(e.Graphics,b.Text,b.Font,b.ClientRectangle,primary?Color.White:theme.Colors.Text,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.HidePrefix);};
            form.AcceptButton=nativeOk;form.CancelButton=nativeCancel;SetContext(appearance.ContextVisible,false);count=new Label();LayoutToolbar();
        }
        private void LayoutToolbar()
        {
            if(searchBar==null||options==null||layingOutToolbar)return;layingOutToolbar=true;
            try
            {
                int optionWidth=options.Padding.Horizontal+options.Controls.Cast<Control>().Sum(c=>c.Width+c.Margin.Horizontal);
                bool stacked=toolbar.Width<optionWidth+Px(300);
                root.RowStyles[1].Height=Px(stacked?105:56);
                options.SetBounds(Math.Max(Px(12),toolbar.Width-optionWidth),stacked?Px(48):0,optionWidth,Px(52));
                int available=stacked?toolbar.Width-Px(112):toolbar.Width-optionWidth-Px(113);
                searchBar.SetBounds(Px(92),Px(8),Math.Max(1,Math.Min(stacked?available:Px(345),available)),Px(36));
            }
            finally{layingOutToolbar=false;}
        }
        private void LayoutHelp()
        {
            if(layingOutContext||helpFlow==null)return;layingOutContext=true;
            try{int width=Math.Max(80,helpFlow.ClientSize.Width-12);foreach(Control item in helpFlow.Controls){var label=item as Label;if(label!=null){var maximum=new Size(width,0);if(label.MaximumSize!=maximum)label.MaximumSize=maximum;}else item.Width=width;}helpScroll.Arrange();}
            finally{layingOutContext=false;}
        }
        private void LayoutNativePage()
        {
            if(layingOutPage||holder.Controls.Count==0)return;layingOutPage=true;
            try{Control page=holder.Controls[0];page.Dock=DockStyle.None;page.Anchor=AnchorStyles.Top|AnchorStyles.Left;holder.AutoScrollMinSize=page.MinimumSize;if(contentScroll!=null)contentScroll.Arrange();page.SetBounds(holder.AutoScrollPosition.X,holder.AutoScrollPosition.Y,Math.Max(holder.ClientSize.Width,page.MinimumSize.Width),Math.Max(holder.ClientSize.Height,page.MinimumSize.Height));}
            finally{layingOutPage=false;}
        }
        private ComboBox Choice(string name,string label,int width,object[] items){var combo=new ComboBox {Name=name,AccessibleName=label,DropDownStyle=ComboBoxStyle.DropDownList,Width=Px(width),Font=font,Margin=new Padding(0,0,Px(8),0),FlatStyle=FlatStyle.Flat};combo.Items.AddRange(items);tips.SetToolTip(combo,label);return combo;}
        private IEnumerable<ToolStripItem> NativeItems(ToolStripItemCollection items){foreach(ToolStripItem item in items){yield return item;var dropdown=item as ToolStripDropDownItem;if(dropdown!=null)foreach(ToolStripItem child in NativeItems(dropdown.DropDownItems))yield return child;}}
        private ToolStripButton ExpertButton(){return NativeItems(nativeTools.Items).OfType<ToolStripButton>().FirstOrDefault(b=>b.Name.IndexOf("Advanced",StringComparison.OrdinalIgnoreCase)>=0||b.Text.IndexOf("mode",StringComparison.OrdinalIgnoreCase)>=0);}
        private bool IsExpert(){var button=ExpertButton();return button!=null&&button.Checked;}
        private static void ClickNative(ToolStripItem item){var split=item as ToolStripSplitButton;if(split!=null)split.PerformButtonClick();else item.PerformClick();}
        private void SetMode(bool expert){var button=ExpertButton();if(button==null||button.Checked==expert)return;Control current=tree.SelectedNode==null?null:NodeSection(tree.SelectedNode);ClickNative(button);history.Clear();historyIndex=-1;var node=Flatten(tree.Nodes).FirstOrDefault(n=>Object.ReferenceEquals(NodeSection(n),current))??Flatten(tree.Nodes).FirstOrDefault();if(node!=null){Navigate(node);RecordHistory(node);}UpdateSearch();}
        private void InvokeNativeTool(string name){var item=NativeItems(nativeTools.Items).FirstOrDefault(i=>i.Name.IndexOf(name=="logs"?"logs":"configToolStrip",StringComparison.OrdinalIgnoreCase)>=0);if(item!=null)ClickNative(item);else status.Text="Dieser Ordnerzugriff wird von der Installation nicht angeboten.";}
        private void ShowNativeTools(Control anchor)
        {
            var menu=new ContextMenuStrip {Font=font};
            Action<ToolStripItemCollection,ToolStripItemCollection> copy=null;copy=delegate(ToolStripItemCollection source,ToolStripItemCollection target){foreach(ToolStripItem original in source){if(original is ToolStripSeparator){target.Add(new ToolStripSeparator());continue;}var proxy=new ToolStripMenuItem(original.Text){Enabled=original.Enabled};var checkedButton=original as ToolStripButton;if(checkedButton!=null)proxy.Checked=checkedButton.Checked;ToolStripItem action=original;target.Add(proxy);var dropdown=original as ToolStripDropDownItem;if(dropdown!=null&&dropdown.DropDownItems.Count>0)copy(dropdown.DropDownItems,proxy.DropDownItems);else proxy.Click+=delegate{ClickNative(action);};}};
            copy(nativeTools.Items,menu.Items);theme.Apply(menu);menu.Closed+=delegate{menu.Dispose();};menu.Show(anchor,new Point(0,anchor.Height));
        }
        private void TraverseHistory(int direction){int next=historyIndex+direction;while(next>=0&&next<history.Count){if(history[next].TreeView==tree){traversingHistory=true;try{if(Navigate(history[next]))historyIndex=next;}finally{traversingHistory=false;}UpdateHistoryButtons();return;}next+=direction;}}
        private void RecordHistory(TreeNode node){if(!traversingHistory&&(historyIndex<0||!Object.ReferenceEquals(history[historyIndex],node))){if(historyIndex+1<history.Count)history.RemoveRange(historyIndex+1,history.Count-historyIndex-1);history.Add(node);historyIndex=history.Count-1;}UpdateHistoryButtons();}
        private void UpdateHistoryButtons(){backButton.Enabled=historyIndex>0;forwardButton.Enabled=historyIndex+1<history.Count;}
        private void StepTextScale(int direction){int[] sizes={100,115,125,150,175};int next=direction>0?sizes.FirstOrDefault(s=>s>appearance.TextScale):sizes.Reverse().FirstOrDefault(s=>s<appearance.TextScale);if(next>0)SetTextScale(next);}
        public void SetTextScale(int percent)
        {
            percent=Math.Max(100,Math.Min(175,percent));if(percent==appearance.TextScale||settingSize)return;settingSize=true;
            try{using(FrameRendering.Suspend(form)){
                float ratio=percent/(float)appearance.TextScale;
                foreach(Control c in Descendants(root).ToArray())if(!sections.Any(page=>Object.ReferenceEquals(page,c)||page.Contains(c)))c.Font=MakeFont(c.Font.SizeInPoints*ratio,c.Font.Style);
                appearance.TextScale=percent;InitializeTypography();
                root.RowStyles[0].Height=Px(66);root.RowStyles[1].Height=Px(56);root.RowStyles[3].Height=Px(58);body.ColumnStyles[0].Width=Px(250);body.ColumnStyles[2].Width=appearance.ContextVisible?Px(225):0;
                // Scale shell geometry as well as fonts. Native pages have their own
                // geometry scaling and are excluded from this traversal.
                foreach(Control c in new Control[]{header,toolbar,sidebar,context,footer})c.Scale(new SizeF(ratio,ratio));
                center.Padding=new Padding(Px(24),Px(21),Px(24),Px(14));
                foreach(Control c in center.Controls)if(c.Dock==DockStyle.Top)c.Height=(int)Math.Round(c.Height*ratio);
                pageIcon.Width=Px(42);navigation.Font=font;navigation.Sync(tree.SelectedNode,true);foreach(Control page in sections)pageZoom.Apply(page,PageScale);
                int selected=Array.IndexOf(new[]{100,115,125,150,175},percent);if(selected>=0)sizeChoice.SelectedIndex=selected;appearance.Save();RefreshPage();form.PerformLayout();LayoutToolbar();LayoutHelp();
            }}finally{settingSize=false;}
        }
        private string Lead(TreeNode node){switch(navigation.Group(node)){case "Allgemein":return "Start, Verhalten und Grundeinstellungen deines Media Centers.";case "Oberfläche":return "Skin, Sprache und Darstellung von MediaPortal.";case "Wiedergabe":return "Player, Ton und Bild – mit den originalen MediaPortal-Einstellungen.";case "Medienordner":return "Quellen und Bibliotheken für Videos, Musik und Bilder.";case "Bedienung":return "Tastatur, Fernbedienung und Eingabegeräte einrichten.";case "Plugins":return "Erweiterungen aktivieren und ihre Konfiguration öffnen.";default:return "Informationen, Kompatibilität und Diagnose.";}}
    }
}

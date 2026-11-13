using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
        private float PageScale {get{int dpi=96;try{dpi=form.DeviceDpi;}catch(Exception){}return dpi>=120?1f:UiScale*Typography.NativeBody/8.25f;}}
        private int Px(float value){return (int)Math.Round(value*UiScale);}
        private Font MakeFont(float size,FontStyle style){var result=Typography.Create(size,style);shellFonts.Add(result);return result;}
        private Font MakeHeading(float size){var result=Typography.CreateHeading(size);shellFonts.Add(result);return result;}
        private void InitializeTypography()
        {font=MakeFont(Typography.Body*UiScale,FontStyle.Regular);labelFont=MakeFont(Typography.Small*UiScale,FontStyle.Regular);headingFont=MakeHeading(Typography.Heading*UiScale);treeRootFont=font;}
        [DllImport("user32.dll",EntryPoint="SendMessageW",CharSet=CharSet.Unicode)] private static extern IntPtr SendCue(IntPtr h,int m,IntPtr w,string text);
        private void Build()
        {
            form.Text="MP1 Configuration Studio · MediaPortal 1.39";form.AutoScroll=false;form.MinimumSize=new Size(1150,740);
            Rectangle screen=Screen.FromControl(form).WorkingArea;form.Size=new Size(Math.Min(screen.Width,appearance.Width),Math.Min(screen.Height,appearance.Height));form.StartPosition=FormStartPosition.CenterScreen;
            form.Controls.Clear();
            InjectFault("build-cleared");
            root=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=4,Margin=Padding.Empty,Padding=Padding.Empty,Font=font,Name="studioRoot"};
            root.RowStyles.Add(new RowStyle(SizeType.Absolute,Px(66)));root.RowStyles.Add(new RowStyle(SizeType.Absolute,Px(56)));root.RowStyles.Add(new RowStyle(SizeType.Percent,100));root.RowStyles.Add(new RowStyle(SizeType.Absolute,Px(58)));root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));form.Controls.Add(root);
            var backend=new Panel {Visible=false,Name="studioNativeBackend",Size=new Size(1,1)};form.Controls.Add(backend);tree.Parent=backend;tree.Visible=false;nativeTools.Parent=backend;nativeTools.Visible=false;if(donate!=null)donate.Parent=backend;
            InjectFault("build-backend");
            header=new Panel {Dock=DockStyle.Fill,Margin=Padding.Empty};
            header.Controls.Add(new StudioLogo {Colors=()=>theme.Colors,Bounds=new Rectangle(Px(23),Px(14),Px(36),Px(36)),Font=MakeFont(13*UiScale,FontStyle.Bold)});
            header.Controls.Add(new Label {Text="MP1 / CONFIGURATION",AutoSize=true,Location=new Point(Px(72),Px(10)),Font=MakeHeading(15*UiScale)});
            header.Controls.Add(new Label {Text="MEDIAPORTAL · STUDIO",AutoSize=true,Location=new Point(Px(74),Px(39)),Font=MakeFont(Typography.Small*UiScale,FontStyle.Regular),Name="studioBrandSubtitle"});root.Controls.Add(header,0,0);
            toolbar=new Panel {Dock=DockStyle.Fill,Margin=Padding.Empty};root.Controls.Add(toolbar,0,1);
            backButton=new StudioAction {Name="studioHistoryBack",Text="",IconName="back",Colors=()=>theme.Colors,Size=new Size(Px(35),Px(35)),Location=new Point(Px(13),Px(8)),AccessibleName="Zurück",Enabled=false};backButton.Click+=delegate{TraverseHistory(-1);};
            forwardButton=new StudioAction {Name="studioHistoryForward",Text="",IconName="forward",Colors=()=>theme.Colors,Size=backButton.Size,Location=new Point(Px(48),Px(8)),AccessibleName="Vorwärts",Enabled=false};forwardButton.Click+=delegate{TraverseHistory(1);};toolbar.Controls.Add(backButton);toolbar.Controls.Add(forwardButton);backButton.TabIndex=0;forwardButton.TabIndex=1;
            tips.SetToolTip(backButton,Localization.T("Zurück · Alt+Links"));tips.SetToolTip(forwardButton,Localization.T("Vorwärts · Alt+Rechts"));
            options=new FlowLayoutPanel {Dock=DockStyle.None,Width=Px(550),WrapContents=false,Padding=new Padding(0,Px(8),Px(12),0),Margin=Padding.Empty};toolbar.Controls.Add(options);
            modeChoice=Choice("studioMode",Localization.T("Standard- oder Expertenansicht"),100,new object[]{Localization.T("Standard"),Localization.T("Experte")});modeChoice.SelectedIndex=IsExpert()?1:0;modeChoice.SelectedIndexChanged+=delegate{SetMode(modeChoice.SelectedIndex==1);};
            sizeChoice=Choice("studioTextSize",Localization.T("Schrift und Oberfläche vergrößern"),135,new object[]{Localization.T("Größe 100%"),Localization.T("Größe 115%"),Localization.T("Größe 125%"),Localization.T("Größe 150%"),Localization.T("Größe 175%")});sizeChoice.SelectedIndex=Array.IndexOf(new[]{100,115,125,150,175},appearance.TextScale);if(sizeChoice.SelectedIndex<0)sizeChoice.SelectedIndex=2;sizeChoice.SelectedIndexChanged+=delegate{if(!settingSize&&sizeChoice.SelectedIndex>=0)SetTextScale(new[]{100,115,125,150,175}[sizeChoice.SelectedIndex]);};
            themeChoice=Choice("studioTheme",Localization.T("Hell oder Dunkel"),85,new object[]{Localization.T("Dunkel"),Localization.T("Hell")});themeChoice.SelectedIndex=appearance.Dark?0:1;themeChoice.SelectedIndexChanged+=delegate{SetDark(themeChoice.SelectedIndex==0);};
            contextButton=ActionButton("",ToggleContext,Px(43));contextButton.Name="studioContextToggle";contextButton.AccessibleName=Localization.T("Hilfe ein- oder ausblenden");((StudioAction)contextButton).IconName="help";tips.SetToolTip(contextButton,Localization.T("Rechte Hilfespalte ein- oder ausblenden"));
            var more=(StudioAction)ActionButton(Localization.T("Werkzeuge"),delegate{},Px(140));more.IconName="settings";more.Click+=delegate{ShowNativeTools(more);};
            options.Controls.Add(modeChoice);options.Controls.Add(sizeChoice);options.Controls.Add(themeChoice);options.Controls.Add(contextButton);options.Controls.Add(more);modeChoice.TabIndex=3;sizeChoice.TabIndex=4;themeChoice.TabIndex=5;contextButton.TabIndex=6;more.TabIndex=7;
            searchBar=new Panel {Name="studioSearchBar",Height=Px(36),Padding=new Padding(Px(34),Px(7),Px(7),Px(5))};
            search=new TextBox {Dock=DockStyle.Fill,BorderStyle=BorderStyle.None,AutoSize=false,Font=font,Name="studioSearch",AccessibleName=Localization.T("Einstellung oder Bereich suchen"),TabIndex=2};
            search.HandleCreated+=delegate{SendCue(search.Handle,0x1501,IntPtr.Zero,"");};
            clearSearch=ActionButton("×",delegate{search.Clear();search.Focus();},Px(25));clearSearch.Dock=DockStyle.Right;clearSearch.Visible=false;clearSearch.AccessibleName=Localization.T("Suche zurücksetzen");searchBar.Controls.Add(search);searchBar.Controls.Add(clearSearch);
            searchPlaceholder=new Label {Text=Localization.T("Einstellungen suchen …  Strg+F"),Font=font,UseMnemonic=false,AutoEllipsis=true,TextAlign=ContentAlignment.MiddleLeft,Name="studioSearchPlaceholder"};searchBar.Controls.Add(searchPlaceholder);searchPlaceholder.BringToFront();searchPlaceholder.Click+=delegate{search.Focus();};searchBar.Layout+=delegate{searchPlaceholder.Bounds=search.Bounds;};search.Enter+=delegate{searchPlaceholder.Visible=false;};search.Leave+=delegate{searchPlaceholder.Visible=search.TextLength==0;};
            searchBar.Paint+=delegate(object sender,PaintEventArgs e){e.Graphics.Clear(theme.Colors.Input);using(var path=StudioIcons.Rounded(new RectangleF(0,0,searchBar.Width-1,searchBar.Height-1),5))using(var pen=new Pen(theme.Colors.Border))e.Graphics.DrawPath(pen,path);StudioIcons.Draw(e.Graphics,"search",new Rectangle(Px(10),Px(10),Px(16),Px(16)),Palette.HighContrastActive?SystemColors.WindowText:theme.Colors.Muted);};
            toolbar.Controls.Add(searchBar);toolbar.Resize+=delegate{LayoutToolbar();};
            toolbar.Paint+=delegate(object sender,PaintEventArgs e){using(var pen=new Pen(theme.Colors.Border))e.Graphics.DrawLine(pen,0,toolbar.Height-1,toolbar.Width,toolbar.Height-1);};
            body=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=3,RowCount=1,Margin=Padding.Empty};body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,Px(250)));body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,Px(225)));body.RowStyles.Add(new RowStyle(SizeType.Percent,100));root.Controls.Add(body,0,2);
            sidebar=new Panel {Dock=DockStyle.Fill,Padding=new Padding(Px(10),Px(10),Px(6),Px(10)),Margin=Padding.Empty};navigation=new StudioNavigation(tree,NativeContract.NodeSection,Navigate,()=>theme.Colors,InvokeNativeTool){Font=font};sidebar.Controls.Add(navigation);navigation.TabIndex=0;
            results=new ListBox {Dock=DockStyle.Fill,Visible=false,IntegralHeight=false,BorderStyle=BorderStyle.None,Font=font,Name="studioSearchResults",DisplayMember="Label",AccessibleName=Localization.T("Suchergebnisse")};sidebar.Controls.Add(results);results.TabIndex=1;
            results.DoubleClick+=delegate{OpenResult();};results.KeyDown+=delegate(object sender,KeyEventArgs e){if(e.KeyCode==Keys.Enter){e.SuppressKeyPress=true;OpenResult();}};
            search.TextChanged+=delegate{clearSearch.Visible=search.TextLength>0;searchPlaceholder.Visible=search.TextLength==0&&!search.Focused;UpdateSearch();};search.KeyDown+=delegate(object sender,KeyEventArgs e){if((e.KeyCode==Keys.Down||e.KeyCode==Keys.Enter)&&results.Visible&&results.Items.Count>0){results.SelectedIndex=0;results.Focus();if(e.KeyCode==Keys.Enter)OpenResult();e.SuppressKeyPress=true;}};
            center=new Panel {Dock=DockStyle.Fill,Name="studioContent",Margin=Padding.Empty,Padding=new Padding(Px(24),Px(21),Px(24),Px(14))};
            var headings=new Panel {Dock=DockStyle.Top,Height=Px(92)};breadcrumb=new Label {AutoEllipsis=true,Dock=DockStyle.Top,Height=Px(22),Font=labelFont,Name="studioMuted"};
            pageLead=new Label {Dock=DockStyle.Bottom,Height=Px(24),Font=labelFont,Name="studioPageLead",AutoEllipsis=true};
            var titleRow=new Panel {Dock=DockStyle.Fill};pageIcon=new Panel {Dock=DockStyle.Left,Width=Px(42)};pageIcon.Paint+=delegate(object sender,PaintEventArgs e){if(tree.SelectedNode!=null)StudioIcons.Draw(e.Graphics,navigation.Icon(tree.SelectedNode),new Rectangle(0,Px(9),Px(29),Px(29)),theme.Colors.Accent);};title=new Label {Dock=DockStyle.Fill,Font=headingFont,AutoEllipsis=true,UseMnemonic=false,Name="studioPageTitle"};titleRow.Controls.Add(title);titleRow.Controls.Add(pageIcon);headings.Controls.Add(titleRow);headings.Controls.Add(pageLead);headings.Controls.Add(breadcrumb);
            tabs=new FlowLayoutPanel {Dock=DockStyle.Top,AutoSize=false,Height=Px(32),WrapContents=true,AutoScroll=false,AccessibleName=Localization.T("Unterbereiche"),Margin=Padding.Empty,TabIndex=8};
            tabs.Layout+=delegate{if(tabs.Width<=0)return;int expected=tabs.GetPreferredSize(new Size(tabs.ClientSize.Width,0)).Height;if(expected>0&&tabs.Height!=expected)tabs.Height=expected;};
            holder.Padding=Padding.Empty;holder.Margin=Padding.Empty;holder.AutoScroll=true;contentScroll=new StudioScrollHost(holder,()=>holder.AutoScrollMinSize,()=>theme.Colors,true);center.Controls.Add(contentScroll);center.Controls.Add(tabs);center.Controls.Add(headings);
            context=new Panel {Dock=DockStyle.Fill,Margin=Padding.Empty,Padding=new Padding(Px(20),Px(22),Px(18),Px(12)),Name="studioContext",AccessibleName="Hilfe und Kontext"};
            contextCaption=new Label {Text=Localization.T("HILFE & KONTEXT"),Dock=DockStyle.Top,Height=Px(31),Font=labelFont,UseMnemonic=false,Name="studioMuted"};helpTitle=new Label {Dock=DockStyle.Top,Height=Px(46),Font=MakeHeading((Typography.Body+1f)*UiScale),UseMnemonic=false,AutoEllipsis=true};helpText=new Label {Dock=DockStyle.Top,Height=Px(147),Font=font};
            var help=ActionButton(Localization.T("Online-Hilfe öffnen"),OpenHelp,Px(180));help.Dock=DockStyle.Top;var changesHeading=new Label {Text=Localization.T("ÄNDERUNGEN"),Dock=DockStyle.Top,Height=Px(44),Padding=new Padding(0,Px(16),0,0),Font=labelFont,Name="studioMuted"};changeText=new Label {Dock=DockStyle.Fill,Font=font,AutoEllipsis=true};hint=new Label {Text=Localization.T("OK speichert mit der originalen MediaPortal-Prüfung. Abbrechen verwirft die Einstellungen."),Dock=DockStyle.Bottom,Height=Px(97),Font=labelFont,Name="studioMuted"};
            context.Controls.Add(changeText);context.Controls.Add(hint);context.Controls.Add(changesHeading);context.Controls.Add(help);context.Controls.Add(helpText);context.Controls.Add(helpTitle);context.Controls.Add(contextCaption);body.Controls.Add(sidebar,0,0);body.Controls.Add(center,1,0);body.Controls.Add(context,2,0);
            helpFlow=new FlowLayoutPanel {FlowDirection=FlowDirection.TopDown,WrapContents=false,AutoScroll=true,Padding=new Padding(0,0,0,SystemInformation.HorizontalScrollBarHeight)};
            foreach(Control item in new Control[]{contextCaption,helpTitle,helpText,help,changesHeading,changeText,hint})
            {
                item.Parent=helpFlow;item.Dock=DockStyle.None;item.Margin=new Padding(0,0,0,Px(18));
                var label=item as Label;if(label!=null){label.Padding=Padding.Empty;label.AutoSize=true;label.UseMnemonic=false;}
            }
            helpScroll=new StudioScrollHost(helpFlow,()=>new Size(0,Math.Max(0,helpFlow.GetPreferredSize(new Size(Math.Max(1,helpFlow.ClientSize.Width-10),0)).Height-SystemInformation.HorizontalScrollBarHeight)),()=>theme.Colors,false);context.Controls.Add(helpScroll);helpFlow.SizeChanged+=delegate{LayoutHelp();};
            footer=new Panel {Dock=DockStyle.Fill,Padding=new Padding(Px(23),Px(11),Px(18),Px(10)),Margin=Padding.Empty};var actions=new FlowLayoutPanel {Dock=DockStyle.Right,FlowDirection=FlowDirection.RightToLeft,WrapContents=false,Width=Px(290)};
            nativeOk.Parent=actions;nativeOk.Text=Localization.T("&OK – Speichern");nativeOk.Size=new Size(Px(151),Px(36));nativeOk.Font=font;nativeOk.Margin=new Padding(Px(10),0,0,0);nativeOk.Tag="studio-primary";nativeOk.TabIndex=0;
            nativeCancel.Parent=actions;nativeCancel.Text=Localization.T("&Abbrechen");nativeCancel.Size=new Size(Px(115),Px(36));nativeCancel.Font=font;nativeCancel.Margin=new Padding(Px(8),0,0,0);nativeCancel.TabIndex=1;
            if(nativeApply!=null){nativeApply.Parent=actions;if(nativeApply.Visible){nativeApply.Size=new Size(Px(110),Px(36));actions.Width+=Px(120);}}
            InjectFault("build-reparented");
            status=new Label {Dock=DockStyle.Fill,AutoEllipsis=true,TextAlign=ContentAlignment.MiddleLeft,Font=labelFont,Name="studioStatus",AccessibleName="Änderungsstatus"};footer.Controls.Add(status);footer.Controls.Add(actions);root.Controls.Add(footer,0,3);
            form.AcceptButton=nativeOk;form.CancelButton=nativeCancel;SetContext(appearance.ContextVisible,false);count=new Label();LayoutToolbar();
            nativeOk.MouseDown+=delegate(object sender,MouseEventArgs e){if(e.Button==MouseButtons.Left)PreSaveBackup();};
            nativeOk.KeyDown+=delegate(object sender,KeyEventArgs e){if(e.KeyCode==Keys.Enter||e.KeyCode==Keys.Space)PreSaveBackup();};
            if(nativeApply!=null)
            {
                nativeApply.MouseDown+=delegate(object sender,MouseEventArgs e){if(e.Button==MouseButtons.Left)PreSaveBackup();};
                nativeApply.KeyDown+=delegate(object sender,KeyEventArgs e){if(e.KeyCode==Keys.Enter||e.KeyCode==Keys.Space)PreSaveBackup();};
            }
            InjectFault("build-complete");
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
        private void SetMode(bool expert){var button=ExpertButton();if(button==null||button.Checked==expert)return;Control current=tree.SelectedNode==null?null:NativeContract.NodeSection(tree.SelectedNode);ClickNative(button);history.Clear();historyIndex=-1;var node=Flatten(tree.Nodes).FirstOrDefault(n=>Object.ReferenceEquals(NativeContract.NodeSection(n),current))??Flatten(tree.Nodes).FirstOrDefault();if(node!=null){Navigate(node);RecordHistory(node);}UpdateSearch();}
        private void InvokeNativeTool(string name){var item=NativeItems(nativeTools.Items).FirstOrDefault(i=>i.Name.IndexOf(name=="logs"?"logs":"configToolStrip",StringComparison.OrdinalIgnoreCase)>=0);if(item!=null)ClickNative(item);else status.Text=Localization.T("Dieser Ordnerzugriff wird von der Installation nicht angeboten.");}
        private void ShowNativeTools(Control anchor)
        {
            var menu=new ContextMenuStrip {Font=font};
            Action<ToolStripItemCollection,ToolStripItemCollection> copy=null;copy=delegate(ToolStripItemCollection source,ToolStripItemCollection target){foreach(ToolStripItem original in source){if(original is ToolStripSeparator){target.Add(new ToolStripSeparator());continue;}var proxy=new ToolStripMenuItem(original.Text){Enabled=original.Enabled};var checkedButton=original as ToolStripButton;if(checkedButton!=null)proxy.Checked=checkedButton.Checked;ToolStripItem action=original;target.Add(proxy);var dropdown=original as ToolStripDropDownItem;if(dropdown!=null&&dropdown.DropDownItems.Count>0)copy(dropdown.DropDownItems,proxy.DropDownItems);else proxy.Click+=delegate{ClickNative(action);};}};
            copy(nativeTools.Items,menu.Items);
            menu.Items.Add(new ToolStripSeparator());
            var backup=new ToolStripMenuItem(Localization.T("Konfiguration sichern…"));
            backup.Click+=delegate{BackupConfiguration();};
            menu.Items.Add(backup);
            var restore=new ToolStripMenuItem(Localization.T("Konfiguration wiederherstellen…"));
            restore.Click+=delegate{RestoreConfiguration();};
            menu.Items.Add(restore);
            var manageBackups=new ToolStripMenuItem(Localization.T("Backups verwalten…"));
            manageBackups.Click+=delegate{ManageBackups();};
            menu.Items.Add(manageBackups);
            var autoBackup=new ToolStripMenuItem(Localization.T("Vor dem Speichern automatisch sichern")){Checked=appearance.PreSaveBackup,CheckOnClick=true};
            autoBackup.Click+=delegate{appearance.PreSaveBackup=autoBackup.Checked;appearance.Save();StudioLog.Info("Pre-save backup "+(appearance.PreSaveBackup?"enabled":"disabled"));};
            menu.Items.Add(autoBackup);
            var nativeSize=new ToolStripMenuItem(Localization.T("Seite in Originalgröße"));
            nativeSize.Click+=delegate{UseNativePageSize();};
            menu.Items.Add(nativeSize);
            var keyboard=new ToolStripMenuItem(Localization.T("Tastenkürzel…"));
            keyboard.Click+=delegate{OpenKeyboardHelp();};
            menu.Items.Add(keyboard);
            menu.Items.Add(new ToolStripSeparator());
            var restart=new ToolStripMenuItem(Localization.T("Studio neu starten"));
            restart.Click+=delegate{RestartStudio();};
            menu.Items.Add(restart);
            var classic=new ToolStripMenuItem(Localization.T("Klassische Oberfläche starten"));
            classic.Click+=delegate{StartClassic();};
            menu.Items.Add(classic);
            var support=new ToolStripMenuItem(Localization.T("Support-Bundle erzeugen…"));
            support.Click+=delegate{CreateSupportBundle();};
            menu.Items.Add(support);
            var language=new ToolStripMenuItem(Localization.IsEnglish?"Language":"Sprache");
            var german=new ToolStripMenuItem(Localization.IsEnglish?"German":"Deutsch"){Checked=appearance.Language=="de"};
            german.Click+=delegate{SetLanguage("de");};
            var english=new ToolStripMenuItem("English"){Checked=appearance.Language=="en"};
            english.Click+=delegate{SetLanguage("en");};
            language.DropDownItems.Add(german);language.DropDownItems.Add(english);
            menu.Items.Add(language);
            var about=new ToolStripMenuItem(Localization.T("Über MP1 Configuration Studio"));
            about.Click+=delegate{ShowAbout();};
            menu.Items.Add(about);
            theme.Apply(menu);menu.Closed+=delegate{menu.Dispose();};menu.Show(anchor,new Point(0,anchor.Height));
        }
        private string MediaPortalConfigPath()
        {
            return ConfigurationBackup.ConfigPath(NativeContract.ConfigFolder("Config"));
        }
        private void UseNativePageSize()
        {
            Control page=NativeContract.NodeSection(tree.SelectedNode);
            if(page==null)return;
            pageZoom.Apply(page,1f);
            LayoutNativePage();
            if(status!=null)status.Text="Seite auf Originalgröße gesetzt.";
            StudioLog.Info("Native page size requested: "+page.GetType().FullName);
        }
        private void BackupConfiguration()
        {
            try
            {
                using(var dialog=new SaveFileDialog {Filter="Studio-Backup (*.zip)|*.zip",Title=Localization.T("Konfiguration sichern"),FileName="MediaPortal-Backup-"+DateTime.Now.ToString("yyyyMMdd-HHmmss")+".zip"})
                {
                    if(dialog.ShowDialog(form)!=DialogResult.OK)return;
                    string path=ConfigurationBackup.Create(MediaPortalConfigPath(),dialog.FileName);
                    MessageBox.Show(form,Localization.T("Konfiguration gesichert:\r\n")+path,"Configuration Studio",MessageBoxButtons.OK,MessageBoxIcon.Information);
                }
            }
            catch(Exception ex)
            {
                StudioLog.Error(ex,"Configuration backup failed");
                MessageBox.Show(form,Localization.T("Die Konfiguration konnte nicht gesichert werden.\r\n\r\n")+ex.Message+"\r\n\r\n"+Localization.T("Protokoll: ")+StudioLog.FilePath,"Configuration Studio",MessageBoxButtons.OK,MessageBoxIcon.Warning);
            }
        }
        private void SetLanguage(string code)
        {
            if(appearance.Language==code)return;
            appearance.Language=code;
            appearance.Save();
            MessageBox.Show(form,
                code=="en"?"Language changed. Please restart Studio to apply it.":"Sprache geändert. Bitte Studio neu starten.",
                "MP1 Configuration Studio",MessageBoxButtons.OK,MessageBoxIcon.Information);
        }
        private void ManageBackups()
        {
            try
            {
                using(var dialog=new BackupManagerDialog())
                {
                    if(dialog.ShowDialog(form)!=DialogResult.OK)return;
                    if(String.IsNullOrEmpty(dialog.SelectedBackupPath))return;
                    ConfigurationBackup.Restore(dialog.SelectedBackupPath,MediaPortalConfigPath());
                    StudioLog.Info("Configuration restored from backup manager; restarting classic");
                    restartClassic=true;
                    nativeCancel.PerformClick();
                    if(!form.IsDisposed)form.Close();
                }
            }
            catch(Exception ex)
            {
                StudioLog.Error(ex,"Backup manager restore failed");
                ErrorDialog.Show(form,Localization.T("Die Konfiguration konnte nicht wiederhergestellt werden.\r\n\r\n")+ex.Message,
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Configuration.exe"),AppDomain.CurrentDomain.BaseDirectory);
            }
        }
        private void RestoreConfiguration()
        {
            try
            {
                using(var dialog=new OpenFileDialog {Filter="Studio-Backup (*.zip)|*.zip",Title=Localization.T("Konfiguration wiederherstellen"),CheckFileExists=true})
                {
                    if(dialog.ShowDialog(form)!=DialogResult.OK)return;
                    if(MessageBox.Show(form,Localization.IsEnglish
                            ? "The current MediaPortal.xml is backed up first. Studio then closes and the original interface starts with the restored configuration.\r\n\r\nUnsaved changes will be lost. Continue?"
                            : "Die aktuelle MediaPortal.xml wird vorher gesichert. Danach wird Studio geschlossen und die originale Oberfläche mit der wiederhergestellten Konfiguration gestartet.\r\n\r\nNicht gespeicherte Änderungen gehen verloren. Fortfahren?",
                        Localization.T("Konfiguration wiederherstellen"),MessageBoxButtons.YesNo,MessageBoxIcon.Warning,MessageBoxDefaultButton.Button2)!=DialogResult.Yes)return;
                    ConfigurationBackup.Restore(dialog.FileName,MediaPortalConfigPath());
                    StudioLog.Info("Configuration restored; restarting classic");
                    restartClassic=true;
                    nativeCancel.PerformClick();
                    if(!form.IsDisposed)form.Close();
                }
            }
            catch(Exception ex)
            {
                StudioLog.Error(ex,"Configuration restore failed");
                MessageBox.Show(form,Localization.T("Die Konfiguration konnte nicht wiederhergestellt werden.\r\n\r\n")+ex.Message+"\r\n\r\n"+Localization.T("Protokoll: ")+StudioLog.FilePath,"Configuration Studio",MessageBoxButtons.OK,MessageBoxIcon.Warning);
            }
        }
        private void RestartStudio()
        {
            if(MessageBox.Show(form,CloseGuard.RestartPrompt(edits.Count,Localization.IsEnglish),Localization.T("Studio neu starten"),
                MessageBoxButtons.YesNo,MessageBoxIcon.Question,MessageBoxDefaultButton.Button2)!=DialogResult.Yes)return;
            restartStudio=true;
            StudioLog.Info("Studio restart requested");
            nativeCancel.PerformClick();
            if(!form.IsDisposed)form.Close();
        }
        private void StartClassic()
        {
            if(MessageBox.Show(form,Localization.IsEnglish
                    ? "The Studio window will close. Unsaved changes will be lost.\r\n\r\nThe original interface will then start. Continue?"
                    : "Das Studio-Fenster wird geschlossen. Nicht gespeicherte Änderungen gehen verloren.\r\n\r\nDie originale Oberfläche wird anschließend gestartet. Fortfahren?",
                Localization.T("Klassische Oberfläche"),MessageBoxButtons.YesNo,MessageBoxIcon.Question,MessageBoxDefaultButton.Button2)!=DialogResult.Yes)return;
            restartClassic=true;
            StudioLog.Info("Classic restart requested");
            nativeCancel.PerformClick();
            if(!form.IsDisposed)form.Close();
        }
        private void CreateSupportBundle()
        {
            using(var dialog=new SaveFileDialog {Filter=Localization.T("ZIP-Archiv (*.zip)|*.zip"),Title=Localization.T("Support-Bundle speichern"),FileName="MP1-Configuration-Studio-Support-"+DateTime.Now.ToString("yyyyMMdd-HHmmss")+".zip"})
            {
                if(dialog.ShowDialog(form)!=DialogResult.OK)return;
                string path=SupportBundle.Create(dialog.FileName,appearance.PathName,sections);
                if(!String.IsNullOrEmpty(path))
                {
                    MessageBox.Show(form,Localization.T("Support-Bundle erstellt:\r\n")+path,"Configuration Studio",MessageBoxButtons.OK,MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(form,Localization.T("Das Support-Bundle konnte nicht erstellt werden. Details stehen im Protokoll.")+"\r\n"+StudioLog.FilePath,"Configuration Studio",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                }
            }
        }
        private void ShowAbout()
        {
            string version=System.Diagnostics.FileVersionInfo.GetVersionInfo(Application.ExecutablePath).FileVersion;
            string text="MP1 Configuration Studio "+version+"\r\n\r\n"
                +(Localization.IsEnglish?"For MediaPortal 1.39 x64.\r\n\r\n":"Für MediaPortal 1.39 x64.\r\n\r\n")
                +(Localization.IsEnglish?"Unofficial project, not affiliated with Team MediaPortal.\r\n":"Inoffizielles Projekt, nicht mit Team MediaPortal verbunden.\r\n")
                +"License: GPL-3.0-or-later.\r\n\r\n"
                +(Localization.IsEnglish?"Log: ":"Protokoll: ")+StudioLog.FilePath+"\r\n"
                +"Project: https://github.com/gheitmann/MP1-Configuration-Studio";
            MessageBox.Show(form,text,"Über MP1 Configuration Studio",MessageBoxButtons.OK,MessageBoxIcon.Information);
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
                pageIcon.Width=Px(42);navigation.Font=font;navigation.Sync(tree.SelectedNode,true);foreach(Control page in preparedPages)if(CompatibilityPolicy.Resolve(page.GetType().FullName).Zoom&&!PageZoom.UsesManagedLayout(page))pageZoom.Apply(page,PageScale);
                int selected=Array.IndexOf(new[]{100,115,125,150,175},percent);if(selected>=0)sizeChoice.SelectedIndex=selected;appearance.Save();RefreshPage();form.PerformLayout();LayoutToolbar();LayoutHelp();
            }}finally{settingSize=false;}
        }
        private string Lead(TreeNode node)
        {
            switch(navigation.Icon(node))
            {
                case "settings":return Localization.IsEnglish?"Startup, behavior, and basic settings for your media center.":"Start, Verhalten und Grundeinstellungen deines Media Centers.";
                case "surface":return Localization.IsEnglish?"Skin, language, and presentation of MediaPortal.":"Skin, Sprache und Darstellung von MediaPortal.";
                case "play":return Localization.IsEnglish?"Player, audio, and video with the original MediaPortal settings.":"Player, Ton und Bild – mit den originalen MediaPortal-Einstellungen.";
                case "folder":return Localization.IsEnglish?"Sources and libraries for videos, music, and pictures.":"Quellen und Bibliotheken für Videos, Musik und Bilder.";
                case "keyboard":return Localization.IsEnglish?"Set up keyboard, remote, and input devices.":"Tastatur, Fernbedienung und Eingabegeräte einrichten.";
                case "plugin":return Localization.IsEnglish?"Enable extensions and open their configuration.":"Erweiterungen aktivieren und ihre Konfiguration öffnen.";
                default:return Localization.IsEnglish?"Information, compatibility, and diagnostics.":"Informationen, Kompatibilität und Diagnose.";
            }
        }
    }
}

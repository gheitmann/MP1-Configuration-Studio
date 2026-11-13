using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private readonly HashSet<Control> preparedPages = new HashSet<Control>();
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
        private bool restartClassic;
        private bool restartStudio;
        private bool dpiNoticeShown;
        private Timer dpiWatch;
        private DateTime lastPreSaveBackup;
        private bool initializing;
        private Font font,headingFont,labelFont,treeRootFont;
        private string fieldHelp = "";
        internal static Action<string> FaultHook {get;set;}
        internal static bool SuppressOnboarding {get;set;}
        internal static bool SuppressCloseGuard {get;set;}
        public int OriginalSectionCount { get { return sections.Count; } }
        public int ChangedCount { get { return edits.Count; } }
        public bool ContextVisible { get { return appearance.ContextVisible; } }
        public bool IsDark { get { return appearance.Dark; } }
        public int NativeTreeCount { get { return Flatten(tree.Nodes).Count(); } }
        public bool PreparedBeforeFirstDisplay {get;private set;}

        public static StudioShell Attach(Form form,string preferences)
        {
            var stopwatch=Stopwatch.StartNew();
            if(SystemInformation.HighContrast)StudioLog.Info("Windows High Contrast is active; using system palette");
            if(File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Configuration.exe")))
            {
                try
                {
                    string backupRoot=null;
                    if(File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"studio-test-runtime.marker")))
                        backupRoot=Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"..","backups"));
                    string firstBackup=ConfigurationBackup.EnsureFirstRunBackup(NativeContract.ConfigFolder("Config"),backupRoot);
                    if(!String.IsNullOrEmpty(firstBackup))StudioLog.Info("First-run configuration backup: "+firstBackup);
                }
                catch(Exception ex)
                {
                    StudioLog.Warn("First-run configuration backup failed: "+ex.Message);
                }
            }
            List<Control> sections=NativeContract.ReadSections(form);
            StudioLog.Info("Attach: read "+sections.Count+" native sections; compatibility sections="+CompatibilityPolicy.Count);
            InjectFault("snapshot");
            var snapshot=new NativeUiSnapshot(form,sections);
            StudioShell shell=null;
            try
            {
                shell=new StudioShell(form,NativeContract.Require<TreeView>(form,NativeContract.SectionTreeField),
                    NativeContract.Require<Panel>(form,NativeContract.HolderPanelField),
                    NativeContract.Require<Button>(form,NativeContract.OkButtonField),
                    NativeContract.Require<Button>(form,NativeContract.CancelButtonField),
                    NativeContract.FindField(form,NativeContract.ApplyButtonField) as Button,
                    NativeContract.Require<ToolStrip>(form,NativeContract.ToolStripField),
                    NativeContract.FindField(form,NativeContract.DonateLinkField) as Control,
                    preferences,sections);
                long elapsed=stopwatch.ElapsedMilliseconds;
                StudioLog.Info("Attach transaction completed in "+elapsed+" ms");
                if(elapsed>2000)StudioLog.Warn("Attach exceeded the 2000 ms budget: "+elapsed+" ms");
                return shell;
            }
            catch(Exception ex)
            {
                StudioLog.Error("Attach transaction failed after "+stopwatch.ElapsedMilliseconds+" ms",ex);
                if(shell!=null)shell.DisposeFailedAttach();
                snapshot.Restore();
                throw;
            }
        }
        internal static StudioShell AttachFixture(Form form, TreeView tree, Panel holder, Button ok, Button cancel, ToolStrip tools,string preferences,List<Control> sections)
        {
            SuppressOnboarding=true;
            var snapshot=new NativeUiSnapshot(form,sections);
            StudioShell shell=null;
            try
            {
                shell=new StudioShell(form,tree,holder,ok,cancel,null,tools,null,preferences,sections);
                return shell;
            }
            catch(Exception ex)
            {
                StudioLog.Error(ex,"Fixture attach transaction failed");
                if(shell!=null)shell.DisposeFailedAttach();
                snapshot.Restore();
                throw;
            }
        }
        private static void InjectFault(string stage)
        {
            Action<string> hook=FaultHook;
            if(hook!=null)hook(stage);
        }
        private void PreSaveBackup()
        {
            if(!appearance.PreSaveBackup)return;
            if((DateTime.UtcNow-lastPreSaveBackup).TotalSeconds<1)return;
            lastPreSaveBackup=DateTime.UtcNow;
            try
            {
                string backup=ConfigurationBackup.CreatePreSaveBackup(NativeContract.ConfigFolder("Config"));
                if(!String.IsNullOrEmpty(backup))StudioLog.Info("Pre-save configuration backup: "+backup);
            }
            catch(Exception ex)
            {
                StudioLog.Warn("Pre-save configuration backup failed: "+ex.Message);
            }
        }
        private void OfferDpiRestart(string reason)
        {
            if(dpiNoticeShown)return;
            dpiNoticeShown=true;
            StudioLog.Warn(reason+"; offering restart");
            DialogResult answer=MessageBox.Show(form,Localization.IsEnglish
                    ?"Display scaling changed. Restart Studio now so the layout is recalculated?\r\n\r\nUnsaved changes will be lost."
                    :"Die Bildschirmskalierung hat sich geändert. Studio jetzt neu starten, damit das Layout neu berechnet wird?\r\n\r\nNicht gespeicherte Änderungen gehen verloren.",
                "MP1 Configuration Studio",MessageBoxButtons.YesNo,MessageBoxIcon.Question,MessageBoxDefaultButton.Button1);
            if(answer!=DialogResult.Yes)return;
            form.BeginInvoke((Action)delegate{
                if(form.IsDisposed||form.Disposing)return;
                restartStudio=true;StudioLog.Info("Studio restart requested after display DPI change");
                nativeCancel.PerformClick();
                if(!form.IsDisposed)form.Close();
            });
        }
        private void StartStudioProcess()
        {
            try
            {
                string exe=Application.ExecutablePath;
                string runtime=AppDomain.CurrentDomain.BaseDirectory;
                Process.Start(new ProcessStartInfo(exe,"--runtime=\""+runtime+"\""){WorkingDirectory=Path.GetDirectoryName(exe),UseShellExecute=true});
                StudioLog.Info("Studio restarted after display DPI change");
            }
            catch(Exception ex)
            {
                StudioLog.Error(ex,"Studio restart after DPI change failed");
                MessageBox.Show(Localization.IsEnglish
                        ?"Studio could not be restarted. Please start it manually.\r\n\r\n"+ex.Message
                        :"Studio konnte nicht neu gestartet werden. Bitte manuell starten.\r\n\r\n"+ex.Message,
                    "MP1 Configuration Studio",MessageBoxButtons.OK,MessageBoxIcon.Warning);
            }
        }
        private void DisposeFailedAttach()
        {
            try{tips.Dispose();}catch{}
            try{pageZoom.Dispose();}catch{}
            try{if(dpiWatch!=null){dpiWatch.Stop();dpiWatch.Dispose();}}catch{}
            foreach(Font owned in shellFonts){try{owned.Dispose();}catch{}}
        }
        private StudioShell(Form f,TreeView originalTree,Panel originalHolder,Button ok,Button cancel,Button apply,ToolStrip tools,Control donateLink,string preferencePath,List<Control> originalSections)
        {
            form=f;form.Icon=StudioIcon.Value;tree=originalTree;holder=originalHolder;nativeOk=ok;nativeCancel=cancel;nativeApply=apply;nativeTools=tools;donate=donateLink;sections=originalSections;
            PreparedBeforeFirstDisplay=!FrameRendering.IsVisible(form);
            initializing=PreparedBeforeFirstDisplay;
            bool newAppearance=!File.Exists(preferencePath);
            appearance=new Appearance(preferencePath);
            if(newAppearance)
            {
                int dpi=96;
                try{dpi=form.DeviceDpi;}catch(Exception){}
                int defaultScale=Appearance.DefaultTextScaleForDpi(dpi);
                if(appearance.TextScale!=defaultScale)
                {
                    appearance.TextScale=defaultScale;
                    StudioLog.Info("High-DPI display detected ("+dpi+" dpi); default internal scale="+defaultScale+"%");
                }
            }
            Localization.Set(appearance.Language);
            theme=new Theme(appearance.Dark);
            InitializeTypography();
            FrameRendering.EnableComposition(form);
            using(FrameRendering.Suspend(form))
            {
            InjectFault("before-build");
            building=true;form.SuspendLayout();
            try { Build(); }
            finally { building=false;form.ResumeLayout(true); }
            InjectFault("after-build");

            // Prepare the original pages lazily on first activation. Preparing all
            // 61 pages before the first display is the main startup cost.
            PreparePage(NativeContract.NodeSection(tree.SelectedNode));

            if(tree.SelectedNode!=null&&NativeContract.NodeSection(tree.SelectedNode)!=null&&NativeContract.NodeSection(tree.SelectedNode).GetType().Name=="Project")
            {
                TreeNode start=Flatten(tree.Nodes).FirstOrDefault(n=>NativeContract.NodeSection(n)!=null&&NativeContract.NodeSection(n).GetType().Name=="General");
                if(start!=null)Navigate(start);
            }
            InjectFault("before-refresh");
            RefreshPage();ApplyTheme();
            LayoutNativePage();
            LayoutHelp();

            InjectFault("wire-events");
            tree.BeforeSelect+=delegate(object sender,TreeViewCancelEventArgs e){switching=!e.Cancel;if(!e.Cancel){CompleteSelectionFrame(null,EventArgs.Empty);selectionFrame=FrameRendering.Suspend(center);}};
            lastSelection=tree.SelectedNode;
            tree.AfterSelect+=delegate{try{switching=false;RefreshPage();InvalidateSelection(lastSelection);InvalidateSelection(tree.SelectedNode);lastSelection=tree.SelectedNode;}finally{CompleteSelectionFrame(null,EventArgs.Empty);}};
            Application.Idle+=CompleteSelectionFrame;
            form.Shown+=delegate{
                if(!initializing)return;
                foreach(Control input in baselines.Keys.ToArray())if(!input.IsDisposed)baselines[input]=Value(input);
                edits.Clear();userTouched.Clear();initializing=false;UpdateStatus();
                if(!appearance.OnboardingSeen&&!SuppressOnboarding)
                {
                    appearance.OnboardingSeen=true;
                    appearance.Save();
                    using(var dialog=new FirstRunDialog())
                    {
                        theme.Apply(dialog);
                        dialog.ShowDialog(form);
                    }
                }
            };
            nativeTools.ItemClicked+=delegate { if(form.IsHandleCreated)form.BeginInvoke((Action)delegate{RefreshPage();UpdateSearch();}); };
            holder.ControlAdded+=delegate(object sender,ControlEventArgs e){Control added=e.Control;Watch(added);SectionPolicy addedPolicy=CompatibilityPolicy.Resolve(added.GetType().FullName);if(!String.Equals(addedPolicy.ThemeMode,"native",StringComparison.OrdinalIgnoreCase))theme.Prepare(added);else theme.Skip(added);LayoutNativePage();};
            form.FormClosed+=delegate{
                Application.Idle-=CompleteSelectionFrame;CompleteSelectionFrame(null,EventArgs.Empty);
                if(form.WindowState==FormWindowState.Normal){appearance.Width=form.Width;appearance.Height=form.Height;}
                appearance.Save();tips.Dispose();pageZoom.Dispose();if(dpiWatch!=null){dpiWatch.Stop();dpiWatch.Dispose();}foreach(Font owned in shellFonts)owned.Dispose();
                if(restartClassic){restartClassic=false;Program.StartNative(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Configuration.exe"),AppDomain.CurrentDomain.BaseDirectory);}
                else if(restartStudio){restartStudio=false;SingleInstance.Release();StartStudioProcess();}
            };
            form.FormClosing+=delegate(object sender,FormClosingEventArgs e){
                if(!CloseGuard.ShouldPrompt(e.CloseReason,edits.Count,restartClassic||restartStudio,SuppressCloseGuard))return;
                if(MessageBox.Show(form,CloseGuard.Prompt(edits.Count,Localization.IsEnglish),Localization.T("Ungespeicherte Änderungen"),
                    MessageBoxButtons.YesNo,MessageBoxIcon.Warning,MessageBoxDefaultButton.Button2)!=DialogResult.Yes)e.Cancel=true;
            };
            holder.SizeChanged+=delegate{LayoutNativePage();};
            foreach(Button button in new[]{nativeOk,nativeCancel})button.Paint+=delegate(object sender,PaintEventArgs e){var b=(Button)sender;bool primary=Object.ReferenceEquals(b,nativeOk);e.Graphics.Clear(theme.Colors.Panel);using(var path=StudioIcons.Rounded(new RectangleF(0,0,b.Width-1,b.Height-1),6)){if(primary)using(var brush=new SolidBrush(theme.Colors.Primary))e.Graphics.FillPath(brush,path);else using(var pen=new Pen(theme.Colors.Border))e.Graphics.DrawPath(pen,path);}TextRenderer.DrawText(e.Graphics,b.Text,b.Font,b.ClientRectangle,primary?theme.Colors.PrimaryText:theme.Colors.Text,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.HidePrefix);};
            form.KeyPreview=true;
            form.KeyDown+=OnKeyDown;
            form.DpiChanged+=delegate{OfferDpiRestart("Display DPI changed");};
            int systemDpi=DisplayScale.SystemDpi();
            dpiWatch=new Timer {Interval=2000};
            dpiWatch.Tick+=delegate{
                int current=DisplayScale.SystemDpi();
                if(DisplayScale.ShouldOffer(systemDpi,current,dpiNoticeShown))OfferDpiRestart("System DPI changed");
                systemDpi=current;
            };
            dpiWatch.Start();
            }
        }
        private void CompleteSelectionFrame(object sender,EventArgs e)
        {if(selectionFrame!=null){var pending=selectionFrame;selectionFrame=null;pending.Dispose();}}
        private void PreparePage(Control section)
        {
            if(section==null||section.IsDisposed||preparedPages.Contains(section))return;
            SectionPolicy policy=CompatibilityPolicy.Resolve(section.GetType().FullName);
            if(policy.Tier==CompatibilityTier.C)return;
            bool themed=!String.Equals(policy.ThemeMode,"native",StringComparison.OrdinalIgnoreCase);
            if(themed)theme.Include(section);else theme.Skip(section);
            if(policy.Zoom||policy.Tier==CompatibilityTier.A)
            {
                section.Font=section.Font;
                int contentWidth=section.Controls.Cast<Control>().Select(c=>c.Right).DefaultIfEmpty(section.Width).Max();
                int contentHeight=section.Controls.Cast<Control>().Select(c=>c.Bottom).DefaultIfEmpty(section.Height).Max();
                section.MinimumSize=new Size(Math.Max(section.MinimumSize.Width,contentWidth+4),Math.Max(section.MinimumSize.Height,contentHeight+8));
            }
            Watch(section);
            if(themed)theme.Prepare(section);
            if(policy.Zoom&&!PageZoom.UsesManagedLayout(section)){pageZoom.Capture(section);pageZoom.Apply(section,PageScale);}
            else{section.Font=form.Font;section.PerformLayout();if(!PageZoom.UsesManagedLayout(section))pageZoom.EnsureContentFits(section);}
            preparedPages.Add(section);
            StudioLog.Info("Prepared page "+section.GetType().FullName+" tier="+policy.Tier+" ("+preparedPages.Count+"/"+sections.Count+")");
        }

        private Button ActionButton(string text,Action action,int width)
        {
            var b=new StudioAction { Text=text,Width=width,Height=Px(36),Colors=()=>theme.Colors,Font=font,Margin=new Padding(0,0,Px(8),0),UseVisualStyleBackColor=false };
            b.Click+=delegate{action();};return b;
        }
        private void OnKeyDown(object sender,KeyEventArgs e)
        {
            if(e.KeyCode==Keys.Enter&&!e.Shift&&!e.Control&&!e.Alt&&!search.Focused&&!results.Focused)PreSaveBackup();
            if(e.Control&&e.KeyCode==Keys.F){search.Focus();search.SelectAll();e.SuppressKeyPress=true;}
            else if(e.Alt&&e.KeyCode==Keys.Left){TraverseHistory(-1);e.SuppressKeyPress=true;}
            else if(e.Alt&&e.KeyCode==Keys.Right){TraverseHistory(1);e.SuppressKeyPress=true;}
            else if(e.Control&&(e.KeyCode==Keys.Add||e.KeyCode==Keys.Oemplus)){StepTextScale(1);e.SuppressKeyPress=true;}
            else if(e.Control&&(e.KeyCode==Keys.Subtract||e.KeyCode==Keys.OemMinus)){StepTextScale(-1);e.SuppressKeyPress=true;}
            else if(e.KeyCode==Keys.Escape&&(search.Focused||results.Focused)&&search.TextLength>0){search.Clear();search.Focus();e.SuppressKeyPress=true;}
            else if(e.KeyCode==Keys.F1){if(e.Shift)OpenKeyboardHelp();else OpenHelp();e.SuppressKeyPress=true;}
        }
        public void ToggleContext(){SetContext(!appearance.ContextVisible,true);}
        private void SetContext(bool visible,bool save)
        {
            using(FrameRendering.Suspend(form))
            {
            appearance.ContextVisible=visible;context.Visible=visible;
            body.ColumnStyles[2].Width=visible?Px(225):0;
            contextButton.Text="";contextButton.AccessibleName=Localization.T(visible?"Hilfe ausblenden":"Hilfe einblenden");tips.SetToolTip(contextButton,contextButton.AccessibleName);
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
                foreach(Control section in preparedPages)if(!form.Contains(section))theme.Apply(section);
                foreach(Form dialog in dialogs.ToArray())if(!dialog.IsDisposed)theme.Apply(dialog);
                var p=theme.Colors;
                if(themeChoice!=null){bool highContrast=SystemInformation.HighContrast;themeChoice.Enabled=!highContrast;themeChoice.AccessibleName=highContrast?"Hoher Kontrast aktiv":"Hell oder Dunkel";}
                center.BackColor=p.Background;holder.BackColor=p.Background;tree.BackColor=p.Panel;tabs.BackColor=p.Background;
                foreach(Control c in Descendants(form))if(c.Name=="studioMuted")c.ForeColor=p.Muted;
                title.ForeColor=p.Text;breadcrumb.ForeColor=p.Muted;pageLead.ForeColor=p.Muted;hint.ForeColor=p.Muted;helpText.ForeColor=p.Muted;changeText.ForeColor=p.Muted;status.ForeColor=p.Muted;
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
            TextRenderer.DrawText(e.Graphics,e.Node.Text,e.Node.Parent==null?treeRootFont:font,bounds,selected?p.SelectionText:p.Text,TextFormatFlags.VerticalCenter|TextFormatFlags.Left|TextFormatFlags.EndEllipsis);
        }
        private void InvalidateSelection(TreeNode node)
        {
            if(node!=null&&node.TreeView==tree&&!node.Bounds.IsEmpty)
                tree.Invalidate(new Rectangle(0,node.Bounds.Top,tree.ClientSize.Width,tree.ItemHeight));
        }
        private void RefreshPage()
        {
            var stopwatch=Stopwatch.StartNew();
            TreeNode node=tree.SelectedNode;if(node==null)return;
            Control selectedSection=NativeContract.NodeSection(node);
            SectionPolicy selectedPolicy=CompatibilityPolicy.Resolve(selectedSection==null?"":selectedSection.GetType().FullName);
            if(selectedPolicy.Tier==CompatibilityTier.C)
            {
                if(status!=null)status.Text=Localization.T("Dieser Bereich ist in Studio nicht unterstützt. Bitte die klassische Oberfläche verwenden.");
                StudioLog.Warn("Blocked Tier C page: "+(selectedSection==null?"":selectedSection.GetType().FullName));
                return;
            }
            PreparePage(selectedSection);
            CorrectStoredSettingDisplay(NativeContract.NodeSection(node));
            navigation.Sync(node,false);RecordHistory(node);
            breadcrumb.Text=Localization.T("Konfiguration  /  ")+navigation.Group(node);title.Text=navigation.Label(node);pageLead.Text=Lead(node)+"  ·  "+TierLabel(selectedPolicy);pageIcon.Invalidate();
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
            foreach(Control c in holder.Controls){Watch(c);}
            if(holder.Controls.Count>0&&CompatibilityPolicy.Resolve(holder.Controls[0].GetType().FullName).Zoom&&!PageZoom.UsesManagedLayout(holder.Controls[0]))pageZoom.EnsureContentFits(holder.Controls[0]);
            LayoutNativePage();
            UpdateStatus();
            if(stopwatch.ElapsedMilliseconds>300)StudioLog.Warn("Page switch exceeded 300 ms: "+node.Text+" ("+stopwatch.ElapsedMilliseconds+" ms)");
        }
        private void AddTab(TreeNode node,TreeNode selected)
        {
            string caption=navigation.Label(node);var button=new StudioTabButton {Text=caption,Colors=()=>theme.Colors,Font=font,Width=Math.Min(Px(240),Math.Max(Px(68),TextRenderer.MeasureText(caption,font).Width+Px(28))),UseVisualStyleBackColor=false};
            button.Click+=delegate{Navigate(node);};
            button.Tag=node;button.Height=Px(28);button.Margin=new Padding(0,0,Px(3),Px(8));button.AccessibleName="Bereich "+caption;tips.SetToolTip(button,node.FullPath);
            tabs.Controls.Add(button);theme.Apply(button);SetTabColor(button);
        }
        private void SetTabColor(Button b)
        {
            if(b==null)return;bool active=Object.ReferenceEquals(b.Tag,tree.SelectedNode);b.BackColor=theme.Colors.Background;b.ForeColor=active?theme.Colors.Accent:theme.Colors.Muted;b.FlatAppearance.BorderSize=0;var tab=b as StudioTabButton;if(tab!=null){tab.Selected=active;tab.Invalidate();}
        }
        internal bool Navigate(TreeNode node)
        {
            if(node==null||node.TreeView!=tree)return false;
            Control page=NativeContract.NodeSection(node);
            if(page!=null)
            {
                SectionPolicy policy=CompatibilityPolicy.Resolve(page.GetType().FullName);
                if(policy.Tier==CompatibilityTier.C)
                {
                    if(status!=null)status.Text=Localization.T("Dieser Bereich ist in Studio nicht unterstützt. Bitte die klassische Oberfläche verwenden.");
                    StudioLog.Warn("Blocked Tier C navigation: "+page.GetType().FullName);
                    return false;
                }
            }
            switching=true;
            try{tree.SelectedNode=node;node.EnsureVisible();}
            finally{switching=false;}
            if(tree.SelectedNode!=node)return false;
            return true;
        }
        private static string TierLabel(SectionPolicy policy)
        {
            if(policy==null)return "Tier B";
            if(policy.Tier==CompatibilityTier.A)return Localization.T("Tier A · voll unterstützt");
            if(policy.Tier==CompatibilityTier.C)return Localization.T("Tier C · nicht unterstützt");
            return Localization.T("Tier B · nativer Look");
        }
        private static string SectionHelp(TreeNode node)
        {
            string root=node.FullPath.ToLowerInvariant();
            if(Localization.IsEnglish)
            {
                if(root.Contains("codec")||root.Contains("renderer"))return "Choose the installed decoders and the video and audio output. F1 opens the documentation for this section.";
                if(root.Contains("plugin"))return "Select an extension to enable it or open its configuration. Some plugins save directly in their own dialog.";
                if(root.Contains("gui")||root.Contains("skin"))return "Choose the MediaPortal interface and behavior. Light/dark changes only this configuration window, not the media interface.";
                if(root.Contains("tv")||root.Contains("radio"))return "The TV/radio client settings remain available here. The separate TV Server configuration still belongs to TV Server.";
                if(root.Contains("keyboard")||root.Contains("remote")||root.Contains("keys"))return "Set up keyboard shortcuts and the remote control. Search finds key bindings and individual options faster.";
                if(root.Contains("folder")||root.Contains("picture"))return "Manage media sources and pictures. Folder and library changes apply to the selected media type.";
                if(root.Contains("general"))return "The tabs cover volume, keyboard shortcuts, refresh rate, and startup behavior. Ctrl+F searches individual settings.";
                return "Select a subsection or use Ctrl+F to search for a setting. F1 opens the matching documentation.";
            }
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
        private void OpenKeyboardHelp()
        {
            using(var dialog=new KeyboardHelpDialog())
            {
                theme.Apply(dialog);
                dialog.ShowDialog(form);
            }
        }
        private static IEnumerable<TreeNode> Flatten(TreeNodeCollection nodes)
        {
            foreach(TreeNode node in nodes){yield return node;foreach(TreeNode child in Flatten(node.Nodes))yield return child;}
        }
        private static IEnumerable<Control> Descendants(Control control)
        {
            foreach(Control child in control.Controls){yield return child;foreach(Control c in Descendants(child))yield return c;}
        }
        private void UpdateSearch()
        {
            var stopwatch=Stopwatch.StartNew();
            string query=search.Text.Trim();bool has=query.Length>0;
            tree.Visible=false;navigation.Visible=!has;results.Visible=has;results.Items.Clear();
            if(!has)return;
            foreach(TreeNode node in Flatten(tree.Nodes))
            {
                if(node.FullPath.IndexOf(query,StringComparison.CurrentCultureIgnoreCase)>=0||navigation.Label(node).IndexOf(query,StringComparison.CurrentCultureIgnoreCase)>=0||navigation.Group(node).IndexOf(query,StringComparison.CurrentCultureIgnoreCase)>=0)
                {results.Items.Add(new SearchResult(node,null,navigation.Group(node)+" / "+navigation.Label(node)));continue;}
                Control section=NativeContract.NodeSection(node);if(section==null)continue;
                foreach(Control c in Descendants(section))
                {
                    var checks=c as CheckedListBox;
                    if(checks!=null)
                    {
                        for(int item=0;item<checks.Items.Count;item++)
                        {
                            string option=checks.GetItemText(checks.Items[item]);
                            if(option.IndexOf(query,StringComparison.CurrentCultureIgnoreCase)>=0)results.Items.Add(new SearchResult(node,c,navigation.Label(node)+" · "+option,item));
                            if(results.Items.Count>=150)break;
                        }
                        continue;
                    }
                    var combo=c as ComboBox;
                    if(combo!=null)
                    {
                        for(int item=0;item<combo.Items.Count;item++)
                        {
                            string option=combo.GetItemText(combo.Items[item]);
                            if(option.IndexOf(query,StringComparison.CurrentCultureIgnoreCase)>=0)results.Items.Add(new SearchResult(node,c,navigation.Label(node)+" · "+option,item));
                            if(results.Items.Count>=150)break;
                        }
                        continue;
                    }
                    var listBox=c as ListBox;
                    if(listBox!=null)
                    {
                        for(int item=0;item<listBox.Items.Count;item++)
                        {
                            string option=listBox.GetItemText(listBox.Items[item]);
                            if(option.IndexOf(query,StringComparison.CurrentCultureIgnoreCase)>=0)results.Items.Add(new SearchResult(node,c,navigation.Label(node)+" · "+option,item));
                            if(results.Items.Count>=150)break;
                        }
                        continue;
                    }
                    var listView=c as ListView;
                    if(listView!=null)
                    {
                        for(int item=0;item<listView.Items.Count;item++)
                        {
                            string option=listView.Items[item].Text;
                            if(option.IndexOf(query,StringComparison.CurrentCultureIgnoreCase)>=0)results.Items.Add(new SearchResult(node,c,navigation.Label(node)+" · "+option,item));
                            if(results.Items.Count>=150)break;
                        }
                        continue;
                    }
                    if(!(c is Label)&&!(c is CheckBox)&&!(c is RadioButton)&&!(c is GroupBox)&&!(c is Button))continue;
                    string text=c.Text.Replace("&","").Trim();
                    if(text.Length>0&&text.IndexOf(query,StringComparison.CurrentCultureIgnoreCase)>=0)
                    {results.Items.Add(new SearchResult(node,c,navigation.Label(node)+" · "+text));if(results.Items.Count>=150)break;}
                }
                if(results.Items.Count>=150)break;
            }
            if(results.Items.Count==0)results.Items.Add(new SearchResult(null,null,Localization.T("Keine Treffer in dieser Ansicht")));
            tips.SetToolTip(results,Localization.T("Doppelklick oder Enter öffnet den Treffer. Für zusätzliche Bereiche oben in den Expertenmodus wechseln."));
            if(stopwatch.ElapsedMilliseconds>150)StudioLog.Warn("Search exceeded 150 ms: results="+results.Items.Count+" ("+stopwatch.ElapsedMilliseconds+" ms)");
        }
        private void OpenResult()
        {
            var result=results.SelectedItem as SearchResult;if(result==null||result.Node==null)return;
            if(Navigate(result.Node))
            {
                search.Clear();
                if(result.Control!=null&&!result.Control.IsDisposed)
                {
                    holder.ScrollControlIntoView(result.Control);
                    var checks=result.Control as CheckedListBox;
                    if(checks!=null&&result.ItemIndex>=0&&result.ItemIndex<checks.Items.Count)checks.SelectedIndex=result.ItemIndex;
                    var combo=result.Control as ComboBox;
                    if(combo!=null&&result.ItemIndex>=0&&result.ItemIndex<combo.Items.Count)combo.SelectedIndex=result.ItemIndex;
                    var listBox=result.Control as ListBox;
                    if(listBox!=null&&result.ItemIndex>=0&&result.ItemIndex<listBox.Items.Count)listBox.SelectedIndex=result.ItemIndex;
                    var listView=result.Control as ListView;
                    if(listView!=null&&result.ItemIndex>=0&&result.ItemIndex<listView.Items.Count){listView.Items[result.ItemIndex].Selected=true;listView.Items[result.ItemIndex].Focused=true;listView.EnsureVisible(result.ItemIndex);}
                    Control target=result.Control.CanFocus?result.Control:result.Control.Parent;target.Focus();helpText.Text=Localization.T("Gefundene Option: ")+result.Label+"\r\n\r\n"+SectionHelp(result.Node);
                }
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
                var nativeTabs=c as TabControl;if(nativeTabs!=null)nativeTabs.SelectedIndexChanged+=delegate{if(!building&&!settingSize&&holder.Controls.Count>0){if(!PageZoom.UsesManagedLayout(holder.Controls[0]))pageZoom.EnsureContentFits(holder.Controls[0]);LayoutNativePage();}};
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
            string info=edits.Count==0?Localization.T("Bereit · Keine offenen Änderungen"):Localization.T("{0} ungespeicherte Änderungen",edits.Count);
            status.Text=info;
            if(!String.IsNullOrEmpty(appearance.LastError))status.Text=Localization.T("Darstellung konnte nicht gespeichert werden: ")+appearance.LastError;
            var names=edits.Where(c=>!c.IsDisposed).Select(FieldLabel).Where(s=>!String.IsNullOrWhiteSpace(s)).Distinct().Take(9).ToArray();
            changeText.Text=names.Length==0?Localization.T("Keine Feldänderungen erkannt.\r\n\r\nPlugin-Dialoge können eigene Speicheraktionen haben."):String.Join("\r\n",names)+"\r\n\r\n"+Localization.T("Speichern über OK.");
        }
        internal sealed class SearchResult
        {
            public readonly TreeNode Node;public readonly Control Control;public readonly int ItemIndex;public string Label{get;private set;}
            public SearchResult(TreeNode node,Control control,string label,int itemIndex=-1){Node=node;Control=control;Label=label;ItemIndex=itemIndex;}
            public override string ToString(){return Label;}
        }
    }
}


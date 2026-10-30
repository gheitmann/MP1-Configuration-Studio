using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MP1.ConfigurationStudio
{
    internal static class ShellTests
    {
        private static int passed;
        private static void Check(bool condition,string name)
        {
            if(!condition)throw new Exception("FAIL: "+name);
            passed++;Console.WriteLine("PASS: "+name);
        }
        public static int Run()
        {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);
            string directory=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"test-results");Directory.CreateDirectory(directory);
            string prefs=Path.Combine(directory,"appearance-test.xml");
            Check(Program.RunnerFileName("Configuration.exe")!="Configuration.exe","renamed download cannot overwrite the original native executable");
            var initial=new Appearance(prefs);initial.Dark=true;initial.ContextVisible=true;initial.Save();
            using(var form=new Form { Size=new Size(1360,900),Text="Fixture" })
            {
                var tree=new TreeView();var holder=new Panel {Size=new Size(502,455)};var ok=new Button();var cancel=new Button();
                int saves=0,cancels=0,activations=0;ok.Click+=delegate{saves++;};cancel.Click+=delegate{cancels++;};
                var section1=new UserControl {Text="GUI",Size=new Size(502,455)};
                var section2=new UserControl {Text="Screensaver",Size=new Size(502,455)};
                var input=new TextBox {Name="skinName",Text="Titan",Location=new Point(20,40)};
                var label=new Label {Text="Skin",Location=new Point(20,18)};
                var check=new CheckBox {Text="Sound effects",Checked=true,Location=new Point(20,80)};
                var swatch=new Panel {Name="colorSwatch",BackColor=Color.Red,Location=new Point(20,120),Size=new Size(30,30)};
                var checklist=new CheckedListBox {Name="guiOptions",Location=new Point(160,80),Size=new Size(220,65)};checklist.Items.Add("Enable media sound",true);checklist.Items.Add("Hide media suffixes",false);
                var framedGroup=new GroupBox {Text="Appearance",Location=new Point(20,180),Size=new Size(250,80)};
                var groupedRadio=new RadioButton {Text="Original radio choice",Checked=true,Location=new Point(15,28)};framedGroup.Controls.Add(groupedRadio);
                var selection=new PaintProbeCombo {Name="nativeSelection",Location=new Point(20,280),Size=new Size(180,25),DropDownStyle=ComboBoxStyle.DropDownList};selection.Items.AddRange(new object[]{"Native choice","Other choice"});selection.SelectedIndex=0;
                var nativeTabs=new PaintProbeTabs {Location=new Point(20,320),Size=new Size(250,100)};nativeTabs.TabPages.Add("First");nativeTabs.TabPages.Add("Second");
                section1.Controls.AddRange(new Control[]{label,input,check,swatch,checklist,framedGroup,selection,nativeTabs});
                var childInput=new NumericUpDown {Name="seconds",Value=30,Location=new Point(20,40)};section2.Controls.Add(childInput);
                var parent=tree.Nodes.Add("GUI");parent.Tag=section1;var child=parent.Nodes.Add("Screensaver");child.Tag=section2;
                var blocked=parent.Nodes.Add("Blocked");blocked.Tag=section2;
                tree.BeforeSelect+=delegate(object sender,TreeViewCancelEventArgs e){if(e.Node==blocked)e.Cancel=true;};
                tree.AfterSelect+=delegate(object sender,TreeViewEventArgs e){holder.Controls.Clear();var page=(Control)e.Node.Tag;page.Dock=DockStyle.Fill;holder.Controls.Add(page);activations++;};
                var tools=new ToolStrip();var help=new ToolStripButton {Name="helpToolStripSplitButton",Text="Help"};int helpCalls=0;help.Click+=delegate{helpCalls++;};tools.Items.Add(help);
                int configCalls=0,logCalls=0,modeCalls=0;var configTool=new ToolStripSplitButton {Name="configToolStripSplitButton",Text="Files"};configTool.ButtonClick+=delegate{configCalls++;};var logTool=new ToolStripMenuItem {Name="logsToolStripMenuItem",Text="Logs"};logTool.Click+=delegate{logCalls++;};configTool.DropDownItems.Add(logTool);var expertTool=new ToolStripButton {Name="toolStripButtonSwitchAdvanced",Text="Switch mode",CheckOnClick=true};expertTool.Click+=delegate{modeCalls++;};tools.Items.Add(configTool);tools.Items.Add(expertTool);
                form.Controls.AddRange(new Control[]{tree,holder,ok,cancel,tools});
                form.ShowInTaskbar=false;form.Show();tree.SelectedNode=parent;
                var shell=StudioShell.AttachFixture(form,tree,holder,ok,cancel,tools,prefs,new List<Control>{section1,section2});
                Check(shell.OriginalSectionCount==2,"all original pages retained");
                bool reviewRefused=false;try{shell.StartFullReview(Path.Combine(directory,"refused-review.xml"));}catch(InvalidOperationException){reviewRefused=true;}Check(reviewRefused,"extended save and layout review refuses a runtime without the isolation marker");
                Check(shell.NativeTreeCount==3,"original navigation nodes retained");
                bool persistenceRefused=false;try{shell.StartPersistenceReview(Path.Combine(directory,"refused-persistence.xml"));}catch(InvalidOperationException){persistenceRefused=true;}Check(persistenceRefused,"native file persistence review refuses an unmarked production runtime");
                var navigation=form.Controls.Find("studioNavigation",true).OfType<StudioNavigation>().Single();
                Check(!tree.Visible&&navigation.Width>=260,"grouped icon navigation replaces the visible native tree with a readable wider sidebar");
                Check(All(navigation).OfType<StudioAction>().Any(b=>b.IconName=="surface"&&b.Text=="Oberfläche"),"mockup navigation has a German surface category with a vector icon");
                All(navigation).OfType<StudioAction>().Single(b=>b.Text=="Konfigurationsdateien").PerformClick();All(navigation).OfType<StudioAction>().Single(b=>b.Text=="Protokolle").PerformClick();Check(configCalls==1&&logCalls==1,"sidebar tools invoke the original native folder and log handlers");
                var mode=form.Controls.Find("studioMode",true).OfType<ComboBox>().Single();mode.SelectedIndex=1;Check(expertTool.Checked&&modeCalls==1,"expert selector invokes the original native mode toggle");mode.SelectedIndex=0;
                Check((NavigationChrome.SendMessage(tree.Handle,0x112d,IntPtr.Zero,IntPtr.Zero).ToInt64()&4)!=0,"native tree double buffering is enabled");
                Check(holder.Controls[0]==section1&&section1.Controls.Contains(input),"native control identity retained");
                Check(input.Text=="Titan"&&check.Checked&&childInput.Value==30,"theme construction preserves input values");
                Check(input.BorderStyle==BorderStyle.None&&checklist.BorderStyle==BorderStyle.None,"input and options-list white frames removed");
                Check(selection.SelectedIndex==0&&Object.ReferenceEquals(groupedRadio.Parent,framedGroup),"custom chrome preserves combo selection and native radio group");
                selection.ScreenPaints=selection.MemoryPaints=0;selection.Invalidate();selection.Update();
                Check(selection.ScreenPaints==0&&selection.MemoryPaints==0,"standard combo paints once without invoking the native frame renderer");
                int oldComboWidth=selection.Width;selection.Width=460;selection.Enabled=false;
                using(var image=new Bitmap(selection.Width,selection.Height))
                {
                    selection.DrawToBitmap(image,selection.ClientRectangle);
                    Check(image.GetPixel(selection.Width-60,selection.Height/2).ToArgb()==Palette.For(true).Input.ToArgb(),"resized disabled combo has no white trailing native field");
                    Check(image.GetPixel(selection.Width-1,selection.Height/2).ToArgb()==Palette.For(true).Border.ToArgb(),"combo border follows the full resized native client area");
                }
                selection.Width=oldComboWidth;selection.Enabled=true;
                holder.ScrollControlIntoView(nativeTabs);nativeTabs.ScreenPaints=nativeTabs.MemoryPaints=0;nativeTabs.Invalidate();nativeTabs.Update();
                Check(nativeTabs.ScreenPaints==0&&nativeTabs.MemoryPaints>0,"native tabs render to memory without an intermediate screen frame");
                selection.SelectedIndex=1;nativeTabs.SelectedIndex=1;Application.DoEvents();
                Check(selection.SelectedIndex==1&&nativeTabs.SelectedIndex==1,"buffered native controls retain selection behavior");selection.SelectedIndex=0;
                using(var slider=new PaintProbeSlider {Minimum=0,Maximum=100,Value=35,Location=new Point(280,330),Width=180})
                {
                    section1.Controls.Add(slider);slider.ScreenPaints=0;slider.Invalidate();slider.Update();
                    Check(slider.ScreenPaints==0&&slider.Value==35,"native music slider paints without a legacy frame and preserves its value");
                    int sliderChanges=0;slider.ValueChanged+=delegate{sliderChanges++;};slider.Value=65;slider.Update();
                    Check(slider.Value==65&&sliderChanges==1,"native slider value and notifications remain functional");
                }
                using(var pluginForm=new FirstPaintProbeForm())
                using(var observer=new DialogAppearanceObserver(delegate(Form candidate){if(Object.ReferenceEquals(candidate,pluginForm)){pluginForm.PreparedBeforeShow=!IsWindowVisible(candidate.Handle);preflightDialogTheme(candidate);pluginForm.Prepared=true;}}))
                {
                    pluginForm.ShowInTaskbar=false;pluginForm.Show(form);pluginForm.Refresh();Application.DoEvents();
                    Check(pluginForm.PreparedBeforeShow&&pluginForm.FirstPaintPrepared,"plugin dialog is themed before native visibility and its first paint");
                    pluginForm.Close();
                }
                int navigationThemeChanges=0;childInput.Invalidated+=delegate{navigationThemeChanges++;};
                var preflightTheme=new Theme(true);preflightTheme.Apply(section2);navigationThemeChanges=0;
                preflightTheme.Prepare(section2);Check(navigationThemeChanges==0,"preparing an existing page avoids redundant input invalidation");
                using(var image=new Bitmap(framedGroup.Width,framedGroup.Height))
                {
                    framedGroup.DrawToBitmap(image,framedGroup.ClientRectangle);
                    Check(image.GetPixel(0,40).ToArgb()==framedGroup.BackColor.ToArgb(),"group side frame is replaced by its flat surface");
                    Check(image.GetPixel(120,framedGroup.Height-1).ToArgb()==Palette.For(true).Border.ToArgb(),"group separator uses a muted theme color");
                }
                Check(shell.ChangedCount==0,"no dirty state after presentation changes");
                shell.SetDark(false);Check(!shell.IsDark&&input.ForeColor.GetBrightness()<.5,"light theme readable");
                Check(swatch.BackColor==Color.Red,"semantic color swatch preserved in light mode");
                shell.SetDark(true);Check(shell.IsDark&&input.ForeColor.GetBrightness()>.5,"dark theme readable");
                Check(selection.SelectedIndex==0&&selection.Items.Count==2,"native combo items survive light and dark switches");
                Check(input.Text=="Titan"&&check.Checked&&shell.ChangedCount==0,"theme switch has no setting side effects");
                shell.ToggleContext();Check(!shell.ContextVisible,"context can collapse");
                shell.ToggleContext();Check(shell.ContextVisible,"context can expand");
                input.Text="Apollo";Check(shell.ChangedCount==1,"field edit detected");
                shell.ToggleContext();shell.SetDark(false);Check(input.Text=="Apollo"&&shell.ChangedCount==1,"edits survive theme and context changes");
                input.Text="Titan";Check(shell.ChangedCount==0,"return to original value clears edit marker");
                tree.SelectedNode=child;Check(holder.Controls[0]==section2&&activations==2,"native page activation still runs");
                var tabs=form.Controls.Find("studioRoot",true)[0].Controls.Cast<Control>().SelectMany(All).OfType<Button>().Where(b=>Object.ReferenceEquals(b.Tag,parent)).ToArray();
                Check(tabs.Length==1,"subsection tab points to original navigation node");
                Check(tabs[0] is StudioTabButton&&tabs[0].FlatAppearance.BorderSize==0,"subsection tabs use underline styling without box frames");
                var tabStrip=(FlowLayoutPanel)tabs[0].Parent;
                var extraTabs=new List<Button>();
                for(int i=0;i<16;i++){var extra=new Button {Width=180,Height=40,Margin=new Padding(0,0,16,5)};extraTabs.Add(extra);tabStrip.Controls.Add(extra);}
                form.PerformLayout();tabStrip.PerformLayout();Application.DoEvents();
                Check(!tabStrip.AutoScroll&&!tabStrip.HorizontalScroll.Visible&&!tabStrip.VerticalScroll.Visible,"wrapped subsection tabs never expose native white scrollbars");
                Check(extraTabs.All(b=>b.Bottom<=tabStrip.ClientSize.Height),"all wrapped tab rows fit their automatically sized strip");
                foreach(var extra in extraTabs)extra.Dispose();tabStrip.PerformLayout();form.PerformLayout();
                tabs[0].PerformClick();Check(tree.SelectedNode==parent&&holder.Controls[0]==section1,"tab uses native activation lifecycle");
                Check(!tabs[0].IsDisposed&&form.Controls.Find("studioRoot",true)[0].Controls.Cast<Control>().SelectMany(All).Contains(tabs[0]),"navigation within a group reuses its tabs without rebuilding them");
                tree.SelectedNode=blocked;Check(tree.SelectedNode==parent,"native CanActivate veto preserved");
                check.Checked=false;Check(shell.ChangedCount==1,"change tracking recovers after native navigation veto");
                shell.SetDark(true);Check(swatch.BackColor==Color.Red,"semantic color swatch preserved in dark mode");
                if(shell.ContextVisible)shell.ToggleContext();var loaded=new Appearance(prefs);Check(loaded.Dark&&!loaded.ContextVisible,"appearance preferences persist independently");
                Check(saves==0&&cancels==0,"appearance never triggers native save or cancel");
                ok.PerformClick();cancel.PerformClick();Check(saves==1&&cancels==1,"original save and cancel handlers retained");
                Check(Object.ReferenceEquals(form.AcceptButton,ok)&&Object.ReferenceEquals(form.CancelButton,cancel),"Enter and Escape retain native buttons");
                Check(tools.Items.Contains(help),"native toolbar actions retained");
                var search=form.Controls.Find("studioSearch",true).OfType<TextBox>().Single();search.Text="Screensaver";
                var results=form.Controls.Find("studioSearchResults",true).OfType<ListBox>().Single();Check(results.Items.Count==1,"search finds original subsection");
                search.Text="Sound effects";Check(results.Items.Count==1,"search finds option labels without removing pages");
                search.Text="Bildschirmschoner";Check(results.Items.Count==1,"global search also finds German page aliases");search.Text="Sound effects";
                Check(tree.Nodes.Count==1&&parent.Nodes.Count==2,"search does not mutate native navigation");
                search.Text="media sound";Check(results.Items.Count==1,"search finds native checked-list options");search.Clear();
                int beforeChecks=shell.ChangedCount;checklist.SetItemChecked(1,true);Application.DoEvents();Check(shell.ChangedCount==beforeChecks+1,"native checked-list edits detected after ItemCheck");
                shell.SetDark(false);Check(checklist.GetItemChecked(0)&&checklist.GetItemChecked(1),"checked-list values survive theme switching");
                string audit=Path.Combine(directory,"fixture-inventory.xml");shell.StartAudit(audit);Check(File.Exists(audit),"read-only inventory generated");
                var back=form.Controls.Find("studioHistoryBack",true).OfType<Button>().Single();var forward=form.Controls.Find("studioHistoryForward",true).OfType<Button>().Single();
                back.PerformClick();Check(tree.SelectedNode==child&&holder.Controls[0]==section2,"history back uses the original native activation lifecycle");forward.PerformClick();Check(tree.SelectedNode==parent,"history forward restores the original page");
                float oldFont=input.Font.SizeInPoints;int dirty=shell.ChangedCount;shell.SetTextScale(150);
                Check(input.Font.SizeInPoints>oldFont&&input.Height>20,"larger type scales native field text together with its geometry");
                Check(input.Text=="Titan"&&childInput.Value==30&&shell.ChangedCount==dirty&&Object.ReferenceEquals(groupedRadio.Parent,framedGroup),"font scaling preserves native values, radio groups and edit state");
                Check(new Appearance(prefs).TextScale==150,"readability scale persists independently of MediaPortal settings");shell.SetTextScale(125);
                using(FrameRendering.Suspend(form))
                {
                    Check(!IsWindowVisible(form.Handle),"render transaction blocks intermediate visible frames");
                    using(FrameRendering.Suspend(form)){}
                    Check(!IsWindowVisible(form.Handle),"nested render transaction cannot expose an unfinished outer frame");
                }
                Check(IsWindowVisible(form.Handle),"render transaction restores the previously visible form");
                Check(FrameRendering.IsComposited(form),"native child windows share the form composition buffer");
                int originalScale=new Appearance(prefs).TextScale;Size originalSize=form.Size;
                shell.SetTextScale(175);form.Size=new Size(1150,800);form.PerformLayout();Application.DoEvents();
                var searchBar=form.Controls.Find("studioSearchBar",true).Single();
                var modeSelector=form.Controls.Find("studioMode",true).Single();var optionStrip=modeSelector.Parent;
                Check(!searchBar.Bounds.IntersectsWith(optionStrip.Bounds)&&optionStrip.Bottom<=optionStrip.Parent.ClientSize.Height,"large text at narrow window width keeps search and toolbar options on separate visible rows");
                Check(optionStrip.Controls.Cast<Control>().All(c=>c.Right<=optionStrip.ClientSize.Width),"all toolbar actions fit their responsive row at 175 percent");
                form.Size=originalSize;shell.SetTextScale(originalScale);
                File.WriteAllText(Path.Combine(directory,"passed.txt"),passed+" preservation checks passed\r\n");
            }
            using(var list=new ListView {View=View.Details,Font=new Font("Segoe UI",12)})
            using(var bitmap=new Bitmap(250,34))
            using(var graphics=Graphics.FromImage(bitmap))
            {
                list.Columns.Add("Name",250);var row=list.Items.Add("DefaultWideHD – Copy");row.SubItems.Add("1.4.0.0");
                foreach(bool dark in new[]{true,false})
                {
                    Palette palette=Palette.For(dark);graphics.Clear(Color.Magenta);
                    Theme.PaintSelectedCell(graphics,list,row,row.SubItems[0],0,new Rectangle(0,0,250,34),palette);
                    Check(bitmap.GetPixel(240,3).ToArgb()==palette.Selection.ToArgb(),"selected list rows have an explicit themed background in "+(dark?"dark":"light")+" mode");
                    int textPixels=0;for(int y=4;y<30;y++)for(int x=8;x<220;x++)if(bitmap.GetPixel(x,y).ToArgb()==palette.Text.ToArgb())textPixels++;
                    Check(textPixels>20,"selected list text uses the readable theme foreground in "+(dark?"dark":"light")+" mode");
                }
            }
            foreach(bool dark in new[]{true,false})
            {
                Palette palette=Palette.For(dark);Color readable=Theme.ReadableForeground(Color.FromArgb(25,45,75),palette.Input,palette.Text);
                Check(Theme.Contrast(readable,palette.Input)>=4.5,"legacy blue list text remains readable in "+(dark?"dark":"light")+" mode");
                Check(Theme.ReadableForeground(palette.Text,palette.Input,palette.Text)==palette.Text,"already readable list colors are preserved");
            }
            using(var zoom=new PageZoom())
            using(var page=new UserControl {Size=new Size(700,450)})
            {
                var group=new GroupBox {Bounds=new Rectangle(10,10,650,120),Anchor=AnchorStyles.Left|AnchorStyles.Top|AnchorStyles.Right};
                var field=new TextBox {Bounds=new Rectangle(430,35,180,25),Text="unchanged"};group.Controls.Add(field);page.Controls.Add(group);zoom.Capture(page);
                zoom.Apply(page,1.25f);Rectangle expectedGroup=group.Bounds,expectedField=field.Bounds;
                for(int i=0;i<8;i++){page.Width=1100;zoom.Apply(page,1.75f);page.Width=900;zoom.Apply(page,1);zoom.Apply(page,1.25f);}
                Check(group.Bounds==expectedGroup&&field.Bounds==expectedField,"repeated zoom and viewport resize restore anchored groups and fields without geometry drift");
                Check(field.Text=="unchanged","stable geometry scaling preserves native text values");
                var lateField=new TextBox {Bounds=new Rectangle(5,65,100,25),Text="late native field"};group.Controls.Add(lateField);zoom.Apply(page,1.25f);
                Check(Math.Abs(lateField.Font.SizeInPoints-field.Font.SizeInPoints)<.05f&&lateField.Width==125,"late native controls inherit the original font baseline and are scaled at the current zoom");
            }
            using(var testForm=new Form())
            using(var zoom=new PageZoom())
            {
                var page=new UserControl {Size=new Size(500,300)};var group=new GroupBox {Bounds=new Rectangle(10,10,200,80)};
                var field=new TextBox {Bounds=new Rectangle(170,45,150,25),Text="content"};group.Controls.Add(field);page.Controls.Add(group);testForm.Controls.Add(page);testForm.Show();zoom.Capture(page);zoom.EnsureContentFits(page);
                Check(field.Right<=group.ClientSize.Width&&group.Right<=page.Width,"native groups and their page minimum expand to retain overflowing fields");
                Size stable=page.MinimumSize;zoom.EnsureContentFits(page);Check(page.MinimumSize==stable,"content fitting is idempotent and does not grow docked layouts on each refresh");
                var slider=new TrackBar();page.Controls.Add(slider);IntPtr handle=slider.Handle;
                NavigationChrome.SendMessage(handle,0x0408,new IntPtr(1),new IntPtr(1000));NavigationChrome.SendMessage(handle,0x0405,new IntPtr(1),new IntPtr(500));
                Check(slider.Value==500,"native slider fixture starts with a direct HWND range above the WinForms default");
                using(var state=new NativePresentationState(slider)){slider.Font=new Font("Segoe UI",14);}
                Check(slider.Value==500&&slider.Maximum==1000,"native slider range and value survive handle recreation during presentation changes");
                var theme=new Theme(true);theme.Apply(slider);theme.Set(false);theme.Apply(slider);Check(slider.Value==500,"native buffering slider value survives both themes");
                page.BackColorChanged+=delegate{slider.Value=10;};theme.Set(true);theme.Apply(page);
                Check(slider.Value==500,"the complete presentation transaction preserves slider state even when a parent change triggers native sibling handlers");
                var loading=new ComboBox {DropDownStyle=ComboBoxStyle.DropDown};page.Controls.Add(loading);loading.Text="Loading grabber scripts...";
                string placeholder=loading.Text;theme.Set(false);theme.Apply(page);
                Check(placeholder=="Loading grabber scripts..."&&loading.Text==placeholder&&loading.SelectedIndex==-1,"unselected combo text survives a theme change without selecting a nonexistent item");
                testForm.Close();
            }
            Check(Theme.Contrast(Palette.For(true).Text,Palette.For(true).Hover)>=4.5,"brighter hover highlight retains readable text contrast");
            using(var validation=new TextBox {Text="native validation",BackColor=Color.YellowGreen})
            {
                var theme=new Theme(true);theme.Apply(validation);
                Check(validation.BackColor==Color.YellowGreen&&Theme.Contrast(validation.ForeColor,validation.BackColor)>=4.5,"native successful validation color remains exact and readable in dark mode");
                validation.BackColor=Color.Red;theme.Set(false);theme.Apply(validation);
                Check(validation.BackColor==Color.Red&&Theme.Contrast(validation.ForeColor,validation.BackColor)>=4.5,"native failed validation color remains exact and readable after a theme switch");
            }
            VerifyStartup(prefs);
            Console.WriteLine(passed+" checks passed.");return 0;
        }
        private static void VerifyStartup(string prefs)
        {
            using(var hidden=new Form())
            {
                IntPtr handle=hidden.Handle;using(FrameRendering.Suspend(hidden)){}
                Check(!IsWindowVisible(handle),"render transaction never shows a previously hidden startup form");
            }
            using(var startup=new FirstPaintProbeForm {ShowInTaskbar=false})
            {
                var tree=new TreeView();var holder=new Panel();var ok=new Button();var cancel=new Button();var tools=new ToolStrip();
                var page=new UserControl {Text="General",Size=new Size(500,400)};var input=new TextBox {Text="before native Load"};page.Controls.Add(input);holder.Controls.Add(page);
                TreeNode node=tree.Nodes.Add("General");node.Tag=page;tree.SelectedNode=node;
                startup.Controls.AddRange(new Control[]{tree,holder,ok,cancel,tools});
                startup.Load+=delegate{input.Text="loaded by native Load";};
                StudioShell attached=null;
                bool firstSurfacePaint=false,completeSurfacePaint=false;
                using(var observer=new DialogAppearanceObserver(delegate(Form candidate){
                    if(!Object.ReferenceEquals(candidate,startup))return;
                    startup.PreparedBeforeShow=!IsWindowVisible(candidate.Handle);
                    attached=StudioShell.AttachFixture(startup,tree,holder,ok,cancel,tools,prefs,new List<Control>{page});
                    var surface=startup.Controls.Find("studioStatus",true).Single();
                    surface.Paint+=delegate{if(!firstSurfacePaint&&IsWindowVisible(surface.Handle)){firstSurfacePaint=true;completeSurfacePaint=startup.Prepared&&startup.Controls.Find("studioRoot",true).Length==1;}};
                    startup.Prepared=true;
                }))
                {
                    startup.Show();startup.Refresh();Application.DoEvents();
                    Check(attached!=null&&attached.PreparedBeforeFirstDisplay&&startup.PreparedBeforeShow,"main shell attaches before the first native display instead of Idle");
                    Check(firstSurfacePaint&&completeSurfacePaint&&startup.UnpreparedVisiblePaints==0,"first visible main-window surface already contains the completed Studio layout");
                    Check(input.Text=="loaded by native Load"&&attached.ChangedCount==0,"early presentation setup preserves native Load and clean input baselines");
                    startup.Close();
                }
            }
        }
        private static IEnumerable<Control> All(Control root){yield return root;foreach(Control child in root.Controls)foreach(Control c in All(child))yield return c;}
        public static int Preview()
        {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);
            var form=new Form {Size=new Size(1600,1000)};var tree=new TreeView();var holder=new Panel();var ok=new Button();var cancel=new Button();var tools=new ToolStrip();
            var pages=new List<Control>();TreeNode selected=null;
            foreach(string kind in new[]{"General","GUI","Movies","Music","Pictures","Remote","PluginsNew","Project"})
            {
                var page=new UserControl {Text=kind,Size=new Size(520,420)};var group=new GroupBox {Text="Darstellung",Bounds=new Rectangle(12,12,490,190)};
                group.Controls.Add(new Label {Text="Skin",Bounds=new Rectangle(18,30,130,24)});var choice=new ComboBox {DropDownStyle=ComboBoxStyle.DropDownList,Bounds=new Rectangle(160,28,280,26)};choice.Items.Add("Titan");choice.SelectedIndex=0;group.Controls.Add(choice);
                group.Controls.Add(new CheckBox {Text="Skin-Soundeffekte aktivieren",Checked=true,Bounds=new Rectangle(18,78,400,25)});group.Controls.Add(new CheckBox {Text="Zuletzt ausgewählten Eintrag merken",Checked=true,Bounds=new Rectangle(18,115,420,25)});page.Controls.Add(group);pages.Add(page);
                TreeNode node=tree.Nodes.Add(kind);node.Tag=page;if(kind=="GUI")selected=node;
            }
            tree.AfterSelect+=delegate(object sender,TreeViewEventArgs e){holder.Controls.Clear();holder.Controls.Add((Control)e.Node.Tag);};tree.SelectedNode=selected;
            form.Controls.AddRange(new Control[]{tree,holder,ok,cancel,tools});cancel.Click+=delegate{form.Close();};
            form.Load+=delegate{StudioShell.AttachFixture(form,tree,holder,ok,cancel,tools,Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"design-preview.xml"),pages);form.Text+=" · DESIGN-VORSCHAU MIT TESTFELDERN";};
            Application.Run(form);return 0;
        }
        private sealed class PaintProbeCombo : ComboBox
        {
            public int ScreenPaints,MemoryPaints;
            protected override void WndProc(ref Message m){if(m.Msg==0xf)ScreenPaints++;if(m.Msg==0x318)MemoryPaints++;base.WndProc(ref m);}
        }
        private sealed class PaintProbeTabs : TabControl
        {
            public int ScreenPaints,MemoryPaints;
            protected override void WndProc(ref Message m){if(m.Msg==0xf)ScreenPaints++;if(m.Msg==0x318)MemoryPaints++;base.WndProc(ref m);}
        }
        private sealed class PaintProbeSlider : TrackBar
        {
            public int ScreenPaints;
            protected override void WndProc(ref Message m){if(m.Msg==0xf)ScreenPaints++;base.WndProc(ref m);}
        }
        [System.Runtime.InteropServices.DllImport("user32.dll")] private static extern bool IsWindowVisible(IntPtr hwnd);
        [System.Runtime.InteropServices.DllImport("user32.dll")] private static extern int GetScrollPos(IntPtr hwnd,int bar);
        private static void preflightDialogTheme(Form candidate){new Theme(true).Apply(candidate);}
        private sealed class FirstPaintProbeForm : Form
        {
            public bool Prepared,PreparedBeforeShow,FirstPaintPrepared,FirstPaintStudio;public int UnpreparedVisiblePaints;private bool painted;
            protected override void OnPaint(PaintEventArgs e){if(IsWindowVisible(Handle)){if(!Prepared)UnpreparedVisiblePaints++;if(!painted){FirstPaintPrepared=Prepared;FirstPaintStudio=Controls.Find("studioRoot",true).Length==1;painted=true;}}base.OnPaint(e);}
        }
    }
}


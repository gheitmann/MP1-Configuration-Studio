using System;
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
        // Explicit developer-only integration review, refused outside a marked
        // runtime with every mutable native folder redirected to its copy.
        public void StartFullReview(string path)
        {
            string runtime=AppDomain.CurrentDomain.BaseDirectory;
            if(!File.Exists(Path.Combine(runtime,"studio-test-runtime.marker")))throw new InvalidOperationException("Full review requires an isolated runtime.");
            var config=Assembly.Load("Utils").GetType("MediaPortal.Configuration.Config");var dir=config.GetNestedType("Dir");
            string isolatedRoot=Path.GetFullPath(Path.Combine(runtime,"..")).TrimEnd('\\')+"\\";
            foreach(string kind in new[]{"Config","Log","Database","Thumbs","Cache"})
            {
                string location=Convert.ToString(config.GetMethod("GetFolder").Invoke(null,new[]{Enum.Parse(dir,kind)}));
                if(!Path.GetFullPath(location).StartsWith(isolatedRoot,StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("Non-isolated folder: "+kind);
            }
            SetMode(true);
            form.Text="MP1 Configuration Studio · ISOLIERTE SEITENPRÜFUNG";
            var nodes=Flatten(tree.Nodes).ToArray();int index=0;int originalScale=appearance.TextScale;bool originalDark=appearance.Dark;
            var document=new XmlDocument();var root=document.CreateElement("fullReview");document.AppendChild(root);
            root.SetAttribute("build",System.Diagnostics.FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion);root.SetAttribute("expectedPages",sections.Count.ToString());root.SetAttribute("nodes",nodes.Length.ToString());
            var timer=new Timer {Interval=120};
            Action save=delegate{Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));document.Save(path);};
            timer.Tick+=delegate{
                timer.Stop();
                if(index>=nodes.Length){RunPluginDialogChecks(document,root,isolatedRoot);SetTextScale(originalScale);SetDark(originalDark);root.SetAttribute("complete","true");save();timer.Dispose();nativeCancel.PerformClick();return;}
                TreeNode node=nodes[index++];Control page=NodeSection(node);var result=document.CreateElement("page");root.AppendChild(result);
                result.SetAttribute("path",node.FullPath);result.SetAttribute("type",page==null?"":page.GetType().Name);
                try
                {
                    bool active=Navigate(node)&&holder.Controls.Count>0&&Object.ReferenceEquals(holder.Controls[0],page);
                    result.SetAttribute("activated",active.ToString());
                    if(active)
                    {
                        var transient=Descendants(page).OfType<ComboBox>().Where(c=>c.Name=="mpComboBoxAvailableDatabases"&&(c.Items.Count==0||c.Text=="Loading grabber scripts...")).ToArray();
                        foreach(var status in transient){var note=document.CreateElement("transientStatus");note.SetAttribute("control",status.Name);note.SetAttribute("reason","asynchronous grabber discovery; this selector is not a persisted setting");result.AppendChild(note);}
                        var inputs=Descendants(page).Where(c=>Value(c)!=null&&!transient.Contains(c)).ToDictionary(c=>c,Value);
                        ReviewSliderState(page,result,document,"activated");
                        bool stable=true;
                        foreach(int scale in new[]{100,125,175})
                        {
                            pageZoom.Apply(page,scale/100f*9.5f/8.25f);LayoutNativePage();page.PerformLayout();
                            ReviewSliderState(page,result,document,"zoom "+scale);
                            foreach(bool dark in new[]{false,true})
                            {
                                theme.Set(dark);theme.Apply(page);
                                ReviewSliderState(page,result,document,"theme "+dark+" zoom "+scale);
                                foreach(var input in inputs.Where(v=>v.Key.IsDisposed||Value(v.Key)!=v.Value))
                                {
                                    stable=false;var difference=document.CreateElement("valueDifference");difference.SetAttribute("control",input.Key.Name);difference.SetAttribute("type",input.Key.GetType().Name);difference.SetAttribute("scale",scale.ToString());difference.SetAttribute("dark",dark.ToString());
                                    if(input.Key is TrackBar||input.Key is NumericUpDown||input.Key is CheckBox||input.Key is RadioButton){difference.SetAttribute("before",input.Value);difference.SetAttribute("after",input.Key.IsDisposed?"disposed":Value(input.Key));}
                                    var combo=input.Key as ComboBox;if(combo!=null){difference.SetAttribute("before",input.Value);difference.SetAttribute("after",combo.Text);difference.SetAttribute("selectedIndex",combo.SelectedIndex.ToString());difference.SetAttribute("style",combo.DropDownStyle.ToString());}
                                    result.AppendChild(difference);
                                }
                            }
                        }
                        pageZoom.Apply(page,PageScale);theme.Set(originalDark);theme.Apply(page);pageZoom.EnsureContentFits(page);LayoutNativePage();
                        result.SetAttribute("themeAndScaleValuesPreserved",stable.ToString());
                        int tabPages=0;
                        foreach(var tabs in Descendants(page).OfType<TabControl>().ToArray())
                        {
                            int selected=tabs.SelectedIndex;foreach(TabPage tab in tabs.TabPages){tabs.SelectedTab=tab;tab.PerformLayout();tabPages++;}if(selected>=0&&selected<tabs.TabCount)tabs.SelectedIndex=selected;
                        }
                        result.SetAttribute("detailTabsActivated",tabPages.ToString());
                        result.SetAttribute("controls",Descendants(page).Count().ToString());
                        if(page.GetType().Name=="MovieViews"||page.GetType().Name=="MusicViews")result.SetAttribute("freshViewRows",Descendants(page).OfType<DataGridView>().Select(grid=>grid.DataSource as System.Data.DataTable).Where(table=>table!=null).Sum(table=>table.Rows.Count).ToString());
                        result.SetAttribute("lowContrastListCells",Descendants(page).OfType<ListView>().Where(list=>list.Enabled).SelectMany(list=>list.Items.Cast<ListViewItem>()).SelectMany(item=>item.SubItems.Cast<ListViewItem.ListViewSubItem>()).Count(cell=>Theme.Contrast(cell.ForeColor,cell.BackColor)<4.5).ToString());
                        result.SetAttribute("minimumFontPoints",Descendants(page).Where(c=>c.Visible&&!String.IsNullOrEmpty(c.Text)).Select(c=>c.Font.SizeInPoints).DefaultIfEmpty(0).Min().ToString(System.Globalization.CultureInfo.InvariantCulture));
                        foreach(var method in page.GetType().GetMethods(BindingFlags.Public|BindingFlags.Instance).Where(m=>m.Name=="SaveSettings"||m.Name=="LoadSettings"))
                        {var api=document.CreateElement("nativeMethod");api.SetAttribute("signature",method.ToString());result.AppendChild(api);}
                        if(new[]{"General","GeneralStartupResume","GuiControls","GuiThumbs","GuiScreensaver","GuiFileMenu","GuiOSD","GuiSkipSteps"}.Contains(page.GetType().Name))ReviewRoundTrip(page,result,document,isolatedRoot);
                        foreach(var control in Descendants(page).Where(c=>c.Visible&&c.Parent!=null&&!(c.Parent is ScrollableControl&&((ScrollableControl)c.Parent).AutoScroll)&&!(c is TabPage)&&c.Dock==DockStyle.None&&(c.Right>c.Parent.ClientSize.Width+3||c.Bottom>c.Parent.ClientSize.Height+3)))
                        {var warning=document.CreateElement("overflowCandidate");warning.SetAttribute("control",control.Name);warning.SetAttribute("type",control.GetType().Name);warning.SetAttribute("parent",control.Parent.Name);warning.SetAttribute("bounds",control.Bounds.ToString());warning.SetAttribute("parentSize",control.Parent.ClientSize.ToString());result.AppendChild(warning);}
                    }
                }
                catch(Exception ex){result.SetAttribute("error",ex.GetBaseException().Message);}
                root.SetAttribute("reviewed",index.ToString());save();timer.Start();
            };
            timer.Start();
        }
        private void ReviewSliderState(Control page,XmlElement result,XmlDocument document,string phase)
        {
            foreach(var slider in Descendants(page).OfType<TrackBar>())
            {
                var state=document.CreateElement("sliderState");state.SetAttribute("phase",phase);state.SetAttribute("name",slider.Name);state.SetAttribute("value",slider.Value.ToString());state.SetAttribute("minimum",slider.Minimum.ToString());state.SetAttribute("maximum",slider.Maximum.ToString());state.SetAttribute("handle",slider.IsHandleCreated.ToString());
                if(slider.IsHandleCreated){state.SetAttribute("nativeMinimum",NavigationChrome.SendMessage(slider.Handle,0x0401,IntPtr.Zero,IntPtr.Zero).ToString());state.SetAttribute("nativeMaximum",NavigationChrome.SendMessage(slider.Handle,0x0402,IntPtr.Zero,IntPtr.Zero).ToString());}
                result.AppendChild(state);
            }
        }
        private void ReviewRoundTrip(Control page,XmlElement result,XmlDocument document,string isolatedRoot)
        {
            var test=document.CreateElement("saveReload");result.AppendChild(test);
            var field=Descendants(page).OfType<CheckBox>().FirstOrDefault(c=>c.Enabled&&c.Visible&&c.ThreeState==false);
            if(field==null){test.SetAttribute("status","no suitable checkbox");return;}
            var save=page.GetType().GetMethod("SaveSettings",Type.EmptyTypes);var load=page.GetType().GetMethod("LoadSettings",Type.EmptyTypes);
            if(save==null||load==null){test.SetAttribute("status","native methods unavailable");return;}
            bool original=field.Checked;test.SetAttribute("field",field.Name);
            Type profile=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("MediaPortal.Profile.Settings",false)).FirstOrDefault(t=>t!=null);
            MethodInfo flush=profile==null?null:profile.GetMethod("SaveCache",BindingFlags.Public|BindingFlags.Static,null,Type.EmptyTypes,null);
            MethodInfo clear=profile==null?null:profile.GetMethod("ClearCache",BindingFlags.Public|BindingFlags.Static,null,Type.EmptyTypes,null);
            string configFile=Path.Combine(isolatedRoot,"config","MediaPortal.xml");string before=File.Exists(configFile)?File.ReadAllText(configFile):"";
            try
            {
                field.Checked=!original;save.Invoke(page,null);if(flush!=null)flush.Invoke(null,null);
                string persisted=File.Exists(configFile)?File.ReadAllText(configFile):"";
                if(clear!=null)clear.Invoke(null,null);load.Invoke(page,null);
                test.SetAttribute("changedValueReloaded",(field.Checked==!original).ToString());test.SetAttribute("profileFileChanged",(persisted!=before).ToString());test.SetAttribute("cacheFlushed",(flush!=null).ToString());test.SetAttribute("cacheCleared",(clear!=null).ToString());
                test.SetAttribute("status",field.Checked==!original&&persisted!=before?"passed":"failed");
            }
            catch(Exception ex){test.SetAttribute("status","failed");test.SetAttribute("error",ex.GetBaseException().Message);}
            finally
            {
                field.Checked=original;save.Invoke(page,null);if(flush!=null)flush.Invoke(null,null);if(clear!=null)clear.Invoke(null,null);load.Invoke(page,null);test.SetAttribute("originalValueRestored",(field.Checked==original).ToString());
            }
        }
    }
}

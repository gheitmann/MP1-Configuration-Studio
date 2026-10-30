using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Windows.Forms;
using System.Xml;

namespace MP1.ConfigurationStudio
{
    public sealed partial class StudioShell
    {
        public void StartPersistenceReview(string path)
        {
            string runtime=AppDomain.CurrentDomain.BaseDirectory,rootPath=Path.GetFullPath(Path.Combine(runtime,"..")).TrimEnd('\\')+"\\";
            if(!File.Exists(Path.Combine(runtime,"studio-test-runtime.marker")))throw new InvalidOperationException("Persistence tests require an isolated runtime.");
            Type config=Assembly.Load("Utils").GetType("MediaPortal.Configuration.Config"),dir=config.GetNestedType("Dir");
            foreach(string name in new[]{"Config","Log","Database","Thumbs","Cache","Skin","Language"})
            {
                string folder=Path.GetFullPath(Convert.ToString(config.GetMethod("GetFolder").Invoke(null,new[]{Enum.Parse(dir,name)})));
                if(!folder.StartsWith(rootPath,StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("Persistence test refused non-isolated "+name+" directory.");
            }
            if(!Path.GetFullPath(IsolatedRuntime.DebugMarkerPath()).StartsWith(rootPath,StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("Persistence test refused non-isolated debug marker path.");
            var policy=new XmlDocument();policy.Load(Path.Combine(rootPath,"persistence-policy.xml"));
            var report=new XmlDocument();var root=report.CreateElement("persistenceReview");report.AppendChild(root);
            root.SetAttribute("build",System.Diagnostics.FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion);
            root.SetAttribute("sourceCommit",policy.DocumentElement.GetAttribute("sourceCommit"));
            SetMode(true);form.Text="MP1 Configuration Studio · ISOLIERTE SPEICHERPRÜFUNG";
            TreeNode[] nodes=Flatten(tree.Nodes).ToArray();int index=0;var timer=new Timer {Interval=180};
            timer.Tick+=delegate{
                timer.Stop();
                if(index==nodes.Length){root.SetAttribute("complete","true");SaveReview(report,path);timer.Dispose();nativeCancel.PerformClick();return;}
                TreeNode node=nodes[index++];Control page=NodeSection(node);var item=report.CreateElement("page");root.AppendChild(item);
                string type=page.GetType().Name;item.SetAttribute("type",type);item.SetAttribute("path",node.FullPath);
                try
                {
                    XmlElement entry=policy.SelectSingleNode("/persistencePolicy/page[@type='"+type+"']") as XmlElement;
                    string scope=entry==null?"unclassified":entry.GetAttribute("scope");item.SetAttribute("scope",scope);
                    if(scope!="isolated-file"){item.SetAttribute("status",scope=="readonly"?"no persistence":"separate test required");}
                    else if(type=="ThirdPartyChecks"){throw new InvalidOperationException("System checks do not belong to the file persistence test.");}
                    else
                    {
                        if(!Navigate(node))throw new InvalidOperationException("Native activation vetoed.");
                        MethodInfo save=page.GetType().GetMethod("SaveSettings",Type.EmptyTypes),load=page.GetType().GetMethod("LoadSettings",Type.EmptyTypes);
                        var baseline=report.CreateElement("baseline");item.AppendChild(baseline);
                        var before=ConfigHashes(rootPath);save.Invoke(page,null);baseline.SetAttribute("saveCompleted","True");FlushNativeProfile(false);FlushNativeProfile(true);
                        bool freshReload=type=="MovieViews"||type=="MusicViews";
                        if(!freshReload){ReloadReviewPage(page,load);CorrectStoredSettingDisplay(page,true);baseline.SetAttribute("loadCompleted","True");}
                        else baseline.SetAttribute("restartRequired","True");
                        baseline.SetAttribute("status",freshReload?"serialized; reload in fresh process":"serialized and reloaded");RecordChangedFiles(report,baseline,before,ConfigHashes(rootPath));
                        var names=new HashSet<string>(entry.SelectNodes("field").Cast<XmlElement>().Select(e=>e.GetAttribute("name")));
                        var candidates=Descendants(page).Where(c=>names.Contains(c.Name)&&c.Enabled).OrderBy(c=>c is CheckBox?0:c is NumericUpDown?1:c is TrackBar?2:c is ComboBox?3:4).ToArray();
                        int tried=0;var kinds=new HashSet<Type>();
                        foreach(Control candidate in candidates)
                        {
                            if(tried>=3)break;
                            // Enabling a physical remote can have immediate effects outside
                            // the profile. Its verbose flags and numeric delays remain eligible.
                            if(type.StartsWith("Remote")&&candidate is CheckBox&&candidate.Name.IndexOf("verbose",StringComparison.OrdinalIgnoreCase)<0)continue;
                            var check=candidate as CheckBox;var number=candidate as NumericUpDown;var slider=candidate as TrackBar;var combo=candidate as ComboBox;var text=candidate as TextBox;
                            string original=Value(candidate),changed=null;Type kind=candidate is CheckBox?typeof(CheckBox):candidate is NumericUpDown?typeof(NumericUpDown):candidate is TrackBar?typeof(TrackBar):candidate is ComboBox?typeof(ComboBox):typeof(TextBox);
                            if(kinds.Contains(kind))continue;
                            if(check!=null&&!check.ThreeState)changed=check.Checked?"Unchecked":"Checked";
                            else if(number!=null&&number.Maximum>number.Minimum)changed=(number.Value<number.Maximum?Math.Min(number.Maximum,number.Value+number.Increment):Math.Max(number.Minimum,number.Value-number.Increment)).ToString(CultureInfo.InvariantCulture);
                            else if(slider!=null&&slider.Maximum>slider.Minimum)changed=(slider.Value<slider.Maximum?Math.Min(slider.Maximum,slider.Value+Math.Max(1,slider.TickFrequency)):Math.Max(slider.Minimum,slider.Value-Math.Max(1,slider.TickFrequency))).ToString(CultureInfo.InvariantCulture);
                            else if(combo!=null&&combo.Items.Count>1&&!System.Text.RegularExpressions.Regex.IsMatch(combo.Name,"device|player|renderer|codec|filter|database|drive|port|profile",System.Text.RegularExpressions.RegexOptions.IgnoreCase)){int next=(combo.SelectedIndex+1)%combo.Items.Count;changed=combo.GetItemText(combo.Items[next]);}
                            else if(text!=null&&System.Text.RegularExpressions.Regex.IsMatch(text.Name,"delay|timeout|seconds|interval|limit",System.Text.RegularExpressions.RegexOptions.IgnoreCase)){int value;if(Int32.TryParse(text.Text,out value)&&value>=0&&value<100000)changed=(value+1).ToString(CultureInfo.InvariantCulture);}
                            if(changed==null||original==changed)continue;
                            kinds.Add(kind);tried++;
                            var test=report.CreateElement("mutation");test.SetAttribute("field",candidate.Name);test.SetAttribute("kind",kind.Name);item.AppendChild(test);
                            string name=candidate.Name;
                            CheckBox dependency=null;bool dependencyOriginal=false;
                            try
                            {
                                if(type=="MusicDSP"&&name=="trackBarCompressor")
                                {
                                    dependency=Descendants(page).OfType<CheckBox>().First(c=>c.Name=="checkBoxCompressor");dependencyOriginal=dependency.Checked;dependency.Checked=true;
                                    save.Invoke(page,null);FlushNativeProfile(false);FlushNativeProfile(true);ReloadReviewPage(page,load);test.SetAttribute("conditionalEffectEnabledForTest","True");
                                }
                                ActivateFieldTab(candidate);SetReviewValue(candidate,changed);before=ConfigHashes(rootPath);save.Invoke(page,null);FlushNativeProfile(false);FlushNativeProfile(true);ReloadReviewPage(page,load);
                                CorrectStoredSettingDisplay(page,true);Control reloaded=Descendants(page).FirstOrDefault(c=>c.Name==name);bool matches=reloaded!=null&&Value(reloaded)==changed;
                                RecordChangedFiles(report,test,before,ConfigHashes(rootPath));bool fileChanged=test.SelectNodes("changedFile").Cast<XmlElement>().Any(file=>!file.GetAttribute("relativePath").EndsWith(".bak",StringComparison.OrdinalIgnoreCase));
                                test.SetAttribute("status",matches&&fileChanged?"passed":matches?"not serialized":"value not reloaded");
                            }
                            catch(Exception ex){test.SetAttribute("status","failed");test.SetAttribute("error",ex.GetBaseException().Message);}
                            finally
                            {
                                Control field=Descendants(page).FirstOrDefault(c=>c.Name==name);if(field==null)throw new InvalidOperationException("Original field no longer exists: "+name);
                                SetReviewValue(field,original);save.Invoke(page,null);FlushNativeProfile(false);FlushNativeProfile(true);ReloadReviewPage(page,load);
                                if(dependency!=null){dependency.Checked=dependencyOriginal;save.Invoke(page,null);FlushNativeProfile(false);FlushNativeProfile(true);ReloadReviewPage(page,load);test.SetAttribute("conditionalEffectRestored",(dependency.Checked==dependencyOriginal).ToString());}
                                CorrectStoredSettingDisplay(page,true);field=Descendants(page).FirstOrDefault(c=>c.Name==name);test.SetAttribute("originalRestored",(field!=null&&Value(field)==original).ToString());
                            }
                        }
                        item.SetAttribute("status","completed");
                    }
                }
                catch(Exception ex){item.SetAttribute("status","failed");item.SetAttribute("error",ex.GetBaseException().Message);item.SetAttribute("stack",ex.GetBaseException().StackTrace);}
                root.SetAttribute("reviewed",index.ToString());SaveReview(report,path);timer.Start();
            };
            timer.Start();
        }
        private void ActivateFieldTab(Control field){var chain=new List<TabPage>();for(Control p=field.Parent;p!=null;p=p.Parent)if(p is TabPage)chain.Add((TabPage)p);foreach(var page in chain.AsEnumerable().Reverse()){var tabs=page.Parent as TabControl;if(tabs!=null)tabs.SelectedTab=page;}}
        private void ReloadReviewPage(Control page,MethodInfo load)
        {
            if(page.GetType().Name=="MusicDSP")
            {
                Type settings=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("MediaPortal.Player.DSP.Settings",false)).First(t=>t!=null);
                settings.GetField("m_Instance",BindingFlags.NonPublic|BindingFlags.Static).SetValue(null,null);
            }
            load.Invoke(page,null);
            if(page.GetType().Name=="FiltersSection")foreach(Control child in Descendants(page))
            {
                Type type=child.GetType();for(Type parent=type.BaseType;parent!=null;parent=parent.BaseType)if(parent.FullName=="MediaPortal.Configuration.SectionSettings"){type.GetMethod("LoadSettings",Type.EmptyTypes).Invoke(child,null);break;}
            }
        }
        private static void SetReviewValue(Control field,string value)
        {
            if(field is CheckBox)((CheckBox)field).CheckState=(CheckState)Enum.Parse(typeof(CheckState),value);
            else if(field is NumericUpDown)((NumericUpDown)field).Value=Decimal.Parse(value,CultureInfo.InvariantCulture);
            else if(field is TrackBar)((TrackBar)field).Value=Int32.Parse(value,CultureInfo.InvariantCulture);
            else if(field is ComboBox){var combo=(ComboBox)field;int index=combo.FindStringExact(value);if(index>=0)combo.SelectedIndex=index;else combo.Text=value;}
            else field.Text=value;
        }
        private static void FlushNativeProfile(bool clear)
        {
            Type profile=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("MediaPortal.Profile.Settings",false)).First(t=>t!=null);
            profile.GetMethod(clear?"ClearCache":"SaveCache",BindingFlags.Public|BindingFlags.Static,null,Type.EmptyTypes,null).Invoke(null,null);
        }
        private static Dictionary<string,string> ConfigHashes(string root)
        {
            var hashes=new Dictionary<string,string>();
            foreach(string kind in new[]{"config","debug"})
            {
                string config=Path.Combine(root,kind);if(!Directory.Exists(config))continue;
                foreach(string file in Directory.GetFiles(config,"*",SearchOption.AllDirectories))
                {using(var sha=SHA256.Create())using(var stream=File.Open(file,FileMode.Open,FileAccess.Read,FileShare.ReadWrite))hashes[kind+"/"+file.Substring(config.Length+1)]=BitConverter.ToString(sha.ComputeHash(stream)).Replace("-","");}
            }
            return hashes;
        }
        private static void RecordChangedFiles(XmlDocument report,XmlElement item,Dictionary<string,string> before,Dictionary<string,string> after)
        {foreach(var file in after){string hash;if(!before.TryGetValue(file.Key,out hash)||hash!=file.Value){var changed=report.CreateElement("changedFile");changed.SetAttribute("relativePath",file.Key);item.AppendChild(changed);}}}
        private static void SaveReview(XmlDocument report,string path){Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));report.Save(path);}
    }
}

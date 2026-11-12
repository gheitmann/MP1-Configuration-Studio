#if DIAGNOSTICS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;

namespace MP1.ConfigurationStudio
{
    public sealed partial class StudioShell
    {
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
                    string isolatedRoot=Path.GetFullPath(Path.Combine(runtime,"..")).TrimEnd('\\')+"\\";
                    foreach(string kind in new[]{"Config","Log","Database","Thumbs","Cache"})
                    {
                        string location=NativeContract.ConfigFolder(kind);
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
                        TreeNode node=Flatten(tree.Nodes).FirstOrDefault(n=>NativeContract.NodeSection(n)!=null&&NativeContract.NodeSection(n).GetType().Name==name);
                        bool navigated=node!=null&&Navigate(node);
                        checks.Add(new KeyValuePair<string,bool>("Native page activation: "+name,navigated&&holder.Controls.Count>0&&Object.ReferenceEquals(holder.Controls[0],NativeContract.NodeSection(node))));
                    }
                    search.Text="Enable skin sound";checks.Add(new KeyValuePair<string,bool>("Search includes native checked-list options",results.Items.Cast<SearchResult>().Any(r=>r.Node!=null)));search.Clear();
                    TreeNode bdZoom=Flatten(tree.Nodes).FirstOrDefault(n=>NativeContract.NodeSection(n)!=null&&NativeContract.NodeSection(n).GetType().Name=="BDZoom");
                    bool bdChoicePreserved=false;
                    if(bdZoom!=null&&Navigate(bdZoom))
                    {
                        var choice=Descendants(NativeContract.NodeSection(bdZoom)).OfType<ComboBox>().First(c=>c.Name=="defaultZoomModeComboBox");int originalIndex=choice.SelectedIndex;
                        if(choice.Items.Count>1){int selected=(originalIndex+1)%choice.Items.Count;choice.SelectedIndex=selected;Navigate(Flatten(tree.Nodes).First(n=>NativeContract.NodeSection(n)!=null&&NativeContract.NodeSection(n).GetType().Name=="General"));Navigate(bdZoom);bdChoicePreserved=choice.SelectedIndex==selected;choice.SelectedIndex=originalIndex;}
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
    }
}
#endif

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;

namespace MP1.ConfigurationStudio
{
    public sealed partial class StudioShell
    {
        public void StartNativeSaveCycle(string path)
        {
            string runtime=AppDomain.CurrentDomain.BaseDirectory,rootPath=Path.GetFullPath(Path.Combine(runtime,"..")).TrimEnd('\\')+"\\";
            if(!File.Exists(Path.Combine(runtime,"studio-test-runtime.marker")))throw new InvalidOperationException("OK cycle requires an isolated runtime.");
            Type config=Assembly.Load("Utils").GetType("MediaPortal.Configuration.Config"),dir=config.GetNestedType("Dir");
            foreach(string name in new[]{"Config","Log","Database","Thumbs","Cache","Skin","Language"})
            {string folder=Path.GetFullPath(Convert.ToString(config.GetMethod("GetFolder").Invoke(null,new[]{Enum.Parse(dir,name)})));if(!folder.StartsWith(rootPath,StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("Non-isolated OK cycle path: "+name);}
            if(!Path.GetFullPath(IsolatedRuntime.DebugMarkerPath()).StartsWith(rootPath,StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("Non-isolated OK cycle debug marker.");
            var report=new XmlDocument();bool restart=false;
            if(File.Exists(path)){report.Load(path);restart=report.DocumentElement.GetAttribute("stage")=="saved";}
            if(!restart){report.RemoveAll();report.AppendChild(report.CreateElement("nativeOkCycle"));}
            var root=report.DocumentElement;var timer=new Timer {Interval=500};
            timer.Tick+=delegate{
                timer.Stop();timer.Dispose();
                try
                {
                    TreeNode node=Flatten(tree.Nodes).First(n=>NodeSection(n).GetType().Name=="General");Navigate(node);Control page=NodeSection(node);
                    var field=Descendants(page).OfType<CheckBox>().First(c=>c.Name=="checkBoxAutoRestart");
                    if(restart)
                    {
                        bool original=Boolean.Parse(root.GetAttribute("original")),expected=Boolean.Parse(root.GetAttribute("expected"));
                        root.SetAttribute("freshProcessLoadedSavedValue",(field.Checked==expected).ToString());field.Checked=original;page.GetType().GetMethod("SaveSettings",Type.EmptyTypes).Invoke(page,null);FlushNativeProfile(false);FlushNativeProfile(true);
                        root.SetAttribute("originalRestored",(ReadRestartOption(rootPath)==original).ToString());root.SetAttribute("stage","complete");root.SetAttribute("complete","true");SaveReview(report,path);nativeCancel.PerformClick();
                    }
                    else
                    {
                        root.SetAttribute("original",field.Checked.ToString());root.SetAttribute("expected",(!field.Checked).ToString());root.SetAttribute("stage","awaiting original OK");field.Checked=!field.Checked;form.Text="MP1 Configuration Studio · ISOLIERTER OK-SPEICHERTEST";SaveReview(report,path);
                        form.FormClosed+=delegate{root.SetAttribute("savedValueMatches",(ReadRestartOption(rootPath)==Boolean.Parse(root.GetAttribute("expected"))).ToString());root.SetAttribute("stage","saved");SaveReview(report,path);};
                        nativeOk.PerformClick();
                    }
                }
                catch(Exception ex){root.SetAttribute("error",ex.GetBaseException().Message);root.SetAttribute("complete","true");SaveReview(report,path);nativeCancel.PerformClick();}
            };
            timer.Start();
        }
        private static bool ReadRestartOption(string root)
        {
            var profile=new XmlDocument();profile.Load(Path.Combine(root,"config","MediaPortal.xml"));XmlNode value=profile.SelectSingleNode("/profile/section[@name='general']/entry[@name='restartOnError']");
            return value!=null&&(value.InnerText.Equals("yes",StringComparison.OrdinalIgnoreCase)||value.InnerText.Equals("true",StringComparison.OrdinalIgnoreCase)||value.InnerText=="1");
        }
    }
}

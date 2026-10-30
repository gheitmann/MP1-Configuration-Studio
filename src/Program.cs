using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using System.Security.Principal;
using System.Windows.Forms;
using Microsoft.Win32;

[assembly: AssemblyTitle("MP1 Configuration Studio")]
[assembly: AssemblyDescription("Presentation layer for the original MediaPortal 1.39 configurator")]
[assembly: AssemblyVersion("1.39.0.0")]
[assembly: AssemblyFileVersion("1.39.0.14")]

namespace MP1.ConfigurationStudio
{
    internal static class Program
    {
        [STAThread]
        public static int Main(string[] args)
        {
            try
            {
                if (Array.IndexOf(args, "--self-test") >= 0) return ShellTests.Run();
                if (Array.IndexOf(args, "--design-preview") >= 0) return ShellTests.Preview();
                string runtime = null, preferences = null, audit = null, verify = null, review = null, persistence = null, saveCycle = null;
                var forwarded = new List<string>();
                foreach (string arg in args)
                {
                    if (arg.StartsWith("--runtime=", StringComparison.OrdinalIgnoreCase)) runtime = arg.Substring(10).Trim('"');
                    else if (arg.StartsWith("--preferences=", StringComparison.OrdinalIgnoreCase)) preferences = arg.Substring(14).Trim('"');
                    else if (arg.StartsWith("--audit=", StringComparison.OrdinalIgnoreCase)) audit = arg.Substring(8).Trim('"');
                    else if (arg.StartsWith("--save-test=", StringComparison.OrdinalIgnoreCase)) saveCycle = arg.Substring(12).Trim('"');
                    else if (arg.StartsWith("--persistence=", StringComparison.OrdinalIgnoreCase)) persistence = arg.Substring(14).Trim('"');
                    else if (arg.StartsWith("--review=", StringComparison.OrdinalIgnoreCase)) review = arg.Substring(9).Trim('"');
                    else if (arg.StartsWith("--verify=", StringComparison.OrdinalIgnoreCase)) verify = arg.Substring(9).Trim('"');
                    else forwarded.Add(arg);
                }
                runtime = FindRuntime(runtime);
                string native = Path.Combine(runtime, "Configuration.exe");
                var version = FileVersionInfo.GetVersionInfo(native);
                if (version.FileMajorPart != 1 || version.FileMinorPart != 39)
                    throw new InvalidOperationException("Diese Ausgabe unterstützt MediaPortal 1.39. Gefunden: " + version.FileVersion + ". Das Original wurde nicht verändert.");
                var architecture=AssemblyName.GetAssemblyName(Path.Combine(runtime,"Core.dll")).ProcessorArchitecture;
                bool requires32=architecture==ProcessorArchitecture.X86,requires64=architecture==ProcessorArchitecture.Amd64;
                if((requires32&&Environment.Is64BitProcess)||(requires64&&!Environment.Is64BitProcess))
                {
                    string sibling=Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),"MP1ConfigurationStudio."+(requires32?"x86":"x64")+".exe");
                    if(!File.Exists(sibling))throw new FileNotFoundException("Der passende "+(requires32?"32":"64")+"-Bit-Starter fehlt. Bitte das vollständige Paket entpacken.",sibling);
                    var redirected=new List<string>(args);redirected.Add("--runtime="+runtime);
                    Process.Start(new ProcessStartInfo(sibling,String.Join(" ",redirected.Select(Quote))) {UseShellExecute=true,WorkingDirectory=Path.GetDirectoryName(sibling)});return 0;
                }
                // Native plugins also use WinForms Application.StartupPath, which is process-based.
                // Run a separate presentation executable beside the original, leaving it untouched.
                // An AppDomain alone cannot preserve this path for BASS and external plugin tools.
                if (!String.Equals(Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory).TrimEnd('\\'), runtime.TrimEnd('\\'), StringComparison.OrdinalIgnoreCase))
                    return LaunchBesideNative(runtime, args);
                if (preferences == null)
                    preferences = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MP1 Configuration Studio", "appearance.xml");
                var setup = new AppDomainSetup { ApplicationBase = runtime, ConfigurationFile = native + ".config", PrivateBinPath = @"plugins\ExternalPlayers;plugins\process;plugins\subtitle;plugins\windows" };
                var domain = AppDomain.CreateDomain("MediaPortal Configuration Studio", null, setup);
                NativeBridge bridge=null;
                try
                {
                    bridge = (NativeBridge)domain.CreateInstanceFromAndUnwrap(Assembly.GetExecutingAssembly().Location, typeof(NativeBridge).FullName);
                    bridge.Prepare(preferences, audit, verify, review, persistence, saveCycle);
                    Directory.SetCurrentDirectory(runtime);
                    return domain.ExecuteAssembly(native, forwarded.ToArray());
                }
                finally {try{if(bridge!=null)bridge.Stop();}finally{AppDomain.Unload(domain);} }
            }
            catch (Exception ex)
            {
                while (ex is TargetInvocationException && ex.InnerException != null) ex = ex.InnerException;
                string error = "Configuration Studio konnte nicht gestartet werden.\r\n\r\n" + ex.Message + "\r\n\r\nDer originale Konfigurator bleibt über Configuration.exe erreichbar.";
                if (Array.IndexOf(args, "--self-test") >= 0) Console.Error.WriteLine(ex);
                else MessageBox.Show(error, "MP1 Configuration Studio", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 1;
            }
        }

        private static int LaunchBesideNative(string runtime, string[] args)
        {
            string source = Assembly.GetExecutingAssembly().Location;
            string target = Path.Combine(runtime, RunnerFileName(source));
            var launchArguments=new List<string>(args);launchArguments.Add("--runtime="+runtime);
            try
            {
                bool different = !File.Exists(target) || !Hash(source).SequenceEqual(Hash(target));
                if (different)
                {
                    if (File.Exists(target)) File.Copy(target,target+".previous",true);
                    File.Copy(source,target,true);
                }
                string nativeConfig = Path.Combine(runtime,"Configuration.exe.config");
                if (File.Exists(nativeConfig)) File.Copy(nativeConfig,target+".config",true);
                Process.Start(new ProcessStartInfo(target,String.Join(" ",launchArguments.Select(Quote))) { WorkingDirectory=runtime,UseShellExecute=true });
                return 0;
            }
            catch (UnauthorizedAccessException)
            {
                using(var identity=WindowsIdentity.GetCurrent())
                    if(new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator))throw;
                // Normal Windows elevation request; only these additional Studio files are installed.
                var elevated = new ProcessStartInfo(source,String.Join(" ",launchArguments.Select(Quote))) { UseShellExecute=true,Verb="runas",WorkingDirectory=Path.GetDirectoryName(source) };
                Process.Start(elevated);return 0;
            }
        }
        internal static string RunnerFileName(string source)
        {
            string suffix=source.EndsWith(".x86.exe",StringComparison.OrdinalIgnoreCase)?".x86":source.EndsWith(".x64.exe",StringComparison.OrdinalIgnoreCase)?".x64":"";
            return "MP1ConfigurationStudio"+suffix+".exe";
        }
        private static byte[] Hash(string path){using(var sha=SHA256.Create())using(var stream=File.OpenRead(path))return sha.ComputeHash(stream);}
        private static string Quote(string value)
        {
            if(value.Length>0&&value.IndexOfAny(new[]{' ','\t','"'})<0)return value;
            var text=new StringBuilder("\"");int slashes=0;
            foreach(char c in value){if(c=='\\'){slashes++;continue;}if(c=='"'){text.Append('\\',slashes*2+1).Append(c);slashes=0;continue;}text.Append('\\',slashes).Append(c);slashes=0;}
            text.Append('\\',slashes*2).Append('"');return text.ToString();
        }

        private static string FindRuntime(string requested)
        {
            if (!String.IsNullOrEmpty(requested))
            {
                string full = Path.GetFullPath(requested);
                if (!File.Exists(Path.Combine(full, "Configuration.exe"))) throw new FileNotFoundException("Configuration.exe fehlt in " + full);
                return full;
            }
            var candidates = new List<string>();
            candidates.Add(AppDomain.CurrentDomain.BaseDirectory);
            foreach (RegistryView view in new[] { RegistryView.Registry64, RegistryView.Registry32 })
            {
                try
                {
                    using (var hive = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view))
                    using (var key = hive.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\MediaPortal"))
                        if (key != null) candidates.Add(Convert.ToString(key.GetValue("InstallPath", key.GetValue("InstallLocation", ""))));
                }
                catch (System.Security.SecurityException) { }
            }
            candidates.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Team MediaPortal\MediaPortal"));
            candidates.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Team MediaPortal\MediaPortal"));
            foreach (string path in candidates)
                if (!String.IsNullOrWhiteSpace(path) && File.Exists(Path.Combine(path, "Configuration.exe"))) return Path.GetFullPath(path);
            using (var picker = new FolderBrowserDialog { Description = "Installationsordner von MediaPortal 1.39 auswählen", ShowNewFolderButton = false })
                if (picker.ShowDialog() == DialogResult.OK && File.Exists(Path.Combine(picker.SelectedPath, "Configuration.exe"))) return picker.SelectedPath;
            throw new DirectoryNotFoundException("Keine MediaPortal-1.39-Installation ausgewählt.");
        }
    }

    public sealed class NativeBridge : MarshalByRefObject
    {
        private readonly HashSet<Form> seen = new HashSet<Form>();
        private string preferences, audit, verify, review, persistence, saveCycle;
        private StudioShell shell;
        private DialogAppearanceObserver dialogObserver;
        public override object InitializeLifetimeService() { return null; }

        public void Prepare(string preferencesPath, string auditPath, string verifyPath, string reviewPath, string persistencePath, string saveCyclePath)
        {
            IsolatedRuntime.RedirectDebugMarkers();
            preferences = preferencesPath; audit = auditPath;verify=verifyPath;review=reviewPath;persistence=persistencePath;saveCycle=saveCyclePath;
            // Observe the very first show request, not the first Idle after a
            // completed native frame has already reached the display.
            dialogObserver=new DialogAppearanceObserver(PrepareForm);
            Application.Idle += Observe;
        }
        public void Stop(){Application.Idle-=Observe;if(dialogObserver!=null){dialogObserver.Dispose();dialogObserver=null;}}

        private void PrepareForm(Form form)
        {
                if(form.IsDisposed||seen.Contains(form))return;
                FrameRendering.EnableComposition(form);
                if (form.GetType().FullName == "MediaPortal.Configuration.SettingsForm")
                {
                    seen.Add(form);
                    try
                    {
                        shell = StudioShell.Attach(form, preferences);
                        Action ready=delegate{
                            if (!String.IsNullOrEmpty(audit)) shell.StartAudit(audit);
                            if (!String.IsNullOrEmpty(verify)) shell.StartNativeChecks(verify);
                            else if (!String.IsNullOrEmpty(review)) shell.StartFullReview(review);
                            else if (!String.IsNullOrEmpty(persistence)) shell.StartPersistenceReview(persistence);
                            else if (!String.IsNullOrEmpty(saveCycle)) shell.StartNativeSaveCycle(saveCycle);
                        };
                        if(FrameRendering.IsVisible(form))ready();else form.Shown+=delegate{ready();};
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(form, "Die neue Oberfläche konnte nicht initialisiert werden. Bitte dieses Fenster schließen und Configuration.exe verwenden.\r\n\r\n" + ex.Message,
                            "Configuration Studio", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else if (shell != null)
                {
                    seen.Add(form);
                    shell.ThemeDialog(form);
                }
        }
        private void Observe(object sender, EventArgs e)
        {
            var forms = new List<Form>();
            foreach (Form form in Application.OpenForms) forms.Add(form);
            foreach(Form form in forms)
            {
                if(!form.IsDisposed&&form.Visible&&!seen.Contains(form))PrepareForm(form);
            }
        }
    }
}

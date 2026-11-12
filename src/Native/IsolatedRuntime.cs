using System;
using System.IO;

namespace MP1.ConfigurationStudio
{
    internal static class IsolatedRuntime
    {
        public static void RedirectDebugMarkers()
        {
            string runtime=AppDomain.CurrentDomain.BaseDirectory;
            if(!File.Exists(Path.Combine(runtime,"studio-test-runtime.marker")))return;
            string isolated=Path.GetFullPath(Path.Combine(runtime,"..","debug"));
            Directory.CreateDirectory(isolated);
            string original=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),"Team MediaPortal","MediaPortal","debug");
            if(Directory.Exists(original))
            {
                foreach(string file in Directory.GetFiles(original,"*.txt"))
                {
                    File.Copy(file,Path.Combine(isolated,Path.GetFileName(file)),true);
                }
            }
            NativeContract.SetDebugSettingsPath(isolated+Path.DirectorySeparatorChar);
        }

        public static string DebugMarkerPath()
        {
            return NativeContract.DebugSettingsPath();
        }
    }
}

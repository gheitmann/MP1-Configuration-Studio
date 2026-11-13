using System;
using System.Runtime.InteropServices;

namespace MP1.ConfigurationStudio
{
    internal static class NativeDarkMode
    {
        [DllImport("uxtheme.dll", EntryPoint = "#135", SetLastError = true)]
        private static extern int SetPreferredAppMode(int mode);
        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        private static extern int SetWindowTheme(IntPtr hWnd, string subAppName, string subIdList);
        [DllImport("uxtheme.dll", EntryPoint = "#133", SetLastError = true)]
        private static extern bool AllowDarkModeForWindow(IntPtr hWnd, bool allow);
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hWnd, int attribute, ref int value, int size);

        /// <summary>Opts this process into the Windows dark theme so native scrollbars
        /// and dropdowns render dark instead of the default light system colors.</summary>
        public static void Enable()
        {
            try { SetPreferredAppMode(2); }
            catch (DllNotFoundException) { }
            catch (EntryPointNotFoundException) { }
        }

        public static void Apply(IntPtr handle)
        {
            if (handle == IntPtr.Zero) return;
            try
            {
                AllowDarkModeForWindow(handle, true);
                SetWindowTheme(handle, "DarkMode_Explorer", null);
            }
            catch (DllNotFoundException) { }
            catch (EntryPointNotFoundException) { }
        }

        public static void ApplyWindow(IntPtr handle, bool dark)
        {
            if (handle == IntPtr.Zero) return;
            Apply(handle);
            try
            {
                int value = dark ? 1 : 0;
                DwmSetWindowAttribute(handle, 20, ref value, 4);
            }
            catch (DllNotFoundException) { }
            catch (EntryPointNotFoundException) { }
        }
    }
}

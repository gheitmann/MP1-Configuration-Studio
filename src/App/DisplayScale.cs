using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace MP1.ConfigurationStudio
{
    internal static class DisplayScale
    {
        [DllImport("user32.dll")]
        private static extern uint GetDpiForSystem();
        [DllImport("user32.dll")]
        private static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr window,uint flags);
        [DllImport("shcore.dll")]
        private static extern int GetDpiForMonitor(IntPtr monitor,int dpiType,out uint dpiX,out uint dpiY);

        public static int SystemDpi()
        {
            try
            {
                uint x,y;
                IntPtr monitor=MonitorFromWindow(GetDesktopWindow(),1);
                if(monitor!=IntPtr.Zero&&GetDpiForMonitor(monitor,0,out x,out y)==0&&x>=96&&x<=960)return (int)x;
            }
            catch(Exception)
            {
            }
            try
            {
                int dpi=(int)GetDpiForSystem();
                if(dpi>=96&&dpi<=960)return dpi;
            }
            catch(Exception)
            {
            }
            try
            {
                using(var graphics=Graphics.FromHwnd(IntPtr.Zero))
                    return (int)Math.Round(graphics.DpiX);
            }
            catch(Exception)
            {
                return 96;
            }
        }

        public static bool ShouldOffer(int previous,int current,bool alreadyOffered)
        {
            return !alreadyOffered&&previous>0&&current>0&&current!=previous;
        }
    }
}

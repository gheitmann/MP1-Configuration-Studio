using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;

namespace MP1.ConfigurationStudio
{
    internal static class NativeFilterProperties
    {
        private static readonly Guid IID_IBaseFilter = new Guid("56a86895-0ad4-11ce-b03a-0020af0ba770");

        [StructLayout(LayoutKind.Sequential)]
        private struct DsCAUUID
        {
            public int cElems;
            public IntPtr pElems;
        }

        [ComImport, Guid("B196B28B-BAB4-101A-B69C-00AA00341D07"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface ISpecifyPropertyPages
        {
            int GetPages(out DsCAUUID pages);
        }

        [DllImport("oleaut32.dll", CharSet = CharSet.Unicode)]
        private static extern int OleCreatePropertyFrame(IntPtr owner, int x, int y, string caption, int objects,
            [MarshalAs(UnmanagedType.Interface)] ref object unknown, int pageCount, IntPtr pageClsIds,
            int locale, int reserved, IntPtr user);

        public static void Show(IWin32Window owner, NativeFilterKind kind, string filterName)
        {
            try
            {
                object device = NativeFilterCatalog.Device(kind, filterName);
                StudioLog.Info("Filter properties: kind=" + kind + " name=" + (filterName ?? "(null)") + " found=" + (device != null) + " available=" + NativeFilterCatalog.Names(kind).Count);
                if (device == null)
                {
                    MessageBox.Show(owner, Localization.IsEnglish ? "This filter is not registered on this system." : "Dieser Filter ist auf diesem System nicht registriert.", "MP1 Configuration Studio", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                PropertyInfo monikerProperty = device.GetType().GetProperty("Mon");
                IMoniker moniker = monikerProperty == null ? null : monikerProperty.GetValue(device, null) as IMoniker;
                if (moniker == null)
                {
                    MessageBox.Show(owner, Localization.IsEnglish ? "No property page is available for this filter." : "Für diesen Filter ist keine Eigenschaftsseite verfügbar.", "MP1 Configuration Studio", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                object filter;
                Guid iid = IID_IBaseFilter;
                moniker.BindToObject(null, null, ref iid, out filter);
                ISpecifyPropertyPages pages = filter as ISpecifyPropertyPages;
                if (pages == null)
                {
                    MessageBox.Show(owner, Localization.IsEnglish ? "No property page is available for this filter." : "Für diesen Filter ist keine Eigenschaftsseite verfügbar.", "MP1 Configuration Studio", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                DsCAUUID guids;
                int result = pages.GetPages(out guids);
                if (result != 0 || guids.cElems == 0)
                {
                    MessageBox.Show(owner, Localization.IsEnglish ? "No property page is available for this filter." : "Für diesen Filter ist keine Eigenschaftsseite verfügbar.", "MP1 Configuration Studio", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                try
                {
                    result = OleCreatePropertyFrame(owner.Handle, 30, 30, filterName, 1, ref filter, guids.cElems, guids.pElems, 0, 0, IntPtr.Zero);
                }
                finally
                {
                    if (guids.pElems != IntPtr.Zero) Marshal.FreeCoTaskMem(guids.pElems);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(owner, (Localization.IsEnglish ? "The filter configuration could not be opened.\r\n\r\n" : "Die Filterkonfiguration konnte nicht geöffnet werden.\r\n\r\n") + ex.Message, "MP1 Configuration Studio", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}

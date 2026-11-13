using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace MP1.ConfigurationStudio
{
    internal static class Preflight
    {
        public static bool ConfirmMediaPortalClosed()
        {
            try
            {
                Process[] mediaPortal = Process.GetProcessesByName("MediaPortal");
                Process[] configuration = Process.GetProcessesByName("Configuration");
                int count = mediaPortal.Length + configuration.Length;
                foreach (Process process in mediaPortal)
                {
                    process.Dispose();
                }
                foreach (Process process in configuration)
                {
                    process.Dispose();
                }

                if (count == 0)
                {
                    return true;
                }

                StudioLog.Warn("MediaPortal or Configuration process is still running; asking the user.");
                DialogResult answer = MessageBox.Show(
                    Localization.IsEnglish
                        ? "MediaPortal or the original configurator is still running.\r\n\r\n"
                            + "Please close both applications before changing the configuration so that no files are written concurrently.\r\n\r\n"
                            + "Continue anyway?"
                        : "MediaPortal oder der originale Konfigurator läuft noch.\r\n\r\n"
                            + "Bitte schließen Sie beide Anwendungen, bevor Sie die Konfiguration ändern, damit keine Dateien gleichzeitig geschrieben werden.\r\n\r\n"
                            + "Trotzdem fortfahren?",
                    "MP1 Configuration Studio",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2);
                return answer == DialogResult.Yes;
            }
            catch (Exception ex)
            {
                StudioLog.Warn("Preflight process check failed: " + ex.Message);
                return true;
            }
        }
    }
}

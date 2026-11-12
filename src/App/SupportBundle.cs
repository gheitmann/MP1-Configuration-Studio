using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MP1.ConfigurationStudio
{
    internal static class SupportBundle
    {
        public static string Create(string targetPath, string appearancePath = null, IEnumerable<Control> sections = null)
        {
            if (String.IsNullOrWhiteSpace(targetPath))
            {
                throw new ArgumentException("Ein Zielpfad für das Support-Bundle fehlt.", "targetPath");
            }

            try
            {
                targetPath = Path.GetFullPath(targetPath);
                string directory = Path.GetDirectoryName(targetPath);
                if (!String.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                if (File.Exists(targetPath))
                {
                    File.Delete(targetPath);
                }

                using (ZipArchive archive = ZipFile.Open(targetPath, ZipArchiveMode.Create))
                {
                    WriteText(archive, "README.txt", ReadmeText());
                    WriteText(archive, "version.txt", VersionText());
                    WriteText(archive, "environment.txt", EnvironmentText());
                    WriteText(archive, "compatibility.txt", CompatibilityText());
                    if (!String.IsNullOrEmpty(appearancePath) && File.Exists(appearancePath))
                    {
                        WriteFile(archive, "appearance.xml", appearancePath);
                    }
                    if (sections != null)
                    {
                        WriteText(archive, "sections.txt", SectionsText(sections));
                    }
                    WriteLogs(archive);
                }

                StudioLog.Info("Support bundle created: " + targetPath);
                return targetPath;
            }
            catch (Exception ex)
            {
                StudioLog.Error(ex, "Support bundle creation failed");
                return null;
            }
        }

        private static string ReadmeText()
        {
            return "MP1 Configuration Studio – Support-Bundle\r\n"
                + "\r\n"
                + "Inhalt:\r\n"
                + "- version.txt: Version, Sitzung und Build\r\n"
                + "- environment.txt: Windows-, .NET-, Anzeige- und MediaPortal-Umgebung\r\n"
                + "- compatibility.txt: Abschnittstypen und Kompatibilitätsstufen\r\n"
                + "- sections.txt: geladene Abschnittstypen und Titel\r\n"
                + "- appearance.xml: Darstellungspräferenzen (Theme, Größe, Skalierung)\r\n"
                + "- logs/: Studio-Protokolle\r\n"
                + "\r\n"
                + "Nicht enthalten: MediaPortal.xml, Plugin-Konfigurationen, Einstellungswerte, Zugangsdaten oder Medienpfade.\r\n"
                + "Das Protokoll wird vor dem Schreiben von Token- und Passwortmustern bereinigt.\r\n"
                + "Bitte das Bundle nur im Supportfall weitergeben.\r\n";
        }

        private static string VersionText()
        {
            var builder = new StringBuilder();
            builder.AppendLine("MP1 Configuration Studio");
            builder.AppendLine("FileVersion: " + SafeFileVersion(Application.ExecutablePath));
            builder.AppendLine("ProductVersion: " + Application.ProductVersion);
            builder.AppendLine("Session: " + StudioLog.SessionId);
            builder.AppendLine("Process: " + Path.GetFileName(Application.ExecutablePath));
            builder.AppendLine("BaseDirectory: " + AppDomain.CurrentDomain.BaseDirectory);
            builder.AppendLine("Created: " + DateTime.Now.ToString("o", CultureInfo.InvariantCulture));
            return builder.ToString();
        }

        private static string EnvironmentText()
        {
            var builder = new StringBuilder();
            builder.AppendLine("OS: " + Environment.OSVersion.VersionString);
            builder.AppendLine("OS architecture: " + (Environment.Is64BitOperatingSystem ? "x64" : "x86"));
            builder.AppendLine("Process architecture: " + (Environment.Is64BitProcess ? "x64" : "x86"));
            builder.AppendLine("CLR: " + Environment.Version);
            builder.AppendLine("Culture: " + CultureInfo.CurrentCulture.Name);
            builder.AppendLine("UI culture: " + CultureInfo.CurrentUICulture.Name);
            builder.AppendLine("CurrentDirectory: " + Environment.CurrentDirectory);
            builder.AppendLine("PrimaryScreen: " + SystemInformation.PrimaryMonitorSize.Width + "x" + SystemInformation.PrimaryMonitorSize.Height);

            try
            {
                using (Graphics graphics = Graphics.FromHwnd(IntPtr.Zero))
                {
                    builder.AppendLine("DPI: " + graphics.DpiX.ToString("0.##", CultureInfo.InvariantCulture) + "x" + graphics.DpiY.ToString("0.##", CultureInfo.InvariantCulture));
                }
            }
            catch (Exception)
            {
                builder.AppendLine("DPI: unavailable");
            }

            string native = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configuration.exe");
            if (File.Exists(native))
            {
                builder.AppendLine("MediaPortal Configuration: " + SafeFileVersion(native));
            }
            else
            {
                builder.AppendLine("MediaPortal Configuration: not found in BaseDirectory");
            }

            return builder.ToString();
        }

        private static string CompatibilityText()
        {
            var builder = new StringBuilder();
            builder.AppendLine("Section\tTier\tZoom\tTheme\tSearch");
            foreach (SectionPolicy policy in CompatibilityPolicy.All())
            {
                builder.AppendLine(String.Join("\t", new[]
                {
                    policy.TypeName,
                    policy.Tier.ToString(),
                    policy.Zoom ? "yes" : "no",
                    policy.ThemeMode,
                    policy.Search ? "yes" : "no"
                }));
            }
            builder.AppendLine();
            builder.AppendLine("Unknown section types default to Tier B.");
            return builder.ToString();
        }

        private static string SectionsText(IEnumerable<Control> sections)
        {
            var builder = new StringBuilder();
            builder.AppendLine("Type\tTitle");
            foreach (Control section in sections)
            {
                if (section == null)
                {
                    continue;
                }
                builder.AppendLine((section.GetType().FullName ?? section.GetType().Name) + "\t" + section.Text);
            }
            return builder.ToString();
        }

        private static void WriteLogs(ZipArchive archive)
        {
            string directory = StudioLog.DirectoryPath;
            if (!Directory.Exists(directory))
            {
                return;
            }

            foreach (string file in Directory.GetFiles(directory, "studio*.log").OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
            {
                WriteFile(archive, "logs/" + Path.GetFileName(file), file);
            }
        }

        private static void WriteText(ZipArchive archive, string name, string text)
        {
            ZipArchiveEntry entry = archive.CreateEntry(name, CompressionLevel.Optimal);
            using (var writer = new StreamWriter(entry.Open(), new UTF8Encoding(false)))
            {
                writer.Write(text ?? String.Empty);
            }
        }

        private static void WriteFile(ZipArchive archive, string name, string path)
        {
            ZipArchiveEntry entry = archive.CreateEntry(name, CompressionLevel.Optimal);
            using (var input = File.OpenRead(path))
            using (Stream output = entry.Open())
            {
                input.CopyTo(output);
            }
        }

        private static string SafeFileVersion(string path)
        {
            try
            {
                return System.Diagnostics.FileVersionInfo.GetVersionInfo(path).FileVersion ?? "unknown";
            }
            catch (Exception)
            {
                return "unknown";
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml;

namespace MP1.ConfigurationStudio
{
    internal sealed class BackupInfo
    {
        public string Path { get; private set; }
        public DateTime Created { get; private set; }
        public long Size { get; private set; }
        public bool FirstRun { get; private set; }

        public string Display
        {
            get
            {
                return Created.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)
                    + "  " + (FirstRun ? "First-Run" : "Manuell")
                    + "  " + (Size / 1024).ToString(CultureInfo.InvariantCulture) + " KB";
            }
        }

        public BackupInfo(string path, DateTime created, long size, bool firstRun)
        {
            Path = path;
            Created = created;
            Size = size;
            FirstRun = firstRun;
        }
    }

    internal static class ConfigurationBackup
    {
        public const string MediaPortalFileName = "MediaPortal.xml";

        public static string DefaultBackupDirectory
        {
            get
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "MP1 Configuration Studio",
                    "backups");
            }
        }

        public static string ConfigPath(string configFolder)
        {
            return Path.Combine(configFolder, MediaPortalFileName);
        }

        public static string Create(string mediaPortalPath, string targetPath)
        {
            if (String.IsNullOrWhiteSpace(mediaPortalPath) || !File.Exists(mediaPortalPath))
            {
                throw new FileNotFoundException("MediaPortal.xml wurde nicht gefunden.", mediaPortalPath);
            }
            if (String.IsNullOrWhiteSpace(targetPath))
            {
                throw new ArgumentException("Ein Zielpfad für die Sicherung fehlt.", "targetPath");
            }

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
                ZipArchiveEntry manifest = archive.CreateEntry("backup.txt", CompressionLevel.Optimal);
                using (var writer = new StreamWriter(manifest.Open(), new UTF8Encoding(false)))
                {
                    writer.WriteLine("MP1 Configuration Studio configuration backup");
                    writer.WriteLine("Created: " + DateTime.Now.ToString("o", CultureInfo.InvariantCulture));
                    writer.WriteLine("Contains: " + MediaPortalFileName);
                }

                ZipArchiveEntry entry = archive.CreateEntry(MediaPortalFileName, CompressionLevel.Optimal);
                using (var input = File.OpenRead(mediaPortalPath))
                using (Stream output = entry.Open())
                {
                    input.CopyTo(output);
                }
            }

            StudioLog.Info("Configuration backup created");
            return targetPath;
        }

        public static void Restore(string backupPath, string mediaPortalPath)
        {
            if (String.IsNullOrWhiteSpace(backupPath) || !File.Exists(backupPath))
            {
                throw new FileNotFoundException("Die Sicherungsdatei wurde nicht gefunden.", backupPath);
            }
            if (String.IsNullOrWhiteSpace(mediaPortalPath))
            {
                throw new ArgumentException("Der Zielpfad für MediaPortal.xml fehlt.", "mediaPortalPath");
            }

            byte[] data;
            using (ZipArchive archive = ZipFile.OpenRead(backupPath))
            {
                ZipArchiveEntry entry = archive.GetEntry(MediaPortalFileName);
                if (entry == null)
                {
                    throw new InvalidDataException("Die Sicherung enthält keine MediaPortal.xml.");
                }

                using (var memory = new MemoryStream())
                using (Stream stream = entry.Open())
                {
                    stream.CopyTo(memory);
                    data = memory.ToArray();
                }
            }

            ValidateXml(data);

            string target = Path.GetFullPath(mediaPortalPath);
            string directory = Path.GetDirectoryName(target);
            if (!String.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string temporary = target + ".studio-restore.tmp";
            File.WriteAllBytes(temporary, data);

            string safety = target + ".before-restore-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".xml";
            try
            {
                if (File.Exists(target))
                {
                    File.Copy(target, safety, true);
                }
                File.Copy(temporary, target, true);
            }
            finally
            {
                if (File.Exists(temporary))
                {
                    File.Delete(temporary);
                }
            }

            StudioLog.Info("Configuration restored; safety copy: " + Path.GetFileName(safety));
        }

        public static List<BackupInfo> ListBackups(string backupRoot = null)
        {
            string root = String.IsNullOrWhiteSpace(backupRoot) ? DefaultBackupDirectory : backupRoot;
            var result = new List<BackupInfo>();
            if (!Directory.Exists(root))
            {
                return result;
            }

            foreach (string file in Directory.GetFiles(root, "*.zip"))
            {
                try
                {
                    using (ZipArchive archive = ZipFile.OpenRead(file))
                    {
                        if (archive.GetEntry(MediaPortalFileName) == null)
                        {
                            continue;
                        }
                    }
                    var info = new FileInfo(file);
                    result.Add(new BackupInfo(
                        info.FullName,
                        info.LastWriteTime,
                        info.Length,
                        info.Name.StartsWith("first-run-", StringComparison.OrdinalIgnoreCase)));
                }
                catch (Exception ex)
                {
                    StudioLog.Warn("Backup could not be inspected: " + ex.Message);
                }
            }

            result.Sort(delegate(BackupInfo left, BackupInfo right) { return right.Created.CompareTo(left.Created); });
            return result;
        }

        public static void DeleteBackup(string path)
        {
            if (!String.IsNullOrWhiteSpace(path) && File.Exists(path))
            {
                File.Delete(path);
                StudioLog.Info("Configuration backup deleted: " + Path.GetFileName(path));
            }
        }

        public static string EnsureFirstRunBackup(string configFolder, string backupRoot = null)
        {
            if (String.IsNullOrWhiteSpace(configFolder))
            {
                return null;
            }

            string root = String.IsNullOrWhiteSpace(backupRoot) ? DefaultBackupDirectory : backupRoot;
            string marker = Path.Combine(root, "first-run.done");
            if (File.Exists(marker))
            {
                return null;
            }

            string source = ConfigPath(configFolder);
            if (!File.Exists(source))
            {
                return null;
            }

            Directory.CreateDirectory(root);
            string target = Path.Combine(root, "first-run-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".zip");
            Create(source, target);
            File.WriteAllText(marker, DateTime.Now.ToString("o", CultureInfo.InvariantCulture), new UTF8Encoding(false));
            StudioLog.Info("First-run configuration backup created");
            return target;
        }

        private static void ValidateXml(string path)
        {
            using (XmlReader reader = XmlReader.Create(path, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null }))
            {
                ValidateDocument(reader);
            }
        }

        private static void ValidateXml(byte[] data)
        {
            using (var stream = new MemoryStream(data))
            using (XmlReader reader = XmlReader.Create(stream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null }))
            {
                ValidateDocument(reader);
            }
        }

        private static void ValidateDocument(XmlReader reader)
        {
            var document = new XmlDocument { XmlResolver = null };
            document.Load(reader);
            if (document.DocumentElement == null || document.DocumentElement.Name != "profile")
            {
                throw new InvalidDataException("Die Sicherung enthält keine gültige MediaPortal.xml.");
            }
        }
    }
}

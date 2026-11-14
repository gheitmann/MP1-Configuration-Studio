using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml;

namespace MP1.ConfigurationStudio
{
    internal interface IStudioFileEditor
    {
        bool HasUnsavedChanges { get; }
        bool ConfirmDiscardChanges();
        bool SaveChanges();
    }

    internal static class NativeXmlFileStore
    {
        public static string BackupDirectory
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MP1 Configuration Studio", "backups");
            }
        }

        public static string ReadText(string path, out Encoding encoding)
        {
            byte[] bytes = File.ReadAllBytes(path);
            int offset = 0;
            if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            {
                encoding = new UTF8Encoding(true);
                offset = 3;
            }
            else if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
            {
                encoding = Encoding.Unicode;
                offset = 2;
            }
            else if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
            {
                encoding = Encoding.BigEndianUnicode;
                offset = 2;
            }
            else
            {
                encoding = new UTF8Encoding(false);
            }
            return encoding.GetString(bytes, offset, bytes.Length - offset);
        }

        public static XmlDocument Load(string path)
        {
            var document = new XmlDocument { XmlResolver = null, PreserveWhitespace = true };
            document.Load(path);
            return document;
        }

        public static string SaveText(string path, string text, Encoding encoding)
        {
            var document = new XmlDocument { XmlResolver = null };
            document.LoadXml(text);
            return SaveDocument(path, document, encoding, false);
        }

        public static string SaveDocument(string path, XmlDocument document, Encoding encoding)
        {
            return SaveDocument(path, document, encoding, true);
        }

        private static string SaveDocument(string path, XmlDocument document, Encoding encoding, bool preserveWhitespace)
        {
            if (path == null) throw new ArgumentNullException("path");
            if (document == null) throw new ArgumentNullException("document");
            Directory.CreateDirectory(BackupDirectory);
            string backup = Path.Combine(BackupDirectory, "file-" + Path.GetFileName(path) + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmss-fff") + ".zip");
            using (ZipArchive archive = ZipFile.Open(backup, ZipArchiveMode.Create))
            {
                if (File.Exists(path))
                {
                    ZipArchiveEntry entry = archive.CreateEntry(Path.GetFileName(path), CompressionLevel.Optimal);
                    using (Stream output = entry.Open())
                    using (Stream input = File.OpenRead(path))
                        input.CopyTo(output);
                }
            }
            string temporary = path + ".studio.tmp";
            try
            {
                var settings = new XmlWriterSettings
                {
                    Encoding = encoding,
                    Indent = !preserveWhitespace,
                    OmitXmlDeclaration = false,
                    CloseOutput = true
                };
                using (var writer = XmlWriter.Create(temporary, settings))
                {
                    document.Save(writer);
                }
                if (File.Exists(path)) File.Replace(temporary, path, null);
                else File.Move(temporary, path);
            }
            catch
            {
                try { if (File.Exists(temporary)) File.Delete(temporary); } catch { }
                throw;
            }
            return backup;
        }
    }
}

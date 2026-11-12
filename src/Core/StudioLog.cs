using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace MP1.ConfigurationStudio
{
    internal static class StudioLog
    {
        private static readonly object Sync = new object();
        private static readonly Regex SecretPattern = new Regex(
            @"(ghp_[A-Za-z0-9]{20,}|github_pat_[A-Za-z0-9_]{20,}|gho_[A-Za-z0-9]{20,}|ghs_[A-Za-z0-9]{20,}|ghu_[A-Za-z0-9]{20,}|ghr_[A-Za-z0-9]{20,}|(?i:password|passwort|token)\s*=\s*\S+)",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static string directory;
        private static long maxBytes = 2 * 1024 * 1024;
        private static int maxFiles = 3;

        public static string SessionId { get; private set; }

        public static string DirectoryPath
        {
            get
            {
                EnsureInitialized();
                return directory;
            }
        }

        public static string FilePath
        {
            get { return Path.Combine(DirectoryPath, "studio.log"); }
        }

        public static void Initialize(string path = null)
        {
            string target = path;
            if (String.IsNullOrWhiteSpace(target))
            {
                target = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "MP1 Configuration Studio",
                    "logs");
            }

            target = Path.GetFullPath(target);
            lock (Sync)
            {
                if (String.Equals(directory, target, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                directory = target;
                SessionId = Guid.NewGuid().ToString("N").Substring(0, 12);
                try
                {
                    Directory.CreateDirectory(directory);
                }
                catch (Exception)
                {
                    // Logging must never prevent the application from starting.
                }
            }
        }

        private static void EnsureInitialized()
        {
            if (String.IsNullOrEmpty(directory))
            {
                Initialize();
            }
        }

        public static void Info(string message)
        {
            Write("INFO", message, null);
        }

        public static void Warn(string message)
        {
            Write("WARN", message, null);
        }

        public static void Error(string message)
        {
            Write("ERROR", message, null);
        }

        public static void Error(string message, Exception exception)
        {
            Write("ERROR", message, exception);
        }

        public static void Error(Exception exception, string context)
        {
            Write("ERROR", context, exception);
        }

        public static string Redact(string text)
        {
            if (String.IsNullOrEmpty(text))
            {
                return text;
            }
            return SecretPattern.Replace(text, "***");
        }

        private static void Write(string level, string message, Exception exception)
        {
            EnsureInitialized();

            string line = String.Format(
                CultureInfo.InvariantCulture,
                "[{0:yyyy-MM-dd HH:mm:ss.fff}] [{1}] [{2}] [{3}] {4}{5}",
                DateTime.Now,
                level,
                SessionId,
                Thread.CurrentThread.ManagedThreadId,
                Redact(message ?? String.Empty),
                exception == null ? String.Empty : Environment.NewLine + Redact(exception.ToString()));

            lock (Sync)
            {
                try
                {
                    Directory.CreateDirectory(directory);
                    Rotate();
                    File.AppendAllText(Path.Combine(directory, "studio.log"), line + Environment.NewLine, new UTF8Encoding(false));
                }
                catch (Exception)
                {
                    // Logging failures are never fatal.
                }
            }
        }

        private static void Rotate()
        {
            string current = Path.Combine(directory, "studio.log");
            if (!File.Exists(current) || new FileInfo(current).Length < maxBytes)
            {
                return;
            }

            for (int index = maxFiles; index >= 1; index--)
            {
                string target = Path.Combine(directory, "studio." + index + ".log");
                if (File.Exists(target))
                {
                    File.Delete(target);
                }

                string source = index == 1
                    ? current
                    : Path.Combine(directory, "studio." + (index - 1) + ".log");
                if (File.Exists(source))
                {
                    File.Move(source, target);
                }
            }
        }
    }
}

using System;
using System.Threading;

namespace MP1.ConfigurationStudio
{
    internal static class SingleInstance
    {
        private static Mutex mutex;

        public static bool TryAcquire(string name)
        {
            if (mutex != null)
            {
                return false;
            }

            bool created;
            var candidate = new Mutex(true, name, out created);
            if (!created)
            {
                candidate.Dispose();
                return false;
            }

            mutex = candidate;
            return true;
        }

        public static void Release()
        {
            if (mutex == null)
            {
                return;
            }

            try
            {
                mutex.ReleaseMutex();
            }
            catch (ApplicationException)
            {
                // The mutex was not owned by this thread; disposal is still safe.
            }
            finally
            {
                mutex.Dispose();
                mutex = null;
            }
        }
    }
}

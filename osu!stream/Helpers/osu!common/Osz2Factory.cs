using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace osu_common.Libraries.Osz2
{
    public static class Osz2Factory
    {
        private static List<MapPackage> openPackages = new List<MapPackage>();
        private static List<AutoResetEvent> Waithandles = new List<AutoResetEvent>();
        private static object locker = new object();



        public static MapPackage TryOpen(string path)
        {
            return TryOpen(path, false, false);
        }

        public static MapPackage TryOpen(string path, bool createIfNotFound)
        {
            return TryOpen(path, createIfNotFound, false);
        }

        // returns null if already openened or writelocked
        public static MapPackage TryOpen (string path, bool createIfNotFound, bool metadataOnly)
        {
            lock (locker)
            {
                string absolute = Path.GetFullPath(path).ToLower();
                MapPackage package = openPackages.Find(s => s.Filename.Equals(absolute));

                if (package == null)
                {
                    try
                    {
                        package = new MapPackage(absolute,null, createIfNotFound, metadataOnly);
                    }
#if DEBUG
                    catch (ExecutionEngineException)
#else
                    catch (Exception)
#endif
                    {
                        return null;
                    }

                    openPackages.Add(package);
                }

                //we now use the locker built into the mappackage
                //lockInternal(package);
                return package;
            }
        }

        public static AutoResetEvent TryGetWaitHandle(string path)
        {
            lock (locker)
            {
                string absolute = Path.GetFullPath(path).ToLower();
                int index = openPackages.FindIndex(s => s.Filename.Equals(absolute));
                if (index == -1)
                    return null;

                return Waithandles[index];
            }
        }

        public static void CloseMapPackage (MapPackage package)
        {
            lock (locker)
            {
                int index = openPackages.IndexOf(package);

                package.Close();

                openPackages.Remove(package);

                if (index == -1)
                    return;
            }
        }

        //no mappackage involved, just for other file operations
        //public static void ManualLock(string path)
        //{
        //    if (TryGetWaitHandle(path)!=null)
        //        throw new Exception("Cannot Lock when already opened");

        //    string absolute = Path.GetFullPath(path).ToLower();
        //    lockInternal(absolute, null);
        //}

        public static void ManualUnlock(string path)
        {

            string absolute = Path.GetFullPath(path).ToLower();
            int index = openPackages.FindIndex(s => s.Filename.Equals(absolute));
            if (index == -1)
                throw new Exception("Cannot unlock as it's not locked");
            if (openPackages[index] != null)
                throw new Exception("Cannot unlock a mappackage manually");

            unlockInternal(index);
        }

        private static void unlockInternal(int index)
        {
            Waithandles[index].Set();
            openPackages.RemoveAt(index);
            Waithandles.RemoveAt(index);
        }

        private static void lockInternal(MapPackage package)
        {
            openPackages.Add(package);
            Waithandles.Add(new AutoResetEvent(false));
        }
    }
}

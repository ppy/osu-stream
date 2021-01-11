//TODO: change this class to just be a static. singleton is pointless.

using System.IO;

namespace osum.AssetManager
{
    /// <summary>
    /// AssetManagers abstract the file IO to manage assets in a multi-platform environment.
    /// Assets are skins, hitsounds, textures that come with the game.
    /// These are, depending on the platform, located in the executable itself.
    /// Maps are not included as assets to prevent oversized executables.
    /// This base implementation of this class uses normal file IO.
    /// </summary>
    public class NativeAssetManager
    {
        internal static NativeAssetManager Instance { get; private set; }

        public NativeAssetManager()
        {
            //if (Instance != null)
            //    throw new Exception("singleton");

            Instance = this;
        }

        internal virtual bool FileExists(string filename)
        {
            return File.Exists("/sdcard/" + filename);
        }

        internal virtual Stream GetFileStream(string filename)
        {
            return File.OpenRead("/sdcard/" + filename);
        }

        internal virtual byte[] GetFileBytes(string filename)
        {
            return File.ReadAllBytes("/sdcard/" + filename);
        }
    }
}
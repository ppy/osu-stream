using System.IO;

namespace osum.AssetManager
{
    // this is all TEMPORARY.
    internal class NativeAssetManagerAndroid : NativeAssetManager
    {
        public NativeAssetManagerAndroid() : base()
        { }

        internal override bool FileExists(string filename)
        {
            return base.FileExists("/sdcard/" + filename);
        }

        internal override Stream GetFileStream(string filename)
        {
            return base.GetFileStream("/sdcard/" + filename);
        }

        internal override byte[] GetFileBytes(string filename)
        {
            return base.GetFileBytes("/sdcard/" + filename);
        }
    }
}
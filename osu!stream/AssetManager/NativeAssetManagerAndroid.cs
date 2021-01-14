using System.IO;
using System.Linq;

namespace osum.AssetManager
{
    // this is all TEMPORARY.
    internal class NativeAssetManagerAndroid : NativeAssetManager
    {
        public static Android.Content.Res.AssetManager manager;

        public NativeAssetManagerAndroid() : base()
        { }

        internal override bool FileExists(string filename)
        {
            return manager.List(Path.GetDirectoryName(filename))
                .Any(s => s == Path.GetFileName(filename));
        }

        internal override Stream GetFileStream(string filename)
        {
            return manager.Open(filename);
        }

        private byte[] GetStreamBytes(Stream stream)
        {
            byte[] buffer = new byte[16 * 1024];

            using (MemoryStream ms = new MemoryStream())
            {
                int read = 0;

                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }

                return ms.ToArray();
            }
        }

        internal override byte[] GetFileBytes(string filename)
        {
            return GetStreamBytes(GetFileStream(filename));
        }
    }
}
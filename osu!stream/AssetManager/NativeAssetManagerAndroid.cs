using System.IO;
using System.Linq;

namespace osum.AssetManager
{
    internal class NativeAssetManagerAndroid : NativeAssetManager
    {
        public static Android.Content.Res.AssetManager Manager;

        internal override bool FileExists(string filename)
        {
            return Manager.List(Path.GetDirectoryName(filename))
                .Any(s => s == Path.GetFileName(filename));
        }

        internal override Stream GetFileStream(string filename)
        {
            return Manager.Open(filename);
        }

        private byte[] GetStreamBytes(Stream stream)
        {
            byte[] buffer = new byte[16 * 1024];

            using (MemoryStream ms = new MemoryStream())
            {
                int read;

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

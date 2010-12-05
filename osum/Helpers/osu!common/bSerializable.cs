using System.IO;
using osu_common.Helpers;

namespace osu_common.Bancho
{
    public interface bSerializable
    {
        void ReadFromStream(SerializationReader sr);
        void WriteToStream(SerializationWriter sw);
    }
}
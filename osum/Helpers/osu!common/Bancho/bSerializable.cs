using System.IO;
using osu_common.Helpers;

namespace osu_common.Tencho
{
    public interface bSerializable
    {
        void ReadFromStream(SerializationReader sr);
        void WriteToStream(SerializationWriter sw);
    }

    public interface iSerializable
    {
        void WriteToStreamIrc(SerializationWriter sw);
    }

}
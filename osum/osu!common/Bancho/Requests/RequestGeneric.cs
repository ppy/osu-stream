using System.IO;
using osu_common.Bancho.Requests;

namespace osu_common.Bancho.Requests
{
    public class RequestGeneric : Request
    {
        public RequestGeneric(RequestType type, bSerializable payload)
        {
            this.type = type;
            this.payload = payload;
        }

        public override void Process(Stream s)
        {
        }
    }
}
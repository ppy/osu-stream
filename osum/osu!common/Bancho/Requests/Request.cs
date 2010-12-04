using System;
using System.IO;
using osu_common.Helpers;
using Ionic.Zlib;

namespace osu_common.Bancho.Requests
{
    public abstract class Request
    {
        public const int HEADER_LEN = sizeof (ushort) + sizeof (uint) + sizeof (byte);

        internal bSerializable payload;
        public RequestType type;

        public abstract void Process(Stream s);

        public virtual long Send(Stream s)
        {
            long sentbytes = 1;

            s.Write(BitConverter.GetBytes((ushort) type), 0, sizeof (ushort));
            sentbytes += sizeof(ushort);

            if (payload != null)
            {
                using (MemoryStream payloadBuffer = new MemoryStream(8))
                {
                    payload.WriteToStream(new SerializationWriter(payloadBuffer));

                    int payloadSize = (int) payloadBuffer.Length;

                    if (payloadSize < 150 || !OsuCommon.UseCompression)
                    {
                        s.WriteByte(0);

                        s.Write(BitConverter.GetBytes((uint) payloadSize), 0, sizeof (uint));
                        s.Write(payloadBuffer.GetBuffer(),0,payloadSize);
#if FULL_DEBUG
                        Console.WriteLine("S" + payloadSize + ": " + type);
#endif
                        sentbytes += payloadSize + sizeof (uint) + 1;
                    }
                    else
                    {
                        using (MemoryStream writeBuffer = new MemoryStream(32))
                        {
                            using (GZipStream comp = new GZipStream(writeBuffer, CompressionMode.Compress, CompressionLevel.BestCompression, true))
                                comp.Write(payloadBuffer.GetBuffer(), 0, payloadSize);

                            s.WriteByte(1);
                            s.Write(BitConverter.GetBytes((uint) writeBuffer.Length), 0, sizeof (uint));
                            s.Write(writeBuffer.GetBuffer(), 0, (int) writeBuffer.Length);
#if FULL_DEBUG
                            Console.WriteLine("S" + writeBuffer.Length + "(uncompressed" + payloadSize + ": " + type);
#endif
                            sentbytes += writeBuffer.Length + sizeof (uint);
                        }
                    }
                }
            }
            else
            {
                s.WriteByte(0); 
                s.Write(BitConverter.GetBytes((uint)0), 0, sizeof(uint));
#if DEBUG
                if (type != RequestType.Bancho_Ping && type != RequestType.Osu_Pong)
                    Console.WriteLine("S0: " + type);
#endif
                sentbytes += sizeof(uint) + 1;
            }

            s.Flush();

            return sentbytes;
        }
    }
}
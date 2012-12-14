using System;
using System.IO;
using osu_common.Helpers;

namespace osu_common.Tencho.Requests
{
    public enum RequestTarget { Osu = 0, Irc = 1, Stream = 2 };

    public class Request
    {
        public const int HEADER_LEN = sizeof(ushort) + sizeof(uint) + sizeof(byte);

        public object payload;
        public RequestType type;
        byte[][] byteCache = new byte[2][];

        protected Request()
        { }

        public Request(RequestType type, object payload, bool cache = false)
        {
            this.type = type;
            this.payload = payload;
            Cache = cache;
        }

        public bool Cache;

        public int Send(Stream s, SerializationWriter sw)
        {
            int len = Send(RequestTarget.Osu, sw);
            s.Write(((MemoryStream)sw.BaseStream).GetBuffer(),0,len);
            return len;
        }

        public int Send(RequestTarget target, SerializationWriter sw)
        {
            if (Cache)
            {
                byte[] cache = byteCache[(int)target];

                if (cache != null)
                {
                    //use the byte-cache, since it is present.
                    if (cache.Length > 0)
                    {
                        sw.Seek(0, SeekOrigin.Begin);
                        sw.BaseStream.Write(cache, 0, cache.Length);
                    }

                    return cache.Length;
                }

                //cache miss; we will fill it later on...
            }

            int sendLength;

            switch (target)
            {
                case RequestTarget.Irc:
                    sendLength = sendIrc(sw);
                    break;
                default:
                case RequestTarget.Osu:
                case RequestTarget.Stream:
                    sendLength = sendOsu(sw);
                    break;
            }

            if (Cache)
            {
                //fill the byte-cache.
                byte[] cache = new byte[sendLength];
                if (sendLength > 0)
                {
                    sw.Seek(0, SeekOrigin.Begin);
                    sw.BaseStream.Read(cache, 0, sendLength);
                }

                byteCache[(int)target] = cache;
            }

            return sendLength;
        }

        private int sendOsu(SerializationWriter sw)
        {
            bSerializable bPayload = payload as bSerializable;

            if (type == RequestType.Irc_Only)
                return 0;

            long sentBytes = 0;

            //set the position to after the header..
            MemoryStream payloadBuffer = (MemoryStream)sw.BaseStream;
            sw.Seek(HEADER_LEN, SeekOrigin.Begin);


            int payloadSize = 0;

            if (bPayload != null)
            {
                bPayload.WriteToStream(sw);
                payloadSize = (int)payloadBuffer.Position - HEADER_LEN;
            }

            sw.Seek(0, SeekOrigin.Begin);

            sw.Write((ushort)type);
            sw.Write((byte)0);
            sw.Write((uint)payloadSize);

            return HEADER_LEN + payloadSize;
        }

        private int sendIrc(SerializationWriter sw)
        {
            iSerializable iPayload = payload as iSerializable;
            if (iPayload == null) return 0;

            MemoryStream payloadBuffer = (MemoryStream)sw.BaseStream;
            sw.Seek(0, SeekOrigin.Begin);
            iPayload.WriteToStreamIrc(sw);

            return (int)payloadBuffer.Position;
        }
    }
}
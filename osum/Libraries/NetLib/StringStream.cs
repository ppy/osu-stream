namespace osu_common.Libraries.NetLib
{
    using System;
    using System.IO;
    using System.Text;

    public class StringStream : MemoryStream
    {
        public StringStream() : this(string.Empty, new object[0])
        {
        }

        public StringStream(string text) : this(text, new object[0])
        {
        }

        public StringStream(string text, params object[] args)
        {
            StreamWriter writer = new StreamWriter(this, Encoding.Default);
            
            if (args.Length == 0)
                writer.Write(text);
            else
                writer.Write(text, args);

            writer.Flush();
            this.Seek(0L, SeekOrigin.Begin);
        }

        public string StringData
        {
            get
            {
                long position = this.Position;
                this.Seek(0L, SeekOrigin.Begin);
                string str = new StreamReader(this, Encoding.Default).ReadToEnd();
                this.Seek(position, SeekOrigin.Begin);
                return str;
            }
        }
    }
}


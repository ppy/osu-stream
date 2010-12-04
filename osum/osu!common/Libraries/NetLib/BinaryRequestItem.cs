namespace osu_common.Libraries.NetLib
{
    using System;
    using System.IO;

    public class BinaryRequestItem : HttpRequestItem
    {
        protected internal override void AddData(byte[] data, int index, int length)
        {
            if (base.Owner != null)
            {
                if (base.Owner.dataStream == null)
                {
                    GetDataStreamEventArgs e = new GetDataStreamEventArgs(this);
                    base.Owner.OnGetDataStream(e);
                    base.Owner.dataStream = e.Stream;
                }
                if (base.Owner.dataStream != null)
                {
                    base.Owner.dataStream.Write(data, index, length);
                }
            }
        }

        protected internal override void AfterAddData()
        {
            if ((base.Owner != null) && (base.Owner.dataStream != null))
            {
                base.Owner.dataStream.Seek(0L, SeekOrigin.Begin);
                base.Owner.OnDataAdded(new DataAddedEventArgs(this, base.Owner.dataStream));
            }
        }

        public override Stream GetData()
        {
            GetDataStreamEventArgs e = new GetDataStreamEventArgs(this);
            if (base.Owner != null)
            {
                base.Owner.OnGetDataSourceStream(e);
            }
            if (e.Stream == null)
            {
                return Stream.Null;
            }
            e.Stream.Seek(0L, SeekOrigin.Begin);
            return e.Stream;
        }
    }
}


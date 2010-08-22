namespace Un4seen.Bass.AddOn.Fx
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public sealed class BASS_BFX_MIX : IDisposable
    {
        private IntPtr ptr;
        private GCHandle hgc;
        public BASSFXChan[] lChannel;
        public BASS_BFX_MIX(int numChans)
        {
            ptr = IntPtr.Zero;
            lChannel = new BASSFXChan[numChans];
            for (int i = 0; i < numChans; i++)
            {
                lChannel[i] = (BASSFXChan) (((int) 1) << i);
            }
        }

        public BASS_BFX_MIX(params BASSFXChan[] channels)
        {
            ptr = IntPtr.Zero;
            lChannel = new BASSFXChan[channels.Length];
            for (int i = 0; i < channels.Length; i++)
            {
                lChannel[i] = channels[i];
            }
        }

        internal void Set()
        {
            if (hgc.IsAllocated)
            {
                hgc.Free();
                ptr = IntPtr.Zero;
            }
            int[] numArray = new int[lChannel.Length];
            for (int i = 0; i < lChannel.Length; i++)
            {
                numArray[i] = (int) lChannel[i];
            }
            hgc = GCHandle.Alloc(numArray, GCHandleType.Pinned);
            ptr = hgc.AddrOfPinnedObject();
        }

        internal void Get()
        {
            if (ptr != IntPtr.Zero)
            {
                int[] destination = new int[lChannel.Length];
                Marshal.Copy(ptr, destination, 0, destination.Length);
                for (int i = 0; i < destination.Length; i++)
                {
                    lChannel[i] = (BASSFXChan) destination[i];
                }
            }
        }

        public void Dispose()
        {
            if (hgc.IsAllocated)
            {
                hgc.Free();
                ptr = IntPtr.Zero;
            }
        }
    }
}


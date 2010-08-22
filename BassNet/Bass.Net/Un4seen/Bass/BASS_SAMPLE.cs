namespace Un4seen.Bass
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public sealed class BASS_SAMPLE
    {
        public int freq;
        public float volume;
        public float pan;
        public BASSFlag flags;
        public int length;
        public int max;
        public int origres;
        public int chans;
        public int mingap;
        public BASS3DMode mode3d;
        public float mindist;
        public float maxdist;
        public int iangle;
        public int oangle;
        public float outvol;
        public BASSVam vam;
        public int priority;
        public BASS_SAMPLE()
        {
            freq = 0xac44;
            volume = 1f;
            max = 1;
            chans = 2;
            outvol = 1f;
            vam = BASSVam.BASS_VAM_HARDWARE;
        }

        public BASS_SAMPLE(int Freq, float Volume, float Pan, BASSFlag Flags, int Length, int Max, int OrigRes, int Chans, int MinGap, BASS3DMode Flag3D, float MinDist, float MaxDist, int IAngle, int OAngle, float OutVol, BASSVam FlagsVam, int Priority)
        {
            freq = 0xac44;
            volume = 1f;
            max = 1;
            chans = 2;
            outvol = 1f;
            vam = BASSVam.BASS_VAM_HARDWARE;
            freq = Freq;
            volume = Volume;
            pan = Pan;
            flags = Flags;
            length = Length;
            max = Max;
            origres = OrigRes;
            chans = Chans;
            mingap = MinGap;
            mode3d = Flag3D;
            mindist = MinDist;
            maxdist = MaxDist;
            iangle = IAngle;
            oangle = OAngle;
            outvol = OutVol;
            vam = FlagsVam;
            priority = Priority;
        }

        public override string ToString()
        {
            return string.Format("Frequency={0}, Volume={1}, Pan={2}", freq, volume, pan);
        }
    }
}


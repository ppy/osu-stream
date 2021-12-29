using ManagedBass;
using osum.AssetManager;

namespace osum.Audio
{
    /// <summary>
    /// Play short-lived sound effects, and handle caching.
    /// </summary>
    public class SoundEffectPlayerBass : SoundEffectPlayer
    {
        public SoundEffectPlayerBass()
        {
            sourceInfo = new Source[MAX_SOURCES];

            for (int i = 0; i < MAX_SOURCES; i++)
            {
                Source info = sourceInfo[i];

                if (info == null)
                    sourceInfo[i] = new SourceBass();
                else
                {
                    info.BufferId = 0;
                }
            }

            GameBase.Scheduler.Add(CheckUnload, 1000);
        }

        private void CheckUnload()
        {
            foreach (Source s in sourceInfo)
                if (s.Disposable && !s.Playing)
                    s.DeleteBuffer();
            GameBase.Scheduler.Add(CheckUnload, 1000);
        }

        /// <summary>
        /// Loads the specified sound file.
        /// </summary>
        /// <param name="filename">Filename of a 44khz 16-bit wav sample.</param>
        /// <returns>-1 on error, bufferId on success.</returns>
        public override int Load(string filename)
        {
#if DEBUG
            if (!NativeAssetManager.Instance.FileExists(filename)) return -1;
#endif
            byte[] bytes = NativeAssetManager.Instance.GetFileBytes(filename);

            int address = Bass.SampleLoad(bytes, 0, bytes.Length, 32, BassFlags.SampleOverrideLongestPlaying);

            return address;
        }
    }

    public class SourceBass : Source
    {
        public SourceBass()
            : base(-1)
        {
        }

        public override int BufferId
        {
            get => base.BufferId;
            set
            {
                base.BufferId = value;

                sourceId = Bass.SampleGetChannel(bufferId);
            }
        }

        public override bool Playing => Bass.ChannelIsActive(sourceId) == PlaybackState.Playing;

        private float audioFrequency = -1;

        public override float Pitch
        {
            get => base.Pitch;
            set
            {
                if (audioFrequency == -1) Bass.ChannelGetAttribute(sourceId, ChannelAttribute.Frequency, out audioFrequency);
                Bass.ChannelSetAttribute(sourceId, ChannelAttribute.Frequency, audioFrequency * value);

                base.Pitch = value;
            }
        }

        internal override void Play()
        {
            Bass.ChannelPlay(sourceId, true);
        }

        internal override void Stop()
        {
            Bass.ChannelStop(sourceId);
        }

        internal override void DeleteBuffer()
        {
            Bass.SampleFree(bufferId);
            base.DeleteBuffer();
        }

        public override bool Looping
        {
            get => base.Looping;
            set
            {
                base.Looping = value;

                if (Looping)
                    Bass.ChannelFlags(sourceId, BassFlags.Loop, BassFlags.Loop);
                else
                    Bass.ChannelFlags(sourceId, 0, BassFlags.Loop);
            }
        }

        public override float Volume
        {
            get => base.Volume;
            set
            {
                base.Volume = value;
                Bass.ChannelSetAttribute(sourceId, ChannelAttribute.Volume, Volume);
            }
        }
    }
}

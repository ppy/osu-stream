using System;
using System.Collections.Generic;
using osum.Support;
using osum.Audio;
using System.IO;
using Un4seen.Bass;

namespace osum
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
                    //info.SourceId = sources[i];
                    info.BufferId = 0;
                }
            }

            GameBase.Scheduler.Add(CheckUnload, 1000);
        }

        void CheckUnload()
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
            
            int address = Bass.BASS_SampleLoad(bytes, 0, bytes.Length, 32, BASSFlag.BASS_SAMPLE_OVER_POS);

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
            get
            {
                return base.BufferId;
            }
            set
            {
                base.BufferId = value;

                sourceId = Bass.BASS_SampleGetChannel(bufferId, false);
            }
        }

        public override bool Playing
        {
            get { return Bass.BASS_ChannelIsActive(sourceId) == BASSActive.BASS_ACTIVE_PLAYING; }
        }

        float originalFrequency = -1;
        public override float Pitch
        {
            get
            {
                return base.Pitch;
            }
            set
            {
                value = Math.Max(0.5f, Math.Min(2f, value));
                if (value == base.Pitch) return;

                if (originalFrequency < 0)
                    Bass.BASS_ChannelGetAttribute(sourceId, BASSAttribute.BASS_ATTRIB_FREQ, ref originalFrequency);
                Bass.BASS_ChannelSetAttribute(sourceId, BASSAttribute.BASS_ATTRIB_FREQ, originalFrequency * value);

                base.Pitch = value;
            }
        }

        internal override void Play()
        {
            Bass.BASS_ChannelPlay(sourceId, false);
        }

        internal override void Stop()
        {
            Bass.BASS_ChannelStop(sourceId);
        }

        internal override void DeleteBuffer()
        {
            Bass.BASS_SampleFree(bufferId);
            base.DeleteBuffer();
        }

        public override bool Looping
        {
            get
            {
                return base.Looping;
            }
            set
            {
                base.Looping = value;
                if (Looping)
                    Bass.BASS_ChannelFlags(sourceId, BASSFlag.BASS_SAMPLE_LOOP, BASSFlag.BASS_SAMPLE_LOOP);
                else
                    Bass.BASS_ChannelFlags(sourceId, 0, BASSFlag.BASS_SAMPLE_LOOP);
            }
        }

        public override float Volume
        {
            get
            {
                return base.Volume;
            }
            set
            {
                base.Volume = value;
                Bass.BASS_ChannelSetAttribute(sourceId, BASSAttribute.BASS_ATTRIB_VOL, Volume);
            }
        }
    }
}


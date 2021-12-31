using System;
using System.IO;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using osum.AssetManager;
using osum.Helpers.Audio;

namespace osum.Audio
{
    /// <summary>
    /// Play short-lived sound effects, and handle caching.
    /// </summary>
    public class SoundEffectPlayerOpenAL : SoundEffectPlayer
    {
        /// <summary>
        /// Current OpenAL context.
        /// </summary>
        private AudioContext context;

        public SoundEffectPlayerOpenAL()
        {
            try
            {
                context = new AudioContext();
            }
            catch (DllNotFoundException)
            {
                //needs openal32.dll
                throw new ApplicationException("OpenAL failed to initialize. Please run oainst.exe and try again.");
            }
            catch (AudioException)
            {
                //todo: handle error here.
            }
            catch (TypeInitializationException)
            {
                throw new ApplicationException("OpenAL failed to initialize. Please run oainst.exe and try again.");
            }

            int[] sources = AL.GenSources(MAX_SOURCES);
            sourceInfo = new Source[MAX_SOURCES];

            for (int i = 0; i < MAX_SOURCES; i++)
            {
                Source info = sourceInfo[i];

                if (info == null)
                    sourceInfo[i] = new SourceAL(sources[i]);
                else
                {
                    info.SourceId = sources[i];
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

            int buffer = AL.GenBuffer();

            using (Stream str = NativeAssetManager.Instance.GetFileStream(filename))
            using (WaveReader sound = new WaveReader(str))
            {
                SoundData s = sound.ReadToEnd();
                AL.BufferData(buffer, s.SoundFormat.SampleFormatAsOpenALFormat, s.Data, s.Data.Length, s.SoundFormat.SampleRate);
            }

            if (AL.GetError() != ALError.NoError)
                return -1;

            return buffer;
        }
    }

    public class SourceAL : Source
    {
        public SourceAL(int source)
            : base(source)
        {
        }

        public override float Pitch
        {
            get => base.Pitch;
            set
            {
                value = Math.Max(0.5f, Math.Min(2f, value));
                if (value == base.Pitch) return;

                base.Pitch = value;
                AL.Source(sourceId, ALSourcef.Pitch, value);
            }
        }

        public override bool Playing => AL.GetSourceState(sourceId) == ALSourceState.Playing;

        public override int BufferId
        {
            get => base.BufferId;
            set
            {
                base.BufferId = value;
                AL.Source(sourceId, ALSourcei.Buffer, BufferId);
            }
        }

        public override float Volume
        {
            get => base.Volume;
            set
            {
                if (value == Volume) return;

                base.Volume = value;

                AL.Source(sourceId, ALSourcef.Gain, Volume);
            }
        }

        public override bool Looping
        {
            get => base.Looping;
            set
            {
                base.Looping = value;
                AL.Source(sourceId, ALSourceb.Looping, Looping);
            }
        }


        internal override void Play()
        {
            if (!Playing)
                AL.SourcePlay(sourceId);
        }

        internal override void Stop()
        {
            if (Playing)
                AL.SourceStop(sourceId);
        }

        internal override void DeleteBuffer()
        {
            //must unload before deleting.
            AL.Source(sourceId, ALSourcei.Buffer, 0);
            AL.DeleteBuffer(bufferId);

            base.DeleteBuffer();
        }
    }
}
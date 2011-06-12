using System;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using System.Collections.Generic;
using osum.Support;
using osum.Audio;
using System.IO;

namespace osum
{
    /// <summary>
    /// Play short-lived sound effects, and handle caching.
    /// </summary>
    public class SoundEffectPlayer : IUpdateable
    {
        /// <summary>
        /// Current OpenAL context.
        /// </summary>
        AudioContext context;

        /// <summary>
        /// All loaded samples are accessible here in a filename-bufferid dictionary.
        /// </summary>
        Dictionary<string, int> BufferCache = new Dictionary<string, int>();

        const int MAX_SOURCES = 32; //hardware limitation

        int[] sources;
        Source[] sourceInfo;

        public SoundEffectPlayer()
        {
            try
            {
                context = new AudioContext();
            }
            catch (DllNotFoundException)
            {
                //needs openal32.dll
            }
            catch (AudioException)
            {
                //todo: handle error here.
            }

            sources = AL.GenSources(MAX_SOURCES);
            sourceInfo = new Source[MAX_SOURCES];

            for (int i = 0; i < MAX_SOURCES; i++)
            {
                Source info = sourceInfo[i];

                if (info == null)
                    sourceInfo[i] = new Source(sources[i]);
                else
                {
                    info.SourceId = sources[i];
                    info.BufferId = -1;
                }
            }
        }

        /// <summary>
        /// Loads the specified sound file.
        /// </summary>
        /// <param name="filename">Filename of a 44khz 16-bit wav sample.</param>
        /// <returns>-1 on error, bufferId on success.</returns>
        public int Load(string filename)
        {
            if (!NativeAssetManager.Instance.FileExists(filename)) return -1;

            int buffer = AL.GenBuffer();

            using (Stream str = NativeAssetManager.Instance.GetFileStream(filename))
            using (AudioReader sound = new AudioReader(str))
            {
                byte[] readSound = sound.ReadToEnd().Data;
                AL.BufferData(buffer, OpenTK.Audio.OpenAL.ALFormat.Stereo16, readSound, readSound.Length, 44100);
            }

            if (AL.GetError() != ALError.NoError)
                return -1;

            return buffer;
        }

        /// <summary>
        /// Unloads all samples and clears cache.
        /// </summary>
        public void UnloadAll()
        {
            foreach (int id in BufferCache.Values)
                AL.DeleteBuffer(id);
            BufferCache.Clear();

            AL.DeleteSources(sources);
        }

        /// <summary>
        /// Plays the sample in provided buffer on a new source.
        /// </summary>
        /// <param name="buffer">The bufferId.</param>
        /// <returns></returns>
        public Source PlayBuffer(int buffer, float volume, bool loop = false, bool reserve = false)
        {
            int freeSource = -1;

            for (int i = 0; i < MAX_SOURCES; i++)
            {
                Source n = sourceInfo[i];

                if (n.Reserved || n.Playing)
                    continue;

                if (n.BufferId == buffer)
                {
                    //can reuse without a rebind.
                    freeSource = i;
                    break;
                }

                if (freeSource == -1)
                    freeSource = i;
            }

            if (freeSource == -1)
                return null; //no free sources

            Source info = sourceInfo[freeSource];

            info.Reserved = reserve;
            info.BufferId = buffer;
            info.Volume = volume;
            info.Looping = loop;
            info.Pitch = 1;

            info.Play();

            return info;
        }

        /// <summary>
        /// Updates this instance. Called every frame when loaded as a component.
        /// </summary>
        public void Update()
        {

        }
    }

    public class Source
    {
        int sourceId;
        public int SourceId
        {
            get { return sourceId; }
            set
            {
                sourceId = value;
                BufferId = -1;
                pitch = 1;
            }
        }

        float pitch = 1;
        public float Pitch
        {
            get { return pitch; }
            set
            {
                value = Math.Max(0.5f, Math.Min(2f, value));

                if (pitch == value)
                    return;

                pitch = value;
                AL.Source(sourceId, ALSourcef.Pitch, pitch);
            }
        }

        public bool Reserved;

        int bufferId = -1;
        public int BufferId
        {
            get { return bufferId; }
            set
            {
                if (bufferId == value)
                    return;
                bufferId = value;

                AL.Source(sourceId, ALSourcei.Buffer, bufferId);
            }
        }

        public Source(int source)
        {
            sourceId = source;
        }

        float volume = 1;
        public float Volume
        {
            get { return volume; }
            set
            {
                if (value == volume) return;

                volume = value;
                AL.Source(sourceId, ALSourcef.Gain, volume);
            }
        }

        bool looping;
        public bool Looping
        {
            get { return looping; }
            set
            {
                if (looping == value) return;
                looping = value;
                AL.Source(sourceId, ALSourceb.Looping, looping);
            }
        }

        public bool Playing { get { return AL.GetSourceState(sourceId) == ALSourceState.Playing; } }

        internal void Play()
        {
            if (!Playing)
                AL.SourcePlay(sourceId);
        }

        internal void Stop()
        {
            if (Playing)
                AL.SourceStop(sourceId);
        }
    }
}


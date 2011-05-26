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
    public class SoundEffectPlayer : IUpdateable, ISoundEffectPlayer
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
        public int PlayBuffer(int buffer)
        {
            int i = 0;
            while (AL.GetSourceState(sources[i]) == ALSourceState.Playing)
            {
                if (++i >= MAX_SOURCES)
                    return -1; //ran out of sources
            }

            AL.Source(sources[i], ALSourcei.Buffer, buffer);
            AL.SourcePlay(sources[i]);

            return sources[i];
        }

        /// <summary>
        /// Updates this instance. Called every frame when loaded as a component.
        /// </summary>
        public void Update()
        {

        }
    }
}


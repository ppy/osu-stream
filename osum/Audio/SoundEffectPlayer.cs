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
        /// Extension which provides more control over how buffers are stored.
        /// </summary>
        XRamExtension XRam;

        /// <summary>
        /// Current OpenAL context.
        /// </summary>
        AudioContext context;

        /// <summary>
        /// All loaded samples are accessible here in a filename-bufferid dictionary.
        /// </summary>
        Dictionary<string, int> BufferCache = new Dictionary<string, int>();

        /// <summary>
        /// Active sources are temporarily stored here so they can be managed and cleaned up.
        /// </summary>
        List<int> Sources = new List<int>();

        public SoundEffectPlayer()
        {
            try
            {
                AudioContext AC = new AudioContext();
            }
            catch (AudioException e)
            {
                //todo: handle error here.
            }

            XRam = new XRamExtension();
        }

        /// <summary>
        /// Loads the specified sound file.
        /// </summary>
        /// <param name="filename">Filename of a 44khz 16-bit wav sample.</param>
        /// <returns>-1 on error, bufferId on success.</returns>
        public int Load(string filename)
        {
            if (!File.Exists(filename)) return -1;
			
			int[] buffers = AL.GenBuffers(1);

            // Load a .wav file from disk
            if (XRam.IsInitialized) XRam.SetBufferMode(0, ref buffers[0], XRamExtension.XRamStorage.Hardware); // optional

            AudioReader sound = new AudioReader(filename);
            byte[] readSound = sound.ReadToEnd().Data;
            AL.BufferData(buffers[0], OpenTK.Audio.OpenAL.ALFormat.Stereo16, readSound, readSound.Length, 44100);

            if (AL.GetError() != ALError.NoError)
                return -1;

            return buffers[0];
        }

        /// <summary>
        /// Unloads all samples and clears cache.
        /// </summary>
        public void UnloadAll()
        {
            foreach (int id in BufferCache.Values)
                AL.DeleteBuffer(id);
            BufferCache.Clear();
        }

        /// <summary>
        /// Plays the sample in provided buffer on a new source.
        /// </summary>
        /// <param name="buffer">The bufferId.</param>
        /// <returns></returns>
        public int PlayBuffer(int buffer)
        {
            int[] sources = AL.GenSources(1);
            AL.Source(sources[0], ALSourcei.Buffer, buffer);
            AL.SourcePlay(sources[0]);

            Sources.Add(sources[0]);

            return sources[0];
        }

        /// <summary>
        /// Updates this instance. Called every frame when loaded as a component.
        /// </summary>
        public void Update()
        {

            foreach (int id in Sources.FindAll(i => AL.GetSourceState(i) != ALSourceState.Playing))
            {
                AL.DeleteSource(id);
                Sources.Remove(id);
            }
        }
    }
}


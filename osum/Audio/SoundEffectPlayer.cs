using System;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using System.Collections.Generic;
using osum.Support;

namespace osum
{
	public class SoundEffectPlayer : IUpdateable
	{
		XRamExtension XRam;
        AudioContext context;

        Dictionary<string, int> BufferCache = new Dictionary<string,int>();
		
        List<int> Sources = new List<int>();

		public SoundEffectPlayer()
		{
            try
			{
				AudioContext AC = new AudioContext();
			} catch( AudioException e)
			{ 
                // problem with Device or Context, cannot continue
			}

            XRam = new XRamExtension(); // must be instantiated per used Device if X-Ram is desired.
 		}

        public int Load(string filename)
        {
            int[] buffers = AL.GenBuffers(1);
			
			// Load a .wav file from disk
			if ( XRam.IsInitialized ) XRam.SetBufferMode( 0,ref buffers[0], XRamExtension.XRamStorage.Hardware ); // optional
			 
			AudioReader sound = new AudioReader(filename);
			byte[] readSound = sound.ReadToEnd().Data;
			AL.BufferData(buffers[0], OpenTK.Audio.OpenAL.ALFormat.Stereo16,readSound,readSound.Length,44100);

            if ( AL.GetError() != ALError.NoError )
			{
			    // respond to load error etc.
                return -1;
			}


            return buffers[0];
        }

        public void UnloadAll()
        {
            foreach (int id in BufferCache.Values)
                AL.DeleteBuffer(id);
            BufferCache.Clear();
        }

        public int PlayBuffer(int buffer)
        {
            int[] sources = AL.GenSources(1);
			AL.Source(sources[0], ALSourcei.Buffer, buffer);
			AL.SourcePlay(sources[0]);

            Sources.Add(sources[0]);

            return sources[0];
        }

        public void Update()
        {
            foreach (int id in Sources)
            {
                //dispose of old sources
            }
        }
	}
}


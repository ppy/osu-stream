using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Un4seen.Bass;
using System.IO;
using System.Runtime.InteropServices;

namespace osum.Audio
{
    class BackgroundAudioPlayerDesktop : IBackgroundAudioPlayer
    {
        private GCHandle audioHandle;
        private static int audioStream;
        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundAudioPlayerDesktop"/> class.
        /// </summary>
        public BackgroundAudioPlayerDesktop()
        {
            BassNet.Registration("poo@poo.com", "2X25242411252422");

            Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, (IntPtr)0, null);

            Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_BUFFER, 100);
            Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_UPDATEPERIOD, 10);
        }

        /// <summary>
        /// Gets the current volume.
        /// </summary>
        /// <value>The current volume.</value>
        public float CurrentVolume
        {
            get { return 0; }
        }

        /// <summary>
        /// Plays the loaded audio.
        /// </summary>
        /// <returns></returns>
        public bool Play()
        {
            Bass.BASS_ChannelPlay(audioStream, true);
            return true;
        }

        /// <summary>
        /// Stops the playing audio (and unloads it).
        /// </summary>
        /// <returns></returns>
        public bool Stop()
        {
            Bass.BASS_ChannelStop(audioStream);
            FreeMusic();
            return true;
        }

        /// <summary>
        /// Updates this instance. Called every frame when loaded as a component.
        /// </summary>
        public void Update()
        {

        }

        internal void FreeMusic()
        {
            if (audioStream != 0)
            {
                if (audioHandle.IsAllocated)
                    audioHandle.Free();

                Bass.BASS_ChannelStop(audioStream);
                Bass.BASS_StreamFree(audioStream);
                audioStream = 0;
            }
        }

        public bool Load(byte[] audio)
        {
            FreeMusic();
            audioHandle = GCHandle.Alloc(audio, GCHandleType.Pinned);

            audioStream = Bass.BASS_StreamCreateFile(audioHandle.AddrOfPinnedObject(), 0, audio.Length, BASSFlag.BASS_STREAM_PRESCAN);

            return true;
        }

        public double CurrentTime
        {
            get {
                if (audioStream == 0) return 0;

                long audioTimeRaw = Bass.BASS_ChannelGetPosition(audioStream);
                return Bass.BASS_ChannelBytes2Seconds(audioStream, audioTimeRaw); 
            }
        }

        public bool Pause()
        {
            Bass.BASS_ChannelPause(audioStream);
            return true;
        }

        public bool SeekTo(int milliseconds)
        {
            if (audioStream == 0) return false;

            Bass.BASS_ChannelSetPosition(audioStream, milliseconds / 1000d);
            return true;

        }
    }
}
